#pragma once

#include "MediaFile.h"

namespace imageviewer {

using namespace System;
using namespace System::IO;
using namespace System::Collections::Generic;


public ref class UnknownFile : public MediaFile
{


private:

	

public:

	UnknownFile(String ^location) : MediaFile(location) {

	}


	virtual List<MetaDataThumb ^> ^generateThumbnails() override {

		return(gcnew List<MetaDataThumb ^>());
	}
	
	property MediaType MediaFormat
	{
		virtual MediaType get() override {

			return(MediaType::UNKNOWN);
		}
	}

};

}