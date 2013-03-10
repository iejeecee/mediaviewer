#pragma once
#include "stdafx.h"
#include <sstream>
#include <iostream>
#include <iomanip>
#include <float.h>
#include <vector>
#include "VideoDecoder.h"
#include "WindowsFileUtil.h"
#include "VideoLibException.h"

namespace Native {

class VideoPlayer : public VideoDecoder {

private:

	std::string videoLocation;
	std::string videoFileName;

	int round(float r) {
		return (r > 0.0) ? (int)floor(r + 0.5) : (int)ceil(r - 0.5);
	}

public:

	// video
	int durationSeconds;
	int64_t sizeBytes;
	
	// audio
	int samplesPerSecond;
	int bytesPerSample;
	int nrChannels;

	VideoPlayer() 	
	{
		samplesPerSecond = 0;
		bytesPerSample = 0;
		nrChannels = 0;
		durationSeconds = 0;
		sizeBytes = 0;
	}

	virtual void open(const std::string &location, PixelFormat format = PIX_FMT_BGR24, AVDiscard discardMode = AVDISCARD_DEFAULT) {

		VideoDecoder::open(location, discardMode);

		if(getWidth() == 0 || getHeight() == 0) {

			throw gcnew VideoLib::VideoLibException("invalid video stream");
		}

		initImageConverter(format, getWidth(), getHeight(), X);

		// get metadata
		durationSeconds = getDurationSeconds();

		sizeBytes = formatContext->pb ? avio_size(formatContext->pb) : 0;

		if(hasAudio()) {

			samplesPerSecond = audioCodecContext->sample_rate;
			bytesPerSample = av_get_bytes_per_sample(audioCodecContext->sample_fmt);
			
			nrChannels = audioCodecContext->channels;
		}

	}

	virtual void close() {

		VideoDecoder::close();

		durationSeconds = 0;
		sizeBytes = 0;

	}

	virtual ~VideoPlayer() {

	
	}


};

}

