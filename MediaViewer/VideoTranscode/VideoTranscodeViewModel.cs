using MediaViewer.DirectoryPicker;
using MediaViewer.Infrastructure.Video.TranscodeOptions;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Mvvm;
using MediaViewer.Progress;
using MediaViewer.Model.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MediaViewer.Model.Utils;

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
                    vm.WindowIcon = IconUri;

                    Task task = vm.startTranscodeAsync();
                    progress.Show();
                    OnClosingRequest();
                    await task;

                    MiscUtils.insertIntoHistoryCollection(settings.TranscodeOutputDirectoryHistory, OutputPath);
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

            SupportedAudioEncoders = new ListCollectionView(Enum.GetValues(typeof(AudioEncoders)));
            SupportedAudioEncoders.Filter = supportedAudioEncodersFilter;
            SupportedVideoEncoders = new ListCollectionView(Enum.GetValues(typeof(VideoEncoders)));
            SupportedVideoEncoders.Filter = supportedVideoEncodersFilter;

            Items = new List<MediaFileItem>();
            Title = "Transcode Video(s)";
            IconUri = "/MediaViewer;component/Resources/Icons/videotranscode.ico";

            setDefaults();

            IsTimeRangeEnabled = false;
            StartTimeRange = 0;
            EndTimeRange = 0;
        }

        bool supportedVideoEncodersFilter(object videoEncoder)
        {
            VideoEncoders encoder = (VideoEncoders)videoEncoder;

            switch (ContainerFormat)
            {
                case ContainerFormats.MP4:
                    if (encoder == VideoEncoders.libx264 || encoder == VideoEncoders.libx265) return true;
                    else return (false);            
                case ContainerFormats.MOV:
                    if (encoder == VideoEncoders.libx264) return true;
                    else return (false);               
                case ContainerFormats.AVI:
                    if (encoder == VideoEncoders.libx264) return true;
                    else return (false);            
                case ContainerFormats.MKV:
                    if (encoder == VideoEncoders.libx264 || encoder == VideoEncoders.libvpx) return true;
                    else return (false);
                case ContainerFormats.FLV:
                    if (encoder == VideoEncoders.libx264) return true;
                    else return (false);
                case ContainerFormats.WMV:
                    if (encoder == VideoEncoders.msmpeg4) return true;
                    else return (false);
                case ContainerFormats.WEBM:
                    if (encoder == VideoEncoders.libvpx) return true;
                    else return (false); 
                case ContainerFormats.GIF:
                    if (encoder == VideoEncoders.gif) return true;
                    else return (false);                
                case ContainerFormats.MP3:
                    if (encoder == VideoEncoders.none) return (true);
                    else return (false);              
                case ContainerFormats.M4A:
                    if (encoder == VideoEncoders.none) return (true);
                    else return (false);
                case ContainerFormats.OGG:
                    if (encoder == VideoEncoders.none) return (true);
                    else return (false);   
                default:
                    return (false);
            }
        }

        bool supportedAudioEncodersFilter(Object audioEncoder)
        {
            AudioEncoders encoder = (AudioEncoders)audioEncoder;

            switch (ContainerFormat)
            {
                case ContainerFormats.MP4:
                    if (encoder == AudioEncoders.libmp3lame || encoder == AudioEncoders.libvo_aacenc) return (true);
                    else return (false);                   
                case ContainerFormats.MOV:
                    if (encoder == AudioEncoders.libmp3lame || encoder == AudioEncoders.libvo_aacenc) return (true);
                    else return (false);                
                case ContainerFormats.AVI:
                    if (encoder == AudioEncoders.libmp3lame) return (true);
                    else return (false);
                case ContainerFormats.MKV:
                    if (encoder == AudioEncoders.libmp3lame || encoder == AudioEncoders.libvo_aacenc || encoder == AudioEncoders.libvorbis) return (true);
                    else return (false);    
                case ContainerFormats.FLV:
                    if (encoder == AudioEncoders.libmp3lame || encoder == AudioEncoders.libvo_aacenc) return (true);
                    else return (false);
                case ContainerFormats.WMV:
                    if (encoder == AudioEncoders.wmav2) return true;
                    else return (false);
                case ContainerFormats.WEBM:
                    if (encoder == AudioEncoders.libvorbis) return true;
                    else return (false); 
                case ContainerFormats.GIF:
                    if (encoder == AudioEncoders.none) return (true);
                    else return (false);
                case ContainerFormats.MP3:
                    if (encoder == AudioEncoders.libmp3lame) return (true);
                    else return (false);
                case ContainerFormats.M4A:
                    if (encoder == AudioEncoders.libvo_aacenc) return (true);
                    else return (false);
                case ContainerFormats.OGG:
                    if(encoder == AudioEncoders.libvorbis) return(true);
                    else return (false);
                default:
                    return (false);
            }
        }


        void setDefaults()
        {
            ContainerFormat = ContainerFormats.MP4;

            VideoStreamMode = StreamOptions.Copy;
            VideoEncoder = VideoEncoders.libx264;
            VideoEncoderPreset = VideoEncoderPresets.Medium;
            
            AudioStreamMode = StreamOptions.Copy;
            AudioEncoder = AudioEncoders.libvo_aacenc;
            
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
            set {                                 
                SetProperty(ref containerFormat, value);

                SupportedAudioEncoders.Refresh();
                SupportedVideoEncoders.Refresh();

                if (containerFormat == ContainerFormats.MP3 || containerFormat == ContainerFormats.M4A ||
                    containerFormat == ContainerFormats.OGG)
                {
                    IsContainerSupportsVideoStream = false;
                    IsContainerSupportsAudioStream = true;
                    VideoStreamMode = StreamOptions.Discard;
                    AudioStreamMode = StreamOptions.Encode;
                
                }
                else if (containerFormat == ContainerFormats.GIF)
                {
                    IsContainerSupportsVideoStream = true;
                    IsContainerSupportsAudioStream = false;
                    AudioStreamMode = StreamOptions.Discard;
                    VideoStreamMode = StreamOptions.Encode;                  
                }
                else
                {
                    IsContainerSupportsVideoStream = true;
                    IsContainerSupportsAudioStream = true;
                    VideoStreamMode = StreamOptions.Copy;                    
                    AudioStreamMode = StreamOptions.Copy;                    
                }

                SupportedVideoEncoders.MoveCurrentToFirst();
                SupportedAudioEncoders.MoveCurrentToFirst();
            }
        }

        bool isContainerSupportsVideoStream;

        public bool IsContainerSupportsVideoStream
        {
            get { return isContainerSupportsVideoStream; }
            protected set { SetProperty(ref isContainerSupportsVideoStream, value); }
        }
        bool isContainerSupportsAudioStream;

        public bool IsContainerSupportsAudioStream
        {
            get { return isContainerSupportsAudioStream; }
            protected set { SetProperty(ref isContainerSupportsAudioStream, value); }
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

        ListCollectionView supportedVideoEncoders;

        public ListCollectionView SupportedVideoEncoders
        {
            get { return supportedVideoEncoders; }
            set { supportedVideoEncoders = value; }
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

        ListCollectionView supportedAudioEncoders;

        public ListCollectionView SupportedAudioEncoders
        {
            get { return supportedAudioEncoders; }
            set { supportedAudioEncoders = value; }
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
            set { SetProperty(ref sampleRate, value); }
        }

        int? nrChannels;

        public int? NrChannels
        {
            get { return nrChannels; }
            set { SetProperty(ref nrChannels, value); }
        }

        String title;

        public String Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        String iconUri;

        public String IconUri
        {
            get { return iconUri; }
            set { SetProperty(ref iconUri, value); }
        }

        bool isTimeRangeEnabled;

        public bool IsTimeRangeEnabled
        {
            get { return isTimeRangeEnabled; }
            set { SetProperty(ref isTimeRangeEnabled, value); }
        }

        double startTimeRange;

        public double StartTimeRange
        {
            get { return startTimeRange; }
            set { SetProperty(ref startTimeRange, value); }
        }
        double endTimeRange;

        public double EndTimeRange
        {
            get { return endTimeRange; }
            set { SetProperty(ref endTimeRange, value); }
        }
    }
}
