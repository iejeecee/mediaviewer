#pragma once
#include "PagerControl.h"
#include "ImageGridControl.h"

using namespace System;


namespace imageviewer {

	public ref class ImageGridPagerControl : public PagerControl
	{
	private:

		ImageGridControl ^imageGrid;

	public:
	
		ImageGridPagerControl() {
		
			imageGrid = nullptr;
			beginButtonClick += gcnew EventHandler<EventArgs ^>(this, &ImageGridPagerControl::beginPage_Click);
			prevButtonClick += gcnew EventHandler<EventArgs ^>(this, &ImageGridPagerControl::prevPage_Click);
			nextButtonClick += gcnew EventHandler<EventArgs ^>(this, &ImageGridPagerControl::nextPage_Click);
			endButtonClick += gcnew EventHandler<EventArgs ^>(this, &ImageGridPagerControl::endPage_Click);
		}

		property ImageGridControl ^ImageGrid {

			ImageGridControl ^get() {

				return(imageGrid);
			}

			void set(ImageGridControl ^imageGrid) {

				this->imageGrid = imageGrid;

				if(imageGrid != nullptr) {

					imageGrid->UpdateImages += gcnew EventHandler<EventArgs ^>(this, &ImageGridPagerControl::imageGrid_UpdateImages);
				}
			}
		}


		private: System::Void beginPage_Click(System::Object^  sender, System::EventArgs^  e) {

				 imageGrid->displayPage(0);
			 }
private: System::Void prevPage_Click(System::Object^  sender, System::EventArgs^  e) {

			 imageGrid->displayPrevPage();
		 }
private: System::Void nextPage_Click(System::Object^  sender, System::EventArgs^  e) {

			 imageGrid->displayNextPage();
		 }
private: System::Void endPage_Click(System::Object^  sender, System::EventArgs^  e) {

			 imageGrid->displayPage(imageGrid->getNrPages() - 1);
		 }

 private: System::Void imageGrid_UpdateImages(System::Object^  sender, System::EventArgs^  e) {

		 if(imageGrid->getCurrentPage() == 0) {

			 PrevButtonEnabled = false;
			 BeginButtonEnabled = false;

		 } else {

			 PrevButtonEnabled = true;
			 BeginButtonEnabled = true;
		 }


		 if(imageGrid->getCurrentPage() >= imageGrid->getNrPages() - 1) {

			 NextButtonEnabled = false;
			 EndButtonEnabled = false;

		 } else {

			 NextButtonEnabled = true;
			 EndButtonEnabled = true;
		 }

		 int curPage = imageGrid->getNrPages() > 0 ? imageGrid->getCurrentPage() + 1 : 0;

		 CurrentPage = curPage;
		 TotalPages = imageGrid->getNrPages();

	 }
		
	};

}