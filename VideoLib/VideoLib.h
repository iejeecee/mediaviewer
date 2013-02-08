// VideoLib.h

#pragma once
#include "VideoFrameGrabber.h"
#include "VideoPlayer.h"
#include "VideoFrame.h"
#include "PacketQueue.h"

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

		double videoClock;
		Format pixelFormat;

		double synchronizeVideo(int repeatFrames, double pts);
		void fillBuffer(Surface ^frame, BYTE* pY, BYTE* pV, BYTE* pU);
		
	public:

		PacketQueue ^packetQueue;

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

		property int BitRate {

			int get() {

				return(videoPlayer->bitRate);
			}
		}

		property int BitsPerSample {

			int get() {

				return(videoPlayer->bitsPerSample);
			}
		}

		property int NrChannels {

			int get() {

				return(videoPlayer->nrChannels);
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

		VideoPlayer(Device ^device, Format pixelFormat);
		~VideoPlayer();

		void open(String ^videoLocation);
		int decodeFrame();
		void close();

		
	};
}
