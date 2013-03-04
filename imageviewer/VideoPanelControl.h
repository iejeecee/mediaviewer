#pragma once
// Directx9 tutorial: http://www.drunkenhyena.com/cgi-bin/dx9_net.pl
// Implementing videoRender in directx: http://www.codeproject.com/Articles/207642/Video-Shadering-with-Direct3D
#include "ImageUtils.h"
#include "Util.h"
#include "WindowsUtils.h"
#include "HRTimerFactory.h"
#include "StreamingAudioBuffer.h"
#include "VideoRender.h"
#include "VideoDebugForm.h"
#include <stdio.h>
#include <string.h>

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;
using namespace Microsoft::DirectX;
using namespace VideoLib;
namespace D3D = Microsoft::DirectX::Direct3D;
namespace DS = Microsoft::DirectX::DirectSound;
using namespace Diagnostics;

namespace imageviewer {

	/// <summary>
	/// Summary for VideoPanelControl
	/// </summary>
	public ref class VideoPanelControl : public System::Windows::Forms::UserControl
	{
	public:
		VideoPanelControl(void)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
			//videoRender = nullptr;
			//mediaPlayer->Dock = DockStyle::Fill;
			//mediaPlayer->stretchToFit = true;

			videoRender = gcnew VideoRender(VideoPanel);
			audioPlayer = gcnew StreamingAudioBuffer(this);

			volumeTrackBar->Value = Util::lerp<int>(audioPlayer->Volume, volumeTrackBar->Minimum, volumeTrackBar->Maximum);

			//videoRender->initialize(0,0);
			videoDecoder = gcnew VideoPlayer(nullptr);	
			
			videoRefreshTimer = HRTimerFactory::create(HRTimerFactory::TimerType::TIMER_QUEUE);
			videoRefreshTimer->Tick += gcnew EventHandler(this, &VideoPanelControl::videoRefreshTimer_Tick);
			videoRefreshTimer->SynchronizingObject = this;
			videoRefreshTimer->AutoReset = false;

			audioRefreshTimer = HRTimerFactory::create(HRTimerFactory::TimerType::TIMER_QUEUE);
			audioRefreshTimer->Tick += gcnew EventHandler(this, &VideoPanelControl::audioRefreshTimer_Tick);
			audioRefreshTimer->AutoReset = false;
			audioRefreshTimer->SynchronizingObject = nullptr;

			videoDebug = gcnew VideoDebugForm();

			audioDiffAvgCoef  = Math::Exp(Math::Log(0.01) / AUDIO_DIFF_AVG_NB);

			syncMode = SyncMode::VIDEO_SYNCS_TO_AUDIO;
			
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~VideoPanelControl()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::ComponentModel::BackgroundWorker^  videoDecoderBW;
	protected: 


	private: System::Windows::Forms::SplitContainer^  splitContainer;
	private: System::Windows::Forms::TrackBar^  timeTrackBar;
	private: System::Windows::Forms::Button^  stopButton;

	private: System::Windows::Forms::CheckBox^  debugVideoCheckBox;
	private: System::Windows::Forms::TrackBar^  volumeTrackBar;
	private: System::Windows::Forms::CheckBox^  muteCheckBox;
	private: System::Windows::Forms::ToolTip^  toolTip1;
	private: System::Windows::Forms::ImageList^  imageList;
	private: System::Windows::Forms::Label^  videoTimeLabel;
	private: System::Windows::Forms::CheckBox^  playCheckBox;
	private: System::Windows::Forms::Button^  forwardButton;
























	private: System::ComponentModel::IContainer^  components;
	protected: 

	protected: 

	protected: 

	protected: 

	private:
		/// <summary>
		/// Required designer variable.
		/// </summary>


#pragma region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent(void)
		{
			this->components = (gcnew System::ComponentModel::Container());
			System::ComponentModel::ComponentResourceManager^  resources = (gcnew System::ComponentModel::ComponentResourceManager(VideoPanelControl::typeid));
			this->videoDecoderBW = (gcnew System::ComponentModel::BackgroundWorker());
			this->splitContainer = (gcnew System::Windows::Forms::SplitContainer());
			this->forwardButton = (gcnew System::Windows::Forms::Button());
			this->playCheckBox = (gcnew System::Windows::Forms::CheckBox());
			this->imageList = (gcnew System::Windows::Forms::ImageList(this->components));
			this->videoTimeLabel = (gcnew System::Windows::Forms::Label());
			this->muteCheckBox = (gcnew System::Windows::Forms::CheckBox());
			this->volumeTrackBar = (gcnew System::Windows::Forms::TrackBar());
			this->debugVideoCheckBox = (gcnew System::Windows::Forms::CheckBox());
			this->stopButton = (gcnew System::Windows::Forms::Button());
			this->timeTrackBar = (gcnew System::Windows::Forms::TrackBar());
			this->toolTip1 = (gcnew System::Windows::Forms::ToolTip(this->components));
			this->splitContainer->Panel2->SuspendLayout();
			this->splitContainer->SuspendLayout();
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->volumeTrackBar))->BeginInit();
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->timeTrackBar))->BeginInit();
			this->SuspendLayout();
			// 
			// videoDecoderBW
			// 
			this->videoDecoderBW->WorkerReportsProgress = true;
			this->videoDecoderBW->WorkerSupportsCancellation = true;
			this->videoDecoderBW->DoWork += gcnew System::ComponentModel::DoWorkEventHandler(this, &VideoPanelControl::videoDecoderBW_DoWork);
			// 
			// splitContainer
			// 
			this->splitContainer->BorderStyle = System::Windows::Forms::BorderStyle::Fixed3D;
			this->splitContainer->Dock = System::Windows::Forms::DockStyle::Fill;
			this->splitContainer->FixedPanel = System::Windows::Forms::FixedPanel::Panel2;
			this->splitContainer->IsSplitterFixed = true;
			this->splitContainer->Location = System::Drawing::Point(0, 0);
			this->splitContainer->Name = L"splitContainer";
			this->splitContainer->Orientation = System::Windows::Forms::Orientation::Horizontal;
			// 
			// splitContainer.Panel2
			// 
			this->splitContainer->Panel2->Controls->Add(this->forwardButton);
			this->splitContainer->Panel2->Controls->Add(this->playCheckBox);
			this->splitContainer->Panel2->Controls->Add(this->videoTimeLabel);
			this->splitContainer->Panel2->Controls->Add(this->muteCheckBox);
			this->splitContainer->Panel2->Controls->Add(this->volumeTrackBar);
			this->splitContainer->Panel2->Controls->Add(this->debugVideoCheckBox);
			this->splitContainer->Panel2->Controls->Add(this->stopButton);
			this->splitContainer->Panel2->Controls->Add(this->timeTrackBar);
			this->splitContainer->Size = System::Drawing::Size(800, 484);
			this->splitContainer->SplitterDistance = 388;
			this->splitContainer->TabIndex = 0;
			// 
			// forwardButton
			// 
			this->forwardButton->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"forwardButton.Image")));
			this->forwardButton->Location = System::Drawing::Point(117, 37);
			this->forwardButton->MaximumSize = System::Drawing::Size(60, 37);
			this->forwardButton->Name = L"forwardButton";
			this->forwardButton->Size = System::Drawing::Size(51, 36);
			this->forwardButton->TabIndex = 9;
			this->toolTip1->SetToolTip(this->forwardButton, L"Jump Forward");
			this->forwardButton->UseVisualStyleBackColor = true;
			this->forwardButton->Click += gcnew System::EventHandler(this, &VideoPanelControl::forwardButton_Click);
			// 
			// playCheckBox
			// 
			this->playCheckBox->Appearance = System::Windows::Forms::Appearance::Button;
			this->playCheckBox->ImageIndex = 3;
			this->playCheckBox->ImageList = this->imageList;
			this->playCheckBox->Location = System::Drawing::Point(3, 37);
			this->playCheckBox->Margin = System::Windows::Forms::Padding(0, 3, 3, 3);
			this->playCheckBox->MaximumSize = System::Drawing::Size(51, 36);
			this->playCheckBox->Name = L"playCheckBox";
			this->playCheckBox->Size = System::Drawing::Size(51, 36);
			this->playCheckBox->TabIndex = 8;
			this->playCheckBox->TextAlign = System::Drawing::ContentAlignment::MiddleCenter;
			this->toolTip1->SetToolTip(this->playCheckBox, L"Play / Pause");
			this->playCheckBox->UseVisualStyleBackColor = true;
			this->playCheckBox->CheckedChanged += gcnew System::EventHandler(this, &VideoPanelControl::playCheckBox_CheckedChanged);
			// 
			// imageList
			// 
			this->imageList->ImageStream = (cli::safe_cast<System::Windows::Forms::ImageListStreamer^  >(resources->GetObject(L"imageList.ImageStream")));
			this->imageList->TransparentColor = System::Drawing::Color::Transparent;
			this->imageList->Images->SetKeyName(0, L"1361908321_1777.ico");
			this->imageList->Images->SetKeyName(1, L"1361908340_1776.ico");
			this->imageList->Images->SetKeyName(2, L"1361914813_22929.ico");
			this->imageList->Images->SetKeyName(3, L"1361914794_22964.ico");
			this->imageList->Images->SetKeyName(4, L"1361914838_22942.ico");
			// 
			// videoTimeLabel
			// 
			this->videoTimeLabel->AutoSize = true;
			this->videoTimeLabel->Dock = System::Windows::Forms::DockStyle::Right;
			this->videoTimeLabel->Location = System::Drawing::Point(515, 37);
			this->videoTimeLabel->MaximumSize = System::Drawing::Size(137, 36);
			this->videoTimeLabel->MinimumSize = System::Drawing::Size(0, 36);
			this->videoTimeLabel->Name = L"videoTimeLabel";
			this->videoTimeLabel->Size = System::Drawing::Size(137, 36);
			this->videoTimeLabel->TabIndex = 7;
			this->videoTimeLabel->Text = L"00:00:00/00:00:00";
			this->videoTimeLabel->TextAlign = System::Drawing::ContentAlignment::MiddleRight;
			// 
			// muteCheckBox
			// 
			this->muteCheckBox->Appearance = System::Windows::Forms::Appearance::Button;
			this->muteCheckBox->Dock = System::Windows::Forms::DockStyle::Right;
			this->muteCheckBox->ImageIndex = 0;
			this->muteCheckBox->ImageList = this->imageList;
			this->muteCheckBox->Location = System::Drawing::Point(652, 37);
			this->muteCheckBox->MaximumSize = System::Drawing::Size(40, 36);
			this->muteCheckBox->Name = L"muteCheckBox";
			this->muteCheckBox->Size = System::Drawing::Size(40, 36);
			this->muteCheckBox->TabIndex = 6;
			this->muteCheckBox->TextAlign = System::Drawing::ContentAlignment::MiddleCenter;
			this->toolTip1->SetToolTip(this->muteCheckBox, L"Mute");
			this->muteCheckBox->UseVisualStyleBackColor = true;
			this->muteCheckBox->CheckedChanged += gcnew System::EventHandler(this, &VideoPanelControl::muteCheckBox_CheckedChanged);
			// 
			// volumeTrackBar
			// 
			this->volumeTrackBar->AutoSize = false;
			this->volumeTrackBar->Dock = System::Windows::Forms::DockStyle::Right;
			this->volumeTrackBar->Location = System::Drawing::Point(692, 37);
			this->volumeTrackBar->Maximum = 100;
			this->volumeTrackBar->MaximumSize = System::Drawing::Size(104, 36);
			this->volumeTrackBar->Name = L"volumeTrackBar";
			this->volumeTrackBar->Size = System::Drawing::Size(104, 36);
			this->volumeTrackBar->TabIndex = 5;
			this->volumeTrackBar->TickStyle = System::Windows::Forms::TickStyle::None;
			this->toolTip1->SetToolTip(this->volumeTrackBar, L"Volume");
			this->volumeTrackBar->ValueChanged += gcnew System::EventHandler(this, &VideoPanelControl::volumeTrackBar_ValueChanged);
			// 
			// debugVideoCheckBox
			// 
			this->debugVideoCheckBox->Appearance = System::Windows::Forms::Appearance::Button;
			this->debugVideoCheckBox->Location = System::Drawing::Point(216, 37);
			this->debugVideoCheckBox->Name = L"debugVideoCheckBox";
			this->debugVideoCheckBox->Size = System::Drawing::Size(66, 36);
			this->debugVideoCheckBox->TabIndex = 4;
			this->debugVideoCheckBox->Text = L"Debug";
			this->debugVideoCheckBox->TextAlign = System::Drawing::ContentAlignment::MiddleCenter;
			this->debugVideoCheckBox->UseVisualStyleBackColor = true;
			this->debugVideoCheckBox->CheckedChanged += gcnew System::EventHandler(this, &VideoPanelControl::debugVideoCheckBox_CheckedChanged);
			// 
			// stopButton
			// 
			this->stopButton->ImageIndex = 4;
			this->stopButton->ImageList = this->imageList;
			this->stopButton->Location = System::Drawing::Point(60, 37);
			this->stopButton->MaximumSize = System::Drawing::Size(60, 37);
			this->stopButton->Name = L"stopButton";
			this->stopButton->Size = System::Drawing::Size(51, 36);
			this->stopButton->TabIndex = 2;
			this->toolTip1->SetToolTip(this->stopButton, L"Stop");
			this->stopButton->UseVisualStyleBackColor = true;
			this->stopButton->Click += gcnew System::EventHandler(this, &VideoPanelControl::stopButton_Click);
			// 
			// timeTrackBar
			// 
			this->timeTrackBar->AutoSize = false;
			this->timeTrackBar->Dock = System::Windows::Forms::DockStyle::Top;
			this->timeTrackBar->Location = System::Drawing::Point(0, 0);
			this->timeTrackBar->Maximum = 5000;
			this->timeTrackBar->Name = L"timeTrackBar";
			this->timeTrackBar->Size = System::Drawing::Size(796, 37);
			this->timeTrackBar->TabIndex = 0;
			this->timeTrackBar->TickStyle = System::Windows::Forms::TickStyle::None;
			this->timeTrackBar->MouseDown += gcnew System::Windows::Forms::MouseEventHandler(this, &VideoPanelControl::timeTrackBar_MouseDown);
			// 
			// VideoPanelControl
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->Controls->Add(this->splitContainer);
			this->DoubleBuffered = true;
			this->Name = L"VideoPanelControl";
			this->Size = System::Drawing::Size(800, 484);
			this->splitContainer->Panel2->ResumeLayout(false);
			this->splitContainer->Panel2->PerformLayout();
			this->splitContainer->ResumeLayout(false);
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->volumeTrackBar))->EndInit();
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->timeTrackBar))->EndInit();
			this->ResumeLayout(false);

		}
