using MediaViewer.Timers;
using MediaViewer.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VideoLib;

namespace MediaViewer.VideoPanel
{
    class VideoPanelViewModel
    {
        System.ComponentModel.BackgroundWorker videoDecoderBW;

	    static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		VideoPlayer videoDecoder;
		VideoRender videoRender;
		StreamingAudioBuffer audioPlayer;

		VideoDebugForm videoDebug;

		// no AV sync correction is done if below the AV sync threshold 
		static const double AV_SYNC_THRESHOLD = 0.01;
		// no AV sync correction is done if too big error 
		static const double AV_NOSYNC_THRESHOLD = 10.0;

		static const double AUDIO_SAMPLE_CORRECTION_PERCENT_MAX = 10;

		// we use about AUDIO_DIFF_AVG_NB A-V differences to make the average 
		static const int AUDIO_DIFF_AVG_NB = 5;//20;

		enum SyncMode {

			AUDIO_SYNCS_TO_VIDEO,
			VIDEO_SYNCS_TO_AUDIO

		};

        SyncMode syncMode;

		VideoState videoState;

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

		bool updateTimeTrackBar;
		CustomToolTip timeTrackBarToolTip;

		delegate void UpdateUIDelegate(double, double, int);

		Control VideoPanel {

			get {

				return(splitContainer.Panel1);
			}
		}

        public VideoPanelViewModel()
		{
			
			//
			//TODO: Add the constructor code here
			//
			//videoRender = null;
			//mediaPlayer.Dock = DockStyle.Fill;
			//mediaPlayer.stretchToFit = true;


			videoRender = new VideoRender(VideoPanel);
			audioPlayer = new StreamingAudioBuffer(this);

			//videoRender.initialize(0,0);
			videoDecoder = new VideoPlayer();	
			videoDecoder.FrameQueue.Closed += new EventHandler(frameQueue_Closed);

			videoDecoder.setLogCallback(new VideoPlayer.LogCallbackDelegate(videoDecoderLogCallback),
				true, true);
			
			videoRefreshTimer = HRTimerFactory.create(HRTimerFactory.TimerType.TIMER_QUEUE);
			videoRefreshTimer.Tick += new EventHandler(videoRefreshTimer_Tick);
			//videoRefreshTimer.SynchronizingObject = this;
			videoRefreshTimer.AutoReset = false;

			audioRefreshTimer = HRTimerFactory.create(HRTimerFactory.TimerType.TIMER_QUEUE);
			audioRefreshTimer.Tick += new EventHandler(audioRefreshTimer_Tick);
			audioRefreshTimer.AutoReset = false;
			audioRefreshTimer.SynchronizingObject = null;

			videoDebug = new VideoDebugForm();

			audioDiffAvgCoef  = Math.Exp(Math.Log(0.01) / AUDIO_DIFF_AVG_NB);

			//syncMode = SyncMode.VIDEO_SYNCS_TO_AUDIO;
			syncMode = SyncMode.AUDIO_SYNCS_TO_VIDEO;
			VideoState = VideoState.CLOSED;

			updateTimeTrackBar = true;

			timeTrackBarToolTip = new CustomToolTip();
			timeTrackBarToolTip.BackColor = SystemColors.Info;
			this.Controls.Add(timeTrackBarToolTip);

			timeTrackBarToolTip.Show();
			timeTrackBarToolTip.BringToFront();
			timeTrackBarToolTip.Visible = false;			
			
			muteCheckBox.Checked = bool.Parse(Settings.getVar(Settings.VarName.VIDEO_MUTED));
			volumeTrackBar.Value = Util.lerp<int>(Double.Parse(Settings.getVar(Settings.VarName.VIDEO_VOLUME)), volumeTrackBar.Minimum, volumeTrackBar.Maximum);

		}


		~VideoPanelViewModel()
		{
			if (components)
			{
				delete components;
			}
		}
	

		

		void invokeUpdateUI() {

			int curTime = (int)Math.Floor(getVideoClock());
			int totalTime = videoDecoder.DurationSeconds;

			videoTimeLabel.Text = Util.formatTimeSeconds(curTime) + "/" + Util.formatTimeSeconds(totalTime);

			if(updateTimeTrackBar == true) {

				double pos = Util.invlerp<int>(curTime,0,totalTime);
				timeTrackBar.Value = Util.lerp<int>(pos, timeTrackBar.Minimum, timeTrackBar.Maximum);
			}
		}

