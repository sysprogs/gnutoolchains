using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCCBuilder.Hotfixes
{
    enum GCCBuildStage
    {
        Unknown,
        GCCBuild,
    }

    interface IHotfix
    {
        bool Apply(GCCBuildStage component, string[] LastBuildOutput, string lastBuildDir, BuildJob job, IBuildProgressReporter reporter, ICommandLineService cmdExec);
    }
}
