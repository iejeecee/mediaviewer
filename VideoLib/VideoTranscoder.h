#include "VideoTranscode.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Windows::Media::Imaging;
using namespace System::Threading;

namespace VideoLib {

	public ref class VideoTranscoder 
	{
		System::Runtime::InteropServices::GCHandle gch;

		VideoTranscode *videoTranscode;

	public:

		enum class LogLevel {

			LOG_LEVEL_FATAL = AV_LOG_FATAL,
			LOG_LEVEL_ERROR = AV_LOG_ERROR,		
			LOG_LEVEL_WARNING = AV_LOG_WARNING,
			LOG_LEVEL_INFO = AV_LOG_INFO,
			LOG_LEVEL_DEBUG = AV_LOG_DEBUG,
			
		};

		delegate void LogCallbackDelegate(int level, String ^message);
		delegate void TranscodeProgressDelegate(double progress);		

		VideoTranscoder();
		~VideoTranscoder();

		void setLogCallback(LogCallbackDelegate ^callback, bool enableLibAVLogging, LogLevel level);

		void transcode(String ^inputVideoLocation, String ^output, CancellationToken token, Dictionary<String ^, Object ^> ^options,
			TranscodeProgressDelegate ^progressCallback, String ^inputAudioLocation);
	};
}