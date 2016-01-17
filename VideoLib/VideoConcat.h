#pragma once
#include "VideoDecoderFactory.h"
#include "IVideoDecoder.h"
#include "VideoEncoder.h"
#include "VideoTransformer.h"
#include "Utils.h"
#include <vector>

using namespace MediaViewer::Infrastructure::Video::TranscodeOptions;
using namespace System::Collections::Generic;
using namespace msclr::interop;
using namespace MediaViewer::Infrastructure::Utils;
using namespace MediaViewer::Infrastructure::Logging;

namespace VideoLib {

class VideoConcat : public VideoTransformer {

protected:
	
	std::vector<VideoTransformerInputInfo *> input;
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
	

	VideoConcat() {
			
		output = NULL;
	}

	virtual ~VideoConcat()
	{
		for(int i = 0; i < (int)input.size(); i++) {

			delete input[i];		
		}

		input.clear();
	
		if(output != NULL)
		{
			delete output;			
		}
	}
				

	void concat(List<OpenVideoArgs ^> ^openArgs, String ^outputFilename, System::Threading::CancellationToken token, 
		Dictionary<String ^,Object ^> ^options, ProgressCallback progressCallback = NULL) 
	{			
		
		VideoTransformerOutputInfo *outputInfo = NULL;

		try {

			for(int i = 0; i < openArgs->Count; i++) {
				
				IVideoDecoder *decoder = VideoDecoderFactory::create(openArgs[i]);
				decoder->open(openArgs[i], token);
				
				input.push_back(new VideoTransformerInputInfo(decoder));
				
			}
				
			VideoEncoder *encoder = new VideoEncoder();
			encoder->open(outputFilename);

			output = new VideoTransformerOutputInfo(encoder);
		
			initialize(options, token);
			
			for(int i = 0; i < (int)input.size(); i++) {

				addInput(input[i]);
			}

			addOutput(output);

			checkInputsValidity(options);

			initFilters(options);

			av_dump_format(output->encoder->getFormatContext(), 0, output->encoder->getFormatContext()->filename, 1);
			
			transform(token, progressCallback, &VideoConcat::getNextPacketSeqOrder);

		} finally {
						
			for(int i = 0; i < (int)input.size(); i++) {
			
				delete input[i];		
			}

			input.clear();
	
			if(output != NULL) {

				delete output;	
				output = NULL;
			}

			clearTransformer();
		}		
	}

protected:

	void checkInputsValidity(Dictionary<String ^,Object ^> ^options) {

		StreamTransformMode videoStreamMode = streamOptionsToStreamTransformMode((StreamOptions)options["videoStreamMode"]);
		StreamTransformMode audioStreamMode = streamOptionsToStreamTransformMode((StreamOptions)options["audioStreamMode"]);

		if(videoStreamMode == StreamTransformMode::COPY) {
			
			Stream *outStream = output->encoder->getStream(output->encoder->getVideoStreamIndex());

			for(int i = 0; i < input.size(); i++) {

				String ^filename = gcnew String(input[i]->decoder->getFormatContext()->filename);

				if(!input[i]->decoder->hasVideo()) {
					
					throw gcnew VideoLibException(filename + " missing video stream");
				}

				Stream *inStream = input[i]->decoder->getStream(input[i]->decoder->getVideoStreamIndex());
							
				if(inStream->getCodecID() != outStream->getCodecID()) 
				{
					throw gcnew VideoLibException(filename + " video codec does not match output");
				}

				if(inStream->getCodecContext()->width != outStream->getCodecContext()->width ||
					inStream->getCodecContext()->height != outStream->getCodecContext()->height)
				{
					throw gcnew VideoLibException(filename + " resolution does not match output");
				}

				if(inStream->getCodecContext()->pix_fmt != outStream->getCodecContext()->pix_fmt)
				{
					throw gcnew VideoLibException(filename + " pixel format does not match output");
				}
			}

		}

		if(audioStreamMode == StreamTransformMode::COPY) {
			
			Stream *outStream = output->encoder->getStream(output->encoder->getAudioStreamIndex());

			for(int i = 0; i < input.size(); i++) {

				String ^filename = gcnew String(input[i]->decoder->getFormatContext()->filename);

				if(!input[i]->decoder->hasAudio()) {
					
					throw gcnew VideoLibException(filename + " missing audio stream");
				}

				Stream *inStream = input[i]->decoder->getStream(input[i]->decoder->getAudioStreamIndex());
											
				if(inStream->getCodecID() != outStream->getCodecID())  
				{
					throw gcnew VideoLibException(filename + " audio codec does not match output");
				}

				if(inStream->getCodecContext()->channel_layout != outStream->getCodecContext()->channel_layout)
				{
					throw gcnew VideoLibException(filename + " audio channel layout does not match output");
				}

				if(inStream->getCodecContext()->sample_fmt != outStream->getCodecContext()->sample_fmt)
				{
					throw gcnew VideoLibException(filename + " audio sample format does not match output");
				}

				if(inStream->getCodecContext()->sample_rate != outStream->getCodecContext()->sample_rate)
				{
					throw gcnew VideoLibException(filename + " audio sample rate does not match output");
				}
			}

		}
	}

