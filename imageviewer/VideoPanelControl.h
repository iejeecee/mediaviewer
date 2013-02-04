#pragma once
#include "ImageUtils.h"
#include "Util.h"

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
			videoPlayer = gcnew VideoPlayer();
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
			this->Paint += gcnew System::Windows::Forms::PaintEventHandler(this, &VideoPanelControl::videoPanelControl_Paint);
			this->ResumeLayout(false);

		}
#pragma endregion
	private:

		VideoPlayer ^videoPlayer;
		double previousPts;
		double previousDelay;
		double frameTimer;

		bool skipFrame;

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
	private: System::Void videoPanelControl_Paint(System::Object^  sender, System::Windows::Forms::PaintEventArgs^  e) {

				 if(videoDecoderBW->IsBusy == false) {

					 // paint can be triggered before we actually start decoding
					 return;
				 }

				 int width = videoPlayer->Width;
				 int height = videoPlayer->Height;
				 int scaledWidth, scaledHeight;

				 ImageUtils::stretchRectangle(width, height,
					 Width,Height,scaledWidth, scaledHeight);

				 Rectangle canvas = ImageUtils::centerRectangle(Rectangle(0,0,Width,Height),
					 Rectangle(0,0,scaledWidth,scaledHeight));

				 Graphics ^g = e->Graphics;

				 // Maximize performance
				 g->CompositingMode = CompositingMode::SourceOver;
				 g->PixelOffsetMode = PixelOffsetMode::HighSpeed;
				 g->CompositingQuality = CompositingQuality::HighSpeed;
				 g->InterpolationMode = InterpolationMode::NearestNeighbor;
				 g->SmoothingMode = SmoothingMode::None;

				 VideoFrame ^currentFrame = videoPlayer->decodedFrames->Take();

				 if(skipFrame == false) {

					 g->DrawImage(currentFrame->Bitmap, canvas);

				 } else {

					 Util::DebugOut(videoPlayer->TimeNow.ToString() + ": Skipping frame");
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
				 double actualDelay = frameTimer - videoPlayer->TimeNow;

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
private: System::Void videoDecoderBW_DoWork(System::Object^  sender, System::ComponentModel::DoWorkEventArgs^  e) {
			 			
				frameTimer = videoPlayer->TimeNow;

				while(videoPlayer->decodeFrame() && !videoDecoderBW->CancellationPending) {

					int i = 0;
				}
				
				int i = 0;
		 }
private: System::Void videoRefreshTimer_Tick(System::Object^  sender, System::EventArgs^  e) {

			 
			 videoRefreshTimer->Stop();
			 
			 // redraw screen
			 this->Invalidate();
		 }
};
}
