#include "VideoFrameGrabber.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Windows::Media::Imaging;
using namespace System::Threading;

namespace VideoLib {

	public ref class VideoThumb 
	{
		BitmapSource ^thumb;
		long positionSeconds;

	public: 

		VideoThumb(BitmapSource ^thumb, long positionSeconds);
		
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


	public ref class VideoPreview
	{
	public:

		delegate void DecodedFrameProgressDelegate(VideoThumb ^thumb);

	private:
		
		VideoFrameGrabber *frameGrabber;
		System::Runtime::InteropServices::GCHandle gch;

		delegate void DecodedFrameDelegate(void *data, AVPacket *packet, AVFrame *frame, Video::FrameType type);

		void decodedFrameCallback(void *data, AVPacket *packet, 
			AVFrame *frame, Video::FrameType type);

		List<VideoThumb ^> ^thumbs;

		int durationSeconds;

		__int64 sizeBytes;

		int width;
		int height;

		String ^container;
		String ^videoCodecName;
		String ^pixelFormat;
		List<String ^> ^metaData;

		float frameRate;

		String ^audioCodecName;
		int samplesPerSecond;
		int bytesPerSample;
		int nrChannels;

		DecodedFrameProgressDelegate ^decodedFrameProgressCallback;

	public:
		
		VideoPreview();
		~VideoPreview();

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


		void open(String ^videoLocation);
		void close();

		List<VideoThumb ^> ^grabThumbnails(int thumbWidth, 
			int captureInterval, int nrThumbs, double startOffset, 
			System::Threading::CancellationToken ^cancellationToken, DecodedFrameProgressDelegate ^decodedFrameProgressCallback);

		List<VideoThumb ^> ^grabThumbnails(int maxThumbWidth, int maxThumbHeight, 
			int captureInterval, int nrThumbs, double startOffset, System::Threading::CancellationToken ^cancellationToken, int timeoutSeconds);

		
	};
}