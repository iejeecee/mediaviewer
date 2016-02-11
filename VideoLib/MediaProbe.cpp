#include "stdafx.h"
#include <msclr\marshal_cppstd.h>

#include "MediaProbe.h"
#include "Video\VideoFrameGrabber.h"

namespace VideoLib {

using namespace msclr::interop;
using namespace System::Runtime::InteropServices;

MediaThumb::MediaThumb(BitmapSource ^thumb, long positionSeconds) {

	this->thumb = thumb;
	this->positionSeconds = positionSeconds;
}


MediaProbe::MediaProbe() {

	frameGrabber = new VideoFrameGrabber();

	// marshal a managed delegate to a native function pointer
	//http://msdn.microsoft.com/en-us/library/367eeye0%28v=vs.80%29.aspx
	DecodedFrameDelegate ^managedDecodedFrameCallback = 
		gcnew DecodedFrameDelegate(this, &MediaProbe::decodedFrameCallback);

	// make sure the delegate doesn't get garbage collected
	gch = GCHandle::Alloc(managedDecodedFrameCallback);

	IntPtr voidPtr = Marshal::GetFunctionPointerForDelegate(managedDecodedFrameCallback);
		
	DECODED_FRAME_CALLBACK nativeDecodedFrameCallback = static_cast<DECODED_FRAME_CALLBACK>(voidPtr.ToPointer());

	frameGrabber->setDecodedFrameCallback(nativeDecodedFrameCallback, nullptr);

	thumbs = gcnew List<MediaThumb ^>();
	
}

MediaProbe::~MediaProbe() {

	delete frameGrabber;
	gch.Free();
}

void MediaProbe::UTF8ToWString(const std::string &input, String ^%output) {
		
	// How long will the UTF-16 string be
	int wstrlen = MultiByteToWideChar(CP_UTF8, 0, input.c_str(), (int)input.length(), NULL, NULL );
	
	// allocate a buffer
	wchar_t *buf = (wchar_t * ) malloc( wstrlen * 2 + 2 );
	// convert to UTF-16
	MultiByteToWideChar(CP_UTF8, 0, input.c_str(), (int)input.length(), buf, wstrlen);
	// null terminate
	buf[wstrlen] = '\0';
	
	output = marshal_as<String ^>(buf);

	delete buf;
}

void MediaProbe::open(String ^mediaLocation, System::Threading::CancellationToken token) {

	try {
		
		frameGrabber->open(gcnew OpenVideoArgs(mediaLocation), token);

		metaData = gcnew List<String ^>();

		for(int i = 0; i < (int)frameGrabber->metaData.size(); i++) {

			String^ info;
			UTF8ToWString(frameGrabber->metaData[i], info);

			metaData->Add(info);
		}

		durationSeconds = frameGrabber->getDurationSeconds();
		sizeBytes = frameGrabber->getSizeBytes();
		width = frameGrabber->getWidth();
		height = frameGrabber->getHeight();
		container = marshal_as<String ^>(frameGrabber->container);
		videoCodecName = marshal_as<String ^>(frameGrabber->videoCodecName);
		frameRate = frameGrabber->getFrameRate();
		pixelFormat =  marshal_as<String ^>(frameGrabber->pixelFormat);
		bitsPerPixel = frameGrabber->bitsPerPixel;
		isVideo = frameGrabber->isVideo();
		isAudio = frameGrabber->isAudio();
		isImage = frameGrabber->isImage();

		audioCodecName = marshal_as<String ^>(frameGrabber->audioCodecName);
		bytesPerSample = frameGrabber->getAudioBytesPerSample();
		samplesPerSecond = frameGrabber->getAudioSamplesPerSecond();
		nrChannels = frameGrabber->getAudioNrChannels();

	} catch (Exception ^) {

		close();
		throw;
	}
}

void MediaProbe::close() {

	frameGrabber->close();
}

void MediaProbe::decodedFrameCallback(void *data, AVPacket *packet, 
									   AVFrame *frame, Video::FrameType type)
{	
	int sizeBytes = frameGrabber->getThumbHeight() * frame->linesize[0];

	BitmapSource ^bitmap = BitmapSource::Create(
		frameGrabber->getThumbWidth(), frameGrabber->getThumbHeight(),
		1 / 72.0,  1 / 72.0,
		System::Windows::Media::PixelFormats::Bgr24,
		nullptr,
		IntPtr(frame->data[0]),
		sizeBytes,
		frame->linesize[0]);
	
	bitmap->Freeze();
		
	long positionSeconds = frameGrabber->getStream(packet->stream_index)->getTimeSeconds(packet->dts);

	MediaThumb ^thumb = gcnew MediaThumb(bitmap, positionSeconds);

	thumbs->Add(thumb);

	if(decodedFrameProgressCallback != nullptr) {

		decodedFrameProgressCallback(thumb);
	}

}

/*List<MediaThumb ^> ^MediaProbe::grabThumbnails(int thumbWidth, int captureInterval, int nrThumbs, double startOffset, 
												 CancellationToken cancellationToken, DecodedFrameProgressDelegate ^decodedFrameProgressCallback) 
{

	this->decodedFrameProgressCallback = decodedFrameProgressCallback;

	thumbs->Clear();

	frameGrabber->grab(thumbWidth, captureInterval, nrThumbs, startOffset, cancellationToken);

	return(thumbs);
}*/


List<MediaThumb ^> ^MediaProbe::grabThumbnails(int maxThumbWidth, int maxThumbHeight, 
			double captureIntervalSeconds, int nrThumbs, double startOffset, 
			CancellationToken cancellationToken, int timeoutSeconds,
			DecodedFrameProgressDelegate ^decodedFrameProgressCallback) 
{

	this->decodedFrameProgressCallback = decodedFrameProgressCallback;

	thumbs->Clear();

	frameGrabber->grab(maxThumbWidth, maxThumbHeight,
			captureIntervalSeconds, nrThumbs, startOffset, cancellationToken, timeoutSeconds);

	return(thumbs);
}

}