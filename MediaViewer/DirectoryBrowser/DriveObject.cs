using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.DirectoryBrowser
{
    class DriveObject : PathObject
    {          
        public DriveObject(DriveInfo info)
        {
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
           
        }
    }
}
