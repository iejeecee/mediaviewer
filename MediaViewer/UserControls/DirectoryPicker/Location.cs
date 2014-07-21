using ICSharpCode.TreeView;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.MediaFileModel;
using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.Utils.Windows;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MediaViewer.UserControls.DirectoryPicker
{
   
    public class Location : SharpTreeNode
    {
        protected static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected InfoGatherTask infoGatherTask;

        protected Location(InfoGatherTask infoGatherTask)
        {
            this.infoGatherTask = infoGatherTask;

            MediaFileWatcher.Instance.MediaState.NrImportedItemsChanged += new EventHandler<MediaStateChangedEventArgs>(importStateChanged);
      
        }

        protected virtual void importStateChanged(object sender, MediaStateChangedEventArgs e)
        {

            if (e.NewItems != null)
            {
                foreach (MediaFileItem item in e.NewItems)
                {
                    if (item.Location.StartsWith(fullName))
                    {
                        NrImported++;
                    }
                }
            }

            if (e.OldItems != null)
            {

                foreach (MediaFileItem item in e.OldItems)
                {
                    if (item.Location.StartsWith(fullName))
                    {

                        NrImported--;
                    }
                }
            }

        }

        Task loadingChildrenTask;

        public Task LoadingChildrenTask
        {
            get { return loadingChildrenTask; }
            protected set { loadingChildrenTask = value; }
        }
       
        protected override void LoadChildren()
        {       
            LoadingChildrenTask = Task.Run(() =>
            {
                List<SharpTreeNode> nodes = getDirectoryNodes(FullName);
                App.Current.Dispatcher.Invoke(() =>
                {
                    Children.AddRange(nodes);                   
                });
            });
                
        }
        
        List<SharpTreeNode> getDirectoryNodes(String fullName)
        {
            List<SharpTreeNode> directories = new List<SharpTreeNode>();

            try
            {
                IEnumerable<DirectoryInfo> dirInfos = new DirectoryInfo(fullName).EnumerateDirectories();

                foreach (DirectoryInfo dirInfo in dirInfos)
                {
                    if (dirInfo.Attributes.HasFlag(FileAttributes.System))
                    {
                        continue;
                    }

                    Location directory = new DirectoryLocation(dirInfo, infoGatherTask);

                    directories.Add(directory);
                }
            }
            catch (Exception e)
            {
                log.Error("Cannot read directories", e);
            }

            return (directories);
        }
    
        String volumeLabel;

        public String VolumeLabel
        {
            get { return volumeLabel; }
            set { volumeLabel = value;
            RaisePropertyChanged("VolumeLabel");
            }
        }

        String imageUrl;

        public String ImageUrl
        {
            get { return imageUrl; }
            set { imageUrl = value;
            RaisePropertyChanged("ImageUrl");
            RaisePropertyChanged("Icon");
            }
        }

        String fullName;

        public String FullName
        {
          get { return fullName; }
          set { fullName = value;
          RaisePropertyChanged("FullName");
          }
        }

        String name;

        public String Name
        {
            get { return name; }
            set { name = value;
         
            RaisePropertyChanged("Name");
            RaisePropertyChanged("Text");
            }
        }

        int nrImported;

        public virtual int NrImported
        {
            get { return nrImported; }
            set
            {
                nrImported = value;           
                RaisePropertyChanged("NrImported");
            }
        }

        Nullable<DateTime> creationDate;

        public Nullable<DateTime> CreationDate
        {
            get { return creationDate; }
            set { creationDate = value;
            RaisePropertyChanged("CreationDate");
            }
        }

        long freeSpaceBytes;

        public long FreeSpaceBytes
        {
            get { return freeSpaceBytes; }
            set { freeSpaceBytes = value;
            RaisePropertyChanged("FreeSpaceBytes");
            }
        }

        public override object ToolTip
        {
            get
            {
                return Text;
            }
        }

        public override object Text
        {           
            get
            {
                if (String.IsNullOrEmpty(VolumeLabel))
                {
                    return (Name);
                }
                else
                {
                    return (VolumeLabel + " (" + Name.ToUpper() + ")");
                }
            }
        }

        public override object Icon
        {
            get
            {
                return loadIcon();
            }
        }

        protected ImageSource loadIcon()
        {
            var frame = BitmapFrame.Create(new Uri(ImageUrl, UriKind.Absolute));
            return frame;
        }

        protected virtual void nodePropertyChanged(Location location)
        {
            (Parent as Location).nodePropertyChanged(location);                        
        }
    }
}
