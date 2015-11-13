#pragma once
#include "VideoDecoder.h"
#include "VideoEncoder.h"
#include "FilterGraph.h"
#include "BitStreamFilter.h"
#include "MuxedAVInputStream.h"

#define DISCARD_STREAM -1

using namespace MediaViewer::Infrastructure::Video::TranscodeOptions;
using namespace System::Collections::Generic;
using namespace msclr::interop;
using namespace MediaViewer::Infrastructure::Utils;
using namespace MediaViewer::Infrastructure::Logging;


namespace VideoLib {

class VideoTranscode {

protected:

	struct StreamInfo {

		StreamInfo(int outStreamIdx = DISCARD_STREAM) {

			dtsOffset = posSeconds = 0;			
			bitStreamFilter = NULL;
			filterGraph = NULL;		
			this->outStreamIdx = outStreamIdx;	
			offsetSet = false;
		}

		~StreamInfo() {

			if(bitStreamFilter != NULL) {
				delete bitStreamFilter;
				bitStreamFilter = NULL;
			}

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

	MuxedAVInputStream *input;
	VideoEncoder *output;

	std::vector<StreamInfo *> streamInfo;
	int inputStreamOffset;

	double smallestOffset;

	void setDtsOffsets(const AVPacket &packet) {
				
		//get the dts value of the first input packet and subtract it from subsequent dts & pts
		//values to make sure the output video starts at time zero.	
		if(streamInfo[packet.stream_index]->offsetSet == true) return;
			
		double dtsOffsetSeconds = 0;
		
		if(packet.dts != AV_NOPTS_VALUE) {

			// packet has a dts value, subtract from subsequent dts/pts values
			dtsOffsetSeconds = -input->getStream(packet.stream_index)->getTimeSeconds(packet.dts);
			
		} else if(packet.pts != AV_NOPTS_VALUE) {

			// packet only has a pts value, subtract from subsequent dts/pts values
			dtsOffsetSeconds = -input->getStream(packet.stream_index)->getTimeSeconds(packet.pts);

		} else {

			return;
		}

		streamInfo[packet.stream_index]->offsetSet = true;

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

public:

	typedef void (__stdcall *ProgressCallback)(double);

	VideoTranscode() {
		
		input = new MuxedAVInputStream();
		output = new VideoEncoder();		
	}

	~VideoTranscode()
	{
		delete input;
		delete output;
	}
				
	void transcode(String ^inputVideoLocation, String ^outputFilename, System::Threading::CancellationToken token, 
		Dictionary<String ^,Object ^> ^options, ProgressCallback progressCallback = NULL,
		String ^inputAudioLocation = nullptr) 
	{
		
		AVPacket *packet;
		
		AVFrame *frame = NULL;
		AVMediaType type;
		int ret = 0;	
				
		int got_frame;
		
		try {

			initialize(inputVideoLocation, outputFilename, options, token, inputAudioLocation);
			
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

			int i = 0;
			double progress = 0;
	
			smallestOffset = -DBL_MAX;
		
			// read all packets 
			while (1) {

				if(token.IsCancellationRequested) {

					throw gcnew VideoLib::VideoLibException("Cancelled transcoding: " + outputFilename);
				}

				if (input->readFrame(&packet) != VideoDecoder::ReadFrameResult::OK) {
					// finished
					break;
				}
						
				unsigned int inStreamIdx = packet->stream_index;	
				unsigned int outStreamIdx = streamInfo[inStreamIdx]->outStreamIdx;
				
				if(outStreamIdx != DISCARD_STREAM) {
					
					//get the dts value of the first input packet and subtract it from subsequent dts & pts
					//values to make sure the output video starts at time zero.										
					setDtsOffsets(*packet);
					
					// check if every stream has passed the end of timerange 
					// stop encoding if true
					streamInfo[inStreamIdx]->posSeconds = input->getStream(inStreamIdx)->getTimeSeconds(packet->pts);

					bool isPassedEndTimeRange = true;

					for(unsigned int j = 0; j < streamInfo.size(); j++) {

						if(streamInfo[j]->outStreamIdx != DISCARD_STREAM) {

							isPassedEndTimeRange = isPassedEndTimeRange && (streamInfo[j]->posSeconds > endTimeRange);
						}
					}
						
					if(isPassedEndTimeRange) break;
				}

				if(i++ % 500 && progressCallback != NULL) {
										
					double endTime = Math::Min(endTimeRange, input->getDurationSeconds());
					double totalSeconds = endTime - startTimeRange;

					double progress = (streamInfo[inStreamIdx]->posSeconds - startTimeRange) / totalSeconds; 

					progressCallback(progress);

				}
			
				type = input->getStream(inStreamIdx)->getCodecType();

				if(outStreamIdx == DISCARD_STREAM) {

					// discard packets from this input stream

				} else if(streamInfo[inStreamIdx]->filterGraph != NULL) {
										
					frame = av_frame_alloc();
					if (!frame) {

						throw gcnew VideoLib::VideoLibException("Out of memory");	
					}
					
					if(packet->dts != AV_NOPTS_VALUE) {

						packet->dts += streamInfo[inStreamIdx]->dtsOffset;
					}

					if(packet->pts != AV_NOPTS_VALUE) {

						packet->pts += streamInfo[inStreamIdx]->dtsOffset;
					}
								
					packet->dts = av_rescale_q_rnd(packet->dts,
						input->getStream(inStreamIdx)->getAVStream()->time_base,
						input->getStream(inStreamIdx)->getAVStream()->codec->time_base,
						(AVRounding)(AV_ROUND_NEAR_INF|AV_ROUND_PASS_MINMAX));

					packet->pts = av_rescale_q_rnd(packet->pts,
						input->getStream(inStreamIdx)->getAVStream()->time_base,
						input->getStream(inStreamIdx)->getAVStream()->codec->time_base,
						(AVRounding)(AV_ROUND_NEAR_INF|AV_ROUND_PASS_MINMAX));
					
					
					if(type == AVMEDIA_TYPE_VIDEO) {

						ret = input->getVideoDecoder()->decodeVideoFrame(frame, &got_frame, &(*packet));

					} else {

						ret = input->getAudioDecoder()->decodeAudioFrame(frame, &got_frame, &(*packet));
					}
				
					if (ret < 0) {

						av_frame_free(&frame);
						Logger::Log->Error("Error decoding input: " + inputVideoLocation);
						break;										
					}

					if (got_frame) {

						frame->pts = av_frame_get_best_effort_timestamp(frame);

						ret = filterEncodeWriteFrame(frame, inStreamIdx);
						av_frame_free(&frame);
						if (ret < 0) {
							return;
						}
													
					} else {

						av_frame_free(&frame);
					}

				} else {
													
					packet->stream_index = outStreamIdx;

					if(packet->dts != AV_NOPTS_VALUE) {

						packet->dts += streamInfo[inStreamIdx]->dtsOffset;
					}

					if(packet->pts != AV_NOPTS_VALUE) {

						packet->pts += streamInfo[inStreamIdx]->dtsOffset;
					}
					
					// remux this frame without reencoding 
					packet->dts = av_rescale_q_rnd(packet->dts,
						input->getStream(inStreamIdx)->getAVStream()->time_base,
						output->getFormatContext()->streams[outStreamIdx]->time_base,
						(AVRounding)(AV_ROUND_NEAR_INF | AV_ROUND_PASS_MINMAX));

					packet->pts = av_rescale_q_rnd(packet->pts,
						input->getStream(inStreamIdx)->getAVStream()->time_base,
						output->getFormatContext()->streams[outStreamIdx]->time_base,
						(AVRounding)(AV_ROUND_NEAR_INF|AV_ROUND_PASS_MINMAX));

					packet->duration = av_rescale_q(packet->duration, 
						input->getStream(inStreamIdx)->getAVStream()->time_base, 
						output->getFormatContext()->streams[outStreamIdx]->time_base);

					packet->pos = -1;

					output->writeEncodedPacket(&(*packet));				

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
		
			av_frame_free(&frame);

			for (unsigned int i = 0; i < streamInfo.size(); i++) {
				
				delete streamInfo[i];
				streamInfo[i] = NULL;							
			}

			streamInfo.clear();
						
			input->close();
			output->close();
		}		

	}

protected:


	void initOutputStream(Stream *inStream, StreamOptions streamMode, Dictionary<String ^, Object ^> ^options) 
	{

		if(streamMode == StreamOptions::Discard) {

			streamInfo.push_back(new StreamInfo());	
			return;
		}
													
		if(streamMode == StreamOptions::Copy) {

			output->createStream(inStream);

		} else {

			VideoLib::Stream *outStream;

			AVCodecContext *dec_ctx = inStream->getCodecContext();

			if(inStream->getCodecType() == AVMEDIA_TYPE_VIDEO) {

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
		}
		
		streamInfo.push_back(new StreamInfo(output->getNrStreams() - 1));	
				
	}

		
	void initialize(String ^inputVideoLocation, String ^outputFilename, Dictionary<String ^,Object ^> ^options, 
		System::Threading::CancellationToken token, String ^inputAudioLocation = nullptr) 
	{
		inputStreamOffset = 0;
		
		output->open(outputFilename);

		streamInfo.clear();	

		input->open(inputVideoLocation, token, inputAudioLocation);
										
		for(unsigned int i = 0; i < input->getNrStreams(); i++) {
				
			Stream *inStream = input->getStream(i);
											
			if (inStream->getCodecType() == AVMEDIA_TYPE_VIDEO || inStream->getCodecType() == AVMEDIA_TYPE_AUDIO) {

				StreamOptions streamMode = inStream->getCodecType() == AVMEDIA_TYPE_VIDEO ? 
					(StreamOptions)options["videoStreamMode"] : (StreamOptions)options["audioStreamMode"];
							
				initOutputStream(inStream, streamMode, options);
							
			} else if (inStream->getCodecType() == AVMEDIA_TYPE_UNKNOWN) {

				throw gcnew VideoLib::VideoLibException("Elementary stream is of unknown type, cannot proceed: " + outputFilename);

			} else {

				streamInfo.push_back(new StreamInfo());				
			}
			
		}
		
		output->writeHeader();

		initFilters(options);
	}

	int flushEncoder(unsigned int inStreamIdx)
	{		
		int outStreamIdx = streamInfo[inStreamIdx]->outStreamIdx;

		if (!(output->stream[outStreamIdx]->getCodec()->capabilities & CODEC_CAP_DELAY))
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
	
		if(streamInfo[inStreamIdx]->bitStreamFilter != NULL) {

			streamInfo[inStreamIdx]->bitStreamFilter->filterPacket(&enc_pkt, output->stream[outStreamIdx]->getCodecContext());
		}

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
											
			if (input->getStream(i)->getCodecType() == AVMEDIA_TYPE_VIDEO) {

				if((StreamOptions)options["videoStreamMode"] != StreamOptions::Encode) continue;

				streamInfo[i]->filterGraph = new FilterGraph(input->getVideoCodecContext(),
					output->getVideoCodecContext()->pix_fmt, SWS_BICUBIC);

				int outputFmt = (int)output->getVideoCodecContext()->pix_fmt;
				int inWidth = input->getVideoDecoder()->getWidth();
				int inHeight = input->getVideoDecoder()->getHeight();
				int outWidth = output->getVideoCodecContext()->width;
				int outHeight = output->getVideoCodecContext()->height;
					
				System::Drawing::Rectangle inRect(0,0,inWidth,inHeight);
				System::Drawing::Rectangle outRect(0,0,outWidth,outHeight);

				System::Drawing::Rectangle scaledRect = ImageUtils::stretchRectangle(inRect,outRect);
				System::Drawing::Rectangle centeredRect = ImageUtils::centerRectangle(outRect, scaledRect);

				String ^videoGraph = "scale=" + centeredRect.Width + "x" + centeredRect.Height + 
					",pad=" + outWidth + ":" + outHeight + ":" + centeredRect.X + ":" + centeredRect.Y +
					",format=" + outputFmt;
				
				std::string value = msclr::interop::marshal_as<std::string>(videoGraph);

				streamInfo[i]->filterGraph->createGraph(value.c_str());				

			} else if(input->getStream(i)->getCodecType() == AVMEDIA_TYPE_AUDIO) {

				if((StreamOptions)options["audioStreamMode"] != StreamOptions::Encode) continue;

				streamInfo[i]->filterGraph = new FilterGraph(input->getAudioCodecContext(),
					output->getAudioCodecContext()->sample_fmt, output->getAudioCodecContext()->channel_layout,
					output->getAudioCodecContext()->sample_rate);

				String ^audioGraph = "anull";

				if(output->getAudioCodecContext()->frame_size != 0) {

					audioGraph = "asetnsamples=n=" + output->getAudioCodecContext()->frame_size;
				}

				if(output->getAudioCodecContext()->sample_rate != input->getAudioDecoder()->getAudioCodecContext()->sample_rate)
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

				if(output->getAudioCodecContext()->codec_id == AV_CODEC_ID_AAC) {

					streamInfo[i]->bitStreamFilter = new BitStreamFilter();
					streamInfo[i]->bitStreamFilter->add("aac_adtstoasc");
				}

			}
			
		}

		return 0;
	}

	

};

}