// This is the main DLL file.

#include "stdafx.h"
#include <msclr\marshal_cppstd.h>

#include "VideoLib.h"
#include "VideoFramegrabber.h"

using namespace msclr::interop;
using namespace System::Runtime::InteropServices;

namespace VideoLib {


List<RawImageRGB24 ^> ^VideoPreview::grab(String ^videoLocation, int maxThumbWidth, int maxThumbHeight, 
			int captureInterval, int nrThumbs) 
{

	List<RawImageRGB24 ^> ^images = gcnew List<RawImageRGB24 ^>();

	VideoFramegrabber grabber;

	try {

		grabber.grab(marshal_as<std::string>(videoLocation), maxThumbWidth, maxThumbHeight,
			captureInterval, nrThumbs);

		for(int i = 0; i < (int)grabber.getThumbs().size(); i++) {

			ImageRGB24 *thumb = grabber.getThumbs()[i];

			cli::array<unsigned char> ^data = gcnew cli::array<unsigned char>(thumb->getSizeBytes());

			Marshal::Copy(IntPtr((void *)thumb->getData()), data, 0, thumb->getSizeBytes());

			images->Add(gcnew RawImageRGB24(thumb->getWidth(), thumb->getHeight(), 
				thumb->getTimeStampSeconds(), data));

		}

	} catch(Exception ^) {

	} finally {

		grabber.clearThumbs();
		
	}

	return(images);
}


}

