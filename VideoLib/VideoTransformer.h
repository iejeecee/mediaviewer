#pragma once
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

protected:
	
	std::vector<VideoTransformerInput *> inputs;
	std::vector<VideoTransformerOutput *> outputs;
		
	FilterGraph *videoFilterGraph;
	FilterGraph *audioFilterGraph;

	bool getNextPacketSeqOrder(AVPacket *packet, int &inputIdx)
	{
		for(int i = 0; i < inputs.size(); i++) {

			if(inputs[i]->getIsInputFinished() == false) {
				
				bool success = true;

				if(inputs[i]->packet.data == NULL) {

					success = inputs[i]->readFrame();
				}
			
				if(success == false) continue;

				int result = av_packet_ref(packet, &inputs[i]->packet);
				if(result < 0) {

					throw gcnew VideoLibException("Error copying packet: ", result);
				}

				av_packet_unref(&inputs[i]->packet);			
				inputIdx = i;

				return(true);
			}
		}

		return(false);
	}

	bool getNextPacketDTSOrder(AVPacket *packet, int &inputIdx) {

		// select the packet with the smallest dts value from all inputs
		// as the next packet to process

		AVPacket *nextPacket = NULL;
		double smallestDts = DBL_MAX;

		for(int i = 0; i < inputs.size(); i++) {

			if(inputs[i]->getIsInputFinished() == false) {
				
				bool success = true;

				if(inputs[i]->packet.data == NULL) {

					success = inputs[i]->readFrame();
				}
			
				if(success == false) continue;

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
	}

	typedef void (__stdcall *ProgressCallback)(double);
	typedef bool (VideoLib::VideoTransformer::*GetNextPacketFunc)(AVPacket *,int &);

	VideoTransformer() {
			
		videoFilterGraph = new FilterGraph();
		audioFilterGraph = new FilterGraph();
	}

	virtual ~VideoTransformer()
	{
		for(int i = 0; i < inputs.size(); i++) {

			delete inputs[i];
		}
		inputs.clear();

		for(int i = 0; i < outputs.size(); i++) {

			delete outputs[i];
		}
		outputs.clear();

		delete videoFilterGraph;
		delete audioFilterGraph;
	}

	void addInput(VideoTransformerInputInfo *inputInfo) {

		inputs.push_back(new VideoTransformerInput(inputInfo));	

		for(int i = 0; i < inputInfo->decoder->getNrStreams(); i++) 
		{
			VideoLib::Stream *inStream = inputInfo->decoder->getStream(i);

			if(inputInfo->streamsInfo[i]->mode == StreamTransformMode::ENCODE) {

				if(inStream->isVideo()) 
				{
					videoFilterGraph->addInputStream(inStream, inputInfo->streamsInfo[i]->name);
				} 
				else if(inStream->isAudio())
				{
					audioFilterGraph->addInputStream(inStream, inputInfo->streamsInfo[i]->name);
				}

			}
		}
	}

	void addOutput(VideoTransformerOutputInfo *outputInfo) {

		outputs.push_back(new VideoTransformerOutput(outputInfo));

		for(int i = 0; i < outputInfo->encoder->getNrStreams(); i++) 
		{
			VideoLib::Stream *outStream = outputInfo->encoder->getStream(i);

			if(outputInfo->streamsInfo[i]->mode == StreamTransformMode::ENCODE) {

				if(outStream->isVideo()) 
				{
					videoFilterGraph->addOutputStream(outStream, outputInfo->streamsInfo[i]->name);
				} 
				else if(outStream->isAudio())
				{
					audioFilterGraph->addOutputStream(outStream, outputInfo->streamsInfo[i]->name);
				}

			}
		}
	}
				
	void transform(System::Threading::CancellationToken token, ProgressCallback progressCallback = NULL,
		GetNextPacketFunc getNextPacket = &VideoTransformer::getNextPacketDTSOrder) 
	{				
		AVPacket packet;		
		AVFrame *frame = NULL;		
		int ret = 0;	
				
		int got_frame;
		unsigned int nrPackets = 0;
		
		try {

			for(int i = 0; i < outputs.size(); i++) {

				outputs[i]->encoder->writeHeader();
			}	

			for(int i = 0; i < inputs.size(); i++) {

				if(inputs[i]->startTimeRange > 0) {

					bool result = inputs[i]->decoder->seek(inputs[i]->startTimeRange);				
					if(result == false) {
					
						throw gcnew VideoLib::VideoLibException("Error searching in input");
					}

				}

			}
																		
			// read all packets 
			while (1) {

				if(token.IsCancellationRequested) {

					token.ThrowIfCancellationRequested();
				}

				int inputIdx;

				if ((this->*getNextPacket)(&packet, inputIdx) == false) 
				{
					// finished
					break;
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

						throw gcnew VideoLib::VideoLibException("Out of memory");	
					}
																																
					if(inStream->isVideo()) {

						ret = inputs[inputIdx]->decoder->decodeVideoFrame(frame, &got_frame, &packet);

					} else {

						ret = inputs[inputIdx]->decoder->decodeAudioFrame(frame, &got_frame, &packet);
					}
				
					if (ret < 0) {

						av_frame_free(&frame);
						throw gcnew VideoLibException("Error decoding input");															
					}

					if (got_frame) {

						frame->pts = av_frame_get_best_effort_timestamp(frame);
						
						if(frame->pts == AV_NOPTS_VALUE) {

							throw gcnew VideoLib::VideoLibException("Cannot encode frame without pts value");	
						}

						ret = 0;

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
														
							ret = filterEncodeWriteFrame(frame, inputIdx, inStreamIdx);
						}

						av_frame_free(&frame);
																						
						if (ret < 0) {

							return;
						}
													
					} else {

						av_frame_free(&frame);
					}

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
									
					// remux this frame without reencoding 
					packet.dts = av_rescale_q_rnd(packet.dts,
						inStream->getAVStream()->time_base,
						outStream->getAVStream()->time_base,
						(AVRounding)(AV_ROUND_NEAR_INF | AV_ROUND_PASS_MINMAX));

					packet.pts = av_rescale_q_rnd(packet.pts,
						inStream->getAVStream()->time_base,
						outStream->getAVStream()->time_base,
						(AVRounding)(AV_ROUND_NEAR_INF|AV_ROUND_PASS_MINMAX));

					packet.duration = av_rescale_q(packet.duration, 
						inStream->getAVStream()->time_base,
						outStream->getAVStream()->time_base);

					packet.pos = -1;
										
					outputs[outIdx]->streamsInfo[outStreamIdx]->bitStreamFilter->filterPacket(&packet, outStream->getCodecContext());
					
					outputs[outIdx]->encoder->writeEncodedPacket(&packet);											

				}

				av_packet_unref(&packet);

				if((nrPackets++ % 50 == 0) && progressCallback != NULL) {
										
					double progress = calculateProgress();

					progressCallback(progress);
				}
				
			}

			// flush filters and encoders 
			for(int inputIdx = 0; inputIdx < inputs.size(); inputIdx++) 
			{
				for(int inStreamIdx = 0; inStreamIdx < inputs[inputIdx]->decoder->getNrStreams(); inStreamIdx++) 
				{
					if (inputs[inputIdx]->streamsInfo[inStreamIdx]->mode != StreamTransformMode::ENCODE) continue;

					// flush filter 					
					ret = filterEncodeWriteFrame(NULL, inputIdx, inStreamIdx);
					if (ret < 0) {

						throw gcnew VideoLib::VideoLibException("Flushing filter failed.");			
					}

					// flush encoder 
					ret = flushEncoder(inputIdx, inStreamIdx);
					if (ret < 0) {

						throw gcnew VideoLib::VideoLibException("Flushing encoder failed.");					
					}
				}
			}

			for(int i = 0; i < outputs.size(); i++) {

				outputs[i]->encoder->writeTrailer();
			}	

		} finally {
		
			av_packet_unref(&packet);
			av_frame_free(&frame);
														
		}		

	}

	virtual double calculateProgress() {

		return 0;					
	}

	void initVideoFilterGraph(const char *filterSpec) {

		videoFilterGraph->createGraph(filterSpec);		
	}

	void initAudioFilterGraph(const char *filterSpec) {

		audioFilterGraph->createGraph(filterSpec);		
	}

