#pragma once
#include "VideoDecoder.h"
#include "DashVideoDecoder.h"

namespace VideoLib {

	class VideoDecoderFactory {

	public:
		static IVideoDecoder *create(OpenVideoArgs ^args, IVideoDecoder *oldDecoder = NULL) {

			if(args->VideoLocation != nullptr && args->AudioLocation != nullptr) {

				DashVideoDecoder *dashDecoder = dynamic_cast<DashVideoDecoder *>(oldDecoder);

				if(dashDecoder == NULL) {

					if(oldDecoder != NULL) {

						VideoDecoder *normalDecoder = (VideoDecoder *)oldDecoder;

						delete normalDecoder;
					}

					return new DashVideoDecoder();

				} else {

					return oldDecoder;
				}

			} else if(args->VideoLocation != nullptr || args->AudioLocation != nullptr) {

				VideoDecoder *normalDecoder = dynamic_cast<VideoDecoder *>(oldDecoder);

				if(normalDecoder == NULL) {

					if(oldDecoder != NULL) {

						DashVideoDecoder *dashDecoder = (DashVideoDecoder *)oldDecoder;

						delete dashDecoder;
					}

					return new VideoDecoder();

				} else {

					return oldDecoder;
				}

			} else {

				throw gcnew VideoLibException("incorrect OpenVideoArgs");
			}

		}

	};

}