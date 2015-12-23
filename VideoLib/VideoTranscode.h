#pragma once
#include "VideoDecoderFactory.h"
#include "IVideoDecoder.h"
#include "VideoEncoder.h"
#include "FilterGraph.h"
#include "BitStreamFilter.h"
#include "VideoTranscodeStreamsInfo.h"

#define DISCARD_STREAM -1

using namespace MediaViewer::Infrastructure::Video::TranscodeOptions;
using namespace System::Collections::Generic;
using namespace msclr::interop;
using namespace MediaViewer::Infrastructure::Utils;
using namespace MediaViewer::Infrastructure::Logging;


namespace VideoLib {

class VideoTranscode {

protected:
	
	IVideoDecoder *input;
	VideoEncoder *output;

	VideoTranscodeStreamsInfo streamInfo;
	int inputStreamOffset;

public:

	typedef void (__stdcall *ProgressCallback)(double);

	VideoTranscode() {
		
		input = NULL;
		output = new VideoEncoder();	
				
	}

	~VideoTranscode()
	{
		if(input != NULL) {
			delete input;
		}
		delete output;		
	}
				
	void transcode(OpenVideoArgs ^openArgs, String ^outputFilename, System::Threading::CancellationToken token, 
		Dictionary<String ^,Object ^> ^options, ProgressCallback progressCallback = NULL) 
	{
		
		AVPacket packet;
		
		AVFrame *frame = NULL;
		AVMediaType type;
		int ret = 0;	
				
		int got_frame;
		
		try {

			initialize(openArgs, outputFilename, options, token);
			
			av_dump_format(output->getFormatContext(), 0, output->getFormatContext()->filename, 1);

			double startTimeRange = 0;

			if(options->ContainsKey("startTimeRange")) {

				startTimeRange = (double)options["startTimeRange"];

				bool result = input->seek(startTimeRange);				
				if(result == false) {
					
					throw gcnew VideoLib::VideoLibException("Could not search in input stream(s)");
				}
			}

			double endTimeRange = DBL_MAX;
			
			if(options->ContainsKey("endTimeRange")) {

				endTimeRange = (double)options["endTimeRange"];						
			}

			streamInfo.initialize(input, startTimeRange, endTimeRange);

			int i = 0;
			double progress = 0;
						
			// read all packets 
			while (1) {

				if(token.IsCancellationRequested) {

					token.ThrowIfCancellationRequested();
				}

				if (input->readFrame(&packet) != VideoDecoder::ReadFrameResult::OK) {
					// finished
					break;
				}
						
				unsigned int inStreamIdx = packet.stream_index;	
				unsigned int outStreamIdx = streamInfo[inStreamIdx]->outStreamIdx;
															
				type = input->getStream(inStreamIdx)->getCodecType();

				// discard packets from this input stream
				if(outStreamIdx == DISCARD_STREAM) continue;
												
				if(streamInfo[inStreamIdx]->filterGraph != NULL) {
					
					// transcode packets
					frame = av_frame_alloc();
					if (!frame) {

						throw gcnew VideoLib::VideoLibException("Out of memory");	
					}
																																
					if(type == AVMEDIA_TYPE_VIDEO) {

						ret = input->decodeVideoFrame(frame, &got_frame, &packet);

					} else {

						ret = input->decodeAudioFrame(frame, &got_frame, &packet);
					}
				
					if (ret < 0) {

						av_frame_free(&frame);
						Logger::Log->Error("Error decoding input: " + openArgs->VideoLocation);
						break;										
					}

					if (got_frame) {

						frame->pts = av_frame_get_best_effort_timestamp(frame);
						
						if(frame->pts == AV_NOPTS_VALUE) {

							throw gcnew VideoLib::VideoLibException("Cannot encode frame without pts value");	
						}

						ret = 0;

						// skip frames which are outside the specified timerange 
						double frameTimeSeconds = input->getStream(inStreamIdx)->getTimeSeconds(frame->pts);
						if(frameTimeSeconds >= streamInfo.getStartTimeSeconds()) {
							
							streamInfo.calcDtsOffsets(inStreamIdx, frame->pts, AV_NOPTS_VALUE);

							// subtract starting offset from frame pts value												
							frame->pts += streamInfo[inStreamIdx]->dtsOffset;

							// rescale pts from stream time base to codec time base
							frame->pts = av_rescale_q_rnd(frame->pts,
								input->getStream(inStreamIdx)->getAVStream()->time_base,
								input->getStream(inStreamIdx)->getAVStream()->codec->time_base,
								(AVRounding)(AV_ROUND_NEAR_INF|AV_ROUND_PASS_MINMAX));
														
							ret = filterEncodeWriteFrame(frame, inStreamIdx);
						}

						av_frame_free(&frame);

						if(streamInfo.hasEveryStreamPassedEndTime()) break;
																
						if (ret < 0) {
							return;
						}
													
					} else {

						av_frame_free(&frame);
					}

				} else {
					
					//get the dts value of the first input packet and subtract it from subsequent dts & pts
					//values to make sure the output video starts at time zero.										
					streamInfo.calcDtsOffsets(packet.stream_index, packet.pts, packet.dts);

					if(streamInfo.hasEveryStreamPassedEndTime()) break;

					// copy packets					
					packet.stream_index = outStreamIdx;

					// subtract starting offset from packet pts and dts values
					if(packet.dts != AV_NOPTS_VALUE) {

						packet.dts += streamInfo[inStreamIdx]->dtsOffset;
					}

					if(packet.pts != AV_NOPTS_VALUE) {

						packet.pts += streamInfo[inStreamIdx]->dtsOffset;
					}
									
					// remux this frame without reencoding 
					packet.dts = av_rescale_q_rnd(packet.dts,
						input->getStream(inStreamIdx)->getAVStream()->time_base,
						output->getFormatContext()->streams[outStreamIdx]->time_base,
						(AVRounding)(AV_ROUND_NEAR_INF | AV_ROUND_PASS_MINMAX));

					packet.pts = av_rescale_q_rnd(packet.pts,
						input->getStream(inStreamIdx)->getAVStream()->time_base,
						output->getFormatContext()->streams[outStreamIdx]->time_base,
						(AVRounding)(AV_ROUND_NEAR_INF|AV_ROUND_PASS_MINMAX));

					packet.duration = av_rescale_q(packet.duration, 
						input->getStream(inStreamIdx)->getAVStream()->time_base, 
						output->getFormatContext()->streams[outStreamIdx]->time_base);

					packet.pos = -1;
										
					streamInfo[inStreamIdx]->bitStreamFilter->filterPacket(&packet, output->getStream(outStreamIdx)->getCodecContext());
					
					output->writeEncodedPacket(&packet);											

				}

				av_packet_unref(&packet);

				if(i++ % 500 && progressCallback != NULL) {
										
					double endTime = Math::Min(endTimeRange, input->getDurationSeconds());
					double totalSeconds = endTime - startTimeRange;

					double progress = (streamInfo[inStreamIdx]->posSeconds - startTimeRange) / totalSeconds; 

					progressCallback(progress);

				}
				
			}

			// flush filters and encoders 
			for(unsigned int i = 0; i < input->getNrStreams(); i++) 
			{
				if (streamInfo[i]->filterGraph == NULL) continue;

				// flush filter 					
				ret = filterEncodeWriteFrame(NULL, i);
				if (ret < 0) {

					throw gcnew VideoLib::VideoLibException("Flushing filter failed.");			
				}

				// flush encoder 
				ret = flushEncoder(i);
				if (ret < 0) {

					throw gcnew VideoLib::VideoLibException("Flushing encoder failed.");					
				}
			}

			output->writeTrailer();

		} finally {
		
			av_packet_unref(&packet);
			av_frame_free(&frame);
			
			streamInfo.clear();
						
			input->close();
			output->close();
		}		

	}

protected:


