#pragma once
#include "..\..\Video\VideoEncoder.h"
#include "..\..\Frame\VideoFrame.h"
#include "..\..\Frame\Packet.h"
#include "..\MediaFilter\BufferSinkFilter\BufferSinkFilter.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Threading;
using namespace System::Threading::Tasks;

namespace VideoLib2
{
	public interface class IMediaSink {

		void open();
		void close();

		void addFrames();

		property List<BufferSinkFilter ^> ^SinkFilter {

			List<BufferSinkFilter ^> ^get();
		}

	};

}