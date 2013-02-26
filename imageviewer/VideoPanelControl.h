#pragma once
// Directx9 tutorial: http://www.drunkenhyena.com/cgi-bin/dx9_net.pl
// Implementing videoRender in directx: http://www.codeproject.com/Articles/207642/Video-Shadering-with-Direct3D
#include "ImageUtils.h"
#include "Util.h"
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
	private: System::Windows::Forms::Button^  playButton;
	private: System::Windows::Forms::CheckBox^  debugVideoCheckBox;
	private: System::Windows::Forms::TrackBar^  volumeTrackBar;
	private: System::Windows::Forms::CheckBox^  muteCheckBox;
	private: System::Windows::Forms::ToolTip^  toolTip1;
	private: System::Windows::Forms::ImageList^  imageList;
	private: System::Windows::Forms::Label^  videoTimeLabel;























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
			this->videoTimeLabel = (gcnew System::Windows::Forms::Label());
			this->muteCheckBox = (gcnew System::Windows::Forms::CheckBox());
			this->imageList = (gcnew System::Windows::Forms::ImageList(this->components));
			this->volumeTrackBar = (gcnew System::Windows::Forms::TrackBar());
			this->debugVideoCheckBox = (gcnew System::Windows::Forms::CheckBox());
			this->stopButton = (gcnew System::Windows::Forms::Button());
			this->playButton = (gcnew System::Windows::Forms::Button());
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
			this->splitContainer->Panel2->Controls->Add(this->videoTimeLabel);
			this->splitContainer->Panel2->Controls->Add(this->muteCheckBox);
			this->splitContainer->Panel2->Controls->Add(this->volumeTrackBar);
			this->splitContainer->Panel2->Controls->Add(this->debugVideoCheckBox);
			this->splitContainer->Panel2->Controls->Add(this->stopButton);
			this->splitContainer->Panel2->Controls->Add(this->playButton);
			this->splitContainer->Panel2->Controls->Add(this->timeTrackBar);
			this->splitContainer->Size = System::Drawing::Size(800, 484);
			this->splitContainer->SplitterDistance = 388;
			this->splitContainer->TabIndex = 0;
			// 
			// videoTimeLabel
			// 
			this->videoTimeLabel->Dock = System::Windows::Forms::DockStyle::Right;
			this->videoTimeLabel->Location = System::Drawing::Point(515, 37);
			this->videoTimeLabel->MaximumSize = System::Drawing::Size(137, 36);
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
			// imageList
			// 
			this->imageList->ImageStream = (cli::safe_cast<System::Windows::Forms::ImageListStreamer^  >(resources->GetObject(L"imageList.ImageStream")));
			this->imageList->TransparentColor = System::Drawing::Color::Transparent;
			this->imageList->Images->SetKeyName(0, L"1361908321_1777.ico");
			this->imageList->Images->SetKeyName(1, L"1361908340_1776.ico");
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
			this->debugVideoCheckBox->Location = System::Drawing::Point(128, 37);
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
			this->stopButton->Location = System::Drawing::Point(60, 37);
			this->stopButton->Name = L"stopButton";
			this->stopButton->Size = System::Drawing::Size(51, 36);
			this->stopButton->TabIndex = 2;
			this->stopButton->Text = L"Stop";
			this->stopButton->UseVisualStyleBackColor = true;
			this->stopButton->Click += gcnew System::EventHandler(this, &VideoPanelControl::stopButton_Click);
			// 
			// playButton
			// 
			this->playButton->Location = System::Drawing::Point(3, 37);
			this->playButton->Name = L"playButton";
			this->playButton->Size = System::Drawing::Size(51, 36);
			this->playButton->TabIndex = 1;
			this->playButton->Text = L"Play";
			this->playButton->UseVisualStyleBackColor = true;
			this->playButton->Click += gcnew System::EventHandler(this, &VideoPanelControl::playButton_Click);
			// 
			// timeTrackBar
			// 
			this->timeTrackBar->AutoSize = false;
			this->timeTrackBar->Dock = System::Windows::Forms::DockStyle::Top;
			this->timeTrackBar->Location = System::Drawing::Point(0, 0);
			this->timeTrackBar->Maximum = 1000;
			this->timeTrackBar->Name = L"timeTrackBar";
			this->timeTrackBar->Size = System::Drawing::Size(796, 37);
			this->timeTrackBar->TabIndex = 0;
			this->timeTrackBar->TickStyle = System::Windows::Forms::TickStyle::None;
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
		// no AV correction is done if too big error 
		static const double AV_NOSYNC_THRESHOLD = 10.0;

		double previousVideoPts;
		double previousVideoDelay;
		static double frameTimer;
		HRTimer ^videoRefreshTimer;
		static double audioFrameTimer;
		HRTimer ^audioRefreshTimer;

		double previousAudioPts;
		double previousAudioDelay;

		bool skipVideoFrame;

		delegate void UpdateUIDelegate(double, double, int);

		property Control ^VideoPanel {

			Control ^get() {

				return(splitContainer->Panel1);
			}
		}

		void invokeUpdateUI() {

			int curTime = (int)Math::Floor(audioPlayer->getAudioClock());
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


		void processVideoFrame() {

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
			
			bool success = videoDecoder->frameQueue->getDecodedVideoFrame(videoFrame);
			if(success == false) return;

			if(skipVideoFrame == false) {

				videoRender->display(videoFrame, canvas, Color::Black);
				videoDebug->VideoFrames = videoDebug->VideoFrames + 1;
			} 

			// calculate delay to display next frame
			double delay = videoFrame->Pts - previousVideoPts;	

			if(delay <= 0 || delay >= 1.0) {
				// if incorrect delay, use previous one 
				delay = previousVideoDelay;
			}

			previousVideoPts = videoFrame->Pts;
			previousVideoDelay = delay;

			// update delay to sync to audioPlayer 
			double refClock = audioPlayer->getAudioClock();
			double diff = videoFrame->Pts - refClock;

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

			// adjust delay based on the actual current time
			frameTimer += delay;
			double actualDelay = frameTimer - getTimeNow();

			videoDebug->VideoDelay = delay;
			videoDebug->ActualVideoDelay = actualDelay;
			videoDebug->AudioSync = refClock;
			videoDebug->update();

			updateUI();

			if(actualDelay < 0.010) {

				//Util::DebugOut("Video Delay too small: " + actualDelay.ToString());					
				skipVideoFrame = true;
				videoDebug->VideoDropped = videoDebug->VideoDropped + 1;
			} else {

				skipVideoFrame = false;
			}

			// queue current frame in freeFrames to be used again
			videoDecoder->frameQueue->enqueueFreeVideoFrame(videoFrame);	

			if(skipVideoFrame == true) {
			
				goto restartvideo;
			}

			// start timer with delay for next frame
			videoRefreshTimer->Interval = int(actualDelay * 1000 + 0.5);
			videoRefreshTimer->start();		

		}

		void processAudioFrame() {

restartaudio:
			AudioFrame ^audioFrame = nullptr;

			bool success = videoDecoder->frameQueue->getDecodedAudioFrame(audioFrame);
			if(success == false) return;

			audioPlayer->write(audioFrame->Stream, audioFrame->Length);
			videoDebug->AudioFrames = videoDebug->AudioFrames + 1;
			
			// calculate delay to display next frame
			double delay = audioFrame->Pts - previousAudioPts;	

			if(delay <= 0 || delay >= 1.0) {
				// if incorrect delay, use previous one 
				delay = previousAudioDelay;
			}

			previousAudioPts = audioFrame->Pts;
			previousAudioDelay = delay;

			// adjust delay based on the actual current time
			audioFrameTimer += delay;
			double actualDelay = audioFrameTimer - getTimeNow();

			videoDebug->AudioDelay = delay;
			videoDebug->ActualAudioDelay = actualDelay;
			videoDebug->AudioFrameSize = audioFrame->Length;

			// queue current frame in freeFrames to be used again
			videoDecoder->frameQueue->enqueueFreeAudioFrame(audioFrame);

			if(actualDelay <= 0) {// 0.010) {

				// delay too small, play next frame as quickly as possible
				videoDebug->AudioDropped = videoDebug->AudioDropped + 1;
				goto restartaudio;

			} 
		
			// start timer with delay for next frame
			audioRefreshTimer->Interval = int(actualDelay * 1000 + 0.5);
			audioRefreshTimer->start();
			
		}

		double getTimeNow() {

			double timeNow = Diagnostics::Stopwatch::GetTimestamp() / double(Diagnostics::Stopwatch::Frequency);
			return(timeNow);
		}

	public:

		property bool IsPlaying {

			bool get() {

				return(videoDecoderBW->IsBusy);
			}
		}

		void open(String ^location) {

			stop();
			close();
			videoDebug->clear();			
			
			videoDecoder->open(location);
			videoRender->initialize(videoDecoder->Width, videoDecoder->Height);
			
			audioPlayer->initialize(videoDecoder->SamplesPerSecond, videoDecoder->BytesPerSample,
				videoDecoder->NrChannels, videoDecoder->MaxAudioFrameSize * 2);			
  
			videoDebug->VideoQueueSize = videoDecoder->frameQueue->MaxVideoQueueSize;
			videoDebug->VideoQueueSizeBytes = videoDecoder->frameQueue->VideoQueueSizeBytes;	
			videoDebug->AudioQueueSize = videoDecoder->frameQueue->MaxAudioQueueSize;
			videoDebug->AudioQueueSizeBytes = videoDecoder->frameQueue->AudioQueueSizeBytes;
		}

		void stop() {

			audioPlayer->stop();
			videoRefreshTimer->stop();
			audioRefreshTimer->stop();

			videoDecoder->frameQueue->stop();

			videoDecoderBW->CancelAsync();
			while(IsPlaying) {

				Application::DoEvents();
			}


		}

		void play() {

			videoDecoder->frameQueue->start();

			videoDecoderBW->RunWorkerAsync();

			previousVideoPts = 0;
			previousVideoDelay = 0.04;
			skipVideoFrame = false;	

			previousAudioPts = 0;
			previousAudioDelay = 0.04;

			videoRefreshTimer->start();
			audioRefreshTimer->start();
			
		}

		void close() {

			videoDecoder->close();
		}


private: System::Void videoDecoderBW_DoWork(System::Object^  sender, System::ComponentModel::DoWorkEventArgs^  e) {
			 			
				//frameTimer = videoDecoder->TimeNow;
				audioFrameTimer = frameTimer = getTimeNow();

				while(videoDecoder->decodeFrame(VideoPlayer::DecodeMode::DECODE_VIDEO_AND_AUDIO) && 
					!videoDecoderBW->CancellationPending) 
				{

					videoDebug->VideoQueue = videoDecoder->frameQueue->VideoQueueSize;
					videoDebug->AudioQueue = videoDecoder->frameQueue->AudioQueueSize;
					
				}
				
		 }
private: System::Void videoRefreshTimer_Tick(System::Object^  sender, System::EventArgs^  e) {			
 
	
			 processVideoFrame();

		 }

private: System::Void audioRefreshTimer_Tick(Object^  sender, EventArgs ^e) {

			 processAudioFrame();
		 }

private: System::Void playButton_Click(System::Object^  sender, System::EventArgs^  e) {

			 play();
		 }
private: System::Void stopButton_Click(System::Object^  sender, System::EventArgs^  e) {

			 stop();
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
};
}
