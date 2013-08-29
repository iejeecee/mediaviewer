#pragma once

#include "FileMetaData.h"
#include "Util.h"

namespace imageviewer {

using namespace System;
using namespace System::IO;
using namespace System::Collections::Generic;
namespace DB = MediaDatabase;

public ref class MediaFile abstract : public EventArgs
{

public:

	enum class MetaDataMode
	{
		AUTO,
		LOAD_FROM_DISK,
		LOAD_FROM_DATABASE
	};

protected:

	static log4net::ILog ^log = log4net::LogManager::GetLogger(System::Reflection::MethodBase::GetCurrentMethod()->DeclaringType);

	static const int MAX_THUMBNAIL_WIDTH = 160;
	static const int MAX_THUMBNAIL_HEIGHT = 160;

	String ^location;
	String ^name;

	String ^mimeType;	
	FileMetaData ^metaData;

	Stream ^data;
	__int64 sizeBytes;

	Exception ^openError;
	Exception ^metaDataError;

	Object ^userState;

	MetaDataMode mode;

	MediaFile() {

		location = nullptr;
		data = nullptr;
		metaData = nullptr;
		mimeType = nullptr;
		openError = nullptr;
		metaDataError = nullptr;
		userState = nullptr;
		mode = MetaDataMode::AUTO;
	}

	MediaFile(String ^location, String ^mimeType, Stream ^data, MetaDataMode mode) {

		this->location = location;
		this->data = data;
		this->mimeType = mimeType;
		this->mode = mode;

		metaData = nullptr;
		openError = nullptr;
		metaDataError = nullptr;
		userState = nullptr;

		if(String::IsNullOrEmpty(Location) || MediaFormat == MediaType::UNKNOWN) return;

		readMetaData();
	}

	virtual void readMetaData() {

		try {

			if(Util::isUrl(Location) == false) {

				MetaData = gcnew FileMetaData();	

				switch(mode) {
					case MetaDataMode::AUTO:
						{

							MetaData->load(Location);
							break;
						}
					case MetaDataMode::LOAD_FROM_DATABASE:
						{

							MetaData->loadFromDataBase(Location);
							break;
						}
					case MetaDataMode::LOAD_FROM_DISK:
						{

							MetaData->loadFromDisk(Location);
							break;
						}
					default:
						{
							System::Diagnostics::Debug::Assert(false);
							break;
						}

				}
			}

		} catch (Exception ^e) {

			log->Warn("Cannot read metadata: " + Location, e);
			MetaDataError = e;
		}
	}

	virtual List<MetaDataThumb ^> ^generateThumbnails() = 0;

public:

	enum class MediaType {
		UNKNOWN,
		IMAGE,
		VIDEO
	};

	property String ^Location {

		void set(String ^location) {

			this->location = location;
		}

		String ^get() {

			return(location);
		}
	}

	property String ^Name {

		String ^get() {

			return(Path::GetFileName(location));
		}
	}

	property String ^MimeType {

		String ^get() {

			return(mimeType);
		}

		void set(String ^mimeType) {

			this->mimeType = mimeType;
		}
	}

	property FileMetaData ^MetaData {

		void set(FileMetaData ^metaData) {

			this->metaData = metaData;
		}

		FileMetaData ^get() {

			return(metaData);
		}
	}

	property Stream ^Data {

		Stream ^get() {

			return(data);
		}

		void set(Stream ^data) {

			this->data = data;

			if(data != nullptr) {

				SizeBytes = data->Length;
			}
		}
	}

	property Exception ^OpenError {

		Exception ^get() {

			return(openError);
		}

		void set(Exception ^openError) {

			this->openError = openError;
		}
	}

	property Exception ^MetaDataError {

		Exception ^get() {

			return(metaDataError);
		}

		void set(Exception ^metaDataError) {

			this->metaDataError = metaDataError;
		}
	}
	
	property bool OpenSuccess {

		bool get() {

			return(OpenError != nullptr ? false : true);
		}

	}

	property MediaType MediaFormat {

		virtual MediaType get() = 0;
	}

	property Object ^UserState {

		Object ^get() {

			return(userState);
		}

		void set(Object ^userState) {

			this->userState = userState;
		}
	}

	property __int64 SizeBytes {

		__int64 get() {

			return(sizeBytes);
		}

		void set(__int64 sizeBytes) {

			this->sizeBytes = sizeBytes;
		}
	}

	List<MetaDataThumb ^> ^getThumbnails()
	{
		
		List<MetaDataThumb ^> ^thumbs = gcnew List<MetaDataThumb ^>();

		if(MediaFormat == MediaFile::MediaType::UNKNOWN) {

			return(thumbs);

		} else if(Util::isUrl(Location)) {

			thumbs = generateThumbnails();
			return(thumbs);
		}

		DB::Context ^ctx = gcnew DB::Context();

		DB::Media ^mediaItem = ctx->getMediaByLocation(Location);

		if(mediaItem != nullptr) {

			FileMetaData ^temp = gcnew FileMetaData(mediaItem);

			ctx->close();
			
			if(temp->Thumbnail->Count == 0) {

				thumbs = generateThumbnails();
				MetaData->Thumbnail = thumbs;
				MetaData->saveToDatabase();

			} else {

				thumbs = temp->Thumbnail;
			}

		} else {

			ctx->close();

			thumbs = generateThumbnails();

			if(MetaDataError == nullptr) {
				
				MetaData->Thumbnail = thumbs;
				MetaData->saveToDatabase();

			} else {

				mediaItem = DB::Context::newMediaItem(gcnew FileInfo(Location));
				mediaItem->CanStoreMetaData = 0;

				FileMetaData ^temp = gcnew FileMetaData(mediaItem);
				temp->Thumbnail = thumbs;

				temp->saveToDatabase();
			}
		}

		return(thumbs);
	}

	virtual String ^getDefaultCaption() {

		return("");
	}

	virtual String ^getDefaultFormatCaption() {

		return("");
	}

	virtual void close() {

		if(data != nullptr) {

			data->Close();
		}

		if(metaData != nullptr) {

			metaData->closeFile();
		}
	}
};

}