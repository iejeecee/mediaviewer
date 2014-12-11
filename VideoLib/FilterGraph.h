#pragma once
#pragma warning(disable : 4244)
// unsafe function warning disable
#pragma warning(disable : 4996)
#include <algorithm>
#include "stdafx.h"
#include "VideoInit.h"
#include "VideoLibException.h"
#include <msclr\marshal.h>


namespace VideoLib {

class FilterGraph {

protected:

	AVFilterGraph *filterGraph; 

	AVFilter *buffersrc;
	AVFilter *buffersink;
	AVFilterContext *buffersrc_ctx;
	AVFilterContext *buffersink_ctx;
	
	void initialize() {

		filterGraph = avfilter_graph_alloc();				
		if (!filterGraph) {
				
			throw gcnew VideoLib::VideoLibException("Not enough memory");	
		}

		avfilter_graph_set_auto_convert(filterGraph, AVFILTER_AUTO_CONVERT_ALL);
	}


public:

	FilterGraph(AVCodecContext *inputContext, AVPixelFormat outputPixFMT, int64_t scalemode) {

		enum AVPixelFormat pix_fmts[] = { outputPixFMT, AV_PIX_FMT_NONE };

		char args[512];
		int result = 0;

		initialize();

		AVBufferSinkParams *buffersink_params = NULL;

		try {

			buffersink_params = av_buffersink_params_alloc();
			buffersink_params->pixel_fmts = pix_fmts;

			buffersrc = avfilter_get_by_name("buffer");
			buffersink = avfilter_get_by_name("buffersink");

			if (!buffersrc || !buffersink) {

				throw gcnew VideoLib::VideoLibException("Filtering source or sink element not found");					
			}

			_snprintf(args, sizeof(args),
				"video_size=%dx%d:pix_fmt=%d:time_base=%d/%d:pixel_aspect=%d/%d",
				inputContext->width, inputContext->height, inputContext->pix_fmt,
				inputContext->time_base.num, inputContext->time_base.den,
				inputContext->sample_aspect_ratio.num,
				inputContext->sample_aspect_ratio.den);

			result = avfilter_graph_create_filter(&buffersrc_ctx, buffersrc, "in",
				args, NULL, filterGraph);
			if (result < 0) {

				throw gcnew VideoLib::VideoLibException("Cannot create buffer source");					
			}
		
			result = avfilter_graph_create_filter(&buffersink_ctx, buffersink, "out",
				NULL, buffersink_params, filterGraph);
			if (result < 0) {

				throw gcnew VideoLib::VideoLibException("Cannot create buffer sink");		
			}
		
			char sws_flags_str[128];

			_snprintf(sws_flags_str, sizeof(sws_flags_str), "flags=%I64d", scalemode);
			filterGraph->scale_sws_opts = av_strdup(sws_flags_str);

		} finally {

			if(buffersink_params != NULL) {

				av_freep(&buffersink_params);
			}
		}
		
	}

	FilterGraph(AVCodecContext *inputContext,  AVSampleFormat outputSampleFMT, uint64_t outputChannelLayout, int outputSampleRate) 
	{		
		char args[512];
		int result = 0;

		initialize();

		buffersrc = avfilter_get_by_name("abuffer");
		buffersink = avfilter_get_by_name("abuffersink");

		if (!buffersrc || !buffersink) {

			throw gcnew VideoLib::VideoLibException("filtering source or sink element not found");					
		}

		if (!inputContext->channel_layout) {

			inputContext->channel_layout = av_get_default_channel_layout(inputContext->channels);
		}

		_snprintf(args, sizeof(args),
			"time_base=%d/%d:sample_rate=%d:sample_fmt=%s:channel_layout=0x%I64u",
			inputContext->time_base.num, inputContext->time_base.den, inputContext->sample_rate,
			av_get_sample_fmt_name(inputContext->sample_fmt),
			inputContext->channel_layout);

		result = avfilter_graph_create_filter(&buffersrc_ctx, buffersrc, "in",
			args, NULL, filterGraph);
		if (result < 0) {

			throw gcnew VideoLib::VideoLibException("Cannot create audio buffer source");					
		}

		result = avfilter_graph_create_filter(&buffersink_ctx, buffersink, "out",
			NULL, NULL, filterGraph);
		if (result < 0) {

			throw gcnew VideoLib::VideoLibException("Cannot create audio buffer sink");				
		}

		result = av_opt_set_bin(buffersink_ctx, "sample_fmts",
			(uint8_t*)&outputSampleFMT, sizeof(outputSampleFMT),
			AV_OPT_SEARCH_CHILDREN);
		if (result < 0) {

			throw gcnew VideoLib::VideoLibException("Cannot set output sample format");					
		}

		result = av_opt_set_bin(buffersink_ctx, "channel_layouts",
			(uint8_t*)&outputChannelLayout,
			sizeof(outputChannelLayout), AV_OPT_SEARCH_CHILDREN);
		if (result < 0) {

			throw gcnew VideoLib::VideoLibException("Cannot set output channel layout");	
		}

		result = av_opt_set_bin(buffersink_ctx, "sample_rates",
			(uint8_t*)&outputSampleRate, sizeof(outputSampleRate),
			AV_OPT_SEARCH_CHILDREN);
		if (result < 0) {

			throw gcnew VideoLib::VideoLibException("Cannot set output sample rate");			
		}
		 		
	}

