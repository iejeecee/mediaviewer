#pragma once
#include "Video\VideoInit.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace VideoLib2 {

	[Serializable]
	public ref class VideoLibException : public Exception
	{
	
	public:
		  VideoLibException(String ^message) 
			  : Exception(message) 
		  {
			  IntPtr p = Marshal::StringToHGlobalAnsi(message);
			  const char *messagePtr = static_cast<char*>(p.ToPointer());
							
			  VideoInit::writeToLog(AV_LOG_ERROR, messagePtr);

			  Marshal::FreeHGlobal(p);
		  }

		  VideoLibException(String ^message, int averror) 
			  : Exception(message + VideoInit::errorToString(averror)) 
		  {
			  IntPtr p = Marshal::StringToHGlobalAnsi(message + VideoInit::errorToString(averror));
			  const char *messagePtr = static_cast<char*>(p.ToPointer());
							
			  VideoInit::writeToLog(AV_LOG_ERROR, messagePtr);

			  Marshal::FreeHGlobal(p);
			  
		  }
		 
	};
}