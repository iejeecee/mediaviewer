// This is the main DLL file.
// FFMPEG Video Player tutorial http://dranger.com/ffmpeg/tutorial05.html

#include "stdafx.h"
#include <msclr\marshal_cppstd.h>

#include "VideoLib.h"
#include "VideoFrameGrabber.h"

using namespace msclr::interop;
using namespace System::Runtime::InteropServices;
using namespace Microsoft::DirectX;
using namespace Microsoft::DirectX::Direct3D;

namespace VideoLib {

VideoFrame::VideoFrame(Device ^device, int width, int height, Format pixelFormat) {

	frame = gcnew Texture(device, width, height, 1, Usage::None, pixelFormat, 
		Pool::Managed);
	pts = 0;
}

VideoFrame::~VideoFrame() {

	if(frame != nullptr) {

		delete frame;
	}
}

VideoPreview::VideoPreview() {

	frameGrabber = new VideoFrameGrabber();

	// marshal a managed delegate to a native function pointer
	//http://msdn.microsoft.com/en-us/library/367eeye0%28v=vs.80%29.aspx
	DecodedFrameDelegate ^managedDecodedFrameCallback = 
		gcnew DecodedFrameDelegate(this, &VideoPreview::decodedFrameCallback);

	// make sure the delegate doesn't get garbage collected
	GCHandle gch = GCHandle::Alloc(managedDecodedFrameCallback);

	IntPtr voidPtr =  Marshal::GetFunctionPointerForDelegate(managedDecodedFrameCallback);
		
	DECODED_FRAME_CALLBACK nativeDecodedFrameCallback = static_cast<DECODED_FRAME_CALLBACK>(voidPtr.ToPointer());

	frameGrabber->setDecodedFrameCallback(nativeDecodedFrameCallback, nullptr);

	thumbs = gcnew List<Drawing::Bitmap ^>();
	
}

VideoPreview::~VideoPreview() {

	delete frameGrabber;
	gch.Free();
}

void VideoPreview::open(String ^videoLocation) {

	try {

		frameGrabber->open(marshal_as<std::string>(videoLocation), AVDISCARD_NONKEY);

		metaData = gcnew List<String ^>();

		for(int i = 0; i < (int)frameGrabber->metaData.size(); i++) {

			metaData->Add(marshal_as<String ^>(frameGrabber->metaData[i]));

		}

		durationSeconds = frameGrabber->durationSeconds;
		sizeBytes = frameGrabber->sizeBytes;
		width = frameGrabber->width;
		height = frameGrabber->height;
		container = marshal_as<String ^>(frameGrabber->container);
		videoCodecName = marshal_as<String ^>(frameGrabber->videoCodecName);
		frameRate = frameGrabber->frameRate;

	} catch (Exception ^) {

		close();
	}
}

void VideoPreview::close() {

	frameGrabber->close();
}

void VideoPreview::decodedFrameCallback(void *data, AVPacket *packet, 
									   AVFrame *frame, Video::FrameType type)
{
	Drawing::Bitmap ^bitmap = gcnew Drawing::Bitmap(frameGrabber->getThumbWidth(),
		frameGrabber->getThumbHeight(), Drawing::Imaging::PixelFormat::Format24bppRgb);

	Drawing::Rectangle rect = 
		Drawing::Rectangle(0, 0, bitmap->Width, bitmap->Height);

	// copy raw frame data to bitmap
	Drawing::Imaging::BitmapData ^bmpData = bitmap->LockBits(rect, Drawing::Imaging::ImageLockMode::WriteOnly,
		Drawing::Imaging::PixelFormat::Format24bppRgb);

	IntPtr dest = bmpData->Scan0;

	int sizeBytes = bmpData->Height * bmpData->Stride;

	memcpy(dest.ToPointer(), frame->data[0], sizeBytes);

	bitmap->UnlockBits(bmpData);

	thumbs->Add(bitmap);

}


List<Drawing::Bitmap ^> ^VideoPreview::grabThumbnails(int maxThumbWidth, int maxThumbHeight, 
			int captureInterval, int nrThumbs) 
{

	thumbs->Clear();

	try {

		frameGrabber->grab(maxThumbWidth, maxThumbHeight,
			captureInterval, nrThumbs);

	} finally {

		
		
	}

	return(thumbs);
}



VideoPlayer::VideoPlayer(Microsoft::DirectX::Direct3D::Device ^device) {

	this->device = device;

	videoPlayer = new Native::VideoPlayer();
	// marshal a managed delegate to a native function pointer
	//http://msdn.microsoft.com/en-us/library/367eeye0%28v=vs.80%29.aspx
	DecodedFrameDelegate ^managedDecodedFrameCallback = 
		gcnew DecodedFrameDelegate(this, &VideoPlayer::decodedFrameCallback);

	// make sure the delegate doesn't get garbage collected
	GCHandle gch = GCHandle::Alloc(managedDecodedFrameCallback);

	IntPtr voidPtr =  Marshal::GetFunctionPointerForDelegate(managedDecodedFrameCallback);
		
	DECODED_FRAME_CALLBACK nativeDecodedFrameCallback = static_cast<DECODED_FRAME_CALLBACK>(voidPtr.ToPointer());

	videoPlayer->setDecodedFrameCallback(nativeDecodedFrameCallback, nullptr);

	frameData = gcnew array<VideoFrame ^>(nrFramesInBuffer);

	videoClock = 0;

}

VideoPlayer::~VideoPlayer() {

	delete videoPlayer;
	gch.Free();
}

// calculate a new pts for a videoframe if it's pts is missing
double VideoPlayer::synchronizeVideo(int repeatFrame, double pts) {

	double frameDelay;

	if(pts != 0) {

		// if we have pts, set video clock to it 
		videoClock = pts;

	} else {

		// if we aren't given a pts, set it to the clock 
		pts = videoClock;
	}

	// update the video clock
	frameDelay = av_q2d(videoPlayer->getVideoStream()->time_base);
	// if we are repeating a frame, adjust clock accordingly 
	frameDelay += repeatFrame * (frameDelay * 0.5);
	videoClock += frameDelay;

	return(pts);
}

void VideoPlayer::decodedFrameCallback(void *data, AVPacket *packet, 
									   AVFrame *frame, Video::FrameType type)
{

	// get a frame from the free frames queue
	VideoFrame ^videoFrame = freeFrames->Take();
	Texture ^bitmap = videoFrame->Frame; 		

	Drawing::Rectangle rect = 
		Drawing::Rectangle(0, 0, videoPlayer->getWidth(), videoPlayer->getHeight());

	// copy raw frame data to bitmap
	int pitch;
	GraphicsStream ^stream = bitmap->LockRectangle(0, rect, LockFlags::None, pitch);

	void *dest = stream->InternalDataPointer;

	int sizeBytes = rect.Height * pitch;

	memcpy(dest, frame->data[0], sizeBytes);

	bitmap->UnlockRectangle(0);

	// calculate presentation timestamp (pts)
	double pts = 0;

    if(packet->dts != AV_NOPTS_VALUE) {

      pts = packet->dts;

    } else {

      pts = 0;
    }

	// convert pts to seconds
	pts *= av_q2d(videoPlayer->getVideoStream()->time_base);

	// calculate a pts if pts equals 0
	videoFrame->Pts = synchronizeVideo(frame->repeat_pict, pts);

	// add frame to the rendered frames queue
	decodedFrames->Put(videoFrame);

}

void VideoPlayer::open(String ^videoLocation) {

	try {

		videoPlayer->open(marshal_as<std::string>(videoLocation));

		decodedFrames = gcnew DigitallyCreated::Utilities::Concurrency::LinkedListChannel<VideoFrame ^>();
		freeFrames = gcnew DigitallyCreated::Utilities::Concurrency::LinkedListChannel<VideoFrame ^>();

		for(int i = 0; i < nrFramesInBuffer; i++) {

			if(frameData[i] != nullptr) {

				delete frameData[i];
			}

			frameData[i] = gcnew VideoFrame(device, videoPlayer->getWidth(),
				videoPlayer->getHeight(),
				Format::A8R8G8B8);
		
			freeFrames->Put(frameData[i]);			
		}

	} catch (Exception ^) {

		close();
	}

}
int VideoPlayer::decodeFrame() {

	return(videoPlayer->decode(VideoDecoder::DECODE_VIDEO, 
		VideoDecoder::SKIP_AUDIO, 1));
}

void VideoPlayer::close() {

	videoPlayer->close();
}

}