	void initOutputStream(Stream *inStream, StreamOptions streamMode, Dictionary<String ^, Object ^> ^options) 
	{
				
		if(streamMode == StreamOptions::Discard) {

			streamInfo.add();
		
		} else if(streamMode == StreamOptions::Copy) {

			output->createStream(inStream);

			int outStreamIdx = output->getNrStreams() - 1;

			streamInfo.add(outStreamIdx);
			
		} else {

			VideoLib::Stream *outStream;

			AVCodecContext *dec_ctx = inStream->getCodecContext();

			if(inStream->isVideo()) {

				std::string encoderName = marshal_as<std::string>(((VideoEncoders)options["videoEncoder"]).ToString());
				 
				int width = dec_ctx->width;
				int height = dec_ctx->height;

				if(options->ContainsKey("width")) {

					width = (int)options["width"];

					if(!options->ContainsKey("height")) {

						height = int(((float)width / dec_ctx->width) * dec_ctx->height);
					}

				} 
				
				if(options->ContainsKey("height")) {

					height = (int)options["height"];

					if(!options->ContainsKey("width")) {

						width = int(((float)height / dec_ctx->height) * dec_ctx->width);
					}
				}

				outStream = output->createStream(encoderName, width, height, dec_ctx->sample_aspect_ratio, dec_ctx->time_base);
								
				if(outStream->getCodecID() == AV_CODEC_ID_H264 || outStream->getCodecID() == AV_CODEC_ID_HEVC) 
				{						
					String ^preset = ((VideoEncoderPresets)options["videoEncoderPreset"]).ToString()->ToLower();

					std::string value = marshal_as<std::string>(preset);

					outStream->setPrivateOption("preset", value.c_str());
				
				} else if(outStream->getCodecID() == AV_CODEC_ID_VP8) {
			
					outStream->getCodecContext()->bit_rate = 3200000;

					//outStream->setOption("crf", "5");
					outStream->setOption("qmin", "4");
					outStream->setOption("qmax", "50");

				} else if(outStream->getCodecID() == AV_CODEC_ID_GIF) {
								
					
				}

				
			} else {

				std::string encoderName = marshal_as<std::string>(((AudioEncoders)options["audioEncoder"]).ToString());

				int sampleRate = dec_ctx->sample_rate;

				if(options->ContainsKey("sampleRate")) {

					sampleRate = (int)options["sampleRate"];
				}

				int nrChannels = dec_ctx->channels;

				if(options->ContainsKey("nrChannels")) {

					nrChannels = (int)options["nrChannels"];
				} 

				outStream = output->createStream(encoderName, sampleRate, nrChannels);		
			}
								
			outStream->open();
			
			int outStreamIdx = output->getNrStreams() - 1;

			streamInfo.add(outStreamIdx);
		}			
						
	}

		
	void initialize(OpenVideoArgs ^openArgs, String ^outputFilename, Dictionary<String ^,Object ^> ^options, 
		System::Threading::CancellationToken token) 
	{
		inputStreamOffset = 0;
		
		output->open(outputFilename);

		streamInfo.clear();	

		input = VideoDecoderFactory::create(input, openArgs);

		input->open(openArgs, token);
										
		for(unsigned int i = 0; i < input->getNrStreams(); i++) {
				
			Stream *inStream = input->getStream(i);
											
			if ((inStream->isVideo() && i == input->getVideoStreamIndex())
				|| (inStream->isAudio() && i == input->getAudioStreamIndex())) {

				StreamOptions streamMode = inStream->getCodecType() == AVMEDIA_TYPE_VIDEO ? 
					(StreamOptions)options["videoStreamMode"] : (StreamOptions)options["audioStreamMode"];
							
				initOutputStream(inStream, streamMode, options);
							
			} else if (inStream->getCodecType() == AVMEDIA_TYPE_UNKNOWN) {

				throw gcnew VideoLib::VideoLibException("Elementary stream is of unknown type, cannot proceed: " + outputFilename);

			} else {

				streamInfo.add();				
			}
			
		}
		
		output->writeHeader();

		initFilters(options);
	}