		void updateUI() {

			if(this.InvokeRequired) {

				// do not block (important!)
				this.BeginInvoke(new Action(this, &VideoPanelControl.invokeUpdateUI));

			} else {
		
				invokeUpdateUI();
			}
		}

		double getVideoClock() {

			if(VideoState == VideoState.PAUSED) {

				return(videoPts);

			} else {

				return(videoPtsDrift - HRTimer.getTimestamp());
			}
		}

		void processVideoFrame() {

			bool skipVideoFrame = false;

restartvideo:
			
			double actualDelay = 0.04;

			Rectangle scaledVideoRec = ImageUtils.stretchRectangle(
				new Rectangle(0,0,videoDecoder.Width, videoDecoder.Height),
				videoRender.Canvas);

			Rectangle canvas = ImageUtils.centerRectangle(videoRender.Canvas,
				scaledVideoRec);

			// grab a decoded frame, returns false if the queue is stopped
			VideoFrame videoFrame = videoDecoder.FrameQueue.getDecodedVideoFrame();

			if(VideoState == VideoState.CLOSED && videoFrame == null) {

				return;

			} else if(VideoState == VideoState.PLAYING) {

				videoPts = videoFrame.Pts;
				videoPtsDrift = videoFrame.Pts + HRTimer.getTimestamp();

				if(skipVideoFrame == false) {

					videoRender.display(videoFrame, canvas, Color.Black, VideoRender.RenderMode.NORMAL);
					videoDebug.VideoFrames = videoDebug.VideoFrames + 1;
				} 					

				actualDelay = synchronizeVideo(videoPts);					

			} else if(VideoState == VideoState.PAUSED) {

				videoRender.display(null, canvas, Color.Black, VideoRender.RenderMode.PAUSED);			
			}

			// do not update ui elements on main thread inside videoStateLock
			// or we can get a deadlock
			videoDebug.update();
			updateUI();

			if(actualDelay < 0.010) {

				// delay is too small skip next frame
				skipVideoFrame = true;
				videoDebug.NrVideoFramesDropped = videoDebug.NrVideoFramesDropped + 1;
				goto restartvideo;

			} 

			// start timer with delay for next frame
			videoRefreshTimer.Interval = (int)(actualDelay * 1000 + 0.5);
			videoRefreshTimer.start();		

		}


		double synchronizeVideo(double videoPts) {

			// assume delay to next frame equals delay between previous frames
			double delay = videoPts - previousVideoPts;	

			if(delay <= 0 || delay >= 1.0) {
				// if incorrect delay, use previous one 
				delay = previousVideoDelay;
			}

			previousVideoPts = videoPts;
			previousVideoDelay = delay;

			if(videoDecoder.HasAudio && syncMode == SyncMode.VIDEO_SYNCS_TO_AUDIO) {

				// synchronize video to audio
				double diff = getVideoClock() - audioPlayer.getAudioClock();

				// Skip or repeat the frame. Take delay into account
				// FFPlay still doesn't "know if this is the best guess."
				double sync_threshold = (delay > AV_SYNC_THRESHOLD) ? delay : AV_SYNC_THRESHOLD;

				if(Math.Abs(diff) < AV_NOSYNC_THRESHOLD) {

					if(diff <= -sync_threshold) {

						delay = 0;

					} else if(diff >= sync_threshold) {

						delay = 2 * delay;
					}
				}

			}

			// adjust delay based on the actual current time
			videoFrameTimer += delay;
			double actualDelay = videoFrameTimer - HRTimer.getTimestamp();

			videoDebug.VideoDelay = delay;
			videoDebug.ActualVideoDelay = actualDelay;
			videoDebug.VideoSync = getVideoClock();
			videoDebug.AudioSync = audioPlayer.getAudioClock();
			videoDebug.VideoQueueSize = videoDecoder.FrameQueue.VideoPacketsInQueue;
			videoDebug.AudioQueueSize = videoDecoder.FrameQueue.AudioPacketsInQueue;
			

			return(actualDelay);
		}


