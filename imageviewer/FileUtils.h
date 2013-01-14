#pragma once
#include "ProgressForm.h"
#include "FileUtilsEventArgs.h"
#include "GEventArgs.h"

using namespace System;
using namespace System::Security::Permissions;
//using namespace System::Runtime::InteropServices;
using namespace System::Collections::Specialized;

namespace imageviewer {

using namespace System::Runtime::InteropServices;

public ref class FileUtils
{

private:

	enum class CopyFileCallbackAction
	{
		CONTINUE = 0,
		CANCEL = 1,
		STOP = 2,
		QUIET = 3
	};

	enum class CopyFileOptions
	{
		NONE = 0x0,
		FAIL_IF_DESTINATION_EXISTS = 0x1,
		RESTARTABLE = 0x2,
		ALLOW_DECRYPTED_DESTINATION = 0x8,
		ALL = FAIL_IF_DESTINATION_EXISTS | RESTARTABLE | ALLOW_DECRYPTED_DESTINATION
	};

	enum class CopyProgressCallbackReason
	{
		CALLBACK_CHUNK_FINISHED = 0x0,
		CALLBACK_STREAM_SWITCH  = 0x1
	};


	enum class CreateFileAccess 
	{
		GENERIC_READ = 0x80000000,
		GENERIC_WRITE = 0x40000000
	};


	enum class CreateFileShare 
	{
		NONE = 0x00000000,
		FILE_SHARE_READ = 0x00000001
	};

	enum class CreateFileCreationDisposition 
	{
		OPEN_EXISTING = 3
	};

	enum class CreateFileAttributes 
	{
		NORMAL = 0x00000080,
		FILE_FLAG_RANDOM_ACCESS = 0x10000000
	};

	const static int ERROR_SHARING_VIOLATION = 32;

	enum class Action
	{
		COPY,
		MOVE
	};

	ref class AsyncState {

	private:

		void progressForm_CancelEvent(Object ^sender, EventArgs ^e) {

			copyCallbackAction = CopyFileCallbackAction::CANCEL;
		}	

	public:

		StringCollection ^sourcePaths;
		StringCollection ^destPaths;
		ProgressForm ^progressForm;

		CopyFileCallbackAction copyCallbackAction;

		AsyncState(StringCollection ^sourcePaths, StringCollection ^destPaths) {

			this->sourcePaths = sourcePaths;
			this->destPaths = destPaths;
			progressForm = gcnew ProgressForm();
			progressForm->OnCancelEvent += gcnew ProgressForm::CancelEventHandler(this, &AsyncState::progressForm_CancelEvent);

			copyCallbackAction = CopyFileCallbackAction::CONTINUE;
		}


	};

	delegate int CopyProgressDelegate(
		__int64 totalFileSize, __int64 totalBytesTransferred, __int64 streamSize, 
		__int64 streamBytesTransferred, int streamNumber, CopyProgressCallbackReason callbackReason,
		IntPtr sourceFile, IntPtr destinationFile, IntPtr data);

	[System::Security::SuppressUnmanagedCodeSecurity]
	[DllImport("Kernel32.dll", CharSet=CharSet::Auto, SetLastError = true, CallingConvention=CallingConvention::Winapi)]
	static bool CopyFileEx(
		String ^lpExistingFileName, String ^lpgcnewFileName,
		CopyProgressDelegate ^lpProgressRoutine,
		IntPtr lpData, int *pbCancel, int dwCopyFlags);

	[System::Security::SuppressUnmanagedCodeSecurity]
	[DllImport("kernel32.dll", CharSet = CharSet::Auto, SetLastError = true, CallingConvention=CallingConvention::Winapi)]
	static IntPtr CreateFileW(
	   String ^lpFileName,
	   int dwDesiredAccess,
	   int dwShareMode,
	   IntPtr lpSecurityAttributes,
	   int dwCreationDisposition,
	   int dwFlagsAndAttributes,
	   IntPtr hTemplateFile);

	int copyProgress(__int64 totalFileSize, __int64 totalBytesTransferred, __int64 streamSize, 
		__int64 streamBytesTransferred, int streamNumber, CopyProgressCallbackReason callbackReason,
		IntPtr sourceFile, IntPtr destinationFile, IntPtr data) 
	{

		int fileProgress = int((100 * totalBytesTransferred) / totalFileSize);

		GCHandle h = GCHandle::FromIntPtr(data);
		AsyncState ^state = (AsyncState ^)h.Target;

		state->progressForm->itemProgressValue = fileProgress;

		return((int)state->copyCallbackAction);

	}

	bool copyFile(String ^source, String ^destination, CopyFileOptions options, AsyncState ^state)
	{

		GCHandle handle = GCHandle::Alloc(state);
		IntPtr statePtr = GCHandle::ToIntPtr(handle);

		try {

			FileIOPermission ^sourcePermission = gcnew FileIOPermission(FileIOPermissionAccess::Read, source);
			sourcePermission->Demand();

			FileIOPermission ^destinationPermission = gcnew FileIOPermission(
				FileIOPermissionAccess::Write, destination);
			destinationPermission->Demand();

			String ^destinationDir = System::IO::Path::GetDirectoryName(destination);

			if(!Directory::Exists(destinationDir)) {

				Directory::CreateDirectory(destinationDir);
				OnAfterCopy(this, gcnew FileUtilsEventArgs(destinationDir, true));
			}

			CopyProgressDelegate ^progressCallback = gcnew CopyProgressDelegate(this, &imageviewer::FileUtils::copyProgress);

			int cancel = 0;

			if(!CopyFileEx(source, destination, progressCallback, 
				statePtr, &cancel, (int)options))
			{

				Win32Exception ^win32exception = gcnew Win32Exception();

				if(win32exception->NativeErrorCode == 1235) {

					// copy was cancelled
					return(false);
				}

				throw gcnew IOException(win32exception->Message);
			}

			return(true);

		} finally {

			handle.Free();
		}
	}

	void getAllFiles(String ^path, StringCollection ^directories, StringCollection ^files) {

		cli::array<String ^> ^directoriesInPath = Directory::GetDirectories(path);

		for each(String ^directory in directoriesInPath) {

			getAllFiles(directory, directories, files);

			if(directories != nullptr) {
				directories->Add(directory);
			}
		}

		if(files == nullptr) return;

		cli::array<String ^> ^filesInPath = Directory::GetFiles(path);

		for each(String ^file in filesInPath) {

			files->Add(file);
		}

	}

	bool addIfNotExists(String ^path, StringCollection ^paths) {

		bool containsPath = paths->Contains(path);

		if(!containsPath) {

			paths->Add(path);

		}

		return(!containsPath);
	}


	void asyncCopy(Object ^args) {

		AsyncState ^state = dynamic_cast<AsyncState ^>(args);

		try {

			StringCollection ^sourcePaths = state->sourcePaths;
			StringCollection ^destPaths = state->destPaths;

			StringCollection ^movePaths = gcnew StringCollection();
			StringCollection ^moveDestPaths = gcnew StringCollection();

			StringCollection ^copyPaths = gcnew StringCollection();
			StringCollection ^copyDestPaths = gcnew StringCollection();

			StringCollection ^removePaths = gcnew StringCollection();

			getPaths(sourcePaths, destPaths, movePaths, moveDestPaths, copyPaths, copyDestPaths, removePaths, Action::COPY);

			state->progressForm->totalProgressMaximum = movePaths->Count + copyPaths->Count;

			bool success = true;

			for(int i = 0; i < movePaths->Count; i++) {

				state->progressForm->totalProgressValue = i;
				state->progressForm->itemInfo = movePaths[i];

				success = copyFile(movePaths[i], moveDestPaths[i], CopyFileOptions::ALL, state);

				if(success == false) break;

				bool isDirectory = Directory::Exists(moveDestPaths[i]);

				OnAfterCopy(this, gcnew FileUtilsEventArgs(moveDestPaths[i], isDirectory));

				//state->progressForm->addInfoString(movePaths[i] + " -> " + moveDestPaths[i]);
			}


			for(int i = 0; (i < copyPaths->Count) && (success == true); i++) {

				state->progressForm->totalProgressValue = movePaths->Count + i;
				state->progressForm->itemInfo = copyPaths[i];

				success = copyFile(copyPaths[i], copyDestPaths[i], CopyFileOptions::ALL, state);

				if(success == false) break;

				OnAfterCopy(this, gcnew FileUtilsEventArgs(copyDestPaths[i], false));

				//state->progressForm->addInfoString(copyPaths[i] + " -> " + copyDestPaths[i]);

			}

			if(success == true) {

				state->progressForm->totalProgressValue = state->progressForm->totalProgressMaximum;
			}

		} catch (Exception ^e) {

			MessageBox::Show(e->Message, "Copy Exception");
		}

		state->progressForm->actionFinished();

	}

	void getPaths(StringCollection ^sourcePaths, StringCollection ^destPaths,
		StringCollection ^movePaths, StringCollection ^moveDestPaths,
		StringCollection ^copyPaths, StringCollection ^copyDestPaths,
		StringCollection ^removePaths, Action action) 
	{

		for(int i = 0; i < sourcePaths->Count; i++) {

			String ^sourceRoot = System::IO::Path::GetPathRoot(sourcePaths[i]);
			String ^destRoot = System::IO::Path::GetPathRoot(destPaths[i]);

			if(sourcePaths[i]->Equals(destPaths[i])) {
				
				if(action == Action::MOVE) {

					// don't do anything

				} else {

					if(Directory::Exists(sourcePaths[i])) continue;

					// copy to unique filename
					copyPaths->Add(sourcePaths[i]);
					copyDestPaths->Add(getUniqueFileName(sourcePaths[i]));
				}

			} else if(sourceRoot->Equals(destRoot)) {

				// files can be moved on the same drive
				movePaths->Add(sourcePaths[i]);
				moveDestPaths->Add(destPaths[i]);

			} else {

				// files need to be copied between drives
				if(Directory::Exists(sourcePaths[i])) {

					// file is a directory, get all it's subdirectories and files
					StringCollection ^subPaths = gcnew StringCollection();

					addIfNotExists(sourcePaths[i], removePaths);

					getAllFiles(sourcePaths[i], nullptr, subPaths);

					for each(String ^subPath in subPaths) {

						if(addIfNotExists(subPath, copyPaths)) {

							String ^postfix = subPath->Remove(0,sourcePaths[i]->Length);

							String ^destPath = destPaths[i] + postfix;

							copyDestPaths->Add(destPath);
						}
					}

				} else {


					if(addIfNotExists(sourcePaths[i], copyPaths)) {

						copyDestPaths->Add(destPaths[i]);
					}
				}
			}
		}		
	}

	void asyncMove(Object ^args) {

		AsyncState ^state = dynamic_cast<AsyncState ^>(args);

		try {

			StringCollection ^sourcePaths = state->sourcePaths;
			StringCollection ^destPaths = state->destPaths;

			StringCollection ^movePaths = gcnew StringCollection();
			StringCollection ^moveDestPaths = gcnew StringCollection();

			StringCollection ^copyPaths = gcnew StringCollection();
			StringCollection ^copyDestPaths = gcnew StringCollection();

			StringCollection ^removePaths = gcnew StringCollection();

			getPaths(sourcePaths, destPaths, movePaths, moveDestPaths, copyPaths, copyDestPaths, removePaths, Action::MOVE);

			state->progressForm->totalProgressMaximum = movePaths->Count + copyPaths->Count;

			for(int i = 0; i < movePaths->Count; i++) {

				state->progressForm->totalProgressValue = i;
				state->progressForm->itemInfo = movePaths[i];

				System::IO::Directory::Move(movePaths[i], moveDestPaths[i]);

				bool isDirectory = Directory::Exists(moveDestPaths[i]);

				OnAfterCopy(this, gcnew FileUtilsEventArgs(moveDestPaths[i], isDirectory));
				OnAfterDelete(this, gcnew FileUtilsEventArgs(movePaths[i], isDirectory));

				//state->progressForm->addInfoString(movePaths[i] + " -> " + moveDestPaths[i]);
			}

			bool success = true;

			for(int i = 0; i < copyPaths->Count; i++) {

				state->progressForm->totalProgressValue = movePaths->Count + i;
				state->progressForm->itemInfo = copyPaths[i];

				success = copyFile(copyPaths[i], copyDestPaths[i], CopyFileOptions::ALL, state);

				if(success == false) break;

				OnAfterCopy(this, gcnew FileUtilsEventArgs(copyDestPaths[i], false));

				//state->progressForm->addInfoString(copyPaths[i] + " -> " + copyDestPaths[i]);

			}

			if(success == true) {

				state->progressForm->totalProgressValue = state->progressForm->totalProgressMaximum;

				for each(String ^copySource in copyPaths) {

					System::IO::File::Delete(copySource);
					OnAfterDelete(this, gcnew FileUtilsEventArgs(copySource, false));
				}

				for each(String ^directorySource in removePaths) {

					System::IO::Directory::Delete(directorySource, true);
					OnAfterDelete(this, gcnew FileUtilsEventArgs(directorySource, true));
				}
			}

		} catch (Exception ^e) {

			MessageBox::Show(e->Message, "Move Exception");
		}

		state->progressForm->actionFinished();

	}

	static bool isFileLockedException(IOException ^exception)
	{
		int errorCode = Marshal::GetHRForException(exception) & ((1 << 16) - 1);
		return errorCode == 32 || errorCode == 33;
	}

public:

	delegate void FileUtilsDelegate(System::Object ^sender, FileUtilsEventArgs ^e);
	event FileUtilsDelegate ^OnAfterCopy;
	event FileUtilsDelegate ^OnAfterDelete;

	FileUtils() {


	}

	void copy(StringCollection ^sourcePaths, StringCollection ^destPaths) {

		if(sourcePaths->Count != destPaths->Count) {

			throw gcnew System::ArgumentException();
		}

		WaitCallback ^asyncCopy = gcnew WaitCallback(this, &FileUtils::asyncCopy);

		AsyncState ^args = gcnew AsyncState(sourcePaths, destPaths);
		args->progressForm->Text = "Copy Files";
		args->progressForm->Show();

		ThreadPool::QueueUserWorkItem(asyncCopy, args);

	}

	void move(StringCollection ^sourcePaths, StringCollection ^destPaths) {

		if(sourcePaths->Count != destPaths->Count) {

			throw gcnew System::ArgumentException();
		}

		WaitCallback ^asyncMove = gcnew WaitCallback(this, &FileUtils::asyncMove);

		AsyncState ^args = gcnew AsyncState(sourcePaths, destPaths);
		args->progressForm->Text = "Moving Files";
		args->progressForm->Show();

		ThreadPool::QueueUserWorkItem(asyncMove, args);

	}

	void remove(StringCollection ^sourcePaths) {

	}

	static Stream ^waitForFileAccess(String ^filePath, FileAccess access, int timeoutMs, 
		ModifiableGEventArgs<bool> ^isCancelled)
	{
		IntPtr fHandle;
		int errorCode;
		DateTime start = DateTime::Now;

		int desiredAccess = (int)CreateFileAccess::GENERIC_READ; 
		int fileShare = (int)CreateFileShare::FILE_SHARE_READ;

		if(access == FileAccess::Write || access == FileAccess::ReadWrite) {

			desiredAccess |= (int)CreateFileAccess::GENERIC_WRITE; 
			fileShare = (int)CreateFileShare::NONE;
		}

		int creationDisposition = (int)CreateFileCreationDisposition::OPEN_EXISTING;
		int fileAttributes = (int)(CreateFileAttributes::NORMAL | CreateFileAttributes::FILE_FLAG_RANDOM_ACCESS);

		while(true)
		{
			
			fHandle = CreateFileW(filePath, desiredAccess, fileShare, IntPtr::Zero,
				creationDisposition, fileAttributes, IntPtr::Zero);

			if(fHandle != IntPtr::Zero && fHandle.ToInt64() != -1L) {

				Microsoft::Win32::SafeHandles::SafeFileHandle ^handle = gcnew Microsoft::Win32::SafeHandles::SafeFileHandle(fHandle, true);

				return gcnew FileStream(handle, access);

			}

			errorCode = Marshal::GetLastWin32Error();

			if (errorCode != ERROR_SHARING_VIOLATION)
				break;
			if (timeoutMs >= 0 && (DateTime::Now - start).TotalMilliseconds > timeoutMs)
				break;
			if (isCancelled != nullptr && isCancelled->Value == true)
				break;
			Thread::Sleep(100);
		}


		Win32Exception ^e = gcnew System::ComponentModel::Win32Exception(errorCode);

		throw gcnew IOException(e->Message, errorCode);
	}


/*
	static FileStream ^openAndLockFile(String ^fileName, FileAccess fileAccess, FileShare fileShare, int maxAttempts, int timeoutMs, bool ignoreIOExceptions) {

		int attempt = 0;

		while(true) {

			try {

				return File::Open(fileName, FileMode::Open, fileAccess, fileShare); 

			} catch(IOException ^e) {

				if(!isFileLockedException(e)) {

					// file is not locked but some other exception happend
					// rethrow the exception

					if(ignoreIOExceptions == false) {
						throw;
					}
				}

				if(++attempt > maxAttempts) {

					// attempts to open file exceeded
					throw;
				}

				Thread::Sleep(timeoutMs);

			}
		}
	}
*/
	static bool isFileLocked(String ^fileName, bool ignoreIOExceptions) {

		FileStream ^stream = nullptr;

		try {

			stream = File::Open(fileName, FileMode::Open, FileAccess::Read, FileShare::None); 

		} catch(IOException ^e) {

			if(isFileLockedException(e)) {

				return(true);
			}

			// file is not locked but some other exception happend
			// rethrow the exception
			if(ignoreIOExceptions == false) {
				throw;
			}

		} finally {

			if(stream != nullptr) {

				stream->Close();
			}
		}

		return(false);
	}

	static String ^getUniqueFileName(String ^fileName) {

		String ^uniqueName = fileName;
		String ^dir = Path::GetDirectoryName(fileName);
		String ^name = Path::GetFileNameWithoutExtension(fileName);
		String ^ext = Path::GetExtension(fileName);

		int i = 0;

		while(File::Exists(uniqueName)) {

			uniqueName = dir + "\\" + name + " (" + Convert::ToString(++i) + ")" + ext;

		}

		return(uniqueName);
	}
};

}