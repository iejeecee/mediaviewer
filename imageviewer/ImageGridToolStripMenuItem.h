#pragma once

#include "ImageGridContextMenuStrip.h"
#include "ImageGridMouseEventArgs.h"

using namespace System::Windows::Forms;

namespace imageviewer {

public ref class ImageGridToolStripMenuItem : public ToolStripMenuItem
{
private:
	

public:
	
	delegate void GridMouseDownEventHandler(System::Object^ sender, ImageGridMouseEventArgs^ e);
	event GridMouseDownEventHandler ^OnGridMouseDown;

	virtual void OnMouseDown(System::Windows::Forms::MouseEventArgs ^e) override {

		ToolStripMenuItem::OnMouseDown(e);

		ImageGridContextMenuStrip ^strip = dynamic_cast<ImageGridContextMenuStrip ^>(Owner);

		int row, column, localImageNr, globalImageNr;

		ImageGridItem ^item = strip->getImageInfo(row, column, localImageNr, globalImageNr);

		ImageGridMouseEventArgs ^gridEvent = gcnew ImageGridMouseEventArgs(e,localImageNr,globalImageNr, row, column, item);

		OnGridMouseDown(this, gridEvent);

	}

};

}