#pragma once

namespace imageviewer {

using namespace System;
using namespace System::IO;
using namespace System::Collections::Generic;


public ref class MediaFileException : public Exception
{


private:

	

public:

	MediaFileException(String ^message) : Exception(message) {

	}
};

}