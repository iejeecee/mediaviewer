#pragma once
#include "stdafx.h"
#include <winbase.h>
#include <msclr\marshal_cppstd.h>
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

			if(av_lockmgr_register(lockmgr)) {
 
				return(false);
			}

			
		}		

		return(isAVlibInitialized);
	}

	static void listAllRegisteredCodecs() {

		AVCodec *codec = NULL;

		while(codec = av_codec_next(codec)) {

			std::cout << codec->name << " (" << codec->long_name << ")\n"; 
	
		} 

	}

/*
	static int lockmgr(void **mtx, enum AVLockOp op)
	{
		switch(op) 
		{
		case AV_LOCK_CREATE:
			{
				*mtx = malloc(sizeof(pthread_mutex_t));
				if(!*mtx)
					return 1;
				return !!pthread_mutex_init(*mtx, NULL);
			}
		case AV_LOCK_OBTAIN:
			{
				return !!pthread_mutex_lock(*mtx);
			}
		case AV_LOCK_RELEASE:
			{
				return !!pthread_mutex_unlock(*mtx);
			}
		case AV_LOCK_DESTROY:
			{
				pthread_mutex_destroy(*mtx);
				free(*mtx);
				return 0;
			}
		}
		return 1;
	}
*/
	static int lockmgr(void **mutex, enum AVLockOp op)
	{
		CRITICAL_SECTION **critSec = (CRITICAL_SECTION **)mutex;
        switch (op) {
        case AV_LOCK_CREATE:
                *critSec = new CRITICAL_SECTION();
                InitializeCriticalSection(*critSec);
                break;
        case AV_LOCK_OBTAIN:
                EnterCriticalSection(*critSec);
                break;
        case AV_LOCK_RELEASE:
                LeaveCriticalSection(*critSec);
                break;
        case AV_LOCK_DESTROY:
                DeleteCriticalSection(*critSec);
                delete *critSec;
                break;
        }
        return 0; 
	}


};

bool VideoInit::isAVlibInitialized = false;