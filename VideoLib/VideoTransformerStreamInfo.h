#pragma once
#include <vector>
#include <limits>
#include "IVideoDecoder.h"

namespace VideoLib {

	enum class StreamTransformMode {
		COPY,
		ENCODE,
		DISCARD
	};
		
	struct VideoTransformerInputStreamInfo {

		int inputStreamIndex;
		int outputStreamIndex;
		int outputIndex;
		StreamTransformMode mode;
		char *name;
		
		VideoTransformerInputStreamInfo(int inputStreamIndex, int outputStreamIndex, StreamTransformMode mode, int outputIndex = 0, char *name = "in") 
		{
			this->inputStreamIndex = inputStreamIndex;
			this->outputStreamIndex = outputStreamIndex;
			this->outputIndex = outputIndex;
			this->mode = mode;
			this->name = name;			
		}

	};

	struct VideoTransformerInputInfo {
	
	public:

		IVideoDecoder *decoder;

		std::vector<VideoTransformerInputStreamInfo *> streamsInfo;
		double startTimeRange;
		double endTimeRange;

		VideoTransformerInputInfo(IVideoDecoder *decoder, double startTimeRange = 0, double endTimeRange = DBL_MAX)
		{
			this->decoder = decoder;
			this->startTimeRange = startTimeRange;
			this->endTimeRange = endTimeRange;						
		}

		~VideoTransformerInputInfo()
		{
			for(int i = 0; i < streamsInfo.size(); i++) {

				delete streamsInfo[i];
			}
		}

		void addStreamInfo(VideoTransformerInputStreamInfo *streamInfo) {

			streamsInfo.push_back(streamInfo);
		}

		
	};

	struct VideoTransformerOutputStreamInfo {
				
		int outputIndex;	
		char *name;
		StreamTransformMode mode;
		std::vector<char *> bitStreamFilters;
		
		VideoTransformerOutputStreamInfo(int outputIndex, StreamTransformMode mode, char *name = "out") 
		{					
			this->outputIndex = outputIndex;	
			this->name = name;
			this->mode = mode;
		}

		void addBitstreamFilter(char *name) {

			bitStreamFilters.push_back(name);
		}

	};

	struct VideoTransformerOutputInfo {

		VideoEncoder *encoder;
		std::vector<VideoTransformerOutputStreamInfo *> streamsInfo;

		VideoTransformerOutputInfo(VideoEncoder *encoder) 
		{		
			this->encoder = encoder;									
		}

		~VideoTransformerOutputInfo()
		{
			for(int i = 0; i < streamsInfo.size(); i++) {

				delete streamsInfo[i];
			}
		}

		void addStreamInfo(VideoTransformerOutputStreamInfo *streamInfo) {

			streamsInfo.push_back(streamInfo);
		}

	};

	

}