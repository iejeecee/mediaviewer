using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MediaViewer.DirectoryBrowser
{
    public class DirectoryBrowserViewModel : ObservableObject
    {
        protected static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public delegate void PathSelectedDelegate(PathModel item);
       
        PathSelectedDelegate pathSelectedCallback;

        // callback function will be called whenever a path node is selected
        public PathSelectedDelegate PathSelectedCallback
        {
            get { return pathSelectedCallback; }
            set { pathSelectedCallback = value; }
        }

        public DirectoryBrowserViewModel()
        {
            drives = null;
        }

        private ObservableCollection<PathModel> drives;

        public ObservableCollection<PathModel> Drives
        {
            get
            {
                if (drives == null)
                {
                    drives = updateDrives();
                }

                return (drives);
            }
            set { drives = value; }
        }

        private static DriveInfo[] getDrives()
        {
            DriveInfo[] drivesInfo = new DriveInfo[0];
            try
            {
                drivesInfo = DriveInfo.GetDrives();
            }
            catch (Exception e)
            {
                log.Warn("Cannot read system drives: ", e);
            }

            return (drivesInfo);
        }

        private ObservableCollection<PathModel> updateDrives()
        {

            ObservableCollection<PathModel> drives = new ObservableCollection<PathModel>();

            DriveInfo[] drivesArray = DriveInfo.GetDrives();

            foreach (DriveInfo driveInfo in drivesArray)
            {
                DrivePathModel drive = new DrivePathModel(driveInfo, this);               
             
                drives.Add(drive);
            }

            return (drives);
        }

        public async void selectPath(string path)
        {
           
            path = path.Replace('/', '\\');

            string root = System.IO.Path.GetPathRoot(path).ToUpper();
            PathModel node = null;

            foreach (PathModel drive in Drives)
            {

                if (drive.Name.Equals(root))
                {
                    node = drive;
                    break;
                }

            }

            if (node == null)
            {
                return;
            }
          
            string seperator = "\\";

            string[] splitDirs = path.Split(seperator.ToCharArray());

            for (int i = 1; i < splitDirs.Length; i++)
            {
                await PathModel.updateSubDirectoriesAsync(node);
                   
                foreach (PathModel directory in node.Directories)
                {
                    if (directory.Name.Equals(splitDirs[i]))
                    {
                        node = directory;                  
                        break;
                    }
                }
            }

            if (node.Parent != null)
            {
                node.Parent.IsExpanded = true;
            }
            node.IsSelected = true;
        }
     
    }

}
