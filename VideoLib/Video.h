#pragma once
#pragma warning(disable : 4244)
// unsafe function warning disable
#pragma warning(disable : 4996)
#include <algorithm>
#include "stdafx.h"
#include "VideoInit.h"

#define __STDC_CONSTANT_MACROS

extern "C" {

#include "libavformat/avformat.h"

#include "libavcodec/avcodec.h"
//#include "libavcodec/audioconvert.h"

#include "libavutil/avutil.h"
#include "libavutil/audioconvert.h"
#include "libavutil/mathematics.h"
#include "libavutil/pixdesc.h"

#include "libavutil/time.h"

#include "libswscale/swscale.h"
#include "libswresample/swresample.h"

#ifdef PixelFormat
#undef PixelFormat
#endif

}

typedef void (__stdcall *LOG_CALLBACK)(int level, const char *message);

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

	static bool libAVLogOnlyImportant;
	static LOG_CALLBACK logCallback;

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

	static void libAVLogCallback(void *ptr, int level, const char *fmt, va_list vargs)
    {
		char message[65536];   
		const char *module = NULL;

		// Comment back in to filter only "important" messages
		if(libAVLogOnlyImportant == true && level > AV_LOG_WARNING)
			return;

		// Get module name
		if(ptr)
		{
			AVClass *avc = *(AVClass**) ptr;
			module = avc->item_name(ptr);
		}

		std::string fullMessage = "LibAV";

		if(module)
		{
			fullMessage.append(" (");
			fullMessage.append(module);
			fullMessage.append(")");
		}

		vsnprintf(message, sizeof(message), fmt, vargs);

		fullMessage.append(": ");
		fullMessage.append(message);

		// remove trailing newline
		fullMessage.erase(std::remove(fullMessage.begin(), fullMessage.end(), '\n'), fullMessage.end());

		if(logCallback != NULL) {

			logCallback(level, fullMessage.c_str());
		}
      
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

		if(videoCodecContext != NULL) {

			avcodec_close(videoCodecContext);				
		}

		if(audioCodecContext != NULL) {

			avcodec_close(audioCodecContext);				
		}
	
		if(formatContext != NULL) {
					
			avformat_close_input(&formatContext);
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

	int getVideoStreamIndex() const {

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

	static void enableLibAVLogging(bool logOnlyImportant) {

		libAVLogOnlyImportant = logOnlyImportant;
		av_log_set_callback(&Video::libAVLogCallback);
			
	}

	static void disableLibAVLogging() {

		av_log_set_callback(NULL);	
	}

	static void setLogCallback(LOG_CALLBACK logCallback) 
	{
		Video::logCallback = logCallback;
	}

	static void writeToLog(int level, char *message) {

		if(logCallback != NULL) {

			logCallback(level, message);
		}
	}
};

bool Video::libAVLogOnlyImportant = false;
LOG_CALLBACK Video::logCallback = NULL;

