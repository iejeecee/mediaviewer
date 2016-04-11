#pragma once
#include "..\..\Video\VideoDecoderFactory.h"
#include "..\..\Frame\VideoFrame.h"
#include "..\..\Frame\Packet.h"
#include "..\MediaFilter\OutMediaFilter.h"
#include "MediaSourceState.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Threading;
using namespace System::Threading::Tasks;

namespace VideoLib2
{
	public interface class IMediaSource {
			
		property MediaSourceState State { 

			MediaSourceState get();
		}

		void open();
		void close();

		// return false when finished otherwise true
		bool decodeFrame();
		
		property List<OutMediaFilter ^> ^SourceFilter {

			List<OutMediaFilter ^> ^get();
		}
		
	};

}