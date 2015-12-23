#pragma once
#include "IVideoDecoder.h"
#include "VideoEncoder.h"
#include "FilterGraph.h"
#include "BitStreamFilter.h"

#define DISCARD_STREAM -1

using namespace MediaViewer::Infrastructure::Video::TranscodeOptions;
using namespace System::Collections::Generic;
using namespace msclr::interop;
using namespace MediaViewer::Infrastructure::Utils;
using namespace MediaViewer::Infrastructure::Logging;


namespace VideoLib {
	
	struct VideoTranscodeStreamInfo {

		VideoTranscodeStreamInfo(int outStreamIdx = DISCARD_STREAM) {

			dtsOffset = posSeconds = 0;			
			bitStreamFilter = new BitStreamFilter();
			filterGraph = NULL;		
			this->outStreamIdx = outStreamIdx;	
			offsetSet = false;
		}

		~VideoTranscodeStreamInfo() {
			
			delete bitStreamFilter;
			bitStreamFilter = NULL;
			
			if(filterGraph != NULL) {
				delete filterGraph;
				filterGraph = NULL;
			}
		}
			
		int outStreamIdx;
		
		bool offsetSet;
		int64_t dtsOffset;	
		double posSeconds;

		FilterGraph *filterGraph;
		BitStreamFilter *bitStreamFilter;
		
	};

	class VideoTranscodeStreamsInfo 
	{
	
		IVideoDecoder *input;
		std::vector<VideoTranscodeStreamInfo *> streamInfo;
		
		double startTimeSeconds,endTimeSeconds;

		double smallestOffset;

	public:
		
		VideoTranscodeStreamsInfo() 
		{
			
		}

		void initialize(IVideoDecoder *input, double startTimeSeconds, double endTimeSeconds) {

			this->input = input;
			this->startTimeSeconds = startTimeSeconds;
			this->endTimeSeconds = endTimeSeconds;

			smallestOffset = -DBL_MAX;
		}

		~VideoTranscodeStreamsInfo() {

			clear();
		}

		void add(int outStreamIdx = DISCARD_STREAM) {

			streamInfo.push_back(new VideoTranscodeStreamInfo(outStreamIdx));
		}

		void clear() {

			for(int i = 0; i < streamInfo.size(); i++) {

				delete streamInfo[i];
			}

			streamInfo.clear();
		}

		VideoTranscodeStreamInfo *operator[](int i) {

			return(streamInfo[i]);
		}

		void calcDtsOffsets(int streamIndex, int64_t pts, int64_t dts) {
			
			if(pts != AV_NOPTS_VALUE) {

				streamInfo[streamIndex]->posSeconds = input->getStream(streamIndex)->getTimeSeconds(pts);

				// seeking can be inexact due to keyframes, in that case adjust the startTimeSeconds 
				startTimeSeconds = min(streamInfo[streamIndex]->posSeconds, startTimeSeconds);
			}

			//get the dts value of the first input packet and subtract it from subsequent dts & pts
			//values to make sure the output video starts at time zero.			
			if(streamInfo[streamIndex]->offsetSet == true) return;
			
			double dtsOffsetSeconds = 0;
		
			if(dts != AV_NOPTS_VALUE) {

				// packet has a dts value, subtract from subsequent dts/pts values
				dtsOffsetSeconds = -input->getStream(streamIndex)->getTimeSeconds(dts);
			
			} else if(pts != AV_NOPTS_VALUE) {

				// packet only has a pts value, subtract from subsequent dts/pts values
				dtsOffsetSeconds = -input->getStream(streamIndex)->getTimeSeconds(pts);

			} else {

				return;
			}

			streamInfo[streamIndex]->offsetSet = true;

			// check if the pts/dts value of the current packet is smaller as the current smallest value we found
			// if so use this value instead
			if(dtsOffsetSeconds > smallestOffset) {

				smallestOffset = dtsOffsetSeconds;

			} else {

				dtsOffsetSeconds = smallestOffset;		
			}
			   
			for(unsigned int i = 0; i < input->getNrStreams(); i++) {

				if(streamInfo[i]->outStreamIdx != DISCARD_STREAM) {

					streamInfo[i]->dtsOffset = input->getStream(i)->getTimeBaseUnits(dtsOffsetSeconds);
				}
			}
					
		}

		double getEndTimeSeconds() const {

			return(endTimeSeconds);
		}
		
		double getStartTimeSeconds() const {

			return(startTimeSeconds);
		}

		bool hasEveryStreamPassedEndTime() const {

			bool isPassedEndTimeRange = true;

			for(unsigned int i = 0; i < streamInfo.size(); i++) {

				if(streamInfo[i]->outStreamIdx != DISCARD_STREAM) {

					isPassedEndTimeRange = isPassedEndTimeRange && (streamInfo[i]->posSeconds >= endTimeSeconds);
				}
			}

			return isPassedEndTimeRange;
		}

	};
}