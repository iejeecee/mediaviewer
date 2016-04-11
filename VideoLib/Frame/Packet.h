#pragma once
#include "..\Video\VideoDecoder.h"

namespace VideoLib {

	public enum class PacketType
	{
		NORMAL_PACKET,
		LAST_PACKET
	};

	public ref class Packet 
	{

		AVPacket *avPacket;
		PacketType type;

		Packet(PacketType type) {

			this->type = type;
			avPacket = NULL;			
		}

	public:

		static Packet ^finalPacket;

		property AVPacket *AVLibPacketData
		{
			AVPacket *get() {

				return(avPacket);
			}
		}
		
		property PacketType Type
		{
			PacketType get() {

				return(type);
			}
		}
		
		static Packet() {

			finalPacket = gcnew Packet(PacketType::LAST_PACKET);
		}

		Packet() {

			avPacket = new AVPacket();
			av_init_packet(avPacket);
			avPacket->data = NULL;
			avPacket->size = 0;

			type = PacketType::NORMAL_PACKET;
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
		}

	};
	
}