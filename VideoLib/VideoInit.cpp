#include "stdafx.h"
#include <msclr\marshal_cppstd.h>
#include "VideoInit.h"

namespace VideoLib {

bool VideoInit::isAVlibInitialized = false;
//ThreadSafeList<CRITICAL_SECTION **> *VideoInit::criticalSections = new ThreadSafeList<CRITICAL_SECTION **>();

}