#pragma once
#include <string>       
#include <iostream>     
#include <sstream> 
#include "..\Video\VideoDecoderFactory.h"
#include "..\Video\IVideoDecoder.h"
#include "..\Video\VideoEncoder.h"
#include "..\FilterGraph\FilterGraph.h"
#include "BitStreamFilter.h"
#include "..\Utils\Utils.h"
#include "MediaSource\IMediaSource.h"
#include "MediaFilter\BufferSourceFilter\BufferSourceFilter.h"
#include "MediaSink\IMediaSink.h"
#include "MediaFilter\InOutMediaFilter.h"

using namespace MediaViewer::Infrastructure::Video::TranscodeOptions;
using namespace System::Collections::Generic;
using namespace msclr::interop;
using namespace MediaViewer::Infrastructure::Utils;
using namespace MediaViewer::Infrastructure::Logging;
using namespace System::Threading::Tasks;

namespace VideoLib2 {

	public ref class MediaTransformer {

	public:

		typedef void (__stdcall *ProgressCallback)(int, double);

	private:

		System::Runtime::InteropServices::GCHandle gch;

	protected:

		List<IMediaSource ^> ^inputs;
		List<IMediaSink ^> ^outputs;
		List<InOutMediaFilter ^> ^filters;

		FilterGraph *filterGraph;
		Task ^transformTask;
		CancellationTokenSource ^tokenSource;

		int scheduler(bool &isFinished) {

			// request a output frame from the filtergraph
			filterGraph->requestFrame();

			// pick the media item(s) with the highest amount of failed request as the next
			// input to read a packet from
			int maxFailedRequests = -1;
			std::vector<int> potentialInputs;

			for(int i = 0; i < inputs->Count; i++) {

				if(inputs[i]->State == MediaSourceState::CLOSED) continue;		

				int inputMaxFailedRequests = -1;			

				for(int j = 0; j < inputs[i]->SourceFilter->Count; j++)
				{
					int streamFailedRequests = 1;

					BufferSourceFilter ^bufferSource = dynamic_cast<BufferSourceFilter ^>(inputs[i]->SourceFilter[j]);

					if(bufferSource != nullptr) {

						streamFailedRequests = bufferSource->NrFailedRequests;
					}

					if(streamFailedRequests > inputMaxFailedRequests) {

						inputMaxFailedRequests = streamFailedRequests;
					}

				}

				if(inputMaxFailedRequests > maxFailedRequests) {

					maxFailedRequests = inputMaxFailedRequests;
					potentialInputs.clear();

					potentialInputs.push_back(i);
				} 
				else if(inputMaxFailedRequests == maxFailedRequests)
				{
					potentialInputs.push_back(i);
				}
			}

			if(potentialInputs.empty()) {

				isFinished = true;
				return -1;

			} else {

				isFinished = false;

				int selectedInput = rand() % potentialInputs.size();

				return potentialInputs[selectedInput];
			}

		}

		void transform(Object ^state) 
		{				
			CancellationToken ^token = (CancellationToken ^)state;
			
			try {

				for(int i = 0; i < inputs->Count; i++) {

					inputs[i]->open();
				}

				for(int i = 0; i < outputs->Count; i++) {

					outputs[i]->open();				
				}	

				buildGraph();

				filterGraph->verifyGraph();

				// process frames 
				while (!token->IsCancellationRequested) {

					bool isFinished = false;

					int inputIdx = scheduler(isFinished);

					if (isFinished)  
					{
						// all inputs are finished
						break;
					}

					bool success = inputs[inputIdx]->decodeFrame();

					if(!success) {

						// inputIdx is finished
						continue;
					}

					for(int i = 0; i < outputs->Count; i++) {

						outputs[i]->addFrames();
					}
				
				}	
				
			} finally {

				for(int i = 0; i < inputs->Count; i++) {

					inputs[i]->close();
				}

				for(int i = 0; i < outputs->Count; i++) {

					outputs[i]->close();				
				}	

				filterGraph->clear();
			}
		}

		virtual void buildGraph() {

		}

		
		MediaTransformer() {
			
			filterGraph = new FilterGraph();

			inputs = gcnew List<IMediaSource ^>();
			outputs = gcnew List<IMediaSink ^>();
			filters = gcnew List<InOutMediaFilter ^>();

			tokenSource = gcnew CancellationTokenSource();
		}

		void addFilter(InOutMediaFilter ^filter) {

			filters->Add(filter);
		}

		

	public:

		enum class LogLevel {

			LOG_LEVEL_FATAL = AV_LOG_FATAL,
			LOG_LEVEL_ERROR = AV_LOG_ERROR,		
			LOG_LEVEL_WARNING = AV_LOG_WARNING,
			LOG_LEVEL_INFO = AV_LOG_INFO,
			LOG_LEVEL_DEBUG = AV_LOG_DEBUG,
			
		};

		delegate void LogCallbackDelegate(int level, String ^message);
		
		virtual ~MediaTransformer()
		{		
			if(gch.IsAllocated) {

				gch.Free();
			}

			this->!MediaTransformer();				
		}

		!MediaTransformer()
		{
			delete filterGraph;
		}

		void addInput(IMediaSource ^source) {

			inputs->Add(source);						
		}

		void addOutput(IMediaSink ^output) {

			outputs->Add(output);		
		}
						
		void start() {

			transformTask = Task::Factory->StartNew(gcnew Action<Object ^>(this, &MediaTransformer::transform), tokenSource->Token, tokenSource->Token);

		}
						
		IntPtr getFilterGraph(){

			return(IntPtr::IntPtr(filterGraph));
		}

		void enableLibAVLogging(LogLevel level)
		{
			VideoInit::enableLibAVLogging((int)level);
		}

		void disableLibAVLogging()
		{
			VideoInit::disableLibAVLogging();
		}

		void setLogCallback(LogCallbackDelegate ^logCallback)
		{
			if(gch.IsAllocated) {

				gch.Free();
			}

			gch = GCHandle::Alloc(logCallback);

			IntPtr voidPtr = Marshal::GetFunctionPointerForDelegate(logCallback);
		
			LOG_CALLBACK nativeLogCallback = static_cast<LOG_CALLBACK>(voidPtr.ToPointer());

			VideoInit::setLogCallback(nativeLogCallback);
	
		}
		
	};

}