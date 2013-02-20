#pragma once
// http://stackoverflow.com/questions/530211/creating-a-blocking-queuet-in-net/530228#530228

#include "VideoFrame.h"

using namespace Microsoft::DirectX::Direct3D;
using namespace System::Collections::Generic;
using namespace System::Threading;

namespace VideoLib {

	generic<typename T> public ref class ThreadSafeQueue
	{
	private:

		Queue<T> ^queue;
		int maxQueueSize;
		bool closing;

	public:

		ThreadSafeQueue(int maxQueueSize) {

			this->maxQueueSize = maxQueueSize;
			queue = gcnew Queue<T>();
			closing = false;
		}

		property int MaxQueueSize {

			int get() {

				return(maxQueueSize);
			}
		}

		property int QueueSize {

			int get() {

				return(queue->Count);
			}
		}

		void open() {

			Monitor::Enter(queue);
			
			closing = false;

			Monitor::Exit(queue);
		}

		void close()
		{
			Monitor::Enter(queue);
			
			closing = true;
			Monitor::PulseAll(queue);

			Monitor::Exit(queue);
		}

		bool tryGet(T %item) {

			Monitor::Enter(queue);

			try {

				while(queue->Count == 0) {

					if(closing == true) {

						return(false);
					}

					// queue is empty put thread into wait state
					// threads in a wait state need a explicit pulse 
					// to wake them up 
					Monitor::Wait(queue);
				}

				item = queue->Dequeue();

				if(queue->Count == maxQueueSize - 1) {

					// wake up any thread which was waiting 
					// on a full queue
					Monitor::PulseAll(queue);
				}

				return(true);

			} finally {

				Monitor::Exit(queue);
			}
		}

		void add(T item) {

			Monitor::Enter(queue);

			try {

				while(queue->Count >= maxQueueSize) {

					// queue is full
					Monitor::Wait(queue);
				}

				queue->Enqueue(item);

				if(queue->Count == 1) {

					// wake up any thread which was send sleeping
					// trying to take something from a empty queue
					Monitor::PulseAll(queue);
				}

			} finally {
				Monitor::Exit(queue);
			}
		}

		
	};
}