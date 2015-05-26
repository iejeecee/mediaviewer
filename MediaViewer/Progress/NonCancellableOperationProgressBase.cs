using MediaViewer.Model.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Progress
{
    public class NonCancellableOperationProgressBase : CloseableBindableBase
    {
        string windowTitle;

        public string WindowTitle
        {
            get { return windowTitle; }
            set { SetProperty(ref windowTitle, value); }
        }

        string windowIcon;

        public string WindowIcon
        {
            get { return windowIcon; }
            set { SetProperty(ref windowIcon, value); }
        }

        int totalProgress;

        public int TotalProgress
        {
            get { return totalProgress; }
            set { SetProperty(ref totalProgress, value); }
        }

        int totalProgressMax;

        public int TotalProgressMax
        {
            get { return totalProgressMax; }
            set { SetProperty(ref totalProgressMax, value); }
        }
       
    }
}
