using Aga.Controls.Tree;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MediaViewer.DirectoryBrowser
{
    class DirectoryBrowserViewModel : ObservableObject, ITreeModel
    {
        protected static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public delegate void PathSelectedDelegate(PathModel item);
        public event EventHandler<String> SelectPathEvent;

        PathSelectedDelegate pathSelectedCallback;

        // called whenever a path node is selected
        public PathSelectedDelegate PathSelectedCallback
        {
            get { return pathSelectedCallback; }
            set { pathSelectedCallback = value; }
        }

        ObservableCollection<PathModel> drives;

        public ObservableCollection<PathModel> Drives
        {
            get { return drives; }
            set { drives = value; }
        }

        void updateDrives()
        {
            drives = new ObservableCollection<PathModel>();

            DriveInfo[] drivesArray = new DriveInfo[0];
            try
            {
                drivesArray = DriveInfo.GetDrives();
            }
            catch (Exception e)
            {
                log.Warn("Cannot read system drives: ", e);
            }

            foreach (DriveInfo driveInfo in drivesArray)
            {
                DrivePathModel drive = new DrivePathModel(driveInfo);

                drives.Add(drive);
            }

        }

        // wait without blocking the ui thread
        void DoEvents()
        {
            App.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                  new Action(delegate { }));
        }

        public void selectPath(String path)
        {
            if (SelectPathEvent == null)
            {
                throw new Exception("setPathEvent not bound");
            }

            SelectPathEvent(this, path);
        }

        public System.Collections.IEnumerable GetChildren(object parent)
        {
            if (parent == null)
            {        
                Task task = Task.Factory.StartNew(() => updateDrives());
          
                while (!task.IsCompleted)
                {
                    DoEvents();
                }
              
                return (Drives);
            }
            else
            {
                PathModel path = (PathModel)parent;

                if (path.Directories == null)
                {                 
                    Task task = Task.Factory.StartNew(() => path.updateSubDirectories());

                    while (!task.IsCompleted)
                    {
                        DoEvents();
                    }
                }

                return (path.Directories);
            }

          
        }

        public bool HasChildren(object parent)
        {
            Task<bool> task = Task<bool>.Factory.StartNew(() =>
            {
                if (parent as DrivePathModel != null)
                {
                    return (true);
                }

                DirectoryPathModel directory = parent as DirectoryPathModel;

                DirectoryInfo dirInfo = new DirectoryInfo(directory.getFullPath());

                DirectoryInfo[] subDirsInfo = new DirectoryInfo[0];
                try
                {
                    subDirsInfo = dirInfo.GetDirectories();
                }
                catch (Exception e)
                {
                    log.Warn("Cannot access directory: " + dirInfo.FullName, e);
                }

                if (subDirsInfo.Length > 0) return (true);
                else return (false);

            });

            while (!task.IsCompleted)
            {
                DoEvents();
            }

            return (task.Result);
        }
    }
}
