#pragma once

#include "FileMetaData.h"
#include "ImageFile.h"
#include "VideoFile.h"
#include "UnknownFile.h"
#include "MediaFormatConvert.h"
#include "FileUtils.h"
#include "GEventArgs.h"
#include <msclr\lock.h>
#include "Util.h"

namespace imageviewer {

using namespace System;
using namespace System::IO;
using namespace System::Collections::Generic;
using namespace System::Net;
using namespace System::Threading;

// 60 seconds
#define HTTP_TIMEOUT_MS (60 * 1000) 
#define HTTP_READ_BUFFER_SIZE_BYTES 8096
// 1 hour
#define FILE_OPEN_TIMEOUT_MS 60 * 60 * 1000

public ref class MediaFileFactory 
{

private:

	ref class AsyncState {

	private:

		String ^location;
		Object ^userState;
		ModifiableGEventArgs<bool> ^isCancelled;

	public:

		AsyncState(String ^location, Object ^userState) {

			this->location = location;
			this->userState = userState;
			isCancelled = gcnew ModifiableGEventArgs<bool>(false);
		}

		property Object ^UserState {

			Object ^get() {

				return(userState);
			}

			void set(Object ^userState) {

				this->userState = userState;
			}
		}

		property String ^Location {

			void set(String ^location) {

				this->location = location;
			}

			String ^get() {

				return(location);
			}
		}

		property ModifiableGEventArgs<bool> ^IsCancelled {

			void set(ModifiableGEventArgs<bool> ^isCancelled) {

				this->isCancelled = isCancelled;
			}

			ModifiableGEventArgs<bool> ^get() {

				return(isCancelled);
			}
		}
	};

	
	DigitallyCreated::Utilities::Concurrency::FifoSemaphore ^openSemaphore;
	DigitallyCreated::Utilities::Concurrency::FifoSemaphore ^stateSemaphore;
	List<AsyncState ^> ^activeStates;

	MediaFile ^readWebData(AsyncState ^state) {

		HttpWebResponse ^response = nullptr;
		Stream ^responseStream = nullptr;

		try {

			HttpWebRequest ^request = (HttpWebRequest^)WebRequest::Create(state->Location);
			request->Method = L"GET";
			request->Timeout = HTTP_TIMEOUT_MS;

			IAsyncResult ^requestResult = request->BeginGetResponse(nullptr, nullptr);

			while(!requestResult->IsCompleted) {

				if(state->IsCancelled->Value == true) {

					request->Abort();
					throw gcnew Exception("Aborting opening image");
				}

				Thread::Sleep(100);
			}

			response = dynamic_cast<HttpWebResponse ^>(request->EndGetResponse(requestResult));

			responseStream = response->GetResponseStream();
			responseStream->ReadTimeout = HTTP_TIMEOUT_MS;	

			MediaFile ^media = newMediaFromMimeType(state, response->ContentType);
			media->Data = gcnew MemoryStream();

			int bufSize = HTTP_READ_BUFFER_SIZE_BYTES;
			int count = 0;

			cli::array<unsigned char> ^buffer = gcnew cli::array<unsigned char>(bufSize);

			while((count = responseStream->Read(buffer, 0, bufSize)) > 0) {

				if(state->IsCancelled->Value == true) {							

					throw gcnew Exception("Aborting reading image");
				}

				media->Data->Write(buffer, 0, count);
			}

			media->Data->Seek(0, System::IO::SeekOrigin::Begin);

			return(media);

		} finally {

			if(responseStream != nullptr) {

				responseStream->Close();
			}

			if(response != nullptr) {

				response->Close();
			}
		}
	}

	MediaFile ^readFileData(AsyncState ^state) {

		Stream ^data = FileUtils::waitForFileAccess(state->Location, FileAccess::Read,
			FILE_OPEN_TIMEOUT_MS, state->IsCancelled);
		String ^mimeType = MediaFormatConvert::fileNameToMimeType(state->Location);

		FileMetaData ^metaData = nullptr;
		Exception ^metaDataError = nullptr;

		try {

			metaData = gcnew FileMetaData(state->Location);	
			metaData->closeFile();

		} catch (Exception ^e) {

			metaDataError = e;
		}

		MediaFile ^media = newMediaFromMimeType(state, mimeType);
		media->Data = data;
		media->MetaData = metaData;
		media->MetaDataError = metaDataError;

		return(media);
	}

	void asyncOpen(Object ^asyncState) {

		AsyncState ^state = dynamic_cast<AsyncState ^>(asyncState);

		// initialize media with a dummy in case of exceptions
		MediaFile ^media = gcnew UnknownFile(state->Location);

		// only allow one thread to open files at once
		openSemaphore->Acquire();

		try {

			if(String::IsNullOrEmpty(state->Location) || state->IsCancelled->Value == true) {

				return;

			} else if(Util::isUrl(state->Location)) {

				media = readWebData(state);

			} else {
	
				media = readFileData(state);
			}

		} catch (Exception ^e) {

			media->OpenError = e;

			if(media->Data != nullptr) {

				media->Data->Close();
				media->Data = nullptr;
			}			

		} finally {

			stateSemaphore->Acquire();
			activeStates->Remove(state);
			stateSemaphore->Release();

			OpenFinished(this, media);
		}
	}


	MediaFile ^newMediaFromMimeType(AsyncState ^state, String ^mimeType) {

		MediaFile ^media = nullptr;

		if(mimeType->ToLower()->StartsWith("image")) {

			media = gcnew ImageFile(state->Location);			
		
		} else if(mimeType->ToLower()->StartsWith("video")) {

			media = gcnew VideoFile(state->Location);			
		
		} else {

			media = gcnew UnknownFile(state->Location);
		}

		media->MimeType = mimeType;
		media->UserState = state->UserState;

		return(media);
	}

public: 

	event EventHandler<MediaFile ^> ^OpenFinished;

	MediaFileFactory() {

		// it is important to use fifo semaphores to preserve the order in which opening
		// files are requested
		openSemaphore = gcnew DigitallyCreated::Utilities::Concurrency::FifoSemaphore(1);
		stateSemaphore = gcnew DigitallyCreated::Utilities::Concurrency::FifoSemaphore(1);
		activeStates = gcnew List<AsyncState ^>();
	}
	
	void open(String ^location) {

		open(location, nullptr);
	}

	void open(String ^location, Object ^userState) {

		try {

			WaitCallback ^asyncOpen = gcnew WaitCallback(this, &MediaFileFactory::asyncOpen);

			AsyncState ^state = gcnew AsyncState(location, userState);

			// lock active states
			stateSemaphore->Acquire();

			// cancel previously started open(s)
			for(int i = 0; i < activeStates->Count; i++) {

				activeStates[i]->IsCancelled->Value = true;
			}

			activeStates->Add(state);

			stateSemaphore->Release();

			ThreadPool::QueueUserWorkItem(asyncOpen, state);
			
		} catch(Exception ^e) {

			MessageBox::Show(e->Message);
		}

	}

	void releaseOpenLock() {

		openSemaphore->Release();
	}
};

}