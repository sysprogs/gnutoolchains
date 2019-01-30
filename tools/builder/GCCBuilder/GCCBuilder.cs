using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;

namespace GCCBuilder
{
    class GCCBuilder
    {
        BuildJob _Job;
        string _TargetDirectory;

        string _DeployDirectory;
        string _RegistryPrefix;

        string _MinGWPath;

        string _ToolchainPacker;

        readonly int MakeThreads = 16;

        class BuildCommand
        {
            public bool UseShell;
            public bool PreStripExes;
            public string LogFile;
            public string CommandLine;
            public string Directory;
            public string PrimaryOutput;
            public string Comment;
            public string RelativeStatusFile;
        }

        List<BuildCommand> _BuildCommands = new List<BuildCommand>();

        string Win32ToMinGW(string path)
        {
            string prefix = path.Replace('\\', '/') + "/";
            if (prefix[1] == ':')
                prefix = "/" + prefix[0] + prefix.Substring(2);
            return prefix;
        }

        public string JobSuffix
        {
            get
            {
                if (MakeThreads == 1)
                    return "";
                else
                    return " -j" + MakeThreads.ToString();
            }
        }

        bool _NoParallelGCC;

        public GCCBuilder(string targetDirectory, BuildJob job, string target, string MinGWPath, string additionalConfigOptions, int parallelJobs, bool noParallelGCC)
        {
            _Job = job;
            _TargetDirectory = targetDirectory;
            _MinGWPath = MinGWPath;
            _NoParallelGCC = noParallelGCC;
            MakeThreads = parallelJobs;

            _DeployDirectory = Path.Combine(targetDirectory, job.JobString + "-" + target);
            string binutilsDir = Path.Combine(targetDirectory, "binutils-" + job.JobString + "-" + target);
            string gccDir = Path.Combine(targetDirectory, "gcc-" + job.JobString + "-" + target);
            string gdbDir = Path.Combine(targetDirectory, "gdb-" + job.JobString + "-" + target);

            Directory.CreateDirectory(_DeployDirectory);
            Directory.CreateDirectory(binutilsDir);
            Directory.CreateDirectory(gccDir);
            Directory.CreateDirectory(gdbDir);

            _ToolchainPacker = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ToolchainPacker.exe");
            if (!File.Exists(_ToolchainPacker))
                MessageBox.Show("Warning! ToolchainPacker.exe not found!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            string prefix = Win32ToMinGW(_DeployDirectory);

            _RegistryPrefix = string.Format("SysGCC-{0}-{1}", target, job.GCCVersion);
            string gmpPath = Win32ToMinGW(Path.Combine(targetDirectory, "gmp-" + job.GMPVersion));
            string mpfrPath = Win32ToMinGW(Path.Combine(targetDirectory, "mpfr-" + job.MPFRVersion));
            string binutilsCmd = string.Format("../binutils-{0}/configure --target {1} --enable-win32-registry={2} --prefix {3}", _Job.BinutilsVersion, target, _RegistryPrefix, prefix);
            _BuildCommands.Add(new BuildCommand { Directory = binutilsDir, CommandLine = binutilsCmd, PrimaryOutput = Path.Combine(binutilsDir, "Makefile"), UseShell = true, Comment = "Configuring binutils"});
            _BuildCommands.Add(new BuildCommand { Directory = binutilsDir, CommandLine = "make" + JobSuffix, Comment = "Building binutils", RelativeStatusFile = "binutils.build", LogFile = "_make.log" });
            _BuildCommands.Add(new BuildCommand { Directory = binutilsDir, CommandLine = "make install-strip", Comment = "Installing binutils", RelativeStatusFile = "binutils.inst" });

            if (_Job.GlibcVersion != null)
            {
                string gcc0Dir = Path.Combine(targetDirectory, "gccstub-" + job.JobString + "-" + target);
                Directory.CreateDirectory(gcc0Dir);

                string gcc0Cmd = string.Format("../gcc-{0}/configure --target {1} --enable-win32-registry={2} --prefix {3} --enable-languages=c --disable-nls --disable-threads --disable-libgcc", _Job.GCCVersion, target, _RegistryPrefix, prefix, gmpPath, mpfrPath);
                if (!string.IsNullOrEmpty(additionalConfigOptions))
                    gcc0Cmd += " " + additionalConfigOptions;
                _BuildCommands.Add(new BuildCommand { Directory = gcc0Dir, CommandLine = gcc0Cmd, PrimaryOutput = Path.Combine(gcc0Dir, "Makefile"), UseShell = true, Comment = "Configuring GCC-0" });
                _BuildCommands.Add(new BuildCommand { Directory = gcc0Dir, CommandLine = "make" + JobSuffix, Comment = "Building GCC-0", LogFile = "_make.log" });
                _BuildCommands.Add(new BuildCommand { Directory = gcc0Dir, CommandLine = "make install-strip", Comment = "Installing GCC-0" });
            }

            string gccCmd = string.Format("../gcc-{0}/configure --target {1} --enable-win32-registry={2} --prefix {3} --enable-languages=c,c++ --disable-nls", _Job.GCCVersion, target, _RegistryPrefix, prefix, gmpPath, mpfrPath);
            if (_Job.DisableSharedLibraries)
                gccCmd += " --disable-shared";
            if (_Job.NewlibVersion != null)
                gccCmd += string.Format(" --with-newlib --with-headers=../newlib-{0}/newlib/libc/include", _Job.NewlibVersion);
            if (!string.IsNullOrEmpty(additionalConfigOptions))
                gccCmd += " " + additionalConfigOptions;
            _BuildCommands.Add(new BuildCommand { Directory = gccDir, CommandLine = gccCmd, PrimaryOutput = Path.Combine(gccDir, "Makefile"), UseShell = true, Comment = "Configuring GCC" });
            _BuildCommands.Add(new BuildCommand { Directory = gccDir, CommandLine = "make" + (noParallelGCC ? "" : JobSuffix), Comment = "Building GCC", LogFile = "_make.log", RelativeStatusFile = "gcc.build" });
            _BuildCommands.Add(new BuildCommand { Directory = gccDir, CommandLine = "make install-strip", Comment = "Installing GCC", RelativeStatusFile = "gcc.inst" });

            string gdbCmd = string.Format("../gdb-{0}/configure --target {1} --prefix {2} CFLAGS=-fno-omit-frame-pointer", _Job.GDBVersion, target, prefix);
            _BuildCommands.Add(new BuildCommand { Directory = gdbDir, CommandLine = gdbCmd, PrimaryOutput = Path.Combine(gdbDir, "Makefile"), UseShell = true, Comment = "Configuring GDB" });
            _BuildCommands.Add(new BuildCommand { Directory = gdbDir, CommandLine = "make" + JobSuffix, Comment = "Building GDB", LogFile = "_make.log", RelativeStatusFile = "gdb.build" });
            _BuildCommands.Add(new BuildCommand { Directory = gdbDir, CommandLine = "make install", Comment = "Installing GDB", RelativeStatusFile = "gdb.inst", PreStripExes = true });

            if (_Job.NewlibVersion != null)
            {
                string newlibDir = Path.Combine(targetDirectory, "newlib-" + job.JobString + "-" + target);
                Directory.CreateDirectory(newlibDir);

                string newlibCmd = string.Format("../newlib-{0}/configure --target={1} --prefix={2}", _Job.NewlibVersion, target, prefix);
                _BuildCommands.Add(new BuildCommand { Directory = newlibDir, CommandLine = newlibCmd, PrimaryOutput = Path.Combine(newlibDir, "Makefile"), UseShell = true, Comment = "Configuring newlib" });
                _BuildCommands.Add(new BuildCommand { Directory = newlibDir, CommandLine = "make" + JobSuffix, Comment = "Building newlib", LogFile = "_make.log", RelativeStatusFile = "newlib.build" });
                _BuildCommands.Add(new BuildCommand { Directory = newlibDir, CommandLine = "make install", Comment = "Installing newlib", RelativeStatusFile = "newlib.inst" });
            }
        }

        int _CurrentJob = -1;

        public delegate void BuildCompleteHandler(GCCBuilder builder, int exitCode);
        public BuildCompleteHandler BuildComplete;

        public delegate void StringHandler(GCCBuilder builder, string jobName);
        public StringHandler JobStarted, LineReceived;

        public void Start()
        {
            _CurrentJob = -1;
            StartNextJob();
        }

        void StartNextJob()
        {
            if (++_CurrentJob >= _BuildCommands.Count)
            {
                if (BuildComplete != null)
                    BuildComplete(this, 0);
                return;
            }

            var cmd = _BuildCommands[_CurrentJob];

            if (cmd.PrimaryOutput != null && File.Exists(cmd.PrimaryOutput))
            {
                StartNextJob();
                return;
            }

            if (cmd.RelativeStatusFile != null)
            {
                string fn = Path.Combine(cmd.Directory, cmd.RelativeStatusFile);
                if (File.Exists(fn))
                {
                    StartNextJob();
                    return;
                }
            }

            if (cmd.PreStripExes)
            {
                string stripDir = Path.Combine(_MinGWPath, "bin");
                string strip = Path.Combine(stripDir, "strip.exe");
                foreach (var fn in Directory.EnumerateFiles(cmd.Directory, "*.exe", SearchOption.AllDirectories))
                {
                    Process.Start(new ProcessStartInfo { FileName = strip, Arguments = fn, CreateNoWindow = true, UseShellExecute = false }).WaitForExit();
                }
            }

            if (JobStarted != null)
                JobStarted(this, cmd.Comment);

            string path = string.Format(@"{0}\bin;{0}\msys\1.0\bin;{1};{2}\bin", _MinGWPath, Environment.GetEnvironmentVariable("PATH"), _DeployDirectory);
            Process proc = new Process();
            proc.StartInfo.FileName = "cmd.exe";
            string shell = "";
            if (cmd.UseShell)
                shell = _MinGWPath + @"\msys\1.0\bin\sh.exe ";

            proc.StartInfo.Arguments = "/c " +  shell + cmd.CommandLine;
            if (cmd.LogFile != null)
                proc.StartInfo.Arguments += string.Format(" > {0} 2>&1", cmd.LogFile);
            proc.StartInfo.WorkingDirectory = cmd.Directory;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.EnvironmentVariables["PATH"] = path;
            //proc.StartInfo.RedirectStandardError = proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.CreateNoWindow = true;
            proc.Exited += new EventHandler(proc_Exited);
            proc.EnableRaisingEvents = true;
//             proc.OutputDataReceived += new DataReceivedEventHandler(proc_OutputDataReceived);
//             proc.ErrorDataReceived += new DataReceivedEventHandler(proc_ErrorDataReceived);

            if (LineReceived != null)
                LineReceived(this, proc.StartInfo.FileName + " " + proc.StartInfo.Arguments);

            if (cmd.LogFile != null)
            {
                _LogReadingThread = new Thread(LogReadingThread);
                _LogReadingThread.Start(Path.Combine(cmd.Directory, cmd.LogFile));
            }
            else
                _LogReadingThread = null;

            proc.Start();
            
//            proc.BeginErrorReadLine();
//            proc.BeginOutputReadLine();
        }

        Thread _LogReadingThread;

        void LogReadingThread(object arg)
        {
            int cnt = 0;
            for (; ; )
            {
                Thread.Sleep(100);
                List<string> lines = new List<string>();
                using(var fs = File.Open((string)arg, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                        lines.Add(sr.ReadLine());
                }
                
                if (lines.Count < 2)
                    continue;

                while (cnt < (lines.Count - 1))
                    if (LineReceived != null)
                        LineReceived(this, lines[cnt++]);
            }
        }

        void proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                if (LineReceived != null)
                    LineReceived(this, e.Data);
        }

        void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                if (LineReceived != null)
                    LineReceived(this, e.Data);
        }

        void proc_Exited(object sender, EventArgs e)
        {
            if (_LogReadingThread != null)
            {
                _LogReadingThread.Abort();
                _LogReadingThread = null;
            }

            if ((sender as Process).ExitCode != 0)
            {
                if (BuildComplete != null)
                    BuildComplete(this, (sender as Process).ExitCode);
                return;
            }

            var cmd = _BuildCommands[_CurrentJob];
            if (cmd.RelativeStatusFile != null)
            {
                string fn = Path.Combine(cmd.Directory, cmd.RelativeStatusFile);
                File.WriteAllText(fn, DateTime.Now.ToString());
            }
            StartNextJob();
        }
    }
}
