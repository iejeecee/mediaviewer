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

        public DrivePathModel(DriveInfo info, DirectoryBrowserViewModel directoryBrowserViewModel)
            : base(directoryBrowserViewModel)
        {
            this.info = info;
            Parent = null;
            Name = info.Name;
            switch (info.DriveType)
            {
                case DriveType.CDRom:
                    {
                        ImageUrl = "pack://application:,,,/Resources/Icons/CD_Drive.ico";
                        break;
                    }
                case DriveType.Fixed:
                    {
                        ImageUrl = "pack://application:,,,/Resources/Icons/Hard_Drive.ico";
                        break;
                    }
                case DriveType.Network:
                    {
                        ImageUrl = "pack://application:,,,/Resources/Icons/Network_Drive.ico";
                        break;
                    }
                case DriveType.NoRootDirectory:
                    {
                        break;
                    }
                case DriveType.Ram:
                    {
                        ImageUrl = "pack://application:,,,/Resources/Icons/ram.ico";
                        break;
                    }
                case DriveType.Removable:
                    {
                        ImageUrl = "pack://application:,,,/Resources/Icons/Removable_Drive.ico";
                        break;
                    }
                case DriveType.Unknown:
                    {
                        ImageUrl = "pack://application:,,,/Resources/Icons/UnknownDrive.ico";
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

        private void driveIdleMonitor_driveInUse(object sender, string e)
        {
            FreeSpaceBytes = info.TotalFreeSpace;
        }
               
    }
}
