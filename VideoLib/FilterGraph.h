#pragma once
#pragma warning(disable : 4244)
// unsafe function warning disable
#pragma warning(disable : 4996)
#include <algorithm>
#include "stdafx.h"
#include "VideoLibException.h"
#include "Utils.h"
#include <msclr\marshal.h>


namespace VideoLib {

class FilterGraph {

protected:

	AVFilterInOut *inputs;
	AVFilterInOut *outputs;

	AVFilterGraph *filterGraph; 
		
	AVFilterContext *initVideoInput(VideoLib::Stream *videoStream, const char *name) {

		AVCodecContext *videoContext = videoStream->getCodecContext();

		AVFilter *buffersrc = avfilter_get_by_name("buffer");
		if (buffersrc == NULL) {

			throw gcnew VideoLib::VideoLibException("Video buffer source filter not found");					
		}

		/*char args[512];

		_snprintf(args, sizeof(args),
				"video_size=%dx%d:pix_fmt=%d:time_base=%d/%d:pixel_aspect=%d/%d",
				videoContext->width, videoContext->height, videoContext->pix_fmt,
				videoContext->time_base.num, videoContext->time_base.den,
				videoContext->sample_aspect_ratio.num, videoContext->sample_aspect_ratio.den);*/

		AVFilterContext *buffersrc_ctx = avfilter_graph_alloc_filter(filterGraph, buffersrc, name);
		if (buffersrc_ctx == NULL) {

			throw gcnew VideoLib::VideoLibException("Error creating video buffer source filter");					
		}
	
		/*int result = avfilter_graph_create_filter(&buffersrc_ctx, buffersrc, name, NULL, NULL, filterGraph);
		if (result < 0) {

			throw gcnew VideoLib::VideoLibException("Error creating video buffer source filter");					
		}*/

		av_opt_set_image_size(buffersrc_ctx,"video_size", videoContext->width, videoContext->height, AV_OPT_SEARCH_CHILDREN);	
		av_opt_set_pixel_fmt(buffersrc_ctx,"pix_fmt",videoContext->pix_fmt, AV_OPT_SEARCH_CHILDREN);
		av_opt_set_q(buffersrc_ctx,"time_base",videoContext->time_base, AV_OPT_SEARCH_CHILDREN);
		av_opt_set_q(buffersrc_ctx,"pixel_aspect",videoContext->sample_aspect_ratio, AV_OPT_SEARCH_CHILDREN);			

		int result = avfilter_init_str(buffersrc_ctx, NULL);
		if (result < 0) {

			throw gcnew VideoLib::VideoLibException("Cannot initialize video buffer source filter");	
		}
		
		return buffersrc_ctx;
	}

	AVFilterContext *initAudioInput(VideoLib::Stream *audioStream, const char *name) {

		AVCodecContext *audioContext = audioStream->getCodecContext();

		AVFilter *abuffersrc = avfilter_get_by_name("abuffer");
		if (abuffersrc == NULL) {

			throw gcnew VideoLib::VideoLibException("Audio buffer source filter not found");					
		}

		if (!audioContext->channel_layout) {

			audioContext->channel_layout = av_get_default_channel_layout(audioContext->channels);
		}
		
		AVFilterContext *abuffersrc_ctx = avfilter_graph_alloc_filter(filterGraph, abuffersrc, name);
		if (abuffersrc_ctx == NULL) {

			throw gcnew VideoLib::VideoLibException("Error creating audio buffer source filter");					
		}

		// https://www.ffmpeg.org/doxygen/2.2/filter_audio_8c-example.html
		// Set the filter options through the AVOptions API. 
		char ch_layout[64];
		av_get_channel_layout_string(ch_layout, sizeof(ch_layout), 0, audioContext->channel_layout);
		av_opt_set(abuffersrc_ctx, "channel_layout", ch_layout, AV_OPT_SEARCH_CHILDREN);
		av_opt_set (abuffersrc_ctx, "sample_fmt", av_get_sample_fmt_name(audioContext->sample_fmt), AV_OPT_SEARCH_CHILDREN);
		av_opt_set_q(abuffersrc_ctx, "time_base", audioContext->time_base, AV_OPT_SEARCH_CHILDREN);
		av_opt_set_int(abuffersrc_ctx, "sample_rate", audioContext->sample_rate, AV_OPT_SEARCH_CHILDREN);

		int result = avfilter_init_str(abuffersrc_ctx, NULL);
		if (result < 0) {

			throw gcnew VideoLib::VideoLibException("Cannot initialize audio buffer source filter");	
		}
	
		return(abuffersrc_ctx);
	}

