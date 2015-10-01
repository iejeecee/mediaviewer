using MediaViewer.DirectoryPicker;
using MediaViewer.Model.Settings;
using MediaViewer.Model.Global.Commands;
using MediaViewer.Model.Mvvm;
using MediaViewer.Model.Utils;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaViewer.Properties;

namespace MediaViewer.VideoPanel
{
    [Export]
    public class VideoSettingsViewModel : SettingsBase
    {  
        public Command DirectoryPickerCommand { get; set; }
                       
        public VideoSettingsViewModel() :
            base("Video", new Uri(typeof(VideoSettingsView).FullName, UriKind.Relative))
        {                       
            DirectoryPickerCommand = new Command(() =>
            {
                DirectoryPickerView directoryPicker = new DirectoryPickerView();
                DirectoryPickerViewModel vm = (DirectoryPickerViewModel)directoryPicker.DataContext;
                vm.SelectedPath = VideoScreenShotLocation;
                vm.PathHistory = VideoScreenShotLocationHistory;

                if (directoryPicker.ShowDialog() == true)
                {
                    VideoScreenShotLocation = vm.SelectedPath;
                }
            });

            VideoScreenShotLocationHistory = Settings.Default.VideoScreenShotLocationHistory;

            if (String.IsNullOrEmpty(Settings.Default.VideoScreenShotLocation))
            {
                Settings.Default.VideoScreenShotLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            }

            VideoScreenShotLocation = Settings.Default.VideoScreenShotLocation;
            VideoScreenShotTimeOffset = Settings.Default.VideoScreenShotTimeOffset;              
        }

        int videoScreenShotTimeOffset;

        public int VideoScreenShotTimeOffset
        {
            get { return videoScreenShotTimeOffset; }
            set { SetProperty(ref videoScreenShotTimeOffset, value); }
        }

        private String videoScreenShotLocation;

        public String VideoScreenShotLocation
        {
            get { return videoScreenShotLocation; }
            set {
            
                MiscUtils.insertIntoHistoryCollection(VideoScreenShotLocationHistory, value);
                SetProperty(ref videoScreenShotLocation, value); 
            }
        }

        public ObservableCollection<String> VideoScreenShotLocationHistory { get; set; }

       
        protected override void OnSave() 
        {
            Settings.Default.VideoScreenShotLocation = VideoScreenShotLocation;
            Settings.Default.VideoScreenShotLocationHistory = VideoScreenShotLocationHistory;
            Settings.Default.VideoScreenShotTimeOffset = VideoScreenShotTimeOffset;

            
        }
    }
}
