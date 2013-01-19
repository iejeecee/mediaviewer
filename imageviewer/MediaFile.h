#pragma once

#include "FileMetaData.h"

namespace imageviewer {

using namespace System;
using namespace System::IO;
using namespace System::Collections::Generic;

public ref class MediaFile abstract : public EventArgs
{

protected:

	static const int MAX_THUMBNAIL_WIDTH = 160;
	static const int MAX_THUMBNAIL_HEIGHT = 160;

	String ^location;
	String ^name;

	String ^mimeType;	
	FileMetaData ^metaData;

	Stream ^data;
	__int64 sizeBytes;

	Exception ^openError;
	Exception ^metaDataError;

	Object ^userState;

	MediaFile() {

		location = nullptr;
		data = nullptr;
		metaData = nullptr;
		mimeType = nullptr;
		openError = nullptr;
		metaDataError = nullptr;
		userState = nullptr;

	}

	MediaFile(String ^location) {

		this->location = location;
		data = nullptr;
		metaData = nullptr;
		mimeType = nullptr;
		openError = nullptr;
		metaDataError = nullptr;
		userState = nullptr;
	}

public:

	enum class MediaType {
		UNKNOWN,
		IMAGE,
		VIDEO
	};

	property String ^Location {

		void set(String ^location) {

			this->location = location;
		}

		String ^get() {

			return(location);
		}
	}

	property String ^Name {

		String ^get() {

			return(Path::GetFileName(location));
		}
	}

	property String ^MimeType {

		String ^get() {

			return(mimeType);
		}

		void set(String ^mimeType) {

			this->mimeType = mimeType;
		}
	}

	property FileMetaData ^MetaData {

		void set(FileMetaData ^metaData) {

			this->metaData = metaData;
		}

		FileMetaData ^get() {

			return(metaData);
		}
	}

	property Stream ^Data {

		Stream ^get() {

			return(data);
		}

		void set(Stream ^data) {

			this->data = data;

			if(data != nullptr) {

				SizeBytes = data->Length;
			}
		}
	}

	property Exception ^OpenError {

		Exception ^get() {

			return(openError);
		}

		void set(Exception ^openError) {

			this->openError = openError;
		}
	}

	property Exception ^MetaDataError {

		Exception ^get() {

			return(metaDataError);
		}

		void set(Exception ^metaDataError) {

			this->metaDataError = metaDataError;
		}
	}
	
	property bool OpenSuccess {

		bool get() {

			return(OpenError != nullptr ? false : true);
		}

	}

	property MediaType MediaFormat {

		virtual MediaType get() = 0;
	}

	property Object ^UserState {

		Object ^get() {

			return(userState);
		}

		void set(Object ^userState) {

			this->userState = userState;
		}
	}

	property __int64 SizeBytes {

		__int64 get() {

			return(sizeBytes);
		}

		void set(__int64 sizeBytes) {

			this->sizeBytes = sizeBytes;
		}
	}

	virtual List<MetaDataThumb ^> ^generateThumbnails() = 0;

	virtual String ^getDefaultCaption() {

		return("");
	}

	virtual void close() {

		if(data != nullptr) {

			data->Close();
		}

		if(metaData != nullptr) {

			metaData->closeFile();
		}
	}
};

}