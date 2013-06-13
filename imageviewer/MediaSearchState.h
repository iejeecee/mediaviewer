#pragma once

namespace imageviewer {

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;

public ref class MediaSearchState : public EventArgs
{
private:

	String ^query;

	DirectoryInfo ^searchRoot;

	List<String ^> ^searchTags;
	List<FileInfo ^> ^searchFiles;

	List<FileInfo ^> ^matches;

	bool doFileNameSearch;
	bool doTagSearch;

	bool recurseDirectories;

public:

	MediaSearchState() {

		this->query = "";

		this->searchRoot = nullptr;

		searchTags = gcnew List<String ^>();
		matches = gcnew List<FileInfo ^>();
		searchFiles = gcnew List<FileInfo ^>();

		doFileNameSearch = true;
		doTagSearch = false;
		recurseDirectories = true;
	}

	property String ^Query {

		String ^get() {

			return(query);
		}

		void set(String ^query) {

			this->query = query;
		}
	}

	property DirectoryInfo ^SearchRoot {

		DirectoryInfo ^get() {

			return(searchRoot);
		}

		void set(DirectoryInfo ^searchRoot) {

			this->searchRoot = searchRoot;
		}
	}

	property bool DoFileNameSearch {

		bool get() {

			return(doFileNameSearch);
		}

		void set(bool doFileNameSearch) {

			this->doFileNameSearch = doFileNameSearch;
		}
	}

	property bool DoTagSearch {

		bool get() {

			return(doTagSearch);
		}

		void set(bool doTagSearch) {

			this->doTagSearch = doTagSearch;
		}
	}
  
	property bool RecurseDirectories {

		bool get() {

			return(recurseDirectories);
		}

		void set(bool recurseDirectories) {

			this->recurseDirectories = recurseDirectories;
		}
	}
  
	property List<String ^> ^SearchTags {

		List<String ^> ^get() {

			return(searchTags);
		}
	}

	property List<FileInfo ^> ^SearchFiles {

		List<FileInfo ^> ^get() {

			return(searchFiles);
		}
	}

	property List<FileInfo ^> ^Matches {

		List<FileInfo ^> ^get() {

			return(matches);
		}
	}
};

}