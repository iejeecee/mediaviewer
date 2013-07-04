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
    public class PathModel 
    {
        protected static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public event EventHandler<PathModel> Selected;

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
            set { name = value; }
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
            set { imageUrl = value; }
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

                if (Directories.Count == 1 &&
                    Directories[0].GetType() == typeof(DummyPathModel))
                {
                    Task updating = updateSubDirectoriesAsync(this);
                }

                
            }
        }

      

        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;

                if (isSelected == true)
                {
                    bubbleSelected(this);
                }
            }
        }

        void bubbleSelected(PathModel selected)
        {
            if (parent == null)
            {
                Selected(this, selected);
            }
            else
            {
                parent.bubbleSelected(selected);
            }
        }

        public PathModel()
        {
            directories = new ObservableCollection<PathModel>();

            parent = null;
            IsExpanded = false;
            IsSelected = false;
        }        

        public string getFullPath() {

            string fullPath = Name;
            PathModel parent = Parent;

            while (parent != null)
            {
                fullPath = parent.Name + "\\" + fullPath;
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

        public static async Task updateSubDirectoriesAsync(PathModel parent) {

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

                DirectoryPathModel subDir = new DirectoryPathModel(parent, subDirInfo);

                string subDirPath = fullPath + "\\" + subDir.Name;

                subDirsInfo = getSubDirectories(subDirPath);
                if (subDirsInfo.Length > 0)
                {
                    subDir.Directories.Add(new DummyPathModel(subDir));
                }

                subDirs.Add(subDir);

            }

            return (subDirs);
          
         
        }

    
    }
}
