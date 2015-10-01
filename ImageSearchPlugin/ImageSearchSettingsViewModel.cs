using MediaViewer.DirectoryPicker;
using MediaViewer.Model.Mvvm;
using MediaViewer.Model.Settings;
using MediaViewer.Model.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ImageSearchPlugin
{
    [Export]
    public class ImageSearchSettingsViewModel : SettingsBase
    {               
        public Command DownloadDirectoryCommand { get; set; }

        bool isAskDownloadPath;
        public bool IsAskDownloadPath {
            get
            {
                return (isAskDownloadPath);
            }
            set
            {
                SetProperty(ref isAskDownloadPath, value);
            }
        }

        bool isCurrentDownloadPath;
        public bool IsCurrentDownloadPath
        {
            get
            {
                return (isCurrentDownloadPath);
            }
            set
            {
                SetProperty(ref isCurrentDownloadPath, value);
            }
        }

        bool isFixedDownloadPath;
        public bool IsFixedDownloadPath
        {
            get
            {
                return (isFixedDownloadPath);
            }
            set
            {
                SetProperty(ref isFixedDownloadPath, value);
            }
        }

        string fixedDownloadPath;
        public String FixedDownloadPath
        {
            get
            {
                return (fixedDownloadPath);
            }
            set
            {
                SetProperty(ref fixedDownloadPath, value);
            }
        }

        public ObservableCollection<String> FixedDownloadPathHistory { get; set; }
        
        public ImageSearchSettingsViewModel() : base("Image Search Plugin", new Uri(typeof(ImageSearchSettingsView).FullName, UriKind.Relative))
        {            
            DownloadDirectoryCommand = new Command(() =>
            {                            
                DirectoryPickerView directoryPicker = new DirectoryPickerView();                
                DirectoryPickerViewModel vm = (DirectoryPickerViewModel)directoryPicker.DataContext;
                vm.SelectedPath = FixedDownloadPath;

                if (directoryPicker.ShowDialog() == true)
                {
                    FixedDownloadPath = vm.SelectedPath;
                    MiscUtils.insertIntoHistoryCollection(FixedDownloadPathHistory, FixedDownloadPath);
                }
            });
           
            IsAskDownloadPath = ImageSearchPlugin.Properties.Settings.Default.IsAskDownloadPath;
            IsCurrentDownloadPath = ImageSearchPlugin.Properties.Settings.Default.IsCurrentDownloadPath;
            IsFixedDownloadPath = ImageSearchPlugin.Properties.Settings.Default.IsFixedDownloadPath;

            if (String.IsNullOrEmpty(ImageSearchPlugin.Properties.Settings.Default.FixedDownloadPath))
            {
                FixedDownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            }
            else
            {
                FixedDownloadPath = ImageSearchPlugin.Properties.Settings.Default.FixedDownloadPath;
            }
           
            FixedDownloadPathHistory = ImageSearchPlugin.Properties.Settings.Default.FixedDownloadPathHistory;
            
        }

        protected override void OnLoad()
        {
            
        }

        protected override void OnSave()
        {            
            ImageSearchPlugin.Properties.Settings.Default.IsAskDownloadPath = IsAskDownloadPath;
            ImageSearchPlugin.Properties.Settings.Default.IsCurrentDownloadPath = IsCurrentDownloadPath;
            ImageSearchPlugin.Properties.Settings.Default.IsFixedDownloadPath = IsFixedDownloadPath;
            ImageSearchPlugin.Properties.Settings.Default.FixedDownloadPath = FixedDownloadPath;
                        
            ImageSearchPlugin.Properties.Settings.Default.Save();
      
        }


    }
}
