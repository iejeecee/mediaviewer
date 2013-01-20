// This is the main DLL file.

#include "stdafx.h"
#include <msclr\marshal_cppstd.h>

#include "VideoLib.h"
#include "VideoFrameGrabber.h"

using namespace msclr::interop;
using namespace System::Runtime::InteropServices;

namespace VideoLib {

VideoPreview::VideoPreview() {

	frameGrabber = new VideoFrameGrabber();
}

VideoPreview::~VideoPreview() {

	delete frameGrabber;
}

void VideoPreview::open(String ^videoLocation) {

	try {

		frameGrabber->open(marshal_as<std::string>(videoLocation), AVDISCARD_NONKEY);

	} catch (Exception ^) {

		close();
	}
}

void VideoPreview::close() {

	frameGrabber->close();
}


List<RawImageRGB24 ^> ^VideoPreview::grabThumbnails(int maxThumbWidth, int maxThumbHeight, 
			int captureInterval, int nrThumbs) 
{

	List<RawImageRGB24 ^> ^images = gcnew List<RawImageRGB24 ^>();

	try {

		frameGrabber->grab(maxThumbWidth, maxThumbHeight,
			captureInterval, nrThumbs);

		for(int i = 0; i < (int)frameGrabber->getThumbs().size(); i++) {

			ImageRGB24 *thumb = frameGrabber->getThumbs()[i];

			cli::array<unsigned char> ^data = gcnew cli::array<unsigned char>(thumb->getSizeBytes());

			Marshal::Copy(IntPtr((void *)thumb->getData()), data, 0, thumb->getSizeBytes());

			images->Add(gcnew RawImageRGB24(thumb->getWidth(), thumb->getHeight(), 
				thumb->getTimeStampSeconds(), data));

		}

	} catch(Exception ^) {

	} finally {

		frameGrabber->clearThumbs();
		
	}

	return(images);
}

VideoMetaData ^VideoPreview::readMetaData() {

	List<String ^> ^meta = gcnew List<String ^>();

	for(int i = 0; i < (int)frameGrabber->metaData.size(); i++) {

		meta->Add(marshal_as<String ^>(frameGrabber->metaData[i]));

	}

	VideoMetaData ^metaData = gcnew VideoMetaData(
		frameGrabber->durationSeconds, 
		frameGrabber->sizeBytes,
		frameGrabber->width, 
		frameGrabber->height, 
		marshal_as<String ^>(frameGrabber->container),
		marshal_as<String ^>(frameGrabber->videoCodecName),
		meta,
		frameGrabber->frameRate);


	return(metaData);
}

}

