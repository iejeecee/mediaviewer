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
#include "libavfilter/avfiltergraph.h"
#include "libavfilter/avcodec.h"
#include "libavfilter/buffersink.h"
#include "libavfilter/buffersrc.h"
#include "libavutil/opt.h"

/*

#include <pthread.h>

#elif HAVE_W32THREADS
#include "compat/w32pthreads.h"
#elif HAVE_OS2THREADS
#include "compat/os2threads.h"
#endif
*/
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
			avfilter_register_all();
			
			avformat_network_init();

			isAVlibInitialized = true;	
/*
			if(av_lockmgr_register(lockmgr)) {
 
				return(false);
			}
*/
			
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
	static int default_lockmgr_cb(void **arg, enum AVLockOp op)
	{
		void * volatile * mutex = arg;
		int err;

		switch (op) {
		case AV_LOCK_CREATE:
			return 0;
		case AV_LOCK_OBTAIN:
			if (!*mutex) {
				pthread_mutex_t *tmp = av_malloc(sizeof(pthread_mutex_t));
				if (!tmp)
					return AVERROR(ENOMEM);
				if ((err = pthread_mutex_init(tmp, NULL))) {
					av_free(tmp);
					return AVERROR(err);
				}
				if (avpriv_atomic_ptr_cas(mutex, NULL, tmp)) {
					pthread_mutex_destroy(tmp);
					av_free(tmp);
				}
			}

			if ((err = pthread_mutex_lock(*mutex)))
				return AVERROR(err);

			return 0;
		case AV_LOCK_RELEASE:
			if ((err = pthread_mutex_unlock(*mutex)))
				return AVERROR(err);

			return 0;
		case AV_LOCK_DESTROY:
			if (*mutex)
				pthread_mutex_destroy(*mutex);
			av_free(*mutex);
			avpriv_atomic_ptr_cas(mutex, *mutex, NULL);
			return 0;
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


