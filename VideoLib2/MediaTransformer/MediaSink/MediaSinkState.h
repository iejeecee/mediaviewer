#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Threading;
using namespace System::Threading::Tasks;

namespace VideoLib2
{			
	enum class MediaSinkState 
	{			
		OPEN,
		PAUSED,	
		CLOSED	
	};
		
}