// FFMPEG Video Player tutorial http://dranger.com/ffmpeg/tutorial05.html
#include "stdafx.h"
#include <msclr\marshal_cppstd.h>
#include "CodecInfo.h"
#include "Video.h"

namespace VideoLib {

using namespace msclr::interop;
using namespace System::Runtime::InteropServices;

List<CodecInfo ^> ^CodecInfo::getCodecs() {

	List<CodecInfo ^> ^result = gcnew List<CodecInfo ^>();

	AVCodec *codec = av_codec_next(NULL);

	while(codec != NULL) {

		if(codec->encode2 != NULL && codec->name != NULL) {

			result->Add(gcnew CodecInfo(codec));	
		}
		
		codec = av_codec_next(codec);
	}

	return(result);
}

CodecInfo::CodecInfo(AVCodec *codec) {

	name = marshal_as<String ^>(std::string(codec->name));

	supportedSampleRates = gcnew List<int>();

	int i = 0;

	while(codec->supported_samplerates != NULL && codec->supported_samplerates[i] != 0) {

		supportedSampleRates->Add(codec->supported_samplerates[i]);
		i++;
	}

	i = 0;

		
}

}