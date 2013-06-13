#pragma once

#include "MediaPreviewControl.h"
#include "ImageGridMouseEventArgs.h"
#include "ImageGridItem.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Collections::Generic;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;
using namespace System::Diagnostics;


namespace imageviewer {

	/// <summary>
	/// Summary for ImageGridControl
	/// </summary>
	public ref class ImageGridControl : public System::Windows::Forms::UserControl
	{
	public:
		ImageGridControl()
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
			toolTip = gcnew ToolTip();
			toolTip->AutoPopDelay = 32000;

			panel = nullptr;
			imageData = nullptr;
			columns = 0;
			rows = 0;
			gridWidth = 0;
			gridHeight = 0;
			nrImagePanels = 0;
			currentPage = 0;

			defaultColor = System::Drawing::Color::LightGray;
			selectedColor = System::Drawing::Color::Gold;

			useThumbnails = false;
			infoIconsEnabled = false;
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~ImageGridControl()
		{
			if (components)
			{
				delete components;
			}
		}

	private:
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System::ComponentModel::Container ^components;

#pragma region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent(void)
		{
			this->SuspendLayout();
			// 
			// ImageGridControl
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->Name = L"ImageGridControl";
			this->Size = System::Drawing::Size(905, 610);
			this->ResumeLayout(false);

		}
#pragma endregion

private:

	array<MediaPreviewControl ^> ^panel;
	Generic::IList<ImageGridItem ^> ^imageData;
	int columns;
	int rows;
	int gridWidth;
	int gridHeight;
	int nrImagePanels;
	ToolTip ^toolTip;
	int currentPage;
	Color defaultColor;
	Color selectedColor;
	bool useThumbnails;
	bool infoIconsEnabled;

	void doGridLayout() {

		if(panel == nullptr) return;

		gridWidth = Width / columns;
		gridHeight = Height / rows;

		for(int i = 0; i < getNrImagePanels(); i++) {

			panel[i]->Width = gridWidth;
			panel[i]->Height = gridHeight;

			int column = i % columns;
			int row = i / rows;

			int x = column * gridWidth;
			int y = row * gridHeight;					 

			panel[i]->Location = System::Drawing::Point(x, y);

		}
	}

	int panelToImageNr(int panelNr) {

		return(getCurrentPage() * getNrImagePanels() + panelNr);

	}

	int imageToPanelNr(int imageNr) {

		if(imageNr - getCurrentPage() * getNrImagePanels() < 0) {

			return(-1);

		} else if(imageNr - getCurrentPage() * getNrImagePanels() < getNrImagePanels()) {

			return(imageNr - getCurrentPage() * getNrImagePanels());

		} else {

			return(-1);
		}
	}

	void displayImage(int panelNr, ImageGridItem ^imageItem) {

		panel[panelNr]->loadMedia(imageItem->ImageLocation, 
			imageItem);

		if(imageItem->IsSelected) {

			panel[panelNr]->BackColor = selectedColor;

		} else {

			panel[panelNr]->BackColor = defaultColor;
		}
	
	}

	System::Void gridImage_Click(System::Object^  sender, MouseEventArgs^  e) {

		int row, column, panelNr, imageNr;

		Control ^control = dynamic_cast<Control ^>(sender);
		Point screenPoint = control->PointToScreen(e->Location);

		getImageInfoFromScreenPoint(screenPoint, row, column, panelNr, imageNr);

		if(imageNr >= getNrImages()) return;

		ImageGridMouseEventArgs^ args = 
			gcnew ImageGridMouseEventArgs(e, panelNr, imageNr, row, column, imageData[imageNr]);

		ImageGridMouseDown(sender, args);
	}

	System::Void gridImage_DoubleClick(System::Object^  sender, MouseEventArgs^  e) {

		int row, column, panelNr, imageNr;

		Control ^control = dynamic_cast<Control ^>(sender);
		Point screenPoint = control->PointToScreen(e->Location);

		getImageInfoFromScreenPoint(screenPoint, row, column, panelNr, imageNr);

		if(imageNr >= getNrImages()) return;

		ImageGridMouseEventArgs^ args = 
			gcnew ImageGridMouseEventArgs(e, panelNr, imageNr, row, column, imageData[imageNr]);

		ImageGridMouseDoubleClick(sender, args);
	}


public:

	event EventHandler<ImageGridMouseEventArgs^> ^ImageGridMouseDown;
	event EventHandler<ImageGridMouseEventArgs^> ^ImageGridMouseDoubleClick;
	event EventHandler<EventArgs ^> ^UpdateImages;

	void setNrImagePanels(int nrImagePanels, bool useThumbnails) {

		this->nrImagePanels = nrImagePanels;
		this->useThumbnails = useThumbnails;

		double x = Math::Sqrt(nrImagePanels);

		columns = (int)Math::Ceiling(x);
		rows = (int)Math::Round(x);

		if(panel != nullptr) {

			for(int i = 0; i < panel->Length; i++) {

				this->Controls->Remove(panel[i]);
			}
		}

		panel = gcnew array<MediaPreviewControl ^>(nrImagePanels);

		for(int i = 0; i < nrImagePanels; i++) {

			panel[i] = gcnew MediaPreviewControl();
			/*if(panel[i]->IsHandleCreated == false) {

			IntPtr handle = panel[i]->Handle;
			}*/

			panel[i]->PreviewMouseDown += gcnew EventHandler<MouseEventArgs ^>(this, &ImageGridControl::gridImage_Click);
			panel[i]->PreviewMouseDoubleClick += gcnew EventHandler<MouseEventArgs ^>(this, &ImageGridControl::gridImage_DoubleClick);
		
			panel[i]->BackColor = defaultColor;
			panel[i]->BorderStyle = System::Windows::Forms::BorderStyle::Fixed3D;

			this->Controls->Add(panel[i]);

		}

		doGridLayout();
	}
	void initializeImageData(Generic::IList<ImageGridItem ^> ^imageData) {

		this->imageData = imageData;

		displayPage(0);

	}

	void addImageData(ImageGridItem ^addData) {

		List<ImageGridItem ^> ^addDataList = gcnew List<ImageGridItem ^>();

		addDataList->Add(addData);

		addImageData(addDataList);
	}

	void addImageData(Generic::IList<ImageGridItem ^> ^addData) {

		if(imageData == nullptr) {

			imageData = gcnew List<ImageGridItem ^>();
		}

		for(int i = 0; i < addData->Count; i++) {

			imageData->Add(addData[i]);
			int panelNr = imageToPanelNr(getNrImages() - 1);

			if(panelNr != -1) {

				displayImage(panelNr, addData[i]);
			}

			UpdateImages(this, EventArgs::Empty);
		}


	}

	void removeAllImageData() {

		List<ImageGridItem ^> ^emptyList = gcnew List<ImageGridItem ^>();

		initializeImageData(emptyList);
	}

	void removeImageData(ImageGridItem ^removeData) {

		if(removeData == nullptr) return;

		List<ImageGridItem ^> ^removeDataList = gcnew List<ImageGridItem ^>();

		removeDataList->Add(removeData);

		removeImageData(removeDataList);
	}

	void removeImageData(Generic::List<ImageGridItem ^> ^removeData) {

		if(removeData == nullptr) return;

		int startPage = getCurrentPage();

		for(int j = 0; j < removeData->Count; j++) {

			for(int i = 0; i < getNrImages(); i++) {

				if(removeData[j]->ImageLocation->Equals(imageData[i]->ImageLocation)) {

					imageData->RemoveAt(i);
					break;
				}
			}
		}

		while(startPage >= getNrPages()) {

			if(startPage == 0) break;
			else --startPage;
		}


		displayPage(startPage);

	}

	void replaceImageData(int imageNr, ImageGridItem ^updateData) {

		System::Diagnostics::Debug::Assert(imageNr < getNrImages());

		imageData[imageNr] = updateData;

		int panelNr = imageToPanelNr(imageNr);

		if(panelNr != -1) {

			displayImage(panelNr, updateData); 
		}
	}

	void updateImageData(ImageGridItem ^updateData) {

		List<ImageGridItem ^> ^updateDataList = gcnew List<ImageGridItem ^>();

		updateDataList->Add(updateData);

		updateImageData(updateDataList);
	}

	void updateImageData(Generic::IList<ImageGridItem ^> ^updateData) {

		for(int i = 0; i < getNrImages(); i++) {

			for(int j = 0; j < updateData->Count; j++) {

				if(imageData[i]->ImageLocation->Equals(updateData[j]->ImageLocation)) {

					imageData[i] = updateData[j];
				}
			}
		}

		// only have to redraw image if it's on the current page
		for(int i = 0; i < getNrImagePanels() && panelToImageNr(i) < getNrImages(); i++) {

			int k = panelToImageNr(i);

			for(int j = 0; j < updateData->Count; j++) {

				if(imageData[k]->ImageLocation->Equals(updateData[j]->ImageLocation)) {

					displayImage(i, updateData[j]);
				}
			}
		}
	}
	ImageGridItem ^getImageData(String ^imageLocation) {

		for(int i = 0; i < getNrImages(); i++) {

			if(imageData[i]->ImageLocation->Equals(imageLocation)) {

				return(imageData[i]);
			}

		}

		return(nullptr);
	}
	ImageGridItem ^getImageData(int imageNr) {

		return(imageData[imageNr]);	
	}

	List<ImageGridItem ^> ^getSelectedImageData() {

		List<ImageGridItem ^> ^selectedImageData = gcnew List<ImageGridItem ^>();

		for(int i = 0; i < getNrImages(); i++) {

			if(imageData[i]->IsSelected) {

				selectedImageData->Add(imageData[i]);
			}
		}

		return(selectedImageData);
	}

	int getNrImagePanels() {

		return(nrImagePanels);
	}

	int getNrImages() {

		return(imageData->Count);
	}

	int getNrPages() {

		if(imageData == nullptr) return(0);

		int nrPages = (int)Math::Ceiling(double(getNrImages()) / getNrImagePanels());

		return(nrPages); 
	}

	int getCurrentPage() {

		return(currentPage);
	}

	bool displayNextPage() {

		if(currentPage + 1 < getNrPages()) {

			displayPage(currentPage + 1);
			UpdateImages(this, EventArgs::Empty);
			return(true);

		} else {

			return(false);
		}
	}

	bool displayPrevPage() {

		if(currentPage - 1 < 0) {

			return(false);

		} else {

			displayPage(currentPage - 1);
			UpdateImages(this, EventArgs::Empty);
			return(true);
		}

	}


	void displayPage(int page) {

		if(imageData == nullptr) return;

		currentPage = page;

		for(int panelNr = 0; panelNr < nrImagePanels; panelNr++) {

			int imageNr = panelToImageNr(panelNr);

			if(imageNr < getNrImages()) {

				// when changes occur rapidly this check bugs out (fix?)
				//if(!panel[panelNr]->Location->Equals(imageData[imageNr]->ImageLocation)) {

					// only update the image if it is different from the
					// currently displayed image
					displayImage(panelNr, imageData[imageNr]);
				//}

			} else {

				panel[panelNr]->BackColor = defaultColor;
				panel[panelNr]->loadMedia("", nullptr);
				panel[panelNr]->ContextMenuStrip = nullptr;
				//toolTip->SetToolTip(panel[panelNr]->getPictureBox(), L"");
			}

		}

		UpdateImages(this, EventArgs::Empty);
	}

	bool isImageSelected(int imageNr) {

		Debug::Assert(imageNr < getNrImages());

		return(imageData[imageNr]->IsSelected);
	}

	void setImageSelected(int imageNr, bool mode) {

		Debug::Assert(imageNr < getNrImages());

		if(isImageSelected(imageNr) == mode) return;
		else toggleImageSelected(imageNr);
	}

	void toggleImageSelected(int imageNr) {

		Debug::Assert(imageNr < getNrImages());

		if(imageData[imageNr]->IsSelected == true) {

			imageData[imageNr]->IsSelected = false;

		} else {

			imageData[imageNr]->IsSelected = true;
		}

		int panelNr = imageToPanelNr(imageNr);

		if(panelNr >= 0) {

			if(imageData[imageNr]->IsSelected == true) {

				panel[panelNr]->BackColor = selectedColor;

			} else {

				panel[panelNr]->BackColor = defaultColor;
			}
		}

	}

	void setSelectedForAllImages(bool isSelected) {

		for(int i = 0; i < getNrImages(); i++) {

			if(imageData[i]->IsSelected != isSelected) {

				toggleImageSelected(i);
			}
		}

	}

	void getImageInfoFromScreenPoint(Point screenPoint, int &row, int &column, int &panelNr, int &imageNr) {

		//Control ^control = dynamic_cast<Control ^>(sender);
		Point client = this->PointToClient(screenPoint);

		row = client.Y / gridHeight;
		column = client.X / gridWidth;
		panelNr = columns * row + column;
		imageNr = currentPage * getNrImagePanels() + panelNr;
	}


	bool isEmpty(int panelNr) {

		return(panel[panelNr]->IsEmpty ? true : false);
	}

	virtual void OnSizeChanged(EventArgs ^e) override {

		UserControl::OnSizeChanged(e);

		doGridLayout();		

		for(int i = 0; i < getNrImagePanels(); i++) {

			//panel[i]->resizeImage(-1,-1);

		}
	}

};


}
