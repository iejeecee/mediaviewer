#pragma once
#include "VideoInit.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace VideoLib {

	[Serializable]
	public ref class VideoLibException : public Exception
	{
	
	public:
		  VideoLibException(String ^message) 
			  : Exception(message) 
		  {
			  
		  }

		  VideoLibException(String ^message, int averror) 
			  : Exception(message + VideoInit::errorToString(averror)) 
		  {
			  
		  }

		  VideoLibException(String ^message, Exception ^inner) 
			  : Exception(message, inner) 
		  { 

		  }

	};
}