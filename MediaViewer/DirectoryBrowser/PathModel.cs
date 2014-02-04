using MediaViewer.Import;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.MediaFileModel.Watcher;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

        protected PathModel(String name)
        {
            this.name = name;

            MediaFileWatcher.Instance.MediaState.NrImportedItemsChanged += new NotifyCollectionChangedEventHandler(importStateChanged);
        }

        protected virtual void importStateChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            if (e.NewItems != null)
            {
                foreach (MediaFileItem item in e.NewItems)
                {
                    if (item.Location.StartsWith(getFullPath()))
                    {

                        NrImportedFiles++;
                    }
                }
            }

            if (e.OldItems != null)
            {

                foreach (MediaFileItem item in e.OldItems)
                {
                    if (item.Location.StartsWith(getFullPath()))
                    {

                        NrImportedFiles--;
                    }
                }
            }
                                    
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

        virtual public string VolumeLabel
        {
            get { return (""); }
            set { }
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

        int nrImportedFiles;

        public int NrImportedFiles
        {
            get { return nrImportedFiles; }
            set { nrImportedFiles = value;
            NotifyPropertyChanged();
            }
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

    
        public void updateSubDirectories()
        {
            if (Directories == null)
            {
                Directories = new ObservableCollection<PathModel>();
            }
                           
            List<PathModel> subDirs = updateSubDirectories(this);

            for (int i = Directories.Count - 1; i >= 0; i--)
            {
                if (!subDirs.Any(q => q.Name.Equals(Directories[i].Name)))
                {
                    Directories.RemoveAt(i);
                }

            }

            foreach (DirectoryPathModel subDir in subDirs)
            {

                if (!Directories.Any(q => q.Name.Equals(subDir.Name)))
                {
                    Directories.Add(subDir);
                }
            }

        }

        List<PathModel> updateSubDirectories(PathModel parent)
        {

            string fullPath = parent.getFullPath();

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
            
            List<PathModel> subDirs = new List<PathModel>();

            foreach (DirectoryInfo subDirInfo in subDirsInfo)
            {
                if ((subDirInfo.Attributes & FileAttributes.System) != 0)
                {
                    // skip system directories
                    continue;
                }

                DirectoryPathModel subDir = new DirectoryPathModel(parent, subDirInfo);
              
                subDirs.Add(subDir);

            }

            return (subDirs);


        }


    }
}
