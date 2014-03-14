#pragma once
#include "stdafx.h"
#include <winbase.h>
#include <msclr\marshal_cppstd.h>
#include "ThreadSafeList.h"


extern "C" {

#ifdef __cplusplus
#define __STDC_CONSTANT_MACROS
#ifdef _STDINT_H
#undef _STDINT_H
#endif
# include "stdint.h"
#endif

#include "libavformat/avformat.h"
#include "libavcodec/avcodec.h"

}

class VideoInit {

protected:

	static bool isAVlibInitialized;
	static ThreadSafeList<CRITICAL_SECTION **> *criticalSections;

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
		// Avoid using unintialized criticalSections by keeping track of which
		// criticalSections actually exist

		CRITICAL_SECTION **critSec = (CRITICAL_SECTION **)mutex;
		switch (op) {
		case AV_LOCK_CREATE:
			{
				*critSec = new CRITICAL_SECTION();			
				InitializeCriticalSection(*critSec);		
				criticalSections->add(critSec);
				break;
			}
		case AV_LOCK_OBTAIN:	
			{

				if(criticalSections->contains(critSec) == false) {

					*critSec = new CRITICAL_SECTION();			
					InitializeCriticalSection(*critSec);		
					criticalSections->add(critSec);

				}
				EnterCriticalSection(*critSec);
				break;
			}
		case AV_LOCK_RELEASE:
			{
				if(criticalSections->contains(critSec) == false) {
					return 0;
				}
				LeaveCriticalSection(*critSec);
				break;
			}
		case AV_LOCK_DESTROY:
			{
				if(criticalSections->contains(critSec) == false) {
					return 0;
				}
				DeleteCriticalSection(*critSec);
				criticalSections->remove(critSec);

				delete *critSec;
				*critSec = NULL;				

				break;
			}
		}

		return 0; 
	}


};

bool VideoInit::isAVlibInitialized = false;
ThreadSafeList<CRITICAL_SECTION **> *VideoInit::criticalSections = new ThreadSafeList<CRITICAL_SECTION **>();


