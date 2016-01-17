#pragma once
#include "VideoTransformerStreamInfo.h"
#include <string>

namespace VideoLib {

	struct VideoTransformerInputStream {

		bool isFinished;

		int outputIndex;
		int inputStreamIndex;
		int outputStreamIndex;
		StreamTransformMode mode;
		std::string name;
		double dtsOffset;
		double nextDts;
		double posSeconds;

		VideoTransformerInputStream(const VideoTransformerInputStreamInfo *inputStreamInfo) {

			outputIndex = inputStreamInfo->outputIndex;
			inputStreamIndex = inputStreamInfo->inputStreamIndex;
			outputStreamIndex = inputStreamInfo->outputStreamIndex;
			mode = inputStreamInfo->mode;
			
			isFinished = false;
			dtsOffset = -DBL_MAX;			
			posSeconds = 0;
			nextDts = 0;
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
	
		double smallestOffset;
		AVPacket packet;

		VideoTransformerInput(const VideoTransformerInputInfo *inputStreamInfo) {

			decoder = inputStreamInfo->decoder;

			startTimeRange = inputStreamInfo->startTimeRange;
			endTimeRange = inputStreamInfo->endTimeRange;

			smallestOffset = -DBL_MAX;
		
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
		

		void calcDtsOffsets(int streamIndex, int64_t pts, int64_t dts) {
				
			double ts = dts == AV_NOPTS_VALUE ? pts : dts;

			if(ts == AV_NOPTS_VALUE) {

				throw gcnew VideoLibException("packet is missing valid pts and dts value");
			}
			
			streamsInfo[streamIndex]->posSeconds = decoder->getStream(streamIndex)->getTimeSeconds(ts);
			if(streamsInfo[streamIndex]->dtsOffset != -DBL_MAX) return;

			//get the dts value of the first input packet and subtract it from subsequent dts & pts
			//values to make sure the output video starts at time zero.								
			double dtsOffsetSeconds = 0;
												
			dtsOffsetSeconds = -decoder->getStream(streamIndex)->getTimeSeconds(ts);
			
			// check if the pts/dts value of the current packet is smaller as the current smallest value we found
			// if so use this value instead
			if(dtsOffsetSeconds > smallestOffset) {

				smallestOffset = dtsOffsetSeconds;

			} else {

				dtsOffsetSeconds = smallestOffset;		
			}
			   
			for(unsigned int i = 0; i < streamsInfo.size(); i++) {

				streamsInfo[i]->dtsOffset = decoder->getStream(i)->getTimeBaseUnits(dtsOffsetSeconds);			
			}
					
		}
	};

}