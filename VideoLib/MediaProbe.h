#pragma once
#include "Video\VideoFrameGrabber.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Windows::Media::Imaging;
using namespace System::Threading;

namespace VideoLib {

	public ref class MediaThumb 
	{
		BitmapSource ^thumb;
		long positionSeconds;

	public: 

		MediaThumb(BitmapSource ^thumb, long positionSeconds);
		
		property BitmapSource ^Thumb {

			BitmapSource ^get() {

				return(thumb);
			}
		}
		
		property long PositionSeconds {

			long get() {

				return(positionSeconds);
			}
		}
		
	};


	public ref class MediaProbe
	{
	public:

		delegate void DecodedFrameProgressDelegate(MediaThumb ^thumb);

	private:
		
		VideoFrameGrabber *frameGrabber;
		System::Runtime::InteropServices::GCHandle gch;

		delegate void DecodedFrameDelegate(void *data, AVPacket *packet, AVFrame *frame, Video::FrameType type);

		void decodedFrameCallback(void *data, AVPacket *packet, 
			AVFrame *frame, Video::FrameType type);

		void UTF8ToWString(const std::string &input, String ^%output);

		List<MediaThumb ^> ^thumbs;

		int durationSeconds;

		__int64 sizeBytes;

		int width;
		int height;

		String ^container;
		String ^videoCodecName;
		String ^pixelFormat;
		int bitsPerPixel;
		List<String ^> ^metaData;

		float frameRate;

		String ^audioCodecName;
		int samplesPerSecond;
		int bytesPerSample;
		int nrChannels;

		bool isVideo, isAudio, isImage;

		DecodedFrameProgressDelegate ^decodedFrameProgressCallback;

	public:
		
		MediaProbe();
		~MediaProbe();

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

		property String ^PixelFormat {

			String ^get() {

				return(pixelFormat);
			}
		}

		property int BitsPerPixel {

			int get() {

				return(bitsPerPixel);
			}
		}

		property String ^AudioCodecName {

			String ^get() {

				return(audioCodecName);
			}
		}

		property int SamplesPerSecond {

			int get() {

				return(samplesPerSecond);
			}
		}

		property int BytesPerSample {

			int get() {

				return(bytesPerSample);
			}
		}

		property int NrChannels {

			int get() {

				return(nrChannels);
			}
		}

		property bool IsVideo {

			bool get() {

				return(isVideo);
			}
		}

		property bool IsAudio {

			bool get() {

				return(isAudio);
			}
		}

		property bool IsImage {

			bool get() {

				return(isImage);
			}
		}


		void open(String ^mediaLocation, System::Threading::CancellationToken cancellationToken);
		void close();
		
		// If captureintervalseconds > 0 a frame will be captured every captureIntervalSeconds 
		// else a total of nrThumbs will be returned.
		// The first thumbnail is captured at duration * startOffset
		List<MediaThumb ^> ^grabThumbnails(int maxThumbWidth, int maxThumbHeight, 
			double captureIntervalSeconds, int nrThumbs, double startOffset, System::Threading::CancellationToken cancellationToken, int timeoutSeconds,
			DecodedFrameProgressDelegate ^decodedFrameProgressCallback);

		
	};
}