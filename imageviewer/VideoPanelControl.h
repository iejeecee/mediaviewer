#pragma once
#include "ImageUtils.h"

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
			videoPlayer->FrameDecoded += gcnew EventHandler<EventArgs ^>(this, &VideoPanelControl::videoPlayer_FrameDecoded);
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
	private: System::ComponentModel::BackgroundWorker^  backgroundWorker;
	protected: 

	protected: 

	protected: 

	protected: 

	private:
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System::ComponentModel::Container ^components;

#pragma region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent(void)
		{
			this->backgroundWorker = (gcnew System::ComponentModel::BackgroundWorker());
			this->SuspendLayout();
			// 
			// backgroundWorker
			// 
			this->backgroundWorker->WorkerReportsProgress = true;
			this->backgroundWorker->WorkerSupportsCancellation = true;
			this->backgroundWorker->DoWork += gcnew System::ComponentModel::DoWorkEventHandler(this, &VideoPanelControl::backgroundWorker_DoWork);
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
/*
		Device ^device;

		bool initializeD3D() {

			try
			{
				// Now  setup our D3D stuff
				PresentParameters ^presentParams = gcnew PresentParameters();
				presentParams->Windowed = true;
				presentParams->SwapEffect = SwapEffect::Discard;
				device = gcnew Device(0, DeviceType::Hardware, this, 
					CreateFlags::SoftwareVertexProcessing, presentParams);
				return true;
			}
			catch (DirectXException ^)
			{ 
				return false; 
			}
		}
*/

		void videoPlayer_FrameDecoded(Object ^sender, EventArgs ^e) {

			//videoPlayer->FrameData->Save("c:\\test.jpg");
			this->Invalidate();
		}

	public:
		void loadVideo(String ^location) {

			backgroundWorker->RunWorkerAsync(location);
		

			//mediaPlayer->URL = location;
/*			
			if(video == nullptr) {

				video = gcnew Video(location);

			} else {

				video->Open(location);
			}
		
			video->Owner = videoPictureBox;

			int scaledWidth, scaledHeight;

			ImageUtils::stretchRectangle(video->Size.Width, video->Size.Height,
				Width,Height,scaledWidth, scaledHeight);

			Rectangle canvas = ImageUtils::centerRectangle(Rectangle(0,0,Width,Height),
				Rectangle(0,0,scaledWidth,scaledHeight));

			videoPictureBox->Location = Point(canvas.X, canvas.Y);
			videoPictureBox->Size = System::Drawing::Size(canvas.Width, canvas.Height);

			//video->Size = System::Drawing::Size(scaledWidth, scaledHeight);
			

			video->Play();
*/		


		}
	private: System::Void videoPanelControl_Paint(System::Object^  sender, System::Windows::Forms::PaintEventArgs^  e) {

				 while(videoPlayer->Width == 0) {}

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

				Bitmap ^frameData = videoPlayer->decodedFrames->Take();

				g->DrawImage(frameData, canvas);

				videoPlayer->freeFrames->Put(frameData);
				 
			 }
private: System::Void backgroundWorker_DoWork(System::Object^  sender, System::ComponentModel::DoWorkEventArgs^  e) {

				String ^location = dynamic_cast<String ^>(e->Argument);

			 	videoPlayer->open(location);
				videoPlayer->play();

		 }
};
}
