#include "Video\Video.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Windows::Media::Imaging;
using namespace System::Threading;

namespace VideoLib {

public ref class CodecInfo {

private:
	String^ name;
	List<int>^ supportedSampleRates;

	CodecInfo(AVCodec *codec);

public:

	static List<CodecInfo ^> ^getCodecs();

	property String^ Name {

		String^ get() {

			return(name);
		}
	}

	property List<int>^ SupportedSampleRates {

		List<int>^ get() {

			return(supportedSampleRates);
		}
	}

};

}