	void createGraph(const char *filterSpec) {

		AVFilterInOut *outputs = avfilter_inout_alloc();
		if (!outputs) {
				
			throw gcnew VideoLib::VideoLibException("Not enough memory");	
		}

		AVFilterInOut *inputs = avfilter_inout_alloc();
		if (!inputs) {
				
			throw gcnew VideoLib::VideoLibException("Not enough memory");	
		}
		
		int result = 0;

		try {

			// Endpoints for the filter graph. 
			outputs->name = av_strdup("in");
			outputs->filter_ctx = buffersrc_ctx;
			outputs->pad_idx = 0;
			outputs->next = NULL;
			inputs->name = av_strdup("out");
			inputs->filter_ctx = buffersink_ctx;
			inputs->pad_idx = 0;
			inputs->next = NULL;

			if (!outputs->name || !inputs->name) {

				throw gcnew VideoLib::VideoLibException("Not enough memory");	
			}

			result = avfilter_graph_parse_ptr(filterGraph, filterSpec, &inputs, &outputs, NULL);
			if(result < 0) {

				throw gcnew VideoLib::VideoLibException("error parsing filter graph");	
			}
			
		
			result = avfilter_graph_config(filterGraph, NULL);
			if(result < 0) {

				throw gcnew VideoLib::VideoLibException("error configuring filter graph");	
			}
							
		} finally {

			avfilter_inout_free(&inputs);
			avfilter_inout_free(&outputs);
		}
	}
			
	~FilterGraph() {
	
		avfilter_graph_free(&filterGraph);
		
	}

	int filterFrame(AVFrame *input, AVFrame *output)
	{
		int result;	
				
		// push the decoded frame into the filtergraph 
		result = av_buffersrc_add_frame_flags(buffersrc_ctx, input, 0);
		if (result < 0) {

			throw gcnew VideoLib::VideoLibException("Error adding frame to filtergraph");			
		}
/*		
		AVFilterBufferRef *bufferRef;

        result = av_buffersink_get_buffer_ref(buffersink_ctx, &bufferRef, 0);
        if (result < 0) {
            
			return(0);
        }
       
        avfilter_copy_buf_props(output, bufferRef);
*/

		// pull filtered frames from the filtergraph										
		result = av_buffersink_get_frame(buffersink_ctx, output);
		if (result < 0) {

			// if no more frames for output - returns AVERROR(EAGAIN)
			// if flushed and no more frames for output - returns AVERROR_EOF
			// rewrite retcode to 0 to show it as normal procedure completion				
			if (result == AVERROR(EAGAIN) || result == AVERROR_EOF) {

				result = 0;
			}
			
			return(result);
		}
	  
		output->pict_type = AV_PICTURE_TYPE_NONE;
			
		return(1);			
	}
	
	AVFilterContext *getBufferSourceContext() const {

		return(buffersrc_ctx);
	}

	AVFilterContext *getBufferSinkContext() const {

		return(buffersink_ctx);
	}

};



}
