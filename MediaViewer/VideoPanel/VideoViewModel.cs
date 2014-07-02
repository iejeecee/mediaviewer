using MediaViewer.MediaFileModel;
using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.Pager;
using MediaViewer.Utils;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VideoPlayerControl;

namespace MediaViewer.VideoPanel
{
    public class VideoViewModel : ObservableObject, IPageable
    {

        VideoPlayerViewModel videoPlayer;

        public VideoViewModel(VideoPlayerViewModel videoPlayer)
        {
            if (videoPlayer == null)
            {
                throw new ArgumentException("videoPlayer cannot be null");
            }

            this.videoPlayer = videoPlayer;

            selectedMedia = new MediaLockedCollection();

            OpenCommand = new Command<string>(async (location) => {

                selectedMedia.Clear();
                tokenSource = new CancellationTokenSource();
                try
                {
                    videoPlayer.open(location);
                    ScreenShotLocation = MediaFileWatcher.Instance.Path;
                    ScreenShotName = System.IO.Path.GetFileName(location);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error opening " + location + "\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                updatePaging();

                MediaFileItem item = MediaFileItem.Factory.create(location);

                await item.readMetaDataAsync(MediaFactory.ReadOptions.AUTO | MediaFactory.ReadOptions.GENERATE_THUMBNAIL, tokenSource.Token);

                selectedMedia.Add(item);
               
            });
            PlayCommand = new Command(() => {

                if (VideoState == VideoPlayerControl.VideoState.CLOSED && selectedMedia.Count == 1)
                {
                    String location = selectedMedia.Items[0].Location;

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

            PauseCommand = new Command(videoPlayer.pausePlay, false);
            CloseCommand = new Command(videoPlayer.close, false);
            SeekCommand = new Command<double>(videoPlayer.seek, false);
            FrameByFrameCommand = new Command(videoPlayer.frameByFrame, false);

            ScreenShotCommand = new Command(() => {

                try
                {
                    videoPlayer.createScreenShot();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error creating screenshot.\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }, false);

            videoPlayer.StateChanged += videoPlayer_StateChanged;
            videoPlayer.PositionSecondsChanged += videoPlayer_PositionSecondsChanged;
            videoPlayer.DurationSecondsChanged += videoPlayer_DurationSecondsChanged;
            videoPlayer.HasAudioChanged += videoPlayer_HasAudioChanged;

            MediaState.NrItemsInStateChanged += new NotifyCollectionChangedEventHandler((s, e) =>
            {
                updatePaging();

            });

            nextPageCommand = new Command(new Action(() =>
            {

                CurrentPage += 1;

            }));

            prevPageCommand = new Command(new Action(() =>
            {

                CurrentPage -= 1;

            }));

            firstPageCommand = new Command(new Action(() =>
            {
                CurrentPage = 1;
            }));

            lastPageCommand = new Command(new Action(() =>
            {
                CurrentPage = NrPages;
            }));
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

        MediaLockedCollection selectedMedia;

        public MediaLockedCollection SelectedMedia
        {
            get { return selectedMedia; }
            set { selectedMedia = value;
            NotifyPropertyChanged();
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

        public int MinVolume
        {
            get { return videoPlayer.MinVolume; }
        }   

        public int MaxVolume
        {
            get { return videoPlayer.MaxVolume; }
        }

        public double Volume
        {
            set
            {
                videoPlayer.Volume = value;
                NotifyPropertyChanged();
            }
            get
            {
                return videoPlayer.Volume;
            }
        }

        public bool IsMuted
        {
            set
            {
                videoPlayer.IsMuted = value;
                NotifyPropertyChanged();
            }
            get
            {
                return videoPlayer.IsMuted;
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

        MediaState MediaState
        {
            get
            {
                return (MediaFileWatcher.Instance.MediaState);
            }
        }

        bool isPagingEnabled;

        public bool IsPagingEnabled
        {
            get { return isPagingEnabled; }
            set
            {
                isPagingEnabled = value;
                if (value == false)
                {
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        NextPageCommand.CanExecute = false;
                        PrevPageCommand.CanExecute = false;
                        FirstPageCommand.CanExecute = false;
                        LastPageCommand.CanExecute = false;
                    }));

                }
                NotifyPropertyChanged();
            }
        }


        int nrPages;

        public int NrPages
        {
            get
            {
                return nrPages;
            }
            set
            {
                nrPages = value;
                NotifyPropertyChanged();
            }
        }

        int currentPage;

        public int CurrentPage
        {
            get
            {
                return (currentPage);
            }
            set
            {
                if (value <= 0 || value > NrPages || IsPagingEnabled == false) return;
                
                String location = null;

                MediaState.UIMediaCollection.EnterReaderLock();
                try
                {
                   location = getVideoFileByIndex(value - 1);
                }
                finally
                {
                    MediaState.UIMediaCollection.ExitReaderLock();
                }

                if (location != null && !location.ToLower().Equals(videoPlayer.VideoLocation.ToLower())) 
                {
                    CloseCommand.DoExecute();
                    OpenCommand.DoExecute(location);
                    PlayCommand.DoExecute();                       
                }
               
                currentPage = value;
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (CurrentPage + 1 <= NrPages)
                    {
                        nextPageCommand.CanExecute = true;
                        lastPageCommand.CanExecute = true;
                    }
                    else
                    {
                        nextPageCommand.CanExecute = false;
                        lastPageCommand.CanExecute = false;
                    }

                    if (CurrentPage - 1 > 0)
                    {
                        prevPageCommand.CanExecute = true;
                        firstPageCommand.CanExecute = true;
                    }
                    else
                    {
                        prevPageCommand.CanExecute = false;
                        firstPageCommand.CanExecute = false;
                    }
                }));

                NotifyPropertyChanged();
            }
        }

        Command nextPageCommand;

        public Command NextPageCommand
        {
            get
            {
                return nextPageCommand;
            }
            set
            {
                nextPageCommand = value;
            }
        }

        Command prevPageCommand;

        public Command PrevPageCommand
        {
            get
            {
                return prevPageCommand;
            }
            set
            {
                prevPageCommand = value;
            }
        }

        Command firstPageCommand;

        public Command FirstPageCommand
        {
            get
            {
                return firstPageCommand;
            }
            set
            {
                firstPageCommand = value;
            }
        }

        Command lastPageCommand;

        public Command LastPageCommand
        {
            get
            {
                return lastPageCommand;
            }
            set
            {
                lastPageCommand = value;
            }
        }

        void updatePaging()
        {
            MediaState.UIMediaCollection.EnterReaderLock();
            try
            {
                int index = getVideoFileIndex(videoPlayer.VideoLocation);

                if (index == -1)
                {
                    IsPagingEnabled = false;
                }
                else
                {
                    IsPagingEnabled = true;
                    NrPages = getNrVideoFiles();
                    CurrentPage = index + 1;
                }
            }
            finally
            {
                MediaState.UIMediaCollection.ExitReaderLock();
            }
        }

        int getNrVideoFiles()
        {
            int count = 0;

            foreach (MediaFileItem item in MediaState.UIMediaCollection.Items)
            {
                if (MediaFormatConvert.isVideoFile(item.Location))
                {
                    count++;
                }
            }

            return (count);
        }

        string getVideoFileByIndex(int index)
        {

            int i = 0;

            foreach (MediaFileItem item in MediaState.UIMediaCollection.Items)
            {

                if (MediaFormatConvert.isVideoFile(item.Location))
                {
                    if (index == i)
                    {
                        return (item.Location);
                    }

                    i++;
                }

            }

            return (null);
        }

        int getVideoFileIndex(string location)
        {

            int i = 0;

            foreach (MediaFileItem item in MediaState.UIMediaCollection.Items)
            {

                if (MediaFormatConvert.isVideoFile(item.Location))
                {
                    if (item.Location.ToLower().Equals(location.ToLower()))
                    {
                        return (i);
                    }

                    i++;
                }

            }

            return (-1);
        }
    }
}
