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
		
		VideoTransformerInputStreamInfo(int inputStreamIndex, int outputStreamIndex, StreamTransformMode mode, int outputIndex = 0) 
		{
			this->inputStreamIndex = inputStreamIndex;
			this->outputStreamIndex = outputStreamIndex;
			this->outputIndex = outputIndex;
			this->mode = mode;			
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
			for(int i = 0; i < (int)streamsInfo.size(); i++) {

				delete streamsInfo[i];
			}

			decoder->close();
			delete decoder;
		}

		void addStreamInfo(VideoTransformerInputStreamInfo *streamInfo) {

			streamsInfo.push_back(streamInfo);
		}

		
	};

	struct VideoTransformerOutputStreamInfo {
				
		int outputIndex;	
		StreamTransformMode mode;
		std::vector<char *> bitStreamFilters;
		
		VideoTransformerOutputStreamInfo(int outputIndex, StreamTransformMode mode) 
		{					
			this->outputIndex = outputIndex;	
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
			for(int i = 0; i < (int)streamsInfo.size(); i++) {

				delete streamsInfo[i];
			}

			encoder->close();
			delete encoder;
		}

		void addStreamInfo(VideoTransformerOutputStreamInfo *streamInfo) {

			streamsInfo.push_back(streamInfo);
		}

	};

	

}