#pragma once
#include "VideoTransformerStreamInfo.h"
#include <string>

namespace VideoLib {

	struct VideoTransformerInputStream {

		bool isFinished;

		int outputIndex;	
		int outputStreamIndex;
		StreamTransformMode mode;
		std::string name;
	
		bool isTsOffsetSet;
		int64_t tsOffset;

		double nextTs;
		double posSeconds;

		VideoTransformerInputStream(const VideoTransformerInputStreamInfo *inputStreamInfo) {

			outputIndex = inputStreamInfo->outputIndex;		
			outputStreamIndex = inputStreamInfo->outputStreamIndex;
			mode = inputStreamInfo->mode;
			
			if(mode == StreamTransformMode::DISCARD) {

				isFinished = true;	

			} else {

				isFinished = false;
			}
		
			isTsOffsetSet = false;
			tsOffset = 0;			
			posSeconds = 0;
			nextTs = 0;
		}

		void setTsOffset(int64_t tsOffset) {

			this->tsOffset = tsOffset;
			isTsOffsetSet = true;
		}
	};

	struct VideoTransformerInput {
	
	private:
		bool isInputFinished;

		bool checkIsInputFinished() const {

			bool isInputFinished = true;

			for(int i = 0; i < (int)streamsInfo.size(); i++) {
			
				isInputFinished = isInputFinished && streamsInfo[i]->isFinished;
			}

			return isInputFinished;
		}

		

	public:

		IVideoDecoder *decoder;
		std::vector<VideoTransformerInputStream *> streamsInfo;
		
		double startTimeRange;
		double endTimeRange;
					
		AVPacket packet;

		VideoTransformerInput(const VideoTransformerInputInfo *inputStreamInfo) {

			decoder = inputStreamInfo->decoder;

			startTimeRange = inputStreamInfo->startTimeRange;
			endTimeRange = inputStreamInfo->endTimeRange;
				
			av_init_packet(&packet);
			packet.data = NULL;
			packet.size = 0;
			
			for(int i = 0; i < (int)inputStreamInfo->streamsInfo.size(); i++) {

				streamsInfo.push_back(new VideoTransformerInputStream(inputStreamInfo->streamsInfo[i]));				
			}

			isInputFinished = false;
		}

		~VideoTransformerInput() 
		{
			for(int i = 0; i < (int)streamsInfo.size(); i++) {

				delete streamsInfo[i];
			}
		}

		bool readFrame() 
		{

			while(1) {

				VideoDecoder::ReadFrameResult result = decoder->readFrame(&packet); 			
				if(result == VideoDecoder::ReadFrameResult::END_OF_STREAM) {

					isInputFinished = true;
					return false;				
				}

				if(result == VideoDecoder::ReadFrameResult::READ_ERROR) {

					throw gcnew VideoLibException("Error reading packet from input");
				}

				if(streamsInfo[packet.stream_index]->mode == StreamTransformMode::DISCARD) {

					av_packet_unref(&packet);
					continue;
				}

				if(decoder->getStream(packet.stream_index)->getTimeSeconds(packet.dts) > endTimeRange)
				{
					streamsInfo[packet.stream_index]->isFinished = true;
					isInputFinished = checkIsInputFinished();

					if(isInputFinished) return false;
					av_packet_unref(&packet);
					continue;
				}
				
				return true;
			}
		}

		bool getIsInputFinished() const {

			return(isInputFinished);
		}
		
		
		
		
	};

}