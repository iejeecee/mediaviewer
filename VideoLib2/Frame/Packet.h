#pragma once
#include "..\Video\VideoDecoder.h"

namespace VideoLib2 {
	

	public ref class Packet 
	{

		AVPacket *avPacket;
		bool isFinalPacket;
		
	public:
		
		property AVPacket *AVLibPacketData
		{
			AVPacket *get() {

				return(avPacket);
			}
		}
		
		property bool IsFinalPacket
		{
			bool get() {

				return(isFinalPacket);
			}

			void set(bool value) {

				isFinalPacket = value;
			}
		}
	
			
		Packet() {

			avPacket = new AVPacket();
			av_init_packet(avPacket);
			avPacket->data = NULL;
			avPacket->size = 0;

			isFinalPacket = false;
		}
		
		!Packet() {

			free();

			if(avPacket != NULL) {

				delete avPacket;
				avPacket = NULL;
			}
		}

		~Packet() {

			this->!Packet();					
		}

		void free() {

			if(avPacket != NULL && avPacket->data != NULL) {

				av_packet_unref(avPacket);
				av_init_packet(avPacket);
				avPacket->data = NULL;
				avPacket->size = 0;				
			}

			isFinalPacket = false;
		}
	};
}