#pragma once
#include "..\stdafx.h"
#include <sstream>
#include <iostream>
#include <iomanip>
#include <float.h>
#include <vector>
#include "VideoDecoder.h"
#include "WindowsFileUtil.h"

using namespace System::Windows::Media::Imaging;

namespace VideoLib {

class VideoFrameGrabber : public VideoDecoder {

private:

	std::string videoLocation;
	std::string videoFileName;

	int thumbWidth;
	int thumbHeight;

	int frameNr;

	std::string formatTime(double timeStamp, int &hours, int &minutes, int &seconds) const {

		long totalSeconds = (int)timeStamp;

		seconds = int(totalSeconds % 60);
		minutes = int((totalSeconds / 60) % 60);
		hours = int(totalSeconds / 3600);

		std::stringstream ss;
		ss << std::setfill('0');
		ss << std::setw(2) << hours;
		ss << ":";
		ss << std::setw(2) << minutes;
		ss << ":";
		ss << std::setw(2) << seconds;

		return(ss.str());
	}

	int greatestCommonDivisor(int a, int b) const {
	  return (b == 0) ? a : greatestCommonDivisor(b, a % b);
	}

	int round(float r) {
		return (r > 0.0) ? (int)floor(r + 0.5) : (int)ceil(r - 0.5);
	}

	void probe_dict(AVDictionary *dict, const char *name)
	{
		AVDictionaryEntry *entry = NULL;
		if (!dict)
			return;
	
		while ((entry = av_dict_get(dict, "", entry, AV_DICT_IGNORE_SUFFIX))) {
					
			metaData.push_back(std::string(entry->key) + ": " + std::string(entry->value));
		
		}
	
	}

public:

	std::string container;
	std::string videoCodecName;
	std::string pixelFormat;
	int bitsPerPixel;
	std::vector<std::string> metaData;
	int64_t videoBitRate;

	std::string audioCodecName;
	int64_t audioBitRate;

	VideoFrameGrabber() 
	{
		setDecodedFrameCallback(decodedFrame, this);
	
		container = "";

		videoCodecName = "";
		audioCodecName = "";

		bitsPerPixel = 0;
		videoBitRate = 0;
		audioBitRate = 0;
	}

	virtual void open(OpenVideoArgs ^args, System::Threading::CancellationToken ^ token) {

		VideoDecoder::open(args, token);

		container = std::string(formatContext->iformat->long_name);

		if(hasVideo()) 
		{						
			videoCodecName = std::string(getVideoCodec()->name);
				
			pixelFormat = Utils::getPixelformatName(getVideoCodecContext()->pix_fmt);
						
			const AVPixFmtDescriptor *pixFmtDescriptor = av_pix_fmt_desc_get(getVideoCodecContext()->pix_fmt);

			if(pixFmtDescriptor != NULL) 
			{
				bitsPerPixel = av_get_bits_per_pixel(pixFmtDescriptor);
			}
			
			videoBitRate = getStream(getVideoStreamIndex())->getCodecContext()->bit_rate;
		}

		probe_dict(formatContext->metadata, "");

		// audio info
		if(hasAudio()) {

			audioCodecName = std::string(getAudioCodec()->name);	

			audioBitRate = getStream(getAudioStreamIndex())->getCodecContext()->bit_rate;
		}
	}

	virtual void close() {

		VideoDecoder::close();
	
		container = "";
		videoCodecName = "";

		thumbWidth = 0;
		thumbHeight = 0;

		metaData.clear();

	}

	virtual ~VideoFrameGrabber() {

	}

	// If captureintervalseconds > 0 a frame will be captured every captureIntervalSeconds 
	// else a total of nrThumbs will be generated.
	// The first thumbnail is captured at duration * startOffset
	void grabThumbnails(int maxThumbWidth, int maxThumbHeight, double captureIntervalSeconds, int nrThumbs, 
		double startOffset, System::Threading::CancellationToken ^cancellationToken = nullptr, int timeOutSeconds = 0)
	{
		if(!hasVideo()) 
		{			
			return;

		} else if(getWidth() == 0 || getHeight() == 0) {

			throw gcnew VideoLib::VideoLibException("invalid video stream");
		}
		
		float widthScale = 1;
		float heightScale = 1;

		if(maxThumbWidth > 0 && getWidth() > maxThumbWidth) {
			
			widthScale = maxThumbWidth / (float)getWidth();				
		}

		if(maxThumbHeight > 0 && getHeight() > maxThumbHeight) {
			
			heightScale = maxThumbHeight / (float)getHeight();
		}

		thumbWidth = round(getWidth() * std::min<float>(widthScale, heightScale));
		thumbHeight = round(getHeight() * std::min<float>(widthScale, heightScale));

		setVideoOutputFormat(AV_PIX_FMT_BGR24, thumbWidth, thumbHeight, LANCZOS);

		double duration = getDurationSeconds();

		int nrFrames = 0;

		if(captureIntervalSeconds == 0) {

			nrFrames = nrThumbs;

		} else {

			nrFrames = duration / captureIntervalSeconds;

			if(nrFrames == 0) {
				// make sure to grab atleast one frame
				nrFrames = 1;
			}
		}

		double offset = duration * startOffset;
		double step = (duration - offset) / nrFrames;

		for(frameNr = 0; frameNr < nrFrames; frameNr++) {

			if(cancellationToken->IsCancellationRequested) {
				return;
			}

			double pos = offset + frameNr * step;

			if(pos != 0) {

				bool seekSuccess = seek(pos);
			}
			
			bool success = decodeFrame(cancellationToken, timeOutSeconds);

			if(!success) {
							
				throw gcnew VideoLib::VideoLibException("could not decode frame");
								
			}
				
		}
	}
	
