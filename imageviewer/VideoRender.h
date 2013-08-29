#pragma once
//http://www.drunkenhyena.com/cgi-bin/view_net_article.pl?chapter=2;article=10#Lost_Devices
#include "ImageUtils.h"
#include "HRTimer.h"
#include "FileUtils.h"
#include "Settings.h"

namespace imageviewer {

using namespace VideoLib;

namespace D3D = SharpDX::Direct3D9;

public ref class VideoRender
{

private:

	static log4net::ILog ^log = log4net::LogManager::GetLogger(System::Reflection::MethodBase::GetCurrentMethod()->DeclaringType);

	D3D::Direct3D ^direct3D;
	D3D::Device ^device;
	D3D::Surface ^offscreen;
	D3D::Surface ^screenShot;
	VideoFrame ^saveOffscreen;

	Rectangle canvas;

	int videoWidth;
	int videoHeight;

	bool windowed;
	Windows::Forms::Control ^owner;

	static D3D::Format makeFourCC(int ch0, int ch1, int ch2, int ch3)
	{
		int value = (int)(char)(ch0)|((int)(char)(ch1) << 8)| ((int)(char)(ch2) << 16) | ((int)(char)(ch3) << 24);
		return((D3D::Format) value);
	}

	array<D3D::PresentParameters> ^createPresentParams(bool windowed, Windows::Forms::Control ^owner) {

		array<D3D::PresentParameters> ^presentParams = gcnew array<D3D::PresentParameters>(1);

		//No Z (Depth) buffer or Stencil buffer
		presentParams[0].EnableAutoDepthStencil = false;

		//multiple backbuffers for a flipchain
		presentParams[0].BackBufferCount = 3;

		//Set our Window as the Device Window
		presentParams[0].DeviceWindowHandle = owner->Handle;

		//wait for VSync
		presentParams[0].PresentationInterval = D3D::PresentInterval::One;

		//flip frames on vsync
		presentParams[0].SwapEffect = D3D::SwapEffect::Discard;

		//Set Windowed vs. Full-screen
		presentParams[0].Windowed = windowed;

		//We only need to set the Width/Height in full-screen mode
		if(!windowed) {

			presentParams[0].BackBufferHeight = owner->Height;
			presentParams[0].BackBufferWidth = owner->Width;

			D3D::Format format = D3D::Format::X8R8G8B8;

			//Choose a compatible 16-bit mode.
			presentParams[0].BackBufferFormat = format;

		} else {

			presentParams[0].BackBufferHeight = 0;
			presentParams[0].BackBufferWidth = 0;
			presentParams[0].BackBufferFormat = D3D::Format::Unknown;
		}

		return(presentParams);
	}

	void resetDevice() {

		device->Reset(createPresentParams(windowed, owner));
	}


	void aquireResources() {

		if(videoWidth != 0 && videoHeight != 0) {

			D3D::Format pixelFormat = makeFourCC('Y', 'V', '1', '2');

			offscreen = D3D::Surface::CreateOffscreenPlain(device, 
				videoWidth, 
				videoHeight, 
				pixelFormat, 
				D3D::Pool::Default);

			screenShot = D3D::Surface::CreateOffscreenPlain(device, 
				videoWidth, 
				videoHeight, 
				D3D::Format::A8R8G8B8,
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

	void device_DeviceResizing(Object ^sender, System::ComponentModel::CancelEventArgs ^e) {

		e->Cancel = true;

	}

	void device_DeviceReset(Object ^sender, EventArgs ^e) {

		log->Info("d3d device reset");

		aquireResources();
	
	}

	void device_DeviceLost(Object ^sender, EventArgs ^e) {

		log->Info("d3d device lost");
	
		releaseResources();

	}

public: 

	enum class RenderMode {
		NORMAL,
		CLEAR_SCREEN,
		PAUSED
	};

	VideoRender(Windows::Forms::Control ^owner)
	{
		direct3D = nullptr;
		device = nullptr;
		this->owner = owner;
		saveOffscreen = nullptr;
		windowed = true;
	}

	~VideoRender() {

		releaseResources();

		if(saveOffscreen != nullptr) {

			delete saveOffscreen;
		}

		if(device != nullptr) {

			delete device;
		}

		if(direct3D != nullptr) {

			delete direct3D;
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

	void setWindowed() {

		windowed = true;

		if(device != nullptr) {

			device->Reset(createPresentParams(windowed, owner));
		}
	}


	void setFullScreen() {

		windowed = false;

		if(device != nullptr) {

			try {

				device->Reset(createPresentParams(windowed, owner));

			} catch (Exception ^e) {

				log->Error("Error setting fullscreen", e);
			}
		}
	}

	void initialize(int videoWidth, int videoHeight) 
	{
		try {

			this->videoHeight = videoHeight;
			this->videoWidth = videoWidth;

			if(direct3D == nullptr) {

				SharpDX::Result resultCode;

				direct3D = gcnew D3D::Direct3D();

				if(direct3D->CheckDeviceFormatConversion(
					0,
					D3D::DeviceType::Hardware,
					makeFourCC('Y', 'V', '1', '2'),
					D3D::Format::X8R8G8B8, 
					resultCode) == false) 
				{
					throw gcnew SharpDX::SharpDXException("Video Hardware does not support YV12 format conversion");						
				}

				array<D3D::PresentParameters> ^presentParams = 
					createPresentParams(windowed, owner);

				device = gcnew D3D::Device(direct3D,
					0,
					D3D::DeviceType::Hardware,  
					owner->Handle,                  
					D3D::CreateFlags::SoftwareVertexProcessing,
					presentParams);       

				//device->DeviceLost += gcnew EventHandler(this, &VideoRender::device_DeviceLost);
				//device->DeviceReset += gcnew EventHandler(this, &VideoRender::device_DeviceReset);
				//device->DeviceResizing += gcnew System::ComponentModel::CancelEventHandler(this, &VideoRender::device_DeviceResizing);
			}

			releaseResources();
			aquireResources();

			D3D::Surface ^backBuffer = device->GetBackBuffer(0, 0);
			
			canvas = Rectangle(0, 0, backBuffer->Description.Width,
				backBuffer->Description.Height);

			log->Info("Direct3D Initialized");
	
		} catch (SharpDX::SharpDXException ^e){

			log->Error("Direct3D Initialization error", e);
			MessageBox::Show(e->Message, "Direct3D Initialization error");
			
		}

	}

	void createScreenShot(String ^fileName) {

		if(device == nullptr) return;

		try {

			int width = offscreen->Description.Width;
			int height = offscreen->Description.Height;

			SharpDX::Rectangle videoRect = SharpDX::Rectangle(0, 0, width, height);

			device->StretchRectangle(offscreen, videoRect,
				screenShot, videoRect, D3D::TextureFilter::Linear);	

			SharpDX::DataRectangle ^stream = screenShot->LockRectangle(videoRect, D3D::LockFlags::ReadOnly);

			Bitmap ^image = gcnew Bitmap(width, height, stream->Pitch,
				Imaging::PixelFormat::Format32bppArgb, stream->DataPointer);

			String ^path = Util::getPathWithoutFileName(fileName);
			fileName = System::IO::Path::GetFileNameWithoutExtension(fileName);
			fileName += "." + Settings::getVar(Settings::VarName::VIDEO_SCREENSHOT_FILE_TYPE);

			fileName = FileUtils::getUniqueFileName(path + "\\" + fileName);

			image->Save(fileName);

			screenShot->UnlockRectangle();		

		} catch (Exception ^e) {

			log->Error("Screenshot failed", e);
			MessageBox::Show("Screenshot failed: " + e->Message, "Error");
		}
	
	}

	void display(VideoFrame ^videoFrame, Rectangle canvas, Color backColor, RenderMode mode) {

		if(device == nullptr) return;

		SharpDX::Result deviceStatus = device->TestCooperativeLevel();

		if(deviceStatus.Success == true) {

			try {

				SharpDX::ColorBGRA backColorDX = SharpDX::ColorBGRA(backColor.R,
					backColor.G, backColor.B, backColor.A);

				device->Clear(D3D::ClearFlags::Target, backColorDX, 1.0f, 0);

				if(mode == RenderMode::CLEAR_SCREEN) {

					device->Present();
					return;
				}

				device->BeginScene();		

				SharpDX::Rectangle videoRect;
				
				if(mode == RenderMode::NORMAL) {

					videoRect = SharpDX::Rectangle(0, 0, videoFrame->Width, videoFrame->Height);
				
					videoFrame->copyFrameDataToSurface(offscreen);
				
				} else if(mode == RenderMode::PAUSED) {

					videoRect = SharpDX::Rectangle(0, 0, offscreen->Description.Width,  offscreen->Description.Height);
				
				}

				D3D::Surface ^backBuffer = device->GetBackBuffer(0, 0);

				SharpDX::Rectangle canvasDx = SharpDX::Rectangle(canvas.Left,
					canvas.Top, canvas.Right, canvas.Bottom);

				device->StretchRectangle(offscreen, videoRect,
					backBuffer, canvasDx, D3D::TextureFilter::Linear);

				device->EndScene();
				device->Present();

			} catch(SharpDX::SharpDXException ^e) {

				log->Info("lost direct3d device", e);
				deviceStatus = device->TestCooperativeLevel();
			} 
		}

		if(deviceStatus.Code == D3D::ResultCode::DeviceLost->Result) {

			 //Can't Reset yet, wait for a bit

		} else if(deviceStatus.Code == D3D::ResultCode::DeviceNotReset->Result) {
		
			if(owner->InvokeRequired) {

				owner->Invoke(gcnew Action(this, &VideoRender::resetDevice));

			} else {

				resetDevice();
			}
		}
	}


};



}