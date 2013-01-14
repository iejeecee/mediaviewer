#pragma once
#include "stdafx.h"

#define __STDC_CONSTANT_MACROS

extern "C" {

#include "libavformat/avformat.h"
#include "libavcodec/avcodec.h"

}

class VideoInit {

protected:

	static bool isAVlibInitialized;

public:

	static bool initializeAVLib() {

		if(!isAVlibInitialized) {

			av_register_all();
			avcodec_register_all();

			avformat_network_init();

			isAVlibInitialized = true;
		}		

		return(isAVlibInitialized);
	}

	static void listAllRegisteredCodecs() {

		AVCodec *codec = NULL;

		while(codec = av_codec_next(codec)) {

			std::cout << codec->name << " (" << codec->long_name << ")\n"; 
	
		} 

	}
};

bool VideoInit::isAVlibInitialized = false;