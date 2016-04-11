#pragma once
#include "Video\IVideoDecoder.h"
#include "Frame\VideoFrame.h"
#include "Queue\FrameQueue.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Windows::Media::Imaging;
using namespace System::Threading;

namespace VideoLib {

	public ref class VideoPlayer
	{

	private:
		
		IVideoDecoder *videoDecoder;
				
		FrameQueue ^frameQueue;		

		static System::Runtime::InteropServices::GCHandle gch;

		array<bool> ^isFinalPacketAdded;
					
		
		array<bool> ^isSimulateLag;

	public:

		enum class DemuxPacketsResult {
			LAST_PACKET,
			SUCCESS,
			STOPPED
		};


		enum class OutputPixelFormat {
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

		enum class SeekKeyframeMode {

			SEEK_BACKWARDS,
			SEEK_FORWARDS
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
	
		property double DurationSeconds {

			double get() {

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

		property double FramesPerSecond {

			double get() {

				return videoDecoder->getFrameRate();
			}

		}

		property int SamplesPerSecond {

			int get() {

				return(videoDecoder->getOutputSampleRate());
			}
		}

		property int BytesPerSample {

			int get() {

				return(videoDecoder->getOutputBytesPerSample());
			}
		}

		property int NrChannels {

			int get() {

				return(videoDecoder->getOutputNrChannels());
			}
		}

		property bool HasVideo {

			bool get() {

				return(videoDecoder->hasVideo());
			}
		}

		property bool HasAudio {

			bool get() {

				return(videoDecoder->hasAudio());
			}
		}

		property VideoLib::MediaType MediaType {

			VideoLib::MediaType get() {

				return(videoDecoder->getMediaType());
			}
		}

		property VideoLib::SeekMode SeekMode {

			VideoLib::SeekMode get() {

				return(videoDecoder->getSeekMode());
			}
		}

		property int MaxAudioFrameSize {

			int get() {

				return(AVCODEC_MAX_AUDIO_FRAME_SIZE);
			}
		}

		/*property bool HasSeperateAudioStream {

			bool get() {

				return decoder->size() == 1 ? false : true;
			}
		}*/


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

		// simulate lag in input stream
		property array<bool> ^IsSimulateLag {

			array<bool> ^get() {

				return(isSimulateLag);
			}
		}

		event EventHandler<EventArgs ^> ^DecodeException;

		VideoPlayer();
		~VideoPlayer();
		!VideoPlayer();
				
		void open(OpenVideoArgs ^args, OutputPixelFormat videoFormat, System::Threading::CancellationToken ^token);
		bool seek(double posSeconds, SeekKeyframeMode mode);
	
		DemuxPacketsResult demuxPacket();			

		void close();

		static void setLogCallback(LogCallbackDelegate ^callback);
		static void enableLibAVLogging(LogLevel level);
		static void disableLibAVLogging();		
		
		static int getAvFormatVersion();
		static String ^getAvFormatBuildInfo();

	

		
	};

}