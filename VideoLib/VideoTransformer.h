#pragma once
#include <string>       
#include <iostream>     
#include <sstream> 
#include "VideoDecoderFactory.h"
#include "IVideoDecoder.h"
#include "VideoEncoder.h"
#include "FilterGraph.h"
#include "BitStreamFilter.h"
#include "VideoTransformerInput.h"
#include "VideoTransformerOutput.h"
#include "Utils.h"
 
using namespace MediaViewer::Infrastructure::Video::TranscodeOptions;
using namespace System::Collections::Generic;
using namespace msclr::interop;
using namespace MediaViewer::Infrastructure::Utils;
using namespace MediaViewer::Infrastructure::Logging;

namespace VideoLib {

class VideoTransformer {

public:

	typedef void (__stdcall *ProgressCallback)(int, double);

protected:
	
	typedef bool (VideoLib::VideoTransformer::*GetNextPacketFunc)(AVPacket *,int &, bool &);

	std::vector<VideoTransformerInput *> inputs;
	std::vector<VideoTransformerOutput *> outputs;
		
	FilterGraph *filterGraph;
	

	// this should be the default function to use when all input streams are pushed trough 
	// a framegraph	
	bool getNextPacket(AVPacket *packet, int &inputIdx, bool &isInputEOF) {

		// request a output frame from the filtergraph
		filterGraph->requestFrame();
	
		// pick the media item(s) with the highest amount of failed request as the next
		// input to read a packet from
		int maxFailedRequests = -1;
		std::vector<int> potentialInputs;

		for(int i = 0; i < (int)inputs.size(); i++) {
			
			if(inputs[i]->getIsInputFinished()) continue;

			int inputMaxFailedRequests = -1;

			for(int j = 0; j < (int)inputs[i]->streamsInfo.size(); j++) 
			{
				if(inputs[i]->streamsInfo[j]->mode == StreamTransformMode::ENCODE) 
				{					
					int streamFailedRequests = filterGraph->getInputFailedRequests(inputs[i]->streamsInfo[j]->name.c_str());
					if(streamFailedRequests > inputMaxFailedRequests) {
						
						inputMaxFailedRequests = streamFailedRequests;
					}
				}
			}

			if(inputMaxFailedRequests > maxFailedRequests) {

				maxFailedRequests = inputMaxFailedRequests;
				potentialInputs.clear();

				potentialInputs.push_back(i);
			} 
			else if(inputMaxFailedRequests == maxFailedRequests)
			{
				potentialInputs.push_back(i);
			}
		}

		if(potentialInputs.empty()) return false;
		
		// select the packet with the smallest dts from the available potential inputs		
		AVPacket *nextPacket = NULL;
		double smallestDts = DBL_MAX;

		for(int i = 0; i < potentialInputs.size(); i++) {
					
			int curIdx = potentialInputs[i];
			bool success = true;
			
			if(inputs[curIdx]->packet.data == NULL) {

				success = inputs[curIdx]->readFrame();
			}
			
			if(success == false) {
			
				//System::Diagnostics::Debug::Print(curIdx + " IS EOF");

				inputIdx = curIdx;
				isInputEOF = true;
				return true;
			}

			AVPacket *curPacket = &inputs[curIdx]->packet;
			VideoLib::Stream *stream = inputs[curIdx]->decoder->getStream(curPacket->stream_index);

			//inputs[curIdx]->calcDtsOffsets(curPacket);
			
			double currentDts = stream->getTimeSeconds(curPacket->dts + inputs[curIdx]->streamsInfo[curPacket->stream_index]->tsOffset);

			//System::Diagnostics::Debug::Print(curIdx + " - " + maxFailedRequests + " " + currentDts);

			if(currentDts < smallestDts) {

				nextPacket = curPacket;
				inputIdx = curIdx;

				smallestDts = currentDts;
			}

		}

		int result = av_packet_ref(packet, nextPacket);
		if(result < 0) {

			throw gcnew VideoLibException("Error copying packet: ", result);
		}
			
		av_packet_unref(&inputs[inputIdx]->packet);	
		inputs[inputIdx]->packet.size = 0;
		inputs[inputIdx]->packet.data = NULL;

		return true;
	}
	
		
	VideoTransformer() {
			
		filterGraph = new FilterGraph();		
	}

	virtual ~VideoTransformer()
	{
		for(int i = 0; i < (int)inputs.size(); i++) {

			delete inputs[i];
		}
		inputs.clear();

		for(int i = 0; i < (int)outputs.size(); i++) {

			delete outputs[i];
		}
		outputs.clear();

		delete filterGraph;		
	}

