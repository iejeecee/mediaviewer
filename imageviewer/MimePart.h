#pragma once
#include "BytesWrittenEventArgs.h"

using namespace System;
using namespace System::Net;
using namespace System::Text;
using namespace System::IO;
using namespace Microsoft::Win32;
using namespace System::Collections::Specialized;
using namespace System::ComponentModel;

namespace imageviewer {

public ref class MimePart 
{

private:

	array<unsigned char> ^headerData;
	array<unsigned char> ^afterFile;

	array<unsigned char> ^buffer;

protected:

	Stream ^data;
	NameValueCollection ^headerKeys;

	MimePart() {

		afterFile = Encoding::UTF8->GetBytes("\r\n");

		buffer = gcnew array<unsigned char>(8192);

		headerKeys = gcnew NameValueCollection();

	}

public:

	delegate void BytesWrittenDelegate(System::Object ^sender, BytesWrittenEventArgs ^e);
	event BytesWrittenDelegate ^OnBytesWritten;

	void generateHeader(String ^boundary) {

		StringBuilder ^sb = gcnew StringBuilder();

		sb->Append(L"--");
		sb->AppendLine(boundary);

		for each(String ^elem in headerKeys->AllKeys) {

			sb->Append(elem);
			sb->Append(L": ");
			sb->AppendLine(headerKeys[elem]);
		}

		sb->AppendLine();

		headerData = Encoding::UTF8->GetBytes(sb->ToString());
	}

	void writeData(Stream ^stream) {

		BytesWrittenEventArgs ^e = gcnew BytesWrittenEventArgs();

		stream->Write(headerData, 0, headerData->Length);

		e->bytesWritten = headerData->Length;

		if(data != nullptr) {

			int read = 0;

			while((read = data->Read(buffer, 0, buffer->Length)) > 0) {

				stream->Write(buffer, 0, read);

				e->bytesWritten += read;

				OnBytesWritten(this, e);
			}

		}

		stream->Write(afterFile, 0, afterFile->Length);

		e->bytesWritten += afterFile->Length;

		OnBytesWritten(this, e);

	}

	int getSizeBytes() {

		int size = headerData->Length;

		if(data != nullptr) {

			size += int(data->Length);
		}

		size += afterFile->Length;

		return(size);
	}

	void close() {

		if(data != nullptr) {

			data->Close();
		}
	}
};

}