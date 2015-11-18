using MediaViewer.Model.Concurrency;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Mvvm;
using MediaViewer.Model.Utils;
using MediaViewer.Progress;
using MediaViewer.VideoPanel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;


namespace MediaViewer.Transcode.Video
{
    class VideoTranscodeProgressViewModel : CancellableOperationProgressBase
    {
        VideoTranscodeViewModel AsyncState { get; set; }
        ICollection<VideoAudioPair> Items { get; set; }

        public VideoTranscodeProgressViewModel(VideoTranscodeViewModel vm)
        {
            WindowIcon = "pack://application:,,,/Resources/Icons/videotranscode.ico";
            WindowTitle = "Video Transcoding Progress";

            AsyncState = vm;
            Items = vm.Items;

            CancelCommand.IsExecutable = true;
            OkCommand.IsExecutable = false;
                    
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

            if (AsyncState.FramesPerSecond.HasValue)
            {
                options.Add("framesPerSecond", AsyncState.FramesPerSecond.Value); 
            }

            if (AsyncState.SampleRate.HasValue)
            {
                options.Add("sampleRate", AsyncState.SampleRate.Value);
            }

            if (AsyncState.NrChannels.HasValue)
            {
                options.Add("nrChannels", AsyncState.NrChannels.Value);
            }

            if (AsyncState.IsTimeRangeEnabled)
            {
                options.Add("startTimeRange", AsyncState.StartTimeRange);
                options.Add("endTimeRange", AsyncState.EndTimeRange);
            }

            return (options);
        }

        void logCallback(int logLevel, String message)
        {
            InfoMessages.Add(message);
        }

        void startTranscode()
        {
            
            VideoLib.VideoTranscoder videoTranscoder = new VideoLib.VideoTranscoder();
            videoTranscoder.setLogCallback(logCallback, true, VideoLib.VideoTranscoder.LogLevel.LOG_LEVEL_INFO);
            TotalProgressMax = Items.Count;
            TotalProgress = 0;
            ItemProgressMax = 100;
           
            Dictionary<String, Object> options = getOptions();
                
            foreach (VideoAudioPair input in Items)
            {
                ItemProgress = 0;
                ItemInfo = "Transcoding: " + input.Name;

                if (CancellationToken.IsCancellationRequested) return;
                if (MediaFormatConvert.isImageFile(input.Location))
                {
                    InfoMessages.Add("Skipping: " + input.Name + " is not a video file.");
                    TotalProgress++;
                    continue;
                }

                String outFilename;

                if (FileUtils.isUrl(input.Location))
                {
                    outFilename = FileUtils.removeIllegalCharsFromFileName(input.Name," ");

                    if (String.IsNullOrEmpty(outFilename) || String.IsNullOrWhiteSpace(outFilename))
                    {
                        outFilename = "stream";
                    }
                }
                else
                {
                    outFilename = Path.GetFileNameWithoutExtension(input.Location);
                }

                String outLocation = AsyncState.OutputPath + "\\" + outFilename;

                outLocation += "." + AsyncState.ContainerFormat.ToString().ToLower();
                                       
                outLocation = FileUtils.getUniqueFileName(outLocation);

                try
                {
                    videoTranscoder.transcode(input.Location, outLocation, CancellationToken, options,
                        transcodeProgressCallback, input.Audio != null ? input.Audio.Location : null);
                }
                catch (Exception e)
                {
                    InfoMessages.Add("Error transcoding: " + e.Message);

                    try
                    {
                        File.Delete(outLocation);
                    }
                    catch (Exception ex)
                    {
                        InfoMessages.Add("Error deleting: " + outLocation + " " + ex.Message);
                    }
                    
                    return;
                }
                    
                ItemProgress = 100;
                TotalProgress++;

                InfoMessages.Add("Finished Transcoding: " + input.Name + " -> " + outLocation);
            }

        }

        void transcodeProgressCallback(double progress)
        {
            ItemProgress = (int)(progress * 100);
        }
       
    }
}
