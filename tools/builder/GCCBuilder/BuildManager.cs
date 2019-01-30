using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using BSPEngine;
using Microsoft.Win32;

namespace GCCBuilder
{
    class BuildManager
    {
        private BuildJob _Job;
        private string _ExtraLibs;
        private IBuildProgressReporter _Reporter;
        BuildToolkit _Toolkit;

        public const string CygwinDirectory = @"e:\cygwin64.beagle";

        public BuildManager(BuildJob _Job, BuildToolkit toolkit, string extraLibs, IBuildProgressReporter reporter)
        {
            this._Job = _Job;
            _Toolkit = toolkit;
            _ExtraLibs = extraLibs;
            _Reporter = reporter;
        }

        public void DoBuild()
        {
            Download();
            Unpack();
            Build();
            Publish();
        }

        void Download()
        {
            GCCDownloader dl = new GCCDownloader(_Job.Build.LocalDirectory, _Job, _Reporter);
            ManualResetEvent done = new ManualResetEvent(false);
            dl.DownloadCompleted = (s, e) => done.Set();
            dl.DownloadProgress = (t,d,progr) => _Reporter.ReportProgress("Downloading...", d, t);
            dl.Start();
            done.WaitOne();
        }

        void Unpack()
        {
            GCCUnpacker unpacker = new GCCUnpacker(_Job.Build.LocalDirectory, _Job, _Reporter, _Toolkit);
            unpacker.UnpackSync();
        }

        void Build()
        {
            GCCToolchainBuilder builder = new GCCToolchainBuilder(_Job.Build.LocalDirectory, _Job, _Toolkit, _ExtraLibs, _Reporter);
            builder.BuildAll();
            ValidateExes();

            GCCPackerClient packer = new GCCPackerClient(_Job.Build.LocalDirectory, _Job, _Job.Target.Target, _Job.Target.UserFriendlyName, _Job.Build.OutputDirectory);

            packer.CreateToolchainArchive(_Job.Build.ExtraFilesDirectory);
        }

        void ValidateExes()
        {
            string buildDir = Path.Combine(_Job.Build.LocalDirectory, _Job.JobString + "-" + _Job.Target.Target);
            string[] exes = Directory.GetFiles(buildDir + @"\bin", "*.exe");
            if (exes.Length < 29)
                throw new Exception("Unexpectedly low amount of exe files in toolchain's bin directory: " + exes.Length);

            string dumpbin = null;
            var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio");
            foreach (var sub in key.GetSubKeyNames())
            {
                double ver;
                if (!double.TryParse(sub, out ver) || ver < 14.0)
                    continue;
                string instDir = key.OpenSubKey(sub).GetValue("InstallDir") as string;
                if (instDir == null)
                    continue;
                string fn = Path.Combine(instDir, @"..\..\vc\bin\dumpbin.exe");
                if (File.Exists(fn))
                {
                    dumpbin = fn;
                    break;
                }
            }

            if (dumpbin == null)
                throw new Exception("Cannot find the dumpbin tool");

            _Reporter.ReportTextLine("Validating built EXE files...");

            foreach (var exe in exes)
            {
                string tmpFile = Path.GetTempFileName();

                string cmdLine = string.Format("set PATH=%PATH%;{0}\\..\\..\\..\\common7\\ide\r\n\"{0}\" /imports \"{1}\" > \"{2}\"", dumpbin, exe, tmpFile);
                string cmdFile = Path.ChangeExtension(tmpFile, ".cmd");
                File.WriteAllText(cmdFile, cmdLine);
                var info = new System.Diagnostics.ProcessStartInfo { FileName = cmdFile };
                info.UseShellExecute = false;
                info.CreateNoWindow = true;
                var proc = System.Diagnostics.Process.Start(info);
                proc.WaitForExit();
                if (proc.ExitCode != 0)
                    throw new Exception("Failed to dump " + exe);
                File.Delete(cmdFile);
                string[] dump = File.ReadAllLines(tmpFile);
                File.Delete(tmpFile);
                int dllsFound = 0;
                foreach (var line in dump)
                {
                    string trimmedLine = line.Trim();
                    if (trimmedLine.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
                    {
                        dllsFound++;
                        if (!File.Exists(Path.Combine(Environment.SystemDirectory, trimmedLine)))
                        {
                            if (!File.Exists(Path.Combine(buildDir, "bin", trimmedLine)))
                                throw new Exception(exe + " is importing " + trimmedLine + " that is not in the system directory");
                        }
                    }
                }

                if (dllsFound < 3)
                    throw new Exception("Could not find any DLLs imported by " + exe);
                _Reporter.ReportTextLine(Path.GetFileName(exe) + " imports from " + dllsFound + " DLLs");

            }
        }

        void Publish()
        {
            string toolchainListFile = Path.Combine(Path.Combine(_Job.Build.OutputDirectory, _Job.Target.Target), "_data.xml");

            List<DownloadableToolchain> toolchains = new List<DownloadableToolchain>();
            if (File.Exists(toolchainListFile))
                toolchains = XmlTools.LoadObject<List<DownloadableToolchain>>(toolchainListFile);
            DownloadableToolchain newToolchain = new DownloadableToolchain
            {
                SmartInstallVersion = 1,
                BinutilsVersion = _Job.Binutils.BinutilsVersion,
                GCCVersion = _Job.GCC.GCCVersion,
                GDBVersion = _Job.GDB.GDBVersion,
                LIBCVersion = _Job.Target.Libc.UserFriendlyVersion
            };

            foreach (var tc in toolchains)
            {
                if (tc.GCCVersion == newToolchain.GCCVersion)
                    return; //Nothing to do
            }

            toolchains.Insert(0, newToolchain);
            XmlTools.SaveObject(toolchains, toolchainListFile);
            XmlTools.SaveObject(toolchains, string.Format("{0}\\{1}\\_data.xml", _Job.Build.OutputDirectory, _Job.Target.Target));
        }
    }
}
