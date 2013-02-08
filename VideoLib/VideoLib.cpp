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
		pixelFormat =  marshal_as<String ^>(frameGrabber->pixelFormat);

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



VideoPlayer::VideoPlayer(Device ^device, Format pixelFormat) {

	this->device = device;
	this->pixelFormat = pixelFormat;

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

	videoClock = 0;
	
	packetQueue = gcnew PacketQueue(nrFramesInBuffer);

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

void VideoPlayer::fillBuffer(Surface ^frame, BYTE* pY, BYTE* pV, BYTE* pU)
{
	Drawing::Rectangle rect = Drawing::Rectangle(0, 0, Width, Height);

	// copy raw frame data to bitmap
	int pitch;
	GraphicsStream ^stream = frame->LockRectangle(rect, LockFlags::None, pitch);

	BYTE* pict = (BYTE*)stream->InternalDataPointer;
	BYTE* Y = pY;
	BYTE* V = pV;
	BYTE* U = pU;
/*
	switch(pixelFormat)
	{
	case D3DFMT_YV12:
*/
	if(Width == pitch) {

		int ySizeBytes = Width * Height;
		int vSizeBytes = (Width * Height) / 4;
		int uSizeBytes = (Width * Height) / 4;

		memcpy(pict, Y, ySizeBytes);
		memcpy(pict + ySizeBytes, V, vSizeBytes);
		memcpy(pict + ySizeBytes + vSizeBytes, U, uSizeBytes);

	} else {

		for (int y = 0 ; y < Height ; y++)
		{
			memcpy(pict, Y, Width);
			pict += pitch;
			Y += Width;
		}
		for (int y = 0 ; y < Height / 2 ; y++)
		{
			memcpy(pict, V, Width / 2);
			pict += pitch / 2;
			V += Width / 2;
		}
		for (int y = 0 ; y < Height / 2; y++)
		{
			memcpy(pict, U, Width / 2);
			pict += pitch / 2;
			U += Width / 2;
		}
	}
/*		
		break;

	case D3DFMT_NV12:

		for (int y = 0 ; y < newHeight ; y++)
		{
			memcpy(pict, Y, Width);
			pict += pitch;
			Y += Width;
		}
		for (int y = 0 ; y < newHeight / 2 ; y++)
		{
			memcpy(pict, V, Width);
			pict += pitch;
			V += Width;
		}
		break;

	case D3DFMT_YUY2:
	case D3DFMT_UYVY:
	case D3DFMT_R5G6B5:
	case D3DFMT_X1R5G5B5:
	case D3DFMT_A8R8G8B8:
	case D3DFMT_X8R8G8B8:

		memcpy(pict, Y, pitch * newHeight);

		break;
	}
*/
	frame->UnlockRectangle();

}

void VideoPlayer::decodedFrameCallback(void *data, AVPacket *packet, 
									   AVFrame *frame, Video::FrameType type)
{

	// get a frame from the free frames queue
	VideoFrame ^videoFrame; 
	
	bool success = packetQueue->getFreeFrame(videoFrame);
	if(success == false) return;

	Byte *y = frame->data[0];
	Byte *u = frame->data[1];
	Byte *v = frame->data[2];

	// convert the unmanaged frame data to managed data
	fillBuffer(videoFrame->Frame, y, v, u);

	// calculate presentation timestamp (pts)
	double pts = 0;

    if(packet->dts != AV_NOPTS_VALUE) {

      pts = packet->dts;

    } else {

      pts = 0;
    }

	// convert pts to seconds
	pts *= av_q2d(videoPlayer->getVideoStream()->time_base);

	// calculate a pts if no pts is available (pts == 0)
	videoFrame->Pts = synchronizeVideo(frame->repeat_pict, pts);

	// add frame to the decoded frames queue
	packetQueue->queueDecodedFrame(videoFrame);

}


void VideoPlayer::open(String ^videoLocation) {

	try {

		videoPlayer->open(marshal_as<std::string>(videoLocation), PIX_FMT_YUV420P);

		packetQueue->initialize(device, videoPlayer->getWidth(),
			videoPlayer->getHeight(), pixelFormat);

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
	packetQueue->dispose();
}

}