	bool decodeFrame(System::Threading::CancellationToken ^token, int timeOutSeconds = 0) 
	{
		if(isClosed()) return(false);

		int frameFinished = 0;

		DateTime startTime = DateTime::Now;

		while(frameFinished == 0) {
		
			if(timeOutSeconds > 0 && (DateTime::Now - startTime).TotalSeconds > timeOutSeconds) {

				throw gcnew VideoLibException("Timed out during decoding");
			}

			if(token->IsCancellationRequested) {

				throw gcnew OperationCanceledException(*token);
			}

			if(readFrame(&packet) == ReadFrameResult::END_OF_STREAM) {
				
				// flush decoder with null packets
				// see AV_CODEC_CAP_DELAY
				packet.size = 0;
				packet.data = NULL;
				packet.stream_index = videoIdx;
				
			}
		
			if(packet.stream_index == videoIdx)
			{										
				int ret = avcodec_decode_video2(getVideoCodecContext(), frame, &frameFinished, &packet);
				if(ret < 0) {

					//Error decoding video frame
					VideoInit::writeToLog(AV_LOG_WARNING, "could not decode video frame");	
					return(false);
				} 

				if(frameFinished != 0)
				{										
					convertVideoFrame(frame, convertedFrame);				

					if(decodedFrame != NULL) {

						decodedFrame(data, &packet, convertedFrame, VIDEO);
					}

					//av_frame_unref(convertedFrame);
				}

				if(ret == 0) {

					// no more frames to decode
					return(frameFinished != 0 ? true : false);
				}

				av_frame_unref(frame);
			} 

			av_packet_unref(&packet);
		}

		return(true);

	}

	void grabAttachedImages(int maxThumbWidth, int maxThumbHeight, CancellationToken ^token, int timeOutSeconds = 0) {
				
		for(int i = 0; i < (int)stream.size(); i++) {
						
			DateTime startTime = DateTime::Now;
										
			if(stream[i]->getAVStream()->disposition & AV_DISPOSITION_ATTACHED_PIC)
			{
				stream[i]->open();
				//System::Diagnostics::Debug::Print(gcnew String(getFormatContext()->filename) + " " + gcnew String(stream[i]->getCodec()->name));

				AVPacket *picPacket = av_packet_clone(&stream[i]->getAVStream()->attached_pic);

				int frameFinished = 0;
											
				while(frameFinished == 0) {
										
					if(timeOutSeconds > 0 && (DateTime::Now - startTime).TotalSeconds > timeOutSeconds) {

						throw gcnew VideoLibException("Timed out during decoding");
					}

					if(token->IsCancellationRequested) {

						throw gcnew OperationCanceledException(*token);
					}

					int ret = avcodec_decode_video2(stream[i]->getCodecContext(), frame, &frameFinished, picPacket);
					if(ret < 0) {

						//Error decoding video frame
						VideoInit::writeToLog(AV_LOG_WARNING, "could not decode attached image");	
						break;
					} 

					if(frameFinished != 0)
					{					
						float widthScale = 1;
						float heightScale = 1;

						if(maxThumbWidth > 0 && frame->width > maxThumbWidth) {
			
							widthScale = maxThumbWidth / (float)frame->width;				
						}

						if(maxThumbHeight > 0 && frame->height > maxThumbHeight) {
			
							heightScale = maxThumbHeight / (float)frame->height;
						}

						thumbWidth = round(frame->width * std::min<float>(widthScale, heightScale));
						thumbHeight = round(frame->height * std::min<float>(widthScale, heightScale));

						setVideoOutputFormat(AV_PIX_FMT_BGR24, thumbWidth, thumbHeight, LANCZOS);

						convertVideoFrame(frame, convertedFrame);				

						if(decodedFrame != NULL) {

							decodedFrame(data, &packet, convertedFrame, VIDEO);
						}

						//av_frame_unref(convertedFrame);

					} else {
						
						av_packet_unref(picPacket);
						picPacket->size = 0;
						picPacket->data = NULL;
						picPacket->stream_index = videoIdx;
					}

					av_frame_unref(frame);
					
					if(ret == 0) {

						// no more frames to decode
						break;
					}
				}	

				stream[i]->close();
			}
			
		}
		
	}

	int getThumbWidth() const {

		return(thumbWidth);
	}

	int getThumbHeight() const {

		return(thumbHeight);
	}

	

};

}