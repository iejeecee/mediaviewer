#pragma once

using namespace System::Xml;

namespace imageviewer {

public ref class PicasaPhotoMetaData
{

	String ^_fileName;
	String ^_summary;

public:

	PicasaPhotoMetaData() 
	{

		fileName = nullptr;
		summary = nullptr;
		
	}

	PicasaPhotoMetaData(String ^fileName, String ^summary) 
	{

		this->fileName = fileName;
		this->summary = summary;
		
	}

	property String ^fileName {

		String ^get() {

			return(_fileName);
		}

		void set(String ^fileName) {

			_fileName = fileName;
		}

	}

	property String ^summary {

		String ^get() {

			return(_summary);
		}

		void set(String ^summary) {

			_summary = summary;
		}

	}

	String ^toXml() {

		StringBuilder ^s = gcnew StringBuilder();
		s->Append("<entry xmlns='http://www.w3.org/2005/Atom'>\r\n  <title>");
		s->Append(fileName);
		s->Append("</title>\r\n  <summary>");
		s->Append(summary);
		s->Append(".</summary>\r\n  <category scheme=\"http://schemas.google.com/g/2005#kind\"\r\n    term=\"http://schemas.google.com/photos/2007#photo\"/>\r\n</entry>");

		return(s->ToString());
	}

};

}