		void processAudioFrame() {

restartaudio:
			
			AudioFrame audioFrame = videoDecoder.FrameQueue.getDecodedAudioFrame();
			if(audioFrame == null) return;

			videoDebug.AudioFrames = videoDebug.AudioFrames + 1;
			videoDebug.AudioFrameLength = audioFrame.Length;

			// if the audio is lagging behind too much, skip the buffer completely
			double diff = getVideoClock() - audioFrame.Pts;
			if(diff > 0.2 && diff < 3 && syncMode == SyncMode.AUDIO_SYNCS_TO_VIDEO) {

				log.Warn("dropping audio buffer, lagging behind: " + (getVideoClock() - audioFrame.Pts).ToString() + " seconds");
				goto restartaudio;
			}

			//adjustAudioSamplesPerSecond(audioFrame);
			adjustAudioLength(audioFrame);

			audioPlayer.write(audioFrame);

			int frameLength = audioFrame.Length;

			double actualDelay = synchronizeAudio(frameLength);

			if(actualDelay < 0) {

				// delay too small, play next frame as quickly as possible
				videoDebug.NrAudioFramesLaggingBehind = videoDebug.NrAudioFramesLaggingBehind + 1;
				goto restartaudio;

			} 
		
			// start timer with delay for next frame
			audioRefreshTimer.Interval = (int)(actualDelay * 1000 + 0.5);
			audioRefreshTimer.start();

		}

		double synchronizeAudio(int frameLength) {

			// calculate delay to play next frame
			int bytesPerSecond = audioPlayer.SamplesPerSecond * 
				videoDecoder.BytesPerSample * videoDecoder.NrChannels;

			double delay = frameLength / double(bytesPerSecond);

			// adjust delay based on the actual current time
			audioFrameTimer += delay;
			double actualDelay = audioFrameTimer - HRTimer.getTimestamp();

			videoDebug.AudioDelay = delay;
			videoDebug.ActualAudioDelay = actualDelay;			

			return(actualDelay);
		}


		void adjustAudioSamplesPerSecond(AudioFrame frame) {

			videoDebug.AudioFrameLengthAdjust = 0;

			if(syncMode == SyncMode.AUDIO_SYNCS_TO_VIDEO) {

				int n = videoDecoder.NrChannels * videoDecoder.BytesPerSample;

				double diff = audioPlayer.getAudioClock() - getVideoClock();

				if(Math.Abs(diff) < AV_NOSYNC_THRESHOLD) {

					// accumulate the diffs
					audioDiffCum = diff + audioDiffAvgCoef * audioDiffCum;

					if(audioDiffAvgCount < AUDIO_DIFF_AVG_NB) {

						audioDiffAvgCount++;

					} else {

						double avgDiff = audioDiffCum * (1.0 - audioDiffAvgCoef);

						// Shrinking/expanding buffer code....
						if(Math.Abs(avgDiff) >= audioDiffThreshold) {

							int wantedSize = (int)(frame.Length + diff * videoDecoder.SamplesPerSecond * n);
								
							// get a correction percent from 10 to 60 based on the avgDiff
							// in order to converge a little faster
							double correctionPercent = Misc.clamp(10 + (Math.Abs(avgDiff) - audioDiffThreshold) * 15, 10, 60);

							//Util.DebugOut(correctionPercent);

							//AUDIO_SAMPLE_CORRECTION_PERCENT_MAX

							int minSize = (int)(frame.Length * ((100 - correctionPercent)
								/ 100));

							int maxSize = (int)(frame.Length * ((100 + correctionPercent) 
								/ 100));

							if(wantedSize < minSize) {

								wantedSize = minSize;

							} else if(wantedSize > maxSize) {

								wantedSize = maxSize;
							}

							// adjust samples per second to speed up or slow down the audio
							Int64 length = frame.Length;
							Int64 sps = videoDecoder.SamplesPerSecond;
							int samplesPerSecond = (int)((length * sps) / wantedSize);
							videoDebug.AudioFrameLengthAdjust = samplesPerSecond;
							audioPlayer.SamplesPerSecond = samplesPerSecond;
							
						} else {

							audioPlayer.SamplesPerSecond = videoDecoder.SamplesPerSecond;
						}

					}

				} else {

					// difference is TOO big; reset diff stuff 
					audioDiffAvgCount = 0;
					audioDiffCum = 0;
				}
			}
			
		}


