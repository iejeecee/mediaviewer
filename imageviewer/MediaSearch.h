#pragma once
#include "FileUtils.h"
#include "MediaFormatConvert.h"
#include "MediaFileFactory.h"
#include "MediaSearchState.h"
#include "Database.h"
#include "MySQL.h"

namespace imageviewer {

using namespace System;
using namespace System::IO;
namespace Data = MediaDatabase;

public ref class MediaSearch
{
private:

  static log4net::ILog ^log = log4net::LogManager::GetLogger(System::Reflection::MethodBase::GetCurrentMethod()->DeclaringType);

  FileUtils::WalkDirectoryTreeDelegate ^callback; 
  MediaFileFactory ^mediaFactory;
  List<String ^> ^query;

  void parseQuery(MediaSearchState ^state) {

	  

  }

  void mediaFileToMediaData(MediaFile ^media, Data::Media ^item) {

	  FileMetaData ^metaData = media->MetaData;

	  if(metaData->CreationDate != DateTime::MinValue) {

		  item->MetaDataCreated = metaData->CreationDate;
	  }

	  if(metaData->CreationDate != DateTime::MinValue) {

		  item->MetaDataCreated = metaData->CreationDate;
	  }
  }

  Regex ^wildcardToRegex(String ^pattern) {

	  String ^regexPattern =  "^" + 
		  Regex::Escape(pattern)->Replace("\\*", ".*")->Replace("\\?", ".") + "$";

	  Regex ^regex = gcnew Regex(regexPattern, RegexOptions::IgnoreCase);

	  return(regex);
  }

  void mediaFileNameSearch(MediaSearchState ^state) {

	  Regex ^regex = wildcardToRegex(state->Query);

	  for each(FileInfo ^file in state->SearchFiles) {

		  if(regex->IsMatch(file->Name)) {

			  state->Matches->Add(file);
		  }
	  }
  }

  void mediaTagSearch(MediaSearchState ^state) {

	  List<Data::Media ^> ^updateMedia;
	  List<Data::Media ^> ^insertMedia;

	  Data::MediaTable ^mediaTable = gcnew Data::MediaTable();

	  mediaTable->needUpdate(state->SearchRoot, state->SearchFiles,
		  updateMedia, insertMedia);
/*
	  for each(Data::Media ^item in updateMedia) {

		  MediaFile ^media = nullptr;

		  try {

			  media = MediaFileFactory::openBlocking(item->Location,
				  MediaFile::MetaDataMode::LOAD_FROM_DISK);

			  if(media->MetaDataError) {

				  item->CanStoreMetaData = 0;
				  continue;
			  }

			  for each(String ^tag in media->MetaData->Tags) {

				  if(state->SearchTags->Contains(tag)) {

					  state->Matches->Add(file);
				  }
			  }

			  Debug::Write(media->Location + "\n");

		  } catch (Exception ^e) {

			  log->Warn("Cannot open metadata for " + file->FullName + ": " + e->Message);

		  } finally {

			  if(media != nullptr) {

				  media->close();
			  }
		  }

	  }
	  */
  }


  void getMediaFiles(FileInfo ^file, Object ^userData) {

	  if(!MediaFormatConvert::isMediaFile(file->Name)) return;

	  MediaSearchState ^state = dynamic_cast<MediaSearchState ^>(userData);

	  state->SearchFiles->Add(file);
  }

  void updateFileMetaData(String ^rootPath) {

	  MediaSearchState ^state = gcnew MediaSearchState();

	  DirectoryInfo ^root = gcnew DirectoryInfo(rootPath);

	  FileUtils::walkDirectoryTree(root, callback, state, true);

	  for(int i = 0; i < state->SearchFiles->Count; i++) {

		  FileInfo ^file = state->SearchFiles[i];

		  MediaFile ^media = nullptr;

		  try {

			  Util::DebugOut(i.ToString() + " of " + state->SearchFiles->Count.ToString() + " " + file->FullName);

			  Uri ^uri = gcnew Uri(file->FullName, UriKind::Absolute); 

			  String ^uriStr = uri->AbsoluteUri->Replace("'","''");

			  DataSet ^ds = MySQL::query("SELECT * FROM mediatag WHERE uri='" + uriStr + "'");

			  if(ds->Tables[0]->Rows->Count == 0) continue;			 

			  media = MediaFileFactory::openBlocking(file->FullName, 
				  MediaFile::MetaDataMode::LOAD_FROM_DISK);

			  if(media->MetaDataError) {

				  throw media->MetaDataError;
			  }

			  if(DateTime::Compare(media->MetaData->MetaDataDate, DateTime(2013,4,5)) >= 0) {

				  Util::DebugOut(i.ToString() + " of " + state->SearchFiles->Count.ToString() + " SKIPPED " + file->FullName);
				  continue;
			  }			  

			  media->MetaData->Tags->Clear();

			  for each(DataRow ^row in ds->Tables[0]->Rows) {

				  String ^tag = row["tag"]->ToString();

				  media->MetaData->Tags->Add(tag);
			  }

			  media->close();
			  media->MetaData->saveToDisk();


		  } catch (Exception ^e) {

			  log->Error("Problem updating metadata for " + file->FullName + ": " + e->Message);

		  } finally {

			  if(media != nullptr) {

				  media->close();
			  }
		  }

	  }
  }

public:

	MediaSearch() {

		callback = gcnew 
			FileUtils::WalkDirectoryTreeDelegate(this, &MediaSearch::getMediaFiles);

		mediaFactory = gcnew MediaFileFactory();
	}

	MediaSearchState ^searchDirectory(String ^path, MediaSearchState ^state) {

		String ^query = state->Query;

		if(String::IsNullOrEmpty(path) || String::IsNullOrEmpty(query)) return(state);

		try {

			state->SearchRoot = gcnew DirectoryInfo(path);
			 
			FileUtils::walkDirectoryTree(state->SearchRoot, callback, state, state->RecurseDirectories);
			
			if(state->DoFileNameSearch == true) {

				mediaFileNameSearch(state);
			}

			if(state->DoTagSearch == true) {

				mediaTagSearch(state);
			}

		} catch (Exception ^e) {

			log->Warn("Cannot access root directory during search: " + e->Message);

		} 

		return(state);
	}
};

}