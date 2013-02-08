#pragma once
// Directx9 tutorial: http://www.drunkenhyena.com/cgi-bin/dx9_net.pl
// Implementing video in directx: http://www.codeproject.com/Articles/207642/Video-Shadering-with-Direct3D
#include "ImageUtils.h"
#include "Util.h"
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
//using namespace WMPLib;

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
			initializeD3D();
			initializeDS();
			videoPlayer = gcnew VideoPlayer(d3dDevice, makeFourCC('Y', 'V', '1', '2'));	
		
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

	private: System::Windows::Forms::Timer^  videoRefreshTimer;
	private: System::Windows::Forms::SplitContainer^  splitContainer;
	private: System::Windows::Forms::TrackBar^  timeTrackBar;
	private: System::Windows::Forms::Button^  stopButton;
	private: System::Windows::Forms::Button^  playButton;


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
			this->videoDecoderBW = (gcnew System::ComponentModel::BackgroundWorker());
			this->videoRefreshTimer = (gcnew System::Windows::Forms::Timer(this->components));
			this->splitContainer = (gcnew System::Windows::Forms::SplitContainer());
			this->stopButton = (gcnew System::Windows::Forms::Button());
			this->playButton = (gcnew System::Windows::Forms::Button());
			this->timeTrackBar = (gcnew System::Windows::Forms::TrackBar());
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
			// videoRefreshTimer
			// 
			this->videoRefreshTimer->Interval = 40;
			this->videoRefreshTimer->Tick += gcnew System::EventHandler(this, &VideoPanelControl::videoRefreshTimer_Tick);
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
			this->splitContainer->Panel2->Controls->Add(this->stopButton);
			this->splitContainer->Panel2->Controls->Add(this->playButton);
			this->splitContainer->Panel2->Controls->Add(this->timeTrackBar);
			this->splitContainer->Size = System::Drawing::Size(578, 484);
			this->splitContainer->SplitterDistance = 388;
			this->splitContainer->TabIndex = 0;
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
			this->timeTrackBar->Size = System::Drawing::Size(574, 43);
			this->timeTrackBar->TabIndex = 0;
			// 
			// VideoPanelControl
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->Controls->Add(this->splitContainer);
			this->DoubleBuffered = true;
			this->Name = L"VideoPanelControl";
			this->Size = System::Drawing::Size(578, 484);
			this->splitContainer->Panel2->ResumeLayout(false);
			this->splitContainer->ResumeLayout(false);
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->timeTrackBar))->EndInit();
			this->ResumeLayout(false);

		}
