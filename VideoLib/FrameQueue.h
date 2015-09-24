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
#include "PacketQueue.h"
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
			PLAY,      
			PAUSE,		
			BLOCK,
			CLOSE				
		};
		
		
		event EventHandler ^Buffering;
		event EventHandler ^FinishedBuffering;	
		event EventHandler ^Finished;	

		enum class QueueID {
			DEMUXER = 0,
			VIDEO,
			AUDIO,		
		};

	private:
		
		static int nrPackets = 300;		

		FrameQueueState state;

		Object ^lockObject;

		VideoDecoder *videoDecoder, *audioDecoder;

		VideoFrame ^videoFrame, ^convertedVideoFrame;
		AudioFrame ^audioFrame, ^convertedAudioFrame;
		Packet ^audioPacket;
		
		PacketQueue ^videoPackets;
		PacketQueue ^audioPackets;
		PacketQueue ^freePackets;	
			
		array<Packet ^> ^packetData;

		double videoClock;
		double audioClock;
		
		bool singleFrame;
		bool isLastFrame;
		AutoResetEvent ^singleFrameEvent;
		AutoResetEvent ^continueSingleFrameEvent;

		bool isBuffering;
		Tasks::Task ^isBufferingFinishedTask;

		int nrVideoPacketReadErrors;
		int nrAudioPacketReadErrors;

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
				pts = dts * av_q2d(audioDecoder->getAudioStream()->time_base);
				// set clock to current pts;
				audioClock = pts;

			} else {

				// if we aren't given a pts, set it to the clock 
				pts = audioClock;
				// calculate next pts in seconds
				audioClock += sizeBytes / double(audioDecoder->getAudioBytesPerSecond());
			}

			return(pts);
		}

		void setPacketQueueState(Nullable<PacketQueue::PacketQueueState> videoState, Nullable<PacketQueue::PacketQueueState> audioState, 
			Nullable<PacketQueue::PacketQueueState> freeState) 
		{					
			if(videoDecoder->hasVideo() && videoState.HasValue) {

				videoPackets->State = videoState.Value;
			} 
							
			if(audioDecoder->hasAudio() && audioState.HasValue) {

				audioPackets->State = audioState.Value;
			} 

			if(freeState.HasValue) { 

				freePackets->State = freeState.Value;
			}
							
		}

		bool isPacketQueueInState(Nullable<PacketQueue::PacketQueueState> videoState, Nullable<PacketQueue::PacketQueueState> audioState, 
			Nullable<PacketQueue::PacketQueueState> freeState) 
		{
			bool isState = true;

			if(videoDecoder->hasVideo() && videoState.HasValue) {

				isState = videoPackets->State == videoState.Value && isState;
			} 
							
			if(audioDecoder->hasAudio() && audioState.HasValue) {

				isState = audioPackets->State == audioState.Value && isState;
			} 

			if(freeState.HasValue) { 

				isState = freePackets->State == freeState.Value && isState;
			}

			return(isState);
		}

		Nullable<PacketQueue::PacketQueueState> frameToPacketState(Nullable<FrameQueueState> state, bool isStart) {

			if(state.HasValue == false) return(Nullable<PacketQueue::PacketQueueState>());

			switch(state.Value) 
			{
			case FrameQueueState::PLAY:
				{
					return(PacketQueue::PacketQueueState::OPEN);
				}
			case FrameQueueState::PAUSE:
				{
					if(isStart) return(PacketQueue::PacketQueueState::PAUSE_START);
					else return(PacketQueue::PacketQueueState::PAUSE_END);
				}
			case FrameQueueState::BLOCK:
				{
					if(isStart) return(PacketQueue::PacketQueueState::BLOCK_START);
					else return(PacketQueue::PacketQueueState::BLOCK_END);
				}
			case FrameQueueState::CLOSE:
				{
					if(isStart) return(PacketQueue::PacketQueueState::CLOSE_START);
					else return(PacketQueue::PacketQueueState::CLOSE_END);
				}
			default:
				{
					throw gcnew InvalidOperationException();
				}

			}
		}

		void packetQueueFinished() {
							
			if(videoPackets->IsFinished && audioPackets->IsFinished)
			{
				// stop free packets queue 
				freePackets->State = PacketQueue::PacketQueueState::CLOSE_END;
									
				Finished(this, EventArgs::Empty);					
			}
						
		}	

		void isPacketQueueBufferingFinished() {

			Monitor::Enter(lockObject); 
			try {

				while(!videoPackets->IsBufferFull || !audioPackets->IsBufferFull) 
				{
					if(isPacketQueueInState(Nullable<PacketQueue::PacketQueueState>(PacketQueue::PacketQueueState::CLOSE_END),
						Nullable<PacketQueue::PacketQueueState>(PacketQueue::PacketQueueState::CLOSE_END),Nullable<PacketQueue::PacketQueueState>()))
					{
						// packetqueue is closed
						return;
					}					

					Monitor::Wait(lockObject);
				}
							
			} finally {
							
				Monitor::Exit(lockObject);				
			}
		}
		
		void endPacketQueueBuffering(Tasks::Task ^task) {
		
			FinishedBuffering(this,EventArgs::Empty);	
			isBuffering = false;
							
		}
				
	public:

		void setState(Nullable<FrameQueueState> video,Nullable<FrameQueueState> audio, Nullable<FrameQueueState> demuxer) 
		{

			Monitor::Enter(lockObject);
			try {
										
				setPacketQueueState(frameToPacketState(video, true), frameToPacketState(audio, true), frameToPacketState(demuxer, true));
			
				// wait until the packet queue is fully closed	
				while(!isPacketQueueInState(frameToPacketState(video, false), frameToPacketState(audio, false), frameToPacketState(demuxer, false))) 
				{					
					Monitor::Wait(lockObject);
				}				

			} finally {
				Monitor::Exit(lockObject);
			}
		}

		Tasks::Task ^startPacketQueueBuffering() {
						
			// make sure only one buffering task can run at once
			Monitor::Enter(lockObject);
			try {

				if(isBuffering == true) {

					return(isBufferingFinishedTask);
				}

				isBuffering = true;

				setState(Nullable<FrameQueueState>(FrameQueueState::PAUSE), Nullable<FrameQueueState>(FrameQueueState::PAUSE), 
					Nullable<FrameQueueState>(FrameQueueState::PLAY));	

				Buffering(this,EventArgs::Empty);

				return isBufferingFinishedTask = Tasks::Task::Factory->StartNew(gcnew Action(this,&FrameQueue::isPacketQueueBufferingFinished))->ContinueWith(
					gcnew Action<Tasks::Task ^>(this,&FrameQueue::endPacketQueueBuffering));

			} finally {

				Monitor::Exit(lockObject);
			}
						
		}
				
		FrameQueue() {

			lockObject = gcnew Object();
			
			videoFrame = nullptr;
			audioFrame = nullptr;

			convertedVideoFrame = nullptr;
			convertedAudioFrame = nullptr;
									
			videoPackets = gcnew PacketQueue("videoPackets", nrPackets, lockObject, true);
			audioPackets = gcnew PacketQueue("audioPackets", nrPackets, lockObject, true);
			freePackets = gcnew PacketQueue("freePackets",videoPackets->MaxPackets + audioPackets->MaxPackets,lockObject, false);
					
			packetData = gcnew array<Packet ^>(freePackets->MaxPackets);

			for(int i = 0; i < packetData->Length; i++) {

				packetData[i] = gcnew Packet();
			}
											
			singleFrameEvent = gcnew AutoResetEvent(false);
			continueSingleFrameEvent = gcnew AutoResetEvent(false);
				
			audioPacket = nullptr;

			nrVideoPacketReadErrors = 0;
			nrAudioPacketReadErrors = 0;
		
			isBufferingFinishedTask = Tasks::Task::FromResult(0);

			isBuffering = false;					
		}

		~FrameQueue() {

			release();

			for(int i = 0; i < packetData->Length; i++) {

				delete packetData[i];
			}
			
		}

		property int NrVideoPacketReadErrors
		{
			int get() {

				return(nrVideoPacketReadErrors);
			}

			void set(int value)
			{
				nrVideoPacketReadErrors = value;
			}
		}

		property int NrAudioPacketReadErrors
		{
			int get() {

				return(nrAudioPacketReadErrors);
			}

			void set(int value)
			{
				nrAudioPacketReadErrors = value;
			}
		}


		property PacketQueue::PacketQueueState VideoPacketQueueState {

			PacketQueue::PacketQueueState get() {

				return(videoPackets->State);
			}
		}
		
		property PacketQueue::PacketQueueState FreePacketQueueState {

			PacketQueue::PacketQueueState get() {

				return(freePackets->State);
			}
		}

		property PacketQueue::PacketQueueState AudioPacketQueueState {

			PacketQueue::PacketQueueState get() {

				return(audioPackets->State);
			}
		}

		property int MaxFreePackets {

			int get() {

				return(freePackets->MaxPackets);
			}
		}

		property int MaxAudioPackets {

			int get() {

				return(audioPackets->MaxPackets);
			}
		}

		property int MaxVideoPackets {

			int get() {

				return(videoPackets->MaxPackets);
			}
		}
		
		property int FreePacketsInQueue {

			int get() {

				return(freePackets->NrPacketsInQueue);
			}
		}

		property int VideoPacketsInQueue {

			int get() {

				return(videoPackets->NrPacketsInQueue);
			}
		}

		property int AudioPacketsInQueue {

			int get() {

				return(audioPackets->NrPacketsInQueue);
			}
		}

		
		void initialize(VideoDecoder *videoDecoder, VideoDecoder *audioDecoder) {

			release();

			this->videoDecoder = videoDecoder;
			this->audioDecoder = audioDecoder;

			videoClock = 0;
			audioClock = 0;

			if(videoDecoder->hasVideo()) 
			{
				videoFrame = gcnew VideoFrame();

				convertedVideoFrame = gcnew VideoFrame(
					videoDecoder->getWidth(), 
					videoDecoder->getHeight(), 
					videoDecoder->getOutputPixelFormat());
			}
			
			if(audioDecoder->hasAudio()) 
			{
				audioFrame = gcnew AudioFrame();

				convertedAudioFrame = gcnew AudioFrame(
					audioDecoder->getOutputSampleFormat(),
					audioDecoder->getOutputChannelLayout(),
					audioDecoder->getOutputSampleRate());
			} 

			for(int i = 0; i < packetData->Length; i++) {

				freePackets->addPacket(packetData[i]);
			}
				
			audioPacket = nullptr;

			nrVideoPacketReadErrors = 0;
			nrAudioPacketReadErrors = 0;
		}
		
		void flush() {		

			Monitor::Enter(lockObject);
			try {
				
				// clear queues
				freePackets->flush();
				audioPackets->flush();
				videoPackets->flush();

				// add all packets into freepackets queue
				for(int i = 0; i < packetData->Length; i++) {

					packetData[i]->free();
					freePackets->addPacket(packetData[i]);					
				}

				audioPacket = nullptr;

			} finally {

				Monitor::Exit(lockObject);
			}
		
		}
		
		bool startSingleFrame() {

			singleFrame = true;
			isLastFrame = false;
			singleFrameEvent->Reset();
			continueSingleFrameEvent->Reset();

			setState(Nullable<FrameQueueState>(FrameQueueState::PLAY), Nullable<FrameQueueState>(), Nullable<FrameQueueState>());	
		
			singleFrameEvent->WaitOne();

			singleFrame = false;
			if(isLastFrame == true) {
				return(true);
			}

			Monitor::Enter(lockObject);
			try {
	
				//State = FrameQueueState::PAUSED;

				setPacketQueueState(Nullable<PacketQueue::PacketQueueState>(PacketQueue::PacketQueueState::PAUSE_START), 
					Nullable<PacketQueue::PacketQueueState>(),Nullable<PacketQueue::PacketQueueState>());
				
				continueSingleFrameEvent->Set();

				// wait until the packet queue is fully stopped
				while(!isPacketQueueInState(Nullable<PacketQueue::PacketQueueState>(PacketQueue::PacketQueueState::PAUSE_END), 
					Nullable<PacketQueue::PacketQueueState>(),Nullable<PacketQueue::PacketQueueState>()) &&
					!videoPackets->IsFinished) 
				{					
					Monitor::Wait(lockObject);
				}		

			} finally {

				Monitor::Exit(lockObject);				
			}

			return(false);
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
			
			audioPackets->flush();
			videoPackets->flush();
			freePackets->flush();
			
			for(int i = 0; i < packetData->Length; i++) {

				if(packetData[i] != nullptr) {

					packetData[i]->free();
				}

			}

		}
		
		bool getFreePacket(Packet ^%packet) {

			PacketQueue::GetResult result = freePackets->getPacket(packet);

			return(result == PacketQueue::GetResult::SUCCESS);
		}

		void addFreePacket(Packet ^packet) {

			// free packet data before inserting it back into freepackets
			packet->free();
			freePackets->addPacket(packet);
		}

		void addVideoPacket(Packet ^packet) {

			videoPackets->addPacket(packet);
		}

		void addAudioPacket(Packet ^packet) {

			audioPackets->addPacket(packet);
		}

		VideoFrame ^getDecodedVideoFrame() {

			int frameFinished = 0;

			while(!frameFinished) {

				Packet ^videoPacket = nullptr;

				PacketQueue::GetResult result = videoPackets->getPacket(videoPacket);
				if(result != PacketQueue::GetResult::SUCCESS) 
				{								
					if(result == PacketQueue::GetResult::BUFFER) {

						startPacketQueueBuffering();

					} else if(result == PacketQueue::GetResult::FINAL) {

						if(singleFrame) {

							isLastFrame = true;
							singleFrameEvent->Set();						
						}

						packetQueueFinished();
					}
					
					return(nullptr);
				}
			
				videoFrame->setFrameDefaults();

				int ret = videoDecoder->decodeVideoFrame(videoFrame->AVLibFrameData, &frameFinished, 
					videoPacket->AVLibPacketData);
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

			if(singleFrame) {

				singleFrameEvent->Set();
				continueSingleFrameEvent->WaitOne();
			}

			return(convertedVideoFrame);


		}


		AudioFrame ^getDecodedAudioFrame() {

			int frameFinished = 0;		
			
			// a single audio packet can contain multiple frames
			while(1) {

				if(audioPacket == nullptr 
					|| audioPacket->Type == VideoLib::PacketType::LAST_PACKET 
					|| audioPacket->AVLibPacketData->size == 0) 
				{

					if(audioPacket != nullptr) {

						if(audioPacket->Type == VideoLib::PacketType::LAST_PACKET) 
						{
							return(nullptr);
						}

						addFreePacket(audioPacket);					
					}

					PacketQueue::GetResult result = audioPackets->getPacket(audioPacket);
					if(result != PacketQueue::GetResult::SUCCESS) 
					{								
						if(result == PacketQueue::GetResult::BUFFER) {

							startPacketQueueBuffering();

						} else if(result == PacketQueue::GetResult::FINAL) {

							packetQueueFinished();
						}
					
						return(nullptr);
					}
				
				}

				while(audioPacket->AVLibPacketData->size > 0) {

					audioFrame->setFrameDefaults();

					int bytesConsumed = audioDecoder->decodeAudioFrame(audioFrame->AVLibFrameData, 
						&frameFinished, audioPacket->AVLibPacketData);						
					if(bytesConsumed < 0) {
					
						// skip this packet and play silence
						frameFinished = true;
						audioPacket->AVLibPacketData->size = 0;

					} else {

						audioPacket->AVLibPacketData->size -= bytesConsumed;
						audioPacket->AVLibPacketData->data += bytesConsumed;
					}

					if(frameFinished)
					{									
						convertedAudioFrame->Length = audioDecoder->convertAudioFrame(audioFrame->AVLibFrameData, convertedAudioFrame->AVLibFrameData);											
					
						convertedAudioFrame->Pts = synchronizeAudio(convertedAudioFrame->Length, 
								audioPacket->AVLibPacketData->dts);

						convertedAudioFrame->copyAudioDataToManagedMemory();

						return(convertedAudioFrame);
					}
				}
				
			}

		}


	};

}
