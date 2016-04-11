#pragma once
#include "..\OutMediaFilter.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Threading;

namespace VideoLib2
{
	public ref class BufferSourceFilter : OutMediaFilter
	{

	protected:

		BufferSourceFilter(String ^filterName, String ^instanceName, IntPtr filterGraph) :
			OutMediaFilter(filterName,instanceName,filterGraph)
		{

		}
									
	public:		

		void addFrame(AVFrame *frame) 
		{
			int result = av_buffersrc_add_frame_flags(filterContext, frame, AV_BUFFERSRC_FLAG_KEEP_REF);
			if (result < 0) {

				throw gcnew VideoLib2::VideoLibException("Error adding frame to buffer filter: ", result);			
			}

		}
		
		property int NrFailedRequests
		{
			int get() {

				return (int)av_buffersrc_get_nb_failed_requests(filterContext);
			}
		}


	};

}