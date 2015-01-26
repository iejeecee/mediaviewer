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

void VideoTranscoder::transcode(String ^input, String ^output, CancellationToken ^token, 
								Dictionary<String ^, Object ^> ^options, TranscodeProgressDelegate ^progressCallback)
{
	GCHandle gch = GCHandle::Alloc(progressCallback);
	try {
				
		IntPtr ip = Marshal::GetFunctionPointerForDelegate(progressCallback);
		VideoTranscode::ProgressCallback cb = static_cast<VideoTranscode::ProgressCallback>(ip.ToPointer());

		videoTranscode->transcode(input, output, token, options, cb);

	} finally {

		gch.Free();
	}
}

}