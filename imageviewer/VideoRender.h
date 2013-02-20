#pragma once

namespace imageviewer {

using namespace VideoLib;
namespace D3D = Microsoft::DirectX::Direct3D;

public ref class VideoRender
{

private:

	D3D::Device ^device;
	Windows::Forms::Control ^owner;
	Windows::Forms::Control ^window;

public: 

	VideoRender(Windows::Forms::Control ^owner, Windows::Forms::Control ^window)
	{
		this->owner = owner;
		this->window = window;
		initialize();
	}

	property D3D::Device ^Device
	{

		D3D::Device ^get() {

			return(device);
		}
	}

	void initialize() 
	{
		try {
			//Assume this is pre-initialized to your choice of full-screen or windowed mode.
			bool fullScreen = false;

			//Hard-coded to a common format.  A better method will be shown later in this lesson.
			D3D::Format format = D3D::Format::R8G8B8;

			//Allocate our class
			D3D::PresentParameters ^presentParams = gcnew D3D::PresentParameters();

			//No Z (Depth) buffer or Stencil buffer
			presentParams->EnableAutoDepthStencil = false;

			//1 Back buffer for double-buffering
			presentParams->BackBufferCount = 1;

			//Set our Window as the Device Window
			presentParams->DeviceWindow = window;

			//Do not wait for VSync
			presentParams->PresentationInterval = D3D::PresentInterval::Immediate;

			//Discard old frames for better performance
			presentParams->SwapEffect = D3D::SwapEffect::Flip;

			//Set Windowed vs. Full-screen
			presentParams->Windowed = !fullScreen;

			//We only need to set the Width/Height in full-screen mode
			if(fullScreen) {

				presentParams->BackBufferHeight = owner->Height;
				presentParams->BackBufferWidth = owner->Width;

				//Choose a compatible 16-bit mode.
				presentParams->BackBufferFormat = format;

			} else {

				presentParams->BackBufferHeight = 0;
				presentParams->BackBufferWidth = 0;
				presentParams->BackBufferFormat = D3D::Format::Unknown;
			}

			device = gcnew D3D::Device(0,                       //Adapter
				D3D::DeviceType::Hardware,  //Device Type
				owner,                     //Render Window
				D3D::CreateFlags::SoftwareVertexProcessing, //behaviour flags
				presentParams);          //PresentParamters

			device->DeviceLost += gcnew EventHandler(this, &VideoRender::device_DeviceLost);
			device->DeviceReset += gcnew EventHandler(this, &VideoRender::device_DeviceReset);
			device->DeviceResizing += gcnew System::ComponentModel::CancelEventHandler(this, &VideoRender::device_DeviceResizing);

		} catch (D3D::GraphicsException ^exception){

			Util::DebugOut("Error Code:" + exception->ErrorCode);
			Util::DebugOut("Error String:" + exception->ErrorString);
			Util::DebugOut("Message:" + exception->Message);
			Util::DebugOut("StackTrace:" + exception->StackTrace);
		}

	}

	void display(VideoFrame ^videoFrame, Rectangle canvas, Color backColor) {

		device->Clear(D3D::ClearFlags::Target, backColor, 1.0f, 0);

		device->BeginScene();				
	
		D3D::Surface ^backBuffer = device->GetBackBuffer(0,0, D3D::BackBufferType::Mono);

		Rectangle videoRect = Rectangle(0, 0, videoFrame->Image->Description.Width, videoFrame->Image->Description.Height);

		device->StretchRectangle(videoFrame->Image, videoRect,
			backBuffer, canvas, D3D::TextureFilter::Linear);

		device->EndScene();
		device->Present();
	}

	void device_DeviceResizing(Object ^sender, System::ComponentModel::CancelEventArgs ^e) {

		//e->Cancel = true;
		//stop();

	}

	void device_DeviceReset(Object ^sender, EventArgs ^e) {

		//videoPlayer->initializeResources();
	}

	void device_DeviceLost(Object ^sender, EventArgs ^e) {

		if(device->CheckCooperativeLevel() == false) {

			//stop();
			//videoPlayer->disposeResources();
		}

	}


	

};



}