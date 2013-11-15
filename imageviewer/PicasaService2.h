#pragma once
#include "AuthInfo.h"
#include "HttpRequest.h"
#include "PicasaComment.h"
#include "PicasaAsyncState.h"
#include "UploadImage.h"

using namespace System;
using namespace System::IO;
using namespace System::Net;
using namespace System::Text::RegularExpressions;

using namespace System::Collections::Generic;
using namespace System::Xml;
using namespace System::Xml::Linq;

using namespace Google::GData;
using namespace Google::Picasa;

namespace imageviewer {

public ref class PicasaService2
{

private:

	static log4net::ILog ^log = log4net::LogManager::GetLogger(System::Reflection::MethodBase::GetCurrentMethod()->DeclaringType);

	AuthInfo ^_authInfo;
	int timeOutSeconds;
	BackgroundWorker ^bw;


	void setPicasaHeader(HttpWebRequest ^request) {

		request->Headers->Add(HttpRequestHeader::Authorization, "Bearer " + authInfo->accessToken);
	
		request->Headers->Add("GData-Version", "2");
	}

	HttpWebResponse ^picasaPostRequest(String ^url, String ^content) {

		HttpWebRequest ^request = (HttpWebRequest^)WebRequest::Create(url);

		cli::array<unsigned char> ^data = Encoding::UTF8->GetBytes(content);

		request->Method = "POST";
		request->ContentType = "application/atom+xml";	
		request->ContentLength = data->Length;
		request->Timeout = timeOutSeconds * 1000;

		setPicasaHeader(request);

		Stream ^requestStream = request->GetRequestStream();
		requestStream->WriteTimeout = timeOutSeconds * 1000;

		requestStream->Write(data, 0, data->Length);

		HttpWebResponse ^response = (HttpWebResponse ^)request->GetResponse();

		return(response);
	}

	HttpWebResponse ^picasaDeleteRequest(String ^url) {

		HttpWebRequest ^request = (HttpWebRequest^)WebRequest::Create(url);
		
		request->Method = "POST";
		request->Headers->Add("X-HTTP-Method-Override", "DELETE");
		request->Headers->Add("If-Match","*");
		request->ContentType = 	"application/json; charset=UTF-8";	
		request->ContentLength = 0;
		request->Timeout = timeOutSeconds * 1000;

		setPicasaHeader(request);

		HttpWebResponse ^response = (HttpWebResponse ^)request->GetResponse();

		return(response);
	}

	XDocument ^picasaGetRequest(String ^url) {

		HttpWebRequest ^request = (HttpWebRequest^)WebRequest::Create(url);
		request->Method = L"GET";		
		request->Timeout = timeOutSeconds * 1000;

		setPicasaHeader(request);
		
		HttpWebResponse ^response = (HttpWebResponse ^)request->GetResponse();

		String ^responseText = HttpRequest::responseToString(response, timeOutSeconds);

		Stream ^responseStream = response->GetResponseStream();
		responseStream->ReadTimeout = timeOutSeconds * 1000;

		XmlTextReader ^reader = gcnew XmlTextReader(responseStream);
		
		XDocument ^document = gcnew XDocument(reader);

		return(document);

	}

	void asyncUploadImages(Object ^args) {

		PicasaAsyncState ^state = dynamic_cast<PicasaAsyncState ^>(args);

		PicasaPhotoMetaData ^metaData = gcnew PicasaPhotoMetaData();
		MultipartContent::ProgressChangedDelegate ^progressChanged = gcnew MultipartContent::ProgressChangedDelegate(this, &PicasaService2::asyncUploadImageProgress);

		for each(ImageStream ^image in state->uploadImages) {

			metaData->fileName = image->name;

			state->progressForm->itemInfo = image->name;

			String ^url = state->uploadUri->AbsoluteUri;

			UploadImage::picasa(url, metaData, image, progressChanged, state);

			state->progressForm->totalProgressValue = ++state->uploadImageNr;
		}


	}

	void asyncUploadImageProgress(Object ^sender, ProgressChangedEventArgs ^e) {

		PicasaAsyncState ^state = dynamic_cast<PicasaAsyncState ^>(e->UserState);

		state->progressForm->itemProgressValue = e->ProgressPercentage;

	}

	void asyncDeleteEntries(PicasaAsyncState ^state, List<AtomEntry ^> ^entry) {

		try {

			for(int i = 0; i < entry->Count; i++) {
				
				if(state->cancel == true) break;

				String ^url = entry[i]->EditUri->Content;

				HttpWebResponse ^response = picasaDeleteRequest(url);
				state->deletedEntry = i;

				bw->ReportProgress((i * 100) / entry->Count, state);
			}

		} catch (Exception ^e) {

			log->Error("Error Deleting Album", e);
			MessageBox::Show(e->Message, "Error Deleting Album");
		}
	}

	void backgroundWorker_DoWork(Object ^sender, DoWorkEventArgs ^e) {

		PicasaAsyncState ^state = dynamic_cast<PicasaAsyncState ^>(e->Argument);

		switch(state->type) {

			case PicasaAsyncState::OperationType::DELETE_PHOTO: {

				List<AtomEntry ^> ^entry = gcnew List<AtomEntry ^>();

				for each(Photo ^photo in state->deleteImages) {

					entry->Add(photo->AtomEntry);
				}

				asyncDeleteEntries(state, entry);
				break;
			}
			case PicasaAsyncState::OperationType::DELETE_ALBUM: {

				List<AtomEntry ^> ^entry = gcnew List<AtomEntry ^>();

				for each(Album ^album in state->deleteAlbums) {

					entry->Add(album->AtomEntry);
				}

				asyncDeleteEntries(state, entry);
				break;
			}

		}

		e->Result = state;
	}

   void backgroundWorker_ProgressChanged( Object ^sender, ProgressChangedEventArgs^ e) {
     
		AsyncOperationProgress(this, e);
   }

   void backgroundWorker_RunWorkerCompleted( Object ^sender, RunWorkerCompletedEventArgs^ e) {
		
	    AsyncOperationCompleted(this, e);
   }

public: 

	event ProgressChangedEventHandler ^AsyncOperationProgress;
	event RunWorkerCompletedEventHandler ^AsyncOperationCompleted;

	PicasaService2() {

		timeOutSeconds = 60;

		bw = gcnew BackgroundWorker();
		bw->WorkerReportsProgress = true;
		bw->DoWork += gcnew DoWorkEventHandler(this, &PicasaService2::backgroundWorker_DoWork);
		bw->ProgressChanged += gcnew ProgressChangedEventHandler(this, &PicasaService2::backgroundWorker_ProgressChanged);
		bw->RunWorkerCompleted += gcnew RunWorkerCompletedEventHandler(this, &PicasaService2::backgroundWorker_RunWorkerCompleted);

		authInfo = gcnew AuthInfo();
	}

	property AuthInfo ^authInfo {

		void set(AuthInfo ^authInfo) {

			_authInfo = authInfo;
		}

		AuthInfo ^get() {

			return(_authInfo);
		}

	}

	void postComment(String ^content, String ^albumId, String ^photoId) {

		PicasaComment ^comment = gcnew PicasaComment(content);

		String ^picasaUrl = PicasaQuery::CreatePicasaUri("default", albumId, photoId);

		picasaPostRequest(picasaUrl, comment->toXml());

	}

	void test() {

		try {

			TextReader ^tr = gcnew StreamReader("picasaalbums.xml");

			XmlTextReader ^reader = gcnew XmlTextReader(tr);
			
			XDocument ^document = XDocument::Load(reader);

			XElement ^entry = document->Element("entry");

		} catch (Exception ^e) {

			MessageBox::Show(e->Message);
		}
	
	}

	void getUserAlbums() {

		try {

			XDocument ^reader = picasaGetRequest("https://picasaweb.google.com/data/feed/api/user/" + "default");

			int i = 0;

		} catch (Exception ^e) {

			log->Error("Error reading user albums", e);

		}

	}

	void uploadImages(Uri ^uploadUri, List<ImageStream ^> ^imageStream) {

		WaitCallback ^asyncUploadImages = gcnew WaitCallback(this, &PicasaService2::asyncUploadImages);

		PicasaAsyncState ^args = gcnew PicasaAsyncState(PicasaAsyncState::OperationType::UPLOAD_PHOTO);
		args->uploadImages = imageStream;
		args->uploadUri = uploadUri;
		args->progressForm->Show();
		args->progressForm->totalProgressMaximum = imageStream->Count;

		ThreadPool::QueueUserWorkItem(asyncUploadImages, args);
	}

	void asyncAction(PicasaAsyncState ^state) {

		bw->RunWorkerAsync(state);
	
	}

};

}

