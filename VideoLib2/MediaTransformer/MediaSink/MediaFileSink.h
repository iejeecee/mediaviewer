#pragma once
#include <msclr\marshal_cppstd.h>
#include "IMediaSink.h"
#include "..\MediaFilter\BufferSinkFilter\AudioBufferSinkFilter.h"
#include "..\MediaFilter\BufferSinkFilter\VideoBufferSinkFilter.h"
#include "..\BitStreamFilter.h"
#include "..\..\Utils\Rational.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Threading;
using namespace System::Threading::Tasks;

namespace VideoLib2
{
	public ref class MediaFileSink : IMediaSink
	{
		VideoEncoder *encoder;
		List<BufferSinkFilter ^> ^sinkFilter;
		FilterGraph *filterGraph;
		String ^instanceName;
		String ^outputFilename;
		BitStreamFilter *bitStreamFilter;

		String ^videoEncoderName; 
		int width; 
		int height; 
		int framesPerSecond;
		String ^audioEncoderName; 
		int sampleRate; 
		int nrChannels; 
		Dictionary<String ^, Object ^> ^options;

		void init(String ^outputFilename, String ^instanceName, IntPtr filterGraph,
			String ^videoEncoderName, int width, int height, int framesPerSecond,
			String ^audioEncoderName, int sampleRate, int nrChannels, Dictionary<String ^, Object ^> ^options)
		{
			this->outputFilename = outputFilename;
			this->instanceName = instanceName;
			this->filterGraph = (FilterGraph *)filterGraph.ToPointer();

			sinkFilter = gcnew List<BufferSinkFilter ^>();

			encoder = new VideoEncoder();
			
			bitStreamFilter = new BitStreamFilter();

			this->videoEncoderName = videoEncoderName;
			this->width = width;
			this->height = height;
			this->framesPerSecond = framesPerSecond;
			this->audioEncoderName = audioEncoderName; 
			this->sampleRate = sampleRate; 
			this->nrChannels = nrChannels;
			this->options = options;

		}

	public:

		MediaFileSink(String ^outputFilename, String ^instanceName, IntPtr filterGraph,
			String ^videoEncoderName, int width, int height, int framesPerSecond,
			String ^audioEncoderName, int sampleRate, int nrChannels, Dictionary<String ^, Object ^> ^options)
		{
			

			init(outputFilename,instanceName,filterGraph,videoEncoderName, width, height, framesPerSecond,
			audioEncoderName, sampleRate, nrChannels, options);
			
		}

		/*MediaFileSink(String ^outputFilename, String ^instanceName, IntPtr filterGraph, IMediaSource ^mediaSource) 
		{

			init(outputFilename,instanceName,filterGraph);

		

		}*/

		~MediaFileSink()
		{
			this->!MediaFileSink();
		}

		!MediaFileSink()
		{
			encoder->close();
			delete encoder;

			delete bitStreamFilter;
		}

		property List<BufferSinkFilter ^> ^SinkFilter {

			virtual List<BufferSinkFilter ^> ^get()
			{
				return sinkFilter;
			}
	protected:

			void set(List<BufferSinkFilter ^> ^value)
			{
				sinkFilter = value;
			}

		}

		virtual void open() {

			encoder->open(outputFilename);

			if(videoEncoderName != nullptr) {
				
				Dictionary<String ^, Object ^> ^videoEncoderOptions = (Dictionary<String ^, Object ^> ^)options["videoEncoderOptions"];

				String ^temp = videoEncoderName;
			
				Stream *videoStream = encoder->createStream(marshal_as<std::string>(temp), 
					width, 
					height, 
					av_make_q(1, 1), 
					av_make_q(1, framesPerSecond),
					videoEncoderOptions);						
								
			}

			if(audioEncoderName != nullptr) {

				Dictionary<String ^, Object ^> ^audioEncoderOptions = (Dictionary<String ^, Object ^> ^)options["audioEncoderOptions"];

				String ^temp = audioEncoderName;

				encoder->createStream(marshal_as<std::string>(temp),sampleRate,nrChannels, audioEncoderOptions);				
			}

			encoder->writeHeader();

			for(int i = 0; i < encoder->getNrStreams(); i++) 
			{
				Stream *outStream = encoder->getStream(i);

				if(outStream->isAudio()) {

					AudioBufferSinkFilter ^audioBufferSinkFilter = gcnew AudioBufferSinkFilter(instanceName + "a" + i, IntPtr(filterGraph), outStream);

					if(outStream->getCodecContext()->frame_size != 0) {

						audioBufferSinkFilter->setFrameSize(outStream->getCodecContext()->frame_size);
					}

					sinkFilter->Add(audioBufferSinkFilter);

				} else if(outStream->isVideo()) {

					sinkFilter->Add(gcnew VideoBufferSinkFilter(instanceName + "v" + i, IntPtr(filterGraph), outStream));
				}
				
			}


		}

		virtual void close() {

			encoder->close();
		}

		virtual void addFrames() {

			for(int i = 0; i < encoder->getNrStreams(); i++) 
			{
				Stream *outputStream = encoder->getStream(i);
				BufferSinkFilter::GetFrameResult result = BufferSinkFilter::GetFrameResult::VIDEO_FRAME;

				while(result == BufferSinkFilter::GetFrameResult::VIDEO_FRAME ||
					result == BufferSinkFilter::GetFrameResult::AUDIO_FRAME) 
				{

					AVFrame *frame = av_frame_alloc();
					try 
					{
						result = sinkFilter[i]->pullFrame(frame);

						AVPacket encPacket;
						bool success = false;

						if(result == BufferSinkFilter::GetFrameResult::VIDEO_FRAME ||
							result == BufferSinkFilter::GetFrameResult::AUDIO_FRAME)
						{						
							success = encoder->encodeFrame(i, frame, &encPacket);						
						} 
						else if(result == BufferSinkFilter::GetFrameResult::END_OF_FILE)
						{
							// flush encoder
							success = encoder->encodeFrame(i, NULL, &encPacket);
						}

						if(success) {

							bitStreamFilter->filterPacket(&encPacket, outputStream->getCodecContext());
							sinkFilter[i]->rescaleToOutputTimebase(&encPacket, outputStream->getTimeBase());

							encoder->writeEncodedPacket(&encPacket);
						}

					} finally {

						av_frame_unref(frame);
					}
				}

				if(result == BufferSinkFilter::GetFrameResult::END_OF_FILE)
				{
					encoder->writeTrailer();
				}
			}
		}

		void addBitStreamFilter(String ^name)
		{
			bitStreamFilter->add(marshal_as<std::string>(name));
		}
	};
}