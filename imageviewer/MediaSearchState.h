#pragma once

namespace imageviewer {

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;

public ref class MediaSearchState
{
private:

	List<String ^> ^tags;
	List<FileInfo ^> ^matches;

public:

	MediaSearchState() {

		tags = gcnew List<String ^>();
		matches = gcnew List<FileInfo ^>();
	}
  
	property List<String ^> ^Tags {

		List<String ^> ^get() {

			return(tags);
		}
	}

	property List<FileInfo ^> ^Matches {

		List<FileInfo ^> ^get() {

			return(matches);
		}
	}
};

}