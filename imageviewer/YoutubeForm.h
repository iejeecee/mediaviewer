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
	/// Summary for YoutubeForm
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class YoutubeForm : public System::Windows::Forms::Form
	{
	public:
		YoutubeForm(void)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
			imageGrid->setNrImagePanels(16, true);
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~YoutubeForm()
		{
			if (components)
			{
				delete components;
			}
		}
	private: imageviewer::ImageGridControl^  imageGrid;
	protected: 

	protected: 

	protected: 

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
			System::ComponentModel::ComponentResourceManager^  resources = (gcnew System::ComponentModel::ComponentResourceManager(YoutubeForm::typeid));
			this->imageGrid = (gcnew imageviewer::ImageGridControl());
			this->SuspendLayout();
			// 
			// imageGrid
			// 
			this->imageGrid->Location = System::Drawing::Point(18, 12);
			this->imageGrid->Name = L"imageGrid";
			this->imageGrid->Size = System::Drawing::Size(905, 611);
			this->imageGrid->TabIndex = 0;
			// 
			// YoutubeForm
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(940, 666);
			this->Controls->Add(this->imageGrid);
			this->Icon = (cli::safe_cast<System::Drawing::Icon^  >(resources->GetObject(L"$this.Icon")));
			this->Name = L"YoutubeForm";
			this->Text = L"YoutubeForm";
			this->ResumeLayout(false);

		}
#pragma endregion
	};
}
