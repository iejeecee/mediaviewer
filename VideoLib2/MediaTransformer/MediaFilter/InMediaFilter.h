#pragma once
#include "..\..\Video\VideoDecoderFactory.h"
#include "..\..\Frame\VideoFrame.h"
#include "MediaFilter.h"
#include "Option.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Threading;

namespace VideoLib2
{
	public ref class InMediaFilter : MediaFilter
	{
					
	protected:
																
		InMediaFilter(String ^filterName, String ^instanceName, IntPtr filterGraph) :
			MediaFilter(filterName,instanceName,filterGraph)
		{
			
			
						
		}


	};

}