using MediaViewer.DirectoryPicker;
using MediaViewer.Infrastructure.Settings;
using MediaViewer.Model.Mvvm;
using MediaViewer.Model.Utils;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.VideoPanel
{
    public class VideoSettingsViewModel : CloseableBindableBase 
    {
        public Command OkCommand { get; set; }
        public Command DirectoryPickerCommand { get; set; }
        AppSettings Settings { get; set; }

        public VideoSettingsViewModel(AppSettings settings)
        {
            Settings = settings;

            OkCommand = new Command(() => OnClosingRequest());
            DirectoryPickerCommand = new Command(() =>
            {
                DirectoryPickerView directoryPicker = new DirectoryPickerView();
                DirectoryPickerViewModel vm = (DirectoryPickerViewModel)directoryPicker.DataContext;
                vm.MovePath = VideoScreenShotLocation;
                vm.MovePathHistory = VideoScreenShotLocationHistory;

                if (directoryPicker.ShowDialog() == true)
                {
                    VideoScreenShotLocation = vm.MovePath;
                }
            });

            VideoScreenShotLocation = settings.VideoScreenShotLocation;          
            VideoScreenShotLocationHistory = settings.VideoScreenShotLocationHistory;
        }

        private String videoScreenShotLocation;

        public String VideoScreenShotLocation
        {
            get { return videoScreenShotLocation; }
            set {

                Settings.VideoScreenShotLocation = value;
                MiscUtils.insertIntoHistoryCollection(Settings.VideoScreenShotLocationHistory, value);
                SetProperty(ref videoScreenShotLocation, value); 
            }
        }

        public ObservableCollection<String> VideoScreenShotLocationHistory { get; set; }

    }
}