	virtual double calculateProgress(int &inputIdx) {
		
		inputIdx = 0;

		for(int i = 0; i < (int)inputs.size(); i++) {

			if(!inputs[i]->getIsInputFinished()) break;
		
			inputIdx++;
		}

		inputIdx = Math::Min((int)inputs.size() - 1, inputIdx);

		double endTime = inputs[inputIdx]->decoder->getDurationSeconds();
	
		double progress = inputs[inputIdx]->streamsInfo[0]->posSeconds / endTime; 

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
		StreamTransformMode videoStreamMode = streamOptionsToStreamTransformMode((StreamOptions)options["videoStreamMode"]);
		
		if(input[0]->decoder->hasVideo() && videoStreamMode != StreamTransformMode::DISCARD) {
			
			Stream *videoStream = input[0]->decoder->getStream(input[0]->decoder->getVideoStreamIndex());

			initOutputStream(videoStream, videoStreamMode, options);

			VideoTransformerOutputStreamInfo *outputStreamInfo = 
				new VideoTransformerOutputStreamInfo(output->encoder->getNrStreams(), videoStreamMode);

			output->addStreamInfo(outputStreamInfo);
		}

		StreamTransformMode audioStreamMode = streamOptionsToStreamTransformMode((StreamOptions)options["audioStreamMode"]);

		if(input[0]->decoder->hasAudio() && audioStreamMode != StreamTransformMode::DISCARD) {
			
			Stream *audioStream = input[0]->decoder->getStream(input[0]->decoder->getAudioStreamIndex());

			initOutputStream(audioStream, audioStreamMode, options);

			VideoTransformerOutputStreamInfo *outputStreamInfo = 
				new VideoTransformerOutputStreamInfo(output->encoder->getNrStreams(), audioStreamMode);

			output->addStreamInfo(outputStreamInfo);
		}

		for(int inputIdx = 0; inputIdx < (int)input.size(); inputIdx++) {

			IVideoDecoder *decoder = input[inputIdx]->decoder;
			
			for(int inStreamIdx = 0; inStreamIdx < decoder->getNrStreams(); inStreamIdx++) {
				
				VideoTransformerInputStreamInfo *inputStreamInfo;

				Stream *inStream = decoder->getStream(inStreamIdx);
											
				if (inStream->isVideo() && inStreamIdx == decoder->getVideoStreamIndex()
					&& videoStreamMode != StreamTransformMode::DISCARD) 					
				{					
					int outStreamIdx = output->encoder->getVideoStreamIndex();

					inputStreamInfo = new VideoTransformerInputStreamInfo(inStreamIdx, outStreamIdx, videoStreamMode);																
					initBitstreamFilters(inStreamIdx,outStreamIdx, videoStreamMode); 
					
				} 
				else if(inStream->isAudio() && inStreamIdx == decoder->getAudioStreamIndex()
					&& audioStreamMode != StreamTransformMode::DISCARD)
				{
					int outStreamIdx = output->encoder->getAudioStreamIndex();

					inputStreamInfo = new VideoTransformerInputStreamInfo(inStreamIdx, outStreamIdx, audioStreamMode);																
					initBitstreamFilters(inStreamIdx, outStreamIdx, audioStreamMode); 
				}
				else 
				{				
					inputStreamInfo = new VideoTransformerInputStreamInfo(inStreamIdx,-1, StreamTransformMode::DISCARD);
				}
			
				input[inputIdx]->addStreamInfo(inputStreamInfo);
			}

		}
		
	}

