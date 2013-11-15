#pragma once

#include "MultipartContent.h"

#using <System.dll>
//#using <System.Net.dll>

using namespace System;
using namespace System::Net;
using namespace System::Text;
using namespace System::IO;
using namespace System::Threading;

namespace imageviewer {

public ref class HttpRequest 
{
public: 

	enum class MultipartContentType {

		FORM_DATA,
		RELATED
	};

protected:

	
	void debugRequest(MultipartContent^ multipartData) {

		Stream ^stream = gcnew MemoryStream();

		int contentLength = multipartData->getContentLength();

		multipartData->write(stream);
		multipartData->close();

		stream->Position = 0;

		//responseToString(stream, contentLength);

	}

	static String ^contentTypeToString(MultipartContentType contentType) {

		switch(contentType) {

			case MultipartContentType::FORM_DATA:
				{

					return("multipart/form-data;");
				}
			case MultipartContentType::RELATED:
				{

					return("multipart/related;");
				}
		}

		return(nullptr);
	}


public:

		
	

	static String ^responseToString(HttpWebResponse ^response, int timeOutSeconds) {
		
		Stream ^responseStream = response->GetResponseStream();
		responseStream->ReadTimeout = timeOutSeconds * 1000;

		Encoding^ encode = System::Text::Encoding::GetEncoding( "utf-8" );
		// Pipes the stream to a higher level stream reader with the required encoding format.
		StreamReader^ readStream = gcnew StreamReader( responseStream, encode );
	
		String ^responseString = readStream->ReadToEnd();

		return(responseString);
	}

	static HttpWebResponse ^getRequest(String ^requestUriString, int timeOutSeconds) {

		HttpWebRequest ^request = (HttpWebRequest^)WebRequest::Create(requestUriString);

		request->Method = L"GET";

		unsigned int timeOutMiliSeconds = timeOutSeconds * 1000;

		request->Timeout = timeOutMiliSeconds;
	
		HttpWebResponse ^response = (HttpWebResponse ^)request->GetResponse();

		return(response);
	}

	static IAsyncResult ^asyncGetRequest(String ^requestUriString, AsyncCallback ^callback, WaitOrTimerCallback ^timeoutCallback, int timeOutSeconds) {

		HttpWebRequest ^request = (HttpWebRequest^)WebRequest::Create(requestUriString);

		request->Method = L"GET";

		IAsyncResult ^result = request->BeginGetResponse(callback, request);

		unsigned int timeOutMiliSeconds = timeOutSeconds * 1000;

		ThreadPool::RegisterWaitForSingleObject(result->AsyncWaitHandle, timeoutCallback, request, timeOutMiliSeconds, true );

		return(result);
	}

	static HttpWebResponse ^postRequest(String ^requestUriString, String ^content, int timeOutSeconds) {

		cli::array<unsigned char> ^data = Encoding::UTF8->GetBytes(content);

		HttpWebRequest ^request = (HttpWebRequest^)WebRequest::Create(requestUriString);

		request->Method = "POST";
		request->ContentType = "application/x-www-form-urlencoded";
		request->ContentLength = data->Length;
		request->Timeout = timeOutSeconds * 1000;

		Stream ^requestStream = request->GetRequestStream();
		requestStream->WriteTimeout = timeOutSeconds * 1000;

		requestStream->Write(data, 0, data->Length);

		HttpWebResponse ^response = (HttpWebResponse ^)request->GetResponse();

		return(response);
	}

	static HttpWebResponse ^multipartPostRequest(MultipartContentType contentType, String ^requestUriString, MultipartContent^ multipart) {

		HttpWebRequest ^request = (HttpWebRequest^)WebRequest::Create(requestUriString);

		request->Method = L"POST";
		request->ContentType = contentTypeToString(contentType) + " boundary=" + multipart->boundary;
		request->ContentLength = multipart->getContentLength();
		request->Headers->Add("Mime-version", "1.0");

		Stream ^stream = request->GetRequestStream();

		multipart->write(stream);

		stream->Close();
		multipart->close();

		HttpWebResponse ^response = (HttpWebResponse ^)request->GetResponse();

		return(response);
		
	}
	
};

}