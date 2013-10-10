using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MediaViewer.DirectoryBrowser
{
    public class PathModel : ObservableObject
    {
        protected static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        DirectoryBrowserViewModel directoryBrowserViewModel;

        public DirectoryBrowserViewModel DirectoryBrowserViewModel
        {
            get { return directoryBrowserViewModel; }
        }

        private ObservableCollection<PathModel> directories;
        public ObservableCollection<PathModel> Directories
        {
            get { return directories; }
            set { directories = value; }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged();
            }
        }

        virtual public long FreeSpaceBytes
        {
            get { return (0); }
            set {}
        }


        private PathModel parent;
        public PathModel Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        private string imageUrl;

        public string ImageUrl
        {
            get { return imageUrl; }
            set
            {
                imageUrl = value;
                NotifyPropertyChanged();
            }
        }

        private bool isExpanded;

        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                isExpanded = value;

                if (parent != null && isExpanded == true)
                {
                    parent.IsExpanded = true;
                }

                if (Directories.Count == 1)
                {
                    Task updating = updateSubDirectoriesAsync(this);
                }

                NotifyPropertyChanged();
            }
        }

        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;

                NotifyPropertyChanged();

                if (isSelected == true && directoryBrowserViewModel.PathSelectedCallback != null)
                {
                    directoryBrowserViewModel.PathSelectedCallback(this);
                }

            }
        }

        public PathModel(DirectoryBrowserViewModel directoryBrowserViewModel)
        {
            directories = new ObservableCollection<PathModel>();
            this.directoryBrowserViewModel = directoryBrowserViewModel;

            parent = null;
            IsExpanded = false;
            IsSelected = false;
        }

        public string getFullPath()
        {

            string fullPath = Name;
            PathModel parent = Parent;

            while (parent != null)
            {
                fullPath = parent.Name + "\\" + fullPath;
                fullPath = fullPath.Replace("\\\\", "\\");
                parent = parent.Parent;
            }

            return (fullPath);
        }

        static DirectoryInfo[] getSubDirectories(string fullPath)
        {

            DirectoryInfo dirInfo = new DirectoryInfo(fullPath);

            DirectoryInfo[] subDirsInfo = new DirectoryInfo[0];
            try
            {
                subDirsInfo = dirInfo.GetDirectories();
            }
            catch (Exception e)
            {
                log.Warn("Cannot access directory: " + dirInfo.FullName, e);
            }

            return (subDirsInfo);
        }

        public static async Task updateSubDirectoriesAsync(PathModel parent)
        {

            // await for async operation to complete

            List<PathModel> subDirs =
                await Task<List<PathModel>>.Run(() => updateSubDirectories(parent));

            // everything after await gets executed on the ui thread
            // see: http://blogs.msdn.com/b/pfxteam/archive/2011/01/13/10115163.aspx

            for (int i = parent.Directories.Count - 1; i >= 0; i--)
            {
                if (!subDirs.Any(q => q.Name.Equals(parent.Directories[i].Name)))
                {

                    parent.Directories.RemoveAt(i);
                }

            }

            foreach (DirectoryPathModel subDir in subDirs)
            {

                if (!parent.Directories.Any(q => q.Name.Equals(subDir.Name)))
                {
                    parent.Directories.Add(subDir);
                }
            }


        }

        static List<PathModel> updateSubDirectories(PathModel parent)
        {

            string fullPath = parent.getFullPath();

            DirectoryInfo[] subDirsInfo = getSubDirectories(fullPath);
            List<PathModel> subDirs = new List<PathModel>();

            foreach (DirectoryInfo subDirInfo in subDirsInfo)
            {
                if ((subDirInfo.Attributes & FileAttributes.System) != 0)
                {
                    // skip system directories
                    continue;
                }

                DirectoryPathModel subDir = new DirectoryPathModel(parent, subDirInfo, parent.DirectoryBrowserViewModel);
              
                subDirs.Add(subDir);

            }

            return (subDirs);


        }


    }
}
