#pragma once

#include <msclr\marshal_cppstd.h>

using namespace System;
using namespace msclr::interop;
using namespace System::Runtime::InteropServices;

namespace XMPLib {

	[Serializable]
	public ref class XMPLibException : public Exception
	{
	
	public:

		  XMPLibException(String ^message, const char *extraInfo) 
			  : Exception(message + marshal_as<String ^>(extraInfo)) 
		  {

		  }
		  XMPLibException(String ^message) 
			  : Exception(message) 
		  {

		  }

		  XMPLibException(String ^message, Exception ^inner) 
			  : Exception(message, inner) 
		  { 

		  }

	};
}