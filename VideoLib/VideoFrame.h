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

			//frameData = new BYTE[SizeBytes];
			frameData = (BYTE *)av_malloc(SizeBytes);

		}

		VideoFrame(Surface ^frame) :
			Frame(FrameType::VIDEO)
		{

			this->width = frame->Description.Width;
			this->height = frame->Description.Height;

			//frameData = new BYTE[SizeBytes];
			frameData = (BYTE *)av_malloc(SizeBytes);

			int pitch;
			System::Drawing::Rectangle rect(0,0,Width, Height);

			GraphicsStream ^stream = frame->LockRectangle(rect, LockFlags::ReadOnly, pitch);

			Byte *dest = frameData;
			Byte *source = (BYTE*)stream->InternalDataPointer;

			int nrRows = SizeBytes / Width;

			for(int i = 0; i < nrRows; i++) {

				memcpy(dest, source, Width);
				dest += Width;
				source += pitch;
			}

			frame->UnlockRectangle();
		}

		~VideoFrame() {

			if(frameData != NULL) {

				av_free(frameData);				
				//delete frameData;
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

		void saveToDisk(String ^fileName) {

			AVFrame *source = avcodec_alloc_frame();
			if(source == NULL) {

				throw gcnew VideoLib::VideoLibException("Unable to allocate frame memory");
			}

			avcodec_get_frame_defaults(source);

			avpicture_fill((AVPicture *)source, frameData, PIX_FMT_YUV420P, 
				Width, Height);

			source->width = Width;
			source->height = Height;
			source->format = PIX_FMT_YUV420P;
			//uint8_t *temp = source->data[1];
			//source->data[1] = source->data[2];
			//source->data[2] = temp;

/*
			int bufSize = avpicture_fill((AVPicture *)source, NULL, PIX_FMT_YUV420P, 
				Width, Height);
			uint8_t *sourceBuffer = (uint8_t*)av_malloc(bufSize);
			avpicture_fill((AVPicture *)source, sourceBuffer , PIX_FMT_YUV420P, 
				Width, Height);
			
			int pitch;
			BYTE *pos = frameData;

			BYTE *Y = source->data[0];
			BYTE *U = source->data[1];
			BYTE *V = source->data[2];

			pitch = source->linesize[0];

			for(int y = 0 ; y < Height ; y++)
			{
				memcpy(Y, pos, Width);
				Y += pitch;
				pos += Width;
			}

			//pitch = source->linesize[1];

			for(int y = 0 ; y < height / 2 ; y++)
			{
				memcpy(U, pos, Width / 2);
				U += pitch / 2;
				pos += Width / 2;
			}

			//pitch = source->linesize[2];

			for(int y = 0 ; y < height / 2; y++)
			{
				memcpy(V, pos, Width / 2);
				V += pitch / 2;
				pos += Width / 2;
			}
*/
			AVFrame *dest = VideoDecoder::convertFrame(source, PIX_FMT_RGB24, 
				Width, Height, VideoDecoder::SPLINE);

			Bitmap ^image = gcnew Bitmap(Width, Height, dest->linesize[0],
				Imaging::PixelFormat::Format24bppRgb, IntPtr(dest->data[0]));

			image->Save(fileName);

			delete image;

			av_free(source);
			//av_free(sourceBuffer);

			avpicture_free((AVPicture *)dest);
		}

		
	};
}