#pragma endregion
	private:

		VideoPlayer ^videoPlayer;
		double previousPts;
		double previousDelay;
		static double frameTimer;

		bool skipFrame;

		D3D::Device ^d3dDevice;
		D3D::PresentParameters ^presentParams;

		DS::Device ^dsDevice;
		DS::SecondaryBuffer ^audioBuffer;

		property Control ^VideoPanel {

			Control ^get() {

				return(splitContainer->Panel1);
			}
		}

		void initializeD3D() {

			try {
				//Assume this is pre-initialized to your choice of full-screen or windowed mode.
				bool fullScreen = false;

				//Hard-coded to a common format.  A better method will be shown later in this lesson.
				D3D::Format format = D3D::Format::R8G8B8;

				//Allocate our class
				presentParams = gcnew D3D::PresentParameters();

				//No Z (Depth) buffer or Stencil buffer
				presentParams->EnableAutoDepthStencil = false;

				//1 Back buffer for double-buffering
				presentParams->BackBufferCount = 1;

				//Set our Window as the Device Window
				presentParams->DeviceWindow = VideoPanel;

				//Do not wait for VSync
				presentParams->PresentationInterval = D3D::PresentInterval::Immediate;

				//Discard old frames for better performance
				presentParams->SwapEffect = D3D::SwapEffect::Discard;

				//Set Windowed vs. Full-screen
				presentParams->Windowed = !fullScreen;

				//We only need to set the Width/Height in full-screen mode
				if(fullScreen) {

					presentParams->BackBufferHeight = Height;
					presentParams->BackBufferWidth = Width;

					//Choose a compatible 16-bit mode.
					presentParams->BackBufferFormat = format;

				} else {

					presentParams->BackBufferHeight = 0;
					presentParams->BackBufferWidth = 0;
					presentParams->BackBufferFormat = D3D::Format::Unknown;
				}

				d3dDevice = gcnew D3D::Device(0,                       //Adapter
					D3D::DeviceType::Hardware,  //Device Type
					this,                     //Render Window
					D3D::CreateFlags::SoftwareVertexProcessing, //behaviour flags
					presentParams);          //PresentParamters

				d3dDevice->DeviceLost += gcnew EventHandler(this, &VideoPanelControl::device_DeviceLost);
				d3dDevice->DeviceReset += gcnew EventHandler(this, &VideoPanelControl::device_DeviceReset);
				d3dDevice->DeviceResizing += gcnew CancelEventHandler(this, &VideoPanelControl::device_DeviceResizing);

			} catch (D3D::GraphicsException ^exception){

				Util::DebugOut("Error Code:" + exception->ErrorCode);
				Util::DebugOut("Error String:" + exception->ErrorString);
				Util::DebugOut("Message:" + exception->Message);
				Util::DebugOut("StackTrace:" + exception->StackTrace);
			}

		}

		void initializeDS() {

			try {
				dsDevice = gcnew DS::Device();
				dsDevice->SetCooperativeLevel(this, DS::CooperativeLevel::Priority);


				//DS::SecondaryBuffer ^buffer = gcnew DS::SecondaryBuffer(
				//DS::BufferDescription ^desc = gcnew DS::BufferDescription();
				//desc->
			

			} catch (DS::SoundException ^exception){

				Util::DebugOut("Error Code:" + exception->ErrorCode);
				Util::DebugOut("Error String:" + exception->ErrorString);
				Util::DebugOut("Message:" + exception->Message);
				Util::DebugOut("StackTrace:" + exception->StackTrace);
			}

		}

		void render() {

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

			int a = splitContainer->Height;
			int b = splitContainer->Panel1->Height;
			int c = splitContainer->Panel2->Height;
		
			//canvas.X += splitContainer->Location.X;
			//canvas.Y = 0;

			VideoFrame ^currentFrame;
			
			bool success = videoPlayer->packetQueue->getDecodedFrame(currentFrame);
			if(success == false) return;

			if(skipFrame == false) {

				Color color = this->BackColor;
				//Color color = Color::Blue;

				d3dDevice->Clear(D3D::ClearFlags::Target, color, 1.0f, 0);

				d3dDevice->BeginScene();				

				if(currentFrame->Frame->Disposed == true) {

					int wtf = 1;
				}

				D3D::Surface ^backBuffer = d3dDevice->GetBackBuffer(0,0, D3D::BackBufferType::Mono);

				d3dDevice->StretchRectangle(currentFrame->Frame,Rectangle(0,0,videoWidth,videoHeight),
					backBuffer, canvas, D3D::TextureFilter::Linear);

				d3dDevice->EndScene();
				d3dDevice->Present();
			}
			// calculate delay to display next frame
			double delay = currentFrame->Pts - previousPts;	

			if(delay <= 0 || delay >= 1.0) {
				// if incorrect delay, use previous one 
				delay = previousDelay;
			}

			previousPts = currentFrame->Pts;
			previousDelay = delay;

			// adjust delay based on the actual current time
			frameTimer += delay;
			double actualDelay = frameTimer - getTimeNow();

			if(actualDelay < 0.010) {

				Util::DebugOut("Delay too small: " + actualDelay.ToString());					
				skipFrame = true;
				actualDelay = 0.010;

			} else {

				skipFrame = false;
			}

			// queue current frame in freeFrames to be used again
			videoPlayer->packetQueue->queueFreeFrame(currentFrame);

			// start timer with delay for next frame
			videoRefreshTimer->Interval = int(actualDelay * 1000 + 0.5);
			videoRefreshTimer->Start();				

		}

		double getTimeNow() {

			double timeNow = Diagnostics::Stopwatch::GetTimestamp() / double(Diagnostics::Stopwatch::Frequency);
			return(timeNow);
		}

		D3D::Format makeFourCC(int ch0, int ch1, int ch2, int ch3)
		{
			int value = (int)(char)(ch0)|((int)(char)(ch1) << 8)| ((int)(char)(ch2) << 16) | ((int)(char)(ch3) << 24);
			return((D3D::Format) value);
		}

		void device_DeviceResizing(Object ^sender, CancelEventArgs ^e) {

			//e->Cancel = true;
			//stop();
		
		}

		void device_DeviceReset(Object ^sender, EventArgs ^e) {

			//videoPlayer->initializeResources();
		}

		void device_DeviceLost(Object ^sender, EventArgs ^e) {
			
			if(d3dDevice->CheckCooperativeLevel() == false) {

				//stop();
				//videoPlayer->disposeResources();
			}

		}

	public:

		property bool IsPlaying {

			bool get() {

				return(videoDecoderBW->IsBusy);
			}
		}

		void open(String ^location) {

			// stop any previously started video from playing	
			if(IsPlaying) {

				stop();
				close();
			}
			
			videoPlayer->open(location);
			
			DS::WaveFormat format;

            format.SamplesPerSecond = videoPlayer->BitRate;
            format.BitsPerSample = videoPlayer->BitsPerSample;
			format.Channels = videoPlayer->NrChannels;
			format.FormatTag = DS::WaveFormatTag::Pcm;
			format.BlockAlign = (short)(format.Channels * (format.BitsPerSample / 8));
			format.AverageBytesPerSecond = format.SamplesPerSecond * format.BlockAlign;

			DS::BufferDescription ^desc = gcnew DS::BufferDescription(format);
			desc->BufferBytes = 1024;
			desc->DeferLocation = true;
			desc->GlobalFocus = true;

			audioBuffer = gcnew DS::SecondaryBuffer(desc, dsDevice);
  
		}

		void stop() {

			videoPlayer->packetQueue->stop();

			videoDecoderBW->CancelAsync();
			while(IsPlaying) {

				Application::DoEvents();
			}

			
			videoRefreshTimer->Stop();
			
		}

		void play() {

			previousPts = 0;
			previousDelay = 0.04;
			skipFrame = false;	

			videoPlayer->packetQueue->start();

			videoDecoderBW->RunWorkerAsync();

			videoRefreshTimer->Enabled = true;
			videoRefreshTimer->Start();
		}

		void close() {

			videoPlayer->close();
		}


