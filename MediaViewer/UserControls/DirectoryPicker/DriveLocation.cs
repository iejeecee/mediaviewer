using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.Base.State;
using MediaViewer.Model.Utils.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.UserControls.DirectoryPicker
{
    class DriveLocation : Location
    {
        static DriveIdleMonitor driveIdleMonitor = new DriveIdleMonitor();
     
        public DriveLocation(DriveInfo info, InfoGatherTask infoGatherTask, MediaFileState mediaFileState)
            : base(infoGatherTask, mediaFileState)
        {
           
            switch (info.DriveType)
            {
                case DriveType.CDRom:
                    {
                        ImageUrl = "pack://application:,,,/Resources/Icons/CD_Drive.ico";
                        VolumeLabel = "CD ROM";
                        break;
                    }
                case DriveType.Fixed:
                    {
                        ImageUrl = "pack://application:,,,/Resources/Icons/Hard_Drive.ico";

                        try
                        {
                            VolumeLabel = !String.IsNullOrEmpty(info.VolumeLabel) ? info.VolumeLabel : "Local Disk";
                        }
                        catch (Exception)
                        {

                        }

                        break;
                    }
                case DriveType.Network:
                    {
                        ImageUrl = "pack://application:,,,/Resources/Icons/Network_Drive.ico";
                        VolumeLabel = "Network Drive";
                        break;
                    }
                case DriveType.NoRootDirectory:
                    {
                        break;
                    }
                case DriveType.Ram:
                    {
                        ImageUrl = "pack://application:,,,/Resources/Icons/ram.ico";
                        VolumeLabel = "Ram Drive";
                        break;
                    }
                case DriveType.Removable:
                    {
                        ImageUrl = "pack://application:,,,/Resources/Icons/Removable_Drive.ico";
                        VolumeLabel = "Removable Drive";
                        break;
                    }
                case DriveType.Unknown:
                    {
                        ImageUrl = "pack://application:,,,/Resources/Icons/UnknownDrive.ico";
                        VolumeLabel = "Unknown Drive";
                        break;
                    }
            }

            Name = info.Name.TrimEnd(new char[] { '\\' });
          
            if (driveIdleMonitor.DrivesMonitored.Contains(Name))
            {
                FreeSpaceBytes = info.TotalFreeSpace;
                driveIdleMonitor.DriveInUse += new EventHandler<string>(driveIdleMonitor_driveInUse);
            }
            else
            {
                FreeSpaceBytes = 0;
            }

            infoGatherTask.addLocation(this);
          
            LazyLoading = true;


        }
        private void driveIdleMonitor_driveInUse(object sender, string e)
        {
            FreeSpaceBytes = new DriveInfo(Name).TotalFreeSpace;
        }

        public override bool ShowExpander
        {
            get
            {

                return (true);
            }
        }
    }
}
