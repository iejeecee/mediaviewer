#pragma once
#pragma warning(disable : 4244)
// unsafe function warning disable
#pragma warning(disable : 4996)
#include <algorithm>
#include "..\stdafx.h"
#include "..\VideoLibException.h"
#include "..\Utils\Utils.h"
#include <msclr\marshal.h>


namespace VideoLib2 {

class FilterGraph {

protected:

	AVFilterGraph *filterGraph; 
		
	void init() {

		filterGraph = avfilter_graph_alloc();				
		if (!filterGraph) {
				
			throw gcnew VideoLib2::VideoLibException("Error allocating filter graph");	
		}

		avfilter_graph_set_auto_convert(filterGraph, AVFILTER_AUTO_CONVERT_ALL);
		setScaleMode(SWS_BICUBIC);
	
	}

	void free() {

		avfilter_graph_free(&filterGraph);
	
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

		
	int requestFrame() {

		return avfilter_graph_request_oldest(filterGraph);	
	}
			
	void verifyGraph() {
				
		int result = 0;
															
		result = avfilter_graph_config(filterGraph, NULL);
		if(result < 0) {

			throw gcnew VideoLib2::VideoLibException("error configuring filter graph: ", result);	
		}

		char *graph = avfilter_graph_dump(filterGraph, NULL);
		VideoInit::writeToLog(AV_LOG_DEBUG, graph);
		av_free(graph);
			
		
	}
			
	AVFilterContext *getFilter(const char *name) {

		AVFilterContext *filter_ctx = avfilter_graph_get_filter(filterGraph, name);

		return filter_ctx;
	}

	AVFilterGraph *getAVFilterGraph() {

		return filterGraph;
	}

	void clear() {

		free();
		init();
	}
};



}
