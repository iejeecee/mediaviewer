using MediaViewer.Model.Mvvm;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoPlayerControl;

namespace MediaViewer.VideoPanel
{
    class VideoDebugViewModel : CloseableBindableBase
    {
        VideoPlayerViewModel VideoPlayer { get; set; }
        public Command CloseCommand { get; set; }

        public VideoDebugViewModel(VideoPlayerViewModel videoPlayer)
        {
            VideoPlayer = videoPlayer;
            VideoPlayer.DebugVariablesChanged += VideoPlayer_DebugVariablesChanged;

            CloseCommand = new Command(() => {
                
                OnClosingRequest();
            });
        }

        private void VideoPlayer_DebugVariablesChanged(object sender, DebugVariables e)
        {
            MaxVideoPacketsInQueue = e.MaxAudioPacketsInQueue;
            MaxAudioPacketsInQueue = e.MaxAudioPacketsInQueue;
            VideoPacketsInQueue = e.VideoPacketsInQueue;
            AudioPacketsInQueue = e.AudioPacketsInQueue;
            VideoClock = e.VideoClock;
            AudioClock = e.AudioClock;
            NrFramesDropped = e.NrFramesDropped;
        }

        int maxVideoPacketsInQueue;
        public int MaxVideoPacketsInQueue {
            get
            {
                return (maxVideoPacketsInQueue);
            }
            set
            {
                SetProperty(ref maxVideoPacketsInQueue, value);
            }
        }

        int maxAudioPacketsInQueue;
        public int MaxAudioPacketsInQueue
        {
            get
            {
                return (maxAudioPacketsInQueue);
            }
            set
            {
                SetProperty(ref maxAudioPacketsInQueue, value);
            }
        }

        int videoPacketsInQueue;
        public int VideoPacketsInQueue
        {
            get
            {
                return (videoPacketsInQueue);
            }
            set
            {
                SetProperty(ref videoPacketsInQueue, value);
            }
        }

        int audioPacketsInQueue;
        public int AudioPacketsInQueue
        {
            get
            {
                return (audioPacketsInQueue);
            }
            set
            {
                SetProperty(ref audioPacketsInQueue, value);
            }
        }

        double videoClock;
        public double VideoClock
        {
            get
            {
                return (videoClock);
            }
            set
            {
                SetProperty(ref videoClock, value);
            }
        }

        double audioClock;
        public double AudioClock
        {
            get
            {
                return (audioClock);
            }
            set
            {
                SetProperty(ref audioClock, value);
            }
        }

        int nrFramesDropped;
        public int NrFramesDropped
        {
            get
            {
                return (nrFramesDropped);
            }
            set
            {
                SetProperty(ref nrFramesDropped, value);
            }
        }
    }
}
