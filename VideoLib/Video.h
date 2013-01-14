#pragma once
#pragma warning(disable : 4244)
#include "stdafx.h"
#include "VideoInit.h"

#define __STDC_CONSTANT_MACROS

extern "C" {

#include "libavformat/avformat.h"

#include "libavcodec/avcodec.h"
#include "libavcodec/audioconvert.h"

#include "libavutil/avutil.h"
#include "libavutil/audioconvert.h"
#include "libavutil/mathematics.h"
#include "libavutil/pixdesc.h"

#include "libswscale/swscale.h"

}

class Video {

protected:

	AVFormatContext *formatContext;

	AVCodecContext *videoCodecContext;
	AVCodec *videoCodec;

	AVCodecContext *audioCodecContext;
	AVCodec *audioCodec;
	
	AVStream *videoStream;
	int videoStreamIndex;

	AVStream *audioStream;
	int audioStreamIndex;

	Video() {

		formatContext = NULL;

		videoCodecContext = NULL;
		videoCodec = NULL;

		audioCodecContext = NULL;
		audioCodec = NULL;

		videoStream = NULL;
		audioStream = NULL;

		videoStreamIndex = -1;
		audioStreamIndex = -1;

		VideoInit::initializeAVLib();
			
	}

public:

	enum FrameType {
		VIDEO,
		AUDIO
	};

	virtual ~Video() {

		close();
	}

	void close() {

		if(formatContext != NULL) {

			if(formatContext != NULL) {

				avformat_free_context(formatContext);
			}
		
		} else {
			
			if(videoCodecContext != NULL) {

				avcodec_close(videoCodecContext);				
			}

			if(audioCodecContext != NULL) {

				avcodec_close(audioCodecContext);				
			}

		}

		formatContext = NULL;
		videoCodecContext = NULL;
		audioCodecContext = NULL;

		videoStreamIndex = -1;
		audioStreamIndex = -1;

		videoStream = NULL;
		audioStream = NULL;

		videoCodec = NULL;
		audioCodec = NULL;

		videoCodecContext = NULL;
		audioCodecContext = NULL;
	
	}


	AVFormatContext *getFormatContext() const {

		return(formatContext);
	}

	AVCodecContext *getVideoCodecContext() const {

		return(videoCodecContext);
	}

	AVCodec *getVideoCodec() const {

		return(videoCodec);
	}

	AVCodecContext *getAudioCodecContext() const {

		return(audioCodecContext);
	}

	AVCodec *getAudioCodec() const {

		return(audioCodec);
	}
	
	AVStream *getVideoStream() const {

		return(videoStream);
	}

	int getvideoStreamIndex() const {

		return(videoStreamIndex);
	}

	AVStream *getAudioStream() const {

		return(audioStream);
	}

	int getAudioStreamIndex() const {

		return(audioStreamIndex);
	}

	bool hasVideo() const {

		return(videoStreamIndex != -1);
	}

	bool hasAudio() const {

		return(audioStreamIndex != -1);
	}

};

