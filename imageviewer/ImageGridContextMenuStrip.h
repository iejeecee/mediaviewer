#pragma once

#include "ImageGridControl.h"

using namespace System::Windows::Forms;

namespace imageviewer {

public ref class ImageGridContextMenuStrip : public ContextMenuStrip
{
private:

	ImageGridControl ^imageGrid;
	Point screenPoint;

public:
	
	ImageGridContextMenuStrip(ImageGridControl ^imageGrid) {

		this->imageGrid = imageGrid;

	}

	virtual void OnOpened(System::EventArgs ^e) override {
		
		ContextMenuStrip::OnOpened(e);

		// Get cursor position in screen coordinates
		screenPoint = Cursor::get()->Position;

	}
	

	ImageGridItem ^getImageInfo(int &row, int &column, int &localImageNr, int &globalImageNr) {

		imageGrid->getImageInfoFromScreenPoint(screenPoint, row, column, localImageNr, globalImageNr);

		return(imageGrid->getImageData(globalImageNr));
	}

};

}