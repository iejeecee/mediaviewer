#pragma once

#include "MimePart.h"


using namespace System;
using namespace System::Net;
using namespace System::Text;
using namespace System::IO;
using namespace Microsoft::Win32;
using namespace System::Collections::Specialized;

namespace imageviewer {

public ref class StringMimePart : public MimePart
{

private:


public:

	StringMimePart(String ^name, String ^value) {
		
		headerKeys["Content-Disposition"] = "form-data; name=\"" + name + "\"";

		data = gcnew MemoryStream(Encoding::UTF8->GetBytes(value));

	}

	StringMimePart(String ^name, String ^value, String ^mimeType) {
		
		headerKeys["Content-Disposition"] = "form-data; name=\"" + name + "\"";
		headerKeys["Content-Type"] = mimeType;

		data = gcnew MemoryStream(Encoding::UTF8->GetBytes(value));

	}


};

}
