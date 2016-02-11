#pragma once
// unsafe function warning disable
#pragma warning(disable : 4996)
#include "stdafx.h"
#include <winbase.h>
#include <msclr\marshal_cppstd.h>
#include "ThreadSafeList.h"
#include <iostream>   
#include <algorithm>

extern "C" {

#ifdef __cplusplus
#define __STDC_CONSTANT_MACROS
#ifdef _STDINT_H
#undef _STDINT_H
#endif
#include "stdint.h"
#endif

#include "libavformat/avformat.h"
#include "libavcodec/avcodec.h"
#include "libavfilter/avfiltergraph.h"
//#include "libavfilter/avcodec.h"
#include "libavfilter/buffersink.h"
#include "libavfilter/buffersrc.h"
#include "libavutil/opt.h"

#include "libavutil/avutil.h"
//#include "libavutil/audioconvert.h"
#include "libavutil/mathematics.h"
#include "libavutil/pixdesc.h"

#include "libavutil/time.h"

#include "libswscale/swscale.h"
#include "libswresample/swresample.h"

#ifdef PixelFormat
#undef PixelFormat
#endif
}

namespace VideoLib {

typedef void (__stdcall *LOG_CALLBACK)(int level, const char *message);
typedef void (*AV_LOG_CALLBACK)(void*, int, const char*, va_list);

class VideoInit {

protected:

	static bool isAVlibInitialized;
	static void *lockObject;

	static int logLevel;
	static AV_LOG_CALLBACK nativeLogCallback;
	static LOG_CALLBACK managedLogCallback;

	static void libAVLogCallback(void *ptr, int level, const char *fmt, va_list vargs)
    {
		char message[65536];   
		const char *module = NULL;

		// if no logging is done or level is above treshold return
		if(managedLogCallback == NULL || level > av_log_get_level()) return;
	
		// Get module name
		if(ptr)
		{
			AVClass *avc = *(AVClass**) ptr;
			module = avc->item_name(ptr);
		}

		std::string fullMessage = "FFMPEG";

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

		managedLogCallback(level, fullMessage.c_str());
	
    }

public:
	

	static void enableLibAVLogging(int logLevel = AV_LOG_ERROR) {
	
		VideoInit::logLevel = logLevel;
		VideoInit::nativeLogCallback = &libAVLogCallback;

		if(isAVlibInitialized) {

			av_log_set_level(VideoInit::logLevel); 
			av_log_set_callback(VideoInit::nativeLogCallback);			
		}
			
	}

	static void disableLibAVLogging() {

		VideoInit::nativeLogCallback = NULL;

		if(isAVlibInitialized) {

			av_log_set_callback(VideoInit::nativeLogCallback);	
		}
	}

	static void setLogCallback(LOG_CALLBACK logCallback) 
	{
		VideoInit::managedLogCallback = logCallback;
	}

	static void writeToLog(int level, const char *message) {

		if(managedLogCallback != NULL) {

			std::string fullMessage = "VideoLib: ";
			fullMessage.append(message);

			managedLogCallback(level, fullMessage.c_str());
		}
	}

	static void initializeAVLib();
	
	static int getAvFormatVersion() {
		return(avformat_version());
	}

	static std::string getBuildConfig() {
		return(avformat_configuration());
	}

	static void listAllRegisteredCodecs() {

		AVCodec *codec = NULL;

		while(codec = av_codec_next(codec)) {

			std::cout << codec->name << " (" << codec->long_name << ")\n"; 
	
		} 

	}

	static System::String ^errorToString(int err)
	{
		char errbuf[128];
		const char *errbuf_ptr = errbuf;

		if (av_strerror(err, errbuf, sizeof(errbuf)) < 0)
			strerror_s(errbuf,AVUNERROR(err));
		
		return(msclr::interop::marshal_as<System::String^>(errbuf_ptr));
	}


};



}
