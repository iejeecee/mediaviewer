#pragma once
#include "VideoDecoderFactory.h"
#include "IVideoDecoder.h"
#include "VideoEncoder.h"
#include "VideoTransformer.h"
#include "Utils.h"

#define DISCARD_STREAM -1

using namespace MediaViewer::Infrastructure::Video::TranscodeOptions;
using namespace System::Collections::Generic;
using namespace msclr::interop;
using namespace MediaViewer::Infrastructure::Utils;
using namespace MediaViewer::Infrastructure::Logging;


namespace VideoLib {

class VideoTranscode : VideoTransformer {

protected:
	
	IVideoDecoder *input, *input2;
	VideoEncoder *output;

	int inputStreamOffset;
	
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

	typedef void (__stdcall *ProgressCallback)(double);

	VideoTranscode() {
		
		input = NULL;
		output = new VideoEncoder();						
	}

	virtual ~VideoTranscode()
	{
		if(input != NULL) {

			delete input;
		}

		delete output;			
	}
				
	bool getNextPacketCustom(AVPacket *packet, int &inputIdx) {

		return false;
	}

	void transcode(OpenVideoArgs ^openArgs, String ^outputFilename, System::Threading::CancellationToken token, 
		Dictionary<String ^,Object ^> ^options, ProgressCallback progressCallback = NULL) 
	{			
		VideoTransformerInputInfo *inputInfo = NULL,*inputInfo2;
		VideoTransformerOutputInfo *outputInfo = NULL;

		try {

			input = VideoDecoderFactory::create(input, openArgs);
			input->open(openArgs, token);
						
			double startTimeRange = 0;

			if(options->ContainsKey("startTimeRange")) {

				startTimeRange = (double)options["startTimeRange"];				
			}

			double endTimeRange = DBL_MAX;
			
			if(options->ContainsKey("endTimeRange")) {

				endTimeRange = (double)options["endTimeRange"];						
			}

			inputInfo = new VideoTransformerInputInfo(input,startTimeRange,endTimeRange);

			output = new VideoEncoder();
			output->open(outputFilename);

			outputInfo = new VideoTransformerOutputInfo(output);

			initialize(inputInfo, outputInfo, options, token);
			
			addInput(inputInfo);
			addOutput(outputInfo);

			input2 = new VideoDecoder();
			input2->open(gcnew OpenVideoArgs("D:\\game\\XMP-Toolkit-SDK-5.1.2\\samples\\testfiles\\b.mp4"), token);

			inputInfo2 = new VideoTransformerInputInfo(input2);
			inputInfo2->addStreamInfo(new VideoTransformerInputStreamInfo(0,0,StreamTransformMode::ENCODE,0,"in2"));
			inputInfo2->addStreamInfo(new VideoTransformerInputStreamInfo(1,1,StreamTransformMode::ENCODE,0,"in2"));
			addInput(inputInfo2);
			
			initFilters(options);

			av_dump_format(output->getFormatContext(), 0, output->getFormatContext()->filename, 1);
			
			transform(token, progressCallback, &VideoTranscode::getNextPacketSeqOrder);

		} finally {
			
			if(inputInfo != NULL) {

				delete inputInfo;
			}
			
			if(input2 != NULL) {

				input2->close();
				delete input2;
			}

			if(inputInfo2 != NULL) {

				delete inputInfo2;
			}

			if(outputInfo != NULL) 
			{
				delete outputInfo;
			}

			input->close();			
			output->close();
		}		

	}

protected:

	virtual double calculateProgress() {

		double endTime = Math::Min(inputs[0]->endTimeRange, inputs[0]->decoder->getDurationSeconds());
		double totalSeconds = endTime - inputs[0]->startTimeRange;

		double progress = (inputs[0]->streamsInfo[0]->posSeconds - inputs[0]->startTimeRange) / totalSeconds; 

		return progress;
	}

	void initOutputStream(Stream *inStream, StreamTransformMode streamMode, Dictionary<String ^, Object ^> ^options) 
	{
						
		if(streamMode == StreamTransformMode::COPY) 
		{		
			output->createStream(inStream);						
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
					
		}			
						
	}

		
	void initialize(VideoTransformerInputInfo *inputInfo, VideoTransformerOutputInfo *outputInfo,
		Dictionary<String ^,Object ^> ^options, System::Threading::CancellationToken token) 
	{
		inputStreamOffset = 0;				
					
		for(int i = 0; i < input->getNrStreams(); i++) {
				
			VideoTransformerInputStreamInfo *inputStreamInfo;

			Stream *inStream = input->getStream(i);
											
			if ((inStream->isVideo() && i == input->getVideoStreamIndex()) ||
				(inStream->isAudio() && i == input->getAudioStreamIndex()))
			{
				StreamTransformMode streamMode = inStream->isVideo() ? 
					streamOptionsToStreamTransformMode((StreamOptions)options["videoStreamMode"])
					: streamOptionsToStreamTransformMode((StreamOptions)options["audioStreamMode"]);

				int outStreamIdx = output->getNrStreams();

				inputStreamInfo = new VideoTransformerInputStreamInfo(i, outStreamIdx, streamMode);

				if(streamMode != StreamTransformMode::DISCARD) {

					VideoTransformerOutputStreamInfo *outputStreamInfo = 
						new VideoTransformerOutputStreamInfo(outStreamIdx, streamMode);

					outputInfo->addStreamInfo(outputStreamInfo);

					initOutputStream(inStream, streamMode, options);
					initBitstreamFilters(i,outStreamIdx,streamMode,outputInfo); 
				}
			} 
			else 
			{				
				inputStreamInfo = new VideoTransformerInputStreamInfo(i,-1, StreamTransformMode::DISCARD);
			}
			
			inputInfo->addStreamInfo(inputStreamInfo);
		}
		
	}

