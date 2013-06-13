#pragma once

#include "FileMetaData.h"
#include "ImageFile.h"
#include "VideoFile.h"
#include "UnknownFile.h"
#include "MediaFormatConvert.h"
#include "MediaFileException.h"
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
#define FILE_OPEN_ASYNC_TIMEOUT_MS 60 * 60 * 1000
// 5 seconds
#define FILE_OPEN_SYNC_TIMEOUT_MS 5 * 1000

public ref class MediaFileFactory 
{

private:

	ref class AsyncState {

	private:
		
		String ^location;
		Object ^userState;
		ModifiableGEventArgs<bool> ^isCancelled;
		MediaFile::MetaDataMode mode;

	public:

		AsyncState(String ^location, Object ^userState, MediaFile::MetaDataMode mode) {

			this->mode = mode;
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

		property MediaFile::MetaDataMode MetaDataMode
		{

			MediaFile::MetaDataMode get() {

				return(mode);
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

	static log4net::ILog ^log = log4net::LogManager::GetLogger(System::Reflection::MethodBase::GetCurrentMethod()->DeclaringType);

	DigitallyCreated::Utilities::Concurrency::FifoSemaphore ^openSemaphore;
	DigitallyCreated::Utilities::Concurrency::FifoSemaphore ^stateSemaphore;
	List<AsyncState ^> ^activeStates;

	static MediaFile ^openWebData(AsyncState ^state) {

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
					throw gcnew MediaFileException("Aborting opening image");
				}

				Thread::Sleep(100);
			}

			response = dynamic_cast<HttpWebResponse ^>(request->EndGetResponse(requestResult));

			responseStream = response->GetResponseStream();
			responseStream->ReadTimeout = HTTP_TIMEOUT_MS;	
			
			Stream ^data = gcnew MemoryStream();

			int bufSize = HTTP_READ_BUFFER_SIZE_BYTES;
			int count = 0;

			cli::array<unsigned char> ^buffer = gcnew cli::array<unsigned char>(bufSize);

			while((count = responseStream->Read(buffer, 0, bufSize)) > 0) {

				if(state->IsCancelled->Value == true) {							

					throw gcnew MediaFileException("Aborting reading image");
				}

				data->Write(buffer, 0, count);
			}

			data->Seek(0, System::IO::SeekOrigin::Begin);

			MediaFile ^media = newMediaFromMimeType(state, response->ContentType, data);

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

	static MediaFile ^openFileData(AsyncState ^state, int timeoutMs) {

		Stream ^data = FileUtils::waitForFileAccess(state->Location, FileAccess::Read,
			timeoutMs, state->IsCancelled);

		String ^mimeType = MediaFormatConvert::fileNameToMimeType(state->Location);

		MediaFile ^media = newMediaFromMimeType(state, mimeType, data);
	
		return(media);
	}

	void asyncOpen(Object ^asyncState) {

		AsyncState ^state = dynamic_cast<AsyncState ^>(asyncState);

		// initialize media with a dummy in case of exceptions
		MediaFile ^media = gcnew UnknownFile(state->Location, nullptr);

		// only allow one thread to open files at once
		openSemaphore->Acquire();

		try {

			if(String::IsNullOrEmpty(state->Location) || state->IsCancelled->Value == true) {

				return;

			} else if(Util::isUrl(state->Location)) {

				media = openWebData(state);

			} else {
	
				media = openFileData(state, FILE_OPEN_ASYNC_TIMEOUT_MS);
			}

		} catch (Exception ^e) {

			log->Warn("Cannot open media", e);
			media->OpenError = e;
			media->close();		

		} finally {

			stateSemaphore->Acquire();
			activeStates->Remove(state);
			stateSemaphore->Release();

			OpenFinished(this, media);
		}
	}


	static MediaFile ^newMediaFromMimeType(AsyncState ^state, String ^mimeType, Stream ^data) {

		MediaFile ^media = nullptr;

		if(mimeType->ToLower()->StartsWith("image")) {

			media = gcnew ImageFile(state->Location, mimeType, data, 
				state->MetaDataMode);			
		
		} else if(mimeType->ToLower()->StartsWith("video")) {

			media = gcnew VideoFile(state->Location, mimeType, data, 
				state->MetaDataMode);			
		
		} else {

			media = gcnew UnknownFile(state->Location, data);
		}

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
	
	// Open (read only) a file/http stream in a non blocking fashion
	// When the file is successfully opened a OpenFinished event is generated
	// The function will attempt to cancel any pending opens to speed up it's operation
	void openNonBlockingAndCancelPending(String ^location, MediaFile::MetaDataMode mode) {

		openNonBlockingAndCancelPending(location, nullptr, mode);
	}

	// Open (read only) a file/http stream in a non blocking fashion
	// When the file is successfully opened a OpenFinished event is generated
	// The function will attempt to cancel any pending opens to speed up it's operation
	// userstate is attached to the returning mediafile
	void openNonBlockingAndCancelPending(String ^location, Object ^userState, 
		MediaFile::MetaDataMode mode) 
	{

		try {

			WaitCallback ^asyncOpen = gcnew WaitCallback(this, &MediaFileFactory::asyncOpen);

			AsyncState ^state = gcnew AsyncState(location, userState, mode);

			// lock active states
			stateSemaphore->Acquire();

			// cancel previously started open(s)
			for(int i = 0; i < activeStates->Count; i++) {

				activeStates[i]->IsCancelled->Value = true;
			}

			// add current state to active states
			activeStates->Add(state);

			stateSemaphore->Release();

			ThreadPool::QueueUserWorkItem(asyncOpen, state);
			
		} catch(Exception ^e) {

			log->Error("Cannot open media", e);
			MessageBox::Show(e->Message);
		}

	}

	// needs to be called after the user is done with the file
	void releaseNonBlockingOpenLock() {

		openSemaphore->Release();
	}

	static MediaFile ^openBlocking(String ^location, MediaFile::MetaDataMode mode) {

		AsyncState ^state = gcnew AsyncState(location, nullptr, mode);

		// initialize media with a dummy in case of exceptions
		MediaFile ^media = gcnew UnknownFile(state->Location, nullptr);

		try {

			if(String::IsNullOrEmpty(state->Location)) {

				return(media);

			} else if(Util::isUrl(state->Location)) {

				media = openWebData(state);

			} else {
	
				media = openFileData(state, FILE_OPEN_SYNC_TIMEOUT_MS);
			}

		} catch (Exception ^e) {

			log->Error("cannot open media", e);
			media->OpenError = e;

			if(media->Data != nullptr) {

				media->close();
				media->Data = nullptr;
			}			
		}

		return(media);
	}
};

}