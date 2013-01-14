#pragma once

#include "StreamMimePart.h"
#include "StringMimePart.h"

using namespace System;
using namespace System::Net;
using namespace System::Text;
using namespace System::IO;
using namespace Microsoft::Win32;
using namespace System::Collections::Generic;
using namespace System::Collections::Specialized;

namespace imageviewer {

public ref class MultipartContent 
{

private:

	List<MimePart ^> ^parts;
	array<unsigned char> ^_header; 
	array<unsigned char> ^footer; 
	Object ^_userState;

	String ^_boundary;

	int totalSizeBytes;
	int totalBytesWritten;

	void bytesWritten(System::Object ^sender, BytesWrittenEventArgs ^e) {

		totalBytesWritten += e->bytesWritten;

		int progressPercentage = (100 * totalBytesWritten) / totalSizeBytes;

		ProgressChangedEventArgs ^args = gcnew ProgressChangedEventArgs(progressPercentage, userState);

		OnProgressChanged(this, args);
	}

public:

	delegate void ProgressChangedDelegate(System::Object ^sender, ProgressChangedEventArgs ^e);
	event ProgressChangedDelegate ^OnProgressChanged;

	MultipartContent() {

		parts = gcnew List<MimePart ^>();

		_boundary = L"---------------------------" + DateTime::Now.Ticks.ToString(L"x");

		header = nullptr;
		footer = Encoding::UTF8->GetBytes(L"--" + boundary + "\r\n");
		
		userState = nullptr;
	}


	property String ^boundary {

		String ^get() {

			return(_boundary);
		}

	}

	property String ^header {

		void set(String ^header) {

			if(header == nullptr) {

				_header = nullptr;

			} else {

				_header = Encoding::UTF8->GetBytes("\r\n" + header + "\r\n");
			}
		}

		String ^get() {

			if(_header == nullptr) return(nullptr);

			String ^result = Encoding::UTF8->GetString(_header);
			result = result->Remove(result->Length - 2)->Substring(2);
					 
			return( result );
		}

	}

	property Object ^userState {

		void set(Object ^userState) {

			_userState = userState;
		}

		Object ^get() {

			return(_userState);
		}

	}

	void addMimePart(MimePart ^part) {

		part->generateHeader(boundary);
		part->OnBytesWritten += gcnew MimePart::BytesWrittenDelegate(this, &MultipartContent::bytesWritten);

		parts->Add(part);		
	}

	void removeMimePart(MimePart ^part) {

		parts->Remove(part);		
	}

	void insertMimePart(int index, MimePart ^part) {

		part->generateHeader(boundary);
		part->OnBytesWritten += gcnew MimePart::BytesWrittenDelegate(this, &MultipartContent::bytesWritten);

		parts->Insert(index, part);		
	}


	int getContentLength() {

		int sizeBytes = 0;

		if(_header != nullptr) {

			sizeBytes += _header->Length;
		}

		for each(MimePart ^part in parts) {

			sizeBytes += part->getSizeBytes();
		}

		sizeBytes += footer->Length;

		return(sizeBytes);

	}

	void write(Stream ^stream) {

		totalSizeBytes = getContentLength();
		totalBytesWritten = 0;
		
		if(_header != nullptr) {

			stream->Write(_header, 0, _header->Length);
			totalBytesWritten += _header->Length;
		}

		for each(MimePart ^part in parts) {

			part->writeData(stream);

		}

		stream->Write(footer, 0, footer->Length);
		totalBytesWritten += footer->Length;

	}

	void close() {

		for each(MimePart ^part in parts) {

			part->close();

		}

	}

};

}
