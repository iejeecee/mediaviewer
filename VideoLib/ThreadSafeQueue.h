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
		bool stopped;
		bool closing;

	public:

		ThreadSafeQueue(int maxQueueSize) {

			this->maxQueueSize = maxQueueSize;
			queue = gcnew Queue<T>();
			closing = false;
			stopped = false;
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

		property bool Closing {

			bool get() {

				return(closing);
			}
		}

		property bool Stopped {

			bool get() {

				return(stopped);
			}
		}

		// put the queue into ready to start mode
		void open() {

			Monitor::Enter(queue);
			
			stopped = false;
			closing = false;
			Monitor::PulseAll(queue);

			Monitor::Exit(queue);
		}

		// stop the queue immediatly
		void stop() {

			Monitor::Enter(queue);
			
			stopped = true;
			// wakeup waiting threads
			Monitor::PulseAll(queue);

			Monitor::Exit(queue);

		}

		void flush() {

			Monitor::Enter(queue);

			queue->Clear();

			Monitor::Exit(queue);
		}

		// let the queue run until it is empty
		void close()
		{
			Monitor::Enter(queue);
			
			closing = true;
			// wakeup waiting threads
			Monitor::PulseAll(queue);

			Monitor::Exit(queue);
		}

		bool tryGet(T %item) {

			Monitor::Enter(queue);

			try {

				while(queue->Count == 0) {

					if(closing == true || stopped == true) {

						return(false);
					}

					// queue is empty put thread into wait state
					// threads in a wait state need a explicit pulse 
					// to wake them up 
					Monitor::Wait(queue);
				}

				if(stopped == true) return(false);

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