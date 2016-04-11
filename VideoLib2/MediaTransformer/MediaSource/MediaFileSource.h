#pragma once
#include "IMediaSource.h"
#include "..\MediaFilter\BufferSourceFilter\VideoBufferSourceFilter.h"
#include "..\MediaFilter\BufferSourceFilter\AudioBufferSourceFilter.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Threading;
using namespace System::Threading::Tasks;

namespace VideoLib2
{
	public ref class MediaFileSource : IMediaSource
	{
	public:

		property MediaSourceState State {

			virtual MediaSourceState get() {

				return state;
			}

		protected:

			void set(MediaSourceState value)
			{
				state = value;
			}
		}

		property int NrLoops {

			virtual int get() {

				return(nrLoops);
			}
		protected:
			void set(int value)
			{
				nrLoops = value;
			}
		}

		property double StartTimeRange
		{
			virtual double get() {

				return startTimeRange;
			}
		protected:
			void set(double value)
			{
				startTimeRange = value;
			}
		}

		property double EndTimeRange
		{
			virtual double get() {

				return endTimeRange;
			}
		protected:
			void set(double value)
			{
				endTimeRange = value;
			}
		}

		property List<OutMediaFilter ^> ^SourceFilter {

			virtual List<OutMediaFilter ^> ^get() {

				return sourceFilter;
			}

		protected:

			void set(List<OutMediaFilter ^> ^value)
			{
				sourceFilter = value;
			}
		}

	protected:

		MediaSourceState state;

		IVideoDecoder *decoder;
		std::vector<AVFilter *> *buffer;
		String ^instanceName;

		List<OutMediaFilter ^> ^sourceFilter;

		double startTimeRange;
		double endTimeRange;

		int nrLoops;

		List<Packet ^> ^freePackets;
		List<Packet ^> ^mediaPackets;

		OpenVideoArgs ^mediaLocation;
		FilterGraph *filterGraph;

		CancellationTokenSource ^tokenSource;
		Task ^readPacketsTask;

		Object ^lockObject;

		static int MaxBufferedPackets = 1000;

		void initInputFilters() {

			for(int i = 0; i < decoder->getNrStreams(); i++) {

				Stream *inputStream = decoder->getStream(i);

				if(inputStream->isVideo()) {

					SourceFilter->Add(gcnew VideoBufferSourceFilter(instanceName + "v" + i, IntPtr(filterGraph), inputStream));

				} else if(inputStream->isAudio()) {

					SourceFilter->Add(gcnew AudioBufferSourceFilter(instanceName + "a" + i, IntPtr(filterGraph), inputStream));
				}

			}

		}

		void readPackets(Object ^state) {

			CancellationToken token = *(CancellationToken ^)state;			
			try {
				
				State = MediaSourceState::READING;

				IVideoDecoder::ReadFrameResult result = IVideoDecoder::ReadFrameResult::OK;

				while(!token.IsCancellationRequested || result == IVideoDecoder::ReadFrameResult::END_OF_STREAM)
				{	
					Monitor::Enter(lockObject);
					try {

						if(freePackets->Count > 0) {

							Packet ^packet = freePackets[0];
							freePackets->RemoveAt(0);

							result = decoder->readFrame(packet->AVLibPacketData);				

							if(result == IVideoDecoder::ReadFrameResult::OK) 
							{
								mediaPackets->Add(packet);
								Monitor::PulseAll(lockObject);

							} 
							else if(result == IVideoDecoder::END_OF_STREAM)
							{
								packet->IsFinalPacket = true;							

								mediaPackets->Add(packet);
								Monitor::PulseAll(lockObject);

							} else {

								packet->free();
								freePackets->Add(packet);
							}

						} else {

							Monitor::Wait(lockObject);
						}

					} finally {

						Monitor::Exit(lockObject);	
					}
				}

			} finally {
			
				Monitor::PulseAll(lockObject);
			}

		}


		bool decodePacket(AVPacket *packet, AVFrame *frame)
		{			
			int got_frame = 0;
			int bytesConsumed = 0;

			int inStreamIdx = packet->stream_index;
			VideoLib2::Stream *inStream = decoder->getStream(inStreamIdx);	

			if(inStream->isVideo()) {

				bytesConsumed = decoder->decodeVideoFrame(frame, &got_frame, packet);

			} else {

				bytesConsumed = decoder->decodeAudioFrame(frame, &got_frame, packet);							
			}

			if (bytesConsumed < 0) {

				VideoInit::writeToLog(AV_LOG_ERROR, "error decoding input");
				return(false);																		
			}

			if (got_frame) {

				int64_t framePts = av_frame_get_best_effort_timestamp(frame);

				if(framePts == AV_NOPTS_VALUE) {

					throw gcnew VideoLib2::VideoLibException("Cannot encode frame without pts value");	

				} else {

					frame->pts = framePts - inStream->getStartTime();
				}

				if(inStream->isAudio()) {

					// generate next pts for audio packets that contain multiple frames
					double audioDurationSec = (double)frame->nb_samples / decoder->getAudioSamplesPerSecond();

					int64_t audioDuration = decoder->getStream(inStreamIdx)->getTimeBaseUnits(audioDurationSec);

					packet->pts += audioDuration;						
				}

			}														

			if(packet->size > 0) {

				packet->size -= bytesConsumed;
				packet->data += bytesConsumed;

			}

			return(got_frame == 0  ? false : true);

		}

		void flush() {

			while(mediaPackets->Count > 0) {

				Packet ^packet = mediaPackets[0];
				mediaPackets->RemoveAt(0);
				packet->free();
				freePackets->Add(packet);
			}

			Monitor::PulseAll(lockObject);

		}

		void addFrameToBuffer(AVFrame *frame, int inStreamIdx) {

			BufferSourceFilter ^buffer = (BufferSourceFilter ^)SourceFilter[inStreamIdx];

			if(frame != NULL) {

				Stream *inStream = decoder->getStream(inStreamIdx);

				double timeSeconds = inStream->getTimeSeconds(frame->pts);

				if(timeSeconds >= StartTimeRange && timeSeconds <= EndTimeRange)
				{
					frame->pts = av_rescale_q_rnd(frame->pts,
						inStream->getTimeBase(),
						inStream->getCodecContext()->time_base,
						(AVRounding)(AV_ROUND_NEAR_INF|AV_ROUND_PASS_MINMAX));
				
					buffer->addFrame(frame);					
				}

			} else {

				// notify graph input is finished
				buffer->addFrame(NULL);
			}

		}

		bool decodeFrameAndAddToBuffer() {

			Packet ^packet = mediaPackets[0];
			int inStreamIdx = packet->AVLibPacketData->stream_index;

			AVFrame *frame = av_frame_alloc();
			if (!frame) {

				throw gcnew VideoLib2::VideoLibException("Error allocating frame");	
			}

			bool gotFrame = false;

			try 
			{				
				if(packet->IsFinalPacket) {

					// flush decoders streams
					for(int i = 0; i < decoder->getNrStreams(); i++) {

						packet->AVLibPacketData->stream_index = i;

						gotFrame = decodePacket(packet->AVLibPacketData, frame);		

						if(gotFrame) {

							addFrameToBuffer(frame, i);								
							return(true);
						}
					}

					// flush filters
					for(int i = 0; i < decoder->getNrStreams(); i++) {
								
						addFrameToBuffer(NULL, i);																
					}

					State = MediaSourceState::CLOSED;	
					return(false);

				} else {

					gotFrame = decodePacket(packet->AVLibPacketData, frame);
				}

				if(packet->AVLibPacketData->size == 0) {

					mediaPackets->RemoveAt(0);
					packet->free();
					freePackets->Add(packet);

					Monitor::PulseAll(lockObject);
				}

				if(gotFrame) {

					addFrameToBuffer(frame, inStreamIdx);													
				} 

				return gotFrame;

			} finally {

				av_frame_unref(frame);
			}
		}

	public:

		MediaFileSource(OpenVideoArgs ^mediaLocation, String ^instanceName, IntPtr filterGraph, 
			double startTimeRange, double endTimeRange, int nrLoops) 
		{
			this->instanceName = instanceName;
			this->filterGraph = (FilterGraph *)filterGraph.ToPointer();
			this->mediaLocation = mediaLocation;

			decoder = VideoDecoderFactory::create(mediaLocation);
			SourceFilter = gcnew List<OutMediaFilter ^>();	

			freePackets = gcnew List<Packet ^>();

			for(int i = 0; i < MaxBufferedPackets; i++) {

				freePackets->Add(gcnew Packet());
			}

			mediaPackets = gcnew List<Packet ^>();

			lockObject = gcnew Object();
			State = MediaSourceState::CLOSED;

			readPacketsTask = nullptr;
			tokenSource = gcnew CancellationTokenSource();

			StartTimeRange = startTimeRange;
			EndTimeRange = endTimeRange;

			NrLoops = nrLoops;
		}

		virtual ~MediaFileSource() {

			decoder->close();
			delete decoder;
		}


		virtual void open() 
		{	
			close();

			Monitor::Enter(lockObject);
			try
			{
				State = MediaSourceState::OPENING;

				decoder->open(mediaLocation, tokenSource->Token);

				initInputFilters();

				if(startTimeRange > 0) {

					bool result = decoder->seek(startTimeRange);
					if(result == false) {

						throw gcnew VideoLib2::VideoLibException("Error searching in input");
					}
				}

				readPacketsTask = Task::Factory->StartNew(gcnew Action<Object ^>(this, &MediaFileSource::readPackets), tokenSource->Token, tokenSource->Token);

			} finally {
				Monitor::Exit(lockObject);
			}
		}

		virtual void close()
		{		
			tokenSource->Cancel();

			if(readPacketsTask != nullptr) {
				
				readPacketsTask->Wait();				
			}

			Monitor::Enter(lockObject);
			try
			{
				flush();

				decoder->close();

				tokenSource = gcnew CancellationTokenSource();
			
				State = MediaSourceState::CLOSED;

			} finally {

				Monitor::Exit(lockObject);
			}

		}

		// returns false on end of stream
		virtual bool decodeFrame()
		{

			while(true) {

				Monitor::Enter(lockObject);
				try {

					if(state == MediaSourceState::CLOSED) return(false);

					if(mediaPackets->Count == 0) {
						
						Monitor::Wait(lockObject);

					} else {

						
						bool result = decodeFrameAndAddToBuffer();

						if(result == true) return(true);
					}
				}
				finally
				{
					Monitor::Exit(lockObject);
				}

			} 

		}



		bool seek(double posSeconds)
		{
			if(State != MediaSourceState::READING) return(false);

			Monitor::Enter(lockObject);
			try {

				bool result = decoder->seek(posSeconds);

				flush();

				return result;

			} finally {

				Monitor::Exit(lockObject);
			}
		}

	};

}