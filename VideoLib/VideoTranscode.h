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

class VideoTranscode : public VideoTransformer {

protected:
	
	VideoTransformerInputInfo *input;
	VideoTransformerOutputInfo *output;	
	
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

public:
	

	VideoTranscode() {
		
		input = NULL;
		output = NULL;						
	}

	virtual ~VideoTranscode()
	{
		if(input != NULL) {

			delete input;
		}

		if(output != NULL) {

			delete output;			
		}
	}
				

	void transcode(OpenVideoArgs ^openArgs, String ^outputFilename, System::Threading::CancellationToken token, 
		Dictionary<String ^,Object ^> ^options, ProgressCallback progressCallback = NULL) 
	{			
	
		try {

			IVideoDecoder *decoder = VideoDecoderFactory::create(openArgs);
			decoder->open(openArgs, token);
						
			double startTimeRange = 0;

			if(options->ContainsKey("startTimeRange")) {

				startTimeRange = (double)options["startTimeRange"];				
			}

			double endTimeRange = DBL_MAX;
			
			if(options->ContainsKey("endTimeRange")) {

				endTimeRange = (double)options["endTimeRange"];						
			}

			input = new VideoTransformerInputInfo(decoder, startTimeRange,endTimeRange);

			VideoEncoder *encoder = new VideoEncoder();
			encoder->open(outputFilename);

			output = new VideoTransformerOutputInfo(encoder);

			initialize(options, token);
			
			addInput(input);
			addOutput(output);
						
			initFilters(options);

			av_dump_format(output->encoder->getFormatContext(), 0, output->encoder->getFormatContext()->filename, 1);
			
			transform(token, progressCallback, &VideoTranscode::getNextPacketSeqOrder);

		} finally {
			
			if(input != NULL) {
								
				delete input;			
				input = NULL;
			}
									
			if(output != NULL) 
			{			
				delete output;
				output = NULL;
			}
			
			clearTransformer();
		}		

	}

protected:

	virtual double calculateProgress(int &inputIdx) {

		double endTime = Math::Min(inputs[0]->endTimeRange, inputs[0]->decoder->getDurationSeconds());
		double totalSeconds = endTime - inputs[0]->startTimeRange;

		double progress = (inputs[0]->streamsInfo[0]->posSeconds - inputs[0]->startTimeRange) / totalSeconds; 

		return progress;
	}

	void initOutputStream(Stream *inStream, StreamTransformMode streamMode, Dictionary<String ^, Object ^> ^options) 
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

				outStream = output->encoder->createStream(encoderName, width, height, dec_ctx->sample_aspect_ratio, 
					dec_ctx->time_base);
								
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

