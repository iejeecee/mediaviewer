#pragma once

#include "ThreadSafeQueue.h"
#include "VideoFrame.h"
#include "AudioFrame.h"
#include "Packet.h"
#include "VideoDecoder.h"

using namespace Microsoft::DirectX::Direct3D;
using namespace System::Collections::Generic;
using namespace System::Threading;
using namespace System::Diagnostics;

namespace VideoLib {

	public ref class FrameQueue
	{
	private:

		VideoDecoder *videoDecoder;

		VideoFrame ^videoFrame, ^tempVideoFrame;
		AudioFrame ^audioFrame;

		static const int maxVideoPackets = 100;
		static const int maxAudioPackets = 300;

		ThreadSafeQueue<Packet ^> ^freePackets;
		ThreadSafeQueue<Packet ^> ^videoPackets;
		ThreadSafeQueue<Packet ^> ^audioPackets;

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

		}

		~FrameQueue() {

			dispose();
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
			tempVideoFrame = gcnew VideoFrame();
			audioFrame = gcnew AudioFrame();
			
			for(int i = 0; i < packetData->Length; i++) {

				packetData[i] = gcnew Packet();
				freePackets->add(packetData[i]);
			}

		}

		void flush() {

/*			

			// make video render and audio player wait after flush
			decodedVideoFrames->flushAndPause();
			decodedAudioFrames->flushAndPause();
		
			// allow video decoder to continue filling the framequeue
			freeVideoFrames->flush();
			freeAudioFrames->flush();	

			for(int i = 0; i < videoFrameData->Length; i++) {

				freeVideoFrames->add(videoFrameData[i]);
			}
			// reason for video/audioFrameLock:
			// freeAudioFrames is empty here
			// audioThread adds a free frame at this moment
			// this thread blocks because there are too many frames in the queue
			
			for(int i = 0; i < audioFrameData->Length; i++) {

				freeAudioFrames->add(audioFrameData[i]);
			}
*/

		}

		void start() {

			videoPackets->open();
			audioPackets->open();
			freePackets->open();
		}

		void stop() {

			videoPackets->stop();
			audioPackets->stop();
			freePackets->stop();
		}

		void dispose() {

			if(videoFrame != nullptr) {

				delete videoFrame;
				videoFrame = nullptr;
			}

			if(tempVideoFrame != nullptr) {

				delete tempVideoFrame;
				tempVideoFrame = nullptr;
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

			int frameFinished = 0;

			while(!frameFinished) {

				Packet ^videoPacket;

				bool success = videoPackets->tryGet(videoPacket);
				if(success == false) {
					
					return(nullptr);
				}

				avcodec_get_frame_defaults(videoFrame->AVLibFrameData);

				int ret = avcodec_decode_video2(videoDecoder->getVideoCodecContext(), 
					tempVideoFrame->AVLibFrameData, &frameFinished, videoPacket->AVLibPacketData);
				if(ret < 0) {

					//Error decoding video frame
					//return(0);
				}

				if(frameFinished)
				{

					sws_scale(videoDecoder->getImageConvertContext(),
						tempVideoFrame->AVLibFrameData->data,
						tempVideoFrame->AVLibFrameData->linesize,
						0,
						tempVideoFrame->AVLibFrameData->height,
						videoFrame->AVLibFrameData->data,
						videoFrame->AVLibFrameData->linesize);


					videoFrame->Pts = synchronizeVideo(
						tempVideoFrame->AVLibFrameData->repeat_pict, 
						videoPacket->AVLibPacketData->dts);
				}

				av_free_packet(videoPacket->AVLibPacketData);
				freePackets->add(videoPacket);
			}
			

			return(videoFrame);
		}

	
		AudioFrame ^getDecodedAudioFrame() {

			int frameFinished = 0;

			while(!frameFinished) {

				Packet ^audioPacket;

				bool success = audioPackets->tryGet(audioPacket);
				if(success == false) {
					
					return(nullptr);
				}

				avcodec_get_frame_defaults(videoFrame->AVLibFrameData);

				int ret = avcodec_decode_video2(videoDecoder->getVideoCodecContext(), 
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