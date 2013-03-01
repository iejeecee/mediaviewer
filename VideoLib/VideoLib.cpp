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



VideoPlayer::VideoPlayer(Device ^device) {

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

	videoClock = 0;
	audioClock = 0;
	
	frameQueue = gcnew VideoLib::FrameQueue();

}

VideoPlayer::~VideoPlayer() {

	delete videoPlayer;
	gch.Free();
}

// calculate a new pts for a videoframe if it's pts is missing
double VideoPlayer::synchronizeVideo(int repeatFrame, __int64 dts) {

	double pts;

	if(dts != AV_NOPTS_VALUE) {

		// convert pts to seconds
		pts = dts * av_q2d(videoPlayer->getVideoStream()->time_base);
		// set clock to current pts;
		videoClock = pts;

	} else {

		// if we aren't given a pts, set it to the clock 
		pts = videoClock;
	}

	// update the video clock to the pts of the next frame
	double frameDelay = av_q2d(videoPlayer->getVideoStream()->time_base);
	// if we are repeating a frame, adjust clock accordingly 
	frameDelay += repeatFrame * (frameDelay * 0.5);
	videoClock += frameDelay;

	return(pts);
}

double VideoPlayer::synchronizeAudio(int sizeBytes, __int64 dts) {

	double pts;

	if(dts != AV_NOPTS_VALUE) {

		// convert pts to seconds
		pts = dts * av_q2d(videoPlayer->getAudioStream()->time_base);
		// set clock to current pts;
		audioClock = pts;

	} else {

		// if we aren't given a pts, set it to the clock 
		pts = audioClock;
		// calculate next pts in seconds
		audioClock += sizeBytes / double(SamplesPerSecond * BytesPerSample * NrChannels);
	}
	
	return(pts);
}


void VideoPlayer::decodedFrameCallback(void *data, AVPacket *packet, 
									   AVFrame *frame, Video::FrameType type)
{

	if(type == Video::VIDEO) {

		// get a frame from the free frames queue
		VideoFrame ^videoFrame; 

		bool success = frameQueue->getFreeVideoFrame(videoFrame);
		if(success == false) return;

		Byte *y = frame->data[0];
		Byte *u = frame->data[1];
		Byte *v = frame->data[2];

		// convert the unmanaged frame data to managed data
		videoFrame->copyFrameData(y, v, u);

		// calculate presentation timestamp (pts)
		videoFrame->Pts = synchronizeVideo(frame->repeat_pict, packet->dts);
		
		// add frame to the decoded frames queue
		frameQueue->enqueueDecodedVideoFrame(videoFrame);

	} else {

		AudioFrame ^audioFrame; 

		bool success = frameQueue->getFreeAudioFrame(audioFrame);
		if(success == false) return;

		Byte *data = frame->data[0];

		int length = av_samples_get_buffer_size(NULL, NrChannels, frame->nb_samples,
			(AVSampleFormat)frame->format, 1);

		audioFrame->copyFrameData(data, length);

		// calculate presentation timestamp (pts)
		audioFrame->Pts = synchronizeAudio(length, packet->dts);

		// add frame to the decoded frames queue
		frameQueue->enqueueDecodedAudioFrame(audioFrame);
	}

}


void VideoPlayer::open(String ^videoLocation) {

	try {

		videoPlayer->open(marshal_as<std::string>(videoLocation), PIX_FMT_YUV420P);

		frameQueue->initialize(device, Width, Height, AVCODEC_MAX_AUDIO_FRAME_SIZE * 2);

	} catch (Exception ^) {

		close();
	}

}
int VideoPlayer::decodeFrame(DecodeMode mode) {

	VideoDecoder::VideoDecodeMode videoDecodeMode = VideoDecoder::DECODE_VIDEO;
	VideoDecoder::AudioDecodeMode audioDecodeMode = VideoDecoder::DECODE_AUDIO;

	if(mode == DecodeMode::DECODE_VIDEO_ONLY) {

		audioDecodeMode = VideoDecoder::SKIP_AUDIO;

	} else if(mode == DecodeMode::DECODE_AUDIO_ONLY) {

		videoDecodeMode = VideoDecoder::SKIP_VIDEO;
	}

	return(videoPlayer->decode(videoDecodeMode, audioDecodeMode, 1));
}

int VideoPlayer::seek(double posSeconds) {

	return(videoPlayer->seek(posSeconds));
}

void VideoPlayer::close() {

	videoPlayer->close();
	frameQueue->dispose();
}

}