private:

	int flushEncoder(int inputIdx, unsigned int inStreamIdx)
	{		
		int outIdx = inputs[inputIdx]->streamsInfo[inStreamIdx]->outputIndex;
		int outStreamIdx = inputs[inputIdx]->streamsInfo[inStreamIdx]->outputStreamIndex;
		VideoLib::Stream *outStream = outputs[outIdx]->encoder->getStream(outStreamIdx);

		if (!(outStream->getCodec()->capabilities & CODEC_CAP_DELAY))
		{
			return 0;
		}

		while (encodeWriteFrame(NULL, inputIdx, inStreamIdx) == true) {}

		return 0;
	}

	bool encodeWriteFrame(AVFrame *filt_frame, int inputIdx, int inStreamIdx) {
	
		AVPacket enc_pkt;

		int outIdx = inputs[inputIdx]->streamsInfo[inStreamIdx]->outputIndex;
		int outStreamIdx = inputs[inputIdx]->streamsInfo[inStreamIdx]->outputStreamIndex;
		VideoLib::Stream *outStream = outputs[outIdx]->encoder->getStream(outStreamIdx);

		bool gotPacket = outputs[outIdx]->encoder->encodeFrame(outStreamIdx, filt_frame, &enc_pkt);
	
		if (gotPacket == false) return false;					

		// prepare packet for muxing 		

		enc_pkt.dts = av_rescale_q_rnd(enc_pkt.dts,
			outStream->getCodecContext()->time_base,
			outStream->getAVStream()->time_base,
			(AVRounding)(AV_ROUND_NEAR_INF|AV_ROUND_PASS_MINMAX));

		enc_pkt.pts = av_rescale_q_rnd(enc_pkt.pts,
			outStream->getCodecContext()->time_base,
			outStream->getAVStream()->time_base,
			(AVRounding)(AV_ROUND_NEAR_INF|AV_ROUND_PASS_MINMAX));

		enc_pkt.duration = av_rescale_q(enc_pkt.duration,
			outStream->getCodecContext()->time_base,
			outStream->getAVStream()->time_base);
		
		outputs[outIdx]->streamsInfo[outStreamIdx]->bitStreamFilter->filterPacket(&enc_pkt, outStream->getCodecContext());
		
		System::Diagnostics::Debug::Print(enc_pkt.stream_index + " : " + outStream->getTimeSeconds(enc_pkt.dts));

		outputs[outIdx]->encoder->writeEncodedPacket(&enc_pkt);	
		
		return true;
	}

	int filterEncodeWriteFrame(AVFrame *frame, int inputIdx, int inStreamIdx)
	{
		char *inputName = inputs[inputIdx]->streamsInfo[inStreamIdx]->name;

		bool isVideo = inputs[inputIdx]->decoder->getStream(inStreamIdx)->isVideo();
			
		// push frame trough filtergraph
		if(isVideo) {

			videoFilterGraph->pushFrame(frame, inputName);

		} else {

			audioFilterGraph->pushFrame(frame, inputName);
		}
			
		// pull filtered frames from the filtergraph
		while (1) {

			AVFrame *filteredFrame = av_frame_alloc();
			if (!filteredFrame) {

				throw gcnew VideoLib::VideoLibException("Error allocating frame");				
			}

			try {

				bool success;
					
				if(isVideo) {

					success = videoFilterGraph->pullFrame(filteredFrame);			

				} else {

					success = audioFilterGraph->pullFrame(filteredFrame);		
				}

				if (success == false) {
				
					return(0);
				}
		
				encodeWriteFrame(filteredFrame, inputIdx, inStreamIdx);
				
			} finally {

				av_frame_free(&filteredFrame);
			}
		}

		return(0);
	}

};

}