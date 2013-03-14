#pragma once

#include "ThreadSafeQueue.h"
#include "VideoFrame.h"
#include "AudioFrame.h"

using namespace Microsoft::DirectX::Direct3D;
using namespace System::Collections::Generic;
using namespace System::Threading;
using namespace System::Diagnostics;

namespace VideoLib {

	public ref class FrameQueue
	{
	private:

		static const int maxVideoFrames = 100;
		static const int maxAudioFrames = 300;

		ThreadSafeQueue<VideoFrame ^> ^freeVideoFrames;
		ThreadSafeQueue<VideoFrame ^> ^decodedVideoFrames;

		ThreadSafeQueue<AudioFrame ^> ^freeAudioFrames;
		ThreadSafeQueue<AudioFrame ^> ^decodedAudioFrames;
		
		array<VideoFrame ^> ^videoFrameData;
		array<AudioFrame ^> ^audioFrameData;

		Object ^videoFrameInUseLock;
		Object ^audioFrameInUseLock;

		void initializeAudioQueue(int maxAudioFrameBufferSize) {		

			for(int i = 0; i < maxAudioFrames; i++) {

				audioFrameData[i] = gcnew AudioFrame(maxAudioFrameBufferSize);

				freeAudioFrames->add(audioFrameData[i]);
			}

		}

		void initializeVideoQueue(int width, int height) {

			for(int i = 0; i < maxVideoFrames; i++) {

				videoFrameData[i] = gcnew VideoFrame(width, height);

				freeVideoFrames->add(videoFrameData[i]);
			}

		}

	public:

		FrameQueue() {
	
			videoFrameData = gcnew array<VideoFrame ^>(maxVideoFrames);
			audioFrameData = gcnew array<AudioFrame ^>(maxAudioFrames);

			freeVideoFrames = gcnew ThreadSafeQueue<VideoFrame ^>(maxVideoFrames);
			decodedVideoFrames = gcnew ThreadSafeQueue<VideoFrame ^>(maxVideoFrames);
			
			freeAudioFrames = gcnew ThreadSafeQueue<AudioFrame ^>(maxAudioFrames);
			decodedAudioFrames = gcnew ThreadSafeQueue<AudioFrame ^>(maxAudioFrames);
			
			videoFrameInUseLock = gcnew Object();
			audioFrameInUseLock = gcnew Object();
		}

		~FrameQueue() {

			dispose();
		}

		property bool IsStopped {

			bool get() {

				return(decodedVideoFrames->Stopped);
			}
		}

		property int MaxVideoQueueSize {

			int get() {

				return(decodedVideoFrames->MaxQueueSize);
			}
		}

		property int MaxAudioQueueSize {

			int get() {

				return(decodedAudioFrames->MaxQueueSize);
			}
		}

		property int VideoQueueSize {

			int get() {

				return(decodedVideoFrames->QueueSize);
			}
		}

		property int VideoQueueSizeBytes {

			int get() {

				return(videoFrameData->Length * videoFrameData[0]->SizeBytes);
			}
		}

		property int AudioQueueSize {

			int get() {

				return(decodedAudioFrames->QueueSize);
			}
		}

		property int AudioQueueSizeBytes {

			int get() {

				return(audioFrameData->Length * audioFrameData[0]->Data->Length);
			}
		}
	
		void initialize(Device ^device, int width, int height, 
			int maxAudioBufferSize) {

			dispose();

			initializeVideoQueue(width, height);
			initializeAudioQueue(maxAudioBufferSize);

		}

		void start() {

			freeVideoFrames->open();
			freeAudioFrames->open();
			decodedVideoFrames->open();
			decodedAudioFrames->open();
		
		}

		void flush() {

			Monitor::Enter(videoFrameInUseLock);
			Monitor::Enter(audioFrameInUseLock);

			decodedVideoFrames->flush();
			decodedAudioFrames->flush();
		
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

			Monitor::Exit(audioFrameInUseLock);
			Monitor::Exit(videoFrameInUseLock);
			
		}

		void stop() {

			decodedVideoFrames->stop();
			decodedAudioFrames->stop();
			freeVideoFrames->stop();
			freeAudioFrames->stop();			
			
		}

		void dispose() {

			decodedVideoFrames->flush();
			decodedAudioFrames->flush();
		
			freeVideoFrames->flush();
			freeAudioFrames->flush();	

			for(int i = 0; i < videoFrameData->Length; i++) {

				if(videoFrameData[i] != nullptr) {

					delete videoFrameData[i];
					videoFrameData[i] = nullptr;
				}

			}

			for(int i = 0; i < audioFrameData->Length; i++) {

				if(audioFrameData[i] != nullptr) {

					delete audioFrameData[i];
					audioFrameData[i] = nullptr;
				}
			}

		}

		bool getFreeVideoFrame(VideoFrame ^%frame) {

			bool success = freeVideoFrames->tryGet(frame);

			return(success);
		}

		void enqueueFreeVideoFrame(VideoFrame ^frame) {

			freeVideoFrames->add(frame);
			Monitor::Exit(videoFrameInUseLock);

		}

		bool getDecodedVideoFrame(VideoFrame ^%videoFrame) {

			bool success = decodedVideoFrames->tryGet(videoFrame);

			if(success) {
		
				Monitor::Enter(videoFrameInUseLock);
			}

			return(success);
		}

		void enqueueDecodedVideoFrame(VideoFrame ^videoFrame) {

			decodedVideoFrames->add(videoFrame);

		}

		bool getFreeAudioFrame(AudioFrame ^%frame) {

			bool success = freeAudioFrames->tryGet(frame);
		
			return(success);
		}

		void enqueueFreeAudioFrame(AudioFrame ^frame) {

			freeAudioFrames->add(frame);
			Monitor::Exit(audioFrameInUseLock);
		}

		bool getDecodedAudioFrame(AudioFrame ^%audioFrame) {

			bool success = decodedAudioFrames->tryGet(audioFrame);

			if(success) {
				Monitor::Enter(audioFrameInUseLock);
			}
			return(success);
		}

		void enqueueDecodedAudioFrame(AudioFrame ^audioFrame) {

			decodedAudioFrames->add(audioFrame);

	
		}

	
	};
}