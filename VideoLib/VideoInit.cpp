#include "stdafx.h"
#include <msclr\marshal_cppstd.h>
#include "VideoInit.h"

using namespace System;
using namespace System::Threading;
using namespace System::Runtime::InteropServices;

namespace VideoLib {

void VideoInit::initializeAVLib() {
		
	GCHandle h = GCHandle::FromIntPtr(IntPtr(lockObject));
	Object^ managedLockObject = h.Target;

	// use a lock to make sure initialize is called only once in a multi-threaded situation
	// has to be done in a convoluted manner using managed code, because
	// <mutex> and <thread> are not supported when compiling with /clr or /clr:pure.
	Monitor::Enter(managedLockObject);
	try {
						
		if(!isAVlibInitialized) {
		
			av_register_all();
			avcodec_register_all();
			avfilter_register_all();

			avformat_network_init();

			isAVlibInitialized = true;	

			enableLibAVLogging(logLevel);
		}				

	} finally {

		Monitor::Exit(managedLockObject);	
	}
}


bool VideoInit::isAVlibInitialized = false;
void *VideoInit::lockObject = GCHandle::ToIntPtr(GCHandle::Alloc(gcnew Object())).ToPointer();

int VideoInit::logLevel = AV_LOG_WARNING;
AV_LOG_CALLBACK VideoInit::nativeLogCallback = NULL;
LOG_CALLBACK VideoInit::managedLogCallback = NULL;

}