	int flushEncoder(unsigned int inStreamIdx)
	{		
		int outStreamIdx = streamInfo[inStreamIdx]->outStreamIdx;

		if (!(output->getStream(outStreamIdx)->getCodec()->capabilities & CODEC_CAP_DELAY))
		{
			return 0;
		}

		while (encodeWriteFrame(NULL, inStreamIdx)) {}

		return 0;
	}

	bool encodeWriteFrame(AVFrame *filt_frame, unsigned int inStreamIdx) {
	
		AVPacket enc_pkt;

		int outStreamIdx = streamInfo[inStreamIdx]->outStreamIdx;

		bool gotPacket = output->encodeFrame(outStreamIdx, filt_frame, &enc_pkt);
	
		if (gotPacket == false) return false;					

		// prepare packet for muxing 		

		enc_pkt.dts = av_rescale_q_rnd(enc_pkt.dts,
			output->getFormatContext()->streams[outStreamIdx]->codec->time_base,
			output->getFormatContext()->streams[outStreamIdx]->time_base,
			(AVRounding)(AV_ROUND_NEAR_INF|AV_ROUND_PASS_MINMAX));

		enc_pkt.pts = av_rescale_q_rnd(enc_pkt.pts,
			output->getFormatContext()->streams[outStreamIdx]->codec->time_base,
			output->getFormatContext()->streams[outStreamIdx]->time_base,
			(AVRounding)(AV_ROUND_NEAR_INF|AV_ROUND_PASS_MINMAX));

		enc_pkt.duration = av_rescale_q(enc_pkt.duration,
			output->getFormatContext()->streams[outStreamIdx]->codec->time_base,
			output->getFormatContext()->streams[outStreamIdx]->time_base);
		
		streamInfo[inStreamIdx]->bitStreamFilter->filterPacket(&enc_pkt, output->getStream(outStreamIdx)->getCodecContext());
		
		output->writeEncodedPacket(&enc_pkt);	

		return true;
	}

