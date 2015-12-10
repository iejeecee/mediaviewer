#include "stdafx.h"
#include <msclr\marshal_cppstd.h>

#include "VideoPreview.h"
#include "VideoFrameGrabber.h"

namespace VideoLib {

using namespace msclr::interop;
using namespace System::Runtime::InteropServices;

VideoThumb::VideoThumb(BitmapSource ^thumb, long positionSeconds) {

	this->thumb = thumb;
	this->positionSeconds = positionSeconds;
}


VideoPreview::VideoPreview() {

	frameGrabber = new VideoFrameGrabber();

	// marshal a managed delegate to a native function pointer
	//http://msdn.microsoft.com/en-us/library/367eeye0%28v=vs.80%29.aspx
	DecodedFrameDelegate ^managedDecodedFrameCallback = 
		gcnew DecodedFrameDelegate(this, &VideoPreview::decodedFrameCallback);

	// make sure the delegate doesn't get garbage collected
	gch = GCHandle::Alloc(managedDecodedFrameCallback);

	IntPtr voidPtr = Marshal::GetFunctionPointerForDelegate(managedDecodedFrameCallback);
		
	DECODED_FRAME_CALLBACK nativeDecodedFrameCallback = static_cast<DECODED_FRAME_CALLBACK>(voidPtr.ToPointer());

	frameGrabber->setDecodedFrameCallback(nativeDecodedFrameCallback, nullptr);

	thumbs = gcnew List<VideoThumb ^>();
	
}

VideoPreview::~VideoPreview() {

	delete frameGrabber;
	gch.Free();
}

void VideoPreview::UTF8ToWString(const std::string &input, String ^%output) {
		
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

void VideoPreview::open(String ^videoLocation, System::Threading::CancellationToken token) {

	try {
		
		frameGrabber->open(videoLocation, token);

		metaData = gcnew List<String ^>();

		for(int i = 0; i < (int)frameGrabber->metaData.size(); i++) {

			String^ info;
			UTF8ToWString(frameGrabber->metaData[i], info);

			metaData->Add(info);
		}

		durationSeconds = frameGrabber->durationSeconds;
		sizeBytes = frameGrabber->sizeBytes;
		width = frameGrabber->width;
		height = frameGrabber->height;
		container = marshal_as<String ^>(frameGrabber->container);
		videoCodecName = marshal_as<String ^>(frameGrabber->videoCodecName);
		frameRate = frameGrabber->getFrameRate();
		pixelFormat =  marshal_as<String ^>(frameGrabber->pixelFormat);

		audioCodecName = marshal_as<String ^>(frameGrabber->audioCodecName);
		bytesPerSample = frameGrabber->bytesPerSample;
		samplesPerSecond = frameGrabber->samplesPerSecond;
		nrChannels = frameGrabber->nrChannels;

	} catch (Exception ^) {

		close();
		throw;
	}
}

void VideoPreview::close() {

	frameGrabber->close();
}

void VideoPreview::decodedFrameCallback(void *data, AVPacket *packet, 
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
	
	long positionSeconds = packet->dts * av_q2d(frameGrabber->getVideoStream()->time_base);

	VideoThumb ^thumb = gcnew VideoThumb(bitmap, positionSeconds);

	thumbs->Add(thumb);

	if(decodedFrameProgressCallback != nullptr) {

		decodedFrameProgressCallback(thumb);
	}

}

List<VideoThumb ^> ^VideoPreview::grabThumbnails(int thumbWidth, int captureInterval, int nrThumbs, double startOffset, 
												 CancellationToken cancellationToken, DecodedFrameProgressDelegate ^decodedFrameProgressCallback) 
{

	this->decodedFrameProgressCallback = decodedFrameProgressCallback;

	thumbs->Clear();

	frameGrabber->grab(thumbWidth, captureInterval, nrThumbs, startOffset, cancellationToken);

	return(thumbs);
}


List<VideoThumb ^> ^VideoPreview::grabThumbnails(int maxThumbWidth, int maxThumbHeight, 
			int captureInterval, int nrThumbs, double startOffset, CancellationToken cancellationToken, int timeoutSeconds) 
{

	thumbs->Clear();

	frameGrabber->grab(maxThumbWidth, maxThumbHeight,
			captureInterval, nrThumbs, startOffset, cancellationToken, timeoutSeconds);

	return(thumbs);
}

}