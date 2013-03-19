#pragma once

#include "Util.h"
#include "FileUtils.h"

using namespace System;
using namespace System::IO;
using namespace System::Security::Permissions;
using namespace System::Collections::Generic;

namespace imageviewer {

	public ref class MediaFileWatcher
	{
	private:

		FileSystemWatcher ^watcher;
		List<String ^> ^mediaFiles;
		String ^currentMediaFile;

		void findImageFiles(String ^path) {

			DirectoryInfo imageDirInfo(path);

			mediaFiles->Clear();

			cli::array<FileInfo^> ^fileInfo = imageDirInfo.GetFiles();

			for(int i = 0; i < fileInfo->Length; i++) {

				if(MediaFormatConvert::isMediaFile(fileInfo[i]->FullName)) 
				{
					mediaFiles->Add(fileInfo[i]->FullName);
				}				

			}
		}

		void FileChanged(System::Object ^sender, System::IO::FileSystemEventArgs ^e)  {

			if(MediaFormatConvert::isMediaFile(e->FullPath)) {

				if(currentMediaFile->Equals(e->FullPath)) {

					FileSystemEventArgs ^args = newFileSystemEventArgs(System::IO::WatcherChangeTypes::Changed, currentMediaFile);

					CurrentMediaChanged(this, args);
				}

				MediaChanged(this, e);
			}

		}

		void FileCreated(System::Object ^sender, System::IO::FileSystemEventArgs ^e)  {

			if(MediaFormatConvert::isMediaFile(e->FullPath)) {

				mediaFiles->Add(e->FullPath);
				MediaCreated(this, e);

			}

		}

		void FileDeleted(System::Object ^sender, System::IO::FileSystemEventArgs ^e)  {

			if(MediaFormatConvert::isMediaFile(e->FullPath)) {

				int removeIndex = getIndexOf(e->FullPath);

				if(removeIndex != -1) {

					mediaFiles->RemoveAt(removeIndex);
				}

				if(currentMediaFile->Equals(e->FullPath)) {

					currentMediaFile = "";

					FileSystemEventArgs ^args = newFileSystemEventArgs(System::IO::WatcherChangeTypes::Deleted, e->FullPath);

					CurrentMediaChanged(this, args);
				}

				MediaDeleted(this, e);
			}
		}

		void FileRenamed(System::Object ^sender, System::IO::RenamedEventArgs ^e)  {

			if(MediaFormatConvert::isMediaFile(e->FullPath)) {

				int removeIndex = getIndexOf(e->OldFullPath);

				if(removeIndex != -1) {

					mediaFiles->RemoveAt(removeIndex);
				}

				mediaFiles->Add(e->FullPath);

				if(currentMediaFile->Equals(e->OldFullPath)) {

					FileSystemEventArgs ^args = newFileSystemEventArgs(System::IO::WatcherChangeTypes::Renamed, e->FullPath);

					CurrentMediaChanged(this, args);
				}

				MediaRenamed(this, e);
			}

		}

		int getIndexOf(String ^imageFile) {

			for(int i = 0; i < mediaFiles->Count; i++) {

				if(mediaFiles[i]->Equals(imageFile)) {

					return(i);
				}
			}

			return(-1);
		}

		FileSystemEventArgs ^newFileSystemEventArgs(System::IO::WatcherChangeTypes changeType, String ^location) {

			String ^directory = System::IO::Path::GetDirectoryName(location);
			String ^name = System::IO::Path::GetFileName(location);

			FileSystemEventArgs ^e = gcnew FileSystemEventArgs(changeType, directory, name);

			return(e);
		}

	public:

		event System::IO::FileSystemEventHandler ^MediaChanged;
		event System::IO::FileSystemEventHandler ^MediaCreated;
		event System::IO::FileSystemEventHandler ^MediaDeleted;
		event System::IO::RenamedEventHandler ^MediaRenamed;

		event EventHandler<FileSystemEventArgs ^> ^CurrentMediaChanged;

		property String ^Path {

			void set(String ^path) {

				findImageFiles(path);

				watcher->Path = path;
				watcher->EnableRaisingEvents = true;
				
			}

			String ^get() {

				return(watcher->Path);
			}

		}

		property String ^CurrentMediaFile {

			public: void set(String ^currentMediaFile) {

				this->currentMediaFile = currentMediaFile;
			}

			public: String ^get() {

				return(currentMediaFile);
			}

		}

		property List<String ^> ^MediaFiles {

			public: List<String ^> ^get() {

				return(mediaFiles);
			}

			private: void set(List<String ^> ^mediaFiles) {

				this->mediaFiles = mediaFiles;
			}
		}

		[PermissionSet(SecurityAction::Demand, Name="FullTrust")]
		MediaFileWatcher() {

			watcher = gcnew FileSystemWatcher();
			mediaFiles = gcnew List<String ^>();

			/* Watch for changes in LastAccess and LastWrite times, and 
			the renaming of files or directories. */
			watcher->NotifyFilter = static_cast<NotifyFilters>(NotifyFilters::LastAccess |
				NotifyFilters::LastWrite | NotifyFilters::FileName | NotifyFilters::DirectoryName);

			// Only watch text files.
			watcher->Filter = "*.*";

			// Add event handlers.
			watcher->Changed += gcnew FileSystemEventHandler( this, &MediaFileWatcher::FileChanged );
			watcher->Created += gcnew FileSystemEventHandler( this, &MediaFileWatcher::FileCreated );
			watcher->Deleted += gcnew FileSystemEventHandler( this, &MediaFileWatcher::FileDeleted );
			watcher->Renamed += gcnew System::IO::RenamedEventHandler( this, &MediaFileWatcher::FileRenamed );

			currentMediaFile = "";
		}

		void setNextImageFile() {

			int index = getIndexOf(currentMediaFile);

			if(index == -1 || mediaFiles->Count == 1) return;
			
			index = (index + 1) % mediaFiles->Count;

			currentMediaFile = mediaFiles[index];
		
			FileSystemEventArgs ^e = newFileSystemEventArgs(System::IO::WatcherChangeTypes::Changed, currentMediaFile);

			CurrentMediaChanged(this, e);

		}

		void setPrevImageFile() {

			int index = getIndexOf(currentMediaFile);

			if(index == -1 || mediaFiles->Count == 1) return;
			
			index = (index - 1) < 0 ? mediaFiles->Count - 1 : index - 1;

			currentMediaFile = mediaFiles[index];

			FileSystemEventArgs ^e = newFileSystemEventArgs(System::IO::WatcherChangeTypes::Changed, currentMediaFile);

			CurrentMediaChanged(this, e);

		}

	};

}
