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

		event EventHandler ^StartBuffering;		
		event EventHandler ^AddedPacket;
		event EventHandler ^IsFinished;

		enum class PacketQueueState {			
			OPEN,					// items can be added and removed from the queue
			BLOCK_START,			// block getpacket
			BLOCK_END,				// should not be set by user
			CLOSE_START,			// return from getpacket or addpacket in any state	
			CLOSE_END,				// should not be set by user
			PAUSE_START,			// return from getpacket 
			PAUSE_END,				// should not be set by user				
		};

		enum class GetResult {
			SUCCESS,
			CLOSED,
			PAUSED,
			FINAL,
			BUFFER
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
		
			void set(PacketQueueState value) {

				Monitor::Enter(lockObject);
				try {

					if(state != value) {
						
						if(value == PacketQueueState::CLOSE_START &&
							state == PacketQueueState::CLOSE_END) return;

						if(value == PacketQueueState::PAUSE_START &&
							state == PacketQueueState::PAUSE_END) return;

						if(value == PacketQueueState::BLOCK_START &&
							state == PacketQueueState::BLOCK_END) return;

						//System::Diagnostics::Debug::Print(id + ": " + value.ToString());

						state = value;
						Monitor::PulseAll(lockObject);
					}

				} finally {

					Monitor::Exit(lockObject);
				}
			}
		}
	
		PacketQueue(String ^id, int maxPackets, Object ^lockObject) {

			this->id = id;
			this->lockObject = lockObject;

			this->maxPackets = maxPackets;
			queue = gcnew Queue<Packet ^>();
			state = PacketQueueState::OPEN;
													
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
				
		// clear the queue
		void flush() {

			Monitor::Enter(lockObject);
			try {

				queue->Clear();				
				
			} finally {

				Monitor::Exit(lockObject);
			}
		}
		
	
		GetResult getPacket(Packet ^%packet) {

			Monitor::Enter(lockObject);
			try {
			
				while(queue->Count == 0 || State != PacketQueueState::OPEN) 
				{
					switch(State) 
					{
						case PacketQueueState::PAUSE_START:
							{
								State = PacketQueueState::PAUSE_END;							
							}
						case PacketQueueState::PAUSE_END:
							{
								packet = nullptr;																							
								return(GetResult::PAUSED);		
							}
						case PacketQueueState::CLOSE_START:
							{
								State = PacketQueueState::CLOSE_END;						
							}											
						case PacketQueueState::CLOSE_END:
							{																			
								packet = nullptr;																							
								return(GetResult::CLOSED);								
							}										
						case PacketQueueState::BLOCK_START:
							{
								State = PacketQueueState::BLOCK_END;									
							}	
						case PacketQueueState::BLOCK_END:
							{
								break;
							}
						case PacketQueueState::OPEN:
							{
								if(queue->Count == 0) 
								{			
									StartBuffering(this, EventArgs::Empty);
									
									if(State == PacketQueueState::PAUSE_START) {

										packet = nullptr;																							
										return(GetResult::BUFFER);	
									}
								}
								break;
							}
					}
					
					Monitor::Wait(lockObject);					
				}

				packet = queue->Dequeue();
			
				if(queue->Count == maxPackets - 1) {

					// wake threads waiting on full queue
					Monitor::PulseAll(lockObject);
				}

				if(packet->Type == PacketType::LAST_PACKET) {
					
					State = PacketQueueState::CLOSE_END;
					IsFinished(this,EventArgs::Empty);
					return(GetResult::FINAL);
				}

				return(GetResult::SUCCESS);

			} finally {
				
				Monitor::Exit(lockObject);
				
			}
		}

		void addPacket(Packet ^packet) {
						
			Monitor::Enter(lockObject);
			try {
				
				while(queue->Count >= maxPackets) {

					switch(State) {
						
						case PacketQueueState::BLOCK_START:
							{

							}
						case PacketQueueState::BLOCK_END:
							{

							}
						case PacketQueueState::CLOSE_END:
							{								
								return;					
							}
						default:
							{
								
							}
					}
						
					Monitor::Wait(lockObject);
				}
		
				queue->Enqueue(packet);

				AddedPacket(this,EventArgs::Empty);
												 
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