#pragma endregion
	private:

		VideoPlayer ^videoDecoder;
		VideoRender ^videoRender;
		StreamingAudioBuffer ^audioPlayer;

		VideoDebugForm ^videoDebug;

		// no AV sync correction is done if below the AV sync threshold 
		static const double AV_SYNC_THRESHOLD = 0.01;
		// no AV sync correction is done if too big error 
		static const double AV_NOSYNC_THRESHOLD = 10.0;

		static const double AUDIO_SAMPLE_CORRECTION_PERCENT_MAX = 10;

		// we use about AUDIO_DIFF_AVG_NB A-V differences to make the average 
		static const int AUDIO_DIFF_AVG_NB = 20;

		enum class SyncMode {

			AUDIO_SYNCS_TO_VIDEO,
			VIDEO_SYNCS_TO_AUDIO

		} syncMode;

		double previousVideoPts;
		double previousVideoDelay;

		double videoFrameTimer;
		double audioFrameTimer;

		HRTimer ^videoRefreshTimer;		
		HRTimer ^audioRefreshTimer;

		double videoPtsDrift;
		double audioPtsDrift;

		double audioDiffCum;
		double audioDiffAvgCoef;
		double audioDiffThreshold;
		int audioDiffAvgCount;

		bool seekRequest;
		double seekPosition;

		delegate void UpdateUIDelegate(double, double, int);

		property Control ^VideoPanel {

			Control ^get() {

				return(splitContainer->Panel1);
			}
		}

		void invokeUpdateUI() {

			int curTime = (int)Math::Floor(getVideoClock());
			int totalTime = videoDecoder->DurationSeconds;

			videoTimeLabel->Text = Util::formatTimeSeconds(curTime) + "/" + Util::formatTimeSeconds(totalTime);

			double pos = Util::invlerp<int>(curTime,0,totalTime);
			timeTrackBar->Value = Util::lerp<int>(pos, timeTrackBar->Minimum, timeTrackBar->Maximum);
		}

		void updateUI() {

			if(this->InvokeRequired) {

				this->Invoke(gcnew Action(this, &VideoPanelControl::invokeUpdateUI));

			} else {
		
				invokeUpdateUI();
			}
		}

		double getVideoClock() {

			return(videoPtsDrift - HRTimer::getTimestamp());
		}

		void processVideoFrame() {

			bool skipVideoFrame = false;

restartvideo:

			int videoWidth = videoDecoder->Width;
			int videoHeight = videoDecoder->Height;
			Rectangle videoCanvasRec = videoRender->Canvas;
			
			int scaledWidth, scaledHeight;

			ImageUtils::stretchRectangle(videoWidth, videoHeight,
				videoCanvasRec.Width, videoCanvasRec.Height, scaledWidth, scaledHeight);

			Rectangle scaledVideoRec(0,0, scaledWidth, scaledHeight);
			
			Rectangle canvas = ImageUtils::centerRectangle(videoCanvasRec,
				scaledVideoRec);

			VideoFrame ^videoFrame = nullptr;
			
			// grab a decoded frame, returns false if the queue is stopped
			bool success = videoDecoder->FrameQueue->getDecodedVideoFrame(videoFrame);
			if(success == false) return;

			videoPtsDrift = videoFrame->Pts + HRTimer::getTimestamp();

			if(skipVideoFrame == false) {

				videoRender->display(videoFrame, canvas, Color::Black);
				videoDebug->VideoFrames = videoDebug->VideoFrames + 1;
			} 

			updateUI();

			double actualDelay = synchronizeVideo(videoFrame->Pts);

			// queue current frame in freeFrames to be used again
			videoDecoder->FrameQueue->enqueueFreeVideoFrame(videoFrame);	

			if(actualDelay < 0.010) {

				// delay is too small skip next frame
				skipVideoFrame = true;
				videoDebug->VideoDropped = videoDebug->VideoDropped + 1;
				goto restartvideo;

			} 

			// start timer with delay for next frame
			videoRefreshTimer->Interval = int(actualDelay * 1000 + 0.5);
			videoRefreshTimer->start();		

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

			if(videoDecoder->HasAudio && syncMode == SyncMode::VIDEO_SYNCS_TO_AUDIO) {

				// synchronize video to audio
				double diff = getVideoClock() - audioPlayer->getAudioClock();

				// Skip or repeat the frame. Take delay into account
				// FFPlay still doesn't "know if this is the best guess."
				double sync_threshold = (delay > AV_SYNC_THRESHOLD) ? delay : AV_SYNC_THRESHOLD;

				if(Math::Abs(diff) < AV_NOSYNC_THRESHOLD) {

					if(diff <= -sync_threshold) {

						delay = 0;

					} else if(diff >= sync_threshold) {

						delay = 2 * delay;
					}
				}

			}

			// adjust delay based on the actual current time
			videoFrameTimer += delay;
			double actualDelay = videoFrameTimer - HRTimer::getTimestamp();

			videoDebug->VideoDelay = delay;
			videoDebug->ActualVideoDelay = actualDelay;
			videoDebug->VideoSync = getVideoClock();
			videoDebug->AudioSync = audioPlayer->getAudioClock();
			videoDebug->VideoQueue = videoDecoder->FrameQueue->VideoQueueSize;
			videoDebug->AudioQueue = videoDecoder->FrameQueue->AudioQueueSize;
			videoDebug->update();

			return(actualDelay);
		}

		double getAudioClock() {

			return(audioPtsDrift - HRTimer::getTimestamp());
		}

		void processAudioFrame() {

restartaudio:
			AudioFrame ^audioFrame = nullptr;

			bool success = videoDecoder->FrameQueue->getDecodedAudioFrame(audioFrame);
			if(success == false) return;

			audioPtsDrift = audioFrame->Pts + HRTimer::getTimestamp();

			adjustAudioLength(audioFrame);

			audioPlayer->write(audioFrame);

			videoDebug->AudioFrames = videoDebug->AudioFrames + 1;
			videoDebug->AudioFrameSize = audioFrame->Length;
			
			double actualDelay = synchronizeAudio(audioFrame);

			// queue current frame in freeFrames to be used again
			videoDecoder->FrameQueue->enqueueFreeAudioFrame(audioFrame);

			if(actualDelay <= 0) {

				// delay too small, play next frame as quickly as possible
				videoDebug->AudioDropped = videoDebug->AudioDropped + 1;
				goto restartaudio;

			} 
		
			// start timer with delay for next frame
			audioRefreshTimer->Interval = int(actualDelay * 1000 + 0.5);
			audioRefreshTimer->start();

		}

		double synchronizeAudio(AudioFrame ^frame) {

			// calculate delay to play next frame
			int bytesPerSecond = videoDecoder->SamplesPerSecond * 
				videoDecoder->BytesPerSample * videoDecoder->NrChannels;

			double delay = frame->Length / double(bytesPerSecond);

			// adjust delay based on the actual current time
			audioFrameTimer += delay;
			double actualDelay = audioFrameTimer - HRTimer::getTimestamp();

			videoDebug->AudioDelay = delay;
			videoDebug->ActualAudioDelay = actualDelay;			

			return(actualDelay);
		}


		void adjustAudioLength(AudioFrame ^frame) {

			//int n = 2 * videoDecoder->NrChannels;

			if(syncMode == SyncMode::AUDIO_SYNCS_TO_VIDEO) {

				int n = videoDecoder->NrChannels * videoDecoder->BytesPerSample;

				double diff = audioPlayer->getAudioClock() - getVideoClock();

				if(Math::Abs(diff) < AV_NOSYNC_THRESHOLD) {

					// accumulate the diffs
					audioDiffCum = diff + audioDiffAvgCoef * audioDiffCum;

					if(audioDiffAvgCount < AUDIO_DIFF_AVG_NB) {

						audioDiffAvgCount++;

					} else {

						double avgDiff = audioDiffCum * (1.0 - audioDiffAvgCoef);

						// Shrinking/expanding buffer code....
						if(Math::Abs(avgDiff) >= audioDiffThreshold) {

							int wantedSize = frame->Length + 
								(int(diff * videoDecoder->SamplesPerSecond) * n);

							int minSize = int(frame->Length * ((100 - AUDIO_SAMPLE_CORRECTION_PERCENT_MAX)
								/ 100));

							int maxSize = int(frame->Length * ((100 + AUDIO_SAMPLE_CORRECTION_PERCENT_MAX) 
								/ 100));

							if(wantedSize < minSize) {

								wantedSize = minSize;

							} else if(wantedSize > maxSize) {

								wantedSize = maxSize;
							}

							if(wantedSize < frame->Length) {

								// remove samples 
								Util::DebugOut("Removing Samples: " + (frame->Length - wantedSize).ToString());
								frame->Length = wantedSize;

							} else if(wantedSize > frame->Length) {
														
								// add samples by copying final samples
								int nrExtraSamples = wantedSize - frame->Length;
								Util::DebugOut("Adding Samples: " + nrExtraSamples.ToString());
						
								array<unsigned char> ^lastSample = gcnew array<unsigned char>(n);

								for(int i = 0; i < n; i++) {

									lastSample[i] = frame->Data[frame->Length - n + i];
								}

								frame->Stream->Position = frame->Length;

								while(nrExtraSamples > 0) {
									
									frame->Stream->Write(lastSample, 0, n);
									nrExtraSamples -= n;
								}

								frame->Stream->Position = 0;
								frame->Length = wantedSize;
							}

						}


						//audioDiffAvgCount = 0;
					}

				} else {

					// difference is TOO big; reset diff stuff 
					audioDiffAvgCount = 0;
					audioDiffCum = 0;
				}
			}
			
		}

		void pausePlay() {

			videoDecoder->FrameQueue->stop();

			videoDecoderBW->CancelAsync();
	
			while(IsPlaying) {

				Application::DoEvents();
			}


		}

		void startPlay() {

			//if(IsPlaying) return;

			audioPlayer->startPlayAfterNextWrite();

			videoDecoder->FrameQueue->start();
			
			videoDecoderBW->RunWorkerAsync();

			previousVideoPts = 0;
			previousVideoDelay = 0.04;

			audioDiffAvgCount = 0;

			videoRefreshTimer->start();
			audioRefreshTimer->start();
			
		}

		void fillFrameQueue() {

			videoDecoder->FrameQueue->start();

			int nrFramesDecoded;

			do {

				nrFramesDecoded = videoDecoder->decodeFrame(
							VideoPlayer::DecodeMode::DECODE_VIDEO_AND_AUDIO);

				//Util::DebugOut("a: " + videoDecoder->FrameQueue->AudioQueueSize.ToString());
				//Util::DebugOut("v: " + videoDecoder->FrameQueue->VideoQueueSize.ToString());

			} while(videoDecoder->FrameQueue->AudioQueueSize !=
				videoDecoder->FrameQueue->MaxAudioQueueSize &&
				videoDecoder->FrameQueue->VideoQueueSize != 
				videoDecoder->FrameQueue->MaxVideoQueueSize &&
				nrFramesDecoded > 0);
			
		}

		void seek(double seconds) {

			seekPosition = seconds;
			seekRequest = true;			
		}

	public:

		property bool IsPlaying {

			bool get() {

				return(videoDecoderBW->IsBusy);
			}
		}

		void open(String ^location) {

			pause();
			close();
			videoDebug->clear();			
			
			videoDecoder->open(location);
			videoRender->initialize(videoDecoder->Width, videoDecoder->Height);
			
			if(videoDecoder->HasAudio) {

				audioPlayer->initialize(videoDecoder->SamplesPerSecond, videoDecoder->BytesPerSample,
					videoDecoder->NrChannels, videoDecoder->MaxAudioFrameSize * 2);			

				muteCheckBox->Enabled = true;
				volumeTrackBar->Enabled = true;

				audioDiffThreshold = 2.0 * 1024 / videoDecoder->SamplesPerSecond;

			} else {

				muteCheckBox->Enabled = false;
				volumeTrackBar->Enabled = false;
			}
  
			videoDebug->VideoQueueSize = videoDecoder->FrameQueue->MaxVideoQueueSize;
			videoDebug->VideoQueueSizeBytes = videoDecoder->FrameQueue->VideoQueueSizeBytes;	
			videoDebug->AudioQueueSize = videoDecoder->FrameQueue->MaxAudioQueueSize;
			videoDebug->AudioQueueSizeBytes = videoDecoder->FrameQueue->AudioQueueSizeBytes;
			
			//fillFrameQueue();
		}

		void play() {

			playCheckBox->Checked = true;
		}

		void pause() {

			playCheckBox->Checked = false;
		}
	

		void close() {

			videoDecoder->close();
			audioPlayer->flush();
		}


