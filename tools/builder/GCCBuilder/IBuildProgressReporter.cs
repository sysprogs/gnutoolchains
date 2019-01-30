using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCCBuilder
{
    interface IBuildProgressReporter
    {
        void ReportProgress(string statusText, long progressValue, long progressMax, string optionalComment = null);
        void ReportTextLine(string text);

        EventHandler StopCallback { set; }
    }
}
