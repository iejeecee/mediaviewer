#include "stdafx.h"
#include <msclr\marshal_cppstd.h>

#include "VideoOperations.h"
#include "VideoTranscode.h"
#include "VideoConcat.h"

namespace VideoLib {

using namespace msclr::interop;
using namespace System::Runtime::InteropServices;

VideoOperations::VideoOperations() 
{
	
}

VideoOperations::~VideoOperations() 
{
	
}

void VideoOperations::setLogCallback(LogCallbackDelegate ^logCallback, bool enableLibAVLogging, 
									 LogLevel level)
{

	/*if(gch.IsAllocated) {

		gch.Free();
	}

	gch = GCHandle::Alloc(logCallback);

	IntPtr voidPtr = Marshal::GetFunctionPointerForDelegate(logCallback);
		
	LOG_CALLBACK nativeLogCallback = static_cast<LOG_CALLBACK>(voidPtr.ToPointer());

	Video::setLogCallback(nativeLogCallback);

	if(enableLibAVLogging == true) {

		Video::enableLibAVLogging((int)level);

	} else {

		Video::disableLibAVLogging();
	}*/

}

void VideoOperations::transcode(OpenVideoArgs ^openArgs, String ^output, CancellationToken token, 
								Dictionary<String ^, Object ^> ^options, OperationProgressDelegate ^progressCallback)
{
	GCHandle gch = GCHandle::Alloc(progressCallback);
	try {

		VideoTranscode videoTranscode;

		IntPtr ip = Marshal::GetFunctionPointerForDelegate(progressCallback);
		VideoTranscode::ProgressCallback cb = static_cast<VideoTranscode::ProgressCallback>(ip.ToPointer());

		videoTranscode.transcode(openArgs, output, token, options, cb);

	} finally {

		gch.Free();
	}
}

void VideoOperations::concat(List<OpenVideoArgs ^> ^openArgs, String ^output, CancellationToken token, 
								Dictionary<String ^, Object ^> ^options, OperationProgressDelegate ^progressCallback)
{
	GCHandle gch = GCHandle::Alloc(progressCallback);
	try {

		VideoConcat videoConcat;

		IntPtr ip = Marshal::GetFunctionPointerForDelegate(progressCallback);
		VideoTranscode::ProgressCallback cb = static_cast<VideoTranscode::ProgressCallback>(ip.ToPointer());

		videoConcat.concat(openArgs, output, token, options, cb);

	} finally {

		gch.Free();
	}
}

}