	AVFilterContext *initVideoOutput(VideoLib::Stream *videoStream, const char *name) {
		
		AVCodecContext *videoContext = videoStream->getCodecContext();
								
		AVFilter *buffersink = avfilter_get_by_name("buffersink");
		if (buffersink == NULL) {

			throw gcnew VideoLib::VideoLibException("Video buffer sink filter not found");					
		}
			
		AVFilterContext *buffersink_ctx = NULL;

		int result = avfilter_graph_create_filter(&buffersink_ctx, buffersink, name,
			NULL, NULL, filterGraph);
		if (result < 0) {

			throw gcnew VideoLib::VideoLibException("Error creating video buffer sink filter");		
		}
				
		result = av_opt_set_bin(buffersink_ctx, "pix_fmts",
			(uint8_t*)&videoContext->pix_fmt, sizeof(videoContext->pix_fmt),
			AV_OPT_SEARCH_CHILDREN);
		if (result < 0) {

			throw gcnew VideoLib::VideoLibException("Cannot set output pixel format");					
		}
		
		return buffersink_ctx;

	}

	AVFilterContext *initAudioOutput(VideoLib::Stream *audioStream, const char *name) {
													
		AVFilter *abuffersink = avfilter_get_by_name("abuffersink");
		if (abuffersink == NULL) {

			throw gcnew VideoLib::VideoLibException("Audio buffer sink filter not found");					
		}

		AVFilterContext *abuffersink_ctx = NULL;

		int result = avfilter_graph_create_filter(&abuffersink_ctx, abuffersink, name,
			NULL, NULL, filterGraph);
		if (result < 0) {

			throw gcnew VideoLib::VideoLibException("Error creating audio buffer sink filter");				
		}			

		AVCodecContext *audioContext = audioStream->getCodecContext();

		result = av_opt_set_bin(abuffersink_ctx, "sample_fmts",
			(uint8_t*)&audioContext->sample_fmt, sizeof(audioContext->sample_fmt),
			AV_OPT_SEARCH_CHILDREN);
		if (result < 0) {

			throw gcnew VideoLib::VideoLibException("Cannot set output sample format");					
		}

		result = av_opt_set_bin(abuffersink_ctx, "channel_layouts",
			(uint8_t*)&audioContext->channel_layout, sizeof(audioContext->channel_layout), 
			AV_OPT_SEARCH_CHILDREN);
		if (result < 0) {

			throw gcnew VideoLib::VideoLibException("Cannot set output channel layout");	
		}

		result = av_opt_set_bin(abuffersink_ctx, "sample_rates",
			(uint8_t*)&audioContext->sample_rate, sizeof(audioContext->sample_rate),
			AV_OPT_SEARCH_CHILDREN);
		if (result < 0) {

			throw gcnew VideoLib::VideoLibException("Cannot set output sample rate");			
		}
			
		return abuffersink_ctx;
									
	}

	void init() {

		filterGraph = avfilter_graph_alloc();				
		if (!filterGraph) {
				
			throw gcnew VideoLib::VideoLibException("Error allocating filter graph");	
		}

		avfilter_graph_set_auto_convert(filterGraph, AVFILTER_AUTO_CONVERT_ALL);
		setScaleMode(SWS_BICUBIC);

		inputs = NULL;
		outputs = NULL;
	}

	void free() {

		avfilter_graph_free(&filterGraph);

		avfilter_inout_free(&inputs);
		avfilter_inout_free(&outputs);
	}
	
public:

	FilterGraph() {

		init();
	}

	~FilterGraph() {
	
		free();		
	}

	void setScaleMode(int64_t scalemode) {

		char sws_flags_str[128];

		_snprintf(sws_flags_str, sizeof(sws_flags_str), "flags=%I64d", scalemode);
		filterGraph->scale_sws_opts = av_strdup(sws_flags_str);
	}

	void setAudioSinkFrameSize(char *audioSinkName, unsigned int frameSize) {

		AVFilterContext *filter_ctx = avfilter_graph_get_filter(filterGraph, audioSinkName);		
		if(filter_ctx == NULL) {

			throw gcnew VideoLib::VideoLibException("Audio sink filter not found");	
		}

		av_buffersink_set_frame_size(filter_ctx, frameSize);
		
	}

	int getInputFailedRequests(const char *inputName) {

		AVFilterContext *buffer_src = avfilter_graph_get_filter(filterGraph, inputName);		
		if(buffer_src == NULL) {

			throw gcnew VideoLib::VideoLibException("Cannot find input filter");	
		}

		return (int)av_buffersrc_get_nb_failed_requests(buffer_src);
	}

