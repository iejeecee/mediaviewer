#pragma once
#include <string>       
#include <iostream>  
#include <iomanip> 
#include <sstream> 
#include "VideoDecoderFactory.h"
#include "IVideoDecoder.h"
#include "VideoEncoder.h"
#include "VideoTransformer.h"
#include "Utils.h"

using namespace MediaViewer::Infrastructure::Video::TranscodeOptions;
using namespace System::Collections::Generic;
using namespace msclr::interop;
using namespace MediaViewer::Infrastructure::Utils;
using namespace MediaViewer::Infrastructure::Logging;

namespace VideoLib {

class VideoTranscodeBase : public VideoTransformer {

protected:
			
	StreamTransformMode streamOptionsToStreamTransformMode(StreamOptions mode) {

		switch (mode)
		{
		case MediaViewer::Infrastructure::Video::TranscodeOptions::StreamOptions::Discard:
			return(StreamTransformMode::DISCARD);
			break;
		case MediaViewer::Infrastructure::Video::TranscodeOptions::StreamOptions::Copy:
			return(StreamTransformMode::COPY);
			break;
		case MediaViewer::Infrastructure::Video::TranscodeOptions::StreamOptions::Encode:
			return(StreamTransformMode::ENCODE);
			break;
		default:
			throw gcnew VideoLibException("Unknown stream transform mode");
			break;
		}
	}

	bool getNextPacketSeq(AVPacket *packet, int &inputIdx, bool &isInputEOF)
	{
		isInputEOF = false;
				
		for(int i = 0; i < (int)inputs.size(); i++) {

			if(inputs[i]->getIsInputFinished() == false) {
				
				bool success = true;

				if(inputs[i]->packet.data == NULL) {

					success = inputs[i]->readFrame();
				}
			
				inputIdx = i;

				if(success == false) {
					
					isInputEOF = true;
					return true;
				}
				
				int result = av_packet_ref(packet, &inputs[i]->packet);
				if(result < 0) {

					throw gcnew VideoLibException("Error copying packet: ", result);
				}
			
				av_packet_unref(&inputs[i]->packet);			
			
				return(true);
			}
		}

		return(false);
	}

	
	void initOutputStream(Stream *inStream, VideoTransformerOutputInfo *output, StreamTransformMode streamMode, Dictionary<String ^, Object ^> ^options) 
	{						
		if(streamMode == StreamTransformMode::COPY) 
		{		
			output->encoder->createStream(inStream);						
		} 
		else if(streamMode == StreamTransformMode::ENCODE) 
		{			
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

				AVRational frameRate = dec_ctx->time_base;

				if(options->ContainsKey("framesPerSecond")) {

					float newFps = (float)options["framesPerSecond"];

					frameRate = av_make_q(1,(int)newFps);
				}

				outStream = output->encoder->createStream(encoderName, width, height, dec_ctx->sample_aspect_ratio, 
					frameRate);
								
				if(outStream->getCodecID() == AV_CODEC_ID_H264 || outStream->getCodecID() == AV_CODEC_ID_HEVC) 
				{						
					String ^preset = ((VideoEncoderPresets)options["videoEncoderPreset"]).ToString()->ToLower();

					std::string value = marshal_as<std::string>(preset);

					outStream->setOption("preset", value.c_str());
				
				} else if(outStream->getCodecID() == AV_CODEC_ID_VP8) {
			
					outStream->getCodecContext()->bit_rate = 3200000;

					//outStream->setOption("crf", "5");
					outStream->setOption("qmin", "4");
					outStream->setOption("qmax", "50");

				} else if(outStream->getCodecID() == AV_CODEC_ID_GIF) {
								
					
				} else if(outStream->getCodecID() == AV_CODEC_ID_APNG) {

					// loop animation
					//Utils::printOpts((void *)outStream->getCodec());
					//outStream->setOption("plays", "0");
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

				outStream = output->encoder->createStream(encoderName, sampleRate, nrChannels);		
			}
								
			outStream->open();
					
		}			
						
	}

	virtual void initBitstreamFilters(VideoTransformerInputInfo *input,
		VideoTransformerOutputInfo *output, int inputStreamIdx, int outputStreamIdx, StreamTransformMode mode) 
	{
		if(output->encoder->getStream(outputStreamIdx)->isVideo()) {
		
			if(output->encoder->getStream(outputStreamIdx)->getCodecID() == AV_CODEC_ID_MPEG4) {
			
				VideoInit::writeToLog(AV_LOG_DEBUG, "adding mpeg4_unpack_bframes bitstream filter");

				output->streamsInfo[outputStreamIdx]->addBitstreamFilter("mpeg4_unpack_bframes");							
			}

			bool copyNeedsAnnexbFilter = mode == StreamTransformMode::COPY &&
				output->encoder->getStream(outputStreamIdx)->getCodecID() == AV_CODEC_ID_H264
				&& output->encoder->getStream(outputStreamIdx)->getCodecContext()->extradata_size < 4;

			if(copyNeedsAnnexbFilter) {

				VideoInit::writeToLog(AV_LOG_DEBUG, "adding h264_mp4toannexb bitstream filter");

				output->streamsInfo[outputStreamIdx]->addBitstreamFilter("h264_mp4toannexb");
			}


		} else {

				// add aac_adtstoasc bitstream filter if output is mp4 aac and input is dts aac
			bool encodeNeedsAACFilter = mode == StreamTransformMode::ENCODE &&
				output->encoder->getStream(outputStreamIdx)->getCodecID() == AV_CODEC_ID_AAC &&
				strcmp(output->encoder->getOutputFormat()->name,"mp4") == 0;
			
			bool copyNeedsAACFilter = mode == StreamTransformMode::COPY &&
				output->encoder->getStream(outputStreamIdx)->getCodecID() == AV_CODEC_ID_AAC &&
				strcmp(output->encoder->getOutputFormat()->name,"mp4") == 0 &&
				strcmp(input->decoder->getFormatContext()->iformat->name,"mpegts") == 0;
			
			if(encodeNeedsAACFilter || copyNeedsAACFilter) 
			{	
				VideoInit::writeToLog(AV_LOG_DEBUG, "adding aac_adtstoasc bitstream filter");

				output->streamsInfo[outputStreamIdx]->addBitstreamFilter("aac_adtstoasc");
			}

		}
	}

	

	

};

}