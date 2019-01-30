using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCCBuilder.Hotfixes
{
    class TooLongLibgccFix : IHotfix
    {
        public bool Apply(GCCBuildStage component, string[] LastBuildOutput, string lastBuildDir, BuildJob job, IBuildProgressReporter reporter, ICommandLineService cmdExec)
        {
            if (component != GCCBuildStage.GCCBuild || LastBuildOutput == null)
                return false;

            bool found = false;
            foreach(string str in LastBuildOutput)
                if (str.Contains("Heap"))
                {
                    found = true;
                    break;
                }

            if (!found)
                return false;

            return false;
        }
    }
}