private: System::Void videoDecoderBW_DoWork(System::Object^  sender, System::ComponentModel::DoWorkEventArgs^  e) {
			 			
				//frameTimer = videoPlayer->TimeNow;
				frameTimer = getTimeNow();

				while(videoPlayer->decodeFrame() && !videoDecoderBW->CancellationPending) {

					
				}
				
		 }
private: System::Void videoRefreshTimer_Tick(System::Object^  sender, System::EventArgs^  e) {			

			 videoRefreshTimer->Stop();

			 //if(isPlaying == false) return;
			 
			 // redraw screen
			 int result;

			 if(d3dDevice->CheckCooperativeLevel(result)) {

				 try {

					 render();

				 } catch(D3D::DeviceLostException ^) {

					 d3dDevice->CheckCooperativeLevel(result);

				 } catch(D3D::DeviceNotResetException ^) {

					 d3dDevice->CheckCooperativeLevel(result);

				 } catch (D3D::GraphicsException ^exception) {

					 Util::DebugOut("Error Code:" + exception->ErrorCode);
					 Util::DebugOut("Error String:" + exception->ErrorString);
					 Util::DebugOut("Message:" + exception->Message);
					 Util::DebugOut("StackTrace:" + exception->StackTrace);
				 }
			 }

			 if(result == (int)D3D::ResultCode::DeviceLost) {

				 System::Threading::Thread::Sleep(500);    //Can't Reset yet, wait for a bit

			 }else if (result == (int)D3D::ResultCode::DeviceNotReset) {

				 d3dDevice->Reset(presentParams);
			 }
		
		 }


private: System::Void playButton_Click(System::Object^  sender, System::EventArgs^  e) {

			 play();
		 }
private: System::Void stopButton_Click(System::Object^  sender, System::EventArgs^  e) {

			 stop();
		 }
};
}
