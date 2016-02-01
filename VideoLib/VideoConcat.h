#pragma once
#include "VideoDecoderFactory.h"
#include "IVideoDecoder.h"
#include "VideoEncoder.h"
#include "VideoTranscodeBase.h"
#include "Utils.h"
#include <vector>

using namespace MediaViewer::Infrastructure::Video::TranscodeOptions;
using namespace System::Collections::Generic;
using namespace msclr::interop;
using namespace MediaViewer::Infrastructure::Utils;
using namespace MediaViewer::Infrastructure::Logging;

namespace VideoLib {

class VideoConcat : public VideoTranscodeBase {

public:
	
	void concat(List<OpenVideoArgs ^> ^openArgs, String ^outputFilename, System::Threading::CancellationToken token, 
		Dictionary<String ^,Object ^> ^options, ProgressCallback progressCallback = NULL) 
	{			
		
		std::vector<VideoTransformerInputInfo *> input;
		VideoTransformerOutputInfo *output = NULL;

		try {

			for(int i = 0; i < openArgs->Count; i++) {
				
				IVideoDecoder *decoder = VideoDecoderFactory::create(openArgs[i]);
				decoder->open(openArgs[i], token);
				
				input.push_back(new VideoTransformerInputInfo(decoder));				
			}
				
			VideoEncoder *encoder = new VideoEncoder();
			encoder->open(outputFilename);

			output = new VideoTransformerOutputInfo(encoder);
		
			initialize(input, output, options, token);
						
			checkInputsValidity(input, output,options);

			initFilters(input, output, options);

			av_dump_format(output->encoder->getFormatContext(), 0, output->encoder->getFormatContext()->filename, 1);
			
			transform(token, progressCallback, (GetNextPacketFunc)&VideoConcat::getNextPacketSeq);

		} finally {
						
			for(int i = 0; i < (int)input.size(); i++) {
			
				input[i]->decoder->close();
				delete input[i]->decoder;
				delete input[i];		
			}

			input.clear();
	
			if(output != NULL) {

				output->encoder->close();
				delete output->encoder;
				delete output;	
				output = NULL;
			}

			clearTransformer();
		}		
	}

protected:

