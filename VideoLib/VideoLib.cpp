// This is the main DLL file.
// FFMPEG Video Player tutorial http://dranger.com/ffmpeg/tutorial05.html

#include "stdafx.h"
#include <msclr\marshal_cppstd.h>

#include "VideoLib.h"
#include "VideoFrameGrabber.h"


namespace VideoLib {

using namespace msclr::interop;
using namespace System::Runtime::InteropServices;


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

	thumbs = gcnew List<BitmapSource ^>();
	
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
	

	int sizeBytes = frameGrabber->getThumbHeight() * frame->linesize[0];

	BitmapSource ^bitmap = BitmapSource::Create(
		frameGrabber->getThumbWidth(), frameGrabber->getThumbHeight(),
		1 / 72.0,  1 / 72.0,
		System::Windows::Media::PixelFormats::Bgr24,
		nullptr,
		IntPtr(frame->data[0]),
		sizeBytes,
		frame->linesize[0]);
		
/*	

	BitmapFrame(frameGrabber->getThumbWidth(),
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
*/
	thumbs->Add(bitmap);

}


List<BitmapSource ^> ^VideoPreview::grabThumbnails(int maxThumbWidth, int maxThumbHeight, 
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

	if(gch.IsAllocated) {

		gch.Free();
	}

}


void VideoPlayer::open(String ^videoLocation, DecodedVideoFormat videoFormat) {

	try {

		this->videoLocation = videoLocation;

		//

		videoDecoder->open(marshal_as<std::string>(videoLocation));

		AVPixelFormat convertToFormat = PIX_FMT_YUV420P;

		switch(videoFormat) {

		case DecodedVideoFormat::YUV420P:
			{
				convertToFormat = PIX_FMT_YUV420P;
				break;
			}
		case DecodedVideoFormat::ARGB:
			{
				convertToFormat = PIX_FMT_ARGB;
				break;
			}
		case DecodedVideoFormat::RGBA:
			{
				convertToFormat = PIX_FMT_RGBA;
				break;
			}
		case DecodedVideoFormat::ABGR:
			{
				convertToFormat = PIX_FMT_ABGR;
				break;
			}
		case DecodedVideoFormat::BGRA:
			{
				convertToFormat = PIX_FMT_BGRA;
				break;
			}
		}

		videoDecoder->initImageConverter(convertToFormat,
			videoDecoder->getWidth(), videoDecoder->getHeight(), VideoDecoder::X);

		int channelLayout = videoDecoder->getAudioNrChannels() == 1 ? 
			AV_CH_LAYOUT_MONO : AV_CH_LAYOUT_STEREO;

		AVSampleFormat format;

		switch(videoDecoder->getAudioBytesPerSample()) {

			case 1:
				{
					format = AV_SAMPLE_FMT_U8;
					break;
				}
			case 2:
				{
					format = AV_SAMPLE_FMT_S16;
					break;
				}
			case 4:
				{
					format = AV_SAMPLE_FMT_S32;
					break;
				}
			default:
				{
					format = AV_SAMPLE_FMT_NONE;
					break;
				}
		}

		videoDecoder->initAudioConverter(videoDecoder->getAudioSamplesPerSecond(),
			channelLayout, format);

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
		Video::writeToLog(AV_LOG_INFO, "end of stream reached");
		frameQueue->addFreePacket(packet);
		frameQueue->close();
		return(false);

	}

	if(packet->AVLibPacketData->flags & AV_PKT_FLAG_CORRUPT) {

		Video::writeToLog(AV_LOG_WARNING, "corrupt packet");
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

void VideoPlayer::setLogCallback(LogCallbackDelegate ^logCallback, bool enableLibAVLogging,
								 bool onlyLogImportant)
{

	if(gch.IsAllocated) {

		gch.Free();
	}

	gch = GCHandle::Alloc(logCallback);

	IntPtr voidPtr = Marshal::GetFunctionPointerForDelegate(logCallback);
		
	LOG_CALLBACK nativeLogCallback = static_cast<LOG_CALLBACK>(voidPtr.ToPointer());

	Video::setLogCallback(nativeLogCallback);

	if(enableLibAVLogging = true) {

		Video::enableLibAVLogging(onlyLogImportant);

	} else {

		Video::disableLibAVLogging();
	}

}

int VideoPlayer::getAvFormatVersion() {

	return(VideoInit::getAvFormatVersion());
}

String ^VideoPlayer::getAvFormatBuildInfo() {

	std::string info = VideoInit::getBuildConfig();

	return(marshal_as<String ^>(info));

}
}

