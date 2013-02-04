#pragma once
#include "stdafx.h"
#include <sstream>
#include <iostream>
#include <iomanip>
#include <float.h>
#include <vector>
#include "VideoDecoder.h"
#include "WindowsFileUtil.h"


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


};

}

