#pragma once
#include "stdafx.h"
#include <winbase.h>
#include <msclr\marshal_cppstd.h>
#include "ThreadSafeList.h"
#include <iostream>    

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
#include "libavfilter/avcodec.h"
#include "libavfilter/buffersink.h"
#include "libavfilter/buffersrc.h"
#include "libavutil/opt.h"

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


namespace VideoLib {

class VideoInit {

protected:

	static bool isAVlibInitialized;
	static void *lockObject;

public:

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
