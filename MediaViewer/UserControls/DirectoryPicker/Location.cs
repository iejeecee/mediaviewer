using ICSharpCode.TreeView;
using MediaViewer.Infrastructure.Logging;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Media.Base.State;
using Microsoft.Practices.Prism.Mvvm;
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
      
        protected InfoGatherTask infoGatherTask;
        protected MediaFileState MediaFileState { get; set; }

        protected Location(InfoGatherTask infoGatherTask, MediaFileState state)
        {
            MediaFileState = state;
            this.infoGatherTask = infoGatherTask;

            //state.NrImportedItemsChanged += new EventHandler<MediaStateChangedEventArgs>(importStateChanged);
                     
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
                List<SharpTreeNode> directories = createDirectoryNodes(FullName);

                App.Current.Dispatcher.Invoke(() =>
                {
                    Children.AddRange(directories);
                });

                foreach (SharpTreeNode directory in directories)
                {
                    ((Location)directory).infoGatherTask.addLocation((Location)directory);
                }
            });
                
        }
        
        List<SharpTreeNode> createDirectoryNodes(String location)
        {
            List<SharpTreeNode> directories = new List<SharpTreeNode>();

            try
            {                
                IEnumerable<DirectoryInfo> dirInfos = new DirectoryInfo(location).EnumerateDirectories();

                foreach (DirectoryInfo dirInfo in dirInfos)
                {
                    if (dirInfo.Attributes.HasFlag(FileAttributes.System))
                    {
                        continue;
                    }
                    
                    Location directory = new DirectoryLocation(dirInfo, infoGatherTask, MediaFileState);

                    directories.Add(directory);
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error("Cannot read directories", e);
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
      
        public String FullName
        {
          get {

              String fullName = this is DriveLocation ? Name + "\\" : Name;
              Location parent = Parent as Location;
       
              while (parent != null && !parent.IsRoot)
              {
                  fullName = parent.Name + "\\" + fullName;

                  parent = parent.Parent as Location;           
              }

              return (fullName);
          
          }
          
        }

        String name;

        public String Name
        {
            get { return name; }
            set { name = value;
         
            RaisePropertyChanged("Name");
            //RaisePropertyChanged("Text");
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
