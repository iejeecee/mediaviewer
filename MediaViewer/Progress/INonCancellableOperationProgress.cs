using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Progress
{
    interface INonCancellableOperationProgress
    {
        string WindowTitle { get; set; }
        int TotalProgress { get; set; }
        int TotalProgressMax { get; set; }
    }
}
