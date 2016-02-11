#pragma once
#include "Video\Video.h"
#include "OpenVideoArgs.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Windows::Media::Imaging;
using namespace System::Threading;

namespace VideoLib {

	public ref class VideoOperations
	{
		System::Runtime::InteropServices::GCHandle gch;
		
	public:

		enum class LogLevel {

			LOG_LEVEL_FATAL = AV_LOG_FATAL,
			LOG_LEVEL_ERROR = AV_LOG_ERROR,		
			LOG_LEVEL_WARNING = AV_LOG_WARNING,
			LOG_LEVEL_INFO = AV_LOG_INFO,
			LOG_LEVEL_DEBUG = AV_LOG_DEBUG,
			
		};

		delegate void LogCallbackDelegate(int level, String ^message);
		delegate void OperationProgressDelegate(int totalProgress, double progress);		

		VideoOperations();
		~VideoOperations();

		void setLogCallback(LogCallbackDelegate ^callback, bool enableLibAVLogging, LogLevel level);

		void transcode(OpenVideoArgs ^openArgs, String ^output, CancellationToken token, Dictionary<String ^, Object ^> ^options,
			OperationProgressDelegate ^progressCallback);

		void concat(List<OpenVideoArgs ^> ^openArgs, String ^output, CancellationToken token, Dictionary<String ^, Object ^> ^options,
			OperationProgressDelegate ^progressCallback);
	};
}