#pragma once

namespace imageviewer {

public ref class FileUtilsEventArgs : public System::EventArgs
{
private:
	String ^_filePath;
	bool _isDirectory;

public:
	FileUtilsEventArgs(String ^filePath, bool isDirectory) {

		this->filePath = filePath;
		this->isDirectory = isDirectory;
	}

	property String ^filePath {

		String ^get() { return(_filePath); }
		void set(String ^filePath) { _filePath = filePath; }
	}

	property bool isDirectory {

		bool get() { return(_isDirectory); }
		void set(bool isDirectory) { _isDirectory = isDirectory; }
	}
};

}