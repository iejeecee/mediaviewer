#pragma once

#include "ThreadSafeQueue.h"
#include "VideoFrame.h"
#include "AudioFrame.h"
#include "Packet.h"
#include "VideoDecoder.h"

using namespace System::Collections::Generic;
using namespace System::Threading;
using namespace System::Diagnostics;

namespace VideoLib {

	public ref class FrameQueue
	{
	private:

		VideoDecoder *videoDecoder;

		VideoFrame ^videoFrame, ^convertedVideoFrame;
		AudioFrame ^audioFrame;

		static const int maxVideoPackets = 300;
		static const int maxAudioPackets = 300;

		ThreadSafeQueue<Packet ^> ^freePackets;
		ThreadSafeQueue<Packet ^> ^videoPackets;
		ThreadSafeQueue<Packet ^> ^audioPackets;

		AutoResetEvent ^decodingVideoStopped;
		AutoResetEvent ^decodingAudioStopped;
		AutoResetEvent ^demuxingStopped;

		bool pauseDecoding;
		array<WaitHandle ^> ^decodingPaused;
		array<WaitHandle ^> ^decodingContinue;
		
		array<Packet ^> ^packetData;

		double videoClock;
		double audioClock;

		double synchronizeVideo(int repeatFrame, __int64 dts) {

			double pts;

			if(dts != AV_NOPTS_VALUE) {

				// convert pts to seconds
				pts = dts * av_q2d(videoDecoder->getVideoStream()->time_base);
				// set clock to current pts;
				videoClock = pts;

			} else {

				// if we aren't given a pts, set it to the clock 
				pts = videoClock;
			}

			// update the video clock to the pts of the next frame
			double frameDelay = av_q2d(videoDecoder->getVideoStream()->time_base);
			// if we are repeating a frame, adjust clock accordingly 
			frameDelay += repeatFrame * (frameDelay * 0.5);
			videoClock += frameDelay;

			return(pts);
		}

		double synchronizeAudio(int sizeBytes, __int64 dts) {

			double pts;

			if(dts != AV_NOPTS_VALUE) {

				// convert pts to seconds
				pts = dts * av_q2d(videoDecoder->getAudioStream()->time_base);
				// set clock to current pts;
				audioClock = pts;

			} else {

				// if we aren't given a pts, set it to the clock 
				pts = audioClock;
				// calculate next pts in seconds
				audioClock += sizeBytes / double(videoDecoder->getAudioBytesPerSecond());
			}

			return(pts);
		}

	public:

		FrameQueue(VideoDecoder *videoDecoder) {

			this->videoDecoder = videoDecoder;

			videoFrame = nullptr;
			audioFrame = nullptr;

			packetData = gcnew array<Packet ^>(maxVideoPackets + maxAudioPackets);

			freePackets = gcnew ThreadSafeQueue<Packet ^>(maxVideoPackets + maxAudioPackets);

			videoPackets = gcnew ThreadSafeQueue<Packet ^>(maxVideoPackets);
			audioPackets = gcnew ThreadSafeQueue<Packet ^>(maxAudioPackets);

			decodingVideoStopped = gcnew AutoResetEvent(false);
			decodingAudioStopped = gcnew AutoResetEvent(false);
			demuxingStopped = gcnew AutoResetEvent(false);

			pauseDecoding = false;

			decodingPaused = gcnew array<WaitHandle^> {
				gcnew AutoResetEvent(false), gcnew AutoResetEvent(false)}; 

			decodingContinue = gcnew array<WaitHandle^> {
				gcnew AutoResetEvent(false), gcnew AutoResetEvent(false)}; 

		}

		~FrameQueue() {

			dispose();
		}

		property AutoResetEvent ^DemuxingStopped {
			
			AutoResetEvent ^get() {

				return(demuxingStopped);
			}
			
		}

		property int MaxVideoPackets {

			int get() {

				return(maxVideoPackets);
			}
		}

		property int MaxAudioPackets {

			int get() {

				return(maxAudioPackets);
			}
		}

		property int VideoPacketsInQueue {

			int get() {

				return(videoPackets->QueueSize);
			}
		}

		property int AudioPacketsInQueue {

			int get() {

				return(audioPackets->QueueSize);
			}
		}


		void initialize() {

			dispose();

			videoClock = 0;
			audioClock = 0;

			videoFrame = gcnew VideoFrame();
			convertedVideoFrame = gcnew VideoFrame(videoDecoder->getWidth(), 
				videoDecoder->getHeight(), PIX_FMT_YUV420P);
			audioFrame = gcnew AudioFrame();

			for(int i = 0; i < packetData->Length; i++) {

				packetData[i] = gcnew Packet();
				freePackets->add(packetData[i]);
			}

		}

		void flush() {		

			// note that demuxing is not active during this function

			// wait for video and audio decoding to pause
			pauseDecoding = true;
			WaitHandle::WaitAll(decodingPaused);

			videoPackets->flush();
			audioPackets->flush();
			freePackets->flush();

			for(int i = 0; i < packetData->Length; i++) {

				// free packet data before inserting them back into freepackets
				av_free_packet(packetData[i]->AVLibPacketData);
				freePackets->add(packetData[i]);
			}
		
			pauseDecoding = false;
			((AutoResetEvent ^)decodingContinue[0])->Set();
			((AutoResetEvent ^)decodingContinue[1])->Set();
		}

		void start() {

			freePackets->open();
			videoPackets->open();
			audioPackets->open();

		}

		void stop() {

			freePackets->stop();
			demuxingStopped->WaitOne();
			videoPackets->stop();
			decodingVideoStopped->WaitOne();
			audioPackets->stop();
			decodingAudioStopped->WaitOne();

		}

		void dispose() {

			if(convertedVideoFrame != nullptr) {

				delete convertedVideoFrame;
				convertedVideoFrame = nullptr;
			}

			if(videoFrame != nullptr) {

				delete videoFrame;
				videoFrame = nullptr;
			}

			if(audioFrame != nullptr) {

				delete audioFrame;
				audioFrame = nullptr;
			}

			videoPackets->flush();
			audioPackets->flush();

			freePackets->flush();

			for(int i = 0; i < packetData->Length; i++) {

				if(packetData[i] != nullptr) {

					delete packetData[i];
					packetData[i] = nullptr;
				}

			}

		}

		bool getFreePacket(Packet ^%packet) {

			bool result = freePackets->tryGet(packet);

			return(result);
		}

		void addFreePacket(Packet ^packet) {

			freePackets->add(packet);
		}

		void addVideoPacket(Packet ^packet) {

			videoPackets->add(packet);
		}

		void addAudioPacket(Packet ^packet) {

			audioPackets->add(packet);
		}

		VideoFrame ^getDecodedVideoFrame() {

			if(pauseDecoding == true) {

				((AutoResetEvent ^)decodingPaused[0])->Set();
				decodingContinue[0]->WaitOne();
			}

			int frameFinished = 0;

			VideoFrame ^finalFrame = videoFrame;

			while(!frameFinished) {

				Packet ^videoPacket;

				bool success = videoPackets->tryGet(videoPacket);
				if(success == false) {

					decodingVideoStopped->Set();
					return(nullptr);
				}

				avcodec_get_frame_defaults(videoFrame->AVLibFrameData);

				int ret = avcodec_decode_video2(videoDecoder->getVideoCodecContext(), 
					videoFrame->AVLibFrameData, &frameFinished, videoPacket->AVLibPacketData);
				if(ret < 0) {

					//Error decoding video frame
					//return(0);
				}

				if(frameFinished)
				{

					//if(videoFrame->AVLibFrameData->format != PIX_FMT_YUV420P) {

					// convert frame to the right format
					finalFrame = convertedVideoFrame;

					sws_scale(videoDecoder->getImageConvertContext(),
						videoFrame->AVLibFrameData->data,
						videoFrame->AVLibFrameData->linesize,
						0,
						videoFrame->AVLibFrameData->height,
						convertedVideoFrame->AVLibFrameData->data,
						convertedVideoFrame->AVLibFrameData->linesize);

					//}

					finalFrame->Pts = synchronizeVideo(
						videoFrame->AVLibFrameData->repeat_pict, 
						videoPacket->AVLibPacketData->dts);
				}

				av_free_packet(videoPacket->AVLibPacketData);
				freePackets->add(videoPacket);
			}

			return(finalFrame);


		}


		AudioFrame ^getDecodedAudioFrame() {

			if(pauseDecoding == true) {
				
				((AutoResetEvent ^)decodingPaused[1])->Set();
				decodingContinue[1]->WaitOne();
			}

			int frameFinished = 0;		

			while(!frameFinished) {

				Packet ^audioPacket;

				bool success = audioPackets->tryGet(audioPacket);
				if(success == false) {

					decodingAudioStopped->Set();
					return(nullptr);
				}

				avcodec_get_frame_defaults(audioFrame->AVLibFrameData);

				int ret = avcodec_decode_audio4(videoDecoder->getAudioCodecContext(), 
					audioFrame->AVLibFrameData, &frameFinished, audioPacket->AVLibPacketData);
				if(ret < 0) {

					//Error decoding audio frame
					//return(0);
				}

				if(frameFinished)
				{

					audioFrame->Length = av_samples_get_buffer_size(NULL, 
						videoDecoder->getAudioNrChannels(), 
						audioFrame->AVLibFrameData->nb_samples,
						(AVSampleFormat)audioFrame->AVLibFrameData->format, 
						1);

					audioFrame->Pts = synchronizeAudio(audioFrame->Length, 
						audioPacket->AVLibPacketData->dts);

					audioFrame->copyAudioDataToManagedMemory();

				}

				av_free_packet(audioPacket->AVLibPacketData);
				freePackets->add(audioPacket);
			}


			return(audioFrame);


		}


	};
}