	void initFilters(Dictionary<String ^,Object ^> ^options)
	{			
		
		std::stringstream inputVideoFilters;
		std::stringstream inputNames;
		std::stringstream concatFilter;
		std::stringstream outputNames;
		
		StreamOptions videoStreamMode = (StreamOptions)options["videoStreamMode"];
		StreamOptions audioStreamMode = (StreamOptions)options["audioStreamMode"];
		
		concatFilter << "concat=n=" << input.size();

		if(videoStreamMode == StreamOptions::Encode) {

			concatFilter << ":v=1";
			outputNames << " [" << outputs[0]->streamsInfo[output->encoder->getVideoStreamIndex()]->name << "]";

		} else {

			concatFilter << ":v=0";
		}

		if(audioStreamMode == StreamOptions::Encode) {

			concatFilter << ":a=1";
			outputNames << " [" << outputs[0]->streamsInfo[output->encoder->getAudioStreamIndex()]->name << "]";

		} else {

			concatFilter << ":a=0";
		}

		for(int inputIdx = 0; inputIdx < (int)input.size(); inputIdx++) {
			    						
			IVideoDecoder *decoder = input[inputIdx]->decoder;

			int inStreamIdx = decoder->getVideoStreamIndex();

			if (videoStreamMode == StreamOptions::Encode) 
			{																									
				int inWidth = decoder->getWidth();
				int inHeight = decoder->getHeight();
				int outWidth = output->encoder->getVideoCodecContext()->width;
				int outHeight = output->encoder->getVideoCodecContext()->height;
					
				System::Drawing::Rectangle inRect(0,0,inWidth,inHeight);
				System::Drawing::Rectangle outRect(0,0,outWidth,outHeight);
				
				System::Drawing::Rectangle centeredRect;
				
				if(inWidth != outWidth || inHeight != outHeight) {

					// maintain aspect ratio of input
					System::Drawing::Rectangle scaledRect = ImageUtils::stretchRectangle(inRect,outRect);
					centeredRect = ImageUtils::centerRectangle(outRect, scaledRect);

				} else {

					centeredRect = outRect;
				}
							
				int nrVideoFilters = 0;
				std::stringstream videoGraph;

				videoGraph << "[" << inputs[inputIdx]->streamsInfo[inStreamIdx]->name << "] ";

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
			
				if(nrVideoFilters > 0) {

					videoGraph << " [t" << inputIdx << "]; ";
					inputVideoFilters << videoGraph.str();
					inputNames << "[t" << inputIdx << "] ";

				} else {

					inputNames << videoGraph.str();
				}					
			} 
			
			inStreamIdx = decoder->getAudioStreamIndex();

			if(audioStreamMode == StreamOptions::Encode) {
																				
				std::stringstream audioGraph;

				audioGraph << "[" << inputs[inputIdx]->streamsInfo[inStreamIdx]->name << "] ";
				inputNames << audioGraph.str();
				
			}
			
		}		
		
		if(videoStreamMode == StreamOptions::Encode || audioStreamMode == StreamOptions::Encode) {

			std::stringstream filterSpec;
		
			filterSpec << inputVideoFilters.str() << inputNames.str() << concatFilter.str() << outputNames.str();

			initFilterGraph(filterSpec.str().c_str());	

			if(audioStreamMode == StreamOptions::Encode && output->encoder->getAudioCodecContext()->frame_size != 0) {

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
				strcmp(input[0]->decoder->getFormatContext()->iformat->name,"mpegts") == 0;
			
			if(encodeNeedsAACFilter || copyNeedsAACFilter) 
			{					
				output->streamsInfo[outputStreamIdx]->addBitstreamFilter("aac_adtstoasc");
			}

		}
	}

};

}