#pragma once
// Directx9 tutorial: http://www.drunkenhyena.com/cgi-bin/dx9_net.pl
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
using namespace Microsoft::DirectX::Direct3D;
using namespace VideoLib;
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
			videoPlayer = gcnew VideoPlayer(device);
			previousPts = 0;
			previousDelay = 0.04;
			skipFrame = false;
		
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
			// VideoPanelControl
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->DoubleBuffered = true;
			this->Name = L"VideoPanelControl";
			this->Size = System::Drawing::Size(578, 484);
			this->ResumeLayout(false);

		}
#pragma endregion
	private:

		VideoPlayer ^videoPlayer;
		double previousPts;
		double previousDelay;
		static double frameTimer;
		Diagnostics::Stopwatch ^stopwatch;

		bool skipFrame;

		Device ^device;
		Sprite ^sprite; 

		void initializeD3D() {

			try {
				//Assume this is pre-initialized to your choice of full-screen or windowed mode.
				bool fullScreen = false;

				//Hard-coded to a common format.  A better method will be shown later in this lesson.
				Format format = Format::R8G8B8;

				//Allocate our class
				PresentParameters ^presentParams = gcnew PresentParameters();

				//No Z (Depth) buffer or Stencil buffer
				presentParams->EnableAutoDepthStencil = false;

				//1 Back buffer for double-buffering
				presentParams->BackBufferCount = 1;

				//Set our Window as the Device Window
				presentParams->DeviceWindow = this;

				//Do not wait for VSync
				presentParams->PresentationInterval = PresentInterval::Immediate;

				//Discard old frames for better performance
				presentParams->SwapEffect = SwapEffect::Discard;

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
					presentParams->BackBufferFormat = Format::Unknown;
				}

				device = gcnew Device(0,                       //Adapter
					DeviceType::Hardware,  //Device Type
					this,                     //Render Window
					CreateFlags::SoftwareVertexProcessing, //behaviour flags
					presentParams);          //PresentParamters


				sprite = gcnew Sprite(device);

			} catch (GraphicsException ^exception){

				Util::DebugOut("Error Code:" + exception->ErrorCode);
				Util::DebugOut("Error String:" + exception->ErrorString);
				Util::DebugOut("Message:" + exception->Message);
				Util::DebugOut("StackTrace:" + exception->StackTrace);
			}

		}

		void render() {

			int width = videoPlayer->Width;
			int height = videoPlayer->Height;
			int scaledWidth, scaledHeight;

			ImageUtils::stretchRectangle(width, height,
				Width,Height,scaledWidth, scaledHeight);

			Rectangle canvas = ImageUtils::centerRectangle(Rectangle(0,0,Width,Height),
				Rectangle(0,0,scaledWidth,scaledHeight));

			VideoFrame ^currentFrame = videoPlayer->decodedFrames->Take();
	
			if(skipFrame == false) {

				Color color = Color::CadetBlue;

				device->Clear(ClearFlags::Target, color, 1.0f, 0);

				device->BeginScene();

				//Spiffy rendering goes here
				sprite->Begin(SpriteFlags::None);
				sprite->Draw2D(currentFrame->Frame, Rectangle::Empty, 
					SizeF((float)canvas.Width, (float)canvas.Height), 
					PointF((float)canvas.X, (float)canvas.Y), 
					Color::White);
				sprite->End();

				device->EndScene();

				device->Present();
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
				actualDelay = 0.010;					
				skipFrame = true;

			} else {

				skipFrame = false;
			}

			// start timer with delay for next frame
			videoRefreshTimer->Interval = int(actualDelay * 1000 + 0.5);
			videoRefreshTimer->Start();		

			// queue current frame in freeFrames to be used again
			videoPlayer->freeFrames->Put(currentFrame);

		}

		double getTimeNow() {

			double timeNow = Diagnostics::Stopwatch::GetTimestamp() / double(Diagnostics::Stopwatch::Frequency);
			return(timeNow);
		}

	public:

		void playVideo(String ^location) {

			// stop any previously started video from playing
			videoDecoderBW->CancelAsync();
			while(videoDecoderBW->IsBusy) {

				Application::DoEvents();
			}
			videoPlayer->close();
						
			// start decoding video in a seperate thread
			videoPlayer->open(location);
			videoDecoderBW->RunWorkerAsync();

			videoRefreshTimer->Enabled = true;
			videoRefreshTimer->Start();
		}


private: System::Void videoDecoderBW_DoWork(System::Object^  sender, System::ComponentModel::DoWorkEventArgs^  e) {
			 			
			 
				//frameTimer = videoPlayer->TimeNow;
				frameTimer = getTimeNow();

				while(videoPlayer->decodeFrame() && !videoDecoderBW->CancellationPending) {

					int i = 0;
				}
				
				int i = 0;
		 }
private: System::Void videoRefreshTimer_Tick(System::Object^  sender, System::EventArgs^  e) {
			 
			 videoRefreshTimer->Stop();
			 
			 // redraw screen
			 try {

				 render();

			 }catch (GraphicsException ^exception){

				 Util::DebugOut("Error Code:" + exception->ErrorCode);
				 Util::DebugOut("Error String:" + exception->ErrorString);
				 Util::DebugOut("Message:" + exception->Message);
				 Util::DebugOut("StackTrace:" + exception->StackTrace);
			 }
			 //this->Invalidate();
		 }


};
}