	void addInput(VideoTransformerInputInfo *inputInfo) {

		int inputIdx = (int)inputs.size();
		int nrVideoStreams = 0;
		int nrAudioStreams = 0;

		inputs.push_back(new VideoTransformerInput(inputInfo));	

		for(int i = 0; i < inputInfo->decoder->getNrStreams(); i++) 
		{
			VideoLib::Stream *inStream = inputInfo->decoder->getStream(i);

			if(inputInfo->streamsInfo[i]->mode == StreamTransformMode::ENCODE) {
				
				std::stringstream name;

				if(inStream->isVideo()) {

					name << "vi" << inputIdx << nrVideoStreams++;

				} else {

					name << "ai" << inputIdx << nrAudioStreams++;
				}

				inputs[inputIdx]->streamsInfo[i]->name = name.str();

				filterGraph->addInputStream(inStream, name.str().c_str());				
			}
		}
	}

	void addOutput(VideoTransformerOutputInfo *outputInfo) {

		int outputIdx = (int)outputs.size();
		int nrVideoStreams = 0;
		int nrAudioStreams = 0;

		outputs.push_back(new VideoTransformerOutput(outputInfo));

		for(int i = 0; i < outputInfo->encoder->getNrStreams(); i++) 
		{
			VideoLib::Stream *outStream = outputInfo->encoder->getStream(i);

			if(outputInfo->streamsInfo[i]->mode == StreamTransformMode::ENCODE) {
				
				std::stringstream name;

				if(outStream->isVideo()) {

					name << "vo" << outputIdx << nrVideoStreams++;

				} else {

					name << "ao" << outputIdx << nrAudioStreams++;
				}

				outputs[outputIdx]->streamsInfo[i]->name = name.str();

				filterGraph->addOutputStream(outStream, name.str().c_str());				
			}
		}
	}
				
	void transform(System::Threading::CancellationToken token, ProgressCallback progressCallback = NULL,
		GetNextPacketFunc getNextPacketFunc = &VideoTransformer::getNextPacket) 
	{				
		AVPacket packet;				
		int ret = 0;	
					
		unsigned int nrPackets = 0;
		
		try {

			for(int i = 0; i < (int)outputs.size(); i++) {

				outputs[i]->encoder->writeHeader();
			}	

			for(int i = 0; i < (int)inputs.size(); i++) {

				if(inputs[i]->startTimeRange > 0) {

					bool result = inputs[i]->decoder->seek(inputs[i]->startTimeRange);				
					if(result == false) {
					
						throw gcnew VideoLib::VideoLibException("Error searching in input");
					}

				}

			}
																		
			// process packets 
			while (1) {
				
				token.ThrowIfCancellationRequested();
				
				int inputIdx;
				bool isInputEOF = false;

				if ((this->*getNextPacketFunc)(&packet, inputIdx, isInputEOF) == false)  
				{
					// all inputs are finished
					flushDecoders(inputIdx);
					break;
				}

				if(isInputEOF) {
				
					flushDecoders(inputIdx);
					// inform the filtergraph all streams from media item inputIdx are finished
					flushFilters(inputIdx);
					continue;
				}
					
				int inStreamIdx = packet.stream_index;
			
				int outIdx = inputs[inputIdx]->streamsInfo[inStreamIdx]->outputIndex;
				int outStreamIdx = inputs[inputIdx]->streamsInfo[inStreamIdx]->outputStreamIndex;
			
				if(inputs[inputIdx]->streamsInfo[inStreamIdx]->mode == StreamTransformMode::ENCODE) 
				{					
					decodeFilterFrame(inputIdx, &packet);					
				} 
				else if(inputs[inputIdx]->streamsInfo[inStreamIdx]->mode == StreamTransformMode::COPY) 
				{					
					copyPacket(inputIdx, &packet);									
				}

				av_packet_unref(&packet);

				if((nrPackets++ % 50 == 0) && progressCallback != NULL) {
										
					int inputIdx = 0;
					double progress = calculateProgress(inputIdx);

					progressCallback(inputIdx, progress);
				}
				
			}

			// flush filters
			for(int inputIdx = 0; inputIdx < (int)inputs.size(); inputIdx++) 
			{
				flushFilters(inputIdx);
			}

			// flush encoders
			for(int outputIdx = 0; outputIdx < (int)outputs.size(); outputIdx++) {

				for(int outStreamIdx = 0; outStreamIdx < outputs[outputIdx]->encoder->getNrStreams(); outStreamIdx++) 
				{
					if (outputs[outputIdx]->streamsInfo[outStreamIdx]->mode != StreamTransformMode::ENCODE) continue;
					
					flushEncoder(outputIdx, outStreamIdx);					
				}
				
				outputs[outputIdx]->encoder->writeTrailer();
			}	

		} finally {
		
			av_packet_unref(&packet);		
														
		}		

	}

