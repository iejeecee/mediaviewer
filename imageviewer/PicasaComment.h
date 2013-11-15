#pragma once

using namespace System::Xml;

namespace imageviewer {

public ref class PicasaComment
{

	String ^_comment;

public:

	PicasaComment(String ^comment) 
	{

		this->comment = comment;
		
	}

	property String ^comment {

		String ^get() {

			return(_comment);
		}

		void set(String ^comment) {

			_comment = comment;
		}

	}

	String ^toXml() {

		StringBuilder ^s = gcnew StringBuilder();
		s->Append("<?xml version='1.0' encoding='utf-8'?>\r\n<entry xmlns='http://www.w3.org/2005/Atom'>\r\n  <content>");
		s->Append(comment);
		s->Append("</content>\r\n  <category scheme=\"http://schemas.google.com/g/2005#kind\"\r\n    term=\"http://schemas.google.com/photos/2007#comment\"/>\r\n</entry>");

		return(s->ToString());
	}

};

}