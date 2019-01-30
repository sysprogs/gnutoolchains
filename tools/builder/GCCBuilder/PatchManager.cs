using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace GCCBuilder
{
    class PatchManager
    {
        public void PatchIfNeeded(string library, string version, string location)
        {
            try
            {
                string myDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string dir = Path.Combine(myDir, "Patches");
                dir = Path.Combine(dir, library);
                dir = Path.Combine(dir, version);
                if (Directory.Exists(dir))
                {
                    foreach (string fn in Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories))
                    {
                        string newFn = location + fn.Substring(dir.Length);
                        File.Copy(fn, newFn, true);
                    }
                }
            }
            catch 
            {
                return;
            }
        }
    }
}
