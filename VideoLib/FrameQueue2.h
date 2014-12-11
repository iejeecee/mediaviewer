#pragma once

#ifdef __cplusplus
#define __STDC_CONSTANT_MACROS
#ifdef _STDINT_H
#undef _STDINT_H
#endif
# include "stdint.h"
#endif

//#include "PacketQueue.h"
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
	public:

		enum class FrameQueueState {
			ACTIVE,       // items can be added and removed from the queue
			PAUSED,		// queue will block on removing items
			STOPPED	// queue will return false on removing items			
		};

		enum class QueueID {
			FREE_PACKETS = 0,
			VIDEO_PACKETS,
			AUDIO_PACKETS
		};

		event EventHandler ^Finished;

	private:

		Object ^lockObject;

		VideoDecoder *videoDecoder;

		VideoFrame ^videoFrame, ^convertedVideoFrame;
		AudioFrame ^audioFrame, ^convertedAudioFrame;
		
		array<Queue<Packet ^> ^> ^packetQueue;
		array<int> ^maxPackets;

		array<bool> ^packetQueueStopped;
		array<bool> ^packetQueuePaused;
			
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

		

		bool getPacket(QueueID queueID, Packet ^%packet) {

			Monitor::Enter(lockObject);
			try {

				int id = (int)queueID;

				while(packetQueue[id]->Count == 0 || State == FrameQueueState::STOPPED ||
					State == FrameQueueState::PAUSED) 
				{
					
					if(State == FrameQueueState::STOPPED) 
					{
						packetQueueStopped[id] = true;
						Monitor::PulseAll(lockObject);
						return(false);
					} 
					else if(State == FrameQueueState::PAUSED)
					{
						if(packetQueuePaused[id] == false) 
						{
							packetQueuePaused[id] = true;
							Monitor::PulseAll(lockObject);
						}
					}

					Monitor::Wait(lockObject);
				}

				packet = packetQueue[id]->Dequeue();

				if(packetQueue[id]->Count == maxPackets[id] - 1) {
					// wake threads waiting on full queue
					Monitor::PulseAll(lockObject);
				}

				if(packet->Type == PacketType::LAST_PACKET) {

					packetQueueStopped[id] = true;							
					return(false);
				}

				return(true);

			} finally {
				
				Monitor::Exit(lockObject);

				if(packet != nullptr && packet->Type == PacketType::LAST_PACKET) {

					if(packetQueueStopped[(int)QueueID::AUDIO_PACKETS] && 
						packetQueueStopped[(int)QueueID::VIDEO_PACKETS])
					{
						Finished(this, EventArgs::Empty);
					}

				}

			}
		}

		void addPacket(QueueID queueID, Packet ^packet) {
						
			Monitor::Enter(lockObject);
			try {

				int id = (int)queueID;

				while(packetQueue[id]->Count == maxPackets[id]) {

					if(State == FrameQueueState::STOPPED) {	
					
						return;
					} 
					/*else if(State == FrameQueueState::PAUSED)
					{
						if(packetQueuePaused[id] == false) 
						{
							packetQueuePaused[id] = true;
							Monitor::PulseAll(lockObject);
						}
					}*/

					Monitor::Wait(lockObject);
				}
		
				packetQueue[id]->Enqueue(packet);

				if(packetQueue[id]->Count == 1) {
					// wake threads waiting on empty queue
					Monitor::PulseAll(lockObject);
				}
								
			} finally {
		
				Monitor::Exit(lockObject);
			}
		}

		bool isTrue(cli::array<bool> ^vars) {

			return(vars[0] && vars[1] && vars[2]);
		}

		bool isTrueAudioVideo(cli::array<bool> ^vars) {

			return(vars[(int)QueueID::VIDEO_PACKETS] && vars[(int)QueueID::AUDIO_PACKETS]);
		}

		FrameQueueState state;

	public:
		
		property FrameQueueState State {

			FrameQueueState get() {

				return(state);
			}

		private: 

			void set(FrameQueueState state) 
			{
				this->state = state;
			}
		}


		FrameQueue(VideoDecoder *videoDecoder) {

			lockObject = gcnew Object();

			this->videoDecoder = videoDecoder;

			videoFrame = nullptr;
			audioFrame = nullptr;

			convertedVideoFrame = nullptr;
			convertedAudioFrame = nullptr;

			maxPackets = gcnew cli::array<int>(3);

			maxPackets[(int)QueueID::VIDEO_PACKETS] = 300;
			maxPackets[(int)QueueID::AUDIO_PACKETS] = 300;
			maxPackets[(int)QueueID::FREE_PACKETS] = maxPackets[(int)QueueID::VIDEO_PACKETS] + maxPackets[(int)QueueID::AUDIO_PACKETS];
			
			packetQueue = gcnew cli::array<Queue<Packet ^> ^>(3);
			packetQueueStopped = gcnew cli::array<bool>(3);
			packetQueuePaused = gcnew cli::array<bool>(3);
		
			for(int i = 0; i < 3; i++) {

				packetQueue[i] = gcnew Queue<Packet ^>(maxPackets[i]);
				packetQueueStopped[i] = false;
				packetQueuePaused[i] = false;				
			}
			
			packetData = gcnew array<Packet ^>(maxPackets[(int)QueueID::FREE_PACKETS]);

			for(int i = 0; i < packetData->Length; i++) {

				packetData[i] = gcnew Packet();
			}
															
			State = FrameQueueState::ACTIVE;
		}

		~FrameQueue() {

			release();

			for(int i = 0; i < packetData->Length; i++) {

				delete packetData[i];
			}
			
		}

		property int MaxVideoPackets {

			int get() {

				return(maxPackets[(int)QueueID::VIDEO_PACKETS]);
			}
		}

		property int MaxAudioPackets {

			int get() {

				return(maxPackets[(int)QueueID::AUDIO_PACKETS]);
			}
		}

		property int VideoPacketsInQueue {

			int get() {

				return(packetQueue[(int)QueueID::VIDEO_PACKETS]->Count);
			}
		}

		property int AudioPacketsInQueue {

			int get() {

				return(packetQueue[(int)QueueID::AUDIO_PACKETS]->Count);
			}
		}

		
		void initialize() {

			release();

			videoClock = 0;
			audioClock = 0;

			videoFrame = gcnew VideoFrame();

			convertedVideoFrame = gcnew VideoFrame(videoDecoder->getWidth(), 
				videoDecoder->getHeight(), videoDecoder->getImageConvertFormat());
			
			if(videoDecoder->hasAudio()) {

				audioFrame = gcnew AudioFrame();

				convertedAudioFrame = gcnew AudioFrame(
					videoDecoder->getAudioConvertFormat());
			}

			for(int i = 0; i < packetData->Length; i++) {

				packetQueue[(int)QueueID::FREE_PACKETS]->Enqueue(packetData[i]);
			}

		}
		
		void flush() {		

			Monitor::Enter(lockObject);
			try {
				
				for(int i = 0; i < 3; i++) {

					packetQueue[i]->Clear();
				}

				for(int i = 0; i < packetData->Length; i++) {

					addFreePacket(packetData[i]);
				}

			} finally {
				Monitor::Exit(lockObject);
			}
		
		}

		void start() {

			Monitor::Enter(lockObject);
			try {

				if(State == FrameQueueState::ACTIVE) return;

				State = FrameQueueState::ACTIVE;

				for(int i = 0; i < 3; i++) {
				
					packetQueueStopped[i] = false;
					packetQueuePaused[i] = false;				
				}

				// wakeup threads waiting on empty queues
				// and put them in paused state
				Monitor::PulseAll(lockObject);

			} finally {
				Monitor::Exit(lockObject);
			}

		}

		void stop() {

			Monitor::Enter(lockObject);
			try {

				if(State == FrameQueueState::STOPPED) return;

				State = FrameQueueState::STOPPED;
				// wakeup threads waiting on empty queues
				// and put them in paused state
				Monitor::PulseAll(lockObject);

				// wait until the packet queue is fully stopped
				while(!isTrue(packetQueueStopped)) {
					
					Monitor::Wait(lockObject);
				}

			} finally {
				Monitor::Exit(lockObject);
			}

			
		}

		void pause() {

			Monitor::Enter(lockObject);
			try {

				if(State == FrameQueueState::PAUSED ||
					State == FrameQueueState::STOPPED) return;

				State = FrameQueueState::PAUSED;
				// wakeup threads waiting on empty queues
				// and put them in paused state
				Monitor::PulseAll(lockObject);

				// wait until the packet queue is fully paused
				while(!isTrueAudioVideo(packetQueuePaused)) {
					
					Monitor::Wait(lockObject);
				}

			} finally {
				Monitor::Exit(lockObject);
			}

		}
	
		void release() {

			if(convertedVideoFrame != nullptr) {

				delete convertedVideoFrame;
				convertedVideoFrame = nullptr;
			}

			if(convertedAudioFrame != nullptr) {

				delete convertedAudioFrame;
				convertedAudioFrame = nullptr;
			}

			if(videoFrame != nullptr) {

				delete videoFrame;
				videoFrame = nullptr;
			}

			if(audioFrame != nullptr) {

				delete audioFrame;
				audioFrame = nullptr;
			}

			for(int i = 0; i < 3; i++) {

				packetQueue[i]->Clear();
			}

			for(int i = 0; i < packetData->Length; i++) {

				if(packetData[i] != nullptr) {

					packetData[i]->free();
				}

			}

		}
		
		bool getFreePacket(Packet ^%packet) {

			bool result = getPacket(QueueID::FREE_PACKETS, packet);

			return(result);
		}

		void addFreePacket(Packet ^packet) {

			// free packet data before inserting it back into freepackets
			packet->free();
			addPacket(QueueID::FREE_PACKETS, packet);
		}

		void addVideoPacket(Packet ^packet) {

			addPacket(QueueID::VIDEO_PACKETS, packet);
		}

		void addAudioPacket(Packet ^packet) {

			addPacket(QueueID::AUDIO_PACKETS, packet);
		}

		VideoFrame ^getDecodedVideoFrame() {

			int frameFinished = 0;

			while(!frameFinished) {

				Packet ^videoPacket = nullptr;

				bool success = getPacket(QueueID::VIDEO_PACKETS, videoPacket);
				if(success == false) {
			
					return(nullptr);
				}

				avcodec_get_frame_defaults(videoFrame->AVLibFrameData);

				int ret = avcodec_decode_video2(videoDecoder->getVideoCodecContext(), 
					videoFrame->AVLibFrameData, &frameFinished, videoPacket->AVLibPacketData);
				if(ret < 0) {

					Video::writeToLog(AV_LOG_WARNING, "could not decode video frame");
				}

				if(frameFinished)
				{

					videoDecoder->convertVideoFrame(videoFrame->AVLibFrameData, convertedVideoFrame->AVLibFrameData);					

					convertedVideoFrame->Pts = synchronizeVideo(
						videoFrame->AVLibFrameData->repeat_pict, 
						videoPacket->AVLibPacketData->dts);
				}

				addFreePacket(videoPacket);
			}


			return(convertedVideoFrame);


		}


		AudioFrame ^getDecodedAudioFrame() {

			int frameFinished = 0;		

			while(!frameFinished) {

				Packet ^audioPacket = nullptr;

				bool success = getPacket(QueueID::AUDIO_PACKETS, audioPacket);
				if(success == false) {

					return(nullptr);
				}

				avcodec_get_frame_defaults(audioFrame->AVLibFrameData);

				int ret = avcodec_decode_audio4(videoDecoder->getAudioCodecContext(), 
					audioFrame->AVLibFrameData, &frameFinished, audioPacket->AVLibPacketData);
						
				if(ret < 0) {

					Video::writeToLog(AV_LOG_WARNING, "could not decode audio frame");
				}

				if(frameFinished)
				{

					SwrContext *acc = videoDecoder->getAudioConvertContext();

					// convert audio to a packed format ready for playback
					int numSamplesOut = swr_convert(acc,
							convertedAudioFrame->AVLibFrameData->data,
                            audioFrame->AVLibFrameData->nb_samples,
                            (const unsigned char**)audioFrame->AVLibFrameData->extended_data,
                            audioFrame->AVLibFrameData->nb_samples);

					// audio length does not equal linesize, because some extra 
					// padding bytes may be added for alignment.
					// Instead av_samples_get_buffer_size needs to be used
					convertedAudioFrame->Length = av_samples_get_buffer_size(NULL, 
						videoDecoder->getAudioNrChannels(), 
						numSamplesOut,
						(AVSampleFormat)convertedAudioFrame->AVLibFrameData->format, 
						1);
				
					convertedAudioFrame->Pts = synchronizeAudio(audioFrame->Length, 
						audioPacket->AVLibPacketData->dts);

					convertedAudioFrame->copyAudioDataToManagedMemory();

				}

				addFreePacket(audioPacket);
			}


			return(convertedAudioFrame);


		}


	};

}