	void checkInputsValidity(std::vector<VideoTransformerInputInfo *> &input,
		VideoTransformerOutputInfo *output, Dictionary<String ^,Object ^> ^options) 
	{

		StreamTransformMode videoStreamMode = streamOptionsToStreamTransformMode((StreamOptions)options["videoStreamMode"]);
		StreamTransformMode audioStreamMode = streamOptionsToStreamTransformMode((StreamOptions)options["audioStreamMode"]);

		if(videoStreamMode == StreamTransformMode::COPY && output->encoder->hasVideo()) {
			
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

		if(audioStreamMode == StreamTransformMode::COPY && output->encoder->hasAudio()) {
			
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
		
	void initialize(std::vector<VideoTransformerInputInfo *> &input, VideoTransformerOutputInfo *output, 
		Dictionary<String ^,Object ^> ^options, System::Threading::CancellationToken token) 
	{
		StreamTransformMode videoStreamMode = streamOptionsToStreamTransformMode((StreamOptions)options["videoStreamMode"]);
		
		if(input[0]->decoder->hasVideo() && videoStreamMode != StreamTransformMode::DISCARD) {
			
			Stream *videoStream = input[0]->decoder->getStream(input[0]->decoder->getVideoStreamIndex());

			initOutputStream(videoStream, output, videoStreamMode, options);
			
			output->addStreamInfo(output->encoder->getNrStreams(), videoStreamMode);
		}

		StreamTransformMode audioStreamMode = streamOptionsToStreamTransformMode((StreamOptions)options["audioStreamMode"]);

		if(input[0]->decoder->hasAudio() && audioStreamMode != StreamTransformMode::DISCARD) {
			
			Stream *audioStream = input[0]->decoder->getStream(input[0]->decoder->getAudioStreamIndex());

			initOutputStream(audioStream, output, audioStreamMode, options);
		
			output->addStreamInfo(output->encoder->getNrStreams(), audioStreamMode);
		}

		for(int inputIdx = 0; inputIdx < (int)input.size(); inputIdx++) {

			IVideoDecoder *decoder = input[inputIdx]->decoder;
			
			for(int inStreamIdx = 0; inStreamIdx < decoder->getNrStreams(); inStreamIdx++) {
							
				Stream *inStream = decoder->getStream(inStreamIdx);
											
				if (inStream->isVideo() && inStreamIdx == decoder->getVideoStreamIndex()
					&& videoStreamMode != StreamTransformMode::DISCARD) 					
				{					
					int outStreamIdx = output->encoder->getVideoStreamIndex();

					input[inputIdx]->addStreamInfo(outStreamIdx, videoStreamMode);																
					initBitstreamFilters(input[0], output, inStreamIdx, outStreamIdx, videoStreamMode); 
					
				} 
				else if(inStream->isAudio() && inStreamIdx == decoder->getAudioStreamIndex()
					&& audioStreamMode != StreamTransformMode::DISCARD)
				{
					int outStreamIdx = output->encoder->getAudioStreamIndex();

					input[inputIdx]->addStreamInfo(outStreamIdx, audioStreamMode);	
					initBitstreamFilters(input[0], output, inStreamIdx, outStreamIdx, audioStreamMode); 
				}
				else 
				{				
					input[inputIdx]->addStreamInfo(-1, StreamTransformMode::DISCARD);
				}
			
			
			}

		}
		
		for(int i = 0; i < (int)input.size(); i++) {

			addInput(input[i]);
		}

		addOutput(output);

	}

	void initFilters(std::vector<VideoTransformerInputInfo *> &input,
		VideoTransformerOutputInfo *output, Dictionary<String ^,Object ^> ^options)
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

	

	virtual void modifyTS(int inputIdx, int streamIdx, int64_t pts, int64_t dts, int64_t duration) {

		int64_t ts = dts != AV_NOPTS_VALUE ? dts : pts; 	

		VideoTransformerInputStream *inputStream = inputs[inputIdx]->streamsInfo[streamIdx];

		inputStream->posSeconds = inputs[inputIdx]->decoder->getStream(streamIdx)->getTimeSeconds(ts);
			
		if(!inputStream->isTsOffsetSet)
		{		
			for(int i = 0; i < inputs[inputIdx]->streamsInfo.size(); i++) 
			{
				if(inputs[inputIdx]->streamsInfo[i]->mode != StreamTransformMode::COPY) continue;

				// make sure the stream starts at zero 
				int64_t tsOffset = -av_rescale_q_rnd(
					inputs[inputIdx]->decoder->getFormatContext()->start_time,
					av_make_q(1, AV_TIME_BASE),
					inputs[inputIdx]->decoder->getStream(i)->getTimeBase(),
					(AVRounding)(AV_ROUND_NEAR_INF|AV_ROUND_PASS_MINMAX));

				if(inputIdx > 0) {

					// find the exact length of the previous input and add it to the offset				
					double prevInputLengthSeconds = DBL_MIN;

					for(int j = 0; j < inputs[inputIdx - 1]->streamsInfo.size(); j++)
					{
						double streamLengthSeconds = inputs[inputIdx - 1]->decoder->getStream(j)->getTimeSeconds(inputs[inputIdx - 1]->streamsInfo[j]->nextTs);

						if(streamLengthSeconds > prevInputLengthSeconds) {

							prevInputLengthSeconds = streamLengthSeconds;
						}
					}
												
					tsOffset += inputs[inputIdx]->decoder->getStream(i)->getTimeBaseUnits(prevInputLengthSeconds);
				}

				inputs[inputIdx]->streamsInfo[i]->setTsOffset(tsOffset);		
			}
		}

		
		/*int64_t curDts = inputs[inputIdx]->streamsInfo[streamIdx]->tsOffset + dts;
		double dtsSeconds = inputs[inputIdx]->decoder->getStream(streamIdx)->getTimeSeconds(curDts);

		int64_t nextDts = curDts + duration;

		int curPts = inputs[inputIdx]->streamsInfo[streamIdx]->tsOffset + pts;
		double ptsSeconds = inputs[inputIdx]->decoder->getStream(streamIdx)->getTimeSeconds(curPts);*/
			
		int64_t nextPts = inputs[inputIdx]->streamsInfo[streamIdx]->tsOffset + pts + duration;

		if(nextPts > inputs[inputIdx]->streamsInfo[streamIdx]->nextTs) {

			inputs[inputIdx]->streamsInfo[streamIdx]->nextTs = nextPts;
		}
		
		//System::Diagnostics::Debug::Print("file:" + inputIdx + " stream:" + streamIdx + " pts:" + curPts + " pts_time:" + ptsSeconds + " dts:" + curDts + " dts_time:" + dtsSeconds);

	}

};

}