		void adjustAudioLength(AudioFrame frame) {

			videoDebug.AudioFrameLengthAdjust = 0;

			if(syncMode == SyncMode.AUDIO_SYNCS_TO_VIDEO) {

				int n = videoDecoder.NrChannels * videoDecoder.BytesPerSample;

				double diff = audioPlayer.getAudioClock() - getVideoClock();

				if(Math.Abs(diff) < AV_NOSYNC_THRESHOLD) {

					// accumulate the diffs
					audioDiffCum = diff + audioDiffAvgCoef * audioDiffCum;

					if(audioDiffAvgCount < AUDIO_DIFF_AVG_NB) {

						audioDiffAvgCount++;

					} else {

						double avgDiff = audioDiffCum * (1.0 - audioDiffAvgCoef);

						// Shrinking/expanding buffer code....
						if(Math.Abs(avgDiff) >= audioDiffThreshold) {

							int wantedSize = (int)(frame.Length + diff * videoDecoder.SamplesPerSecond * n);
								
							// get a correction percent from 10 to 60 based on the avgDiff
							// in order to converge a little faster
							double correctionPercent = Misc.clamp(10 + (Math.Abs(avgDiff) - audioDiffThreshold) * 15, 10, 60);

							//Util.DebugOut(correctionPercent);

							//AUDIO_SAMPLE_CORRECTION_PERCENT_MAX

							int minSize = (int)(frame.Length * ((100 - correctionPercent)
								/ 100));

							int maxSize = (int)(frame.Length * ((100 + correctionPercent) 
								/ 100));

							if(wantedSize < minSize) {

								wantedSize = minSize;

							} else if(wantedSize > maxSize) {

								wantedSize = maxSize;
							}

							// make sure the samples stay aligned after resizing the buffer
							wantedSize = (wantedSize / n) * n;

							if(wantedSize < frame.Length) {

								// remove samples 
								videoDebug.AudioFrameLengthAdjust = wantedSize - frame.Length;
								frame.Length = wantedSize;

							} else if(wantedSize > frame.Length) {
														
								// add samples by copying final samples
								int nrExtraSamples = wantedSize - frame.Length;
								videoDebug.AudioFrameLengthAdjust = nrExtraSamples;
						
								byte[] lastSample = new byte[n];

								for(int i = 0; i < n; i++) {

									lastSample[i] = frame.Data[frame.Length - n + i];
								}

								frame.Stream.Position = frame.Length;

								while(nrExtraSamples > 0) {
									
									frame.Stream.Write(lastSample, 0, n);
									nrExtraSamples -= n;
								}

								frame.Stream.Position = 0;
								frame.Length = wantedSize;
							}

						}

					}

				} else {

					// difference is TOO big; reset diff stuff 
					audioDiffAvgCount = 0;
					audioDiffCum = 0;
				}
			}
			
		}

		void pausePlay() {

			if(VideoState == VideoState.PAUSED || 
				VideoState == VideoState.CLOSED) {
				
					return;
			}

			VideoState = VideoState.PAUSED;		

			videoDecoder.FrameQueue.stop();

			videoDecoderBW.CancelAsync();
	
			while(videoDecoderBW.IsBusy) {

				Application.DoEvents();
			}

			audioPlayer.stop();

		}

		void startPlay() {

			if(VideoState == VideoState.PLAYING || 
				VideoState == VideoState.CLOSED) {
					
				return;
			}

			VideoState = VideoState.PLAYING;

			audioPlayer.startPlayAfterNextWrite();

			videoDecoder.FrameQueue.start();
			
			videoDecoderBW.RunWorkerAsync();

			previousVideoPts = 0;
			previousVideoDelay = 0.04;

			audioDiffAvgCount = 0;

			videoRefreshTimer.start();
			audioRefreshTimer.start();
			
		}

		void invokeStopButtonClick() {

			stopButton.PerformClick();
		}

		void frameQueue_Closed(Object sender, EventArgs ) {

			log.Info("Video stream end reached");
			this.BeginInvoke(new Action(invokeStopButtonClick));
		}

		void fillFrameQueue() {

			bool success = true;

			while(videoDecoder.FrameQueue.AudioPacketsInQueue !=
				videoDecoder.FrameQueue.MaxAudioPackets &&
				videoDecoder.FrameQueue.VideoPacketsInQueue != 
				videoDecoder.FrameQueue.MaxVideoPackets &&
				success == true)
			{

				success = videoDecoder.demuxPacket();

			} 
			
		}

		void seek(double seconds) {

			seekPosition = seconds;
			seekRequest = true;			
		}

		void videoDecoderLogCallback(int level, string message) {

			if(level < 16) {

				log.Fatal(message);

			} else if(level == 16) {

				log.Error(message);

			} else if(level == 24) {

				log.Warn(message);

			} else {

				log.Info(message);
			}
		}

		VideoState VideoState {
		 
			set {

				videoState = value;
			}
	
			public get {

				return(videoState);
			}
		}

		string VideoLocation
		{

			get {

				return(videoDecoder.VideoLocation);
			}
		}

