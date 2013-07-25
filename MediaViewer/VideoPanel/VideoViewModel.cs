using MediaViewer.MediaFileModel;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Threading;

namespace MediaViewer.VideoPanel
{
    class VideoViewModel : ObservableObject
    {
        DispatcherTimer progressTimer;

        public VideoViewModel()
        {
            // initialize with dummy MediaElement
            VideoPlayer = new MediaElement();
            VideoLocation = "";
            TotalMilliseconds = 0;
            PositionMilliseconds = 0;

            
       
            playCommand = new Command(new Action(() =>
            {
                VideoPlayer.Play();
                isPlaying = true;
            }));

            stopCommand = new Command(new Action(() =>
            {
                VideoPlayer.Stop();
                VideoPlayer.Close();
                isPlaying = false;
            }));

            pauseCommand = new Command(new Action(() =>
            {
                VideoPlayer.Pause();
                isPlaying = false;
            }));

            setPositionMillisecondsCommand = new Command<double>(new Action<double>((ms) =>
            {
                TimeSpan newTime = new TimeSpan(0,0,0,0, (int)ms);

                VideoPlayer.Position = newTime;
            }));

            progressTimer = new DispatcherTimer();
            progressTimer.Interval = TimeSpan.FromSeconds(1);
            progressTimer.Tick += new EventHandler(progressTimer_Tick);
            progressTimer.Start();
        }

        void progressTimer_Tick(Object sender, EventArgs e)
        {
            PositionMilliseconds = VideoPlayer.Position.TotalMilliseconds;
        }

        MediaElement videoPlayer;

        public MediaElement VideoPlayer
        {
            get { return videoPlayer; }
            set { 

                videoPlayer = value;               

                videoPlayer.MediaOpened += new RoutedEventHandler(videoPlayer_MediaOpened);
            }
        }

        private void videoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {

            TotalMilliseconds = VideoPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;

        }

        bool isSeekable;

        public bool IsSeekable
        {
            get { return videoPlayer.IsMuted; }
            set { isSeekable = value; }
        }


        double positionMilliseconds;

        public double PositionMilliseconds
        {
            get { return positionMilliseconds; }
            set { positionMilliseconds = value;
            NotifyPropertyChanged();
            }
        }

        double totalMilliseconds;

        public double TotalMilliseconds
        {
            get { return totalMilliseconds; }
            set { totalMilliseconds = value;
            NotifyPropertyChanged();
            }
        }


        String videoLocation;

        public String VideoLocation
        {
            get { return videoLocation; }
            set { videoLocation = value;
            NotifyPropertyChanged();
            }
        }
    

        public double Volume
        {
            get { return VideoPlayer.Volume; }
            set
            {
                VideoPlayer.Volume = value;
            NotifyPropertyChanged();
            }
        }

        Command<double> setPositionMillisecondsCommand;

        public Command<double> SetPositionMillisecondsCommand
        {
            get { return setPositionMillisecondsCommand; }
            set { setPositionMillisecondsCommand = value; }
        }

        Command playCommand;

        public Command PlayCommand
        {
            get { return playCommand; }
            set { playCommand = value; }
        }
        Command stopCommand;

        public Command StopCommand
        {
            get { return stopCommand; }
            set { stopCommand = value; }
        }
        Command pauseCommand;

        public Command PauseCommand
        {
            get { return pauseCommand; }
            set { pauseCommand = value; }
        }

        bool isPlaying;

        public bool IsPlaying
        {
            get { return isPlaying; }
            set
            {
                if (value == true)
                {
                    PlayCommand.DoExecute();
                }
                else
                {
                    PauseCommand.DoExecute();
                }

                NotifyPropertyChanged();
            }
        }

        public bool IsMuted
        {
            get { return videoPlayer.IsMuted; }
            set
            {
                videoPlayer.IsMuted = value;
                NotifyPropertyChanged();
            }
        }


    }
}
