#include "VideoTranscode.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Windows::Media::Imaging;
using namespace System::Threading;

namespace VideoLib {

	public ref class VideoTranscoder 
	{

		VideoTranscode *videoTranscode;

	public:

		delegate void TranscodeProgressDelegate(double progress);

		VideoTranscoder();
		~VideoTranscoder();

		void transcode(String ^input, String ^output, CancellationToken ^token, Dictionary<String ^, Object ^> ^options,
			TranscodeProgressDelegate ^progressCallback);
	};
}