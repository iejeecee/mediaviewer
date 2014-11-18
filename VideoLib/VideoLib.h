// VideoLib.h
#include "VideoFrameGrabber.h"
#include "VideoDecoder.h"
#include "VideoFrame.h"
//#include "FrameQueue.h"
#include "FrameQueue2.h"

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

		delegate void DecodedFrameDelegate(
		void *data, AVPacket *packet, AVFrame *frame, Video::FrameType type);

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
			int captureInterval, int nrThumbs, double startOffset);

		
	};

	
	public ref class VideoPlayer
	{

	private:

		VideoDecoder *videoDecoder;
		
		FrameQueue ^frameQueue;

		String ^videoLocation;

		System::Runtime::InteropServices::GCHandle gch;

		bool isFinalPacketAdded;

		

	public:

		enum class DemuxPacketsResult {
			LAST_PACKET,
			SUCCESS,
			STOPPED
		};


		enum class DecodedVideoFormat {
			YUV420P,
			ARGB,     
			RGBA,      
			ABGR,     
			BGRA
		};

		enum class DecodeMode {

			DECODE_VIDEO_AND_AUDIO,
			DECODE_VIDEO_ONLY,
			DECODE_AUDIO_ONLY
		};

		enum class LogLevel {

			LOG_LEVEL_FATAL = AV_LOG_FATAL,
			LOG_LEVEL_ERROR = AV_LOG_ERROR,		
			LOG_LEVEL_WARNING = AV_LOG_WARNING,
			LOG_LEVEL_INFO = AV_LOG_INFO,
			LOG_LEVEL_DEBUG = AV_LOG_DEBUG,
			
		};

		delegate void LogCallbackDelegate(
			int level, String ^message);

		property VideoLib::FrameQueue ^FrameQueue {

			VideoLib::FrameQueue ^get() {

				return(frameQueue);
			}
		}

		property String ^VideoLocation {

			String ^get() {

				return(videoLocation);
			}
		}

		property int DurationSeconds {

			int get() {

				return(videoDecoder->getDurationSeconds());
			}
		}

		property int Height {

			int get() {

				return(videoDecoder->getHeight());
			}
		}

		property int Width {

			int get() {

				return(videoDecoder->getWidth());
			}
		}

		property int SamplesPerSecond {

			int get() {

				return(videoDecoder->getAudioSamplesPerSecond());
			}
		}

		property int BytesPerSample {

			int get() {

				return(videoDecoder->getAudioBytesPerSample());
			}
		}

		property int NrChannels {

			int get() {

				return(videoDecoder->getAudioNrChannels());
			}
		}

		property bool HasAudio {

			bool get() {

				return(videoDecoder->hasAudio());
			}
		}

		property int MaxAudioFrameSize {

			int get() {

				return(AVCODEC_MAX_AUDIO_FRAME_SIZE);
			}
		}

		property double TimeNow {

			double get() {

				double time = av_gettime() / 1000000.0;
				return(time);
			}

		}

		property double TimeBase {

			double get() {

				return(av_q2d(videoDecoder->getVideoStream()->time_base));
			}
		}

		event EventHandler<EventArgs ^> ^DecodeException;

		VideoPlayer();
		~VideoPlayer();

		void open(String ^videoLocation, DecodedVideoFormat format);
		bool seek(double posSeconds);
		DemuxPacketsResult demuxPacket();
		void close();

		void setLogCallback(LogCallbackDelegate ^callback, bool enableLibAVLogging,
			LogLevel level);
		
		static int getAvFormatVersion();
		static String ^getAvFormatBuildInfo();
	};
}
