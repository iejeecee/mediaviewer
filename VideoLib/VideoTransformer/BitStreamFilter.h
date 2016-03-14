#pragma once
#pragma warning(disable : 4244)
// unsafe function warning disable
#pragma warning(disable : 4996)
#include <algorithm>
#include "stdafx.h"
#include "..\Video\VideoInit.h"
#include "..\VideoLibException.h"
#include <msclr\marshal.h>

using namespace msclr::interop;

namespace VideoLib {

class BitStreamFilter {

protected:

	std::vector<AVBitStreamFilterContext *> filter;
	


public:

	BitStreamFilter() {

		

	}

	~BitStreamFilter() {

		for(unsigned int i = 0; i < filter.size(); i++) {

			av_bitstream_filter_close(filter[i]);
			filter[i] = NULL;
		}

		filter.clear();
	}

	void add(const std::string &name) {

		AVBitStreamFilterContext *newFilter = av_bitstream_filter_init(name.c_str());
		if(newFilter == NULL) {

			throw gcnew VideoLibException("Could not create bitstream filter: " + marshal_as<String ^>(name));
		}

		filter.push_back(newFilter);

	}

	void filterPacket(AVPacket *packet, AVCodecContext *outputCodecContext) {

		for(unsigned int i = 0; i < filter.size(); i++) 
		{
			AVPacket new_pkt = *packet;

			int result = av_bitstream_filter_filter(filter[i], outputCodecContext, NULL,
				&new_pkt.data, &new_pkt.size,
				packet->data, packet->size,
				packet->flags & AV_PKT_FLAG_KEY);
			if (result > 0) {

				av_packet_unref(packet);
				new_pkt.buf = av_buffer_create(new_pkt.data, new_pkt.size, av_buffer_default_free, NULL, 0);
				if (!new_pkt.buf) {

					throw gcnew VideoLibException("Could not create packet buffer for bitstream filter");
				}
					
			} else if (result < 0) {

				throw gcnew VideoLibException("Error applying bitstream filter");			
				
			}

			*packet = new_pkt;		
		}

	}



};



}
