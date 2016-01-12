#pragma once
#include "VideoTransformerStreamInfo.h"

namespace VideoLib {

	struct VideoTransformerOutputStream {

		char *name;
		int outputIndex;
		StreamTransformMode mode;
		BitStreamFilter *bitStreamFilter;
		
		VideoTransformerOutputStream(const VideoTransformerOutputStreamInfo *streamInfo) {

			name = streamInfo->name;
			outputIndex = streamInfo->outputIndex;
			mode = streamInfo->mode;

			bitStreamFilter = new BitStreamFilter;
		
			for(int i = 0; i < streamInfo->bitStreamFilters.size(); i++) {

				bitStreamFilter->add(streamInfo->bitStreamFilters[i]);
			}
			
		}

		~VideoTransformerOutputStream() {
			
			delete bitStreamFilter;			
		}
	};

	struct VideoTransformerOutput {

		VideoEncoder *encoder;

		std::vector<VideoTransformerOutputStream *> streamsInfo;

		VideoTransformerOutput(VideoTransformerOutputInfo *outputInfo) {
			
			this->encoder = outputInfo->encoder;
			
			for(int i = 0; i < outputInfo->streamsInfo.size(); i++) {

				streamsInfo.push_back(new VideoTransformerOutputStream(outputInfo->streamsInfo[i]));
			}

		}

		~VideoTransformerOutput() 
		{
			for(int i = 0; i < streamsInfo.size(); i++) {

				delete streamsInfo[i];
			}
		}

	};
}