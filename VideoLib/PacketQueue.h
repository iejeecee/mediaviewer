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

		enum class PacketQueueState {
			OPEN,       // items can be added and removed from the queue
			PAUSE,		// queue will block on removing items
			PAUSED,
			STOP,		// queue will return false on removing items			
			STOPPED
		};

	private:

		String ^id;
		Queue<Packet ^> ^queue;
		int maxPackets;

		Object ^lockObject;

		PacketQueueState state;

	public:

		property PacketQueueState State {

			PacketQueueState get() {

				return(state);
			}

		private:
			void set(PacketQueueState value) {

				state = value;
			}
		}
	
		PacketQueue(String ^id, int maxPackets, Object ^lockObject) {

			this->id = id;
			this->lockObject = lockObject;

			this->maxPackets = maxPackets;
			queue = gcnew Queue<Packet ^>();
			State = PacketQueueState::OPEN;
			
		}

	
		~PacketQueue() {

		
		}
				
		property int MaxPackets {

			int get() {

				return(maxPackets);
			}
		}

		property int NrPacketsInQueue {

			int get() {

				return(queue->Count);
			}
		}

	
		// allow threads to remove elements from the queue
		void open() {

			Monitor::Enter(queue);
			
			// wakeup paused threads
			State = PacketQueueState::OPEN;
	
			Monitor::PulseAll(queue);
			Monitor::Exit(queue);
		}

		// threads are not able to remove elements from the queue
		// the get functions will not block, but instead return false
		void stop() {

			Monitor::Enter(queue);
			
			State = PacketQueueState::STOP;
			

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

			State = PacketQueueState::PAUSE;
			// wakeup threads waiting on empty queues
			// and put them in paused state
			Monitor::PulseAll(queue);

			Monitor::Exit(queue);
		}
	
		bool getPacket(Packet ^%packet) {

			Monitor::Enter(lockObject);
			try {
			
				while(queue->Count == 0 || 
					State == PacketQueueState::STOP ||
					State == PacketQueueState::STOPPED ||
					State == PacketQueueState::PAUSE ||
					State == PacketQueueState::PAUSED) 
				{
					
					if(State == PacketQueueState::STOP || State == PacketQueueState::STOPPED) 
					{
						packet = nullptr;

						State = PacketQueueState::STOPPED;																		
						return(false);
					} 
					else if(State == PacketQueueState::PAUSE)
					{
						State = PacketQueueState::PAUSED;						
						Monitor::PulseAll(lockObject);						
					}

					Monitor::Wait(lockObject);
				}

				packet = queue->Dequeue();

				if(queue->Count == maxPackets - 1) {
					// wake threads waiting on full queue
					Monitor::PulseAll(lockObject);
				}

				if(packet->Type == PacketType::LAST_PACKET) {

					State = PacketQueueState::STOPPED;				
					return(false);
				}

				return(true);

			} finally {
				
				Monitor::Exit(lockObject);

				/*if(packet != nullptr && packet->Type == PacketType::LAST_PACKET) {
				
					if(packetQueueStopped[(int)QueueID::AUDIO_PACKETS] && 
						packetQueueStopped[(int)QueueID::VIDEO_PACKETS])
					{
						Finished(this, EventArgs::Empty);
					}

				}*/

			}
		}

		void addPacket(Packet ^packet) {
						
			Monitor::Enter(lockObject);
			try {
				
				while(queue->Count >= maxPackets) {

					if(State == PacketQueueState::STOPPED) {	
										
						// make sure the packet isn't lost when the queue is stopped
						queue->Enqueue(packet);
						return;
					} 
					else if(State == PacketQueueState::PAUSED)
					{
						// make sure the packet IS discarded because a pause should always be followed by a flush
						// e.g. when seeking in the stream
						return;
					}				

					Monitor::Wait(lockObject);
				}
		
				queue->Enqueue(packet);

				if(queue->Count == 1) {
					// wake threads waiting on empty queue
					Monitor::PulseAll(lockObject);
				}
								
			} finally {
		
				Monitor::Exit(lockObject);
			}
		}

		

	};
}