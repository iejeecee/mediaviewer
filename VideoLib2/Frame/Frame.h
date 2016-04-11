#pragma once
#include "..\Video\Video.h"

#define AVCODEC_MAX_AUDIO_FRAME_SIZE 192000 

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::IO;

namespace VideoLib2 {

	public ref class Frame 
	{
	public:

		enum class FrameType {

			EMPTY,
			AUDIO,
			VIDEO
		};

		static int MaxAudioFrameSize = AVCODEC_MAX_AUDIO_FRAME_SIZE * 2;

	protected:

		AVFrame *avFrame;

		double pts,dts;
		int sizeBytes;
		
	public:

		void setFrameDefaults() {

			av_frame_unref(AVLibFrameData);	
			sizeBytes = 0;
		}

		property AVFrame *AVLibFrameData
		{			
			AVFrame *get() {

				return(avFrame);
			}			
		}

		Frame() {

			avFrame = av_frame_alloc();
					
			pts = 0;
			dts = 0;
			sizeBytes = 0;
		}

		!Frame() {

			if(avFrame != NULL) {

				av_free(avFrame);
				avFrame = NULL;
			}
		}

		~Frame() {

			this->!Frame();
		}

		// presentation timestamp of the current frame in seconds, shifted by stream start_time
		property double Pts {

			void set(double pts) {
				this->pts = pts;
			}

			double get() {
				return(pts);
			}
		}

		// decoding timestamp of the current frame in seconds, shifted by stream start_time
		property double Dts {

			void set(double dts) {
				this->dts = dts;
			}

			double get() {
				return(dts);
			}
		}

		property long FramePts {

			long get()
			{ 
				return(avFrame->pts);
			}
		}
		
		property FrameType Type
		{
			FrameType get() {

				if(avFrame->width == 0 && avFrame->sample_rate == 0) {

					return(FrameType::EMPTY);
				}

				return(avFrame->width != 0 ? FrameType::VIDEO : FrameType::AUDIO);
			}

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

		void copyVideoDataToSurface(IntPtr ^surfaceData, int pitch) {
		
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

		void copyAudioDataToManagedMemory(array<unsigned char> ^data)
		{			

			if(SizeBytes > 0) {
				
				Marshal::Copy(IntPtr(AVLibFrameData->data[0]), data, 0, SizeBytes);

			} else {

				// incorrect audio, set to silence
				SizeBytes = 4608;
				Array::Clear(data,0,data->Length);

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

		property int SizeBytes {

			void set(int value)
			{
				sizeBytes = value;
			}

			int get() {

				if(Type == FrameType::VIDEO) {

					int ySizeBytes = Width * Height;
					int vSizeBytes = (Width * Height) / 4;
					int uSizeBytes = (Width * Height) / 4;

					return(ySizeBytes + vSizeBytes + uSizeBytes);

				} else {

					return(sizeBytes);
				}
			}
		}

	};
}