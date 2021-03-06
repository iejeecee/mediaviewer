#pragma once

#include "Frame.h"
#include "..\Video\VideoDecoder.h"

using namespace System;
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
/*
			int numBytes = avpicture_get_size(format, width, height);
			uint8_t *data = (uint8_t*)av_malloc(numBytes);

			avpicture_fill((AVPicture*)AVLibFrameData, data, format, width, height);
*/
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
/*		
		void copyFrameDataTexture(cli::array<SharpDX::Direct3D10::Texture2D ^> ^texture) {
			
			Debug::Assert(texture[0] != nullptr && texture[0]->Description.Width == Width &&
					texture[0]->Description.Height == Height);			

			if(AVLibFrameData->format == PIX_FMT_YUV420P) {				

				SharpDX::DataRectangle data = texture[0]->Map(0, SharpDX::Direct3D10::MapMode::WriteDiscard, SharpDX::Direct3D10::MapFlags::None);

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

				texture[0]->Unmap(D3D10CalcSubresource(0,0,1));

				data = texture[1]->Map(0, SharpDX::Direct3D10::MapMode::WriteDiscard, SharpDX::Direct3D10::MapFlags::None);

				pict = (BYTE*)data.DataPointer.ToPointer();

				for (int y = 0 ; y < Height / 2 ; y++)
				{
					memcpy(pict, V, Width / 2);
					pict += data.Pitch;
					V += Width / 2;
				}

				texture[1]->Unmap(D3D10CalcSubresource(0,0,1));

				data = texture[2]->Map(0, SharpDX::Direct3D10::MapMode::WriteDiscard, SharpDX::Direct3D10::MapFlags::None);

				pict = (BYTE*)data.DataPointer.ToPointer();

				for (int y = 0 ; y < Height / 2; y++)
				{
					memcpy(pict, U, Width / 2);
					pict += data.Pitch;
					U += Width / 2;
				}

				texture[2]->Unmap(D3D10CalcSubresource(0,0,1));

			} else {
				
				SharpDX::DataRectangle data = texture[0]->Map(0, SharpDX::Direct3D10::MapMode::WriteDiscard, SharpDX::Direct3D10::MapFlags::None);

				Byte *pict = (BYTE*)data.DataPointer.ToPointer();

				Byte *source = AVLibFrameData->data[0];
						
				for (int y = 0 ; y < Height ; y++)
				{
					memcpy(pict, source, AVLibFrameData->linesize[0]);
					pict += data.Pitch;
					source += AVLibFrameData->linesize[0];
				}

				texture[0]->Unmap(D3D10CalcSubresource(0,0,1));
			}

			
		}
*/		
	};
}