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

		delegate void LogCallbackDelegate(int level, String ^message);

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

				return(videoDecoder->getAudioConvertSampleRate());
			}
		}

		property int BytesPerSample {

			int get() {

				return(videoDecoder->getAudioConvertBytesPerSample());
			}
		}

		property int NrChannels {

			int get() {

				return(videoDecoder->getAudioConvertNrChannels());
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