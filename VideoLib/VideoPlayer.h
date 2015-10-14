#include "VideoDecoder.h"
#include "VideoFrame.h"
#include "FrameQueue.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Windows::Media::Imaging;
using namespace System::Threading;

namespace VideoLib {

	public ref class VideoPlayer
	{

	private:
		
		VideoDecoder *videoDecoder,*audioDecoder;

		std::vector<VideoDecoder *> *decoder;
		
		FrameQueue ^frameQueue;		

		System::Runtime::InteropServices::GCHandle gch;

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

				return(decoder->operator[](decoder->size() - 1)->getOutputSampleRate());
			}
		}

		property int BytesPerSample {

			int get() {

				return(decoder->operator[](decoder->size() - 1)->getOutputBytesPerSample());
			}
		}

		property int NrChannels {

			int get() {

				return(decoder->operator[](decoder->size() - 1)->getOutputNrChannels());
			}
		}

		property bool HasVideo {

			bool get() {

				return(videoDecoder->hasVideo());
			}
		}

		property bool HasAudio {

			bool get() {

				return(decoder->operator[](decoder->size() - 1)->hasAudio());
			}
		}

		property int MaxAudioFrameSize {

			int get() {

				return(AVCODEC_MAX_AUDIO_FRAME_SIZE);
			}
		}

		property bool HasSeperateAudioStream {

			bool get() {

				return decoder->size() == 1 ? false : true;
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

		// simulate lag in input stream
		property array<bool> ^IsSimulateLag {

			array<bool> ^get() {

				return(isSimulateLag);
			}
		}

		event EventHandler<EventArgs ^> ^DecodeException;

		VideoPlayer();
		~VideoPlayer();
			
		void open(String ^videoLocation, OutputPixelFormat format, String ^inputFormatName, 
			System::Threading::CancellationToken ^token);
		// video with seperate audio stream (e.g. youtube)
		void open(String ^videoLocation, OutputPixelFormat format, String ^videoFormatName,
			String ^audioLocation, String ^audioFormatName, System::Threading::CancellationToken ^token);
		bool seek(double posSeconds);

		// i == 0, video packet
		// i == 1, audio packet		
		DemuxPacketsResult demuxPacketFromStream(int i);
	
		DemuxPacketsResult demuxPacketInterleaved();			

		void close();

		void setLogCallback(LogCallbackDelegate ^callback, bool enableLibAVLogging,
			LogLevel level);
		
		static int getAvFormatVersion();
		static String ^getAvFormatBuildInfo();

	

		
	};

}