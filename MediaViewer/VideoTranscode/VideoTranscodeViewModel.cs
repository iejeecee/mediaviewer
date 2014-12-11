using MediaViewer.DirectoryPicker;
using MediaViewer.Infrastructure.Video.TranscodeOptions;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Mvvm;
using MediaViewer.Progress;
using MediaViewer.Infrastructure.Settings;
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

            setDefaults();
        }

        void setDefaults()
        {
            VideoStreamMode = StreamOptions.Copy;
            VideoEncoder = VideoEncoders.H264;
            VideoEncoderPreset = VideoEncoderPresets.Medium;
            

            AudioStreamMode = StreamOptions.Copy;
            AudioEncoder = AudioEncoders.AAC;
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

        public ContainerFormats ContainerFormat
        {
            get { return containerFormat; }
            set { SetProperty(ref containerFormat, value); }
        }

        StreamOptions videoStreamMode;

        public StreamOptions VideoStreamMode
        {
            get { return videoStreamMode; }
            set { SetProperty(ref videoStreamMode, value); }
        }

        VideoEncoders videoEncoder;

        public VideoEncoders VideoEncoder
        {
            get { return videoEncoder; }
            set { SetProperty(ref videoEncoder, value); }
        }

        VideoEncoderPresets videoEncoderPreset;

        public VideoEncoderPresets VideoEncoderPreset
        {
            get { return videoEncoderPreset; }
            set { SetProperty(ref videoEncoderPreset, value); }
        }

        StreamOptions audioStreamMode;

        public StreamOptions AudioStreamMode
        {
            get { return audioStreamMode; }
            set { SetProperty(ref audioStreamMode, value); }
        }

        AudioEncoders audioEncoder;

        public AudioEncoders AudioEncoder
        {
            get { return audioEncoder; }
            set { SetProperty(ref audioEncoder, value); }
        }

        int? width;

        public int? Width
        {
            get { return width; }
            set { SetProperty(ref width, value); }
        }

        int? height;

        public int? Height
        {
            get { return height; }
            set { SetProperty(ref height, value); }
        }

        int? sampleRate;

        public int? SampleRate
        {
            get { return sampleRate; }
            set { sampleRate = value; }
        }
    }
}
