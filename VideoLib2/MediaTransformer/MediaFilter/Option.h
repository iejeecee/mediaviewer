#pragma once
#include "..\..\Frame\VideoFrame.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Threading;

namespace VideoLib2
{
	public ref class Option 
	{
		String ^name;
		String ^description;
		Object ^defaultValue;
		AVOptionType type;
		double minValue;
		double maxValue;
		Object ^value;	

	};

}