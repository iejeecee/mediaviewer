#pragma once

namespace imageviewer {

using namespace System;
using namespace System::Windows::Forms;
using namespace System::Collections::Generic;
using namespace System::Drawing;


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
};

}