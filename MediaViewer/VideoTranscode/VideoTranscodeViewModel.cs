using MediaViewer.DirectoryPicker;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Mvvm;
using MediaViewer.Progress;
using MediaViewer.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.VideoTranscode
{
    public class VideoTranscodeViewModel : CloseableBindableBase
    {
        public Command DefaultsCommand { get; set; }
        public Command CancelCommand { get; set; }
        public Command OkCommand { get; set; }
        public Command DirectoryPickerCommand { get; set; }

        public ICollection<MediaFileItem> Items { get; set; }

        public VideoTranscodeViewModel(AppSettings settings)
        {
            OutputPathHistory = settings.TranscodeOutputDirectoryHistory;

            OkCommand = new Command(async () =>
                {
                    CancellableOperationProgressView progress = new CancellableOperationProgressView();
                    VideoTranscodeProgressViewModel vm = new VideoTranscodeProgressViewModel(Items, this);
                    progress.DataContext = vm;

                    Task task = vm.startTranscodeAsync();
                    progress.Show();
                    OnClosingRequest();
                    await task;
              
                });


            DefaultsCommand = new Command(() =>
                {
                    setDefaults();
                });

            CancelCommand = new Command(() =>
            {
                OnClosingRequest();
            });

            DirectoryPickerCommand = new Command(() =>
                {
                    DirectoryPickerView directoryPicker = new DirectoryPickerView();
                    DirectoryPickerViewModel vm = (DirectoryPickerViewModel)directoryPicker.DataContext;
                    vm.MovePath = OutputPath;
                    vm.MovePathHistory = OutputPathHistory;

                    if (directoryPicker.ShowDialog() == true)
                    {
                        OutputPath = vm.MovePath;
                    }

                });

        }

        void setDefaults()
        {

        }

        ObservableCollection<String> outputPathHistory;

        public ObservableCollection<String> OutputPathHistory
        {
            get { return outputPathHistory; }
            set { SetProperty(ref outputPathHistory, value); }
        }

        String outputPath;

        public String OutputPath
        {
            get { return outputPath; }
            set {
                SetProperty(ref outputPath, value);
            }
        }

        ContainerFormats containerFormat;

        internal ContainerFormats ContainerFormat
        {
            get { return containerFormat; }
            set { SetProperty(ref containerFormat, value); }
        }

    }
}
