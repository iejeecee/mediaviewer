// FFMPEG Video Player tutorial http://dranger.com/ffmpeg/tutorial05.html
#include "stdafx.h"
#include <msclr\marshal_cppstd.h>
#include "Video\VideoDecoderFactory.h"

#include "VideoPlayer.h"

namespace VideoLib {

using namespace msclr::interop;
using namespace System::Runtime::InteropServices;

VideoPlayer::VideoPlayer() {

	videoDecoder = NULL;
	
	frameQueue = gcnew VideoLib::FrameQueue();
	
	isFinalPacketAdded = gcnew array<bool>(2) {false,false};
	isSimulateLag = gcnew array<bool>(2) {false,false};

}

VideoPlayer::~VideoPlayer() {
		
	delete frameQueue;

	this->!VideoPlayer();
}

VideoPlayer::!VideoPlayer() {

	if(videoDecoder != NULL) {

		delete videoDecoder;
		videoDecoder = NULL;
	}
	
}

void VideoPlayer::open(OpenVideoArgs ^args,  OutputPixelFormat videoFormat, System::Threading::CancellationToken^ token)
{
	try {		
				
		videoDecoder = VideoDecoderFactory::create(args, videoDecoder);
					
		videoDecoder->open(args, token);

		if(videoDecoder->hasVideo()) {

			AVPixelFormat outputPixelFormat = AV_PIX_FMT_YUV420P;

			switch(videoFormat) 
			{
			case OutputPixelFormat::YUV420P:
				{
					outputPixelFormat = AV_PIX_FMT_YUV420P;
					break;
				}
			case OutputPixelFormat::ARGB:
				{
					outputPixelFormat = AV_PIX_FMT_ARGB;
					break;
				}
			case OutputPixelFormat::RGBA:
				{
					outputPixelFormat = AV_PIX_FMT_RGBA;
					break;
				}
			case OutputPixelFormat::ABGR:
				{
					outputPixelFormat = AV_PIX_FMT_ABGR;
					break;
				}
			case OutputPixelFormat::BGRA:
				{
					outputPixelFormat = AV_PIX_FMT_BGRA;
					break;
				}
			}

			videoDecoder->setVideoOutputFormat(outputPixelFormat,
				videoDecoder->getWidth(), videoDecoder->getHeight(), VideoDecoder::X);

		
		}
	

		int channelLayout;
		
		switch(videoDecoder->getAudioNrChannels()) 
		{
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

		switch(videoDecoder->getAudioBytesPerSample()) 
		{
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

		videoDecoder->setAudioOutputFormat(videoDecoder->getAudioSamplesPerSecond(),
			channelLayout, format);

		frameQueue->initialize(videoDecoder);

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

	VideoDecoder::ReadFrameResult result = videoDecoder->readFrame(packet->AVLibPacketData);
	if(result == VideoDecoder::ReadFrameResult::END_OF_STREAM) {

		// end of the video	
		frameQueue->addFreePacket(packet);

		if(isFinalPacketAdded[0] == false) {

			if(videoDecoder->hasVideo())
			{
				frameQueue->addVideoPacket(Packet::finalPacket);				
			}

			if(videoDecoder->hasAudio())
			{
				frameQueue->addAudioPacket(Packet::finalPacket);
			}

			isFinalPacketAdded[0] = true;
		}

		return(DemuxPacketsResult::LAST_PACKET);

	} else {

		isFinalPacketAdded[0] = false;
	}

	if(packet->AVLibPacketData->flags & AV_PKT_FLAG_CORRUPT) {

		VideoInit::writeToLog(AV_LOG_WARNING, "corrupt packet");
	}

	if(packet->AVLibPacketData->stream_index == videoDecoder->getVideoStreamIndex()) {

		if(result == VideoDecoder::ReadFrameResult::READ_ERROR) {

			// discard bad packet(s)
			frameQueue->NrVideoPacketReadErrors++;
			frameQueue->addFreePacket(packet);

		} else {

			// video packet
			frameQueue->addVideoPacket(packet);

			if(videoDecoder->getVideoStream()->disposition & AV_DISPOSITION_ATTACHED_PIC) {

				// there is just a single video packet in this stream, a attached picture		
				frameQueue->addVideoPacket(Packet::finalPacket);	
			}
		}

	} else if(packet->AVLibPacketData->stream_index == videoDecoder->getAudioStreamIndex()) {

		if(result == VideoDecoder::ReadFrameResult::READ_ERROR) {

			// discard bad packet(s)
			frameQueue->NrAudioPacketReadErrors++;
			frameQueue->addFreePacket(packet);

		} else {

			// audio packet
			frameQueue->addAudioPacket(packet);
		}

	} else {

		// unknown packet
		frameQueue->addFreePacket(packet);
	}
	
	return(DemuxPacketsResult::SUCCESS);
}

bool VideoPlayer::seek(double posSeconds, SeekKeyframeMode mode) {

	int flags = 0;

	if(mode == SeekKeyframeMode::SEEK_BACKWARDS) {

		flags = AVSEEK_FLAG_BACKWARD;
	}

	return videoDecoder->seek(posSeconds, flags);
}

void VideoPlayer::close() {

	videoDecoder->close();

	frameQueue->release();
}


void VideoPlayer::enableLibAVLogging(LogLevel level)
{
	VideoInit::enableLibAVLogging((int)level);
}

void VideoPlayer::disableLibAVLogging()
{
	VideoInit::disableLibAVLogging();
}

void VideoPlayer::setLogCallback(LogCallbackDelegate ^logCallback)
{
	if(gch.IsAllocated) {

		gch.Free();
	}

	gch = GCHandle::Alloc(logCallback);

	IntPtr voidPtr = Marshal::GetFunctionPointerForDelegate(logCallback);
		
	LOG_CALLBACK nativeLogCallback = static_cast<LOG_CALLBACK>(voidPtr.ToPointer());

	VideoInit::setLogCallback(nativeLogCallback);
	
}

int VideoPlayer::getAvFormatVersion() {

	return(VideoInit::getAvFormatVersion());
}

String ^VideoPlayer::getAvFormatBuildInfo() {

	std::string info = VideoInit::getBuildConfig();

	return(marshal_as<String ^>(info));

}

}