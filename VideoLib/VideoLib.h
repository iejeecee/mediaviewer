// VideoLib.h

#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;

namespace VideoLib {

	public ref class RawImageRGB24
	{
	private:

		int width;
		int height;

		int timeStampSeconds;
		
		cli::array<unsigned char> ^data;

	public:

		property int Width {

			int get() {

				return(width);
			}
		}

		property int Height {

			int get() {

				return(height);
			}
		}

		property int TimeStampSeconds {

			int get() {

				return(timeStampSeconds);
			}
		}

		property cli::array<unsigned char> ^Data {

			cli::array<unsigned char> ^get() {

				return(data);
			}
		}

		RawImageRGB24(int width, int height, int timeStampSeconds, cli::array<unsigned char> ^data) {

			this->width = width;
			this->height = height;
			this->data = data;
		}

		~RawImageRGB24() {

			if(data != nullptr) {
				delete data;
				data = nullptr;
			}
		}

	};


	public ref class VideoPreview
	{

	public:
		// TODO: Add your methods for this class here.
		List<RawImageRGB24 ^> ^grab(String ^videoLocation, int maxThumbWidth, int maxThumbHeight, 
			int captureInterval, int nrThumbs);
	};
}
