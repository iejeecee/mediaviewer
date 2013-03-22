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

		bool requiresAVPictureFree;
		
	public:

		property int SizeBytes {

			int get() {

				int ySizeBytes = Width * Height;
				int vSizeBytes = (Width * Height) / 4;
				int uSizeBytes = (Width * Height) / 4;

				return(ySizeBytes + vSizeBytes + uSizeBytes);
			}
		}

		property int Width {

			int get() {

				return(AVLibFrameData->width);
			}
		}

		property int Height {

			int get() {

				return(AVLibFrameData->height);
			}
		}

		VideoFrame() :
			Frame(FrameType::VIDEO)
		{
			
			requiresAVPictureFree = false;
		}

		VideoFrame(int width, int height, PixelFormat format) :
			Frame(FrameType::VIDEO)
		{

			avpicture_alloc((AVPicture*)AVLibFrameData, format, width, height);
			AVLibFrameData->width = width;
			AVLibFrameData->height = height;
			AVLibFrameData->format = format;

			requiresAVPictureFree = true;
/*
			int numBytes = avpicture_get_size(format, width, height);
			uint8_t *data = (uint8_t*)av_malloc(numBytes);

			avpicture_fill((AVPicture*)AVLibFrameData, data, format, width, height);
*/
		}

		~VideoFrame() {

			if(requiresAVPictureFree) {

				avpicture_free((AVPicture*)AVLibFrameData);
			}
		}

		void copyFrameDataToSurface(Surface ^frame) {

			Debug::Assert(frame != nullptr && frame->Description.Width == Width &&
				frame->Description.Height == Height);

			Drawing::Rectangle rect = Drawing::Rectangle(0, 0, Width, Height);

			// copy raw frame data to bitmap
			int pitch;
			GraphicsStream ^stream = frame->LockRectangle(rect, LockFlags::None, pitch);

			Byte *pict = (BYTE*)stream->InternalDataPointer;

			Byte *Y = AVLibFrameData->data[0];
			Byte *U = AVLibFrameData->data[1];
			Byte *V = AVLibFrameData->data[2];

			for (int y = 0 ; y < Height ; y++)
			{
				memcpy(pict, Y, Width);
				pict += pitch;
				Y += Width;
			}
			for (int y = 0 ; y < Height / 2 ; y++)
			{
				memcpy(pict, V, Width / 2);
				pict += pitch / 2;
				V += Width / 2;
			}
			for (int y = 0 ; y < Height / 2; y++)
			{
				memcpy(pict, U, Width / 2);
				pict += pitch / 2;
				U += Width / 2;
			}

			
			frame->UnlockRectangle();
		}

		
	};
}