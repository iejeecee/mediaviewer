using VideoPlayerControl.Timers;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VideoLib;
using System.Drawing;
using System.Drawing.Imaging;

namespace VideoPlayerControl
{
   public class VideoPlayerViewModel : ObservableObject, IDisposable
    {
        //protected static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public event EventHandler VideoOpened;
        public event EventHandler VideoClosed;

        Control owner;

        VideoLib.VideoPlayer.DecodedVideoFormat decodedVideoFormat;

        public VideoLib.VideoPlayer.DecodedVideoFormat DecodedVideoFormat
        {
            get
            {
                return (decodedVideoFormat);
            }
        }

        VideoState videoState;

        public VideoState VideoState
        {
            get { return videoState; }
            set
            {
                videoState = value;
                switch (videoState)
                {
                    case VideoState.OPEN:
                        {
                            playCommand.CanExecute = true;
                            pauseCommand.CanExecute = false;
                            screenShotCommand.CanExecute = false;
                            closeCommand.CanExecute = true;
                            break;
                        }
                    case VideoState.PLAYING:
                        {
                            playCommand.CanExecute = false;
                            pauseCommand.CanExecute = true;
                            screenShotCommand.CanExecute = true;
                            closeCommand.CanExecute = true;
                            break;
                        }
                    case VideoState.PAUSED:
                        {
                            playCommand.CanExecute = true;
                            pauseCommand.CanExecute = false;
                            screenShotCommand.CanExecute = true;
                            closeCommand.CanExecute = true;
                            break;
                        }
                    case VideoState.CLOSED:
                        {
                            playCommand.CanExecute = false;
                            pauseCommand.CanExecute = false;
                            screenShotCommand.CanExecute = false;
                            closeCommand.CanExecute = false;
                            break;
                        }

                }
                NotifyPropertyChanged();
            }
        }

        // no AV sync correction is done if below the AV sync threshold 
        const double AV_SYNC_THRESHOLD = 0.01;
        // no AV sync correction is done if too big error 
        const double AV_NOSYNC_THRESHOLD = 10.0;

        const double AUDIO_SAMPLE_CORRECTION_PERCENT_MAX = 10;

        // we use about AUDIO_DIFF_AVG_NB A-V differences to make the average 
        const int AUDIO_DIFF_AVG_NB = 5;//20;

        VideoLib.VideoPlayer videoDecoder;
        AudioPlayer audioPlayer;
        VideoRender videoRender;


        public int Width
        {
            get { return videoDecoder.Width; }

        }

        public int Height
        {
            get { return videoDecoder.Height; }

        }

        enum SyncMode
        {

            AUDIO_SYNCS_TO_VIDEO,
            VIDEO_SYNCS_TO_AUDIO

        };

        SyncMode syncMode;

        double previousVideoPts;
        double previousVideoDelay;

        double videoFrameTimer;
        double audioFrameTimer;

        HRTimer videoRefreshTimer;
        HRTimer audioRefreshTimer;

        double videoPts;
        double videoPtsDrift;

        double audioDiffCum;
        double audioDiffAvgCoef;
        double audioDiffThreshold;
        int audioDiffAvgCount;

        bool seekRequest;
        double seekPosition;     

        Task demuxPacketsTask;
        CancellationTokenSource demuxPacketsCancellationTokenSource;

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

        string screenShotLocation;

        public string ScreenShotLocation
        {
            get { return screenShotLocation; }
            set { screenShotLocation = value;
            NotifyPropertyChanged();
            }
        }

        string screenShotName;

        public string ScreenShotName
        {
            get { return screenShotName; }
            set { screenShotName = value;
            NotifyPropertyChanged();
            }
        }
   
        ImageFormat screenShotFormat;

        public ImageFormat ScreenShotFormat
        {
            get { return screenShotFormat; }
            set { screenShotFormat = value;
            NotifyPropertyChanged();
            }
        }

        int positionSeconds;

