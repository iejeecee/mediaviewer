#pragma once
#include "FileUtils.h"
#include "MediaFormatConvert.h"
#include "FileMetaData.h"
#include "MediaSearchState.h"

namespace imageviewer {

using namespace System;
using namespace System::IO;

public ref class MediaSearch
{
private:

  static log4net::ILog ^log = log4net::LogManager::GetLogger(System::Reflection::MethodBase::GetCurrentMethod()->DeclaringType);

  void searchFile(FileInfo ^file, Object ^state) {

	  if(!MediaFormatConvert::isMediaFile(file->Name)) return;

	  MediaSearchState ^state = dynamic_cast<MediaSearchState ^>(state);

	  try {

		  FileMetaData ^metaData = gcnew FileMetaData(file->FullName);

		  for each(String ^tag in metaData->Tags) {

			  if(state->Tags->Contains(tag)) {

				  state->Matches->Add(file); 
			  }
		  }

	  } catch (Exception ^e) {

		  log->Warning("Cannot open metadata during search: " + e->Message);
	  }
  }

public:

	MediaSearchState ^searchDirectory(String ^path, String ^tag) {

		MediaSearchState ^state = gcnew MediaSearchState();

		try {

			DirectoryInfo ^rootInfo = gcnew DirectoryInfo(path);

			FileUtils::WalkDirectoryTreeDelegate ^callback = gcnew 
				WalkDirectoryTreeDelegate(this, &MediaSearch::searchFile);
			
			state->Tags->Add(tag);

			FileUtils::walkDirectoryTree(rootInfo, callback, state);

		} catch (Exception ^e) {

			log->Warn("Cannot access root directory during search: " + e->Message);

		} finally {

			return(state);
		}

	}
};

}