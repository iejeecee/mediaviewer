// VideoLib.h

#pragma once
#include "VideoFrameGrabber.h"

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

	public ref class VideoMetaData
	{
	private:

		int durationSeconds;

		__int64 sizeBytes;

		int width;
		int height;

		String ^container;
		String ^videoCodecName;
		List<String ^> ^metaData;

		float frameRate;

	public:

		VideoMetaData(int durationSeconds,
			__int64 sizeBytes, int width, int height, String ^container,
			String ^videoCodecName, List<String ^> ^metaData, float frameRate)
		{

			this->durationSeconds = durationSeconds;
			this->sizeBytes = sizeBytes;
			this->width = width;
			this->height = height;
			this->container = container;
			this->videoCodecName = videoCodecName;
			this->frameRate = frameRate;
			this->metaData = metaData;

		}

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

		property int DurationSeconds {

			int get() {

				return(durationSeconds);
			}
		}

		property __int64 SizeBytes {

			__int64 get() {

				return(sizeBytes);
			}
		}

		property String ^Container {

			String ^get() {

				return(container);
			}
		}

		property String ^VideoCodecName {

			String ^get() {

				return(videoCodecName);
			}
		}

		property List<String ^> ^MetaData {

			List<String ^> ^get() {

				return(metaData);
			}
		}

		property float FrameRate {

			float get() {

				return(frameRate);
			}
		}


	};


	public ref class VideoPreview
	{

	private:

		VideoFrameGrabber *frameGrabber;

	public:

		VideoPreview();
		~VideoPreview();

		void open(String ^videoLocation);
		void close();

		VideoMetaData ^readMetaData();

		// TODO: Add your methods for this class here.
		List<RawImageRGB24 ^> ^grabThumbnails(int maxThumbWidth, int maxThumbHeight, 
			int captureInterval, int nrThumbs);

		
	};
}
