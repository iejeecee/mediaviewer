#pragma once
#include "BufferSourceFilter.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Threading;

namespace VideoLib2
{
	public ref class VideoBufferSourceFilter : BufferSourceFilter
	{
									
	public:									
		
		VideoBufferSourceFilter(String ^instanceName, IntPtr filterGraph, Stream *inputStream) :
			BufferSourceFilter("buffer", instanceName, filterGraph)
		{						
			if(!inputStream->isVideo()) {

				throw gcnew VideoLib2::VideoLibException("input stream is not a video stream");	
			}

			AVCodecContext *videoContext = inputStream->getCodecContext();

			av_opt_set_image_size(filterContext,"video_size", videoContext->width, videoContext->height, AV_OPT_SEARCH_CHILDREN);	
			av_opt_set_pixel_fmt(filterContext,"pix_fmt", videoContext->pix_fmt, AV_OPT_SEARCH_CHILDREN);
			av_opt_set_q(filterContext,"time_base", videoContext->time_base, AV_OPT_SEARCH_CHILDREN);
			av_opt_set_q(filterContext,"pixel_aspect", videoContext->sample_aspect_ratio, AV_OPT_SEARCH_CHILDREN);			

			initFilter();
		}


	};

}