		void open(string location) {

			try {

				log.Info("Opening: " + location);

				close();
				videoDebug.clear();			

				videoDecoder.open(location);
				videoRender.initialize(videoDecoder.Width, videoDecoder.Height);

				if(videoDecoder.HasAudio) {

					audioPlayer.initialize(videoDecoder.SamplesPerSecond, videoDecoder.BytesPerSample,
						videoDecoder.NrChannels, videoDecoder.MaxAudioFrameSize * 2);			

					muteCheckBox.Enabled = true;
					volumeTrackBar.Enabled = true;

					audioDiffThreshold = 2.0 * 1024 / videoDecoder.SamplesPerSecond;

				} else {

					muteCheckBox.Enabled = false;
					volumeTrackBar.Enabled = false;
				}

				videoDebug.VideoQueueMaxSize = videoDecoder.FrameQueue.MaxVideoPackets;
				//videoDebug.VideoQueueSizeBytes = videoDecoder.FrameQueue.VideoQueueSizeBytes;	
				videoDebug.AudioQueueMaxSize = videoDecoder.FrameQueue.MaxAudioPackets;
				//videoDebug.AudioQueueSizeBytes = videoDecoder.FrameQueue.AudioQueueSizeBytes;

				if(syncMode == SyncMode.AUDIO_SYNCS_TO_VIDEO) {

					videoDebug.IsVideoSyncMaster = true;

				} else {

					videoDebug.IsAudioSyncMaster = true;
				}

				VideoState = VideoState.OPEN;

			} catch (VideoLibException e) {

				VideoState = VideoState.CLOSED;

				log.Error("Cannot open: " + location, e);
			
				MessageBox.Show("Cannot open: " + location + "\n\n" + 
					e.Message, "Video Error");

			}
		}

		void play() {

			if(playCheckBox.Checked == true && VideoState 
				!= VideoState.PLAYING) {

				startPlay();

			} else {

				playCheckBox.Checked = true;
			}
		}

		void pause() {

			playCheckBox.Checked = false;
		}
	
		void close() {

			if(VideoState == VideoState.CLOSED) {

				return;
			}

			VideoState = VideoState.CLOSED;

			videoDecoder.FrameQueue.stop();			

			videoDecoderBW.CancelAsync();
	
			while(videoDecoderBW.IsBusy) {

				Application.DoEvents();
			}
			
			videoDecoder.close();
			audioPlayer.flush();

			videoPts = 0;
			videoPtsDrift = 0;

			seekRequest = false;
			seekPosition = 0;

		}
	

