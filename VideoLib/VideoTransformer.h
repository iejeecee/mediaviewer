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
	
	std::vector<VideoTransformerInput *> inputs;
	std::vector<VideoTransformerOutput *> outputs;
		
	FilterGraph *filterGraph;

	bool getNextPacketSeqOrder(AVPacket *packet, int &inputIdx, bool &isInputEOF)
	{
		isInputEOF = false;
				
		for(int i = 0; i < (int)inputs.size(); i++) {

			if(inputs[i]->getIsInputFinished() == false) {
				
				bool success = true;

				if(inputs[i]->packet.data == NULL) {

					success = inputs[i]->readFrame();
				}
			
				if(success == false) {

					inputIdx = i;
					isInputEOF = true;
					return true;
				}

				int result = av_packet_ref(packet, &inputs[i]->packet);
				if(result < 0) {

					throw gcnew VideoLibException("Error copying packet: ", result);
				}
			
				av_packet_unref(&inputs[i]->packet);			
				inputIdx = i;

				int inStreamIdx = packet->stream_index;

				if(i > 0 && inputs[i]->streamsInfo[inStreamIdx]->mode == StreamTransformMode::COPY &&
					inputs[i]->streamsInfo[inStreamIdx]->dtsOffset == -DBL_MAX) 
				{					
					// set dts offset for copied streams
					double prevLength = inputs[i - 1]->streamsInfo[inStreamIdx]->nextDts;

					inputs[i]->streamsInfo[inStreamIdx]->dtsOffset = inputs[i - 1]->streamsInfo[inStreamIdx]->dtsOffset + prevLength;
				}
				
				if(inputs[i]->streamsInfo[inStreamIdx]->mode == StreamTransformMode::COPY) {
					
					//calculate next dts					
					inputs[i]->streamsInfo[inStreamIdx]->nextDts = packet->dts + packet->duration;
				}

				return(true);
			}
		}

		return(false);
	}

	bool getNextPacketRequestOrder(AVPacket *packet, int &inputIdx, bool &isInputEOF) {

		// read the next packet from the input media requested by the framegraph
		// this should be the default function to use when all input streams are pushed trough 
		// a framegraph
		int selectedInput = -1;
		int maxFailedRequests = -1;

		for(int i = 0; i < (int)inputs.size(); i++) {
			
			if(inputs[i]->getIsInputFinished()) continue;

			for(int j = 0; j < (int)inputs[i]->streamsInfo.size(); j++) 
			{
				if(inputs[i]->streamsInfo[j]->mode == StreamTransformMode::ENCODE) 
				{					
					int failedRequests = filterGraph->getInputFailedRequests(inputs[i]->streamsInfo[j]->name.c_str());
					if(failedRequests > maxFailedRequests) {

						selectedInput = i;
						maxFailedRequests = failedRequests;
					}
				}

			}
		}

		if(selectedInput = -1) return false;

		inputIdx = selectedInput;
		bool success = inputs[selectedInput]->readFrame();		
			
		if(success == false) {
		
			isInputEOF = true;
			return true;
		}

		int result = av_packet_ref(packet, &inputs[selectedInput]->packet);
		if(result < 0) {

			throw gcnew VideoLibException("Error copying packet: ", result);
		}
			
		av_packet_unref(&inputs[selectedInput]->packet);	

		return true;
	}

	/*bool getNextPacketDTSOrder(AVPacket *packet, int &inputIdx, bool &isInputEOF) {
		
		// select the packet with the smallest dts value from all inputs
		// as the next packet to process

		AVPacket *nextPacket = NULL;
		double smallestDts = DBL_MAX;

		for(int i = 0; i < (int)inputs.size(); i++) {

			if(inputs[i]->getIsInputFinished() == false) {
				
				bool success = true;

				if(inputs[i]->packet.data == NULL) {

					success = inputs[i]->readFrame();
				}
			
				if(success == false) {

					inputIdx = i;
					isInputEOF = true;
					return true;
				}

				AVPacket *curPacket = &inputs[i]->packet;
				VideoLib::Stream *stream = inputs[i]->decoder->getStream(curPacket->stream_index);

				double currentDts = stream->getTimeSeconds(curPacket->dts + inputs[i]->streamsInfo[curPacket->stream_index]->dtsOffset);

				if(currentDts < smallestDts) {

					nextPacket = curPacket;
					inputIdx = i;

					smallestDts = currentDts;
				}
			}
		}

		if(nextPacket == NULL) return false;

		int result = av_packet_ref(packet, nextPacket);
		if(result < 0) {

			throw gcnew VideoLibException("Error copying packet: ", result);
		}

		av_packet_unref(nextPacket);
		inputs[inputIdx]->readFrame();

		return(true);
	}*/

	
	typedef bool (VideoLib::VideoTransformer::*GetNextPacketFunc)(AVPacket *,int &, bool &);

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
		GetNextPacketFunc getNextPacket = &VideoTransformer::getNextPacketRequestOrder) 
	{				
		AVPacket packet;		
		AVFrame *frame = NULL;		
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

				if(token.IsCancellationRequested) {

					token.ThrowIfCancellationRequested();
				}

				int inputIdx;
				bool isInputEOF;

				if ((this->*getNextPacket)(&packet, inputIdx, isInputEOF) == false) 
				{
					// all inputs are finished
					break;
				}

				if(isInputEOF) {

					// inform the filtergraph all streams from media item inputIdx are finished
					flushFilters(inputIdx);
					continue;
				}
					
				int inStreamIdx = packet.stream_index;
				VideoLib::Stream *inStream = inputs[inputIdx]->decoder->getStream(inStreamIdx);	

				int outIdx = inputs[inputIdx]->streamsInfo[inStreamIdx]->outputIndex;
				int outStreamIdx = inputs[inputIdx]->streamsInfo[inStreamIdx]->outputStreamIndex;
				VideoLib::Stream *outStream = outputs[outIdx]->encoder->getStream(outStreamIdx);

				if(inputs[inputIdx]->streamsInfo[inStreamIdx]->mode == StreamTransformMode::ENCODE) {
					
					// filter packets
					frame = av_frame_alloc();
					if (!frame) {

						throw gcnew VideoLib::VideoLibException("Error allocating frame");	
					}
					
					int got_frame = 0;

					if(inStream->isVideo()) {

						ret = inputs[inputIdx]->decoder->decodeVideoFrame(frame, &got_frame, &packet);

					} else {

						ret = inputs[inputIdx]->decoder->decodeAudioFrame(frame, &got_frame, &packet);
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
							inputs[inputIdx]->calcDtsOffsets(inStreamIdx, frame->pts, AV_NOPTS_VALUE);

							// subtract starting offset from frame pts value												
							frame->pts += inputs[inputIdx]->streamsInfo[inStreamIdx]->dtsOffset;

							// rescale pts from stream time base to codec time base
							frame->pts = av_rescale_q_rnd(frame->pts,
								inStream->getAVStream()->time_base,
								inStream->getCodecContext()->time_base,
								(AVRounding)(AV_ROUND_NEAR_INF|AV_ROUND_PASS_MINMAX));
														
							filterEncodeWriteFrame(frame, inputIdx, inStreamIdx);
						}
					}
						
					av_frame_free(&frame);
					
				} else {
					
					//get the dts value of the first input packet and subtract it from subsequent dts & pts
					//values to make sure the output video starts at time zero.										
					inputs[inputIdx]->calcDtsOffsets(packet.stream_index, packet.pts, packet.dts);
					
					// copy packets					
					packet.stream_index = outStreamIdx;

					// subtract starting offset from packet pts and dts values
					if(packet.dts != AV_NOPTS_VALUE) {

						packet.dts += inputs[inputIdx]->streamsInfo[inStreamIdx]->dtsOffset;
					}

					if(packet.pts != AV_NOPTS_VALUE) {

						packet.pts += inputs[inputIdx]->streamsInfo[inStreamIdx]->dtsOffset;
					}
							
					rescaleTimeBase(&packet, 
						inStream->getAVStream()->time_base, 
						outStream->getAVStream()->time_base);
					
					packet.pos = -1;
										
					outputs[outIdx]->streamsInfo[outStreamIdx]->bitStreamFilter->filterPacket(&packet, outStream->getCodecContext());
					
					outputs[outIdx]->encoder->writeEncodedPacket(&packet);											

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
			av_frame_free(&frame);
														
		}		

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