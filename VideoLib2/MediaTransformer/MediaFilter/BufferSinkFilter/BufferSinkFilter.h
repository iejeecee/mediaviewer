#pragma once
#include "..\InMediaFilter.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Threading;

namespace VideoLib2
{
	public ref class BufferSinkFilter : InMediaFilter
	{
		
	protected:
		
		AVFrame *tempFrame;

		BufferSinkFilter(String ^filterName, String ^instanceName, IntPtr filterGraph) :
			InMediaFilter(filterName,instanceName,filterGraph)
		{
			AVFrame *tempFrame = av_frame_alloc();
			if(tempFrame == NULL) {

				throw gcnew VideoLib2::VideoLibException("Error allocating frame");	
			}
		}
									
	public:		
		
		virtual ~BufferSinkFilter()
		{
			this->!BufferSinkFilter();
		}

		!BufferSinkFilter()
		{
			if(tempFrame != NULL) {

				pin_ptr<AVFrame *> p = &tempFrame;
				av_frame_free(p);
				tempFrame = NULL;
			}
		}

		enum class GetFrameResult
		{
			VIDEO_FRAME,
			AUDIO_FRAME,
			EMPTY,
			END_OF_FILE
		};
		
		
		void rescaleToOutputTimebase(AVPacket *packet, AVRational outputTimeBase)
		{
			AVRational inputTimeBase = filterContext->inputs[0]->time_base;
			
			Utils::rescaleTimeBase(packet,inputTimeBase, outputTimeBase);
		
		}

		GetFrameResult peekFrame() {
						
			int result = av_buffersink_get_frame_flags(filterContext, tempFrame, AV_BUFFERSINK_FLAG_PEEK);
			if (result < 0) {

				// if no more frames for output - returns AVERROR(EAGAIN)
				// if flushed and no more frames for output - returns AVERROR_EOF					
				if (result == AVERROR(EAGAIN)) {

					return(GetFrameResult::EMPTY);
				}
				
				if(result == AVERROR_EOF) {

					return(GetFrameResult::END_OF_FILE);
				}

				throw gcnew VideoLib2::VideoLibException("Error pulling frame from filtergraph: ", result);	
			}

			GetFrameResult frameResult = tempFrame->width > 0 ? GetFrameResult::VIDEO_FRAME : GetFrameResult::AUDIO_FRAME;
			
			av_frame_unref(tempFrame);

			return(frameResult);
		}

		GetFrameResult pullFrame(AVFrame *frame) 
		{
			int result = av_buffersink_get_frame_flags(filterContext, frame, 0);
			if (result < 0) {

				// if no more frames for output - returns AVERROR(EAGAIN)
				// if flushed and no more frames for output - returns AVERROR_EOF					
				if (result == AVERROR(EAGAIN)) {

					return(GetFrameResult::EMPTY);
				}
				
				if(result == AVERROR_EOF) {

					return(GetFrameResult::END_OF_FILE);
				}

				throw gcnew VideoLib2::VideoLibException("Error pulling frame from filtergraph: ", result);	
			}
	  
			frame->pict_type = AV_PICTURE_TYPE_NONE;

			GetFrameResult frameResult = frame->width > 0 ? GetFrameResult::VIDEO_FRAME : GetFrameResult::AUDIO_FRAME;
					
			return(frameResult);

		}

	};

}