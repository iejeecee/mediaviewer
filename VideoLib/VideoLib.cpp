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

VideoPlayer::VideoPlayer() {

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

	frameData = gcnew array<Drawing::Bitmap ^>(nrFramesInBuffer);
	decodedFrames = gcnew DigitallyCreated::Utilities::Concurrency::LinkedListChannel<System::Drawing::Bitmap ^>();
}

VideoPlayer::~VideoPlayer() {

	delete videoPlayer;
	gch.Free();
}

void VideoPlayer::decodedFrameCallback(void *data, AVPacket *packet, 
									   AVFrame *frame, Video::FrameType type)
{

	// get a frame from the free frames queue
	Drawing::Bitmap ^bitmap = freeFrames->Take();		

	Drawing::Rectangle rect = 
		Drawing::Rectangle(0, 0, bitmap->Width, bitmap->Height);

	// copy raw frame data to bitmap
	Drawing::Imaging::BitmapData ^bmpData = bitmap->LockBits(rect, Drawing::Imaging::ImageLockMode::WriteOnly,
		Drawing::Imaging::PixelFormat::Format24bppRgb);

	IntPtr dest = bmpData->Scan0;

	int sizeBytes = bmpData->Height * bmpData->Stride;

	memcpy(dest.ToPointer(), frame->data[0], sizeBytes);

	bitmap->UnlockBits(bmpData);

	// add frame to the rendered frames queue
	decodedFrames->Put(bitmap);

	FrameDecoded(this, gcnew EventArgs());
}

void VideoPlayer::open(String ^videoLocation) {

	try {

		videoPlayer->open(marshal_as<std::string>(videoLocation));

		freeFrames = gcnew DigitallyCreated::Utilities::Concurrency::LinkedListChannel<System::Drawing::Bitmap ^>();

		for(int i = 0; i < nrFramesInBuffer; i++) {

			frameData[i] = gcnew Drawing::Bitmap(videoPlayer->getWidth(),
				videoPlayer->getHeight(),
				Drawing::Imaging::PixelFormat::Format24bppRgb);
		
			freeFrames->Put(frameData[i]);			
		}

	} catch (Exception ^) {

		close();
	}

}
void VideoPlayer::play() {

	videoPlayer->play();
}
void VideoPlayer::close() {

	videoPlayer->close();
}

}

