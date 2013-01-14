#pragma once
#include "stdafx.h"
#include <sstream>
#include <iostream>
#include <iomanip>
#include <float.h>
#include <vector>
#include "VideoDecoder.h"
#include "WindowsFileUtil.h"
#include "ImageRGB24.h"


class VideoFramegrabber : protected VideoDecoder {

private:

	std::string videoLocation;
	std::string videoFileName;

	int thumbWidth;
	int thumbHeight;

	int frameNr;

	std::vector<ImageRGB24 *> thumbs;

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

	virtual void close() {

		VideoDecoder::close();

		thumbWidth = 0;
		thumbHeight = 0;

	}

	int greatestCommonDivisor(int a, int b) const {
	  return (b == 0) ? a : greatestCommonDivisor(b, a % b);
	}

	void clearThumbs() {

		for(int i = 0; i < (int)thumbs.size(); i++) {

			delete thumbs[i];
		}

		thumbs.clear();
	}

public:

	VideoFramegrabber() 	
	{
		setDecodedFrameCallback(decodedFrame, this);

	}

	virtual ~VideoFramegrabber() {

	}

	void grab(const std::string &videoLocation, int maxThumbWidth, int maxThumbHeight, 
			int captureInterval, int nrThumbs)
	{

		open(videoLocation, AVDISCARD_NONKEY);
	
		if(getWidth() == 0 || getHeight() == 0) {

			throw std::runtime_error("invalid video stream");
		}

		float widthScale = 1;
		float heightScale = 1;

		if(getWidth() > maxThumbWidth) {
			
			widthScale = maxThumbWidth / (float)getWidth();				
		}

		if(getHeight() > maxThumbHeight) {
			
			heightScale = maxThumbHeight / (float)getHeight();
		}

		thumbWidth = int(getWidth() * std::min(widthScale, heightScale));
		thumbHeight = int(getHeight() * std::min(widthScale, heightScale));

		initImageConverter(PIX_FMT_RGB24, thumbWidth, thumbHeight, SPLINE);

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

		//std::cout << "Grabbing: " << nrFrames << " frames\n";

		double offset = duration * 0.01;
		double step = (duration - offset) / nrFrames;

		for(frameNr = 0; frameNr < nrFrames; frameNr++) {

			double pos = offset + frameNr * step;

			//std::cout << "search pos: " << formatTime(pos, hours, minutes, seconds) << "\n";

			seek(pos);

			if(decode(DECODE_KEY_FRAMES_ONLY, SKIP_AUDIO, 1) != 1) {

				// retry a non-keyframe
				//std::cerr << "grabbing non keyframe\n";
				seek(pos);

				if(decode(DECODE_VIDEO, SKIP_AUDIO, 1) != 1) {

					throw std::runtime_error("could not decode any frames");
				}
			}
		}

		//std::cout << "Completed Output: " << gridFilename << "\n";

		close();
	}

	static void decodedFrame(void *data, AVPacket *packet, AVFrame *frame, Video::FrameType type) {

		VideoFramegrabber *This = (VideoFramegrabber *)data;

		// calculate presentation time for this frame in seconds
		double pts = packet->pts;
		/*
		if(packet.dts != AV_NOPTS_VALUE) {
		pts = packet.dts;
		} else {
		pts = 0;
		}
		*/
		
		double timeStampSeconds = pts * av_q2d(This->videoStream->time_base) - This->startTime;
	
		ImageRGB24 *frameImage = new ImageRGB24(This->thumbWidth, This->thumbHeight, timeStampSeconds, frame->data[0]);

		This->thumbs.push_back(frameImage);

	}

	
};

