// VideoLib.h

#pragma once
#include "VideoFrameGrabber.h"
#include "VideoPlayer.h"
#include "VideoFrame.h"
#include "FrameQueue.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace Microsoft::DirectX::Direct3D;

namespace VideoLib {


	public ref class VideoPreview
	{

	private:

		VideoFrameGrabber *frameGrabber;
		System::Runtime::InteropServices::GCHandle gch;

		delegate void DecodedFrameDelegate(
		void *data, AVPacket *packet, AVFrame *frame, Video::FrameType type);

		void decodedFrameCallback(void *data, AVPacket *packet, 
			AVFrame *frame, Video::FrameType type);

		List<Drawing::Bitmap ^> ^thumbs;

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

		List<Drawing::Bitmap ^> ^grabThumbnails(int maxThumbWidth, int maxThumbHeight, 
			int captureInterval, int nrThumbs, double startOffset);

		
	};

	
	public ref class VideoPlayer
	{

	private:

		Device ^device;

		Native::VideoPlayer *videoPlayer;
		System::Runtime::InteropServices::GCHandle gch;

		delegate void DecodedFrameDelegate(
		void *data, AVPacket *packet, AVFrame *frame, Video::FrameType type);

		void decodedFrameCallback(void *data, AVPacket *packet, 
			AVFrame *frame, Video::FrameType type);

		static int nrFramesInBuffer = 10;

		double videoClock;
		double audioClock;

		double synchronizeVideo(int repeatFrames, __int64 dts);
		double synchronizeAudio(int size, __int64 dts);
		
		FrameQueue ^frameQueue;

		String ^videoLocation;

	public:

		enum class DecodeMode {

			DECODE_VIDEO_AND_AUDIO,
			DECODE_VIDEO_ONLY,
			DECODE_AUDIO_ONLY
		};

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

				return(videoPlayer->durationSeconds);
			}
		}

		property int Height {

			int get() {

				return(videoPlayer->getHeight());
			}
		}

		property int Width {

			int get() {

				return(videoPlayer->getWidth());
			}
		}

		property int SamplesPerSecond {

			int get() {

				return(videoPlayer->samplesPerSecond);
			}
		}

		property int BytesPerSample {

			int get() {

				return(videoPlayer->bytesPerSample);
			}
		}

		property int NrChannels {

			int get() {

				return(videoPlayer->nrChannels);
			}
		}

		property bool HasAudio {

			bool get() {

				return(videoPlayer->hasAudio());
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

				return(av_q2d(videoPlayer->getVideoStream()->time_base));
			}
		}

		event EventHandler<EventArgs ^> ^DecodeException;

		VideoPlayer(Device ^device);
		~VideoPlayer();

		void open(String ^videoLocation);
		bool seek(double posSeconds);
		bool decodeFrame(DecodeMode mode);
		void close();

		
	};
}
