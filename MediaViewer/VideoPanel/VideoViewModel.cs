using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.MetaData;
using MediaViewer.UserControls.Pager;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VideoPlayerControl;
using MediaViewer.Model.Media.State;
using MediaViewer.Model.Utils;
using MediaViewer.Model.Settings;
using MediaViewer.Model.Global.Events;
using Microsoft.Practices.Prism.PubSubEvents;
using System.IO;
using Microsoft.Practices.Prism.Commands;
using MediaViewer.Model.Mvvm;
using Microsoft.Practices.Prism.Regions;
using MediaViewer.MediaFileBrowser;
using MediaViewer.Transcode.Video;
using MediaViewer.Infrastructure.Video.TranscodeOptions;
using MediaViewer.Infrastructure.Global.Events;
using Microsoft.Practices.ServiceLocation;
using MediaViewer.Infrastructure.Logging;
using MediaViewer.Model.Media.Base;
using MediaViewer.UserControls.MediaGrid;
using MediaViewer.MediaFileGrid;
using MediaViewer.Model.Media.Streamed;
using MediaViewer.Model.Utils.Windows;

namespace MediaViewer.VideoPanel
{

    public class VideoViewModel : BindableBase, IDisposable, IMediaFileBrowserContentViewModel
    {

        VideoSettingsViewModel VideoSettings { get; set; }
                  
        IEventAggregator EventAggregator { get; set; }

        SemaphoreSlim isInitializedSignal;
      
