#pragma once
#include "VideoTransformerStreamInfo.h"
#include <string>

namespace VideoLib {

	struct VideoTransformerOutputStream {

		std::string name;
		int outputIndex;
		StreamTransformMode mode;
		BitStreamFilter *bitStreamFilter;
		
		VideoTransformerOutputStream(const VideoTransformerOutputStreamInfo *streamInfo) {

			outputIndex = streamInfo->outputIndex;
			mode = streamInfo->mode;

			bitStreamFilter = new BitStreamFilter;
		
			for(int i = 0; i < (int)streamInfo->bitStreamFilters.size(); i++) {

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
			
			for(int i = 0; i < (int)outputInfo->streamsInfo.size(); i++) {

				streamsInfo.push_back(new VideoTransformerOutputStream(outputInfo->streamsInfo[i]));
			}

		}

		~VideoTransformerOutput() 
		{
			for(int i = 0; i < (int)streamsInfo.size(); i++) {

				delete streamsInfo[i];
			}
		}

	};
}