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
            isVideoScreenShotLocationFixed = Settings.Default.IsVideoScreenShotLocationFixed;
            IsVideoScreenShotLocationAsk = Settings.Default.IsVideoScreenShotLocationAsk;
            IsVideoScreenShotLocationCurrent = Settings.Default.IsVideoScreenShotLocationCurrent;
        }

        int videoScreenShotTimeOffset;

        public int VideoScreenShotTimeOffset
        {
            get { return videoScreenShotTimeOffset; }
            set { SetProperty(ref videoScreenShotTimeOffset, value); }
        }

        bool isVideoScreenShotLocationAsk;
        public bool IsVideoScreenShotLocationAsk
        {
            get
            {
                return (isVideoScreenShotLocationAsk);
            }
            set
            {
                SetProperty(ref isVideoScreenShotLocationAsk, value);
            }
        }

        bool isVideoScreenShotLocationCurrent;
        public bool IsVideoScreenShotLocationCurrent
        {
            get
            {
                return (isVideoScreenShotLocationCurrent);
            }
            set
            {
                SetProperty(ref isVideoScreenShotLocationCurrent, value);
            }
        }

        bool isVideoScreenShotLocationFixed;
        public bool IsVideoScreenShotLocationFixed
        {
            get
            {
                return (isVideoScreenShotLocationFixed);
            }
            set
            {
                SetProperty(ref isVideoScreenShotLocationFixed, value);
            }
        }

        string videoScreenShotLocation;
        public String VideoScreenShotLocation
        {
            get
            {
                return (videoScreenShotLocation);
            }
            set
            {
                SetProperty(ref videoScreenShotLocation, value);
            }
        }

        public ObservableCollection<String> VideoScreenShotLocationHistory { get; set; }

       
        protected override void OnSave() 
        {
            Settings.Default.IsVideoScreenShotLocationAsk = IsVideoScreenShotLocationAsk;
            Settings.Default.IsVideoScreenShotLocationCurrent = IsVideoScreenShotLocationCurrent;
            Settings.Default.IsVideoScreenShotLocationFixed = isVideoScreenShotLocationFixed;
            Settings.Default.VideoScreenShotLocation = VideoScreenShotLocation;
            Settings.Default.VideoScreenShotLocationHistory = VideoScreenShotLocationHistory;
            Settings.Default.VideoScreenShotTimeOffset = VideoScreenShotTimeOffset;

            
        }
    }
}
