#pragma once
#include <string>       
#include <iostream>  
#include <iomanip> 
#include <sstream> 
#include "VideoDecoderFactory.h"
#include "IVideoDecoder.h"
#include "VideoEncoder.h"
#include "VideoTranscodeBase.h"
#include "Utils.h"

using namespace MediaViewer::Infrastructure::Video::TranscodeOptions;
using namespace System::Collections::Generic;
using namespace msclr::interop;
using namespace MediaViewer::Infrastructure::Utils;
using namespace MediaViewer::Infrastructure::Logging;

namespace VideoLib {

class VideoTranscode : public VideoTranscodeBase {

public:
					
	void transcode(OpenVideoArgs ^openArgs, String ^outputFilename, System::Threading::CancellationToken token, 
		Dictionary<String ^,Object ^> ^options, ProgressCallback progressCallback = NULL) 
	{			
	
		VideoTransformerInputInfo *input = NULL;
		VideoTransformerOutputInfo *output = NULL;	

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

			input = new VideoTransformerInputInfo(decoder, startTimeRange, endTimeRange);

			VideoEncoder *encoder = new VideoEncoder();

			char *formatName = NULL;

			if(outputFilename->EndsWith("png")) {

				formatName = "apng";
			}

			encoder->open(outputFilename, formatName);

			output = new VideoTransformerOutputInfo(encoder);

			initialize(input, output, options, token);
						
			if(input->decoder->hasVideo() && input->decoder->getVideoCodecContext()->codec_id != AV_CODEC_ID_GIF
				&& output->encoder->hasVideo() && output->encoder->getVideoCodecContext()->codec_id == AV_CODEC_ID_GIF) 
			{

				transcodeGif(input, output, token, options, progressCallback);

			} else {
			
				addInput(input);
				addOutput(output);

				initFilters(input, output, options);

				av_dump_format(output->encoder->getFormatContext(), 0, output->encoder->getFormatContext()->filename, 1);
			
				transform(token, progressCallback, (GetNextPacketFunc)&VideoTranscode::getNextPacketSeq);
			}

		} finally {
			
			if(input != NULL) {
						
				input->decoder->close();
				delete input->decoder;
				delete input;			
				input = NULL;
			}
									
			if(output != NULL) 
			{			
				output->encoder->close();
				delete output->encoder;
				delete output;
				output = NULL;
			}
			
			clearTransformer();
		}		

	}

protected:

	void transcodeGif(VideoTransformerInputInfo *input, VideoTransformerOutputInfo *output,
		System::Threading::CancellationToken token, Dictionary<String ^,Object ^> ^options, 
		ProgressCallback progressCallback = NULL) 
	{
		// http://blog.pkh.me/p/21-high-quality-gif-with-ffmpeg.html
		// do two passes to generate animated gifs, first creating a global palette which is 
		// then used in the second pass
			
		MemoryStreamAVIOContext *inMemoryStreamCtx = NULL, *outMemoryStreamCtx = NULL;
		VideoEncoder *pngEncoder = NULL;
		VideoDecoder *pngDecoder = NULL;
	
		try {

			MemoryStream memoryStream;
			outMemoryStreamCtx = new MemoryStreamAVIOContext(&memoryStream, MemoryStreamAVIOContext::Mode::WRITEABLE);
			pngEncoder = new VideoEncoder();

			pngEncoder->open(outMemoryStreamCtx, "apng");
			pngEncoder->createStream("apng", 16, 16, av_make_q(1,1), av_make_q(1, 25), AV_PIX_FMT_RGBA)->open();
	
			av_dump_format(pngEncoder->getFormatContext(), 0, NULL, 1);

			VideoTransformerOutputInfo paletteOut(pngEncoder);
			paletteOut.addStreamInfo(0, StreamTransformMode::ENCODE);

			addInput(input);
			addOutput(&paletteOut);

			int nrVideoFilters = 0;
			int videoStreamIdx = input->decoder->getVideoStreamIndex();
			std::stringstream palettegenSpec, paletteuseSpec;

			std::string videoFilterSpec = getVideoFilterSpec(input,output,options, videoStreamIdx, nrVideoFilters);

			std::string ditherMode = "=dither=floyd_steinberg";

			if(nrVideoFilters == 0) {

				palettegenSpec << "[vi00] palettegen [vo00]";
				paletteuseSpec << "[vi00] [vi10] paletteuse" << ditherMode << " [vo00]";
		
			} else {

				palettegenSpec << videoFilterSpec << ", palettegen [vo00]";
				paletteuseSpec << videoFilterSpec << " [x]; [x] [vi10] paletteuse" << ditherMode << " [vo00]";
			}

			initFilterGraph(palettegenSpec.str().c_str()); 

			transform(token, progressCallback);
				
			clearTransformer();
		
			memoryStream.seek(0, SEEK_SET);

			if(input->startTimeRange == 0) {

				input->decoder->seek(0);
			}

			pngDecoder = new VideoDecoder();
			inMemoryStreamCtx = new MemoryStreamAVIOContext(&memoryStream, MemoryStreamAVIOContext::Mode::READABLE);
			pngDecoder->open(inMemoryStreamCtx, token, "png");

			VideoTransformerInputInfo paletteIn(pngDecoder);
			paletteIn.addStreamInfo(0, StreamTransformMode::ENCODE);
			
			addInput(input);
			addInput(&paletteIn);
		
			addOutput(output);

			initFilterGraph(paletteuseSpec.str().c_str());

			transform(token, progressCallback);		

		} finally {
																
			if(pngEncoder != NULL) {

				pngEncoder->close();
				delete pngEncoder;
			}

			if(outMemoryStreamCtx != NULL) {

				delete outMemoryStreamCtx;
			}

			if(pngDecoder != NULL) {
			
				pngDecoder->close();				
				delete pngDecoder;
			}		

			if(inMemoryStreamCtx != NULL) {

				delete inMemoryStreamCtx;
			}						
									
		}
	}

