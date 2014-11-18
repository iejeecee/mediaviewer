using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.MetaData;
using MediaViewer.Pager;
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
using MediaViewer.Settings;
using MediaViewer.Model.Global.Events;
using Microsoft.Practices.Prism.PubSubEvents;
using System.IO;
using Microsoft.Practices.Prism.Commands;
using MediaViewer.Model.Mvvm;
using Microsoft.Practices.Prism.Regions;
using MediaViewer.MediaGrid;
using MediaViewer.MediaFileBrowser;

namespace MediaViewer.VideoPanel
{

    public class VideoViewModel : BindableBase, IDisposable, IMediaFileBrowserContentViewModel
    {
        protected static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        ConcurrentQueue<Tuple<ICommand, Object>> commandQueue;
        IEventAggregator EventAggregator { get; set; }

        public VideoViewModel(AppSettings settings, IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;

            IsInitialized = false;
            CurrentLocation = null;
        
            commandQueue = new ConcurrentQueue<Tuple<ICommand, Object>>();
           
            OpenCommand = new Command<string>(location =>
            {
                if (addCommandToQueue(OpenCommand, location) == true) return;
                            
                try
                {
                    videoPlayer.open(location);
                    CurrentLocation = location;
                    ScreenShotLocation = FileUtils.getPathWithoutFileName(location); //settings.VideoScreenShotLocation;
                    ScreenShotName = System.IO.Path.GetFileName(location);
                   
                }
                catch (Exception e)
                {
                    CurrentLocation = null;
                    MessageBox.Show("Error opening " + location + "\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                EventAggregator.GetEvent<TitleChangedEvent>().Publish(location == null ? null : Path.GetFileName(location));
                
              
            });
            PlayCommand = new Command(() =>
            {
                if (addCommandToQueue(PlayCommand, null) == true) return;

                if (VideoState == VideoPlayerControl.VideoState.CLOSED && CurrentLocation != null)
                {
                    String location = CurrentLocation;

                    try
                    {
                        videoPlayer.open(location);
                        videoPlayer.startPlay();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Error opening " + location + "\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
                else if (VideoState == VideoPlayerControl.VideoState.OPEN || VideoState == VideoPlayerControl.VideoState.PAUSED)
                {
                    videoPlayer.startPlay();
                }

            }, false);

            PauseCommand = new Command(() => {

                if (addCommandToQueue(PauseCommand, null) == true) return;
                videoPlayer.pausePlay(); 

            }, false);

            CloseCommand = new Command(() => {

                if (addCommandToQueue(CloseCommand, null) == true) return;
                videoPlayer.close(); 

            }, false);

            SeekCommand = new Command<double?>((pos) => {

                if (addCommandToQueue(SeekCommand, pos) == true) return;
                videoPlayer.seek(pos.Value); 

            }, false);

            FrameByFrameCommand = new Command(() => {

                if (addCommandToQueue(FrameByFrameCommand, null) == true) return;
                videoPlayer.frameByFrame(); 

            }, false);

            ScreenShotCommand = new Command(() =>
            {
                if (addCommandToQueue(ScreenShotCommand, null) == true) return;

                try
                {
                    videoPlayer.createScreenShot();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error creating screenshot.\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }, false);

         
            HasAudio = true;
            VideoState = VideoPlayerControl.VideoState.CLOSED;
        }
       
        bool isInitialized;

        public bool IsInitialized
        {
            get { return isInitialized; }
            private set {  
            SetProperty(ref isInitialized, value);
            }
        }

        VideoPlayerViewModel videoPlayer;
        
        public void initializeVideoPlayer(VideoPlayerViewModel videoPlayer)
        {
            if (IsInitialized == true) return;

            this.videoPlayer = videoPlayer;
                      
            videoPlayer.Log = log;
                       
            videoPlayer.StateChanged += videoPlayer_StateChanged;
            videoPlayer.PositionSecondsChanged += videoPlayer_PositionSecondsChanged;
            videoPlayer.DurationSecondsChanged += videoPlayer_DurationSecondsChanged;
            videoPlayer.HasAudioChanged += videoPlayer_HasAudioChanged;

            MinVolume = videoPlayer.MinVolume;
            MaxVolume = videoPlayer.MaxVolume;
            videoPlayer.IsMuted = IsMuted;
            videoPlayer.Volume = Volume;
                        
            // execute queued commands
            Tuple<ICommand, Object> tuple;

            while (commandQueue.TryDequeue(out tuple))
            {
                ICommand command = tuple.Item1;
                Object arg = tuple.Item2;

                command.Execute(arg);
            }

            IsInitialized = true;
        }

        bool addCommandToQueue(ICommand command, Object argument)
        {
            
            if (IsInitialized == false)
            {
                commandQueue.Enqueue(new Tuple<ICommand, object>(command, argument));
                return (true);
            }

            return (false);
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
                if (videoPlayer != null)
                {
                    CloseCommand.Execute();

                    videoPlayer.StateChanged -= videoPlayer_StateChanged;
                    videoPlayer.PositionSecondsChanged -= videoPlayer_PositionSecondsChanged;
                    videoPlayer.DurationSecondsChanged -= videoPlayer_DurationSecondsChanged;
                    videoPlayer.HasAudioChanged -= videoPlayer_HasAudioChanged;

                    videoPlayer.Dispose();
                    videoPlayer = null;

                }

                IsInitialized = false;
            }
  
        }

        private void videoPlayer_HasAudioChanged(object sender, bool newHasAudio)
        {
            HasAudio = newHasAudio;
        }
       
        public string ScreenShotLocation
        {
            get { return videoPlayer.ScreenShotLocation; }
            set { videoPlayer.ScreenShotLocation = value;
                OnPropertyChanged("ScreenShotLocation");           
            }
        }

        public string ScreenShotName
        {
            get { return videoPlayer.ScreenShotName; }
            set
            {
                videoPlayer.ScreenShotName = value;
                OnPropertyChanged("ScreenShotName");                
            }
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

        VideoState videoState;

        public VideoState VideoState
        {
            get { return videoState; }
            set {  
                SetProperty(ref videoState, value);
            }
        }

        void videoPlayer_StateChanged(object sender, VideoState newVideoState)
        {
            VideoState = newVideoState;

            switch (videoState)
            {
                case VideoState.OPEN:
                    {
                        playCommand.IsExecutable = true;
                        pauseCommand.IsExecutable = false;
                        screenShotCommand.IsExecutable = false;
                        closeCommand.IsExecutable = true;
                        seekCommand.IsExecutable = false;
                        frameByFrameCommand.IsExecutable = false;
                        break;
                    }
                case VideoState.PLAYING:
                    {
                        playCommand.IsExecutable = false;
                        pauseCommand.IsExecutable = true;
                        screenShotCommand.IsExecutable = true;
                        closeCommand.IsExecutable = true;
                        seekCommand.IsExecutable = true;
                        frameByFrameCommand.IsExecutable = true;
                        break;
                    }
                case VideoState.PAUSED:
                    {
                        playCommand.IsExecutable = true;
                        pauseCommand.IsExecutable = false;
                        screenShotCommand.IsExecutable = true;
                        closeCommand.IsExecutable = true;
                        seekCommand.IsExecutable = true;
                        frameByFrameCommand.IsExecutable = true;
                        break;
                    }
                case VideoState.CLOSED:
                    {
                        playCommand.IsExecutable = true;
                        pauseCommand.IsExecutable = false;
                        screenShotCommand.IsExecutable = false;
                        closeCommand.IsExecutable = false;
                        seekCommand.IsExecutable = false;
                        frameByFrameCommand.IsExecutable = false;
                        break;
                    }
            }
        }


        /// <summary>
        /// VIEWMODEL INTERFACE
        /// </summary>
        Command<String> openCommand;

        public Command<String> OpenCommand
        {
            get { return openCommand; }
            set { openCommand = value; }
        }

        Command playCommand;

        public Command PlayCommand
        {
            get { return playCommand; }
            set { playCommand = value; }
        }
        Command pauseCommand;

        public Command PauseCommand
        {
            get { return pauseCommand; }
            set { pauseCommand = value; }
        }
        Command closeCommand;

        public Command CloseCommand
        {
            get { return closeCommand; }
            set { closeCommand = value; }
        }

        Command screenShotCommand;

        public Command ScreenShotCommand
        {
            get { return screenShotCommand; }
            set { screenShotCommand = value; }
        }

        Command<double?> seekCommand;

        public Command<double?> SeekCommand
        {
            get { return seekCommand; }
            set { seekCommand = value; }
        }

        Command frameByFrameCommand;

        public Command FrameByFrameCommand
        {
            get { return frameByFrameCommand; }
            set { frameByFrameCommand = value; }
        }

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
                videoPlayer.Volume = value;

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
                videoPlayer.IsMuted = value;
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

        String currentLocation;

        public String CurrentLocation
        {
            get
            {
                return currentLocation;
            }
            protected set
            {                
                SetProperty(ref currentLocation, value);
            }
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            if (navigationContext.Uri.Equals(new Uri(typeof(MediaGridView).FullName, UriKind.Relative)))
            {

                CloseCommand.Execute();
            }

            EventAggregator.GetEvent<MediaSelectionEvent>().Unsubscribe(videoView_MediaSelectionEvent);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            String location = (String)navigationContext.Parameters["location"];
            int? offsetSeconds = (int?)navigationContext.Parameters["offsetSeconds"];

            if (!String.IsNullOrEmpty(location))
            {
                OpenCommand.Execute(location);
                PlayCommand.Execute();

                if (offsetSeconds != null)
                {
                    SeekCommand.Execute(offsetSeconds);
                }
            }
            else
            {
                EventAggregator.GetEvent<TitleChangedEvent>().Publish(CurrentLocation == null ? "" : CurrentLocation);
            }

            EventAggregator.GetEvent<MediaSelectionEvent>().Subscribe(videoView_MediaSelectionEvent, ThreadOption.UIThread);

        }

        private void videoView_MediaSelectionEvent(MediaFileItem item)
        {
            if (String.Equals(CurrentLocation, item.Location)) return;

            OpenCommand.Execute(item.Location);
            PlayCommand.Execute();
        }
    }
}
