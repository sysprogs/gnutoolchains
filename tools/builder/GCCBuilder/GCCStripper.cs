using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace GCCBuilder
{
    class GCCStripper
    {
        BuildJob _Job;
        string _Target;
        string _TargetDirectory;
        string _MingwDir;

        public GCCStripper(string targetDirectory, BuildJob job, string target, string MingwDir)
        {
            _Job = job;
            _TargetDirectory = targetDirectory;
            _MingwDir = MingwDir;
            _Target = target;
        }

        bool AreFilesEqual(string fn1, string fn2)
        {
            if (fn1 == null || fn2 == null)
                return false;

            using(var fs1 = File.OpenRead(fn1))
            using (var fs2 = File.OpenRead(fn2))
            {
                if (fs1.Length != fs2.Length)
                    return false;
                byte[] data1 = new byte[fs1.Length], data2 = new byte[fs1.Length];
                fs1.Read(data1, 0, (int)fs1.Length);
                fs2.Read(data2, 0, (int)fs2.Length);

                for (int i = 0; i < data1.Length; i++)
                    if (data1[i] != data2[i])
                        return false;
                return true;
            }
        }

        public void StripAndCopyMake(string makeExe)
        {
            //Stripping is now done with make install-strip
            string binaryDir = Path.Combine(_TargetDirectory, _Job.JobString + "-" + _Target);

            /*string stripDir = Path.Combine(_MingwDir, "bin");
            string strip = Path.Combine(stripDir, "strip.exe");

            List<string> allFiles = new List<string>();

            foreach (var fn in Directory.EnumerateFiles(binaryDir, "*.exe", SearchOption.AllDirectories))
                allFiles.Add(fn);*/

            try
            {
                /*for (int i = 0; i < allFiles.Count; i++)
                {
                    if (allFiles[i] == null)
                        continue;

                    List<string> copies = new List<string>();

                    for (int j = i + 1; j < allFiles.Count; j++)
                    {
                        if (AreFilesEqual(allFiles[i], allFiles[j]))
                        {
                            copies.Add(allFiles[j]);
                            allFiles[j] = null;
                        }
                    }

                    string fn = allFiles[i];
                    Process.Start(new ProcessStartInfo { FileName = strip, Arguments = fn, CreateNoWindow = true, UseShellExecute = false }).WaitForExit();
                    foreach (string copy in copies)
                        File.Copy(fn, copy, true);
                }*/

                if (makeExe != null)
                    File.Copy(makeExe, Path.Combine(binaryDir, "bin\\make.exe"), true);
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
        }
    }
}
