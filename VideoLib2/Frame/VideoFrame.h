#pragma once

#include "Frame.h"
#include "..\Video\VideoDecoder.h"

using namespace System;
using namespace System::Diagnostics;

namespace VideoLib2 {

	/*public ref class VideoFrame : public Frame
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

		property bool IsKey
		{
			bool get() {

				return(AVLibFrameData->key_frame == 1 ? true : false);
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

		}

		!VideoFrame() {

			if(hasAllocatedOwnBuffers && avFrame != NULL) {

				avpicture_free((AVPicture*)avFrame);
				avFrame = NULL;
			}
		}

		~VideoFrame() {
			
			this->!VideoFrame();
		}


		static void copyBufferToSurface(cli::array<byte> ^buffer, IntPtr ^surfaceData, int width, int height, int pitch) 
		{
						
			Byte *pict = (BYTE*)surfaceData->ToPointer();

			int Y = 0;
			
			for (int y = 0 ; y < height ; y++)
			{
				Marshal::Copy(buffer, Y, IntPtr(pict), width);
				pict += pitch;				
				Y += width;
			}
			for (int y = 0 ; y < height / 2 ; y++)
			{
				Marshal::Copy(buffer, Y, IntPtr(pict), width / 2);
				pict += pitch / 2;			
				Y += width / 2;
			}
			for (int y = 0 ; y < height / 2; y++)
			{
				Marshal::Copy(buffer, Y, IntPtr(pict), width / 2);
				pict += pitch / 2;			
				Y += width / 2;
			}

			
		}

		static void copySurfaceToBuffer(IntPtr ^surfaceData, int width, int height, int pitch, cli::array<byte> ^buffer) {
					
			Byte *pict = (BYTE*)surfaceData->ToPointer();

			int Y = 0;
			
			for (int y = 0 ; y < height ; y++)
			{
				Marshal::Copy(IntPtr(pict), buffer, Y, width);
				pict += pitch;				
				Y += width;
			}
			for (int y = 0 ; y < height / 2 ; y++)
			{
				Marshal::Copy(IntPtr(pict), buffer, Y, width / 2);
				pict += pitch / 2;			
				Y += width / 2;
			}
			for (int y = 0 ; y < height / 2; y++)
			{
				Marshal::Copy(IntPtr(pict), buffer, Y, width / 2);
				pict += pitch / 2;			
				Y += width / 2;
			}			
		}

		void copyFrameDataToSurface(IntPtr ^surfaceData, int pitch) {
		
			Byte *pict = (BYTE*)surfaceData->ToPointer();

			Byte *Y = AVLibFrameData->data[0];
			Byte *U = AVLibFrameData->data[1];
			Byte *V = AVLibFrameData->data[2];
						
			AVFrame *temp = AVLibFrameData;

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
						
		}

	};*/
}