private: System::Void videoDecoderBW_DoWork(System::Object^  sender, System::ComponentModel::DoWorkEventArgs^  e) {
			 			
				//videoFrameTimer = videoDecoder->TimeNow;
				audioFrameTimer = videoFrameTimer = HRTimer::getTimestamp();

				int nrFramesDecoded;

				// decode frames one by one, or handle seek requests
				do
				{
					if(seekRequest == true) {

						if(videoDecoder->seek(seekPosition) == 0) {
							
							// flush will only empty, not stop the framequeue
							// This means the videorender and audioplayer thread will 
							// wait until the queue gets filled again
							videoDecoder->FrameQueue->flush();
							audioPlayer->flush();
							
						}
						seekRequest = false;

					} else {

						nrFramesDecoded = videoDecoder->decodeFrame(
							VideoPlayer::DecodeMode::DECODE_VIDEO_AND_AUDIO);
					}
									
				} while(nrFramesDecoded > 0 && !videoDecoderBW->CancellationPending);
				

				// stop the audio
				audioPlayer->stop();
		 }
private: System::Void videoRefreshTimer_Tick(System::Object^  sender, System::EventArgs^  e) {			
 
	
			 processVideoFrame();

		 }

private: System::Void audioRefreshTimer_Tick(Object^  sender, EventArgs ^e) {

			 processAudioFrame();
		 }

