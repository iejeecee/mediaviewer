#pragma once
// http://stackoverflow.com/questions/530211/creating-a-blocking-queuet-in-net/530228#530228

#include "VideoFrame.h"

using namespace Microsoft::DirectX::Direct3D;
using namespace System::Collections::Generic;
using namespace System::Threading;

namespace VideoLib {

	generic<typename T> public ref class ThreadSafeQueue
	{
	public:

		enum class State {
			OPEN,       // items can be added and removed from the queue
			PAUSED,		// queue will block on removing items
			STOPPED,	// queue will return false on removing items
			CLOSED		// queue will fire event when empty
		};

	private:

		Queue<T> ^queue;
		int maxQueueSize;

		State queueState;

		AutoResetEvent ^paused;
		AutoResetEvent ^stopped;

	public:

		event EventHandler ^Closed;

		ThreadSafeQueue(int maxQueueSize) {

			this->maxQueueSize = maxQueueSize;
			queue = gcnew Queue<T>();
			queueState = State::OPEN;

			paused = gcnew AutoResetEvent(false);
			stopped = gcnew AutoResetEvent(false);
		}

		property AutoResetEvent ^Paused {

			AutoResetEvent ^get() {

				return(paused);
			}
		}

		property AutoResetEvent ^Stopped {

			AutoResetEvent ^get() {

				return(stopped);
			}
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

		property State QueueState {

			State get() {

				return(queueState);
			}
		}

		// allow threads to remove elements from the queue
		void open() {

			Monitor::Enter(queue);
			
			// wakeup paused threads
			queueState = State::OPEN;
	
			Monitor::PulseAll(queue);
			Monitor::Exit(queue);
		}

		// threads are not able to remove elements from the queue
		// the get functions will not block, but instead return false
		void stop() {

			Monitor::Enter(queue);
			
			queueState = State::STOPPED;
			// wakeup threads waiting on empty queues
			Monitor::PulseAll(queue);

			Monitor::Exit(queue);

		}

		// clear the queue
		void flush() {

			Monitor::Enter(queue);

			queue->Clear();

			Monitor::Exit(queue);
		}

		void pause() {

			Monitor::Enter(queue);

			queueState = State::PAUSED;
			// wakeup threads waiting on empty queues
			// and put them in paused state
			Monitor::PulseAll(queue);

			Monitor::Exit(queue);
		}

		void close() {

			Monitor::Enter(queue);

			queueState = State::CLOSED;
			// wakeup threads waiting on empty queues
			Monitor::PulseAll(queue);

			Monitor::Exit(queue);
		}


		bool tryGet(T %item) {

			Monitor::Enter(queue);

			try {

				while(queue->Count == 0 || QueueState == State::PAUSED) {

					if(QueueState == State::STOPPED) {

						stopped->Set();
						return(false);

					} else if(QueueState == State::PAUSED) {

						paused->Set();

					} else if(QueueState == State::CLOSED) {

						Closed(this, EventArgs::Empty);
					}

					// queue is empty put thread into wait state
					// threads in a wait state need a explicit pulse 
					// to wake them up 
					Monitor::Wait(queue);
				}

				if(QueueState == State::STOPPED) {
					
					stopped->Set();
					return(false);
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

				if(queue->Count == 1 && QueueState != State::PAUSED) {

					// wake up any thread which was send sleeping
					// trying to take something from a empty queue.
					// Do not wake up a paused queue because the paused 
					// autoresetevent should only be set in the tryGet function
					Monitor::PulseAll(queue);
	
				} 

			} finally {
				Monitor::Exit(queue);
			}
		}

	};
}