				outStream = output->encoder->createStream(encoderName, sampleRate, nrChannels);		
			}
								
			outStream->open();
					
		}			
						
	}

		
	void initialize(Dictionary<String ^,Object ^> ^options, System::Threading::CancellationToken token) 
	{
								
		for(int i = 0; i < input->decoder->getNrStreams(); i++) {
				
			VideoTransformerInputStreamInfo *inputStreamInfo;

			Stream *inStream = input->decoder->getStream(i);
											
			if ((inStream->isVideo() && i == input->decoder->getVideoStreamIndex()) ||
				(inStream->isAudio() && i == input->decoder->getAudioStreamIndex()))
			{
				StreamTransformMode streamMode = inStream->isVideo() ? 
					streamOptionsToStreamTransformMode((StreamOptions)options["videoStreamMode"])
					: streamOptionsToStreamTransformMode((StreamOptions)options["audioStreamMode"]);

				int outStreamIdx = output->encoder->getNrStreams();

				inputStreamInfo = new VideoTransformerInputStreamInfo(i, outStreamIdx, streamMode);

				if(streamMode != StreamTransformMode::DISCARD) {

					VideoTransformerOutputStreamInfo *outputStreamInfo = 
						new VideoTransformerOutputStreamInfo(outStreamIdx, streamMode);

					output->addStreamInfo(outputStreamInfo);

					initOutputStream(inStream, streamMode, options);
					initBitstreamFilters(i, outStreamIdx, streamMode); 
				}
			} 
			else 
			{				
				inputStreamInfo = new VideoTransformerInputStreamInfo(i,-1, StreamTransformMode::DISCARD);
			}
			
			input->addStreamInfo(inputStreamInfo);
		}
		
	}

	void initFilters(Dictionary<String ^,Object ^> ^options)
	{		

		std::stringstream filterSpec;
		int nrAudioFilters = 0;
		int nrVideoFilters = 0;
								
		for (int i = 0; i < input->decoder->getNrStreams(); i++) {
						
			if (input->decoder->getStream(i)->isVideo() && i == input->decoder->getVideoStreamIndex()) {

				StreamOptions videoStreamMode = (StreamOptions)options["videoStreamMode"];	
				
				if(videoStreamMode != StreamOptions::Encode) continue;
																	
				int inWidth = input->decoder->getWidth();
				int inHeight = input->decoder->getHeight();
				int outWidth = output->encoder->getVideoCodecContext()->width;
				int outHeight = output->encoder->getVideoCodecContext()->height;
					
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
							
				std::stringstream videoGraph;

				videoGraph << "[" << inputs[0]->streamsInfo[i]->name << "] ";

				if(inWidth != outWidth || inHeight != outHeight) {

					if(nrVideoFilters++ > 0) videoGraph << ",";

					videoGraph << "scale=" << centeredRect.Width << "x" << centeredRect.Height;
				}

				if(centeredRect.Width != outWidth || centeredRect.Height != outHeight) 
				{
					if(nrVideoFilters++ > 0) videoGraph << ",";

					// create a centered rectangle to maintain input aspect ratio using the padding filter			
					videoGraph << "pad=" << outWidth << ":" << outHeight << ":" << centeredRect.X << ":" << centeredRect.Y;
				}
															
				if(options->ContainsKey("framesPerSecond")) {

					if(nrVideoFilters++ > 0) videoGraph << ",";

					float newFps = (float)options["framesPerSecond"];
					float inFps = input->decoder->getFrameRate();

					double scalePts = inFps / newFps;
					
					videoGraph << std::setprecision(4) << "fps=" << newFps << ",setpts=" << scalePts << "*PTS";
				}
			
				if(nrVideoFilters++ == 0) videoGraph << "null";

				int outStreamIdx = inputs[0]->streamsInfo[i]->outputStreamIndex;

				videoGraph << " [" << outputs[0]->streamsInfo[outStreamIdx]->name << "]";

				if(nrAudioFilters > 0) filterSpec << "; ";
				
				filterSpec << videoGraph.str();				

			} else if(input->decoder->getStream(i)->isAudio() && i == input->decoder->getAudioStreamIndex()) {

				StreamOptions audioStreamMode = (StreamOptions)options["audioStreamMode"];		
				
				if(audioStreamMode != StreamOptions::Encode) continue;
							
				std::stringstream audioGraph;

				audioGraph << "[" << inputs[0]->streamsInfo[i]->name << "] ";
				
				if(output->encoder->getAudioCodecContext()->sample_rate 
					!= input->decoder->getAudioCodecContext()->sample_rate)
				{
					if(nrAudioFilters++ > 0) audioGraph << ",";

					audioGraph << "aresample=" << output->encoder->getAudioCodecContext()->sample_rate;
				}
								
				if(nrAudioFilters++ == 0) audioGraph << "anull";

				int outStreamIdx = inputs[0]->streamsInfo[i]->outputStreamIndex;

				audioGraph << " [" << outputs[0]->streamsInfo[outStreamIdx]->name << "]";

				if(nrVideoFilters > 0) filterSpec << "; ";
				
				filterSpec << audioGraph.str();
			}
			
		}		

		if(nrVideoFilters > 0 || nrAudioFilters > 0) {

			initFilterGraph(filterSpec.str().c_str());	

			if(nrAudioFilters > 0 && output->encoder->getAudioCodecContext()->frame_size != 0) {

				filterGraph->setAudioSinkFrameSize("ao00",output->encoder->getAudioCodecContext()->frame_size);
			}
		}
	
	}

	void initBitstreamFilters(int inputStreamIdx, int outputStreamIdx, StreamTransformMode mode) 
	{
		if(output->encoder->getStream(outputStreamIdx)->isVideo()) {
		
			if(output->encoder->getStream(outputStreamIdx)->getCodecID() == AV_CODEC_ID_MPEG4) {
			
				output->streamsInfo[outputStreamIdx]->addBitstreamFilter("mpeg4_unpack_bframes");			
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
				output->streamsInfo[outputStreamIdx]->addBitstreamFilter("aac_adtstoasc");
			}

		}
	}

	

	

};

}