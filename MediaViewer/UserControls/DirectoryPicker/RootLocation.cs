using ICSharpCode.TreeView;
using MediaViewer.Infrastructure.Logging;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Media.Base.State;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaViewer.Model.Utils;

namespace MediaViewer.UserControls.DirectoryPicker
{
    class RootLocation : Location
    {
        public event EventHandler<Location> NodePropertyChanged;
        
        public RootLocation(InfoGatherTask infoGatherTask, MediaFileState mediaFileState) : base(infoGatherTask, mediaFileState)
        {            
            LazyLoading = true;
                      
            mediaFileState.NrImportedItemsChanged += mediaFileState_NrImportedItemsChanged;
        }

        protected void iterateChildren(Location location, Action<Location> func)
        {
            func(location);

            foreach (Location child in location.Children)
            {
                iterateChildren(child, func);
            }
        }


        protected virtual void mediaFileState_NrImportedItemsChanged(object sender, MediaStateChangedEventArgs e)
        {

            iterateChildren(this, (Action<Location>)(location => {
                            
                if (e.NewItems != null)
                {
                    foreach (MediaFileItem item in e.NewItems)
                    {
                        String path = FileUtils.getPathWithoutFileName(item.Location);

                        if (path.Equals(location.FullName))
                        {
                            location.NrImported++;
                        }
                    }
                }

                if (e.OldItems != null)
                {
                    foreach (MediaFileItem item in e.OldItems)
                    {
                        String path = FileUtils.getPathWithoutFileName(item.Location);

                        if (path.Equals(location.FullName))
                        {
                            location.NrImported--;
                        }
                      
                    }
                }

                if (e.OldLocations != null)
                {
                    foreach (String oldLocation in e.OldLocations)
                    {
                        String path = FileUtils.getPathWithoutFileName(oldLocation);

                        if (path.Equals(location.FullName))
                        {
                            location.NrImported--;
                        }
                    }
                }

            }));
        }
        
        protected override void LoadChildren()
        {
            LoadingChildrenTask = Task.Run(() =>
            {
                List<SharpTreeNode> nodes = getDriveNodes();
                App.Current.Dispatcher.Invoke(() =>
                {
                    Children.AddRange(nodes);
                });
            });

        }
        

        List<SharpTreeNode> getDriveNodes()
        {
            List<SharpTreeNode> drives = new List<SharpTreeNode>();

            try
            {
                DriveInfo[] drivesArray = DriveInfo.GetDrives();
                foreach (DriveInfo driveInfo in drivesArray)
                {
                    Location drive = new DriveLocation(driveInfo, infoGatherTask, MediaFileWatcher.Instance.MediaFileState);

                    drives.Add(drive);
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error("Cannot read system drives", e);
            }

            return (drives);
        }

       

        protected override void nodePropertyChanged(Location location)
        {
            if (NodePropertyChanged != null)
            {
                NodePropertyChanged(this, location);
            }
        }
    }
}
