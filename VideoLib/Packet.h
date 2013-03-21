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
		}
		
		~Packet() {

			av_free_packet(avPacket);
			delete avPacket;
		}

	};
}