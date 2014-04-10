#pragma once

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

		  VideoLibException(String ^message, Exception ^inner) 
			  : Exception(message, inner) 
		  { 

		  }

	};
}