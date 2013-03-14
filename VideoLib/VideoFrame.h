#pragma once

#include "Frame.h"
#include "VideoDecoder.h"

using namespace System;
using namespace Microsoft::DirectX::Direct3D;
using namespace Microsoft::DirectX;
using namespace System::Diagnostics;
using namespace System::Drawing;

namespace VideoLib {

	public ref class VideoFrame : public Frame
	{
		
	private:

		BYTE *frameData;
		int width;
		int height;

	public:

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

		VideoFrame(int width, int height) :
			Frame(FrameType::VIDEO)
		{
			this->width = width;
			this->height = height;

			// av_malloc instead of new will align the data in memory for some added speed
			frameData = (BYTE *)av_malloc(SizeBytes);

		}

		VideoFrame(Surface ^frame) :
			Frame(FrameType::VIDEO) 
		{

			this->width = frame->Description.Width;
			this->height = frame->Description.Height;

			frameData = (BYTE *)av_malloc(SizeBytes);

			copySurfaceToFrameData(frame);
		}

		~VideoFrame() {

			if(frameData != NULL) {

				av_free(frameData);				
				frameData = NULL;
			}
		}

		void setFrameData(BYTE* Y, BYTE* V, BYTE* U)
		{
			BYTE* pict = frameData;
			
			int ySizeBytes = width * height;
			int vSizeBytes = (width * height) / 4;
			int uSizeBytes = (width * height) / 4;

			memcpy(pict, Y, ySizeBytes);
			memcpy(pict + ySizeBytes, V, vSizeBytes);
			memcpy(pict + ySizeBytes + vSizeBytes, U, uSizeBytes);
			
		}

		void copySurfaceToFrameData(Surface ^frame) {

			Debug::Assert(frame != nullptr && frame->Description.Width == width &&
				frame->Description.Height == height);

			Drawing::Rectangle rect = Drawing::Rectangle(0, 0, width, height);

			// copy raw frame data to bitmap
			int pitch;
			GraphicsStream ^stream = frame->LockRectangle(rect, LockFlags::ReadOnly, pitch);

			Byte *source = (BYTE*)stream->InternalDataPointer;
			Byte *dest = frameData; 

			int nrRows =  SizeBytes / Width;

			for(int i = 0; i < nrRows; i++) {

				memcpy(dest, source, Width);
				dest += Width;
				source += pitch;
			}
			
			frame->UnlockRectangle();

		}

		void copyFrameDataToSurface(Surface ^frame) {

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