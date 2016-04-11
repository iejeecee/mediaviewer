#pragma once
#include "BufferSinkFilter.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Threading;

namespace VideoLib2
{
	public ref class VideoBufferSinkFilter : BufferSinkFilter
	{
		void init(AVPixelFormat pixelFormat)
		{
			int result = av_opt_set_bin(filterContext, "pix_fmts",
				(uint8_t*)&pixelFormat, sizeof(pixelFormat),
				AV_OPT_SEARCH_CHILDREN);
			if (result < 0) {

				throw gcnew VideoLib2::VideoLibException("Cannot set output pixel format");					
			}

			initFilter();
		}

	public:									

		VideoBufferSinkFilter(String ^instanceName, IntPtr filterGraph, Stream *outputStream) :
			BufferSinkFilter("buffersink", instanceName, filterGraph)
		{						
			if(!outputStream->isVideo()) {

				throw gcnew VideoLib2::VideoLibException("output stream is not a video stream");	
			}

			AVCodecContext *videoContext = outputStream->getCodecContext();

			init(videoContext->pix_fmt);

		}

		VideoBufferSinkFilter(String ^instanceName, IntPtr filterGraph,AVPixelFormat pixelFormat) :
			BufferSinkFilter("buffersink", instanceName, filterGraph)
		{
			init(pixelFormat);
		}

	};

}