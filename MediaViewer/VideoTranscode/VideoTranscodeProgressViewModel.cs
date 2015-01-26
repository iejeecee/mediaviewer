using MediaViewer.Model.Concurrency;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Mvvm;
using MediaViewer.Model.Utils;
using MediaViewer.Progress;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;


namespace MediaViewer.VideoTranscode
{
    class VideoTranscodeProgressViewModel : CloseableBindableBase, ICancellableOperationProgress
    {
        VideoTranscodeViewModel AsyncState { get; set; }
        ICollection<MediaFileItem> Items { get; set; }

        public VideoTranscodeProgressViewModel(ICollection<MediaFileItem> items, VideoTranscodeViewModel vm)
        {
            WindowIcon = "pack://application:,,,/Resources/Icons/videotranscode.ico";
            WindowTitle = "Video Transcoding Progress";

            AsyncState = vm;
            Items = items;
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            CancellationToken = tokenSource.Token;

            CancelCommand = new Command(() =>
            {
                tokenSource.Cancel();
            });

            OkCommand = new Command(() =>
                {
                    OnClosingRequest();
                },false);

            InfoMessages = new ObservableCollection<string>();
        }

        public async Task startTranscodeAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                startTranscode();

            },CancellationToken, TaskCreationOptions.None, PriorityScheduler.BelowNormal);

            OkCommand.IsExecutable = true;
            CancelCommand.IsExecutable = false;

        }

        Dictionary<String, Object> getOptions()
        {
            Dictionary<String, Object> options = new Dictionary<string, object>();
            options.Add("videoStreamMode", AsyncState.VideoStreamMode);
            options.Add("audioStreamMode", AsyncState.AudioStreamMode);
            options.Add("videoEncoder", AsyncState.VideoEncoder);
            options.Add("audioEncoder", AsyncState.AudioEncoder);
            options.Add("videoEncoderPreset", AsyncState.VideoEncoderPreset);

            if (AsyncState.Width.HasValue)
            {
                options.Add("width", AsyncState.Width.Value);
            }

            if (AsyncState.Height.HasValue)
            {
                options.Add("height", AsyncState.Height.Value);
            }

            if (AsyncState.SampleRate.HasValue)
            {
                options.Add("sampleRate", AsyncState.SampleRate.Value);
            }

            if (AsyncState.IsTimeRangeEnabled)
            {
                options.Add("startTimeRange", AsyncState.StartTimeRange);
                options.Add("endTimeRange", AsyncState.EndTimeRange);
            }

            return (options);
        }

        void startTranscode()
        {
            VideoLib.VideoTranscoder videoTranscoder = new VideoLib.VideoTranscoder();
            TotalProgressMax = Items.Count;
            TotalProgress = 0;
            itemProgressMax = 100;

            try
            {
                Dictionary<String, Object> options = getOptions();
                
                foreach (MediaFileItem input in Items)
                {
                    ItemProgress = 0;
                    ItemInfo = "Transcoding: " + input.Location;

                    if (CancellationToken.IsCancellationRequested) break;
                    if (!MediaFormatConvert.isVideoFile(input.Location))
                    {
                        InfoMessages.Add("Skipping: " + input.Location + " is not a video file.");
                        TotalProgress++;
                        continue;
                    }

                    String outLocation = AsyncState.OutputPath + "\\" + Path.GetFileNameWithoutExtension(input.Location);

                    outLocation += "." + AsyncState.ContainerFormat.ToString().ToLower();
                                       
                    outLocation = FileUtils.getUniqueFileName(outLocation);
                                       
                    videoTranscoder.transcode(input.Location, outLocation, CancellationToken, options, 
                        transcodeProgressCallback);

                    ItemProgress = 100;
                    TotalProgress++;

                    InfoMessages.Add("Finished Transcoding: " + input.Location + " -> " + outLocation);
                }

            }
            catch (Exception e)
            {
                InfoMessages.Add("Error transcoding: " + e.Message);
            }
        }

        void transcodeProgressCallback(double progress)
        {
            ItemProgress = (int)(progress * 100);
        }

        string itemInfo;

        public string ItemInfo
        {
            get
            {
                return itemInfo;
            }
            set
            {
                SetProperty(ref itemInfo, value);
            }
        }

        int itemProgress;

        public int ItemProgress
        {
            get
            {
                return itemProgress;
            }
            set
            {
                SetProperty(ref itemProgress, value);
            }
        }

        int itemProgressMax;

        public int ItemProgressMax
        {
            get
            {
                return itemProgressMax;
            }
            set
            {
                SetProperty(ref itemProgressMax, value);
            }
        }


        ObservableCollection<string> infoMessages;

        public System.Collections.ObjectModel.ObservableCollection<string> InfoMessages
        {
            get
            {
                return infoMessages;
            }
            set
            {

                SetProperty(ref infoMessages, value);
            }
        }

        public Model.Mvvm.Command OkCommand {get;set;}
        public Model.Mvvm.Command CancelCommand { get; set; }

        public System.Threading.CancellationToken CancellationToken { get; set; }

        string windowTitle;

        public string WindowTitle
        {
            get
            {
                return(windowTitle);
            }

            set {

                SetProperty(ref windowTitle, value);
            }
        }

        String windowIcon;

        public string WindowIcon
        {
            get
            {
                return (windowIcon);
            }
            set
            {

                SetProperty(ref windowIcon, value);
            }
        }

        int totalProgress;

        public int TotalProgress
        {
            get
            {
                return (totalProgress);
            }
            set
            {
                SetProperty(ref totalProgress, value);
            }
        }

        int totalProgressMax;

        public int TotalProgressMax
        {
            get
            {
                return (totalProgressMax);
            }
            set
            {
                SetProperty(ref totalProgressMax, value);
            }
        }
    }
}
