using MediaViewer.Utils.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.DirectoryBrowser
{
    class DrivePathModel : PathModel
    {

        static DriveIdleMonitor driveIdleMonitor = new DriveIdleMonitor();

        DriveInfo info;

        public DriveInfo DriveInfo
        {
            get { return info; }           
        }

        public DrivePathModel(DriveInfo info)          
        {
            this.info = info;
            Parent = null;
            Name = info.Name;
                    
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
        
            if (driveIdleMonitor.DrivesMonitored.Contains(Name.Replace("\\","")))
            {
                FreeSpaceBytes = info.TotalFreeSpace;
                driveIdleMonitor.DriveInUse += new EventHandler<string>(driveIdleMonitor_driveInUse);
            }
            else
            {
                FreeSpaceBytes = 0;
            }
            
        }

        long freeSpaceBytes;

        override public long FreeSpaceBytes 
        {
            get { return freeSpaceBytes; }
            set
            {
                freeSpaceBytes = value;          
                NotifyPropertyChanged();
            }
        }

        string volumeLabel;

        override public string VolumeLabel
        {
            get { return volumeLabel; }
            set { volumeLabel = value;
                NotifyPropertyChanged();
            }
        }

        private void driveIdleMonitor_driveInUse(object sender, string e)
        {
            FreeSpaceBytes = info.TotalFreeSpace;
        }
               
    }
}