private: System::Void stopButton_Click(System::Object^  sender, System::EventArgs^  e) {

			 pause();			 
			 close();
			 videoRender->clearScreen(this->BackColor);
			 timeTrackBar->Value = timeTrackBar->Minimum;
		 }

private: System::Void debugVideoCheckBox_CheckedChanged(System::Object^  sender, System::EventArgs^  e) {

			 if(debugVideoCheckBox->Checked == true) {

				videoDebug->Show();

			 } else {

				videoDebug->Hide();
			 }
		 }
private: System::Void volumeTrackBar_ValueChanged(System::Object^  sender, System::EventArgs^  e) {

			 double volume = Util::invlerp<int>(volumeTrackBar->Value,volumeTrackBar->Minimum, volumeTrackBar->Maximum);
			 audioPlayer->Volume = volume;
		 }
private: System::Void muteCheckBox_CheckedChanged(System::Object^  sender, System::EventArgs^  e) {

			 if(muteCheckBox->Checked == true) {

				 audioPlayer->Muted = true;
				 muteCheckBox->ImageIndex = 1;

			 } else {

				 audioPlayer->Muted = false;
				 muteCheckBox->ImageIndex = 0;

			 }
		 }
private: System::Void playCheckBox_CheckedChanged(System::Object^  sender, System::EventArgs^  e) {

			 if(playCheckBox->Checked == true) {

				 startPlay();
				 playCheckBox->ImageIndex = 3;

			 } else {

				 pausePlay();
				 playCheckBox->ImageIndex = 2;

			 }
		 }
private: System::Void forwardButton_Click(System::Object^  sender, System::EventArgs^  e) {

			 static int i = 0;

			 seek(i * 60);
			 i++;
	
		
		 }
private: System::Void timeTrackBar_MouseDown(System::Object^  sender, System::Windows::Forms::MouseEventArgs^  e) {

			 int totalTime = videoDecoder->DurationSeconds;

			 Rectangle chanRec = WindowsUtils::getTrackBarChannelRect(timeTrackBar);

			 double value = Util::invlerp<int>(e->X, chanRec.Left, chanRec.Right);

			 timeTrackBar->Value = Util::lerp<int>(value, timeTrackBar->Minimum, timeTrackBar->Maximum);

			 int seconds = Util::lerp<int>(value, 0, totalTime);

			 seek(seconds);
		 }
};
}
