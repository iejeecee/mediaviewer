// FFMPEG Video Player tutorial http://dranger.com/ffmpeg/tutorial05.html
#include "stdafx.h"
#include <msclr\marshal_cppstd.h>

#include "VideoPlayer.h"

namespace VideoLib {

using namespace msclr::interop;
using namespace System::Runtime::InteropServices;

VideoPlayer::VideoPlayer() {

	videoDecoder = new VideoDecoder();

	frameQueue = gcnew VideoLib::FrameQueue(videoDecoder);
	videoLocation = "";

	isFinalPacketAdded = false;

}

VideoPlayer::~VideoPlayer() {

	delete videoDecoder;
	delete frameQueue;

	if(gch.IsAllocated) {

		gch.Free();
	}

}


void VideoPlayer::open(String ^videoLocation, DecodedVideoFormat videoFormat) {

	try {

		this->videoLocation = videoLocation;

		//

		videoDecoder->open(videoLocation);

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

		int channelLayout;

		switch(videoDecoder->getAudioNrChannels()) {
		case 1:
			{
				channelLayout = AV_CH_LAYOUT_MONO;
				break;
			}
		default: 
			{
				channelLayout = AV_CH_LAYOUT_STEREO;
				break;
			}
		}
		
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

VideoPlayer::DemuxPacketsResult VideoPlayer::demuxPacket() {

	if(videoDecoder->isClosed()) return(DemuxPacketsResult::STOPPED);

	Packet ^packet;
	bool success = frameQueue->getFreePacket(packet);
	if(success == false) {

		return(DemuxPacketsResult::STOPPED);
	}

	int read = av_read_frame(videoDecoder->getFormatContext(), packet->AVLibPacketData);
	if(read < 0) {

		// end of the video
		//Video::writeToLog(AV_LOG_INFO, "end of stream reached");
		frameQueue->addFreePacket(packet);

		if(isFinalPacketAdded == false) {

			frameQueue->addAudioPacket(Packet::finalPacket);
			frameQueue->addVideoPacket(Packet::finalPacket);
			isFinalPacketAdded = true;
		}
		return(DemuxPacketsResult::LAST_PACKET);

	} else {

		isFinalPacketAdded = false;
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

	return(DemuxPacketsResult::SUCCESS);

}

bool VideoPlayer::seek(double posSeconds) {

	return(videoDecoder->seek(posSeconds));
}

void VideoPlayer::close() {

	videoDecoder->close();
	frameQueue->release();
}

void VideoPlayer::setLogCallback(LogCallbackDelegate ^logCallback, bool enableLibAVLogging,
								 LogLevel level)
{

	if(gch.IsAllocated) {

		gch.Free();
	}

	gch = GCHandle::Alloc(logCallback);

	IntPtr voidPtr = Marshal::GetFunctionPointerForDelegate(logCallback);
		
	LOG_CALLBACK nativeLogCallback = static_cast<LOG_CALLBACK>(voidPtr.ToPointer());

	Video::setLogCallback(nativeLogCallback);

	if(enableLibAVLogging == true) {

		Video::enableLibAVLogging((int)level);

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