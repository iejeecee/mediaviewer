#pragma once

#include "ThreadSafeQueue.h"
#include "VideoFrame.h"
#include "AudioFrame.h"

using namespace Microsoft::DirectX::Direct3D;
using namespace System::Collections::Generic;
using namespace System::Threading;

namespace VideoLib {

	public ref class FrameQueue
	{
	private:

		ThreadSafeQueue<VideoFrame ^> ^freeVideoFrames;
		ThreadSafeQueue<VideoFrame ^> ^decodedVideoFrames;

		ThreadSafeQueue<AudioFrame ^> ^freeAudioFrames;
		ThreadSafeQueue<AudioFrame ^> ^decodedAudioFrames;
		
		array<VideoFrame ^> ^videoFrameData;
		array<AudioFrame ^> ^audioFrameData;

		bool isStopped;

		void initializeAudioQueue(int samplesPerSecond, int bytesPerSample, int maxAudioFrameBufferSize) {

			int lineSize = 4608;

			int oneSecondAudioBytes = samplesPerSecond * bytesPerSample;
			int nrFrames = 500;//oneSecondAudioBytes / lineSize;

			audioFrameData = gcnew array<AudioFrame ^>(nrFrames);

			freeAudioFrames = gcnew ThreadSafeQueue<AudioFrame ^>(nrFrames);
			decodedAudioFrames = gcnew ThreadSafeQueue<AudioFrame ^>(nrFrames);

			for(int i = 0; i < nrFrames; i++) {

				audioFrameData[i] = gcnew AudioFrame(maxAudioFrameBufferSize);

				freeAudioFrames->add(audioFrameData[i]);
			}

		}

		void initializeVideoQueue(Device ^device, int width, int height, Format pixelFormat) {

			int nrFrames = 30;

			videoFrameData = gcnew array<VideoFrame ^>(nrFrames);

			freeVideoFrames = gcnew ThreadSafeQueue<VideoFrame ^>(nrFrames);
			decodedVideoFrames = gcnew ThreadSafeQueue<VideoFrame ^>(nrFrames);

			for(int i = 0; i < nrFrames; i++) {

				videoFrameData[i] = gcnew VideoFrame(device, width, height, pixelFormat);

				freeVideoFrames->add(videoFrameData[i]);
			}

		}

	public:

		FrameQueue() {
	
			isStopped = false;
			freeVideoFrames = nullptr;
			decodedVideoFrames= nullptr;

			freeAudioFrames= nullptr;
			decodedAudioFrames= nullptr;
		
			videoFrameData = nullptr;
			audioFrameData = nullptr;
		}


		property bool IsStopped {

			bool get() {

				return(isStopped);
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

		property int AudioQueueSize {

			int get() {

				return(decodedAudioFrames->QueueSize);
			}
		}
	
		void initialize(Device ^device, int width, int height, Format pixelFormat,
			int samplesPerSecond, int bytesPerSample, int maxAudioBufferSize) {

			dispose();

			initializeVideoQueue(device, width, height, pixelFormat);
			initializeAudioQueue(samplesPerSecond, bytesPerSample, maxAudioBufferSize);
		}

		void start() {

			freeVideoFrames->open();
			freeAudioFrames->open();
			decodedVideoFrames->open();
			decodedAudioFrames->open();
			isStopped = false;
		}

		void stop() {

			if(decodedVideoFrames != nullptr) {

				decodedVideoFrames->close();
			}
			if(decodedAudioFrames != nullptr) {

				decodedAudioFrames->close();
			}
			if(freeVideoFrames != nullptr) {

				freeVideoFrames->close();
			}
			if(freeAudioFrames != nullptr) {

				freeAudioFrames->close();			
			}

			isStopped = true;
		}

		void dispose() {

			if(videoFrameData == nullptr) return;
			if(audioFrameData == nullptr) return;

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

			return(freeVideoFrames->tryGet(frame));
		}

		void enqueueFreeVideoFrame(VideoFrame ^frame) {

			freeVideoFrames->add(frame);

		}

		bool getDecodedVideoFrame(VideoFrame ^%videoFrame) {

			return(decodedVideoFrames->tryGet(videoFrame));
		}

		void enqueueDecodedVideoFrame(VideoFrame ^videoFrame) {

			decodedVideoFrames->add(videoFrame);

		}

		bool getFreeAudioFrame(AudioFrame ^%frame) {

			return(freeAudioFrames->tryGet(frame));
		}

		void enqueueFreeAudioFrame(AudioFrame ^frame) {

			freeAudioFrames->add(frame);
			
		}

		bool getDecodedAudioFrame(AudioFrame ^%audioFrame) {

			return(decodedAudioFrames->tryGet(audioFrame));
		}

		void enqueueDecodedAudioFrame(AudioFrame ^audioFrame) {

			decodedAudioFrames->add(audioFrame);

	
		}

	
	};
}