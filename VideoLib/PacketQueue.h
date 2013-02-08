#pragma once

#include "SafeQueue.h"
#include "VideoFrame.h"


using namespace Microsoft::DirectX::Direct3D;
using namespace System::Collections::Generic;
using namespace System::Threading;

namespace VideoLib {

	public ref class PacketQueue
	{
	private:

		SafeQueue<VideoFrame ^> ^freeFrames;
		SafeQueue<VideoFrame ^> ^decodedFrames;

		array<VideoFrame ^> ^frameData;
		int nrFramesInBuffer;

		bool isStopped;
	public:

		property bool IsStopped {

			bool get() {

				return(isStopped);
			}
		}

		PacketQueue(int nrFramesInBuffer) {

			this->nrFramesInBuffer = nrFramesInBuffer;
			frameData = gcnew array<VideoFrame ^>(nrFramesInBuffer);
			isStopped = false;
		}

		void initialize(Device ^device, int width, int height, Format pixelFormat) {

			decodedFrames = gcnew SafeQueue<VideoFrame ^>(nrFramesInBuffer);
			freeFrames = gcnew SafeQueue<VideoFrame ^>(nrFramesInBuffer);

			dispose();

			for(int i = 0; i < frameData->Length; i++) {

				frameData[i] = gcnew VideoFrame(device, width, height,
					pixelFormat);

				freeFrames->add(frameData[i]);			
			}

		}

		void start() {

			freeFrames->open();
			decodedFrames->open();
			isStopped = false;
		}

		void stop() {

			freeFrames->close();
			decodedFrames->close();
			isStopped = true;
		}

		void dispose() {

			for(int i = 0; i < frameData->Length; i++) {

				if(frameData[i] != nullptr) {

					delete frameData[i];
					frameData[i] = nullptr;
				}
			}

		}

		bool getFreeFrame(VideoFrame ^%frame) {

			return(freeFrames->tryGet(frame));
		}

		void queueFreeFrame(VideoFrame ^frame) {

			freeFrames->add(frame);
			
		}

		bool getDecodedFrame(VideoFrame ^%frame) {

			return(decodedFrames->tryGet(frame));
		}

		void queueDecodedFrame(VideoFrame ^frame) {

			decodedFrames->add(frame);
		}
	};
}