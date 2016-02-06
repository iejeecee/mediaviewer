#pragma once
#include "stdafx.h"
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

	std::string audioCodecName;

	VideoFrameGrabber() 
	{
		setDecodedFrameCallback(decodedFrame, this);
	
		container = "";

		videoCodecName = "";
		audioCodecName = "";

		bitsPerPixel = 0;
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
			
		}

		probe_dict(formatContext->metadata, "");

		// audio info
		if(hasAudio()) {

			audioCodecName = std::string(getAudioCodec()->name);		
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

	void grab(int thumbWidth, int captureInterval, int nrThumbs, double startOffset, System::Threading::CancellationToken ^ cancellationToken) {

		if(getWidth() == 0 || getHeight() == 0) {

			throw gcnew VideoLib::VideoLibException("invalid video stream");
		}
				
		float scale = thumbWidth / (float)getWidth();				

		this->thumbWidth = thumbWidth;
		thumbHeight = round(getHeight() * scale);

		startGrab(thumbWidth, thumbHeight, captureInterval, nrThumbs,startOffset, cancellationToken, true);
	}

	void grab(int maxThumbWidth, int maxThumbHeight, 
			int captureInterval, int nrThumbs, double startOffset, System::Threading::CancellationToken ^ cancellationToken, int timeOutSeconds)
	{
		if(!hasVideo() && hasAudio()) 
		{			
			return;

		} else if(getWidth() == 0 || getHeight() == 0) {

			throw gcnew VideoLib::VideoLibException("invalid video stream");
		}
		
		float widthScale = 1;
		float heightScale = 1;

		if(getWidth() > maxThumbWidth) {
			
			widthScale = maxThumbWidth / (float)getWidth();				
		}

		if(getHeight() > maxThumbHeight) {
			
			heightScale = maxThumbHeight / (float)getHeight();
		}

		thumbWidth = round(getWidth() * std::min<float>(widthScale, heightScale));
		thumbHeight = round(getHeight() * std::min<float>(widthScale, heightScale));

		startGrab(thumbWidth, thumbHeight, captureInterval, nrThumbs,startOffset, cancellationToken, false, timeOutSeconds);
	}

	void startGrab(int thumbWidth, int thumbHeight, 
		int captureInterval, int nrThumbs, double startOffset, System::Threading::CancellationToken ^cancellationToken,
		bool suppressError = false, int decodingTimeOut = 0)
	{

		setVideoOutputFormat(AV_PIX_FMT_BGR24, thumbWidth, thumbHeight, LANCZOS);

		double duration = getDurationSeconds();

		int nrFrames = 0;

		if(captureInterval == -1) {

			nrFrames = nrThumbs;

		} else {

			nrFrames = duration / captureInterval;

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
			
			bool frameOk = decodeFrame(cancellationToken, decodingTimeOut);

			if(!frameOk) {
			
				if(suppressError == false) {

					throw gcnew VideoLib::VideoLibException("could not decode frame");

				} else {

					return;
				}
				
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
				av_frame_unref(frame);
			
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

				}

				if(ret == 0) {

					// no more frames to decode
					return(frameFinished != 0 ? true : false);
				}

			} 

			av_packet_unref(&packet);
		}

		return(true);

	}

	/*void grabAttachedImage(int maxThumbWidth, int maxThumbHeight, CancellationToken ^token) {

		for(int i = 0; i < stream.size(); i++) {

			if(stream[i]->getStream()->disposition & AV_DISPOSITION_ATTACHED_PIC)
			{
				AVPacket pkt = stream[i]->getStream()->attached_pic;
				
				array<Byte>^ byteArray = gcnew array< Byte >(pkt.size);
			
				Marshal::Copy((IntPtr)pkt.data, byteArray, 0, pkt.size);
				
				System::IO::Stream ^encodedJpeg = gcnew System::IO::MemoryStream(byteArray); 
				
				JpegBitmapDecoder ^decoder = gcnew JpegBitmapDecoder(encodedJpeg, BitmapCreateOptions::PreservePixelFormat, BitmapCacheOption::Default);
				BitmapSource ^bitmapSource = decoder->Frames[0];

				thumbWidth = bitmapSource->PixelWidth;
				thumbHeight = bitmapSource->PixelHeight;

				int bytesPerPixel = bitmapSource->Format.BitsPerPixel / 8;
				int stride = 4 * ((width * bytesPerPixel + 3) / 4);
				int sizeBytes = stride * thumbHeight;

				frame->data[0] = new uint8_t[sizeBytes];
			    frame->linesize[0] = stride;

				bitmapSource->CopyPixels(System::Windows::Int32Rect::Empty, IntPtr(frame->data[0]), sizeBytes, stride);
				
				if(decodedFrame != NULL) {
					decodedFrame(NULL, NULL, frame,VideoLib::Video::FrameType::VIDEO);
				}

				delete frame->data[0];
				frame->data[0] = NULL;
			}
		}
		
	}*/

	int getThumbWidth() const {

		return(thumbWidth);
	}

	int getThumbHeight() const {

		return(thumbHeight);
	}

	bool isVideo() const {
	
		if(!hasVideo()) return false;	

		if((getVideoStream()->disposition & AV_DISPOSITION_ATTACHED_PIC) > 0) return false; //mp3 cover art etc
		if(container.compare("image2 sequence") == 0) return(false); //jpeg

		bool hasFramerate = getFrameRate() > 0;

		bool hasDuration = getDurationSeconds() > 0;

		return hasFramerate || hasDuration;
	}

	bool isAudio() const {

		if(!hasAudio()) return false;		
		if(getFrameRate() > 0) return false;
		
		if(hasVideo()) {
		
			if((getVideoStream()->disposition & AV_DISPOSITION_ATTACHED_PIC) == 0) return false;
		}

		return true;	
	}

	bool isImage() const {

		if(hasAudio()) return false;
		if(getFrameRate() > 0) return false;
		if(container.compare("image2 sequence") == 0) return(true); //jpeg
		if(getDurationSeconds() > 0) return false;
		if(!hasVideo()) return false;
	
		return(true);

	}

};

}