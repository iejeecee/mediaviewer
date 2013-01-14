#pragma once
#include "MediaFormatConvert.h"

using namespace System;

public ref class ImageStream {

private:

	String ^_name;
	String ^_mimeType;
	Stream ^_data;

public:

	ImageStream(String ^imagePath) {

		name = System::IO::Path::GetFileName(imagePath);
		data = File::OpenRead(imagePath);
		mimeType = MediaFormatConvert::fileNameToMimeType(name);

	}

	ImageStream(String ^name, String ^mimeType, Stream ^data) {

		this->name = name;
		this->mimeType = mimeType;
		this->data = data;
	}

	property String ^name {

		String ^get() {

			return(_name);
		}

		void set(String ^name) {

			_name = name;
		}
	}

	property String ^mimeType {

		String ^get() {

			return(_mimeType);
		}

		void set(String ^mimeType) {

			_mimeType = mimeType;
		}
	}

	property Stream ^data {

		Stream ^get() {

			return(_data);
		}

		void set(Stream ^data) {

			_data = data;
			//_data->Seek
		}
	}

};