	int requestFrame() {

		return avfilter_graph_request_oldest(filterGraph);	
	}
		
	void addInputStream(VideoLib::Stream *inputStream, const char *name = "in") 
	{
		AVFilterInOut *input = avfilter_inout_alloc();
		if (input == NULL) {
				
			throw gcnew VideoLib::VideoLibException("Error allocating input");	
		};

		AVFilterContext *filter_ctx = NULL;

		if(inputStream->isVideo()) {

			filter_ctx = initVideoInput(inputStream, name);
		} 
		else if(inputStream->isAudio()) 
		{
			filter_ctx = initAudioInput(inputStream, name);
		}
		
		input->name = av_strdup(filter_ctx->name);
		input->filter_ctx = filter_ctx;
		input->pad_idx = 0;			
		input->next = NULL;
		
		if(inputs == NULL) { 

			inputs = input;

		} else {

			AVFilterInOut *next = inputs;

			while(next->next != NULL) {

				next = next->next;
			}

			next->next = input;
		}
	}

	void addOutputStream(VideoLib::Stream *outputStream, const char *name = "out")
	{
		AVFilterInOut *output = avfilter_inout_alloc();
		if (output == NULL) {
				
			throw gcnew VideoLib::VideoLibException("Error allocating output");	
		};

		AVFilterContext *filter_ctx = NULL;

		if(outputStream->isVideo()) {

			filter_ctx = initVideoOutput(outputStream, name);
		} 
		else if(outputStream->isAudio()) 
		{
			filter_ctx = initAudioOutput(outputStream, name);
		}

		output->name = av_strdup(filter_ctx->name);
		output->filter_ctx = filter_ctx;
		output->pad_idx = 0;			
		output->next = NULL;
	
		if(outputs == NULL) { 

			outputs = output;

		} else {

			AVFilterInOut *next = outputs;

			while(next->next != NULL) {

				next = next->next;
			}

			next->next = output;
		}
	}
				
	void createGraph(const char *filterSpec) {
				
		int result = 0;
							
		result = avfilter_graph_parse_ptr(filterGraph, filterSpec, &outputs, &inputs, NULL);
		if(result < 0) {

			throw gcnew VideoLib::VideoLibException("error parsing filter graph: ", result);	
		}
							
		result = avfilter_graph_config(filterGraph, NULL);
		if(result < 0) {

			throw gcnew VideoLib::VideoLibException("error configuring filter graph: ", result);	
		}

		char *graph = avfilter_graph_dump(filterGraph, NULL);
		VideoInit::writeToLog(AV_LOG_INFO, graph);
		av_free(graph);
		
		avfilter_inout_free(&inputs);
		avfilter_inout_free(&outputs);
		
	}

	void pushFrame(AVFrame *input, const char *inputName = "in") {
					
		AVFilterContext *filter_ctx = avfilter_graph_get_filter(filterGraph, inputName);		
		if(filter_ctx == NULL) {

			throw gcnew VideoLib::VideoLibException("Cannot find input filter");	
		}

		// push the decoded frame into the filtergraph 
		int result = av_buffersrc_add_frame_flags(filter_ctx, input, 0);
		if (result < 0) {

			throw gcnew VideoLib::VideoLibException("Error adding frame to filtergraph: ", result);			
		}
	}

	bool pullFrame(AVFrame *output, const char *outputName = "out") {

		AVFilterContext *filter_ctx = avfilter_graph_get_filter(filterGraph, outputName);		
		if(filter_ctx == NULL) {

			throw gcnew VideoLib::VideoLibException("Cannot find output filter");	
		}

		
		int result = av_buffersink_get_frame_flags(filter_ctx, output, 0);
		if (result < 0) {

			// if no more frames for output - returns AVERROR(EAGAIN)
			// if flushed and no more frames for output - returns AVERROR_EOF					
			if (result == AVERROR(EAGAIN) || result == AVERROR_EOF) {

				return(false);
			}
			
			throw gcnew VideoLib::VideoLibException("Error pulling frame from filtergraph: ", result);	
		}
	  
		output->pict_type = AV_PICTURE_TYPE_NONE;

		return(true);
	}
		
	AVFilterContext *getFilter(const char *name) {

		AVFilterContext *filter_ctx = avfilter_graph_get_filter(filterGraph, name);

		return filter_ctx;
	}

	void clear() {

		free();
		init();
	}
};



}
