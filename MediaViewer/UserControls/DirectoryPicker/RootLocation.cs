using ICSharpCode.TreeView;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.UserControls.DirectoryPicker
{
    class RootLocation : Location
    {
        public RootLocation()
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

        protected override void importStateChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            
        }

        static List<SharpTreeNode> getDriveNodes()
        {
            List<SharpTreeNode> drives = new List<SharpTreeNode>();

            try
            {
                DriveInfo[] drivesArray = DriveInfo.GetDrives();
                foreach (DriveInfo driveInfo in drivesArray)
                {
                    Location drive = new DriveLocation(driveInfo);

                    drives.Add(drive);
                }
            }
            catch (Exception e)
            {
                log.Error("Cannot read system drives", e);
            }

            return (drives);
        }
    }
}
