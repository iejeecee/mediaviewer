#pragma once
#include "stdafx.h"
#include "Video.h"
#include "VideoLibException.h"
#include "FilterGraph.h"
#include "OpenVideoArgs.h"
#include <algorithm>
#include <msclr\marshal_cppstd.h>

using namespace System::Threading;
using namespace msclr::interop;
using namespace System::Runtime::InteropServices;

namespace VideoLib {

	class IVideoDecoder : public Video {

	public:

		enum ReadFrameResult
		{
			OK,
			END_OF_STREAM,
			READ_ERROR
		};

		enum SamplingMode {
			FAST_BILINEAR = SWS_FAST_BILINEAR,
			BILINEAR = SWS_BILINEAR,
			BICUBIC = SWS_BICUBIC,
			X = SWS_X,
			POINT = SWS_POINT,
			AREA = SWS_AREA,
			BICUBLIN = SWS_BICUBLIN,
			GAUSS = SWS_GAUSS,
			SINC = SWS_SINC,
			LANCZOS = SWS_LANCZOS,
			SPLINE = SWS_SPLINE
		};

		virtual void open(OpenVideoArgs ^args, System::Threading::CancellationToken ^token) = 0;  
		
		virtual int64_t getSizeBytes() const = 0;
		virtual double getFrameRate() const = 0;	

		virtual int getAudioSamplesPerSecond() const = 0;		
		virtual int getAudioBytesPerSample() const = 0;

		virtual int getAudioNrChannels() const = 0;
		virtual int getAudioBytesPerSecond() const = 0;

		virtual double getDurationSeconds() const = 0;

		virtual AVPixelFormat getOutputPixelFormat() const = 0;
		virtual AVSampleFormat getOutputSampleFormat() const = 0;

		virtual int getOutputSampleRate() const = 0;
		virtual int64_t getOutputChannelLayout() const = 0;

		virtual int getOutputNrChannels() const = 0;
		virtual int getOutputBytesPerSample() const = 0;

		virtual FilterGraph *getVideoFilterGraph() const = 0;
		virtual FilterGraph *getAudioFilterGraph() const = 0;
	
		// reads a frame from the input stream and places it into a packet
		virtual ReadFrameResult readFrame(AVPacket *packet) = 0;

		virtual int decodeVideoFrame(AVFrame *picture, int *got_picture_ptr, const AVPacket *avpkt) = 0;

		// returns number of bytes decoded
		virtual int decodeAudioFrame(AVFrame *audio, int *got_audio_ptr, const AVPacket *avpkt) = 0;

		virtual bool seek(double posSeconds, int flags = 0) = 0;
		
		virtual int getWidth() const = 0;
		virtual int getHeight() const = 0;

		virtual void setVideoOutputFormat(AVPixelFormat pixelFormat, int width, int height, 
			SamplingMode sampling) = 0;
	
		virtual void convertVideoFrame(AVFrame *input, AVFrame *output) = 0;
		
		virtual void setAudioOutputFormat(int sampleRate = 44100, int64_t channelLayout = AV_CH_LAYOUT_STEREO, 
			AVSampleFormat sampleFormat = AV_SAMPLE_FMT_S16) = 0;
	
		virtual int convertAudioFrame(AVFrame *input, AVFrame *output) = 0;
	
		virtual bool isClosed() const = 0;

		
	};

}