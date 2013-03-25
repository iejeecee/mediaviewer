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

		array<WaitHandle ^> ^decodingPaused;
	
		array<Packet ^> ^packetData;

		double videoClock;
		double audioClock;

		bool audioPacketsClosed;
		bool videoPacketsClosed;

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

		void videoPackets_Closed(Object ^sender, EventArgs ^) {

			videoPacketsClosed = true;
			if(audioPacketsClosed == true) {

				// send framequeue is closed event once both the
				// audio and video packet queue is closed
				Closed(this, EventArgs::Empty);
				videoPacketsClosed = false;
				audioPacketsClosed = false;
			}
		}

		void audioPackets_Closed(Object ^sender, EventArgs ^) {

			audioPacketsClosed = true;
			if(videoPacketsClosed == true) {

				// send framequeue is closed event once both the
				// audio and video packet queue is closed
				Closed(this, EventArgs::Empty);
				videoPacketsClosed = false;
				audioPacketsClosed = false;
			}
		}

	public:

		event EventHandler ^Closed;

		FrameQueue(VideoDecoder *videoDecoder) {

			this->videoDecoder = videoDecoder;

			videoFrame = nullptr;
			audioFrame = nullptr;

			packetData = gcnew array<Packet ^>(maxVideoPackets + maxAudioPackets);

			for(int i = 0; i < packetData->Length; i++) {

				packetData[i] = gcnew Packet();
			}

			freePackets = gcnew ThreadSafeQueue<Packet ^>(maxVideoPackets + maxAudioPackets);

			videoPackets = gcnew ThreadSafeQueue<Packet ^>(maxVideoPackets);
			audioPackets = gcnew ThreadSafeQueue<Packet ^>(maxAudioPackets);

			decodingPaused = gcnew array<WaitHandle^> {
				audioPackets->Paused, videoPackets->Paused}; 		

			videoPackets->Closed += gcnew EventHandler(this, &FrameQueue::videoPackets_Closed);
			audioPackets->Closed += gcnew EventHandler(this, &FrameQueue::audioPackets_Closed);

			audioPacketsClosed = false;
			videoPacketsClosed = false;
		}

		~FrameQueue() {

			dispose();

			for(int i = 0; i < packetData->Length; i++) {

				delete packetData[i];
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

				freePackets->add(packetData[i]);
			}

		}

		void flush() {		

			// note that demuxing is not active during this function

			// wait for video and audio decoding to pause
			// WEIRD BUG? the paused AutoResetEvent 
			// stays signalled (is it's memory overwritten?, that would be very bad!) 
			// after calls to av_free_packet later in the function
			// temp fixed by resetting it manually 
			videoPackets->Paused->Reset();
			audioPackets->Paused->Reset();
			videoPackets->pause();
			audioPackets->pause();

			WaitHandle::WaitAll(decodingPaused);

			videoPackets->flush();
			audioPackets->flush();
			freePackets->flush();

			for(int i = 0; i < packetData->Length; i++) {

				addFreePacket(packetData[i]);
			}
		
		}

		void start() {

			freePackets->open();
			videoPackets->open();
			audioPackets->open();

		}

		void stop() {

			// stop each queue in turn and wait for it to actually be stopped
			if(freePackets->QueueState != ThreadSafeQueue<Packet ^>::State::STOPPED) {
				freePackets->stop();
				freePackets->Stopped->WaitOne();
			}
			if(videoPackets->QueueState != ThreadSafeQueue<Packet ^>::State::STOPPED) {
				videoPackets->stop();
				videoPackets->Stopped->WaitOne();
			}
			if(audioPackets->QueueState != ThreadSafeQueue<Packet ^>::State::STOPPED) {
				audioPackets->stop();
				audioPackets->Stopped->WaitOne();
			}

		}

		// this function should only be called after the final packet has
		// been read from the video stream to indicate the queue is winding down.
		// A closed event will be fired once the video and audio queues are empty
		void close() {

			// set freePackets stopped autoreset event, since no new packets are produced anymore
			// otherwise calling stop during queue winddown will block.
			freePackets->Stopped->Set();
		
			videoPackets->close();					
			audioPackets->close();
		
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

					packetData[i]->free();
				}

			}

		}

		bool getFreePacket(Packet ^%packet) {

			bool result = freePackets->tryGet(packet);

			return(result);
		}

		void addFreePacket(Packet ^packet) {

			// free packet data before inserting them back into freepackets
			packet->free();
			freePackets->add(packet);
		}

		void addVideoPacket(Packet ^packet) {

			videoPackets->add(packet);
		}

		void addAudioPacket(Packet ^packet) {

			audioPackets->add(packet);
		}

		VideoFrame ^getDecodedVideoFrame() {

			int frameFinished = 0;

			VideoFrame ^finalFrame = videoFrame;

			while(!frameFinished) {

				Packet ^videoPacket;

				bool success = videoPackets->tryGet(videoPacket);
				if(success == false) {

					return(nullptr);
				}

				avcodec_get_frame_defaults(videoFrame->AVLibFrameData);

				if(videoPacket->AVLibPacketData->flags & AV_PKT_FLAG_CORRUPT) {

					System::Diagnostics::Debug::Write("corrupt packet");
				}

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

				addFreePacket(videoPacket);
			}

			return(finalFrame);


		}


		AudioFrame ^getDecodedAudioFrame() {

			int frameFinished = 0;		

			while(!frameFinished) {

				Packet ^audioPacket;

				bool success = audioPackets->tryGet(audioPacket);
				if(success == false) {

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

				addFreePacket(audioPacket);
			}


			return(audioFrame);


		}


	};
}