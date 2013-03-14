#pragma once
//http://www.drunkenhyena.com/cgi-bin/view_net_article.pl?chapter=2;article=10#Lost_Devices
#include "ImageUtils.h"
#include "HRTimer.h"

namespace imageviewer {

using namespace VideoLib;
using namespace Microsoft::DirectX;
namespace D3D = Microsoft::DirectX::Direct3D;

public ref class VideoRender
{

private:

	D3D::Device ^device;
	D3D::Surface ^offscreen;
	D3D::Surface ^screenShot;
	VideoFrame ^tempOffscreen;

	Rectangle canvas;

	int videoWidth;
	int videoHeight;

	Windows::Forms::Control ^owner;

	static D3D::Format makeFourCC(int ch0, int ch1, int ch2, int ch3)
	{
		int value = (int)(char)(ch0)|((int)(char)(ch1) << 8)| ((int)(char)(ch2) << 16) | ((int)(char)(ch3) << 24);
		return((D3D::Format) value);
	}

	D3D::PresentParameters ^createPresentParams() {

		//Assume this is pre-initialized to your choice of full-screen or windowed mode.
		bool fullScreen = false;

		//Hard-coded to a common format.  A better method will be shown later in this lesson.
		D3D::Format format = D3D::Format::R8G8B8;

		//Allocate our class
		D3D::PresentParameters ^presentParams = gcnew D3D::PresentParameters();

		//No Z (Depth) buffer or Stencil buffer
		presentParams->EnableAutoDepthStencil = false;

		//multiple backbuffers for a flipchain
		presentParams->BackBufferCount = 3;

		//Set our Window as the Device Window
		presentParams->DeviceWindow = owner;

		//wait for VSync
		presentParams->PresentationInterval = D3D::PresentInterval::One;

		//flip frames on vsync
		presentParams->SwapEffect = D3D::SwapEffect::Discard;

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

		return(presentParams);
	}

	void resetDevice() {

		device->Reset(createPresentParams());
	}

	void saveResources() {

		tempOffscreen = gcnew VideoFrame(offscreen);
	}

	void restoreResources() {

		tempOffscreen->copyFrameDataToSurface(offscreen);
	}

	void aquireResources() {

		if(videoWidth != 0 && videoHeight != 0) {

			D3D::Format pixelFormat = makeFourCC('Y', 'V', '1', '2');

			offscreen = device->CreateOffscreenPlainSurface(videoWidth, videoHeight, pixelFormat, 
				D3D::Pool::Default);

			screenShot = device->CreateOffscreenPlainSurface(videoWidth, videoHeight, D3D::Format::A8R8G8B8,
				D3D::Pool::Default);
		}
	}

	void releaseResources() {

		if(offscreen != nullptr) {

			delete offscreen;
			offscreen = nullptr;
		}

		if(screenShot != nullptr) {

			delete screenShot;
			screenShot = nullptr;
		}
	}

public: 

	enum class RenderMode {
		NORMAL,
		CLEAR_SCREEN,
		PAUSED
	};

	VideoRender(Windows::Forms::Control ^owner)
	{
		device = nullptr;
		this->owner = owner;
		tempOffscreen = nullptr;
	}

	~VideoRender() {

		releaseResources();

		if(tempOffscreen != nullptr) {

			delete tempOffscreen;
		}

		if(device != nullptr) {

			delete device;
		}
	}

	property D3D::Device ^Device
	{

		D3D::Device ^get() {

			return(device);
		}
	}

	property Rectangle Canvas
	{
		Rectangle get() {

			return(canvas);
		}
	}

	void initialize(int videoWidth, int videoHeight) 
	{
		try {

			this->videoHeight = videoHeight;
			this->videoWidth = videoWidth;

			D3D::PresentParameters ^presentParams = createPresentParams();

			if(device == nullptr) {

				device = gcnew D3D::Device(0,                      
					D3D::DeviceType::Hardware,  
					owner,                  
					D3D::CreateFlags::SoftwareVertexProcessing,
					presentParams);         

				device->DeviceLost += gcnew EventHandler(this, &VideoRender::device_DeviceLost);
				device->DeviceReset += gcnew EventHandler(this, &VideoRender::device_DeviceReset);
				device->DeviceResizing += gcnew System::ComponentModel::CancelEventHandler(this, &VideoRender::device_DeviceResizing);
			}

			releaseResources();
			aquireResources();

			D3D::Surface ^backBuffer = device->GetBackBuffer(0, 0, D3D::BackBufferType::Mono);
			
			canvas = Rectangle(0, 0, backBuffer->Description.Width,
				backBuffer->Description.Height);

	
		} catch (D3D::GraphicsException ^exception){

			MessageBox::Show(exception->Message, "Direct3D Initialization error");
			Util::DebugOut("Error Code:" + exception->ErrorCode);
			Util::DebugOut("Error String:" + exception->ErrorString);
			Util::DebugOut("Message:" + exception->Message);
			Util::DebugOut("StackTrace:" + exception->StackTrace);
		}

	}

	void createScreenShot() {

		if(device == nullptr) return;

		try {

			int width = offscreen->Description.Width;
			int height = offscreen->Description.Height;

			Rectangle videoRect = Rectangle(0, 0, width, height);

			device->StretchRectangle(offscreen, videoRect,
				screenShot, videoRect, D3D::TextureFilter::Linear);	

			int pitch;

			GraphicsStream ^stream = screenShot->LockRectangle(videoRect, D3D::LockFlags::ReadOnly,
				pitch);

			Bitmap ^image = gcnew Bitmap(width, height, pitch,
				Imaging::PixelFormat::Format32bppArgb, IntPtr(stream->InternalDataPointer));

			image->Save("c:\\screenshot.png");

			screenShot->UnlockRectangle();		

		} catch (Exception ^e) {

			MessageBox::Show("Screenshot failed: " + e->Message, "Error");
		}
		//VideoFrame ^screen = gcnew VideoFrame(offscreen);
		//screen->saveToDisk("c:\\screenshot.png");

	}

	void display(VideoFrame ^videoFrame, Rectangle canvas, Color backColor, RenderMode mode) {

		if(device == nullptr) return;

		int deviceStatus;

		if(device->CheckCooperativeLevel(deviceStatus) == true) {

			try {

				device->Clear(D3D::ClearFlags::Target, backColor, 1.0f, 0);

				if(mode == RenderMode::CLEAR_SCREEN) {

					device->Present();
					return;
				}

				device->BeginScene();		

				Rectangle videoRect;
				
				if(mode == RenderMode::NORMAL) {

					videoRect = Rectangle(0, 0, videoFrame->Width, videoFrame->Height);
				
					videoFrame->copyFrameDataToSurface(offscreen);
				
				} else if(mode == RenderMode::PAUSED) {

					videoRect = Rectangle(0, 0, offscreen->Description.Width,  offscreen->Description.Height);
				
				}

				D3D::Surface ^backBuffer = device->GetBackBuffer(0, 0, D3D::BackBufferType::Mono);

				device->StretchRectangle(offscreen, videoRect,
					backBuffer, canvas, D3D::TextureFilter::Linear);

				device->EndScene();
				device->Present();

			} catch(D3D::DeviceLostException ^) {

				device->CheckCooperativeLevel(deviceStatus);

			} catch(D3D::DeviceNotResetException ^) {

				device->CheckCooperativeLevel(deviceStatus);
			}
		}

		if(deviceStatus == (int)D3D::ResultCode::DeviceLost) {

			 //Can't Reset yet, wait for a bit

		} else if(deviceStatus == (int)D3D::ResultCode::DeviceNotReset) {
		
			if(owner->InvokeRequired) {

				owner->Invoke(gcnew Action(this, &VideoRender::resetDevice));

			} else {

				resetDevice();
			}
		}
	}

	void device_DeviceResizing(Object ^sender, System::ComponentModel::CancelEventArgs ^e) {

		e->Cancel = true;

	}

	void device_DeviceReset(Object ^sender, EventArgs ^e) {

		Util::DebugOut("d3d device reset");

		aquireResources();
		restoreResources();
	
	}

	void device_DeviceLost(Object ^sender, EventArgs ^e) {

		Util::DebugOut("d3d device lost");
	
		saveResources();
		releaseResources();

	}


	

};



}