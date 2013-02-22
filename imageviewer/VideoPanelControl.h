#pragma once
// Directx9 tutorial: http://www.drunkenhyena.com/cgi-bin/dx9_net.pl
// Implementing video in directx: http://www.codeproject.com/Articles/207642/Video-Shadering-with-Direct3D
#include "ImageUtils.h"
#include "Util.h"
#include "HRTimerFactory.h"
#include "StreamingAudioBuffer.h"
#include "VideoRender.h"
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
			//video = nullptr;
			//mediaPlayer->Dock = DockStyle::Fill;
			//mediaPlayer->stretchToFit = true;

			video = gcnew VideoRender(this, VideoPanel);
			audio = gcnew StreamingAudioBuffer(this);

			videoPlayer = gcnew VideoPlayer(video->Device, 
				VideoRender::makeFourCC('Y', 'V', '1', '2'));	
			
			videoRefreshTimer = HRTimerFactory::create(HRTimerFactory::TimerType::TIMER_QUEUE);
			videoRefreshTimer->Tick += gcnew EventHandler(this, &VideoPanelControl::videoRefreshTimer_Tick);
			videoRefreshTimer->SynchronizingObject = this;
			videoRefreshTimer->AutoReset = false;

			audioRefreshTimer = HRTimerFactory::create(HRTimerFactory::TimerType::TIMER_QUEUE);
			audioRefreshTimer->Tick += gcnew EventHandler(this, &VideoPanelControl::audioRefreshTimer_Tick);
			audioRefreshTimer->AutoReset = false;
			audioRefreshTimer->SynchronizingObject = nullptr;
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
	private: System::Windows::Forms::Label^  audioQueueLabel;

	private: System::Windows::Forms::Label^  videoQueueLabel;

	private: System::Windows::Forms::Label^  label2;
	private: System::Windows::Forms::Label^  label1;
	private: System::Windows::Forms::Label^  audioSyncLabel;
	private: System::Windows::Forms::Label^  videoSyncLabel;
	private: System::Windows::Forms::Label^  label4;
	private: System::Windows::Forms::Label^  label3;
	private: System::Windows::Forms::Label^  label5;
	private: System::Windows::Forms::Label^  label6;
	private: System::Windows::Forms::Label^  audioDelayLabel;
	private: System::Windows::Forms::Label^  videoDelayLabel;





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
			this->videoDecoderBW = (gcnew System::ComponentModel::BackgroundWorker());
			this->splitContainer = (gcnew System::Windows::Forms::SplitContainer());
			this->audioSyncLabel = (gcnew System::Windows::Forms::Label());
			this->videoSyncLabel = (gcnew System::Windows::Forms::Label());
			this->label4 = (gcnew System::Windows::Forms::Label());
			this->label3 = (gcnew System::Windows::Forms::Label());
			this->audioQueueLabel = (gcnew System::Windows::Forms::Label());
			this->videoQueueLabel = (gcnew System::Windows::Forms::Label());
			this->label2 = (gcnew System::Windows::Forms::Label());
			this->label1 = (gcnew System::Windows::Forms::Label());
			this->stopButton = (gcnew System::Windows::Forms::Button());
			this->playButton = (gcnew System::Windows::Forms::Button());
			this->timeTrackBar = (gcnew System::Windows::Forms::TrackBar());
			this->label5 = (gcnew System::Windows::Forms::Label());
			this->label6 = (gcnew System::Windows::Forms::Label());
			this->audioDelayLabel = (gcnew System::Windows::Forms::Label());
			this->videoDelayLabel = (gcnew System::Windows::Forms::Label());
			this->splitContainer->Panel2->SuspendLayout();
			this->splitContainer->SuspendLayout();
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
			this->splitContainer->Panel2->Controls->Add(this->audioDelayLabel);
			this->splitContainer->Panel2->Controls->Add(this->videoDelayLabel);
			this->splitContainer->Panel2->Controls->Add(this->label5);
			this->splitContainer->Panel2->Controls->Add(this->label6);
			this->splitContainer->Panel2->Controls->Add(this->audioSyncLabel);
			this->splitContainer->Panel2->Controls->Add(this->videoSyncLabel);
			this->splitContainer->Panel2->Controls->Add(this->label4);
			this->splitContainer->Panel2->Controls->Add(this->label3);
			this->splitContainer->Panel2->Controls->Add(this->audioQueueLabel);
			this->splitContainer->Panel2->Controls->Add(this->videoQueueLabel);
			this->splitContainer->Panel2->Controls->Add(this->label2);
			this->splitContainer->Panel2->Controls->Add(this->label1);
			this->splitContainer->Panel2->Controls->Add(this->stopButton);
			this->splitContainer->Panel2->Controls->Add(this->playButton);
			this->splitContainer->Panel2->Controls->Add(this->timeTrackBar);
			this->splitContainer->Size = System::Drawing::Size(800, 484);
			this->splitContainer->SplitterDistance = 388;
			this->splitContainer->TabIndex = 0;
			// 
			// audioSyncLabel
			// 
			this->audioSyncLabel->AutoSize = true;
			this->audioSyncLabel->Location = System::Drawing::Point(450, 65);
			this->audioSyncLabel->Name = L"audioSyncLabel";
			this->audioSyncLabel->Size = System::Drawing::Size(18, 20);
			this->audioSyncLabel->TabIndex = 10;
			this->audioSyncLabel->Text = L"0";
			// 
			// videoSyncLabel
			// 
			this->videoSyncLabel->AutoSize = true;
			this->videoSyncLabel->Location = System::Drawing::Point(450, 45);
			this->videoSyncLabel->Name = L"videoSyncLabel";
			this->videoSyncLabel->Size = System::Drawing::Size(18, 20);
			this->videoSyncLabel->TabIndex = 9;
			this->videoSyncLabel->Text = L"0";
			// 
			// label4
			// 
			this->label4->AutoSize = true;
			this->label4->Location = System::Drawing::Point(361, 65);
			this->label4->Name = L"label4";
			this->label4->Size = System::Drawing::Size(93, 20);
			this->label4->TabIndex = 8;
			this->label4->Text = L"Audio Sync:";
			// 
			// label3
			// 
			this->label3->AutoSize = true;
			this->label3->Location = System::Drawing::Point(361, 45);
			this->label3->Name = L"label3";
			this->label3->Size = System::Drawing::Size(93, 20);
			this->label3->TabIndex = 7;
			this->label3->Text = L"Video Sync:";
			// 
			// audioQueueLabel
			// 
			this->audioQueueLabel->AutoSize = true;
			this->audioQueueLabel->Location = System::Drawing::Point(277, 65);
			this->audioQueueLabel->Name = L"audioQueueLabel";
			this->audioQueueLabel->Size = System::Drawing::Size(31, 20);
			this->audioQueueLabel->TabIndex = 6;
			this->audioQueueLabel->Text = L"0/0";
			// 
			// videoQueueLabel
			// 
			this->videoQueueLabel->AutoSize = true;
			this->videoQueueLabel->Location = System::Drawing::Point(277, 45);
			this->videoQueueLabel->Name = L"videoQueueLabel";
			this->videoQueueLabel->Size = System::Drawing::Size(31, 20);
			this->videoQueueLabel->TabIndex = 5;
			this->videoQueueLabel->Text = L"0/0";
			// 
			// label2
			// 
			this->label2->AutoSize = true;
			this->label2->Location = System::Drawing::Point(174, 65);
			this->label2->Name = L"label2";
			this->label2->Size = System::Drawing::Size(106, 20);
			this->label2->TabIndex = 4;
			this->label2->Text = L"Audio Queue:";
			// 
			// label1
			// 
			this->label1->AutoSize = true;
			this->label1->Location = System::Drawing::Point(174, 45);
			this->label1->Name = L"label1";
			this->label1->Size = System::Drawing::Size(106, 20);
			this->label1->TabIndex = 3;
			this->label1->Text = L"Video Queue:";
			// 
			// stopButton
			// 
			this->stopButton->Location = System::Drawing::Point(60, 49);
			this->stopButton->Name = L"stopButton";
			this->stopButton->Size = System::Drawing::Size(51, 36);
			this->stopButton->TabIndex = 2;
			this->stopButton->Text = L"Stop";
			this->stopButton->UseVisualStyleBackColor = true;
			this->stopButton->Click += gcnew System::EventHandler(this, &VideoPanelControl::stopButton_Click);
			// 
			// playButton
			// 
			this->playButton->Location = System::Drawing::Point(3, 49);
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
			this->timeTrackBar->Name = L"timeTrackBar";
			this->timeTrackBar->Size = System::Drawing::Size(796, 43);
			this->timeTrackBar->TabIndex = 0;
			// 
			// label5
			// 
			this->label5->AutoSize = true;
			this->label5->Location = System::Drawing::Point(578, 65);
			this->label5->Name = L"label5";
			this->label5->Size = System::Drawing::Size(98, 20);
			this->label5->TabIndex = 12;
			this->label5->Text = L"Audio Delay:";
			// 
			// label6
			// 
			this->label6->AutoSize = true;
			this->label6->Location = System::Drawing::Point(578, 45);
			this->label6->Name = L"label6";
			this->label6->Size = System::Drawing::Size(98, 20);
			this->label6->TabIndex = 11;
			this->label6->Text = L"Video Delay:";
			// 
			// audioDelayLabel
			// 
			this->audioDelayLabel->AutoSize = true;
			this->audioDelayLabel->Location = System::Drawing::Point(673, 65);
			this->audioDelayLabel->Name = L"audioDelayLabel";
			this->audioDelayLabel->Size = System::Drawing::Size(18, 20);
			this->audioDelayLabel->TabIndex = 14;
			this->audioDelayLabel->Text = L"0";
			// 
			// videoDelayLabel
			// 
			this->videoDelayLabel->AutoSize = true;
			this->videoDelayLabel->Location = System::Drawing::Point(673, 45);
			this->videoDelayLabel->Name = L"videoDelayLabel";
			this->videoDelayLabel->Size = System::Drawing::Size(18, 20);
			this->videoDelayLabel->TabIndex = 13;
			this->videoDelayLabel->Text = L"0";
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
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->timeTrackBar))->EndInit();
			this->ResumeLayout(false);

		}
