#pragma once

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Collections::Generic;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;
using namespace System::IO;


namespace imageviewer {

	public ref class DirectoryBrowserAsyncState 
	{
	public:
		String ^path;
		TreeNode ^parent;
		List<DirectoryInfo ^> ^directories;
		List<bool> ^hasSubDirs;

		DirectoryBrowserAsyncState() {

			directories = gcnew List<DirectoryInfo ^>();
			hasSubDirs = gcnew List<bool>();
		}
		
	};

}