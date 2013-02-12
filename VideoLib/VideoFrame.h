#pragma once

#include "Frame.h"

using namespace System;
using namespace Microsoft::DirectX::Direct3D;
using namespace Microsoft::DirectX;

namespace VideoLib {

	public ref class VideoFrame : public Frame
	{
	private:

		Surface ^frame;

	public:

		property Surface ^Image {

			void set(Surface ^frame) {
				this->frame = frame;
			}

			Surface ^get() {
				return(frame);
			}
		}

		VideoFrame(Device ^device, int width, int height, Format pixelFormat) :
			Frame(FrameType::VIDEO)
		{

			frame = device->CreateOffscreenPlainSurface(width, height, pixelFormat, 
				Pool::Default);

			
		}

		~VideoFrame() {

			if(frame != nullptr) {

				delete frame;
			}
		}

		void copyFrameData(BYTE* Y, BYTE* V, BYTE* U)
		{

			int width = frame->Description.Width;
			int height = frame->Description.Height;

			Drawing::Rectangle rect = Drawing::Rectangle(0, 0, width, height);


			// copy raw frame data to bitmap
			int pitch;
			GraphicsStream ^stream = frame->LockRectangle(rect, LockFlags::None, pitch);

			BYTE* pict = (BYTE*)stream->InternalDataPointer;

			/*
			switch(pixelFormat)
			{
			case D3DFMT_YV12:
			*/
			if(width == pitch) {

				int ySizeBytes = width * height;
				int vSizeBytes = (width * height) / 4;
				int uSizeBytes = (width * height) / 4;

				memcpy(pict, Y, ySizeBytes);
				memcpy(pict + ySizeBytes, V, vSizeBytes);
				memcpy(pict + ySizeBytes + vSizeBytes, U, uSizeBytes);

			} else {

				for (int y = 0 ; y < height ; y++)
				{
					memcpy(pict, Y, width);
					pict += pitch;
					Y += width;
				}
				for (int y = 0 ; y < height / 2 ; y++)
				{
					memcpy(pict, V, width / 2);
					pict += pitch / 2;
					V += width / 2;
				}
				for (int y = 0 ; y < height / 2; y++)
				{
					memcpy(pict, U, width / 2);
					pict += pitch / 2;
					U += width / 2;
				}
			}
			/*		
			break;

			case D3DFMT_NV12:

			for (int y = 0 ; y < newHeight ; y++)
			{
			memcpy(pict, Y, width);
			pict += pitch;
			Y += width;
			}
			for (int y = 0 ; y < newHeight / 2 ; y++)
			{
			memcpy(pict, V, width);
			pict += pitch;
			V += width;
			}
			break;

			case D3DFMT_YUY2:
			case D3DFMT_UYVY:
			case D3DFMT_R5G6B5:
			case D3DFMT_X1R5G5B5:
			case D3DFMT_A8R8G8B8:
			case D3DFMT_X8R8G8B8:

			memcpy(pict, Y, pitch * newHeight);

			break;
			}
			*/
			frame->UnlockRectangle();

		}
	};
}