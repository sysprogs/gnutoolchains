using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Tar;
using System.IO;
using ICSharpCode.SharpZipLib.BZip2;
using System.Threading;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.GZip;
using System.Diagnostics;

namespace GCCBuilder
{
    class GCCUnpacker
    {
        BuildJob _Job;
        string _TargetDirectory;
        IBuildProgressReporter _Reporter;

        public GCCUnpacker(string targetDirectory, BuildJob job, IBuildProgressReporter reporter, BuildToolkit toolkit)
        {
            _Job = job;
            _TargetDirectory = targetDirectory;
            _Reporter = reporter;
            _Toolkit = toolkit;
        }

        public void UnpackSync()
        {
            string tagFile = Path.Combine(_TargetDirectory, _Job.JobString) + ".unpack";
            if (File.Exists(tagFile))
            {
                _Reporter.ReportTextLine(string.Format("Found {0}. Skipping unpack phase...", tagFile));
                return;
            }

            UnpackTGZ(string.Format("{0}\\gcc-{1}.tar.gz", _TargetDirectory, _Job.GCC.GCCVersion), _TargetDirectory);

            string gccSubdir = string.Format("gcc-{0}", _Job.GCC.GCCVersion);
            string gccDir = Path.Combine(_TargetDirectory, gccSubdir);

            UnpackTBZ(string.Format("{0}\\gmp-{1}.tar.bz2", _TargetDirectory, _Job.GCC.GMPVersion), _TargetDirectory);
            UnpackTBZ(string.Format("{0}\\mpfr-{1}.tar.bz2", _TargetDirectory, _Job.GCC.MPFRVersion), _TargetDirectory);
            UnpackTGZ(string.Format("{0}\\mpc-{1}.tar.gz", _TargetDirectory, _Job.GCC.MPCVersion), _TargetDirectory);
            UnpackTGZ(string.Format("{0}\\gdb-{1}.tar.gz", _TargetDirectory, _Job.GDB.GDBVersion), _TargetDirectory);

            string patchFile = string.Format(@"{0}\_patches\{1}\gcc.patch", _TargetDirectory, _Job.Target.Target);
            PatchFolderIfNeeded(string.Format(@"{0}\gcc-{1}", _TargetDirectory, _Job.GCC.GCCVersion), patchFile);

            if (_Job.Target.Libc.Type == BuildJob.LIBCType.Glibc)
                UnpackTBZ(string.Format("{0}\\glibc-{1}.tar.bz2", _TargetDirectory, _Job.Target.Libc.Version), _TargetDirectory);

            if (_Job.Target.Libc.Type == BuildJob.LIBCType.Newlib)
            {
                UnpackTGZ(string.Format("{0}\\newlib-{1}.tar.gz", _TargetDirectory, _Job.Target.Libc.Version), _TargetDirectory);
                patchFile = string.Format(@"{0}\_patches\{1}\newlib.patch", _TargetDirectory, _Job.Target.Target);
                PatchFolderIfNeeded(string.Format(@"{0}\newlib-{1}", _TargetDirectory, _Job.Target.Libc.Version), patchFile);
            }

            try
            {
                Directory.Move(string.Format("{0}\\gmp-{1}", _TargetDirectory, _Job.GCC.GMPVersion), Path.Combine(gccDir, "gmp"));
                Directory.Move(string.Format("{0}\\mpfr-{1}", _TargetDirectory, _Job.GCC.MPFRVersion), Path.Combine(gccDir, "mpfr"));
                Directory.Move(string.Format("{0}\\mpc-{1}", _TargetDirectory, _Job.GCC.MPCVersion), Path.Combine(gccDir, "mpc"));
            }
            catch (System.Exception)
            {

            }

            UnpackTBZ(string.Format("{0}\\binutils-{1}.tar.bz2", _TargetDirectory, _Job.Binutils.BinutilsVersion), _TargetDirectory);

            File.WriteAllText(tagFile, DateTime.Now.ToString());
        }

        private void PatchFolderIfNeeded(string folder, string patchFile)
        {
            string patch = Path.Combine(_Toolkit.RootDir, "bin\\patch.exe");
            if (File.Exists(patchFile))
            {
                var proc = Process.Start(new ProcessStartInfo { FileName = "cmd.exe", WorkingDirectory = folder, Arguments = string.Format("/c {0} -p1 < {1} > ..\\{2}-patch.log", patch, patchFile.Replace('\\', '/'), Path.GetFileName(folder)) });
                proc.WaitForExit();
                if (proc.ExitCode != 0)
                    throw new Exception("Failed to patch " + folder);
            }
        }


        PatchManager _Patcher = new PatchManager();
        private BuildToolkit _Toolkit;

        void RunCygwinProcessSync(string dir, string cmdline)
        {
            Process proc = new Process();
            _Reporter.ReportTextLine(cmdline);

            proc.StartInfo.FileName = $@"{BuildManager.CygwinDirectory}\bin\bash.exe";
            proc.StartInfo.Arguments = $"--login -c \"cd {dir.Replace('\\', '/')} && {cmdline}\"";
            proc.Start();
            proc.WaitForExit();
            if (proc.ExitCode != 0)
                throw new Exception("Failed to launch " + cmdline);
        }

        void UnpackTBZ(string fn, string targetDirectory)
        {
            _Reporter.ReportTextLine(string.Format("Unpacking {0} to {1}...", fn, targetDirectory));
            RunCygwinProcessSync(targetDirectory, $"tar xf {MakeCygwinPath(fn)}");
#if UNUSED
            using (var strm = File.OpenRead(fn))
                UnpackTar(new BZip2InputStream(strm), targetDirectory);
#endif

        }

        void UnpackTGZ(string fn, string targetDirectory)
        {
            _Reporter.ReportTextLine(string.Format("Unpacking {0} to {1}...", fn, targetDirectory));
            RunCygwinProcessSync(targetDirectory, $"tar xf {MakeCygwinPath(fn)}");
#if UNUSED
            using (var strm = File.OpenRead(fn))
                UnpackTar(new GZipInputStream(strm), targetDirectory);
#endif
        }

        private object MakeCygwinPath(string fn)
        {
            return $"/cygdrive/{fn[0]}/{fn.Substring(3).Replace('\\', '/')}";
        }

        void UnpackTar(Stream strm, string targetDirectory)
        {
            TarInputStream tar = new TarInputStream(strm);

            byte[] tempBuf = new byte[65536];
            //int done = 0;
            for (; ; )
            {
                TarEntry entry = tar.GetNextEntry();
                if (entry == null)
                    break;

                string strName = entry.Name;
                /*string firstComponent = strName.Substring(0, strName.IndexOf('/'));
                if (firstComponent.ToUpper() == m_NamePrefixToDrop.ToUpper())
                    strName = strName.Substring(m_NamePrefixToDrop.Length + 1);*/

                if (strName == "")
                    continue;

                if (entry.IsDirectory)
                    Directory.CreateDirectory(targetDirectory + "\\" + strName);
                else
                {
                    string fn = Path.Combine(targetDirectory, strName);
                    string dir = Path.GetDirectoryName(fn);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    if (Directory.Exists(fn))
                        Directory.Delete(fn);

                    using (FileStream ostrm = new FileStream(fn, FileMode.Create))
                        tar.CopyEntryContents(ostrm);
                    File.SetLastWriteTime(fn, entry.ModTime);
                }

                _Reporter.ReportProgress("Unpacking...", strm.Position, strm.Length);
            }
        }

    }
}
