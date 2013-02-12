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
		ThreadSafeQueue<AudioFrame ^> ^freeAudioFrames;
		ThreadSafeQueue<Frame ^> ^decodedFrames;

		array<VideoFrame ^> ^videoFrameData;
		array<AudioFrame ^> ^audioFrameData;
		int maxQueueSize;

		bool isStopped;

	public:

		property bool IsStopped {

			bool get() {

				return(isStopped);
			}
		}

		property int MaxQueueSize {

			int get() {

				return(maxQueueSize);
			}
		}

		FrameQueue(int maxQueueSize) {

			this->maxQueueSize = maxQueueSize;
			videoFrameData = gcnew array<VideoFrame ^>(maxQueueSize);
			audioFrameData = gcnew array<AudioFrame ^>(maxQueueSize);
			isStopped = false;
		}

		void initialize(Device ^device, int width, int height, Format pixelFormat,
			int audioFrameBufferSize) {

			decodedFrames = gcnew ThreadSafeQueue<Frame ^>(maxQueueSize * 2);
			freeVideoFrames = gcnew ThreadSafeQueue<VideoFrame ^>(maxQueueSize);
			freeAudioFrames = gcnew ThreadSafeQueue<AudioFrame ^>(maxQueueSize);

			dispose();

			for(int i = 0; i < videoFrameData->Length; i++) {

				videoFrameData[i] = gcnew VideoFrame(device, width, height,
					pixelFormat);

				audioFrameData[i] = gcnew AudioFrame(audioFrameBufferSize);

				freeVideoFrames->add(videoFrameData[i]);	
				freeAudioFrames->add(audioFrameData[i]);
			}

		}

		void start() {

			freeVideoFrames->open();
			freeAudioFrames->open();
			decodedFrames->open();
			isStopped = false;
		}

		void stop() {

			decodedFrames->close();
			freeVideoFrames->close();
			freeAudioFrames->close();			
			isStopped = true;
		}

		void dispose() {

			for(int i = 0; i < videoFrameData->Length; i++) {

				if(videoFrameData[i] != nullptr) {

					delete videoFrameData[i];
					videoFrameData[i] = nullptr;
				}

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

		bool getFreeAudioFrame(AudioFrame ^%frame) {

			return(freeAudioFrames->tryGet(frame));
		}

		void enqueueFreeAudioFrame(AudioFrame ^frame) {

			freeAudioFrames->add(frame);
			
		}

		bool getDecodedFrame(Frame ^%frame) {

			return(decodedFrames->tryGet(frame));
		}

		void enqueueDecodedFrame(Frame ^frame) {

			decodedFrames->add(frame);
		}
	};
}