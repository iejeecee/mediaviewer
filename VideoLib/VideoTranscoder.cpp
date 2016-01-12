#include "stdafx.h"
#include <msclr\marshal_cppstd.h>

#include "VideoTranscoder.h"

namespace VideoLib {

using namespace msclr::interop;
using namespace System::Runtime::InteropServices;

VideoTranscoder::VideoTranscoder() 
{
	videoTranscode = new VideoTranscode();
}

VideoTranscoder::~VideoTranscoder() 
{
	delete videoTranscode;
}

void VideoTranscoder::setLogCallback(LogCallbackDelegate ^logCallback, bool enableLibAVLogging, 
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

void VideoTranscoder::transcode(OpenVideoArgs ^openArgs, String ^output, CancellationToken token, 
								Dictionary<String ^, Object ^> ^options, TranscodeProgressDelegate ^progressCallback)
{
	GCHandle gch = GCHandle::Alloc(progressCallback);
	try {
				
		IntPtr ip = Marshal::GetFunctionPointerForDelegate(progressCallback);
		VideoTranscode::ProgressCallback cb = static_cast<VideoTranscode::ProgressCallback>(ip.ToPointer());

		videoTranscode->transcode(openArgs, output, token, options, cb);

	} finally {

		gch.Free();
	}
}

}