	int initFilters(Dictionary<String ^,Object ^> ^options)
	{		
								
		for (int i = 0; i < input->getNrStreams(); i++) {
					
			//int inIdx = i;
			//int outIdx = streamInfo[inIdx]->outStreamIdx;

			if (input->getStream(i)->isVideo() && i == input->getVideoStreamIndex()) {

				StreamOptions videoStreamMode = (StreamOptions)options["videoStreamMode"];	
				
				if(videoStreamMode != StreamOptions::Encode) continue;
																	
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
				
				String ^videoGraph = "";

				if(inWidth != outWidth || inHeight != outHeight) {

					if(!String::IsNullOrEmpty(videoGraph)) videoGraph += ",";

					videoGraph += "scale=" + centeredRect.Width + "x" + centeredRect.Height;
				}

				if(centeredRect.Width != outWidth || centeredRect.Height != outHeight) 
				{
					if(!String::IsNullOrEmpty(videoGraph)) videoGraph += ",";

					// create a centered rectangle to maintain input aspect ratio using the padding filter			
					videoGraph += "pad=" + outWidth + ":" + outHeight + ":" + centeredRect.X + ":" + centeredRect.Y;
				}
															
				if(options->ContainsKey("framesPerSecond")) {

					if(!String::IsNullOrEmpty(videoGraph)) videoGraph += ",";

					float newFps = (float)options["framesPerSecond"];
					float inFps = input->getFrameRate();

					String ^scalePts = (inFps / newFps).ToString(System::Globalization::CultureInfo::InvariantCulture);
					
					videoGraph += "fps=" + newFps + ",setpts=" + scalePts + "*PTS";
				}

				videoGraph = "[in] [in2] concat=v=1:a=0";

				if(String::IsNullOrEmpty(videoGraph)) videoGraph = "null";

				std::string value = msclr::interop::marshal_as<std::string>(videoGraph);

				initVideoFilterGraph(value.c_str());				

			} else if(input->getStream(i)->isAudio() && i == input->getAudioStreamIndex()) {

				StreamOptions audioStreamMode = (StreamOptions)options["audioStreamMode"];		
				
				if(audioStreamMode != StreamOptions::Encode) continue;
																		
				String ^audioGraph = "";

				if(output->getAudioCodecContext()->frame_size != 0) {

					if(!String::IsNullOrEmpty(audioGraph)) audioGraph += ",";

					audioGraph += "asetnsamples=n=" + output->getAudioCodecContext()->frame_size;
				}

				if(output->getAudioCodecContext()->sample_rate != input->getAudioCodecContext()->sample_rate)
				{
					if(!String::IsNullOrEmpty(audioGraph)) audioGraph += ",";

					audioGraph = "aresample=" + output->getAudioCodecContext()->sample_rate;
				}

				//audioGraph = "[in2] asetnsamples=n=" + output->getAudioCodecContext()->frame_size + " [bud]; [in] [bud] concat=v=0:a=1";
				audioGraph = "[in] [in2] concat=v=0:a=1";

				if(String::IsNullOrEmpty(audioGraph)) audioGraph = "anull";

				std::string value = msclr::interop::marshal_as<std::string>(audioGraph);

				initAudioFilterGraph(value.c_str());	

				if(output->getAudioCodecContext()->frame_size != 0) {

					audioFilterGraph->setAudioSinkFrameSize("out",output->getAudioCodecContext()->frame_size);
				}
			}
			
		}		

		return 0;
	}

	void initBitstreamFilters(int inputStreamIdx, int outputStreamIdx, StreamTransformMode mode,
		VideoTransformerOutputInfo *outputInfo) 
	{
		if(output->getStream(outputStreamIdx)->isVideo()) {
		
			if(output->getStream(outputStreamIdx)->getCodecID() == AV_CODEC_ID_MPEG4) {
			
				outputInfo->streamsInfo[outputStreamIdx]->addBitstreamFilter("mpeg4_unpack_bframes");			
			}

		} else {

				// add aac_adtstoasc bitstream filter if output is mp4 aac and input is dts aac
			bool encodeNeedsAACFilter = mode == StreamTransformMode::ENCODE &&
				output->getStream(outputStreamIdx)->getCodecID() == AV_CODEC_ID_AAC &&
				strcmp(output->getOutputFormat()->name,"mp4") == 0;
			
			bool copyNeedsAACFilter = mode == StreamTransformMode::COPY &&
				output->getStream(outputStreamIdx)->getCodecID() == AV_CODEC_ID_AAC &&
				strcmp(output->getOutputFormat()->name,"mp4") == 0 &&
				strcmp(input->getFormatContext()->iformat->name,"mpegts") == 0;
			
			if(encodeNeedsAACFilter || copyNeedsAACFilter) 
			{					
				outputInfo->streamsInfo[outputStreamIdx]->addBitstreamFilter("aac_adtstoasc");
			}

		}
	}

	

	

};

}