        public int PositionSeconds
        {
            get { return positionSeconds; }
            set { positionSeconds = value;
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

        public bool IsMuted
        {
            get { return audioPlayer.IsMuted; }
            set
            {
                audioPlayer.IsMuted = value;
                NotifyPropertyChanged();
            }
        }

        bool hasAudio;

        public bool HasAudio {
            get
            {
                return (hasAudio);
            }
            set
            {
                hasAudio = value;
                NotifyPropertyChanged();
            }
        }

        public double Volume
        {
            get { return audioPlayer.Volume; }
            set { audioPlayer.Volume = value;
            NotifyPropertyChanged();
            }
        }

        public int MaxVolume
        {
            get { return audioPlayer.MaxVolume; }           
        }
  
        public int MinVolume
        {
            get { return audioPlayer.MinVolume; }        
        }

        double videoClock;

        public double VideoClock
        {
            get { return videoClock; }
            set { videoClock = value;
            NotifyPropertyChanged();
            }
        }

        double audioClock;

        public double AudioClock
        {
            get { return audioClock; }
            set { audioClock = value;
            NotifyPropertyChanged();
            }
        }

        log4net.ILog log;

        public log4net.ILog Log
        {
            get { return log; }
            set { log = value; }
        }
   
        public VideoPlayerViewModel(Control owner, 
            VideoLib.VideoPlayer.DecodedVideoFormat decodedVideoFormat = VideoLib.VideoPlayer.DecodedVideoFormat.YUV420P)
        {
            OpenCommand = new Command<string>(open);
            PlayCommand = new Command(startPlay, false);
            PauseCommand = new Command(pausePlay, false);
            CloseCommand = new Command(close, false);

            this.owner = owner;
            this.decodedVideoFormat = decodedVideoFormat;            
       
            videoDecoder = new VideoLib.VideoPlayer();
            videoDecoder.setLogCallback(videoDecoderLogCallback, true, true);

            audioPlayer = new AudioPlayer(owner);
            videoRender = new VideoRender(owner);

            audioDiffAvgCoef = Math.Exp(Math.Log(0.01) / AUDIO_DIFF_AVG_NB);

            syncMode = SyncMode.AUDIO_SYNCS_TO_VIDEO;
            
            videoRefreshTimer = HRTimerFactory.create(HRTimerFactory.TimerType.TIMER_QUEUE);
            videoRefreshTimer.Tick += new EventHandler(videoRefreshTimer_Tick);
            videoRefreshTimer.AutoReset = false;

            audioRefreshTimer = HRTimerFactory.create(HRTimerFactory.TimerType.TIMER_QUEUE);
            audioRefreshTimer.Tick += new EventHandler(audioRefreshTimer_Tick);
            audioRefreshTimer.AutoReset = false;

            ScreenShotLocation = "";
            ScreenShotName = "";
            ScreenShotFormat = ImageFormat.Png;
            ScreenShotCommand = new Command(() =>
            {
                videoRender.createScreenShot(ScreenShotLocation, ScreenShotName, ScreenShotFormat);
            });

            DurationSeconds = 0;
            PositionSeconds = 0;

            owner.HandleDestroyed += new EventHandler((s, e) => close());

            VideoState = VideoState.CLOSED;        
        }
     
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool safe)
        {
            if (safe)
            {
                if (videoRender != null)
                {
                    videoRender.Dispose();
                    videoRender = null;
                }
                if (audioPlayer != null)
                {
                    audioPlayer.Dispose();
                    audioPlayer = null;
                }
                if (videoDecoder != null)
                {
                    videoDecoder.Dispose();
                    videoDecoder = null;
                }
                if (demuxPacketsTask != null)
                {
                    demuxPacketsTask.Dispose();
                    demuxPacketsTask = null;
                }
                if (demuxPacketsCancellationTokenSource != null)
                {
                    demuxPacketsCancellationTokenSource.Dispose();
                    demuxPacketsCancellationTokenSource = null; 
                }
                if (videoRefreshTimer != null)
                {
                    videoRefreshTimer.Dispose();
                    videoRefreshTimer = null;
                }
                if (audioRefreshTimer != null)
                {
                    audioRefreshTimer.Dispose();
                    audioRefreshTimer = null;
                }
                
            }
        }
      
        void videoRefreshTimer_Tick(Object sender, EventArgs e)
        {

            bool skipVideoFrame = false;

restartvideo:
			
			double actualDelay = 0.04;

			// grab a decoded frame, returns false if the queue is stopped
			VideoFrame videoFrame = videoDecoder.FrameQueue.getDecodedVideoFrame();

			if(VideoState == VideoState.CLOSED && videoFrame == null) {

				return;

			} else if(VideoState == VideoState.PLAYING) {

				videoPts = videoFrame.Pts;
				videoPtsDrift = videoFrame.Pts + HRTimer.getTimestamp();

				if(skipVideoFrame == false) {

					videoRender.display(videoFrame, Color.Black, VideoRender.RenderMode.NORMAL);					
				} 					

				actualDelay = synchronizeVideo(videoPts);					

			} else if(VideoState == VideoState.PAUSED) {

				videoRender.display(null, Color.Black, VideoRender.RenderMode.PAUSED);			
			}

            updateObservableVariables();
        
            if (actualDelay < 0.010)
            {

                // delay is too small skip next frame
                skipVideoFrame = true;
                //videoDebug.NrVideoFramesDropped = videoDebug.NrVideoFramesDropped + 1;
                goto restartvideo;

            }

            // start timer with delay for next frame
            videoRefreshTimer.Interval = (int)(actualDelay * 1000 + 0.5);
            videoRefreshTimer.start();

        }

        void updateObservableVariables()
        {
            PositionSeconds = (int)Math.Floor(getVideoClock());
            VideoClock = getVideoClock();
            AudioClock = audioPlayer.getAudioClock();
        }

        double synchronizeVideo(double videoPts)
        {

            // assume delay to next frame equals delay between previous frames
            double delay = videoPts - previousVideoPts;

            if (delay <= 0 || delay >= 1.0)
            {
                // if incorrect delay, use previous one 
                delay = previousVideoDelay;
            }

            previousVideoPts = videoPts;
            previousVideoDelay = delay;

            if (videoDecoder.HasAudio && syncMode == SyncMode.VIDEO_SYNCS_TO_AUDIO)
            {

                // synchronize video to audio
                double diff = getVideoClock() - audioPlayer.getAudioClock();

                // Skip or repeat the frame. Take delay into account
                // FFPlay still doesn't "know if this is the best guess."
                double sync_threshold = (delay > AV_SYNC_THRESHOLD) ? delay : AV_SYNC_THRESHOLD;

                if (Math.Abs(diff) < AV_NOSYNC_THRESHOLD)
                {

                    if (diff <= -sync_threshold)
                    {

                        delay = 0;

                    }
                    else if (diff >= sync_threshold)
                    {

                        delay = 2 * delay;
                    }
                }

            }
            // adjust delay based on the actual current time
            videoFrameTimer += delay;
            double actualDelay = videoFrameTimer - HRTimer.getTimestamp();
            /*
                        videoDebug.VideoDelay = delay;
                        videoDebug.ActualVideoDelay = actualDelay;
                        videoDebug.VideoSync = getVideoClock();
                        videoDebug.AudioSync = audioPlayer.getAudioClock();
                        videoDebug.VideoQueueSize = videoDecoder.FrameQueue.VideoPacketsInQueue;
                        videoDebug.AudioQueueSize = videoDecoder.FrameQueue.AudioPacketsInQueue;
            */

            return (actualDelay);
        }

        void audioRefreshTimer_Tick(Object sender, EventArgs e)
        {

        restartaudio:

            VideoLib.AudioFrame audioFrame = videoDecoder.FrameQueue.getDecodedAudioFrame();
            if (audioFrame == null) return;

            //videoDebug.AudioFrames = videoDebug.AudioFrames + 1;
            //videoDebug.AudioFrameLength = audioFrame.Length;

            // if the audio is lagging behind too much, skip the buffer completely
            double diff = getVideoClock() - audioFrame.Pts;
            if (diff > 0.2 && diff < 3 && syncMode == SyncMode.AUDIO_SYNCS_TO_VIDEO)
            {

                //log.Warn("dropping audio buffer, lagging behind: " + (getVideoClock() - audioFrame.Pts).ToString() + " seconds");
                goto restartaudio;
            }

            //adjustAudioSamplesPerSecond(audioFrame);
            adjustAudioLength(audioFrame);

            audioPlayer.write(audioFrame);

            int frameLength = audioFrame.Length;

            double actualDelay = synchronizeAudio(frameLength);

            if (actualDelay < 0)
            {

                // delay too small, play next frame as quickly as possible
                //videoDebug.NrAudioFramesLaggingBehind = videoDebug.NrAudioFramesLaggingBehind + 1;
                goto restartaudio;

            }

            // start timer with delay for next frame
            audioRefreshTimer.Interval = (int)(actualDelay * 1000 + 0.5);
            audioRefreshTimer.start();

        }

        double synchronizeAudio(int frameLength)
        {

            // calculate delay to play next frame
            int bytesPerSecond = audioPlayer.SamplesPerSecond * videoDecoder.BytesPerSample * videoDecoder.NrChannels;

            double delay = frameLength / (double)bytesPerSecond;

            // adjust delay based on the actual current time
            audioFrameTimer += delay;
            double actualDelay = audioFrameTimer - HRTimer.getTimestamp();

            //videoDebug.AudioDelay = delay;
            //videoDebug.ActualAudioDelay = actualDelay;			

            return (actualDelay);
        }


        void adjustAudioSamplesPerSecond(VideoLib.AudioFrame frame)
        {

            //videoDebug.AudioFrameLengthAdjust = 0;

            if (syncMode == SyncMode.AUDIO_SYNCS_TO_VIDEO)
            {

                int n = videoDecoder.NrChannels * videoDecoder.BytesPerSample;

                double diff = audioPlayer.getAudioClock() - getVideoClock();

                if (Math.Abs(diff) < AV_NOSYNC_THRESHOLD)
                {

                    // accumulate the diffs
                    audioDiffCum = diff + audioDiffAvgCoef * audioDiffCum;

                    if (audioDiffAvgCount < AUDIO_DIFF_AVG_NB)
                    {

                        audioDiffAvgCount++;

                    }
                    else
                    {

                        double avgDiff = audioDiffCum * (1.0 - audioDiffAvgCoef);

                        // Shrinking/expanding buffer code....
                        if (Math.Abs(avgDiff) >= audioDiffThreshold)
                        {

                            int wantedSize = (int)(frame.Length + diff * videoDecoder.SamplesPerSecond * n);

                            // get a correction percent from 10 to 60 based on the avgDiff
                            // in order to converge a little faster
                            double correctionPercent = Utils.clamp(10 + (Math.Abs(avgDiff) - audioDiffThreshold) * 15, 10, 60);

                            //Util.DebugOut(correctionPercent);

                            //AUDIO_SAMPLE_CORRECTION_PERCENT_MAX

                            int minSize = (int)(frame.Length * ((100 - correctionPercent)
                                / 100));

                            int maxSize = (int)(frame.Length * ((100 + correctionPercent)
                                / 100));

                            if (wantedSize < minSize)
                            {

                                wantedSize = minSize;

                            }
                            else if (wantedSize > maxSize)
                            {

                                wantedSize = maxSize;
                            }

                            // adjust samples per second to speed up or slow down the audio
                            Int64 length = frame.Length;
                            Int64 sps = videoDecoder.SamplesPerSecond;
                            int samplesPerSecond = (int)((length * sps) / wantedSize);
                            //videoDebug.AudioFrameLengthAdjust = samplesPerSecond;
                            audioPlayer.SamplesPerSecond = samplesPerSecond;

                        }
                        else
                        {

                            audioPlayer.SamplesPerSecond = videoDecoder.SamplesPerSecond;
                        }

                    }

                }
                else
                {

                    // difference is TOO big; reset diff stuff 
                    audioDiffAvgCount = 0;
                    audioDiffCum = 0;
                }
            }

        }


        void adjustAudioLength(VideoLib.AudioFrame frame)
        {

            //videoDebug.AudioFrameLengthAdjust = 0;

            if (syncMode == SyncMode.AUDIO_SYNCS_TO_VIDEO)
            {

                int n = videoDecoder.NrChannels * videoDecoder.BytesPerSample;

                double diff = audioPlayer.getAudioClock() - getVideoClock();

                if (Math.Abs(diff) < AV_NOSYNC_THRESHOLD)
                {

                    // accumulate the diffs
                    audioDiffCum = diff + audioDiffAvgCoef * audioDiffCum;

                    if (audioDiffAvgCount < AUDIO_DIFF_AVG_NB)
                    {

                        audioDiffAvgCount++;

                    }
                    else
                    {

                        double avgDiff = audioDiffCum * (1.0 - audioDiffAvgCoef);

                        // Shrinking/expanding buffer code....
                        if (Math.Abs(avgDiff) >= audioDiffThreshold)
                        {

                            int wantedSize = (int)(frame.Length + diff * videoDecoder.SamplesPerSecond * n);

                            // get a correction percent from 10 to 60 based on the avgDiff
                            // in order to converge a little faster
                            double correctionPercent = Utils.clamp(10 + (Math.Abs(avgDiff) - audioDiffThreshold) * 15, 10, 60);

                            //Util.DebugOut(correctionPercent);

                            //AUDIO_SAMPLE_CORRECTION_PERCENT_MAX

                            int minSize = (int)(frame.Length * ((100 - correctionPercent)
                                / 100));

                            int maxSize = (int)(frame.Length * ((100 + correctionPercent)
                                / 100));

                            if (wantedSize < minSize)
                            {

                                wantedSize = minSize;

                            }
                            else if (wantedSize > maxSize)
                            {

                                wantedSize = maxSize;
                            }

                            // make sure the samples stay aligned after resizing the buffer
                            wantedSize = (wantedSize / n) * n;

                            if (wantedSize < frame.Length)
                            {

                                // remove samples 
                                //videoDebug.AudioFrameLengthAdjust = wantedSize - frame.Length;
                                frame.Length = wantedSize;

                            }
                            else if (wantedSize > frame.Length)
                            {

                                // add samples by copying final samples
                                int nrExtraSamples = wantedSize - frame.Length;
                                //videoDebug.AudioFrameLengthAdjust = nrExtraSamples;

                                byte[] lastSample = new byte[n];

                                for (int i = 0; i < n; i++)
                                {

                                    lastSample[i] = frame.Data[frame.Length - n + i];
                                }

                                frame.Stream.Position = frame.Length;

                                while (nrExtraSamples > 0)
                                {

                                    frame.Stream.Write(lastSample, 0, n);
                                    nrExtraSamples -= n;
                                }

                                frame.Stream.Position = 0;
                                frame.Length = wantedSize;
                            }

                        }

                    }

                }
                else
                {

                    // difference is TOO big; reset diff stuff 
                    audioDiffAvgCount = 0;
                    audioDiffCum = 0;
                }
            }

        }

        void videoDecoderLogCallback(int level, string message)
        {
            if (Log == null) return;

            if (level < 16)
            {
                Log.Fatal(message);
            }
            else if (level == 16)
            {
                Log.Error(message);
            }
            else if (level == 24)
            {
                Log.Warn(message);
            }
            else
            {
                Log.Info(message);
            }
        }

        void fillFrameQueue()
        {
            bool success = true;

            while (videoDecoder.FrameQueue.AudioPacketsInQueue !=
                videoDecoder.FrameQueue.MaxAudioPackets &&
                videoDecoder.FrameQueue.VideoPacketsInQueue !=
                videoDecoder.FrameQueue.MaxVideoPackets &&
                success == true)
            {

                success = videoDecoder.demuxPacket();
            }

        }


        void demuxPackets(CancellationToken token)
        {
            audioFrameTimer = videoFrameTimer = HRTimer.getTimestamp();

            bool success = true;

            // decode frames one by one, or handle seek requests
            do
            {
                if (seekRequest == true)
                {

                    // wait for video and audio decoding to pause/block
                    // To make sure no packets are in limbo
                    // before flushing any ffmpeg internal or external queues. 
                    videoDecoder.FrameQueue.pause();

                    if (videoDecoder.seek(seekPosition) == true)
                    {

                        // flush the framequeue	and audioplayer buffer				
                        videoDecoder.FrameQueue.flush();
                        audioPlayer.flush();

                        // refill/buffer the framequeue from the new position
                        fillFrameQueue();

                        audioFrameTimer = videoFrameTimer = HRTimer.getTimestamp();

                    }
                    seekRequest = false;

                    // allow video and audio decoding to continue
                    videoDecoder.FrameQueue.start();

                }
                else
                {

                    success = videoDecoder.demuxPacket();
                }

            } while (success == true && !token.IsCancellationRequested);

        }

        double getVideoClock()
        {
            if (videoState == VideoState.PAUSED)
            {
                return (videoPts);
            }
            else
            {
                return (videoPtsDrift - HRTimer.getTimestamp());
            }
        }

        void pausePlay()
        {

            if (VideoState == VideoState.PAUSED ||
                VideoState == VideoState.CLOSED)
            {

                return;
            }

            VideoState = VideoState.PAUSED;

            videoDecoder.FrameQueue.stop();

            demuxPacketsCancellationTokenSource.Cancel();

            demuxPacketsTask.Wait();
          
            audioPlayer.stop();
            

        }

        void startPlay()
        {       
            if (VideoState == VideoState.PLAYING ||
                VideoState == VideoState.CLOSED)
            {

                return;
            }

            VideoState = VideoState.PLAYING;

            audioPlayer.startPlayAfterNextWrite();

            videoDecoder.FrameQueue.start();

            demuxPacketsCancellationTokenSource = new CancellationTokenSource();
            demuxPacketsTask = new Task(() => { demuxPackets(demuxPacketsCancellationTokenSource.Token); },
                demuxPacketsCancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            demuxPacketsTask.Start();

            previousVideoPts = 0;
            previousVideoDelay = 0.04;

            audioDiffAvgCount = 0;

            videoRefreshTimer.start();
            audioRefreshTimer.start();

        }

        public void seek(double seconds)
        {

            seekPosition = seconds;
            seekRequest = true;
        }

        void open(string location)
        {         
            try
            {

                //log.Info("Opening: " + location);

                close();
                //videoDebug.clear();

                if (videoDecoder == null)
                {
                    // initialize videodecoder here instead of the constructor to prevent visual studio's designer
                    // choking on the dll's it attempts to load
                   
                }

                videoDecoder.open(location, decodedVideoFormat);                
                videoRender.initialize(videoDecoder.Width, videoDecoder.Height);

                DurationSeconds = videoDecoder.DurationSeconds;
                HasAudio = videoDecoder.HasAudio;

                if (videoDecoder.HasAudio)
                {

                    audioPlayer.initialize(videoDecoder.SamplesPerSecond, videoDecoder.BytesPerSample,
                        videoDecoder.NrChannels, videoDecoder.MaxAudioFrameSize * 2);

                    audioDiffThreshold = 2.0 * 1024 / videoDecoder.SamplesPerSecond;
                }
                                                 
                //videoDebug.VideoQueueMaxSize = videoDecoder.FrameQueue.MaxVideoPackets;
                //videoDebug.VideoQueueSizeBytes = videoDecoder.FrameQueue.VideoQueueSizeBytes;	
                //videoDebug.AudioQueueMaxSize = videoDecoder.FrameQueue.MaxAudioPackets;
                //videoDebug.AudioQueueSizeBytes = videoDecoder.FrameQueue.AudioQueueSizeBytes;

                if (syncMode == SyncMode.AUDIO_SYNCS_TO_VIDEO)
                {

                    //videoDebug.IsVideoSyncMaster = true;

                }
                else
                {

                    //videoDebug.IsAudioSyncMaster = true;
                }

                VideoState = VideoState.OPEN;

                if(VideoOpened != null) {

                    VideoOpened(this, EventArgs.Empty);
                }

            }
            catch (VideoLib.VideoLibException e)
            {

                VideoState = VideoState.CLOSED;

                //log.Error("Cannot open: " + location, e);

                throw new VideoPlayerException("Cannot open: " + location + "\n\n" + e.Message, e);
             
            }
        }

        void close()
        {

            if (VideoState == VideoState.CLOSED)
            {
                return;
            }          

            VideoState = VideoState.CLOSED;  

            videoDecoder.FrameQueue.stop();

            demuxPacketsCancellationTokenSource.Cancel();

            demuxPacketsTask.Wait();
           
            videoDecoder.close();
            audioPlayer.flush();

            videoPts = 0;
            videoPtsDrift = 0;

            seekRequest = false;
            seekPosition = 0;

            DurationSeconds = 0;
            PositionSeconds = 0;

            videoRender.display(null, Color.Black, VideoRender.RenderMode.CLEAR_SCREEN);

            if (VideoClosed != null)
            {

                VideoClosed(this, EventArgs.Empty);
            }
           
        }

        public void resize()
        {
            videoRender.resize();           
        }

        public void toggleFullScreen()
        {
            if (videoRender.Windowed == true)
            {
                videoRender.setFullScreen();
            }
            else
            {
                videoRender.setWindowed();
            }
        }
    }

}
