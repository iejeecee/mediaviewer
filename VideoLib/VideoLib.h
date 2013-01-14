// VideoLib.h

#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;

namespace VideoLib {

	public ref class VideoPreview
	{
		// TODO: Add your methods for this class here.
		List<Stream ^> ^grab(String ^videoLocation, int maxThumbWidth, int maxThumbHeight, 
			int captureInterval, int nrThumbs);
	};
}
