#pragma once

#include "ImageGridControl.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;


namespace imageviewer {

	/// <summary>
	/// Summary for PagerControl
	/// </summary>
	public ref class PagerControl : public System::Windows::Forms::UserControl
	{
	private: ImageGridControl ^_imageGrid;
	public:
		property ImageGridControl ^imageGrid {

			void set(ImageGridControl ^imageGrid) {

				_imageGrid = imageGrid;

				if(imageGrid != nullptr) {

					_imageGrid->UpdateImages += gcnew EventHandler<EventArgs ^>(this, &PagerControl::imageGrid_UpdateImagesEvent);
				}
			}

			ImageGridControl ^get() {

				return(_imageGrid);
			}
			
		}

		PagerControl(void)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~PagerControl()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::Button^  beginButton;
	private: System::Windows::Forms::Button^  prevButton;
	private: System::Windows::Forms::Button^  endButton;
	protected: 

	protected: 


	private: System::Windows::Forms::Button^  nextButton;
	private: System::Windows::Forms::TextBox^  totalPagesTextBox;
	private: System::Windows::Forms::TextBox^  currentPageTextBox;
	private: System::Windows::Forms::ToolTip^  pagerToolTip;
	private: System::ComponentModel::IContainer^  components;





	protected: 


	private:
		/// <summary>
		/// Required designer variable.
		/// </summary>


#pragma region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent(void)
		{
			this->components = (gcnew System::ComponentModel::Container());
			System::ComponentModel::ComponentResourceManager^  resources = (gcnew System::ComponentModel::ComponentResourceManager(PagerControl::typeid));
			this->beginButton = (gcnew System::Windows::Forms::Button());
			this->prevButton = (gcnew System::Windows::Forms::Button());
			this->endButton = (gcnew System::Windows::Forms::Button());
			this->nextButton = (gcnew System::Windows::Forms::Button());
			this->totalPagesTextBox = (gcnew System::Windows::Forms::TextBox());
			this->currentPageTextBox = (gcnew System::Windows::Forms::TextBox());
			this->pagerToolTip = (gcnew System::Windows::Forms::ToolTip(this->components));
			this->SuspendLayout();
			// 
			// beginButton
			// 
			this->beginButton->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"beginButton.Image")));
			this->beginButton->Location = System::Drawing::Point(0, 0);
			this->beginButton->Name = L"beginButton";
			this->beginButton->Size = System::Drawing::Size(42, 36);
			this->beginButton->TabIndex = 21;
			this->pagerToolTip->SetToolTip(this->beginButton, L"First Page");
			this->beginButton->UseVisualStyleBackColor = true;
			this->beginButton->Click += gcnew System::EventHandler(this, &PagerControl::beginButton_Click);
			// 
			// prevButton
			// 
			this->prevButton->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"prevButton.Image")));
			this->prevButton->Location = System::Drawing::Point(48, 0);
			this->prevButton->Name = L"prevButton";
			this->prevButton->Size = System::Drawing::Size(42, 36);
			this->prevButton->TabIndex = 23;
			this->pagerToolTip->SetToolTip(this->prevButton, L"Previous Page");
			this->prevButton->UseVisualStyleBackColor = true;
			this->prevButton->Click += gcnew System::EventHandler(this, &PagerControl::prevButton_Click);
			// 
			// endButton
			// 
			this->endButton->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"endButton.Image")));
			this->endButton->Location = System::Drawing::Point(232, 0);
			this->endButton->Name = L"endButton";
			this->endButton->Size = System::Drawing::Size(42, 36);
			this->endButton->TabIndex = 24;
			this->pagerToolTip->SetToolTip(this->endButton, L"Last Page");
			this->endButton->UseVisualStyleBackColor = true;
			this->endButton->Click += gcnew System::EventHandler(this, &PagerControl::endButton_Click);
			// 
			// nextButton
			// 
			this->nextButton->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"nextButton.Image")));
			this->nextButton->Location = System::Drawing::Point(184, 0);
			this->nextButton->Name = L"nextButton";
			this->nextButton->Size = System::Drawing::Size(42, 36);
			this->nextButton->TabIndex = 25;
			this->pagerToolTip->SetToolTip(this->nextButton, L"Next Page");
			this->nextButton->UseVisualStyleBackColor = true;
			this->nextButton->Click += gcnew System::EventHandler(this, &PagerControl::nextButton_Click);
			// 
			// totalPagesTextBox
			// 
			this->totalPagesTextBox->Enabled = false;
			this->totalPagesTextBox->Location = System::Drawing::Point(140, 5);
			this->totalPagesTextBox->Name = L"totalPagesTextBox";
			this->totalPagesTextBox->Size = System::Drawing::Size(35, 26);
			this->totalPagesTextBox->TabIndex = 27;
			this->totalPagesTextBox->Text = L"0";
			this->totalPagesTextBox->TextAlign = System::Windows::Forms::HorizontalAlignment::Right;
			// 
			// currentPageTextBox
			// 
			this->currentPageTextBox->Enabled = false;
			this->currentPageTextBox->Location = System::Drawing::Point(99, 5);
			this->currentPageTextBox->Name = L"currentPageTextBox";
			this->currentPageTextBox->Size = System::Drawing::Size(35, 26);
			this->currentPageTextBox->TabIndex = 28;
			this->currentPageTextBox->Text = L"0";
			this->currentPageTextBox->TextAlign = System::Windows::Forms::HorizontalAlignment::Right;
			// 
			// PagerControl
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->Controls->Add(this->currentPageTextBox);
			this->Controls->Add(this->totalPagesTextBox);
			this->Controls->Add(this->nextButton);
			this->Controls->Add(this->endButton);
			this->Controls->Add(this->prevButton);
			this->Controls->Add(this->beginButton);
			this->Name = L"PagerControl";
			this->Size = System::Drawing::Size(275, 38);
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion
	private: System::Void beginButton_Click(System::Object^  sender, System::EventArgs^  e) {

				 imageGrid->displayPage(0);
			 }
private: System::Void prevButton_Click(System::Object^  sender, System::EventArgs^  e) {

			 imageGrid->displayPrevPage();
		 }
private: System::Void nextButton_Click(System::Object^  sender, System::EventArgs^  e) {

			 imageGrid->displayNextPage();
		 }
private: System::Void endButton_Click(System::Object^  sender, System::EventArgs^  e) {

			 imageGrid->displayPage(imageGrid->getNrPages() - 1);
		 }

 private: System::Void imageGrid_UpdateImagesEvent(System::Object^  sender, System::EventArgs^  e) {

		 if(imageGrid->getCurrentPage() == 0) {

			 prevButton->Enabled = false;
			 beginButton->Enabled = false;

		 } else {

			 prevButton->Enabled = true;
			 beginButton->Enabled = true;
		 }


		 if(imageGrid->getCurrentPage() >= imageGrid->getNrPages() - 1) {

			 nextButton->Enabled = false;
			 endButton->Enabled = false;

		 } else {

			 nextButton->Enabled = true;
			 endButton->Enabled = true;
		 }

		 int curPage = imageGrid->getNrPages() > 0 ? imageGrid->getCurrentPage() + 1 : 0;

		 currentPageTextBox->Text = Convert::ToString(curPage);
		 totalPagesTextBox->Text = Convert::ToString(imageGrid->getNrPages());

	 }
};
}
