#pragma once
#include "FileUtils.h"
#include "MediaFormatConvert.h"
#include "MediaFileFactory.h"
#include "MediaSearchState.h"
#include "MySQL.h"

namespace imageviewer {

using namespace System;
using namespace System::IO;

public ref class MediaSearch
{
private:

  static log4net::ILog ^log = log4net::LogManager::GetLogger(System::Reflection::MethodBase::GetCurrentMethod()->DeclaringType);

  FileUtils::WalkDirectoryTreeDelegate ^callback; 
  MediaFileFactory ^mediaFactory;

  Regex ^wildcardToRegex(String ^pattern) {

	  String ^regexPattern =  "^" + 
		  Regex::Escape(pattern)->Replace("\\*", ".*")->Replace("\\?", ".") + "$";

	  Regex ^regex = gcnew Regex(regexPattern, RegexOptions::IgnoreCase);

	  return(regex);
  }

  void mediaTagMatch(FileInfo ^file, MediaSearchState ^state) {

	  MediaFile ^media = nullptr;

	  try {

		  media = MediaFileFactory::openBlocking(file->FullName);

		  if(media->MetaDataError) {

			  throw media->MetaDataError;
		  }

		  for each(String ^tag in media->MetaData->Tags) {

			  if(state->Tags->Contains(tag)) {

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


  void getMediaFiles(FileInfo ^file, Object ^userData) {

	  if(!MediaFormatConvert::isMediaFile(file->Name)) return;

	  MediaSearchState ^state = dynamic_cast<MediaSearchState ^>(userData);

	  state->MediaFiles->Add(file);
  }

  void updateFileMetaData(String ^rootPath) {

	  MediaSearchState ^state = gcnew MediaSearchState();

	  DirectoryInfo ^root = gcnew DirectoryInfo(rootPath);

	  FileUtils::walkDirectoryTree(root, callback, state);

	  for(int i = 0; i < state->MediaFiles->Count; i++) {

		  FileInfo ^file = state->MediaFiles[i];

		  MediaFile ^media = nullptr;

		  try {

			  Util::DebugOut(i.ToString() + " of " + state->MediaFiles->Count.ToString() + " " + file->FullName);

			  Uri ^uri = gcnew Uri(file->FullName, UriKind::Absolute); 

			  String ^uriStr = uri->AbsoluteUri->Replace("'","''");

			  DataSet ^ds = MySQL::query("SELECT * FROM mediatag WHERE uri='" + uriStr + "'");

			  if(ds->Tables[0]->Rows->Count == 0) continue;			 

			  media = MediaFileFactory::openBlocking(file->FullName);

			  if(media->MetaDataError) {

				  throw media->MetaDataError;
			  }

			  if(DateTime::Compare(media->MetaData->MetaDataDate, DateTime(2013,4,5)) >= 0) {

				  Util::DebugOut(i.ToString() + " of " + state->MediaFiles->Count.ToString() + " SKIPPED " + file->FullName);
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

	MediaSearchState ^searchDirectory(String ^path, String ^tag) {

		MediaSearchState ^state = gcnew MediaSearchState();

		if(String::IsNullOrEmpty(path) || String::IsNullOrEmpty(tag)) return(state);

		try {

			DirectoryInfo ^rootInfo = gcnew DirectoryInfo(path);
			
			state->Tags->Add(tag);

			FileUtils::walkDirectoryTree(rootInfo, callback, state);

			Regex ^regex = wildcardToRegex(tag);

			for each(FileInfo ^file in state->MediaFiles) {
/*
				if(regex->IsMatch(file->Name)) {

					state->Matches->Add(file);
				}
*/
				mediaTagMatch(file, state);
			}

		} catch (Exception ^e) {

			log->Warn("Cannot access root directory during search: " + e->Message);

		} 

		return(state);
	}
};

}