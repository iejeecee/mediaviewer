#pragma once

#include "Frame.h"
#include "VideoDecoder.h"

using namespace System;
using namespace SharpDX::Direct3D9;
using namespace System::Diagnostics;
using namespace System::Drawing;

namespace VideoLib {

	public ref class VideoFrame : public Frame
	{
	private:

		bool hasAllocatedOwnBuffers;
		
		static int D3D10CalcSubresource(int MipSlice, int ArraySlice, int MipLevels) 
		{
			return(MipSlice + (ArraySlice * MipLevels));
		}
		
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
			
			hasAllocatedOwnBuffers = false;
		}

		VideoFrame(int width, int height, AVPixelFormat format) :
			Frame(FrameType::VIDEO)
		{

			avpicture_alloc((AVPicture*)AVLibFrameData, format, width, height);
			AVLibFrameData->width = width;
			AVLibFrameData->height = height;
			AVLibFrameData->format = format;

			hasAllocatedOwnBuffers = true;
/*
			int numBytes = avpicture_get_size(format, width, height);
			uint8_t *data = (uint8_t*)av_malloc(numBytes);

			avpicture_fill((AVPicture*)AVLibFrameData, data, format, width, height);
*/
		}

		~VideoFrame() {

			if(hasAllocatedOwnBuffers) {

				avpicture_free((AVPicture*)AVLibFrameData);
			}
		}

		void copyFrameDataToSurface(Surface ^frame) {

			Debug::Assert(frame != nullptr && frame->Description.Width == Width &&
				frame->Description.Height == Height);

			SharpDX::Rectangle rect = SharpDX::Rectangle(0, 0, Width, Height);

			// copy raw frame data to bitmap

			SharpDX::DataRectangle ^stream = frame->LockRectangle(rect, LockFlags::None);

			Byte *pict = (BYTE*)stream->DataPointer.ToPointer();

			Byte *Y = AVLibFrameData->data[0];
			Byte *U = AVLibFrameData->data[1];
			Byte *V = AVLibFrameData->data[2];

			for (int y = 0 ; y < Height ; y++)
			{
				memcpy(pict, Y, Width);
				pict += stream->Pitch;
				Y += Width;
			}
			for (int y = 0 ; y < Height / 2 ; y++)
			{
				memcpy(pict, V, Width / 2);
				pict += stream->Pitch / 2;
				V += Width / 2;
			}
			for (int y = 0 ; y < Height / 2; y++)
			{
				memcpy(pict, U, Width / 2);
				pict += stream->Pitch / 2;
				U += Width / 2;
			}
			
			frame->UnlockRectangle();
		}

	

		void copyFrameDataTexture(SharpDX::Direct3D10::Texture2D ^texture) {

			Debug::Assert(texture != nullptr && texture->Description.Width == Width &&
				texture->Description.Height == Height);
			
			SharpDX::DataRectangle data = texture->Map(0,SharpDX::Direct3D10::MapMode::Write,SharpDX::Direct3D10::MapFlags::None);

			Byte *pict = (BYTE*)data.DataPointer.ToPointer();

			Byte *Y = AVLibFrameData->data[0];
			Byte *U = AVLibFrameData->data[1];
			Byte *V = AVLibFrameData->data[2];

			for (int y = 0 ; y < Height ; y++)
			{
				memcpy(pict, Y, Width);
				pict += data.Pitch;
				Y += Width;
			}
			for (int y = 0 ; y < Height / 2 ; y++)
			{
				memcpy(pict, V, Width / 2);
				pict += data.Pitch / 2;
				V += Width / 2;
			}
			for (int y = 0 ; y < Height / 2; y++)
			{
				memcpy(pict, U, Width / 2);
				pict += data.Pitch / 2;
				U += Width / 2;
			}

			texture->Unmap(D3D10CalcSubresource(0,0,1));
		}
		
	};
}