	virtual void modifyTS(int inputIdx, int streamIdx, int64_t pts, int64_t dts, int64_t duration) {

	}

	virtual double calculateProgress(int &inputIdx) {

		inputIdx = 0;
		return 0;					
	}

	void initFilterGraph(const char *filterSpec) {

		filterGraph->createGraph(filterSpec);		
	}

	void clearTransformer() {

		for(int i = 0; i < (int)inputs.size(); i++) {

			delete inputs[i];
		}
		inputs.clear();

		for(int i = 0; i < (int)outputs.size(); i++) {

			delete outputs[i];
		}
		outputs.clear();

		filterGraph->clear();
	}

private:

	bool decodeFilterFrame(int inputIdx, AVPacket *packet)
	{
		// filter packets
		AVFrame *frame = av_frame_alloc();
		if (!frame) {

			throw gcnew VideoLib::VideoLibException("Error allocating frame");	
		}
	
		try 
		{
			int got_frame = 0;
			int ret = 0;

			int inStreamIdx = packet->stream_index;
			VideoLib::Stream *inStream = inputs[inputIdx]->decoder->getStream(inStreamIdx);	

			if(inStream->isVideo()) {

				ret = inputs[inputIdx]->decoder->decodeVideoFrame(frame, &got_frame, packet);

			} else {

				ret = inputs[inputIdx]->decoder->decodeAudioFrame(frame, &got_frame, packet);
			}
				
			if (ret < 0) {
					
				throw gcnew VideoLibException("Error decoding input");															
			}

			if (got_frame) {

				frame->pts = av_frame_get_best_effort_timestamp(frame);
						
				if(frame->pts == AV_NOPTS_VALUE) {

					throw gcnew VideoLib::VideoLibException("Cannot encode frame without pts value");	
				}
						
				// skip frames which are outside the specified timerange 
				double frameTimeSeconds = inStream->getTimeSeconds(frame->pts);
				if(frameTimeSeconds >= inputs[inputIdx]->startTimeRange) 
				{							
					modifyTS(inputIdx, inStreamIdx, frame->pts, AV_NOPTS_VALUE, av_frame_get_pkt_duration(frame));

					// subtract starting offset from frame pts value												
					frame->pts += inputs[inputIdx]->streamsInfo[inStreamIdx]->tsOffset;

					// rescale pts from stream time base to codec time base
					frame->pts = av_rescale_q_rnd(frame->pts,
						inStream->getAVStream()->time_base,
						inStream->getCodecContext()->time_base,
						(AVRounding)(AV_ROUND_NEAR_INF|AV_ROUND_PASS_MINMAX));
														
					filterEncodeWriteFrame(frame, inputIdx, inStreamIdx);
				}
			}

			if(ret == 0) {
		
				return(true);

			} else {

				return(false);
			}
				
		} finally {

			av_frame_free(&frame);
		}

	}

	void copyPacket(int inputIdx, AVPacket *packet) {

		int inStreamIdx = packet->stream_index;
		VideoLib::Stream *inStream = inputs[inputIdx]->decoder->getStream(inStreamIdx);	

		int outIdx = inputs[inputIdx]->streamsInfo[inStreamIdx]->outputIndex;
		int outStreamIdx = inputs[inputIdx]->streamsInfo[inStreamIdx]->outputStreamIndex;
		VideoLib::Stream *outStream = outputs[outIdx]->encoder->getStream(outStreamIdx);

		//get the dts value of the first input packet and subtract it from subsequent dts & pts
		//values to make sure the output video starts at time zero.
		modifyTS(inputIdx, inStreamIdx, packet->pts, packet->dts, packet->duration);					
								
		packet->stream_index = outStreamIdx;

		// subtract starting offset from packet pts and dts values
		if(packet->dts != AV_NOPTS_VALUE) {

			packet->dts += inputs[inputIdx]->streamsInfo[inStreamIdx]->tsOffset;
		}

		if(packet->pts != AV_NOPTS_VALUE) {

			packet->pts += inputs[inputIdx]->streamsInfo[inStreamIdx]->tsOffset;
		}
							
		rescaleTimeBase(packet, 
			inStream->getAVStream()->time_base, 
			outStream->getAVStream()->time_base);
					
		packet->pos = -1;
										
		outputs[outIdx]->streamsInfo[outStreamIdx]->bitStreamFilter->filterPacket(packet, outStream->getCodecContext());
					
		outputs[outIdx]->encoder->writeEncodedPacket(packet);		

	}

