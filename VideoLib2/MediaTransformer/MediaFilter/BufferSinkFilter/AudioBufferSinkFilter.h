#pragma once
#include "BufferSinkFilter.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Threading;

namespace VideoLib2
{
	public ref class AudioBufferSinkFilter : BufferSinkFilter
	{

	protected:

		void init(AVSampleFormat sampleFormat, uint64_t channelLayout, int sampleRate)
		{
			int result = av_opt_set_bin(filterContext, "sample_fmts",
				(uint8_t*)&sampleFormat, sizeof(sampleFormat),
				AV_OPT_SEARCH_CHILDREN);
			if (result < 0) {

				throw gcnew VideoLib2::VideoLibException("Cannot set output sample format");					
			}

			result = av_opt_set_bin(filterContext, "channel_layouts",
				(uint8_t*)&channelLayout, sizeof(channelLayout), 
				AV_OPT_SEARCH_CHILDREN);
			if (result < 0) {

				throw gcnew VideoLib2::VideoLibException("Cannot set output channel layout");	
			}

			result = av_opt_set_bin(filterContext, "sample_rates",
				(uint8_t*)&sampleRate, sizeof(sampleRate),
				AV_OPT_SEARCH_CHILDREN);
			if (result < 0) {

				throw gcnew VideoLib2::VideoLibException("Cannot set output sample rate");			
			}

			initFilter();
		}

	public:									

		AudioBufferSinkFilter(String ^instanceName, IntPtr filterGraph, Stream *outputStream) :
			BufferSinkFilter("abuffersink", instanceName, filterGraph)
		{						
			if(!outputStream->isAudio()) {

				throw gcnew VideoLib2::VideoLibException("output stream is not a audio stream");	
			}

			AVCodecContext *audioContext = outputStream->getCodecContext();

			init(audioContext->sample_fmt, audioContext->channel_layout, audioContext->sample_rate);
			
		}

		AudioBufferSinkFilter(String ^instanceName, IntPtr filterGraph, 
			AVSampleFormat sampleFormat, uint64_t channelLayout, int sampleRate) :
			BufferSinkFilter("abuffersink", instanceName, filterGraph)
		{	
			init(sampleFormat, channelLayout, sampleRate);
		}

		void setFrameSize(unsigned int frameSize) {
			
			av_buffersink_set_frame_size(filterContext, frameSize);
		
		}

	};

}