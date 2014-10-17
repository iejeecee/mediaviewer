using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.MetaData;
using MediaViewer.Pager;
using MvvmFoundation.Wpf;
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
using MediaViewer.Model.GlobalEvents;
using Microsoft.Practices.Prism.PubSubEvents;
using System.IO;

namespace MediaViewer.VideoPanel
{

    public class VideoViewModel : ObservableObject, IDisposable
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
              
                tokenSource = new CancellationTokenSource();
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

            SeekCommand = new Command<double>((pos) => {

                if (addCommandToQueue(SeekCommand, pos) == true) return;
                videoPlayer.seek(pos); 

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
            private set { isInitialized = value;
            NotifyPropertyChanged();
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

        ~VideoViewModel()
        {
            Dispose(false);
        }

        public virtual void Dispose(bool cleanupManaged)
        {
            if (IsInitialized == false) return;

            if (cleanupManaged)
            {
                // cleanup managed resources
            }

            // cleanup unmanaged
            if (videoPlayer != null)
            {
                CloseCommand.DoExecute();

                videoPlayer.StateChanged -= videoPlayer_StateChanged;
                videoPlayer.PositionSecondsChanged -= videoPlayer_PositionSecondsChanged;
                videoPlayer.DurationSecondsChanged -= videoPlayer_DurationSecondsChanged;
                videoPlayer.HasAudioChanged -= videoPlayer_HasAudioChanged;

                videoPlayer.Dispose();
                videoPlayer = null;

            }

            IsInitialized = false;
        }

      
       
        private void videoPlayer_HasAudioChanged(object sender, bool newHasAudio)
        {
            HasAudio = newHasAudio;
        }
       
        public string ScreenShotLocation
        {
            get { return videoPlayer.ScreenShotLocation; }
            set { videoPlayer.ScreenShotLocation = value;
            NotifyPropertyChanged();
            }
        }

        public string ScreenShotName
        {
            get { return videoPlayer.ScreenShotName; }
            set
            {
                videoPlayer.ScreenShotName = value;
                NotifyPropertyChanged();
            }
        }

        int durationSeconds;

        public int DurationSeconds
        {
            get { return durationSeconds; }
            set { durationSeconds = value;
            NotifyPropertyChanged();
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
            set { positionSeconds = value;
            NotifyPropertyChanged();
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
            set { videoState = value;
            NotifyPropertyChanged();
            }
        }

        void videoPlayer_StateChanged(object sender, VideoState newVideoState)
        {
            VideoState = newVideoState;

            switch (videoState)
            {
                case VideoState.OPEN:
                    {
                        playCommand.CanExecute = true;
                        pauseCommand.CanExecute = false;
                        screenShotCommand.CanExecute = false;
                        closeCommand.CanExecute = true;
                        seekCommand.CanExecute = false;
                        frameByFrameCommand.CanExecute = false;
                        break;
                    }
                case VideoState.PLAYING:
                    {
                        playCommand.CanExecute = false;
                        pauseCommand.CanExecute = true;
                        screenShotCommand.CanExecute = true;
                        closeCommand.CanExecute = true;
                        seekCommand.CanExecute = true;
                        frameByFrameCommand.CanExecute = true;
                        break;
                    }
                case VideoState.PAUSED:
                    {
                        playCommand.CanExecute = true;
                        pauseCommand.CanExecute = false;
                        screenShotCommand.CanExecute = true;
                        closeCommand.CanExecute = true;
                        seekCommand.CanExecute = true;
                        frameByFrameCommand.CanExecute = true;
                        break;
                    }
                case VideoState.CLOSED:
                    {
                        playCommand.CanExecute = true;
                        pauseCommand.CanExecute = false;
                        screenShotCommand.CanExecute = false;
                        closeCommand.CanExecute = false;
                        seekCommand.CanExecute = false;
                        frameByFrameCommand.CanExecute = false;
                        break;
                    }
            }
        }

    
        CancellationTokenSource tokenSource;

        public CancellationTokenSource TokenSource
        {
            get { return tokenSource; }
            set { tokenSource = value; }
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

        Command<double> seekCommand;

        public Command<double> SeekCommand
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
            private set { maxVolume = value;
            NotifyPropertyChanged();
            }
            get { return maxVolume; }
        }

        int minVolume;

        public int MinVolume
        {
            private set { minVolume = value;
            NotifyPropertyChanged();
            }
            get { return minVolume; }
        }

        double volume;

        public double Volume
        {
            set
            {
                volume = videoPlayer.Volume = value;
                NotifyPropertyChanged();
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
                isMuted = videoPlayer.IsMuted = value;
                NotifyPropertyChanged();
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
            set { hasAudio = value;
            NotifyPropertyChanged();
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
                currentLocation = value;
                NotifyPropertyChanged();
            }
        }
    }
}
