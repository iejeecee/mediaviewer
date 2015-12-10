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
		//probe_object_header(name);
		while ((entry = av_dict_get(dict, "", entry, AV_DICT_IGNORE_SUFFIX))) {
					
			metaData.push_back(std::string(entry->key) + ": " + std::string(entry->value));
		
		}
		//probe_object_footer(name);
	}

public:

	int durationSeconds;

	int64_t sizeBytes;

	int width;
	int height;

	std::string container;
	std::string videoCodecName;
	std::string pixelFormat;
	std::vector<std::string> metaData;

	std::string audioCodecName;

	int samplesPerSecond;
	int bytesPerSample;	
	int nrChannels;

	VideoFrameGrabber() 
	{
		setDecodedFrameCallback(decodedFrame, this);

		durationSeconds = 0;
		sizeBytes = 0;
		width = 0;
		height = 0;
		container = "";
		videoCodecName = "";

		audioCodecName = "";

		samplesPerSecond = 0;
		bytesPerSample = 0;	
		nrChannels = 0;

	}

	virtual void open(String ^location, System::Threading::CancellationToken ^ token) {

		VideoDecoder::open(location, token, nullptr);

		// get video metadata
		durationSeconds = getDurationSeconds();

		sizeBytes = formatContext->pb ? avio_size(formatContext->pb) : 0;
		container = std::string(formatContext->iformat->long_name);

		if(hasVideo()) 
		{
			width = getWidth();
			height = getHeight();
				
			videoCodecName = std::string(getVideoCodec()->name);
	
			char buf[64];
			av_get_pix_fmt_string(buf, 64, getVideoCodecContext()->pix_fmt);
			pixelFormat = std::string(buf);

			int pos = (int)pixelFormat.find_first_of(' ');
			if(pos != std::string::npos) {

				pixelFormat = pixelFormat.substr(0, pos);
			}
						
		}

		probe_dict(formatContext->metadata, "");

		// audio info
		if(hasAudio()) {

			audioCodecName = std::string(getAudioCodec()->name);

			samplesPerSecond = getAudioCodecContext()->sample_rate;
			bytesPerSample = av_get_bytes_per_sample(getAudioCodecContext()->sample_fmt);
		
			nrChannels = getAudioCodecContext()->channels;
		}
	}

	virtual void close() {

		VideoDecoder::close();

		durationSeconds = 0;
		sizeBytes = 0;
		width = 0;
		height = 0;
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

		setVideoOutputFormat(PIX_FMT_BGR24, thumbWidth, thumbHeight, SPLINE);

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

			bool seekSuccess = seek(pos);

			if(seekSuccess == false && nrFrames == 1) {
				
				seekSuccess = seek(0);
			}
			
			bool frameOk = decodeFrame(cancellationToken, DECODE_VIDEO, SKIP_AUDIO, decodingTimeOut);

			if(!frameOk) {
			
				if(suppressError == false) {

					throw gcnew VideoLib::VideoLibException("could not decode frame");

				} else {

					return;
				}
				
			}
				
		}

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

};

}