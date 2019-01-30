using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using BSPEngine;
using BSPGenerationTools;

namespace GCCBuilder
{
    class GCCPackerClient
    {
        string _ToolchainPacker;
        BuildJob _Job;
        string _TargetDirectory;
        string _Target, _RegistryPrefix;
        string _UserFriendlyName;
        string _OutputDir;

        public GCCPackerClient(string targetDirectory, BuildJob job, string target, string userFriendlyName, string outputDir)
        {
            _ToolchainPacker = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ToolchainPacker.exe");
            if (!File.Exists(_ToolchainPacker))
                MessageBox.Show("Warning! ToolchainPacker.exe not found!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _Target = target;
            _OutputDir = outputDir;
            _RegistryPrefix = string.Format("SysGCC-{0}-{1}", target, job.GCC.GCCVersion);
            _TargetDirectory = targetDirectory;
            _UserFriendlyName = userFriendlyName;
            _Job = job;
        }

        Toolchain CreateXmlDescriptionFile()
        {
            string dir = Path.Combine(_TargetDirectory, _Job.JobString + "-" + _Target);
            Toolchain toolchain = new BSPEngine.Toolchain();
            string toolchainFile = Path.Combine(dir, "toolchain.xml");
            if (File.Exists(toolchainFile))
                toolchain = XmlTools.LoadObject<BSPEngine.Toolchain>(toolchainFile);
            
            if (string.IsNullOrEmpty(_UserFriendlyName))
                toolchain.ToolchainName = _Target;
            else
                toolchain.ToolchainName = _UserFriendlyName;

            toolchain.GNUTargetID = _Target;
            toolchain.ToolchainID = "com.visualgdb." + _Target;
            toolchain.BinaryDirectory = "bin";
            toolchain.Prefix = _Target + "-";
            toolchain.Make = "make.exe";
            toolchain.BSPIndexUrlFormat = "http://visualgdb.com/hwsupport/BSP/?autofetch=1&target=" + _Target + "&filter={0}";

            toolchain.GCCVersion = _Job.GCC.GCCVersion;
            toolchain.GDBVersion = _Job.GDB.GDBVersion;


            if (toolchain.SourcePackages != null)
            {
                string sourcePackageDir = Path.Combine(_OutputDir, "_SourcePackages");
                Directory.CreateDirectory(sourcePackageDir);
                foreach (var spkg in toolchain.SourcePackages)
                {
                    string fn = string.Format(@"{0}\{1}-{2}.tgz", sourcePackageDir, spkg.UniqueID, spkg.Version);
                    if (!File.Exists(fn))
                        TarPacker.PackDirectoryToTGZ(spkg.OriginalDirectory, fn, null);
                }
            }
            

            string myDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            File.Copy(myDir + "\\make.exe", dir + "\\bin\\make.exe", true);

            BSPEngine.XmlTools.SaveObject(toolchain, toolchainFile);
            return toolchain;
        }

        protected static void CopyDirectoryRecursive(string sourceDirectory, string destinationDirectory)
        {
            sourceDirectory = sourceDirectory.TrimEnd('/', '\\');
            destinationDirectory = destinationDirectory.TrimEnd('/', '\\');

            foreach (var file in Directory.GetFiles(sourceDirectory))
            {
                string relPath = file.Substring(sourceDirectory.Length + 1);
                File.Copy(file, Path.Combine(destinationDirectory, relPath), true);
            }

            foreach (var dir in Directory.GetDirectories(sourceDirectory))
            {
                string relPath = dir.Substring(sourceDirectory.Length + 1);
                string newDir = Path.Combine(destinationDirectory, relPath);
                if (!Directory.Exists(newDir))
                    Directory.CreateDirectory(newDir);
                CopyDirectoryRecursive(dir, newDir);
            }
        }


        public string CreateToolchainArchive(string extraFilesDir)
        {
            string dir = Path.Combine(_TargetDirectory, _Job.JobString + "-" + _Target);

            if (Directory.Exists(Path.Combine(extraFilesDir, _Target)))
                CopyDirectoryRecursive(Path.Combine(extraFilesDir, _Target), dir);
            var toolchain = CreateXmlDescriptionFile();

            string myDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string shortName = string.Format("{0}-gcc{1}", _Target, _Job.GCC.GCCVersion);
            if (toolchain.Revision > 1)
                shortName += "-r" + toolchain.Revision;
            shortName += ".exe";

            string stub = Path.Combine(myDir, "GCCInstaller.exe");

            string targetDir = _OutputDir + "\\" + _Target;
            Directory.CreateDirectory(targetDir);

            string args = string.Format("{0} {1}\\{2} gcc={3} bu={4} gdb={5} newlib={6} tag={7} target={8} stub={9}", dir, targetDir, shortName, _Job.GCC.GCCVersion, _Job.Binutils.BinutilsVersion, _Job.GDB.GDBVersion, _Job.Target.Libc.Version, _RegistryPrefix, _Target, stub);

            Process.Start(_ToolchainPacker, args).WaitForExit();
            string uncompressed = Path.Combine(targetDir, shortName + ".uncompressed");
            if (File.Exists(uncompressed))
                File.Delete(uncompressed);

            return Path.Combine(targetDir, shortName);
        }
    }
}
