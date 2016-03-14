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
        public ListCollectionView ImageSaveMode { get; set; }
        public Command DirectoryPickerCommand { get; set; }
        
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
            DirectoryPickerCommand = new Command(() =>
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

            ImageSaveMode = new ListCollectionView(Enum.GetValues(typeof(MediaViewer.Infrastructure.Constants.SaveLocation)));
            ImageSaveMode.MoveCurrentTo(ImageSearchPlugin.Properties.Settings.Default.ImageSaveMode);
            
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

        protected override void OnSave()
        {
            ImageSearchPlugin.Properties.Settings.Default.ImageSaveMode = (MediaViewer.Infrastructure.Constants.SaveLocation)ImageSaveMode.CurrentItem;
            ImageSearchPlugin.Properties.Settings.Default.FixedDownloadPath = FixedDownloadPath;
                        
            ImageSearchPlugin.Properties.Settings.Default.Save();
      
        }


    }
}
