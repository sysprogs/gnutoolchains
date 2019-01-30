using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;
using GCCBuilder.Hotfixes;
using System.Runtime.InteropServices;

namespace GCCBuilder
{
    interface ICommandLineService
    {
        int RunCommandLineSync(string command, string directory, string logFileName, bool useMinGWShell, GCCBuildStage stage, Dictionary<string, string> env = null);
    }

    struct BuildToolkit
    {
        public string RootDir;
        public bool IsCygwin;
    }

    class GCCToolchainBuilder : ICommandLineService
    {
        BuildJob _Job;
        string _TargetDirectory;

        string _DeployDirectory;
        string _RegistryPrefix;

        BuildToolkit _Toolkit;
        string _ExtraLibsPath;

        string _ToolchainPacker;

        class BuildCommand
        {
            public string CommandLine;
            public string Directory;

            public bool UseShell;
            public bool PreStripExes;
            public string PrimaryOutput;
            public string Comment;
            public string RelativeStatusFile;

            public GCCBuildStage Stage = GCCBuildStage.Unknown;
            public Dictionary<string, string> Environment = new Dictionary<string, string>();
        }

        List<BuildCommand> _BuildCommands = new List<BuildCommand>();

        string Win32ToGNU(string path)
        {
            string prefix = path.Replace('\\', '/') + "/";
            if (prefix[1] == ':')
            {
                prefix = "/" + prefix[0] + prefix.Substring(2);
            }
            return prefix;
        }

        public string JobSuffix
        {
            get
            {
                if (_Job.ParalleliationLevel <= 1)
                    return "";
                else
                    return " -j" + _Job.ParalleliationLevel.ToString();
            }
        }

        string _EnvPath;
        IBuildProgressReporter _Reporter;

