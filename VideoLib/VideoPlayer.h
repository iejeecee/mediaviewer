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


namespace Native {

class VideoPlayer : public VideoDecoder {

private:

	std::string videoLocation;
	std::string videoFileName;

	int round(float r) {
		return (r > 0.0) ? (int)floor(r + 0.5) : (int)ceil(r - 0.5);
	}


public:

	int durationSeconds;

	int64_t sizeBytes;


	VideoPlayer() 	
	{
		//setDecodedFrameCallback(decodedFrame, this);

		durationSeconds = 0;
		sizeBytes = 0;

	}

	virtual void open(const std::string &location, AVDiscard discardMode = AVDISCARD_DEFAULT) {

		VideoDecoder::open(location, discardMode);

		if(getWidth() == 0 || getHeight() == 0) {

			throw std::runtime_error("invalid video stream");
		}

		initImageConverter(PIX_FMT_BGR24, getWidth(), getHeight(), X);

		// get metadata
		durationSeconds = getDurationSeconds();

		sizeBytes = formatContext->pb ? avio_size(formatContext->pb) : 0;
	}

	virtual void close() {

		VideoDecoder::close();

		durationSeconds = 0;
		sizeBytes = 0;

	}

	virtual ~VideoPlayer() {

	
	}

	void play()
	{
	
	

		//std::cout << "Grabbing: " << nrFrames << " frames\n";

		while(decode(DECODE_VIDEO, SKIP_AUDIO, 1) == 1) {


		}

	}
/*
	static void decodedFrame(void *data, AVPacket *packet, AVFrame *frame, Video::FrameType type) {

		VideoPlayer *This = (VideoPlayer *)data;

		// calculate presentation time for this frame in seconds
		double pts = packet->pts;

		if(packet.dts != AV_NOPTS_VALUE) {
		pts = packet.dts;
		} else {
		pts = 0;
		}
	
		
		double timeStampSeconds = pts * av_q2d(This->videoStream->time_base) - This->startTime;
	
		//ImageRGB24 *frameImage = new ImageRGB24(This->thumbWidth, This->thumbHeight, timeStampSeconds, frame->data[0]);


	}
*/

};

}

