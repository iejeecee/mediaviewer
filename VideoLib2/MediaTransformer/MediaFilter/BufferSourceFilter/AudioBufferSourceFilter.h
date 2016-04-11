#pragma once
#include "BufferSourceFilter.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Threading;

namespace VideoLib2
{
	public ref class AudioBufferSourceFilter : BufferSourceFilter
	{
									
	public:									
		
		AudioBufferSourceFilter(String ^instanceName, IntPtr filterGraph, Stream *inputStream) :
			BufferSourceFilter("abuffer", instanceName, filterGraph)
		{						
			if(!inputStream->isAudio()) {

				throw gcnew VideoLib2::VideoLibException("input stream is not a audio stream");	
			}

			AVCodecContext *audioContext = inputStream->getCodecContext();

			// https://www.ffmpeg.org/doxygen/2.2/filter_audio_8c-example.html
			// Set the filter options through the AVOptions API. 
			char ch_layout[64];
			av_get_channel_layout_string(ch_layout, sizeof(ch_layout), 0, audioContext->channel_layout);
			av_opt_set(filterContext, "channel_layout", ch_layout, AV_OPT_SEARCH_CHILDREN);
			av_opt_set (filterContext, "sample_fmt", av_get_sample_fmt_name(audioContext->sample_fmt), AV_OPT_SEARCH_CHILDREN);
			av_opt_set_q(filterContext, "time_base", audioContext->time_base, AV_OPT_SEARCH_CHILDREN);
			av_opt_set_int(filterContext, "sample_rate", audioContext->sample_rate, AV_OPT_SEARCH_CHILDREN);

			initFilter();
		}


	};

}