#pragma endregion
	private:

		VideoPlayer ^videoPlayer;
		VideoRender ^video;
		StreamingAudioBuffer ^audio;

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

		void invokeUpdateUI(double audioDelay, double actualAudioDelay, int audioSize) {

			videoQueueLabel->Text = videoPlayer->frameQueue->VideoQueueSize.ToString() + "/" + 
				videoPlayer->frameQueue->MaxVideoQueueSize.ToString();

			audioQueueLabel->Text = videoPlayer->frameQueue->AudioQueueSize.ToString() + "/" + 
				videoPlayer->frameQueue->MaxAudioQueueSize.ToString();

			videoSyncLabel->Text = frameTimer.ToString("F4");
			audioSyncLabel->Text = audio->getAudioClock().ToString("F4");

			audioDelayLabel->Text = audioDelay.ToString("F4") + " " +
				actualAudioDelay.ToString("F4") + " " +  audioSize.ToString();

		}

		void updateUI(double audioDelay, double actualAudioDelay, int audioSize) {

			if(this->InvokeRequired) {

				array<Object ^> ^args = gcnew array<Object ^>{audioDelay, actualAudioDelay, audioSize};

				this->Invoke(gcnew UpdateUIDelegate(this, &VideoPanelControl::invokeUpdateUI), args);

			} else {
		
				invokeUpdateUI(audioDelay, actualAudioDelay, audioSize);
			}
		}


		void processVideoFrame() {

restartvideo:

			int videoWidth = videoPlayer->Width;
			int videoHeight = videoPlayer->Height;
			int panelWidth = VideoPanel->Width;
			// weird bug??
			int panelHeight = VideoPanel->Height + splitContainer->Panel2->Height + splitContainer->SplitterRectangle.Height;
			int scaledWidth, scaledHeight;

			ImageUtils::stretchRectangle(videoWidth, videoHeight,
				panelWidth, panelHeight, scaledWidth, scaledHeight);

			Rectangle panelRec(0,0, panelWidth, panelHeight);
			Rectangle scaledVideoRec(0,0, scaledWidth, scaledHeight);

			Rectangle canvas = ImageUtils::centerRectangle(panelRec,
				scaledVideoRec);

			VideoFrame ^videoFrame = nullptr;
			
			bool success = videoPlayer->frameQueue->getDecodedVideoFrame(videoFrame);
			if(success == false) return;

			if(skipVideoFrame == false) {

				video->display(videoFrame, canvas, Color::Black);
			} 

			// calculate delay to display next frame
			double delay = videoFrame->Pts - previousVideoPts;	

			if(delay <= 0 || delay >= 1.0) {
				// if incorrect delay, use previous one 
				delay = previousVideoDelay;
			}

			previousVideoPts = videoFrame->Pts;
			previousVideoDelay = delay;

			// update delay to sync to audio 
			double refClock = audio->getAudioClock();
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

			Util::DebugOut("Video D:" + delay.ToString() + " A: " + actualDelay.ToString());		

			if(actualDelay < 0.010) {

				//Util::DebugOut("Video Delay too small: " + actualDelay.ToString());					
				skipVideoFrame = true;

			} else {

				skipVideoFrame = false;
			}

			// queue current frame in freeFrames to be used again
			videoPlayer->frameQueue->enqueueFreeVideoFrame(videoFrame);	

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

			bool success = videoPlayer->frameQueue->getDecodedAudioFrame(audioFrame);
			if(success == false) return;

			audio->write(audioFrame->Stream, audioFrame->Length);
			
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

			Util::DebugOut("Audio D:" + delay.ToString() + " A: " + actualDelay.ToString());	

			updateUI(delay, actualDelay, audioFrame->Length);	

			// queue current frame in freeFrames to be used again
			videoPlayer->frameQueue->enqueueFreeAudioFrame(audioFrame);

			if(actualDelay < 0.010) {

				// delay too small, play next frame as quickly as possible				
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

			// stop any previously started video from playing	
			//if(IsPlaying) {

				stop();
				close();
			//}
			
			videoPlayer->open(location);
			
			audio->initialize(videoPlayer->SamplesPerSecond, videoPlayer->BytesPerSample,
				videoPlayer->NrChannels, videoPlayer->MaxAudioFrameSize * 2);			
  
		}

		void stop() {

			audio->stop();
			videoPlayer->frameQueue->stop();

			videoDecoderBW->CancelAsync();
			while(IsPlaying) {

				Application::DoEvents();
			}

			videoRefreshTimer->stop();
			audioRefreshTimer->stop();
		
		}

		void play() {

			videoPlayer->frameQueue->start();

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

			videoPlayer->close();
		}


private: System::Void videoDecoderBW_DoWork(System::Object^  sender, System::ComponentModel::DoWorkEventArgs^  e) {
			 			
				//frameTimer = videoPlayer->TimeNow;
				audioFrameTimer = frameTimer = getTimeNow();

				while(videoPlayer->decodeFrame(VideoPlayer::DecodeMode::DECODE_VIDEO_AND_AUDIO) && 
					!videoDecoderBW->CancellationPending) 
				{

					
				}
				
		 }
private: System::Void videoRefreshTimer_Tick(System::Object^  sender, System::EventArgs^  e) {			

			 //videoRefreshTimer->stop();	 
	
			 processVideoFrame();

		 }

private: System::Void audioRefreshTimer_Tick(Object^  sender, EventArgs ^e) {

			 //audioRefreshTimer->stop();

			 processAudioFrame();
		 }

private: System::Void playButton_Click(System::Object^  sender, System::EventArgs^  e) {

			 play();
		 }
private: System::Void stopButton_Click(System::Object^  sender, System::EventArgs^  e) {

			 stop();
		 }

};
}
