using MediaViewer.Model.Mvvm;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MediaViewer.UserControls.LocationBox
{
    class LocationBoxViewModel : BindableBase
    {
        public event EventHandler<DirectoryItem> LocationSelected;
        public Command<DirectoryItem> LocationSelectedCommand { get; set; }

        public LocationBoxViewModel() {

            SplitPath = new ObservableCollection<DirectoryItem>();

            LocationSelectedCommand = new Command<DirectoryItem>((item) =>
            {                
                if (LocationSelected != null)
                {
                    LocationSelected(this, item);
                }
            });
        }

        ObservableCollection<DirectoryItem> splitPath;

        public ObservableCollection<DirectoryItem> SplitPath
        {
            get { return splitPath; }
            set { SetProperty(ref splitPath, value); }
        }

        public void setPath(String location)
        {        
            splitPath.Clear();
            
            splitPath.Add(createDrivesItem());

            string[] directories = location.Split(Path.DirectorySeparatorChar);
            String fullPath = "";
            

            for(int i = 0; i < directories.Count(); i++)
            {                                                                
                fullPath += directories[i];

                if (String.IsNullOrEmpty(directories[i])) continue;

                DirectoryItem item;

                if(i == 0) {

                    String volumeLabel;
                    String imageUri;

                    getDriveVolumeAndImage(new DriveInfo(directories[i]), out volumeLabel, out imageUri);

                    item = new DirectoryItem(volumeLabel + " (" + directories[i] + ")", fullPath, LocationSelectedCommand);

                } else {
                    
                    item = new DirectoryItem(directories[i], fullPath, LocationSelectedCommand);
                }
                
                splitPath.Add(item);

                fullPath += Path.DirectorySeparatorChar;

                String[] subDirectories = Directory.GetDirectories(fullPath);

                foreach (String subDirectory in subDirectories)
                {
                    DirectoryInfo info = new DirectoryInfo(subDirectory);
                    if (info.Attributes.HasFlag(FileAttributes.System)) continue;

                    DirectoryItem subDir = new DirectoryItem(Path.GetFileName(subDirectory), subDirectory, LocationSelectedCommand);

                    subDir.Icon = new System.Windows.Controls.Image
                    {
                        Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Folder_Open.ico"))
                    };

                    subDir.Icon.Width = 20;
                    subDir.Icon.Height = 20;

                    item.SubDirectories.Add(subDir);
                }
            }
        }

        void getDriveVolumeAndImage(DriveInfo info, out String volumeLabel, out String imageUrl)
        {
            switch (info.DriveType)
            {
                case DriveType.CDRom:
                    {
                        imageUrl = "pack://application:,,,/Resources/Icons/CD_Drive.ico";
                        volumeLabel = "CD ROM";
                        break;
                    }
                case DriveType.Fixed:
                    {
                        imageUrl = "pack://application:,,,/Resources/Icons/Hard_Drive.ico";

                        try
                        {
                            volumeLabel = !String.IsNullOrEmpty(info.VolumeLabel) ? info.VolumeLabel : "Local Disk";
                        }
                        catch (Exception)
                        {
                            volumeLabel = "";
                        }

                        break;
                    }
                case DriveType.Network:
                    {
                        imageUrl = "pack://application:,,,/Resources/Icons/Network_Drive.ico";
                        volumeLabel = "Network Drive";
                        break;
                    }             
                case DriveType.Ram:
                    {
                        imageUrl = "pack://application:,,,/Resources/Icons/ram.ico";
                        volumeLabel = "Ram Drive";
                        break;
                    }
                case DriveType.Removable:
                    {
                        imageUrl = "pack://application:,,,/Resources/Icons/Removable_Drive.ico";
                        volumeLabel = "Removable Drive";
                        break;
                    }
                case DriveType.Unknown:
                    {
                        imageUrl = "pack://application:,,,/Resources/Icons/UnknownDrive.ico";
                        volumeLabel = "Unknown Drive";
                        break;
                    }
                default:
                    {
                        imageUrl = "";
                        volumeLabel = "";
                        break;
                    }
            }

        }

        DirectoryItem createDrivesItem()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            DirectoryItem drivesItem = new DirectoryItem("", "", LocationSelectedCommand);
            drivesItem.Icon = new System.Windows.Controls.Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/computer.ico"))
            };

            foreach (DriveInfo info in drives)
            {
                String imageUrl = "";
                String volumeLabel = "";

                getDriveVolumeAndImage(info, out volumeLabel, out imageUrl);
                
                DirectoryItem driveItem = new DirectoryItem(volumeLabel + " (" + info.Name.Trim('\\') + ")", info.Name, LocationSelectedCommand);
                driveItem.Icon = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(imageUrl))
                };

                driveItem.Icon.Width = 20;
                driveItem.Icon.Height = 20;

                drivesItem.SubDirectories.Add(driveItem);  
            }

            return (drivesItem);
        }
    }

}
