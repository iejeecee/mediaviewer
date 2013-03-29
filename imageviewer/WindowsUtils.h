#pragma once
#include "MediaFormatConvert.h"

namespace imageviewer {

using namespace System;
using namespace System::Windows::Forms;
using namespace System::Collections::Generic;
using namespace System::Drawing;
using namespace System::Runtime::InteropServices;


public ref class WindowsUtils 
{
private:

  static const int TBM_GETCHANNELRECT = 0x400 + 26;
  static const int TBM_GETTHUMBRECT = 0x400 + 25;

  [StructLayout(LayoutKind::Sequential)]
  ref struct Rect { 
	  int left; 
	  int top;
	  int right; 
	  int bottom;
  };

  [DllImport("user32.dll", EntryPoint = "SendMessageW")]
  static IntPtr SendMessageW(IntPtr hWnd, int msg, IntPtr wp, Rect ^lp);

public:

	static Rectangle getTrackBarThumbRect(TrackBar ^trackBar) {
    
		Rect ^rect = gcnew Rect();

		SendMessageW(trackBar->Handle, TBM_GETTHUMBRECT, IntPtr::Zero, rect);
		return Rectangle(rect->left, rect->top, rect->right - rect->left, 
			rect->bottom - rect->top);
    }

	static Rectangle getTrackBarChannelRect(TrackBar ^trackBar) {
    
		Rect ^rect = gcnew Rect();

		SendMessageW(trackBar->Handle, TBM_GETCHANNELRECT, IntPtr::Zero, rect);
		return Rectangle(rect->left, rect->top, rect->right - rect->left,
			rect->bottom - rect->top);
    }

	static OpenFileDialog ^createOpenMediaFileDialog(bool imageOnly) {

		OpenFileDialog ^openFileDialog = gcnew OpenFileDialog();

		String ^imageFiles = "Image Files|";
		String ^videoFiles = "Video Files|";
		String ^allFiles = "All Files|*.*";

		for each(KeyValuePair<String ^, String ^> ^pair in MediaFormatConvert::extToMimeType) {

			String ^filter = "*." + pair->Key + ";";

			if(pair->Value->StartsWith("image")) {

				imageFiles += filter;

			} else {

				videoFiles += filter;
			}
		}

		imageFiles = imageFiles->Remove(imageFiles->Length - 1) + "|";
		videoFiles = videoFiles->Remove(videoFiles->Length - 1) + "|";

		if(imageOnly == true) {

			videoFiles = "";
		}

		openFileDialog->Filter = imageFiles + videoFiles + allFiles;
		openFileDialog->FilterIndex = 1;

		return(openFileDialog);
	}
};

}