        public VideoViewModel(AppSettings settings, IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
            VideoSettings = ServiceLocator.Current.GetInstance(typeof(VideoSettingsViewModel)) as VideoSettingsViewModel;

            IsInitialized = false;
            isInitializedSignal = new SemaphoreSlim(0, 1);

            CurrentItem = new VideoAudioPair(null, null);
        
                      
            OpenCommand = new AsyncCommand<VideoAudioPair>(async item =>
            {
                await isInitializedSignal.WaitAsync();

                try
                {
                    CurrentItem = item;

                    String videoFormatName = null;

                    if (item.Video is MediaStreamedItem)
                    {
                        if (item.Video.Metadata != null)
                        {
                            videoFormatName = MediaFormatConvert.mimeTypeToExtension(item.Video.Metadata.MimeType);
                        }
                    }

                    String audioLocation = null;
                    String audioFormatName = null;

                    if (item.Audio != null)
                    {
                        audioLocation = item.Audio.Location;

                        if (item.Audio is MediaStreamedItem)
                        {
                            audioFormatName = MediaFormatConvert.mimeTypeToExtension(item.Audio.Metadata.MimeType);
                        }

                    }
                    
                    CloseCommand.IsExecutable = true;

                    await VideoPlayer.close();

                    IsLoading = true;

                    await VideoPlayer.open(item.Video.Location, videoFormatName, audioLocation, audioFormatName);                   
                }
                catch (OperationCanceledException)
                {
                    CloseCommand.IsExecutable = false;
                    throw;
                }
                catch (Exception e)
                {
                    CloseCommand.IsExecutable = false;                   
                    MessageBox.Show("Error opening " + item.Video.Location + "\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw;
                }
                finally
                {
                    IsLoading = false;
                    isInitializedSignal.Release();
                }
               
                EventAggregator.GetEvent<TitleChangedEvent>().Publish(CurrentItem.IsEmpty ? null : CurrentItem.Video.Name);              
                
            });

            PlayCommand = new AsyncCommand(async () =>
            {
                             
                if (VideoState == VideoPlayerControl.VideoState.CLOSED && !CurrentItem.IsEmpty)
                {
                    await openAndPlay(CurrentItem); 
                }
                else if (VideoState == VideoPlayerControl.VideoState.OPEN || VideoState == VideoPlayerControl.VideoState.PAUSED)
                {
                    VideoPlayer.startPlay();
                }                

            }, false);

            PauseCommand = new AsyncCommand(async () => {
              
                await VideoPlayer.pausePlay(); 

            }, false);

            CloseCommand = new AsyncCommand(async () => {
                     
                await VideoPlayer.close(); 

            }, false);

            SeekCommand = new AsyncCommand<double>(async (pos) =>
            {
              
                await VideoPlayer.seek(pos); 

            }, false);

            FrameByFrameCommand = new AsyncCommand(async () => {

                FrameByFrameCommand.IsExecutable = false;
                await VideoPlayer.displayNextFrame();
                FrameByFrameCommand.IsExecutable = true;

            }, false);

            CutVideoCommand = new Command(() =>
            {
                VideoTranscodeView videoTranscode = new VideoTranscodeView();            
                videoTranscode.ViewModel.Items.Add(MediaFileItem.Factory.create(VideoPlayer.VideoLocation));
                videoTranscode.ViewModel.Title = "Cut Video";
                videoTranscode.ViewModel.IconUri = "/MediaViewer;component/Resources/Icons/videocut.ico";

                String outputPath;

                if(FileUtils.isUrl(VideoPlayer.VideoLocation)) {

                    outputPath = MediaFileWatcher.Instance.Path;

                } else {

                    outputPath = FileUtils.getPathWithoutFileName(VideoPlayer.VideoLocation);
                }

                videoTranscode.ViewModel.OutputPath = outputPath;
                videoTranscode.ViewModel.IsTimeRangeEnabled = IsTimeRangeEnabled;
                videoTranscode.ViewModel.StartTimeRange = StartTimeRange;
                videoTranscode.ViewModel.EndTimeRange = EndTimeRange;

                String extension = Path.GetExtension(VideoPlayer.VideoLocation).ToLower().TrimStart('.');

                foreach(ContainerFormats format in Enum.GetValues(typeof(ContainerFormats))) {

                    if (format.ToString().ToLower().Equals(extension))
                    {
                        videoTranscode.ViewModel.ContainerFormat = format;
                    }
                }
                
                videoTranscode.ShowDialog();

            }, false);

            ScreenShotCommand = new Command(() =>
            {             
                try
                {
                    String screenShotName = FileUtils.removeIllegalCharsFromFileName(CurrentItem.Video.Name, " ");

                    screenShotName += "." + "jpg";

                    String fullPath = VideoSettings.Settings.VideoScreenShotLocation + "\\" + screenShotName;
                    fullPath = FileUtils.getUniqueFileName(fullPath);

                    VideoPlayer.createScreenShot(fullPath, VideoSettings.Settings.VideoScreenShotTimeOffset);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error creating screenshot.\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }, false);
        
            SetLeftMarkerCommand = new Command(() =>
                {
                    if (IsTimeRangeEnabled == false)
                    {
                        IsTimeRangeEnabled = true;
                    }

                    StartTimeRange = VideoPlayer.PositionSeconds;

                }, false);

            SetRightMarkerCommand = new Command(() =>
            {
                if (IsTimeRangeEnabled == false)
                {
                    IsTimeRangeEnabled = true;
                    StartTimeRange = VideoPlayer.PositionSeconds;
                }
                else
                {
                    EndTimeRange = VideoPlayer.PositionSeconds;
                }

            }, false);

            OpenLocationCommand = new Command(async () =>
            {
                Microsoft.Win32.OpenFileDialog dialog = FileDialog.createOpenMediaFileDialog(FileDialog.MediaDialogType.VIDEO);
                bool? success = dialog.ShowDialog();
                if (success == true)
                {
                    MediaItem video = MediaItemFactory.create(dialog.FileName);

                    await openAndPlay(new VideoAudioPair(video, null));
                }

            });
         
            HasAudio = true;
            VideoState = VideoPlayerControl.VideoState.CLOSED;
           
            IsTimeRangeEnabled = false;
            StartTimeRange = 0;
            EndTimeRange = 0;
        }
       
        bool isInitialized;

        public bool IsInitialized
        {
            get { 
                
                return isInitialized;             
            }
            private set {  
            SetProperty(ref isInitialized, value);
            }
        }

        public VideoPlayerViewModel VideoPlayer { get; protected set; }
        
        public void initializeVideoPlayer(VideoPlayerViewModel videoPlayer)
        {
            if (IsInitialized == true) return;

            this.VideoPlayer = videoPlayer;
                                                     
            videoPlayer.StateChanged += videoPlayer_StateChanged;
            videoPlayer.PositionSecondsChanged += videoPlayer_PositionSecondsChanged;
            videoPlayer.DurationSecondsChanged += videoPlayer_DurationSecondsChanged;
            videoPlayer.HasAudioChanged += videoPlayer_HasAudioChanged;

            MinVolume = videoPlayer.MinVolume;
            MaxVolume = videoPlayer.MaxVolume;
            videoPlayer.IsMuted = IsMuted;
            videoPlayer.Volume = Volume;
                                          
            IsInitialized = true;
            isInitializedSignal.Release();
        }
       
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool cleanupManaged)
        {
            if (IsInitialized == false) return;

            if (cleanupManaged)
            {
                // cleanup unmanaged
                if (VideoPlayer != null)
                {
                    CloseCommand.Execute();

                    VideoPlayer.StateChanged -= videoPlayer_StateChanged;
                    VideoPlayer.PositionSecondsChanged -= videoPlayer_PositionSecondsChanged;
                    VideoPlayer.DurationSecondsChanged -= videoPlayer_DurationSecondsChanged;
                    VideoPlayer.HasAudioChanged -= videoPlayer_HasAudioChanged;

                    VideoPlayer.Dispose();
                    VideoPlayer = null;

                }

                IsInitialized = false;
            }
  
        }
       
        private void videoPlayer_HasAudioChanged(object sender, bool newHasAudio)
        {
            HasAudio = newHasAudio;
        }                 

        int durationSeconds;

        public int DurationSeconds
        {
            get { return durationSeconds; }
            set {
                SetProperty(ref durationSeconds, value);
            }
        }

        private void videoPlayer_DurationSecondsChanged(object sender, int newDurationSeconds)
        {
            DurationSeconds = newDurationSeconds;
        }

        int positionSeconds;

        public int PositionSeconds
        {
            get { return positionSeconds; }
            set {
                positionSeconds = value;
                OnPropertyChanged("PositionSeconds");
            }
        }

        private void videoPlayer_PositionSecondsChanged(object sender, int newPositionSeconds)
        {
            PositionSeconds = newPositionSeconds;
        }

        bool isTimeRangeEnabled;

        public bool IsTimeRangeEnabled
        {
            get { return isTimeRangeEnabled; }
            set {
               
                CutVideoCommand.IsExecutable = value;                
                SetProperty(ref isTimeRangeEnabled, value); 
            }
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

        VideoState videoState;

        public VideoState VideoState
        {
            get { return videoState; }
            set {  
                SetProperty(ref videoState, value);
            }
        }

        bool isLoading;

        public bool IsLoading
        {
            get { return isLoading; }
            set { SetProperty(ref isLoading, value); }
        }

        void videoPlayer_StateChanged(object sender, VideoState newVideoState)
        {                      
            VideoState = newVideoState;

            switch (videoState)
            {
                case VideoState.OPEN:
                    {
                        PlayCommand.IsExecutable = true;
                        PauseCommand.IsExecutable = false;
                        ScreenShotCommand.IsExecutable = false;
                        CloseCommand.IsExecutable = true;
                        SeekCommand.IsExecutable = false;
                        FrameByFrameCommand.IsExecutable = false;                      
                        SetLeftMarkerCommand.IsExecutable = true;
                        SetRightMarkerCommand.IsExecutable = true;
                        break;
                    }
                case VideoState.PLAYING:
                    {
                        PlayCommand.IsExecutable = false;
                        PauseCommand.IsExecutable = true;
                        ScreenShotCommand.IsExecutable = true;
                        CloseCommand.IsExecutable = true;
                        SeekCommand.IsExecutable = true;
                        FrameByFrameCommand.IsExecutable = true;                      
                        SetLeftMarkerCommand.IsExecutable = true;
                        SetRightMarkerCommand.IsExecutable = true;
                        break;
                    }
                case VideoState.PAUSED:
                    {
                        PlayCommand.IsExecutable = true;
                        PauseCommand.IsExecutable = false;
                        ScreenShotCommand.IsExecutable = true;
                        CloseCommand.IsExecutable = true;
                        SeekCommand.IsExecutable = true;
                        FrameByFrameCommand.IsExecutable = true;                       
                        SetLeftMarkerCommand.IsExecutable = true;
                        SetRightMarkerCommand.IsExecutable = true;
                        break;
                    }
                case VideoState.CLOSED:
                    {
                        PlayCommand.IsExecutable = true;
                        PauseCommand.IsExecutable = false;
                        ScreenShotCommand.IsExecutable = false;
                        CloseCommand.IsExecutable = false;
                        SeekCommand.IsExecutable = false;
                        FrameByFrameCommand.IsExecutable = false;                       
                        IsTimeRangeEnabled = false;                   
                        SetLeftMarkerCommand.IsExecutable = false;
                        SetRightMarkerCommand.IsExecutable = false;
                        break;
                    }
            }
          
        }


        /// <summary>
        /// VIEWMODEL INTERFACE
        /// </summary>
        public Command OpenLocationCommand { get; set; }
        public AsyncCommand<VideoAudioPair> OpenCommand { get; set; }
        public AsyncCommand PlayCommand { get; set; }
        public AsyncCommand PauseCommand { get; set; }
        public AsyncCommand CloseCommand { get; set; }
        public Command ScreenShotCommand { get; set; }
        public AsyncCommand<double> SeekCommand { get; set; }
        public AsyncCommand FrameByFrameCommand { get; set; }      
        public Command SetLeftMarkerCommand { get; set; }
        public Command SetRightMarkerCommand { get; set; }
        public Command CutVideoCommand { get; set; }
   
        int maxVolume;

        public int MaxVolume        
        {
            private set {  
                SetProperty(ref maxVolume, value);
            }
            get { return maxVolume; }
        }

        int minVolume;

        public int MinVolume
        {
            private set {  
             SetProperty(ref minVolume, value);
            }
            get { return minVolume; }
        }

        double volume;

        public double Volume
        {
            set
            {   
                VideoPlayer.Volume = value;

                SetProperty(ref volume, value);
            }
            get
            {
                return volume;
            }
        }

        bool isMuted;

        public bool IsMuted
        {
            set
            {
                VideoPlayer.IsMuted = value;
                SetProperty(ref isMuted, value);
            }
            get
            {
                return isMuted;
            }
        }

        bool hasAudio;

        public bool HasAudio
        {
            get { return hasAudio; }
            set {  
                SetProperty(ref hasAudio, value);
            }
        }

        VideoAudioPair currentItem;

        public VideoAudioPair CurrentItem
        {
            get
            {
                return currentItem;
            }
            protected set
            {                
                SetProperty(ref currentItem, value);
            }
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            if (navigationContext.Uri.Equals(new Uri(typeof(MediaFileGridView).FullName, UriKind.Relative)))
            {

                CloseCommand.Execute();
            }

            EventAggregator.GetEvent<MediaSelectionEvent>().Unsubscribe(videoView_MediaSelectionEvent);
           
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            EventAggregator.GetEvent<MediaSelectionEvent>().Subscribe(videoView_MediaSelectionEvent, ThreadOption.UIThread);

            MediaItem video = (MediaItem)navigationContext.Parameters["item"];
            int? offsetSeconds = (int?)navigationContext.Parameters["offsetSeconds"];
            MediaItem audio = (MediaItem)navigationContext.Parameters["audio"];

            if (video != null)
            {
                await openAndPlay(new VideoAudioPair(video, audio), offsetSeconds);                                
            }
            else
            {
                EventAggregator.GetEvent<TitleChangedEvent>().Publish(CurrentItem.IsEmpty ? null : CurrentItem.Video.Name);
            }
                                  
        }

        private async void videoView_MediaSelectionEvent(MediaSelectionPayload selection)
        {
            if (selection.Items.Count() == 0) return;

            MediaItem first = selection.Items.ElementAt(0);

            if (!CurrentItem.IsEmpty && String.Equals(CurrentItem.Video.Location, first.Location)) return;

            await openAndPlay(new VideoAudioPair(first, null));
        
        }

        async Task openAndPlay(VideoAudioPair item, int? offsetSeconds = null)
        {
            try
            {
                EventAggregator.GetEvent<TitleChangedEvent>().Publish(item.Video.Name);

                await OpenCommand.ExecuteAsync(item);
                PlayCommand.Execute();

                if (offsetSeconds != null)
                {
                    await SeekCommand.ExecuteAsync(offsetSeconds.Value);
                }
                
            }
            catch (Exception)
            {

            }

        }


        bool playerIsInitialized() {

#if DEBUG
            if(IsInitialized == false) {

                throw new Exception("Trying to execute command before videoplayer is initialized");
            }
#endif
            return(IsInitialized);

        }
        
    }
}
