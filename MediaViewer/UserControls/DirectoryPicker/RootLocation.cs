using ICSharpCode.TreeView;
using MediaViewer.MediaFileModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.UserControls.DirectoryPicker
{
    class RootLocation : Location
    {
        public event EventHandler<Location> NodePropertyChanged;
        
        public RootLocation(InfoGatherTask infoGatherTask) : base(infoGatherTask)
        {            
            LazyLoading = true;
                      
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

        protected override void importStateChanged(object sender, MediaStateChangedEventArgs e)
        {
            
        }

        List<SharpTreeNode> getDriveNodes()
        {
            List<SharpTreeNode> drives = new List<SharpTreeNode>();

            try
            {
                DriveInfo[] drivesArray = DriveInfo.GetDrives();
                foreach (DriveInfo driveInfo in drivesArray)
                {
                    Location drive = new DriveLocation(driveInfo, infoGatherTask);

                    drives.Add(drive);
                }
            }
            catch (Exception e)
            {
                log.Error("Cannot read system drives", e);
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
