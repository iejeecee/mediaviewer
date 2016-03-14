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
using System.Windows.Data;

namespace MediaViewer.VideoPanel
{
    [Export]
    public class VideoSettingsViewModel : SettingsBase
    {
        public ListCollectionView VideoScreenShotSaveMode { get; set; }
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

            VideoScreenShotSaveMode = new ListCollectionView(Enum.GetValues(typeof(Infrastructure.Constants.SaveLocation)));
            VideoScreenShotSaveMode.MoveCurrentTo(Settings.Default.VideoScreenShotSaveMode);
           
            VideoScreenShotLocation = Settings.Default.VideoScreenShotLocation;
            VideoScreenShotTimeOffset = Settings.Default.VideoScreenShotTimeOffset;          
            MinNrBufferedPackets = Settings.Default.VideoMinBufferedPackets;
            StepDurationSeconds = Settings.Default.VideoStepDurationSeconds;

            NrPackets = 500;
        }

        int minNrBufferedPackets;

        public int MinNrBufferedPackets
        {
            get { return minNrBufferedPackets; }
            set { SetProperty(ref minNrBufferedPackets, value); }
        }

        int nrPackets;

        public int NrPackets
        {
            get { return nrPackets; }
            set { SetProperty(ref nrPackets, value); }
        }

        int videoScreenShotTimeOffset;

        public int VideoScreenShotTimeOffset
        {
            get { return videoScreenShotTimeOffset; }
            set { SetProperty(ref videoScreenShotTimeOffset, value); }
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

        double stepDurationSeconds;

        public double StepDurationSeconds
        {
            get { return stepDurationSeconds; }
            set
            {
                SetProperty(ref stepDurationSeconds, value);
            }
        }

        public ObservableCollection<String> VideoScreenShotLocationHistory { get; set; }

       
        protected override void OnSave() 
        {
            Settings.Default.VideoScreenShotSaveMode = (Infrastructure.Constants.SaveLocation) VideoScreenShotSaveMode.CurrentItem;          
            Settings.Default.VideoScreenShotLocation = VideoScreenShotLocation;
            Settings.Default.VideoScreenShotLocationHistory = VideoScreenShotLocationHistory;
            Settings.Default.VideoScreenShotTimeOffset = VideoScreenShotTimeOffset;
            Settings.Default.VideoMinBufferedPackets = MinNrBufferedPackets;
            Settings.Default.VideoStepDurationSeconds = StepDurationSeconds;

            base.OnSave();
        }
    }
}
