#pragma once

#include "MimePart.h"

#using <System.dll>
//#using <System.Net.dll>

using namespace System;
using namespace System::Net;
using namespace System::Text;
using namespace System::IO;
using namespace Microsoft::Win32;
using namespace System::Collections::Specialized;

namespace imageviewer {

public ref class StreamMimePart : public MimePart
{

public:

	StreamMimePart(String ^name, String ^fileName, String ^mimeType, Stream ^data) {

		this->data = data;

		headerKeys["Content-Disposition"] = "form-data; name=\"" + name + "\"; filename=\"" + fileName + "\"";
		headerKeys["Content-Type"] = mimeType;
	

	}

};

}
