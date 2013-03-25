// This is the main DLL file.
// FFMPEG Video Player tutorial http://dranger.com/ffmpeg/tutorial05.html

#include "stdafx.h"
#include <msclr\marshal_cppstd.h>

#include "VideoLib.h"
#include "VideoFrameGrabber.h"

using namespace msclr::interop;
using namespace System::Runtime::InteropServices;
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
			int captureInterval, int nrThumbs, double startOffset) 
{

	thumbs->Clear();

	frameGrabber->grab(maxThumbWidth, maxThumbHeight,
			captureInterval, nrThumbs, startOffset);

	return(thumbs);
}



VideoPlayer::VideoPlayer() {

	videoDecoder = new VideoDecoder();
	

	frameQueue = gcnew VideoLib::FrameQueue(videoDecoder);
	videoLocation = "";

}

VideoPlayer::~VideoPlayer() {

	delete videoDecoder;
}


void VideoPlayer::open(String ^videoLocation) {

	try {

		this->videoLocation = videoLocation;

		//

		videoDecoder->open(marshal_as<std::string>(videoLocation));
		videoDecoder->initImageConverter(PIX_FMT_YUV420P,
			videoDecoder->getWidth(), videoDecoder->getHeight(), VideoDecoder::X);

		frameQueue->initialize();

	} catch (Exception ^) {

		close();
		throw;
	}

}
bool VideoPlayer::demuxPacket() {

	if(videoDecoder->isClosed()) return(false);

	Packet ^packet;
	bool success = frameQueue->getFreePacket(packet);
	if(success == false) {

		return(false);
	}

	int read = av_read_frame(videoDecoder->getFormatContext(), packet->AVLibPacketData);
	if(read < 0) {

		// end of the video
		frameQueue->addFreePacket(packet);
		frameQueue->close();
		return(false);

	}

	if(packet->AVLibPacketData->stream_index == videoDecoder->getVideoStreamIndex()) {

		// video packet
		frameQueue->addVideoPacket(packet);

	} else if(packet->AVLibPacketData->stream_index == videoDecoder->getAudioStreamIndex()) {

		// audio packet
		frameQueue->addAudioPacket(packet);

	} else {

		// unknown packet
		frameQueue->addFreePacket(packet);
	}

	return(true);

}

bool VideoPlayer::seek(double posSeconds) {

	return(videoDecoder->seek(posSeconds));
}

void VideoPlayer::close() {

	videoDecoder->close();
	frameQueue->dispose();
}

}