	virtual double calculateProgress(int &inputIdx) {

		double endTime = Math::Min(inputs[0]->endTimeRange, inputs[0]->decoder->getDurationSeconds());
		double totalSeconds = endTime - inputs[0]->startTimeRange;

		double progress = (inputs[0]->streamsInfo[0]->posSeconds - inputs[0]->startTimeRange) / totalSeconds; 

		return progress;
	}
			
	void initialize(VideoTransformerInputInfo *input, VideoTransformerOutputInfo *output,
		Dictionary<String ^,Object ^> ^options, System::Threading::CancellationToken token) 
	{
								
		for(int i = 0; i < input->decoder->getNrStreams(); i++) {
							
			Stream *inStream = input->decoder->getStream(i);
											
			if ((inStream->isVideo() && i == input->decoder->getVideoStreamIndex()) ||
				(inStream->isAudio() && i == input->decoder->getAudioStreamIndex()))
			{
				StreamTransformMode streamMode = inStream->isVideo() ? 
					streamOptionsToStreamTransformMode((StreamOptions)options["videoStreamMode"])
					: streamOptionsToStreamTransformMode((StreamOptions)options["audioStreamMode"]);

				int outStreamIdx = output->encoder->getNrStreams();

				input->addStreamInfo(outStreamIdx, streamMode);

				if(streamMode != StreamTransformMode::DISCARD) {
					
					output->addStreamInfo(outStreamIdx, streamMode);

					initOutputStream(inStream, output, streamMode, options);
					initBitstreamFilters(input, output, i, outStreamIdx, streamMode); 
				}
			} 
			else 
			{				
				input->addStreamInfo(-1, StreamTransformMode::DISCARD);
			}
			
		
		}
		
		

	}

	std::string getVideoFilterSpec(VideoTransformerInputInfo *input, VideoTransformerOutputInfo *output,
		Dictionary<String ^,Object ^> ^options, int inputStreamIdx, int &nrVideoFilters) 
	{
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

		videoGraph << "[" << inputs[0]->streamsInfo[inputStreamIdx]->name << "] ";

		if(inWidth != outWidth || inHeight != outHeight) {

			if(nrVideoFilters++ > 0) videoGraph << ",";

			videoGraph << "scale=" << centeredRect.Width << "x" << centeredRect.Height << ":flags=lanczos";
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
										
			videoGraph << std::setprecision(4) << "fps=" << newFps;
		}
			
		if(nrVideoFilters++ == 0) videoGraph << "null";

		return videoGraph.str();
	}

	void initFilters(VideoTransformerInputInfo *input, VideoTransformerOutputInfo *output,
		Dictionary<String ^,Object ^> ^options)
	{		

		std::stringstream filterSpec;
		int nrAudioFilters = 0;
		int nrVideoFilters = 0;
								
		for (int i = 0; i < input->decoder->getNrStreams(); i++) {
						
			if (input->decoder->getStream(i)->isVideo() && i == input->decoder->getVideoStreamIndex()) {

				StreamOptions videoStreamMode = (StreamOptions)options["videoStreamMode"];	
				
				if(videoStreamMode != StreamOptions::Encode) continue;
				
				std::stringstream videoGraph;

				videoGraph << getVideoFilterSpec(input,output,options, i, nrVideoFilters);

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

		
	virtual void modifyTS(int inputIdx, int streamIdx, int64_t pts, int64_t dts, int64_t duration) {

		int64_t ts = dts != AV_NOPTS_VALUE ? dts : pts; 			
		VideoTransformerInputStream *inputStream = inputs[inputIdx]->streamsInfo[streamIdx];

		inputStream->posSeconds = inputs[inputIdx]->decoder->getStream(streamIdx)->getTimeSeconds(ts);
			
		if(!inputStream->isTsOffsetSet)
		{								
			for(int i = 0; i < inputs[inputIdx]->streamsInfo.size(); i++) {

				int64_t tsOffset = -inputs[inputIdx]->decoder->getStream(i)->getTimeBaseUnits(inputStream->posSeconds);

				inputs[inputIdx]->streamsInfo[i]->setTsOffset(tsOffset);				
			}
		}
					
		inputs[inputIdx]->streamsInfo[streamIdx]->nextTs = inputs[inputIdx]->streamsInfo[streamIdx]->tsOffset + ts + duration;
		
	}
	

};

}