	int filterEncodeWriteFrame(AVFrame *frame, unsigned int inStreamIdx)
	{
				
		// push frame trough filtergraph
		streamInfo[inStreamIdx]->filterGraph->pushFrame(frame);
			
		// pull filtered frames from the filtergraph
		while (1) {

			AVFrame *filteredFrame = av_frame_alloc();
			if (!filteredFrame) {

				throw gcnew VideoLib::VideoLibException("Not enough memory");				
			}

			try {

				bool success = streamInfo[inStreamIdx]->filterGraph->pullFrame(filteredFrame);			
				if (success == false) {
				
					return(0);
				}
		
				encodeWriteFrame(filteredFrame, inStreamIdx);
				
			} finally {

				av_frame_free(&filteredFrame);
			}
		}

		return(0);
	}

	int initFilters(Dictionary<String ^,Object ^> ^options)
	{		
		unsigned int i;
						
		for (i = 0; i < input->getNrStreams(); i++) {
											
			if (input->getStream(i)->isVideo() && i == input->getVideoStreamIndex()) {

				if((StreamOptions)options["videoStreamMode"] != StreamOptions::Encode) continue;

				streamInfo[i]->filterGraph = new FilterGraph(input->getVideoCodecContext(),
					output->getVideoCodecContext()->pix_fmt, SWS_BICUBIC);

				int outputFmt = (int)output->getVideoCodecContext()->pix_fmt;
				int inWidth = input->getWidth();
				int inHeight = input->getHeight();
				int outWidth = output->getVideoCodecContext()->width;
				int outHeight = output->getVideoCodecContext()->height;
					
				System::Drawing::Rectangle inRect(0,0,inWidth,inHeight);
				System::Drawing::Rectangle outRect(0,0,outWidth,outHeight);
				
				System::Drawing::Rectangle centeredRect;
				
				if(options->ContainsKey("width") && options->ContainsKey("height")) {

					// maintain aspect ratio of input
					System::Drawing::Rectangle scaledRect = ImageUtils::stretchRectangle(inRect,outRect);
					centeredRect = ImageUtils::centerRectangle(outRect, scaledRect);

				} else {

					centeredRect = outRect;
				}
				
				String ^videoGraph = "scale=" + centeredRect.Width + "x" + centeredRect.Height;

				if(centeredRect.Width != outWidth || centeredRect.Height != outHeight) 
				{
					// create a centered rectangle to maintain input aspect ratio using the padding filter			
					videoGraph += ",pad=" + outWidth + ":" + outHeight + ":" + centeredRect.X + ":" + centeredRect.Y;
				}
				
				videoGraph += ",format=" + outputFmt;				
				
				if(options->ContainsKey("framesPerSecond")) {

					float newFps = (float)options["framesPerSecond"];
					float inFps = input->getFrameRate();

					String ^scalePts = (inFps / newFps).ToString(System::Globalization::CultureInfo::InvariantCulture);
					
					videoGraph += ",fps=" + newFps + ",setpts=" + scalePts + "*PTS";
				}

				std::string value = msclr::interop::marshal_as<std::string>(videoGraph);

				streamInfo[i]->filterGraph->createGraph(value.c_str());				

			} else if(input->getStream(i)->isAudio() && i == input->getAudioStreamIndex()) {

				StreamOptions audioStreamMode = (StreamOptions)options["audioStreamMode"];

				initAudioBitstreamFilters(i, audioStreamMode);

				if(audioStreamMode != StreamOptions::Encode) continue;
							
				streamInfo[i]->filterGraph = new FilterGraph(input->getAudioCodecContext(),
					output->getAudioCodecContext()->sample_fmt, output->getAudioCodecContext()->channel_layout,
					output->getAudioCodecContext()->sample_rate);

				String ^audioGraph = "anull";

				if(output->getAudioCodecContext()->frame_size != 0) {

					audioGraph = "asetnsamples=n=" + output->getAudioCodecContext()->frame_size;
				}

				if(output->getAudioCodecContext()->sample_rate != input->getAudioCodecContext()->sample_rate)
				{
					if(audioGraph->Equals("anull")) {

						audioGraph = "";

					} else {

						audioGraph += ",";
					}

					audioGraph = "aresample=" + output->getAudioCodecContext()->sample_rate;
				}

				std::string value = msclr::interop::marshal_as<std::string>(audioGraph);

				streamInfo[i]->filterGraph->createGraph(value.c_str());	
				
			}
			
		}

		return 0;
	}

	void initAudioBitstreamFilters(int inputStreamIdx, StreamOptions audioStreamMode) {

		if(audioStreamMode == StreamOptions::Discard) 
		{					
			return;
		}
		else 
		{
			// add aac_adtstoasc bitstream filter if output is mp4 aac and input is dts aac
			bool encodeNeedsBSFilter = audioStreamMode == StreamOptions::Encode &&
				output->getAudioCodecContext()->codec_id == AV_CODEC_ID_AAC &&
				strcmp(output->getOutputFormat()->name,"mp4") == 0;
			
			bool copyNeedsBSFilter = audioStreamMode == StreamOptions::Copy &&
				output->getAudioCodecContext()->codec_id == AV_CODEC_ID_AAC &&
				strcmp(output->getOutputFormat()->name,"mp4") == 0 &&
				strcmp(input->getFormatContext()->iformat->name,"mpegts") == 0;
			
			if(encodeNeedsBSFilter || copyNeedsBSFilter) 
			{					
				streamInfo[inputStreamIdx]->bitStreamFilter->add("aac_adtstoasc");
			}
			
		} 
	}

	

};

}