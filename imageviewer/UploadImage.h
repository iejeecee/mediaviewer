#pragma once

#include "HttpRequest.h"
#include "ImageStream.h"
#include "PicasaPhotoMetaData.h"


using namespace System;
using namespace System::Net;
using namespace System::Text;
using namespace System::IO;


namespace imageviewer {

public ref class UploadImage 
{

protected:


public: 

	// max 7 images in one shot
	static List<HttpWebResponse ^> ^freefap(List<ImageStream ^> ^imageStreams) {

		int uploads = (int)Math::Ceiling(imageStreams->Count / 7.0);

		List<HttpWebResponse ^> ^response = gcnew List<HttpWebResponse ^>();

		for(int j = 0; j < uploads; j++) {

			MultipartContent^ multipartData = gcnew MultipartContent();

			for(int i = j * 7, k = 1; i < imageStreams->Count; i++, k++) {

				ImageStream ^s = imageStreams[i];

				MimePart ^part = gcnew StreamMimePart(L"image_file" + Convert::ToString(k), s->name, s->mimeType, s->data);

				multipartData->addMimePart(part);

			}
			
			MimePart ^part = gcnew StringMimePart(L"action", L"Upload Image");

			multipartData->addMimePart(part);			

			response->Add( HttpRequest::multipartPostRequest(HttpRequest::MultipartContentType::FORM_DATA, 
				L"http://pics.freefap.nl/", multipartData) );

		}

		return(response);
		
	}


	static List<HttpWebResponse ^> ^dumppix(List<ImageStream ^> ^imageStreams) {

		int uploads = (int)Math::Ceiling(imageStreams->Count / 20.0);

		List<HttpWebResponse ^> ^response = gcnew List<HttpWebResponse ^>();

		for(int j = 0; j < uploads; j++) {

			MultipartContent^ multipartData = gcnew MultipartContent();

			for(int i = j * 20, k = 1; i < imageStreams->Count; i++, k++) {

				ImageStream ^s = imageStreams[i];

				String ^p = Convert::ToString(k);

				MimePart ^part = gcnew StringMimePart(L"adult_radio[" + p + L"]", L"adult");					
				multipartData->addMimePart(part);

				part = gcnew StreamMimePart(L"userfile[" + p + L"]",  s->name, s->mimeType, s->data);
				multipartData->addMimePart(part);

				part = gcnew StringMimePart(L"tags[" + p + L"]", L"");
				multipartData->addMimePart(part);

			}
		
			response->Add( HttpRequest::multipartPostRequest(HttpRequest::MultipartContentType::FORM_DATA,
				L"http://www.dumppix.com/upload.php", multipartData) );

		}

		return(response);
		
	}

	static HttpWebResponse ^picasa(String ^picasaUrl, PicasaPhotoMetaData ^metaData, ImageStream ^imageStream, MultipartContent::ProgressChangedDelegate ^progressChangedCallback, Object ^userState) {

		MultipartContent^ multipartData = gcnew MultipartContent();	
		multipartData->header = "Media multipart posting";
		multipartData->userState = userState;

		if(progressChangedCallback != nullptr) {

			multipartData->OnProgressChanged += progressChangedCallback;
		}

		StringMimePart ^metaDataMime = gcnew StringMimePart("metaData", metaData->toXml(), "application/atom+xml");
		multipartData->addMimePart(metaDataMime);

		StreamMimePart ^imageMime = gcnew StreamMimePart("photo", imageStream->name, imageStream->mimeType, imageStream->data);
		multipartData->addMimePart(imageMime);
							
		HttpWebResponse ^response = HttpRequest::multipartPostRequest(HttpRequest::MultipartContentType::RELATED, picasaUrl, multipartData);

		return(response);
	}

	
};

}