        public GCCToolchainBuilder(string targetDirectory, BuildJob job, BuildToolkit toolkit, string extraLibsPath, IBuildProgressReporter reporter)
        {
            _Job = job;
            _TargetDirectory = targetDirectory;
            _Toolkit = toolkit;
            _ExtraLibsPath = extraLibsPath;
            _Reporter = reporter;
            _HostSpecifier = _Toolkit.IsCygwin ? "--host i686-w64-mingw32" : "";


            var target = job.Target.Target;
            _DeployDirectory = Path.Combine(targetDirectory, job.JobString + "-" + target);
            if (!_Toolkit.IsCygwin)
                _EnvPath = string.Format(@"{0}\bin;{0}\msys\1.0\bin;{1};{2}\bin", _Toolkit.RootDir, Environment.GetEnvironmentVariable("PATH"), _DeployDirectory);
            else
                _EnvPath = string.Format(@"{0}\bin;{1};{2}\bin", _Toolkit.RootDir, Environment.GetEnvironmentVariable("PATH"), _DeployDirectory);

            string gccDir = Path.Combine(targetDirectory, "gcc-" + job.JobString + "-" + target);
            string gdbDir = Path.Combine(targetDirectory, "gdb-" + job.JobString + "-" + target);

            Directory.CreateDirectory(_DeployDirectory);
            Directory.CreateDirectory(gccDir);
            Directory.CreateDirectory(gdbDir);

            TouchInfoFiles(string.Format(@"{0}\binutils-{1}", targetDirectory, _Job.Binutils.BinutilsVersion));
            TouchInfoFiles(string.Format(@"{0}\gcc-{1}", targetDirectory, _Job.GCC.GCCVersion));
            TouchInfoFiles(string.Format(@"{0}\gdb-{1}", targetDirectory, _Job.GDB.GDBVersion));
            if (_Job.Target.Libc.Type == BuildJob.LIBCType.Newlib)
                TouchInfoFiles(string.Format(@"{0}\newlib-{1}", targetDirectory, _Job.Target.Libc.Version));

            _ToolchainPacker = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ToolchainPacker.exe");
            if (!File.Exists(_ToolchainPacker))
                MessageBox.Show("Warning! ToolchainPacker.exe not found!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            string prefix = Win32ToGNU(_DeployDirectory);

            _RegistryPrefix = string.Format("SysGCC-{0}-{1}", target, job.GCC.GCCVersion);
            string gmpPath = Win32ToGNU(Path.Combine(targetDirectory, "gmp-" + job.GCC.GMPVersion));
            string mpfrPath = Win32ToGNU(Path.Combine(targetDirectory, "mpfr-" + job.GCC.MPFRVersion));
            string sysrootArgs = "";

            if (!string.IsNullOrEmpty(job.Target.SysrootSuffix))
                sysrootArgs = string.Format(" --with-sysroot={0}{1}/{2}", prefix, target, job.Target.SysrootSuffix);

            QueueBinutilsBuildCommands(target, prefix, sysrootArgs, Path.Combine(targetDirectory, "binutils-" + job.JobString + "-" + target), false);

            if (_Toolkit.IsCygwin)
            {
                QueueBinutilsBuildCommands(target, null, sysrootArgs, Path.Combine(targetDirectory, "xbinutils-" + job.JobString + "-" + target),true);
                QueueGCCBuildCommands("xgcc", targetDirectory, job, target, null, sysrootArgs);
            }

            if (_Job.Target.Libc.Type == BuildJob.LIBCType.Glibc)
            {
                throw new NotImplementedException();
            }

            QueueGCCBuildCommands("gcc", targetDirectory, job, target, prefix, sysrootArgs);
            if (_Job.Target.Target == "arm-eabi")
            {
                //QueueGCCBuildCommands("gcc-noexcept", targetDirectory, job, target, prefix, sysrootArgs, false, "-g -Os -ffunction-sections -fdata-sections -fno-exceptions");
            }

            string gdbCmd = string.Format("../gdb-{0}/configure --target {1} --prefix {2} CFLAGS=-fno-omit-frame-pointer --disable-nls --without-libiconv-prefix {3}", _Job.GDB.GDBVersion, target, prefix, _HostSpecifier);
            gdbCmd += sysrootArgs;
            if (_Job.Target.XMLSupportInGDB)
                gdbCmd += " --with-expat=yes --with-libexpat-prefix=" + _ExtraLibsPath + "/expat-root/";
            _BuildCommands.Add(new BuildCommand { Directory = gdbDir, CommandLine = gdbCmd, PrimaryOutput = Path.Combine(gdbDir, "Makefile"), UseShell = true, Comment = "Configuring GDB" });
            _BuildCommands.Add(new BuildCommand { Directory = gdbDir, CommandLine = "make" + JobSuffix, Comment = "Building GDB", RelativeStatusFile = "gdb.build" });
            _BuildCommands.Add(new BuildCommand { Directory = gdbDir, CommandLine = "make install", Comment = "Installing GDB", RelativeStatusFile = "gdb.inst", PreStripExes = true });

            if (_Job.Target.Libc.Type == BuildJob.LIBCType.Newlib)
            {
                string newlibDir = Path.Combine(targetDirectory, "newlib-" + job.JobString + "-" + target);
                Directory.CreateDirectory(newlibDir);

                string newlibCmd = string.Format("../newlib-{0}/configure --target={1} --prefix={2}", _Job.Target.Libc.Version, target, prefix);
                _BuildCommands.Add(new BuildCommand { Directory = newlibDir, CommandLine = newlibCmd, PrimaryOutput = Path.Combine(newlibDir, "Makefile"), UseShell = true, Comment = "Configuring newlib" });
                _BuildCommands.Add(new BuildCommand { Directory = newlibDir, CommandLine = "make" + JobSuffix, Comment = "Building newlib",  RelativeStatusFile = "newlib.build" });
                _BuildCommands.Add(new BuildCommand { Directory = newlibDir, CommandLine = "make install", Comment = "Installing newlib", RelativeStatusFile = "newlib.inst" });
            }
        }

        private void QueueBinutilsBuildCommands(string target, string prefix, string sysrootArgs, string binutilsDir, bool isXbinutils)
        {
            Directory.CreateDirectory(binutilsDir);
            string binutilsCmd;
            if (prefix == null)
                binutilsCmd = string.Format("../binutils-{0}/configure --target {1} --enable-win32-registry={2} --disable-nls --without-libiconv-prefix", _Job.Binutils.BinutilsVersion, target, _RegistryPrefix);
            else
                binutilsCmd = string.Format("../binutils-{0}/configure --target {1} --enable-win32-registry={2} --prefix {3} --disable-nls --without-libiconv-prefix {4}", _Job.Binutils.BinutilsVersion, target, _RegistryPrefix, prefix, _HostSpecifier);

            string pkgName = isXbinutils ? "xbinutils" : "binutils";
            if (isXbinutils)
                binutilsCmd += " --disable-werror";

            binutilsCmd += sysrootArgs;

            _BuildCommands.Add(new BuildCommand { Directory = binutilsDir, CommandLine = binutilsCmd, PrimaryOutput = Path.Combine(binutilsDir, "Makefile"), UseShell = true, Comment = "Configuring " + pkgName });
            _BuildCommands.Add(new BuildCommand { Directory = binutilsDir, CommandLine = "make" + JobSuffix, Comment = "Building " + pkgName, RelativeStatusFile = "binutils.build" });
            _BuildCommands.Add(new BuildCommand { Directory = binutilsDir, CommandLine = "make install-strip", Comment = "Installing " + pkgName, RelativeStatusFile = "binutils.inst" });
        }

        private void QueueGCCBuildCommands(string moduleName, string targetDirectory, BuildJob job, string target, string prefix, string sysrootArgs, bool install = true, string cxxflagsForTarget = null)
        {
            string gcc0Dir = Path.Combine(targetDirectory, moduleName + "-" + job.JobString + "-" + target);
            Directory.CreateDirectory(gcc0Dir);

            string gccCmd = string.Format("../gcc-{0}/configure --target {1} --enable-win32-registry={2} --enable-languages=c,c++ --disable-nls --without-libiconv-prefix", _Job.GCC.GCCVersion, target, _RegistryPrefix);

            if (prefix != null)
                gccCmd += string.Format(" --prefix {0} {1}", prefix, _HostSpecifier);

            if (_Job.Target.DisableSharedLibraries)
                gccCmd += " --disable-shared";
            if (_Job.Target.Libc.Type == BuildJob.LIBCType.Newlib)
                gccCmd += string.Format(" --with-newlib --with-headers=../newlib-{0}/newlib/libc/include", _Job.Target.Libc.Version);
            if (!string.IsNullOrEmpty(job.Target.AdditionalArgs))
                gccCmd += " " + job.Target.AdditionalArgs;
            gccCmd += sysrootArgs;

            var cmd = new BuildCommand { Directory = gcc0Dir, CommandLine = gccCmd, PrimaryOutput = Path.Combine(gcc0Dir, "Makefile"), UseShell = true, Comment = "Configuring " + moduleName };
            if (cxxflagsForTarget != null)
            {
                cmd.Environment["CXXFLAGS_FOR_TARGET"] = cxxflagsForTarget;
            }
            _BuildCommands.Add(cmd);
            _BuildCommands.Add(new BuildCommand { Directory = gcc0Dir, CommandLine = "make" + JobSuffix, Comment = "Building " + moduleName, RelativeStatusFile = "gcc.build", Stage = GCCBuildStage.GCCBuild });
            
            if (install)
                _BuildCommands.Add(new BuildCommand { Directory = gcc0Dir, CommandLine = "make install-strip", Comment = "Installing " + moduleName, RelativeStatusFile = "gcc.inst" });
        }

        private void TouchInfoFiles(string dir)
        {
            foreach (var fn in Directory.GetFiles(dir, "*.info", SearchOption.AllDirectories))
            {
                File.SetLastWriteTime(fn, DateTime.Now.AddYears(1));    //Fix dependency bug in Cygwin
            }
        }

        public void BuildAll()
        {
            foreach (var cmd in _BuildCommands)
                RunCmdSync(cmd);
        }


        void RunCmdSync(BuildCommand cmd)
        {
            string statusFile = null;
            if (cmd.RelativeStatusFile != null)
                statusFile = Path.Combine(cmd.Directory, cmd.RelativeStatusFile);

            if (cmd.PrimaryOutput != null && File.Exists(cmd.PrimaryOutput))
            {
                _Reporter.ReportTextLine("Found " + cmd.PrimaryOutput + ". Skipping " + cmd.CommandLine);
                return;
            }

            if (statusFile != null && File.Exists(statusFile))
            {
                _Reporter.ReportTextLine("Found " + statusFile + ". Skipping " + cmd.CommandLine);
                return;
            }

            if (cmd.PreStripExes)
            {
                string stripDir = Path.Combine(_Toolkit.RootDir, "bin");
                string strip = Path.Combine(stripDir, "strip.exe");
                foreach (var fn in Directory.EnumerateFiles(cmd.Directory, "*.exe", SearchOption.AllDirectories))
                {
                    _Reporter.ReportTextLine("Stripping " + fn);
                    Process.Start(new ProcessStartInfo { FileName = strip, Arguments = fn, CreateNoWindow = true, UseShellExecute = false }).WaitForExit();
                }
            }

            for (int i = 1; i < 100; i++)
            {
                _Reporter.ReportProgress(cmd.CommandLine, 0, 0, cmd.Comment);
                _Reporter.ReportTextLine(cmd.CommandLine + "[iteration " + i + "]");

                int rc = RunCommandLineSync(cmd.CommandLine, cmd.Directory, "_lastcmd.log", cmd.UseShell, cmd.Stage, cmd.Environment);
                if (rc != 0)
                {
                    string logFile = Path.Combine(cmd.Directory, "_lastcmd.log");
                    try
                    {
                        if (File.Exists(logFile))
                        {
                            string text = File.ReadAllText(logFile);
                            if (text.Contains("couldn't commit memory for cygwin heap") || text.Contains("fork: Resource temporarily unavailable") || text.Contains("jobserver tokens available") || text.Contains("Segmentation fault"))
                            {
                                Thread.Sleep(10000);
                                continue;
                            }
                        }
                    }
                    catch
                    {
                    	
                    }

                    throw new Exception("Build failed - exit code " + rc.ToString());
                }

                break;
            }

            if (statusFile != null)
                File.WriteAllText(statusFile, DateTime.Now.ToString());
        }

        class LogContext
        {
            public string FileName;
            public ManualResetEvent Abort = new ManualResetEvent(false);
        }


        void LogReadingThread(object arg)
        {
            LogContext ctx = (LogContext)arg;
            int cnt = 0;
            for (; ; )
            {
                if (ctx.Abort.WaitOne(100))
                    return;

                if (!File.Exists(ctx.FileName))
                    continue;

                List<string> lines = new List<string>();
                using (var fs = File.Open(ctx.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                    for (; ; )
                    {
                        string str = sr.ReadLine();
                        if (str == null)
                            break;
                        lines.Add(str);
                    }


                if (lines.Count < 2)
                    continue;

                while (cnt < (lines.Count - 1))
                    _Reporter.ReportTextLine(lines[cnt++]);
            }
        }

        IHotfix[] _Hotfixes = new IHotfix[] { new LinuxIncludeDirFix(), new TooLongLibgccFix() };
        private string _HostSpecifier;

        [DllImport("user32")]
        static extern void ShowWindow(IntPtr hwnd, int state);

        public int RunCommandLineSync(string command, string directory, string logFileName, bool useShell, GCCBuildStage stage, Dictionary<string, string> env = null)
        {
            for (int i = 0; ;i++ )
            {
                Process proc = new Process();
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.Arguments = "/c " + command;

                if (_Toolkit.IsCygwin)
                {
                    proc.StartInfo.Arguments = string.Format("/c {0}\\bin\\sh.exe --login -c \"cd {1} && {2}\"", _Toolkit.RootDir, Win32ToGNU(directory), command);
                }
                else
                {
                    if (useShell)
                    {
                        string shell = _Toolkit.RootDir + @"\msys\1.0\bin\sh.exe ";
                        proc.StartInfo.Arguments = "/c " + shell + " " + command;
                    }
                }


                if (logFileName != null)
                    proc.StartInfo.Arguments += string.Format(" > {0} 2>&1", logFileName);
                proc.StartInfo.WorkingDirectory = directory;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.EnvironmentVariables["PATH"] = _EnvPath;
                //proc.StartInfo.CreateNoWindow = true; //Breaks cygwin

                if (env != null)
                    foreach (var kv in env)
                        proc.StartInfo.EnvironmentVariables[kv.Key] = kv.Value; 

                if (i == 0)
                    foreach (var hotfix in _Hotfixes)
                        hotfix.Apply(stage, null, directory, _Job, _Reporter, this);

                var vgagent = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "vgagent.exe");
                bool usingVgagent = false;
                if (File.Exists(vgagent))
                {
                    usingVgagent = true;
                    proc.StartInfo.Arguments = proc.StartInfo.FileName + " " + proc.StartInfo.Arguments;
                    proc.StartInfo.FileName = vgagent;
                }

                _Reporter.ReportTextLine("Launching " + proc.StartInfo.FileName + " " + proc.StartInfo.Arguments);
                LogContext ctx = null;
                string logFN = null;

                if (logFileName != null)
                {
                    var thr = new Thread(LogReadingThread);
                    thr.Start(ctx = new LogContext { FileName = logFN = Path.Combine(directory, logFileName) });
                }

                proc.Start();
                if (usingVgagent)
                    _Reporter.StopCallback = delegate(object sender, EventArgs args)
                    {
                        Semaphore sem = new Semaphore(0, int.MaxValue, string.Format("VisualGDBAgent_Break_{0}", proc.Id));
                        sem.Release();
                        sem.Close();
                    };

                if (!proc.WaitForExit(500))
                {
                    var hwnd = proc.MainWindowHandle;
                    ShowWindow(hwnd, 0);    //Hide the console window
                }

                proc.WaitForExit();
                _Reporter.StopCallback = null;
                if (ctx != null)
                    ctx.Abort.Set();

                if (i == 0 && proc.ExitCode != 0)
                {
                    int hotfixes = 0;
                    if (i == 0)
                    {
                        string[] buildLog = null;
                        if (logFN != null)
                            buildLog = File.ReadAllLines(logFN);

                        foreach (var hotfix in _Hotfixes)
                            if (hotfix.Apply(stage, null, directory, _Job, _Reporter, this))
                                hotfixes++;
                    }

                    if (hotfixes > 0)
                    {
                        _Reporter.ReportTextLine(string.Format("Applied {0} hotfixes. Trying again...", hotfixes));
                        continue;
                    }

                }

                return proc.ExitCode;
            }
        }
    }
}