	void flushDecoders(int inputIdx)
	{
		for(int inStreamIdx = 0; inStreamIdx < inputs[inputIdx]->decoder->getNrStreams(); inStreamIdx++) 
		{
			if (inputs[inputIdx]->streamsInfo[inStreamIdx]->mode != StreamTransformMode::ENCODE) continue;
			
			bool finished = false;

			while(!finished) {

				AVPacket packet;

				av_init_packet(&packet);
				packet.data = NULL;
				packet.size = 0;
				packet.stream_index = inStreamIdx;

				finished = decodeFilterFrame(inputIdx, &packet);									
			}
		}

	}


	void flushFilters(int inputIdx) {

		for(int inStreamIdx = 0; inStreamIdx < inputs[inputIdx]->decoder->getNrStreams(); inStreamIdx++) 
		{
			if (inputs[inputIdx]->streamsInfo[inStreamIdx]->mode != StreamTransformMode::ENCODE) continue;
									
			filterEncodeWriteFrame(NULL, inputIdx, inStreamIdx);									
		}
	}

	void rescaleTimeBase(AVPacket *packet, AVRational currentTimeBase, AVRational newTimeBase)
	{
		packet->dts = av_rescale_q_rnd(packet->dts,
			currentTimeBase,
			newTimeBase,
			(AVRounding)(AV_ROUND_NEAR_INF|AV_ROUND_PASS_MINMAX));

		packet->pts = av_rescale_q_rnd(packet->pts,
			currentTimeBase,	
			newTimeBase,
			(AVRounding)(AV_ROUND_NEAR_INF|AV_ROUND_PASS_MINMAX));

		packet->duration = av_rescale_q(packet->duration,
			currentTimeBase,
			newTimeBase);

	}

	void flushEncoder(int outputIdx, unsigned int outStreamIdx)
	{		
		
		VideoLib::Stream *outStream = outputs[outputIdx]->encoder->getStream(outStreamIdx);

		if (!(outStream->getCodec()->capabilities & CODEC_CAP_DELAY))
		{
			return;
		}

		while (encodeWriteFrame(NULL, outputIdx, outStreamIdx) == true) {}
	
	}

	bool encodeWriteFrame(AVFrame *filt_frame, int outputIdx, int outStreamIdx) {
	
		AVPacket enc_pkt;
		
		VideoLib::Stream *outStream = outputs[outputIdx]->encoder->getStream(outStreamIdx);

		bool gotPacket = outputs[outputIdx]->encoder->encodeFrame(outStreamIdx, filt_frame, &enc_pkt);
	
		if (gotPacket == false) return false;					

		// prepare packet for muxing, convert packet pts/dts/duration from input to output timebase values 	
		AVFilterContext *outFilter = filterGraph->getFilter(outputs[outputIdx]->streamsInfo[outStreamIdx]->name.c_str());
		AVRational inputTimeBase = outFilter->inputs[0]->time_base;
		
		rescaleTimeBase(&enc_pkt, 
			inputTimeBase, 
			outStream->getAVStream()->time_base); 
		
		outputs[outputIdx]->streamsInfo[outStreamIdx]->bitStreamFilter->filterPacket(&enc_pkt, outStream->getCodecContext());
		
		//System::Diagnostics::Debug::Print(enc_pkt.stream_index + " : " + outStream->getTimeSeconds(enc_pkt.dts));

		outputs[outputIdx]->encoder->writeEncodedPacket(&enc_pkt);	
		
		return true;
	}

	void filterEncodeWriteFrame(AVFrame *frame, int inputIdx, int inStreamIdx)
	{
		const char *inputName = inputs[inputIdx]->streamsInfo[inStreamIdx]->name.c_str();
				
		// push frame trough filtergraph		
		filterGraph->pushFrame(frame, inputName);
			
		// pull filtered frames from the filtergraph outputs		
		for(int outputIdx = 0; outputIdx < (int)outputs.size(); outputIdx++) 
		{
			for(int outStreamIdx = 0; outStreamIdx < outputs[outputIdx]->encoder->getNrStreams(); outStreamIdx++) 
			{
				if(outputs[outputIdx]->streamsInfo[outStreamIdx]->mode != StreamTransformMode::ENCODE) continue;

				bool success;
				const char *name = outputs[outputIdx]->streamsInfo[outStreamIdx]->name.c_str();

				do
				{
					AVFrame *filteredFrame = av_frame_alloc();
					if (!filteredFrame) 
					{
						throw gcnew VideoLib::VideoLibException("Error allocating frame");				
					}

					success = filterGraph->pullFrame(filteredFrame, name);							
					if (success) 
					{				
						encodeWriteFrame(filteredFrame, outputIdx, outStreamIdx);
					}
															
					av_frame_free(&filteredFrame);					
				
				} while(success);
			}

		}			
	}

};

}