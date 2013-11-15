#pragma once

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;


namespace imageviewer {

	/// <summary>
	/// Summary for SearchOptionsForm
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class SearchOptionsForm : public System::Windows::Forms::Form
	{
	public:
		SearchOptionsForm(void)
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
		~SearchOptionsForm()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::CheckBox^  searchFilenamesCheckBox;
	private: System::Windows::Forms::CheckBox^  searchTagsCheckBox;
	private: System::Windows::Forms::CheckBox^  recurseSubdirsCheckBox;
	protected: 



	private: System::Windows::Forms::Button^  okButton;
	private: System::Windows::Forms::Button^  cancelButton;


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
			this->searchFilenamesCheckBox = (gcnew System::Windows::Forms::CheckBox());
			this->searchTagsCheckBox = (gcnew System::Windows::Forms::CheckBox());
			this->recurseSubdirsCheckBox = (gcnew System::Windows::Forms::CheckBox());
			this->okButton = (gcnew System::Windows::Forms::Button());
			this->cancelButton = (gcnew System::Windows::Forms::Button());
			this->SuspendLayout();
			// 
			// searchFilenamesCheckBox
			// 
			this->searchFilenamesCheckBox->AutoSize = true;
			this->searchFilenamesCheckBox->Location = System::Drawing::Point(23, 26);
			this->searchFilenamesCheckBox->Name = L"searchFilenamesCheckBox";
			this->searchFilenamesCheckBox->Size = System::Drawing::Size(156, 24);
			this->searchFilenamesCheckBox->TabIndex = 0;
			this->searchFilenamesCheckBox->Text = L"Search Filenames";
			this->searchFilenamesCheckBox->UseVisualStyleBackColor = true;
			// 
			// searchTagsCheckBox
			// 
			this->searchTagsCheckBox->AutoSize = true;
			this->searchTagsCheckBox->Location = System::Drawing::Point(23, 56);
			this->searchTagsCheckBox->Name = L"searchTagsCheckBox";
			this->searchTagsCheckBox->Size = System::Drawing::Size(118, 24);
			this->searchTagsCheckBox->TabIndex = 1;
			this->searchTagsCheckBox->Text = L"Search Tags";
			this->searchTagsCheckBox->UseVisualStyleBackColor = true;
			// 
			// recurseSubdirsCheckBox
			// 
			this->recurseSubdirsCheckBox->AutoSize = true;
			this->recurseSubdirsCheckBox->Location = System::Drawing::Point(23, 86);
			this->recurseSubdirsCheckBox->Name = L"recurseSubdirsCheckBox";
			this->recurseSubdirsCheckBox->Size = System::Drawing::Size(194, 24);
			this->recurseSubdirsCheckBox->TabIndex = 2;
			this->recurseSubdirsCheckBox->Text = L"Recurse Subdirectories";
			this->recurseSubdirsCheckBox->UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this->okButton->DialogResult = System::Windows::Forms::DialogResult::OK;
			this->okButton->Location = System::Drawing::Point(43, 137);
			this->okButton->Name = L"okButton";
			this->okButton->Size = System::Drawing::Size(98, 49);
			this->okButton->TabIndex = 3;
			this->okButton->Text = L"Ok";
			this->okButton->UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this->cancelButton->DialogResult = System::Windows::Forms::DialogResult::Cancel;
			this->cancelButton->Location = System::Drawing::Point(147, 137);
			this->cancelButton->Name = L"cancelButton";
			this->cancelButton->Size = System::Drawing::Size(98, 49);
			this->cancelButton->TabIndex = 4;
			this->cancelButton->Text = L"Cancel";
			this->cancelButton->UseVisualStyleBackColor = true;
			// 
			// SearchOptionsForm
			// 
			this->AcceptButton = this->okButton;
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->CancelButton = this->cancelButton;
			this->ClientSize = System::Drawing::Size(290, 198);
			this->ControlBox = false;
			this->Controls->Add(this->cancelButton);
			this->Controls->Add(this->okButton);
			this->Controls->Add(this->recurseSubdirsCheckBox);
			this->Controls->Add(this->searchTagsCheckBox);
			this->Controls->Add(this->searchFilenamesCheckBox);
			this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::FixedDialog;
			this->Name = L"SearchOptionsForm";
			this->Text = L"Search Options";
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion

public:

	property bool SearchFileNames {

		bool get() {

			return(searchFilenamesCheckBox->Checked);
		}

		void set(bool searchFileNames) {

			this->searchFilenamesCheckBox->Checked = searchFileNames;
		}
	}

	property bool SearchTags {

		bool get() {

			return(searchTagsCheckBox->Checked);
		}

		void set(bool searchTags) {

			searchTagsCheckBox->Checked = searchTags;
		}
	}
  
	property bool RecurseDirectories {

		bool get() {

			return(recurseSubdirsCheckBox->Checked);
		}

		void set(bool recurseDirectories) {

			this->recurseSubdirsCheckBox->Checked = recurseDirectories;
		}
	}
  
	};
}
