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
                if (AsyncState.IsConcat)
                {
                    startConcat();
                }
                else
                {
                    startTranscode();
                }

            }, CancellationToken, TaskCreationOptions.None, PriorityScheduler.BelowNormal);

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

            Dictionary<String, Object> encoderOptions = new Dictionary<string, object>();

            EncoderOptions.setVideoOptions(encoderOptions, AsyncState.VideoEncoderPreset, AsyncState.VideoEncoder);

            options.Add("videoEncoderOptions", encoderOptions);

            return (options);
        }

        void logCallback(int logLevel, String message)
        {
            InfoMessages.Add(message);
        }

        void startTranscode()
        {
            VideoLib.VideoOperations videoOperations = new VideoLib.VideoOperations();
            videoOperations.setLogCallback(logCallback, true, VideoLib.VideoOperations.LogLevel.LOG_LEVEL_INFO);
            TotalProgressMax = Items.Count;
            TotalProgress = 0;
            ItemProgressMax = 100;

            Dictionary<String, Object> options = getOptions();

            foreach (VideoAudioPair input in Items)
            {
                ItemProgress = 0;
                ItemInfo = "Transcoding: " + input.Name;

                if (CancellationToken.IsCancellationRequested) return;
                /*if (MediaFormatConvert.isImageFile(input.Location))
                {
                    InfoMessages.Add("Skipping: " + input.Name + " is not a video file.");
                    TotalProgress++;
                    continue;
                }*/

                String outLocation = getOutputLocation(input);

                try
                {
                    VideoLib.OpenVideoArgs openArgs =
                        new VideoLib.OpenVideoArgs(input.Location, null, input.Audio == null ? null : input.Audio.Location, null);

                    videoOperations.transcode(openArgs, outLocation, CancellationToken, options,
                        transcodeProgressCallback);
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

        void startConcat()
        {
            VideoLib.VideoOperations videoOperations = new VideoLib.VideoOperations();
            videoOperations.setLogCallback(logCallback, true, VideoLib.VideoOperations.LogLevel.LOG_LEVEL_INFO);
            TotalProgressMax = Items.Count;
            TotalProgress = 0;
            ItemProgressMax = 100;

            List<VideoLib.OpenVideoArgs> inputArgs = new List<VideoLib.OpenVideoArgs>();

            foreach (VideoAudioPair input in Items)
            {
                inputArgs.Add(new VideoLib.OpenVideoArgs(input.Location, null, input.Audio == null ? null : input.Audio.Location, null));
            }
            
            String outLocation = getOutputLocation(Items.ElementAt(0));
            Dictionary<String, Object> options = getOptions();

            try
            {
                videoOperations.concat(inputArgs, outLocation, CancellationToken, options,
                    concatProgressCallback);
            }
            catch (Exception e)
            {
                InfoMessages.Add("Error concatenating: " + e.Message);

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

            InfoMessages.Add("Finished concatenating: " + outLocation);

        }

        string getOutputLocation(VideoAudioPair input)
        {
            String outFilename;

            if (FileUtils.isUrl(input.Location))
            {
                outFilename = FileUtils.removeIllegalCharsFromFileName(input.Name, " ");

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

            return outLocation;
        }

        void transcodeProgressCallback(int totalProgress, double progress)
        {          
            ItemProgress = (int)(progress * 100);
        }

        void concatProgressCallback(int totalProgress, double progress)
        {
            TotalProgress = totalProgress;
            ItemInfo = "Concatenating: " + Items.ElementAt(TotalProgress).Name;

            ItemProgress = (int)(progress * 100);
        }

    }
}
