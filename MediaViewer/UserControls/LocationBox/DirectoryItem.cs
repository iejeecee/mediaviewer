using MediaViewer.Model.Mvvm;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MediaViewer.UserControls.LocationBox
{
    class DirectoryItem : BindableBase
    {
        public DirectoryItem(String directoryName, String fullPath, Command<DirectoryItem> isSelectedCommand)
        {
            DirectoryName = directoryName;
            FullPath = fullPath;
            SubDirectories = new ObservableCollection<DirectoryItem>();            
            IsSelectedCommand = isSelectedCommand;            
        }
        
        String directoryName;

        public String DirectoryName
        {
            get { return directoryName; }
            set { SetProperty(ref directoryName, value); }
        }
        String fullPath;

        public String FullPath
        {
            get { return fullPath; }
            set { SetProperty(ref fullPath, value); }
        }

        ObservableCollection<DirectoryItem> subDirectories;

        public ObservableCollection<DirectoryItem> SubDirectories
        {
            get { return subDirectories; }
            set { SetProperty(ref subDirectories, value); }
        }

        Image icon;

        public Image Icon
        {
            get { return icon; }
            set { SetProperty(ref icon, value); }
        }

        public Command<DirectoryItem> IsSelectedCommand { get; set; }
        
    }
}
