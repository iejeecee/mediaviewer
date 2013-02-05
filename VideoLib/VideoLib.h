// VideoLib.h

#pragma once
#include "VideoFrameGrabber.h"
#include "VideoPlayer.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace Microsoft::DirectX::Direct3D;

namespace VideoLib {

	public ref class VideoFrame
	{
	private:

		double pts;
		Texture ^frame;

	public:

		property double Pts {

			void set(double pts) {
				this->pts = pts;
			}

			double get() {
				return(pts);
			}
		}

		property Texture ^Frame {

			void set(Texture ^frame) {
				this->frame = frame;
			}

			Texture ^get() {
				return(frame);
			}
		}

		VideoFrame(Device ^device, int width, int height, Format pixelFormat);
		~VideoFrame();
	};

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
		List<String ^> ^metaData;

		float frameRate;

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

		void open(String ^videoLocation);
		void close();

		// TODO: Add your methods for this class here.
		List<Drawing::Bitmap ^> ^grabThumbnails(int maxThumbWidth, int maxThumbHeight, 
			int captureInterval, int nrThumbs);

		
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

		array<VideoFrame ^> ^frameData;

		double videoClock;

		double synchronizeVideo(int repeatFrames, double pts);

	public:

		DigitallyCreated::Utilities::Concurrency::LinkedListChannel<VideoFrame ^> ^freeFrames;
		DigitallyCreated::Utilities::Concurrency::LinkedListChannel<VideoFrame ^> ^decodedFrames;

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
		int decodeFrame();
		void close();

		
	};
}
