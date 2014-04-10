#pragma once
#include "VideoDecoder.h"

namespace VideoLib {

	public ref class Packet 
	{

		AVPacket *avPacket;

	public:

		property AVPacket *AVLibPacketData
		{
			AVPacket *get() {

				return(avPacket);
			}
		}
		
		Packet() {

			avPacket = new AVPacket();
			av_init_packet(avPacket);
			avPacket->data = NULL;
			avPacket->size = 0;
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

			if(avPacket->data != NULL) {

				av_free_packet(avPacket);
				av_init_packet(avPacket);
				avPacket->data = NULL;
				avPacket->size = 0;
			}
		}

	};
}