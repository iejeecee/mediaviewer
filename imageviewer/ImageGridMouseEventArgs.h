#pragma once

#include "ImageGridItem.h"

using namespace System::Windows::Forms;

namespace imageviewer {

public ref class ImageGridMouseEventArgs : public MouseEventArgs
{
private:
	int _panelNr;
	int _imageNr;
	int _row;
	int _column;
	ImageGridItem ^_item;

public:
	ImageGridMouseEventArgs(MouseEventArgs ^mouseEvent, int panelNr, int imageNr, int row,
		int column, ImageGridItem ^item) :
	  MouseEventArgs(mouseEvent->Button, mouseEvent->Clicks, mouseEvent->X, 
			mouseEvent->Y, mouseEvent->Delta)
	{

		_panelNr = panelNr;
		_row = row;
		_column = column;
		_item = item;
		_imageNr = imageNr;
	}

	property int panelNr {

		int get() { return(_panelNr); }
		void set(int panelNr) { _panelNr = panelNr; }
	}

	property int imageNr {

		int get() { return(_imageNr); }
		void set(int imageNr) { _imageNr = imageNr; }
	}

	property int row {

		int get() { return(_row); }
		void set(int row) { _row = row; }
	}

	property int column {

		int get() { return(_column); }
		void set(int column) { _column = column; }
	}

	property ImageGridItem ^item {

		ImageGridItem ^get() { return(_item); }
		void set(ImageGridItem ^item) { _item = item; }
	}

};

}