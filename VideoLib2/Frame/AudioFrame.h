#pragma once

#include "Frame.h"
#include "..\Video\VideoDecoder.h"

#define AVCODEC_MAX_AUDIO_FRAME_SIZE 192000 

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::IO;

namespace VideoLib2 {

	/*public ref class AudioFrame : public Frame
	{
	private:
	
		System::IO::MemoryStream ^stream;
		array<unsigned char> ^data;
		int length;

		bool hasAllocatedOwnBuffers;

	public:


		AudioFrame() : Frame(FrameType::AUDIO) {

			data = gcnew array<unsigned char>(AVCODEC_MAX_AUDIO_FRAME_SIZE * 2);
			stream = gcnew System::IO::MemoryStream(data);
			length = 0;
		
			hasAllocatedOwnBuffers = false;
		}

		// only packed audio types should be used as format
		AudioFrame(AVSampleFormat format, int64_t channelLayout, int sampleRate) :
			Frame(FrameType::AUDIO)
		{

			data = gcnew array<unsigned char>(AVCODEC_MAX_AUDIO_FRAME_SIZE * 2);
			stream = gcnew System::IO::MemoryStream(data);
			length = 0;

			// planar types should allocate more data planes
			// here
			AVLibFrameData->data[0] = (uint8_t *)av_malloc(AVCODEC_MAX_AUDIO_FRAME_SIZE * 2);

			AVLibFrameData->format = format;
			AVLibFrameData->channel_layout = channelLayout;
			AVLibFrameData->sample_rate = sampleRate;
		
			hasAllocatedOwnBuffers = true;
		}

		!AudioFrame() {

			if(hasAllocatedOwnBuffers == true && avFrame->data[0] != NULL) {

				av_free(avFrame->data[0]);
				avFrame->data[0] = NULL;
			}

		}

		~AudioFrame() {

			if(stream != nullptr) {

				delete stream;
				stream = nullptr;
			}

			this->!AudioFrame();
		}

		property System::IO::MemoryStream ^Stream {

			System::IO::MemoryStream ^get() {

				return(stream);
			}

		}

		property array<unsigned char> ^Data {

			array<unsigned char> ^get() {

				return(data);
			}

		}

		property int Length {
	
			void set(int length) {

				this->length = length;
			}

			int get() {

				return(length);
			}
		
		}

		void copyAudioDataToManagedMemory()
		{			

			if(Length > 0) {
				
				Marshal::Copy(IntPtr(AVLibFrameData->data[0]), data, 0, Length);

			} else {

				// incorrect audio, set to silence
				this->length = 4608;
				Array::Clear(data,0,data->Length);

			}

			stream->Position = 0;
			
		}
	};*/
}