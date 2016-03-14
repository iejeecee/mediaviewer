using MediaViewer.DirectoryPicker;
using MediaViewer.Model.Mvvm;
using MediaViewer.Model.Settings;
using MediaViewer.Model.Utils;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace YoutubePlugin.Settings
{
    [Export]
    public class YoutubeSettingsViewModel : SettingsBase
    {
        public ListCollectionView MaxPlaybackResolution { get; set; }
        public ListCollectionView MaxDownloadResolution { get; set; }

        public ListCollectionView VideoSaveMode { get; set; }
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

        public YoutubeSettingsViewModel()
            : base("Youtube Plugin", new Uri(typeof(YoutubeSettingsView).FullName, UriKind.Relative))
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

            VideoSaveMode = new ListCollectionView(Enum.GetValues(typeof(MediaViewer.Infrastructure.Constants.SaveLocation)));
            VideoSaveMode.MoveCurrentTo(YoutubePlugin.Properties.Settings.Default.VideoSaveMode);

            if (String.IsNullOrEmpty(YoutubePlugin.Properties.Settings.Default.FixedDownloadPath))
            {
                FixedDownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            }
            else
            {
                FixedDownloadPath = YoutubePlugin.Properties.Settings.Default.FixedDownloadPath;
            }

            FixedDownloadPathHistory = YoutubePlugin.Properties.Settings.Default.FixedDownloadPathHistory;

            MaxPlaybackResolution = new ListCollectionView(Enum.GetValues(typeof(YoutubePlugin.Settings.Constants.VideoResolution)));
            MaxPlaybackResolution.MoveCurrentTo(Properties.Settings.Default.MaxPlaybackResolution);

            MaxDownloadResolution = new ListCollectionView(Enum.GetValues(typeof(YoutubePlugin.Settings.Constants.VideoResolution)));
            MaxDownloadResolution.MoveCurrentTo(Properties.Settings.Default.MaxDownloadResolution);
        }      

        protected override void OnSave()
        {
            YoutubePlugin.Properties.Settings.Default.VideoSaveMode = (MediaViewer.Infrastructure.Constants.SaveLocation)VideoSaveMode.CurrentItem;
            YoutubePlugin.Properties.Settings.Default.FixedDownloadPath = FixedDownloadPath;

            YoutubePlugin.Properties.Settings.Default.MaxPlaybackResolution = (YoutubePlugin.Settings.Constants.VideoResolution)MaxPlaybackResolution.CurrentItem;
            YoutubePlugin.Properties.Settings.Default.MaxDownloadResolution = (YoutubePlugin.Settings.Constants.VideoResolution)MaxDownloadResolution.CurrentItem;

            YoutubePlugin.Properties.Settings.Default.Save();
      
        }
    }
}
