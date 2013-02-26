#pragma once

#include "Frame.h"

using namespace System;
using namespace Microsoft::DirectX::Direct3D;
using namespace Microsoft::DirectX;
using namespace System::Diagnostics;

namespace VideoLib {

	public ref class VideoFrame : public Frame
	{
	public:

		enum class VideoFrameType {
			D3D_SURFACE,
			MEMORY
		};

	private:

		Surface ^frame;

		BYTE *frameData;
		int width;
		int height;

		VideoFrameType videoFrameType;

		static Format makeFourCC(int ch0, int ch1, int ch2, int ch3)
		{
			int value = (int)(char)(ch0)|((int)(char)(ch1) << 8)| ((int)(char)(ch2) << 16) | ((int)(char)(ch3) << 24);
			return((Format) value);
		}

	public:

		property Surface ^Image {

			void set(Surface ^frame) {
				this->frame = frame;
			}

			Surface ^get() {
				return(frame);
			}
		}

		property int SizeBytes {

			int get() {

				int ySizeBytes = width * height;
				int vSizeBytes = (width * height) / 4;
				int uSizeBytes = (width * height) / 4;

				return(ySizeBytes + vSizeBytes + uSizeBytes);
			}
		}

		property int Width {

			int get() {

				return(width);
			}
		}

		property int Height {

			int get() {

				return(height);
			}
		}

		VideoFrame(int width, int height, Device ^device) :
			Frame(FrameType::VIDEO)
		{
			this->width = width;
			this->height = height;

			if(device != nullptr) {
				Format pixelFormat = makeFourCC('Y', 'V', '1', '2');

				frame = device->CreateOffscreenPlainSurface(width, height, pixelFormat, 
					Pool::Default);

				videoFrameType = VideoFrameType::D3D_SURFACE;


				frameData = NULL;

			} else {


				frame = nullptr;

				videoFrameType = VideoFrameType::MEMORY;

				frameData = new BYTE[SizeBytes];

			}

		}

		~VideoFrame() {

			if(frame != nullptr) {

				delete frame;
			}

			if(frameData != NULL) {

				delete frameData;
			}
		}

		void copyFrameData(BYTE* Y, BYTE* V, BYTE* U)
		{

			BYTE* pict;
			int pitch;
	
			if(videoFrameType == VideoFrameType::D3D_SURFACE) {

				Drawing::Rectangle rect = Drawing::Rectangle(0, 0, width, height);

				// copy raw frame data to bitmap
				
				GraphicsStream ^stream = frame->LockRectangle(rect, LockFlags::None, pitch);

				pict = (BYTE*)stream->InternalDataPointer;

			} else {

				pitch = width;
				pict = frameData;
			}
			
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
			
			if(videoFrameType == VideoFrameType::D3D_SURFACE) {
				frame->UnlockRectangle();
			}

		}

		void copyFrameDataToSurface(Surface ^frame) {

			Debug::Assert(videoFrameType == VideoFrameType::MEMORY);
			Debug::Assert(frame != nullptr && frame->Description.Width == width &&
				frame->Description.Height == height);

			Drawing::Rectangle rect = Drawing::Rectangle(0, 0, width, height);

			// copy raw frame data to bitmap
			int pitch;
			GraphicsStream ^stream = frame->LockRectangle(rect, LockFlags::None, pitch);

			Byte *pict = (BYTE*)stream->InternalDataPointer;

			if(width == pitch) {

				memcpy(pict, frameData, SizeBytes);

			} else {

				BYTE *pos = frameData;

				for (int y = 0 ; y < height ; y++)
				{
					memcpy(pict, pos, width);
					pict += pitch;
					pos += width;
				}
				for (int y = 0 ; y < height / 2 ; y++)
				{
					memcpy(pict, pos, width / 2);
					pict += pitch / 2;
					pos += width / 2;
				}
				for (int y = 0 ; y < height / 2; y++)
				{
					memcpy(pict, pos, width / 2);
					pict += pitch / 2;
					pos += width / 2;
				}

			}

			frame->UnlockRectangle();
		}

		
	};
}