 void videoDecoderBW_DoWork(System.Object  sender, System.ComponentModel.DoWorkEventArgs  e) {
			 			
				audioFrameTimer = videoFrameTimer = HRTimer.getTimestamp();

				bool success = true;

				// decode frames one by one, or handle seek requests
				do
				{
					if(seekRequest == true) {

						// wait for video and audio decoding to pause/block
						// To make sure no packets are in limbo
						// before flushing any ffmpeg internal or external queues. 
						videoDecoder.FrameQueue.pause();

						if(videoDecoder.seek(seekPosition) == true) {
							
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

					} else {

						success = videoDecoder.demuxPacket();
					}
									
				} while(success == true && !videoDecoderBW.CancellationPending);
				
		 }
 void videoRefreshTimer_Tick(System.Object  sender, System.EventArgs  e) {			
 
	
			 processVideoFrame();

		 }

 void audioRefreshTimer_Tick(Object  sender, EventArgs e) {

			 processAudioFrame();
		 }

 void stopButton_Click(System.Object  sender, System.EventArgs  e) {

			 close();
			 videoRender.display(null, Rectangle.Empty, this.BackColor, VideoRender.RenderMode.CLEAR_SCREEN);
			 timeTrackBar.Value = timeTrackBar.Minimum;
			 playCheckBox.Checked = false;
		 }

 void debugVideoCheckBox_CheckedChanged(System.Object  sender, System.EventArgs  e) {

			 if(debugVideoCheckBox.Checked == true) {

				videoDebug.Show();

			 } else {

				videoDebug.Hide();
			 }
		 }
 void volumeTrackBar_ValueChanged(System.Object  sender, System.EventArgs  e) {

			 double volume = Util.invlerp<int>(volumeTrackBar.Value,volumeTrackBar.Minimum, volumeTrackBar.Maximum);
			 audioPlayer.Volume = volume;
			 Settings.setVar(Settings.VarName.VIDEO_VOLUME, volume);
		 }
 void muteCheckBox_CheckedChanged(System.Object  sender, System.EventArgs  e) {

			 if(muteCheckBox.Checked == true) {

				 audioPlayer.Muted = true;
				 muteCheckBox.ImageIndex = 1;

			 } else {

				 audioPlayer.Muted = false;
				 muteCheckBox.ImageIndex = 0;

			 }

			 Settings.setVar(Settings.VarName.VIDEO_MUTED, muteCheckBox.Checked);
		 }
 void playCheckBox_CheckedChanged(System.Object  sender, System.EventArgs  e) {

			 if(playCheckBox.Checked == true) {

				 if(VideoState == VideoState.CLOSED && !string.IsNullOrEmpty(videoDecoder.VideoLocation)) {

					 open(videoDecoder.VideoLocation);					 
				 }

				 startPlay();
				 playCheckBox.ImageIndex = 3;
				 updateTimeTrackBar = true;

			 } else {

				 pausePlay();
				 playCheckBox.ImageIndex = 2;

			 }
		 }

 void timeTrackBar_MouseDown(System.Object  sender, System.Windows.Forms.MouseEventArgs  e) {

			 updateTimeTrackBar = false;

			 Rectangle chanRec = WindowsUtils.getTrackBarChannelRect(timeTrackBar);

			 double value = (int)Misc.invlerp(e.X, chanRec.Left, chanRec.Right);

			 timeTrackBar.Value = (int)Misc.lerp(value, timeTrackBar.Minimum, timeTrackBar.Maximum);
			
		 }
 void timeTrackBar_MouseUp(System.Object  sender, System.Windows.Forms.MouseEventArgs  e) {

			 int totalTime = videoDecoder.DurationSeconds;

			 Rectangle chanRec = WindowsUtils.getTrackBarChannelRect(timeTrackBar);

			 double value = (int)Misc.invlerp(e.X, chanRec.Left, chanRec.Right);

			 int seconds = (int)Misc.lerp(value, 0, totalTime);

			 seek(seconds);

			 if(VideoState != VideoState.PAUSED) {

				updateTimeTrackBar = true;
			 }
		
		 }

 void nextButton_Click(System.Object  sender, System.EventArgs  e) {
		 }
 void prevButton_Click(System.Object  sender, System.EventArgs  e) {
		 }


 string timeTrackBarPosToTime(int mouseX) {

			 Rectangle chanRec = WindowsUtils.getTrackBarChannelRect(timeTrackBar);

			 double value = (int)Misc.invlerp(mouseX, chanRec.Left, chanRec.Right);

			 int seconds = (int)Misc.lerp(value, 0, videoDecoder.DurationSeconds);

			 return(Misc.formatTimeSeconds(seconds));

		 }

 void timeTrackBar_MouseEnter(System.Object  sender, System.EventArgs  e) {
			 
			 if(VideoState == VideoState.CLOSED) return;

			 Point trackBarPos = this.PointToClient(timeTrackBar.PointToScreen(timeTrackBar.Location));

			 timeTrackBarToolTip.Location = Point(MousePosition.X - timeTrackBarToolTip.Width / 2, 
				 trackBarPos.Y - timeTrackBarToolTip.Height);
			 timeTrackBarToolTip.Text = timeTrackBarPosToTime(MousePosition.X);

			 timeTrackBarToolTip.Refresh();

			 timeTrackBarToolTip.Visible = true;
		 }

 void timeTrackBar_MouseLeave(System.Object  sender, System.EventArgs  e) {

			 timeTrackBarToolTip.Visible = false;
		 }
 void timeTrackBar_MouseMove(System.Object  sender, System.Windows.Forms.MouseEventArgs  e) {

			 Point trackBarPos = this.PointToClient(timeTrackBar.PointToScreen(timeTrackBar.Location));

			 timeTrackBarToolTip.Location = Point(e.X - timeTrackBarToolTip.Width / 2, 
				 trackBarPos.Y - timeTrackBarToolTip.Height);
			 timeTrackBarToolTip.Text = timeTrackBarPosToTime(e.X);

			 timeTrackBarToolTip.Refresh();
		 }
 void screenShotButton_Click(System.Object  sender, System.EventArgs  e) {

			 if(VideoState == VideoState.CLOSED) return;

			 //videoRender.setFullScreen();
			 videoRender.createScreenShot(videoDecoder.VideoLocation);
		 }

    }
}
