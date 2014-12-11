#pragma once
// http://stackoverflow.com/questions/530211/creating-a-blocking-queuet-in-net/530228#530228

#include "VideoFrame.h"
#include "Packet.h"

using namespace System::Collections::Generic;
using namespace System::Threading;

namespace VideoLib {

	public ref class PacketQueue 
	{
	public:

		enum class State {
			OPEN,       // items can be added and removed from the queue
			PAUSED,		// queue will block on removing items
			STOPPED	// queue will return false on removing items			
		};

	private:

		Queue<Packet ^> ^queue;
		int maxQueueSize;

		State queueState;

		AutoResetEvent ^paused;
		AutoResetEvent ^stopped;
		bool isStopSignalled; 

		char *name;

		void writeDebug(char *info) {
			
		    char buffer[100];

			strcpy(buffer,name);
			strcat(buffer," ");
			strcat(buffer,info);
			
			Video::writeToLog(AV_LOG_DEBUG, buffer);			 
		}

	public:

		event EventHandler ^Finished;

		PacketQueue(char *name, int maxQueueSize) {

			this->name = name;

			this->maxQueueSize = maxQueueSize;
			queue = gcnew Queue<Packet ^>();
			queueState = State::OPEN;

			paused = gcnew AutoResetEvent(false);
			stopped = gcnew AutoResetEvent(false);
			isStopSignalled = false;
		}

	

		~PacketQueue() {

			if(paused != nullptr) {
			
				delete paused;
				paused = nullptr;
			}

			if(stopped != nullptr) {

				delete stopped;
				stopped = nullptr;
			}
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
			writeDebug("Packet queue stopped called");
			isStopSignalled = false;

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
		// block threads attempting to remove elements from the queue
		void pause() {

			Monitor::Enter(queue);

			queueState = State::PAUSED;
			// wakeup threads waiting on empty queues
			// and put them in paused state
			Monitor::PulseAll(queue);

			Monitor::Exit(queue);
		}
	
		bool tryGet(Packet ^%item) {

			Monitor::Enter(queue);

			try {

				while(queue->Count == 0 || QueueState == State::PAUSED || QueueState == State::STOPPED) {

					if(QueueState == State::STOPPED) {
						
						if(!isStopSignalled) {
							// only set the stopped signal once whenever the stop function has been called
							// otherwise repeated calls to tryget in stopped state will invalidate the autoresetevent						
							stopped->Set();
							writeDebug("Packet queue stopped, stopped set");
							isStopSignalled = true;
						} 

						return(false);

					} else if(QueueState == State::PAUSED) {

						paused->Set();
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

			    // stop the queue if this is the last packet
				if(item->Type == PacketType::LAST_PACKET) {

					queueState = State::STOPPED;
					writeDebug("Packet queue last packet, stopped set");
					stopped->Set();
					// wakeup waiting threads
					Monitor::PulseAll(queue);				
					return(false);
				}

				return(true);

			} finally {

				Monitor::Exit(queue);

				if(item != nullptr && item->Type == PacketType::LAST_PACKET) {
					Finished(this, EventArgs::Empty);
				}
			}
		}

		void add(Packet ^item) {

			Monitor::Enter(queue);

			try {

				while(queue->Count >= maxQueueSize) {

					// queue is full
					if(QueueState == State::STOPPED) {	
					
						return;
					}

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