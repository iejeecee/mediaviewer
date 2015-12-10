#pragma once
#include "VideoDecoder.h"
#include "VideoEncoder.h"
#include "FilterGraph.h"
#include "BitStreamFilter.h"

using namespace MediaViewer::Infrastructure::Video::TranscodeOptions;
using namespace System::Collections::Generic;
using namespace msclr::interop;
using namespace MediaViewer::Infrastructure::Utils;
using namespace MediaViewer::Infrastructure::Logging;


namespace VideoLib {

// Muxes seperate video and audio input streams when calling readframe
class MuxedAVInputStream {

	VideoDecoder *videoDecoder;
	VideoDecoder *audioDecoder;
	
	int audioStreamOffset;

	double videoDts, audioDts;
	AVPacket videoPacket,audioPacket;

	bool bHasAudioDecoder;

public:

	MuxedAVInputStream()
	{
		this->videoDecoder = new VideoDecoder();	 	
		this->audioDecoder = new VideoDecoder();		

		audioStreamOffset = 0;
		videoPacket.data = NULL;
		videoPacket.size = 0;
		audioPacket.data = NULL;
		audioPacket.size = 0;

		bHasAudioDecoder = false;
	}

	~MuxedAVInputStream()
	{
		close();

		delete videoDecoder;
		videoDecoder = NULL;
		delete audioDecoder;
		audioDecoder = NULL;
		
	}

	VideoDecoder *getVideoDecoder() const
	{
		return(videoDecoder);
	}

	VideoDecoder *getAudioDecoder() const
	{
		if(bHasAudioDecoder) 
		{
			return(audioDecoder);

		} else {

			return(videoDecoder);	
		}
	}

	bool hasAudioDecoder() const
	{
		return(bHasAudioDecoder);
	}

	void open(String ^inputVideoLocation,  System::Threading::CancellationToken token, 
		String ^inputAudioLocation = nullptr)
	{					
		bHasAudioDecoder = false;

		videoDecoder->open(inputVideoLocation, token);

		audioStreamOffset = videoDecoder->getNrStreams();

		if(inputAudioLocation != nullptr)
		{			
			audioDecoder->open(inputAudioLocation, token);		
			bHasAudioDecoder = true;
		} 

	}

	bool seek(double positionSeconds) {

		bool result = videoDecoder->seek(positionSeconds);
		if(result == false) return(result);

		if(bHasAudioDecoder) 
		{
			// Seek the audio stream to the current position of the video stream.
			// Seeking in the video stream can be non-exact due to keyframes.
			readVideoFrame(videoPacket);

			result = audioDecoder->seek(videoDts);
			if(result == false) return(result);

			readAudioFrame(audioPacket);
		}

		return true;
		
	}

	void close() {

		videoDecoder->close();

		if(bHasAudioDecoder) 
		{
			audioDecoder->close();
		}

		if(videoPacket.data != NULL) {

			av_free_packet(&videoPacket);
			videoPacket.data = NULL;
			videoPacket.size = 0;
		}

		if(audioPacket.data != NULL) {

			av_free_packet(&audioPacket);
			audioPacket.data = NULL;
			audioPacket.size = 0;
		}
	}

	AVCodecContext *getAudioCodecContext() const 
	{
		if(bHasAudioDecoder) {

			return audioDecoder->getAudioCodecContext();

		} else {

			return videoDecoder->getAudioCodecContext();
		}
	}

	AVCodecContext *getVideoCodecContext() const
	{
		return videoDecoder->getVideoCodecContext();
	}

	unsigned int getNrStreams() const
	{
		unsigned int nrStreams = (unsigned int)videoDecoder->stream.size();

		if(bHasAudioDecoder) {

			nrStreams += (unsigned int)audioDecoder->stream.size();
		}

		return nrStreams;
	}

	int getBestVideoStreamIndex() const {

		return(videoDecoder->getVideoStreamIndex());
	}

	int getBestAudioStreamIndex() const {

		if(bHasAudioDecoder) {

			return(audioDecoder->getAudioStreamIndex() + audioStreamOffset);

		} else {

			return videoDecoder->getAudioStreamIndex();
		}
	}

	VideoLib::Stream *getStream(int i) const
	{		
		if(i < audioStreamOffset) {

			return(videoDecoder->stream[i]);

		} else {

			return(audioDecoder->stream[i - audioStreamOffset]);
		}

	}

	double getDurationSeconds() const {

		double durationSeconds = videoDecoder->getDurationSeconds();

		if(bHasAudioDecoder) {

			durationSeconds = Math::Min(videoDecoder->getDurationSeconds(), audioDecoder->getDurationSeconds());
		}

		return durationSeconds;
	}

	VideoDecoder::ReadFrameResult readFrame(AVPacket **packet) {

		VideoDecoder::ReadFrameResult result;

		if(bHasAudioDecoder == false) {

			result = videoDecoder->readFrame(&videoPacket);

			*packet = &videoPacket;

			return(result);
		}

		// mux video and audio packets				

		if(videoPacket.data == NULL && audioPacket.data == NULL) {

			if((result = readVideoFrame(videoPacket)) != VideoDecoder::ReadFrameResult::OK) return result;
			if((result = readAudioFrame(audioPacket)) != VideoDecoder::ReadFrameResult::OK) return result;

			audioPacket.stream_index += audioStreamOffset;

		} else {

			if(videoDts <= audioDts) {

				av_free_packet(&videoPacket);

				if((result = readVideoFrame(videoPacket)) != VideoDecoder::ReadFrameResult::OK) return result;
				
			} else {

				av_free_packet(&audioPacket);

				if((result = readAudioFrame(audioPacket)) != VideoDecoder::ReadFrameResult::OK) return result;

				audioPacket.stream_index += audioStreamOffset;
			}
			
		}
				
		if(videoDts <= audioDts) {

			*packet = &videoPacket;
			
		} else {

			*packet = &audioPacket;			
		}
		
		return(result);
	}

	VideoDecoder::ReadFrameResult readVideoFrame(AVPacket &packet) {
				 
		VideoDecoder::ReadFrameResult result = videoDecoder->readFrame(&packet);

		if(result != VideoDecoder::ReadFrameResult::OK) return result;

		if(packet.dts != AV_NOPTS_VALUE) {

			videoDts = videoDecoder->stream[videoDecoder->getVideoStreamIndex()]->getTimeSeconds(packet.dts);

		} else {

			throw gcnew VideoLib::VideoLibException("Input video stream does not contain valid decoding timestamps");
		}

		return(result);

	}

	VideoDecoder::ReadFrameResult readAudioFrame(AVPacket &packet) {
			
		VideoDecoder::ReadFrameResult result = audioDecoder->readFrame(&packet);

		if(result != VideoDecoder::ReadFrameResult::OK) return result;

		if(packet.dts != AV_NOPTS_VALUE) {

			audioDts = audioDecoder->stream[audioDecoder->getAudioStreamIndex()]->getTimeSeconds(packet.dts);

		} else {

			throw gcnew VideoLib::VideoLibException("Input audio stream does not contain valid decoding timestamps");
		}

		return(result);
	}

	
	

};

}