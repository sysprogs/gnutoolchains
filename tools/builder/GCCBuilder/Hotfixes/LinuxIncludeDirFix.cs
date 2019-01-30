using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GCCBuilder.Hotfixes
{
    class LinuxIncludeDirFix : IHotfix
    {
        public bool Apply(GCCBuildStage component, string[] LastBuildOutput, string lastBuildDir, BuildJob job, IBuildProgressReporter reporter, ICommandLineService cmdExec)
        {
            if (component != GCCBuildStage.GCCBuild)
                return false;
            if (LastBuildOutput == null && !string.IsNullOrEmpty(job.Target.SysrootSuffix))
            {
                string fixFile = Path.Combine(lastBuildDir, "gcc\\sysprogs-includefix.h");
                Directory.CreateDirectory(Path.GetDirectoryName(fixFile));

                string fn = Path.Combine(lastBuildDir, "..\\gcc-" + job.GCC.GCCVersion + @"\gcc\cppdefault.c");
                List<string> contents = new List<string>(File.ReadAllLines(fn));
                bool modified = false;
                for (int i = 0; i < contents.Count; i++)
                    if (contents[i].Contains("cpp_include_defaults[]"))
                    {
                        if (!contents[i - 1].Contains("sysprogs"))
                        {
                            contents.Insert(i, "#include \"sysprogs-includefix.h\"");
                            modified = true;
                        }
                        break;
                    }

                if (File.Exists(fixFile) && !modified)
                    return false;   //Already applied

                File.WriteAllLines(fn, contents.ToArray());
                reporter.ReportTextLine("LinuxIncludeDirFix: created sysprogs-includefix.h");
                File.WriteAllText(fixFile, string.Format("#pragma once\n#if defined(__MINGW32__) && defined(TARGET_SYSTEM_ROOT)\n#define NATIVE_SYSTEM_HEADER_DIR \"/usr/include\"\n#define LOCAL_INCLUDE_DIR \"/usr/include/{0}\"\n#endif\n", job.Target.Target));

                return true;
            }
            return false;
        }
    }
}
