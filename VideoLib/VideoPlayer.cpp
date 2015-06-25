// FFMPEG Video Player tutorial http://dranger.com/ffmpeg/tutorial05.html
#include "stdafx.h"
#include <msclr\marshal_cppstd.h>

#include "VideoPlayer.h"

#define DECODER(x) decoder->operator[](x)
#define VIDEODECODER DECODER(0)
#define AUDIODECODER DECODER(decoder->size() - 1)

namespace VideoLib {

using namespace msclr::interop;
using namespace System::Runtime::InteropServices;

VideoPlayer::VideoPlayer() {

	decoder = new std::vector<VideoDecoder *>();
	
	videoDecoder = new VideoDecoder();
	audioDecoder = new VideoDecoder();

	decoder->push_back(videoDecoder);
	
	frameQueue = gcnew VideoLib::FrameQueue();
	
	isFinalPacketAdded = gcnew array<bool>(2) {false,false};


}

VideoPlayer::~VideoPlayer() {
		
	delete videoDecoder;
	delete audioDecoder;
	delete decoder;

	delete frameQueue;

	if(gch.IsAllocated) {

		gch.Free();
	}

}



void VideoPlayer::open(String ^videoLocation, OutputPixelFormat videoFormat, String ^inputFormatName, 
					   System::Threading::CancellationToken ^token) {

	open(videoLocation,videoFormat, inputFormatName, nullptr, nullptr, token);

}

void VideoPlayer::open(String ^videoLocation, OutputPixelFormat videoFormat, String ^videoFormatName,
			String ^audioLocation, String ^audioFormatName,  System::Threading::CancellationToken ^token)
{
	try {		
	
		decoder->clear();
		decoder->push_back(videoDecoder);

		VIDEODECODER->open(videoLocation, token, videoFormatName);

		AVPixelFormat outputPixelFormat = PIX_FMT_YUV420P;

		switch(videoFormat) 
		{
			case OutputPixelFormat::YUV420P:
				{
					outputPixelFormat = PIX_FMT_YUV420P;
					break;
				}
			case OutputPixelFormat::ARGB:
				{
					outputPixelFormat = PIX_FMT_ARGB;
					break;
				}
			case OutputPixelFormat::RGBA:
				{
					outputPixelFormat = PIX_FMT_RGBA;
					break;
				}
			case OutputPixelFormat::ABGR:
				{
					outputPixelFormat = PIX_FMT_ABGR;
					break;
				}
			case OutputPixelFormat::BGRA:
				{
					outputPixelFormat = PIX_FMT_BGRA;
					break;
				}
		}

		VIDEODECODER->setVideoOutputFormat(outputPixelFormat,
			VIDEODECODER->getWidth(), VIDEODECODER->getHeight(), VideoDecoder::X);

		if(audioLocation != nullptr) {

			decoder->push_back(audioDecoder);
			AUDIODECODER->open(audioLocation, token, audioFormatName);			
		}

		int channelLayout;
		
		switch(AUDIODECODER->getAudioNrChannels()) 
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

		switch(AUDIODECODER->getAudioBytesPerSample()) 
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

		AUDIODECODER->setAudioOutputFormat(AUDIODECODER->getAudioSamplesPerSecond(),
			channelLayout, format);

		frameQueue->initialize(VIDEODECODER, AUDIODECODER);

	} catch (Exception ^) {

		close();
		throw;
	}
}

VideoPlayer::DemuxPacketsResult VideoPlayer::demuxPacketInterleaved() {

	if(videoDecoder->isClosed()) return(DemuxPacketsResult::STOPPED);
	
	Packet ^packet;
	bool success = frameQueue->getFreePacket(packet);
	if(success == false) {

		return(DemuxPacketsResult::STOPPED);
	}

	success = videoDecoder->readFrame(packet->AVLibPacketData);
	if(success == false) {

		// end of the video
		//Video::writeToLog(AV_LOG_INFO, "end of stream reached");
		frameQueue->addFreePacket(packet);

		if(isFinalPacketAdded[0] == false) {

			frameQueue->addAudioPacket(Packet::finalPacket);
			frameQueue->addVideoPacket(Packet::finalPacket);
			isFinalPacketAdded[0] = true;
		}

		return(DemuxPacketsResult::LAST_PACKET);

	} else {

		isFinalPacketAdded[0] = false;
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


VideoPlayer::DemuxPacketsResult VideoPlayer::demuxPacketFromStream(int i) {

	if(DECODER(i)->isClosed()) return(DemuxPacketsResult::STOPPED);
		
	Packet ^packet;
	bool success = frameQueue->getFreePacket(packet);
	if(success == false) {

		return(DemuxPacketsResult::STOPPED);
	}

	success = DECODER(i)->readFrame(packet->AVLibPacketData);
	if(success == false) {

		// end of the video		
		frameQueue->addFreePacket(packet);

		if(isFinalPacketAdded[i] == false) {

			if(i == 0) frameQueue->addVideoPacket(Packet::finalPacket);
			else frameQueue->addAudioPacket(Packet::finalPacket);
								
			isFinalPacketAdded[i] = true;
		} 
						
		return(DemuxPacketsResult::LAST_PACKET);
		
	} else {

		isFinalPacketAdded[i] = false;	
	}

	if(packet->AVLibPacketData->flags & AV_PKT_FLAG_CORRUPT) {

		Video::writeToLog(AV_LOG_WARNING, "corrupt packet");
	}

	if(packet->AVLibPacketData->stream_index == 0) {

		// video packet
		if(i == 0) frameQueue->addVideoPacket(packet);
		else frameQueue->addAudioPacket(packet);

	} else {

		// unknown packet
		frameQueue->addFreePacket(packet);
	}
	
	return(DemuxPacketsResult::SUCCESS);
}

bool VideoPlayer::seek(double posSeconds) {

	bool result = true;

	for(int i = 0; i < decoder->size(); i++) {

		result = DECODER(i)->seek(posSeconds) && result;
	}	

	return(result);
}

void VideoPlayer::close() {

	for(int i = 0; i < decoder->size(); i++) {

		DECODER(i)->close();
	}

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