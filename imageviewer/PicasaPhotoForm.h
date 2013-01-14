#pragma once

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Collections::Generic;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;

using namespace Google::GData::Photos;
using namespace Google::Picasa;

namespace imageviewer {

	/// <summary>
	/// Summary for PicasaPhotoForm
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class PicasaPhotoForm : public System::Windows::Forms::Form
	{
	public:
		PicasaPhotoForm(void)
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
		~PicasaPhotoForm()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::Label^  label1;
	protected: 
	private: System::Windows::Forms::Label^  label2;
	private: System::Windows::Forms::Label^  label3;
	private: System::Windows::Forms::Label^  label4;
	private: System::Windows::Forms::Label^  label5;
	private: System::Windows::Forms::Label^  label6;
	private: System::Windows::Forms::Label^  label7;
	private: System::Windows::Forms::TextBox^  photoTitleTextBox;
	private: System::Windows::Forms::TextBox^  photoSummaryTextBox;
	private: System::Windows::Forms::TextBox^  photoAuthorTextBox;
	private: System::Windows::Forms::TextBox^  photoUrlTextBox;
	private: System::Windows::Forms::TextBox^  photoWidthTextBox;





	private: System::Windows::Forms::Label^  label8;
	private: System::Windows::Forms::TextBox^  photoHeightTextBox;



	private: System::Windows::Forms::TextBox^  photoSizeTextBox;

	private: System::Windows::Forms::Button^  cancelButton;
	private: System::Windows::Forms::Button^  okButton;
	private: System::Windows::Forms::Button^  copyUrlButton;
    private: Photo ^_photo;
	private: bool _editable;
	private: System::Windows::Forms::Label^  label9;
	private: System::Windows::Forms::DateTimePicker^  photoDateTimePicker;



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
			System::ComponentModel::ComponentResourceManager^  resources = (gcnew System::ComponentModel::ComponentResourceManager(PicasaPhotoForm::typeid));
			this->label1 = (gcnew System::Windows::Forms::Label());
			this->label2 = (gcnew System::Windows::Forms::Label());
			this->label3 = (gcnew System::Windows::Forms::Label());
			this->label4 = (gcnew System::Windows::Forms::Label());
			this->label5 = (gcnew System::Windows::Forms::Label());
			this->label6 = (gcnew System::Windows::Forms::Label());
			this->label7 = (gcnew System::Windows::Forms::Label());
			this->photoTitleTextBox = (gcnew System::Windows::Forms::TextBox());
			this->photoSummaryTextBox = (gcnew System::Windows::Forms::TextBox());
			this->photoAuthorTextBox = (gcnew System::Windows::Forms::TextBox());
			this->photoUrlTextBox = (gcnew System::Windows::Forms::TextBox());
			this->photoWidthTextBox = (gcnew System::Windows::Forms::TextBox());
			this->label8 = (gcnew System::Windows::Forms::Label());
			this->photoHeightTextBox = (gcnew System::Windows::Forms::TextBox());
			this->photoSizeTextBox = (gcnew System::Windows::Forms::TextBox());
			this->cancelButton = (gcnew System::Windows::Forms::Button());
			this->okButton = (gcnew System::Windows::Forms::Button());
			this->copyUrlButton = (gcnew System::Windows::Forms::Button());
			this->label9 = (gcnew System::Windows::Forms::Label());
			this->photoDateTimePicker = (gcnew System::Windows::Forms::DateTimePicker());
			this->SuspendLayout();
			// 
			// label1
			// 
			this->label1->AutoSize = true;
			this->label1->Location = System::Drawing::Point(13, 13);
			this->label1->Name = L"label1";
			this->label1->Size = System::Drawing::Size(42, 20);
			this->label1->TabIndex = 0;
			this->label1->Text = L"Title:";
			// 
			// label2
			// 
			this->label2->AutoSize = true;
			this->label2->Location = System::Drawing::Point(13, 65);
			this->label2->Name = L"label2";
			this->label2->Size = System::Drawing::Size(80, 20);
			this->label2->TabIndex = 1;
			this->label2->Text = L"Summary:";
			// 
			// label3
			// 
			this->label3->AutoSize = true;
			this->label3->Location = System::Drawing::Point(13, 217);
			this->label3->Name = L"label3";
			this->label3->Size = System::Drawing::Size(61, 20);
			this->label3->TabIndex = 2;
			this->label3->Text = L"Author:";
			// 
			// label4
			// 
			this->label4->AutoSize = true;
			this->label4->Location = System::Drawing::Point(13, 269);
			this->label4->Name = L"label4";
			this->label4->Size = System::Drawing::Size(33, 20);
			this->label4->TabIndex = 3;
			this->label4->Text = L"Url:";
			// 
			// label5
			// 
			this->label5->AutoSize = true;
			this->label5->Location = System::Drawing::Point(13, 425);
			this->label5->Name = L"label5";
			this->label5->Size = System::Drawing::Size(48, 20);
			this->label5->TabIndex = 4;
			this->label5->Text = L"Date:";
			// 
			// label6
			// 
			this->label6->AutoSize = true;
			this->label6->Location = System::Drawing::Point(11, 321);
			this->label6->Name = L"label6";
			this->label6->Size = System::Drawing::Size(44, 20);
			this->label6->TabIndex = 5;
			this->label6->Text = L"Size:";
			// 
			// label7
			// 
			this->label7->AutoSize = true;
			this->label7->Location = System::Drawing::Point(11, 373);
			this->label7->Name = L"label7";
			this->label7->Size = System::Drawing::Size(96, 20);
			this->label7->TabIndex = 6;
			this->label7->Text = L"Dimensions:";
			// 
			// photoTitleTextBox
			// 
			this->photoTitleTextBox->Location = System::Drawing::Point(13, 36);
			this->photoTitleTextBox->Name = L"photoTitleTextBox";
			this->photoTitleTextBox->ReadOnly = true;
			this->photoTitleTextBox->Size = System::Drawing::Size(518, 26);
			this->photoTitleTextBox->TabIndex = 7;
			// 
			// photoSummaryTextBox
			// 
			this->photoSummaryTextBox->Location = System::Drawing::Point(13, 88);
			this->photoSummaryTextBox->Multiline = true;
			this->photoSummaryTextBox->Name = L"photoSummaryTextBox";
			this->photoSummaryTextBox->ReadOnly = true;
			this->photoSummaryTextBox->ScrollBars = System::Windows::Forms::ScrollBars::Vertical;
			this->photoSummaryTextBox->Size = System::Drawing::Size(518, 126);
			this->photoSummaryTextBox->TabIndex = 8;
			// 
			// photoAuthorTextBox
			// 
			this->photoAuthorTextBox->Location = System::Drawing::Point(13, 240);
			this->photoAuthorTextBox->Name = L"photoAuthorTextBox";
			this->photoAuthorTextBox->ReadOnly = true;
			this->photoAuthorTextBox->Size = System::Drawing::Size(518, 26);
			this->photoAuthorTextBox->TabIndex = 9;
			// 
			// photoUrlTextBox
			// 
			this->photoUrlTextBox->Location = System::Drawing::Point(13, 292);
			this->photoUrlTextBox->Name = L"photoUrlTextBox";
			this->photoUrlTextBox->ReadOnly = true;
			this->photoUrlTextBox->Size = System::Drawing::Size(451, 26);
			this->photoUrlTextBox->TabIndex = 10;
			// 
			// photoWidthTextBox
			// 
			this->photoWidthTextBox->Location = System::Drawing::Point(12, 396);
			this->photoWidthTextBox->Name = L"photoWidthTextBox";
			this->photoWidthTextBox->ReadOnly = true;
			this->photoWidthTextBox->Size = System::Drawing::Size(62, 26);
			this->photoWidthTextBox->TabIndex = 11;
			this->photoWidthTextBox->TextAlign = System::Windows::Forms::HorizontalAlignment::Right;
			// 
			// label8
			// 
			this->label8->AutoSize = true;
			this->label8->Location = System::Drawing::Point(80, 399);
			this->label8->Name = L"label8";
			this->label8->Size = System::Drawing::Size(16, 20);
			this->label8->TabIndex = 12;
			this->label8->Text = L"x";
			// 
			// photoHeightTextBox
			// 
			this->photoHeightTextBox->Location = System::Drawing::Point(102, 396);
			this->photoHeightTextBox->Name = L"photoHeightTextBox";
			this->photoHeightTextBox->ReadOnly = true;
			this->photoHeightTextBox->Size = System::Drawing::Size(62, 26);
			this->photoHeightTextBox->TabIndex = 13;
			this->photoHeightTextBox->TextAlign = System::Windows::Forms::HorizontalAlignment::Right;
			// 
			// photoSizeTextBox
			// 
			this->photoSizeTextBox->Location = System::Drawing::Point(13, 344);
			this->photoSizeTextBox->Name = L"photoSizeTextBox";
			this->photoSizeTextBox->ReadOnly = true;
			this->photoSizeTextBox->Size = System::Drawing::Size(61, 26);
			this->photoSizeTextBox->TabIndex = 15;
			this->photoSizeTextBox->TextAlign = System::Windows::Forms::HorizontalAlignment::Right;
			// 
			// cancelButton
			// 
			this->cancelButton->DialogResult = System::Windows::Forms::DialogResult::Cancel;
			this->cancelButton->Location = System::Drawing::Point(103, 490);
			this->cancelButton->Name = L"cancelButton";
			this->cancelButton->Size = System::Drawing::Size(86, 41);
			this->cancelButton->TabIndex = 17;
			this->cancelButton->Text = L"Cancel";
			this->cancelButton->UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this->okButton->DialogResult = System::Windows::Forms::DialogResult::OK;
			this->okButton->Location = System::Drawing::Point(11, 490);
			this->okButton->Name = L"okButton";
			this->okButton->Size = System::Drawing::Size(86, 41);
			this->okButton->TabIndex = 16;
			this->okButton->Text = L"Ok";
			this->okButton->UseVisualStyleBackColor = true;
			// 
			// copyUrlButton
			// 
			this->copyUrlButton->Location = System::Drawing::Point(470, 289);
			this->copyUrlButton->Name = L"copyUrlButton";
			this->copyUrlButton->Size = System::Drawing::Size(61, 33);
			this->copyUrlButton->TabIndex = 18;
			this->copyUrlButton->Text = L"Copy";
			this->copyUrlButton->UseVisualStyleBackColor = true;
			this->copyUrlButton->Click += gcnew System::EventHandler(this, &PicasaPhotoForm::copyUrlButton_Click);
			// 
			// label9
			// 
			this->label9->AutoSize = true;
			this->label9->Location = System::Drawing::Point(79, 347);
			this->label9->Name = L"label9";
			this->label9->Size = System::Drawing::Size(28, 20);
			this->label9->TabIndex = 19;
			this->label9->Text = L"kB";
			// 
			// photoDateTimePicker
			// 
			this->photoDateTimePicker->CustomFormat = L"dddd d MMMM yyyy H:mm:ss";
			this->photoDateTimePicker->Enabled = false;
			this->photoDateTimePicker->Format = System::Windows::Forms::DateTimePickerFormat::Custom;
			this->photoDateTimePicker->Location = System::Drawing::Point(12, 448);
			this->photoDateTimePicker->Name = L"photoDateTimePicker";
			this->photoDateTimePicker->Size = System::Drawing::Size(320, 26);
			this->photoDateTimePicker->TabIndex = 14;
			// 
			// PicasaPhotoForm
			// 
			this->AcceptButton = this->okButton;
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->CancelButton = this->cancelButton;
			this->ClientSize = System::Drawing::Size(544, 536);
			this->Controls->Add(this->label9);
			this->Controls->Add(this->copyUrlButton);
			this->Controls->Add(this->cancelButton);
			this->Controls->Add(this->okButton);
			this->Controls->Add(this->photoSizeTextBox);
			this->Controls->Add(this->photoDateTimePicker);
			this->Controls->Add(this->photoHeightTextBox);
			this->Controls->Add(this->label8);
			this->Controls->Add(this->photoWidthTextBox);
			this->Controls->Add(this->photoUrlTextBox);
			this->Controls->Add(this->photoAuthorTextBox);
			this->Controls->Add(this->photoSummaryTextBox);
			this->Controls->Add(this->photoTitleTextBox);
			this->Controls->Add(this->label7);
			this->Controls->Add(this->label6);
			this->Controls->Add(this->label5);
			this->Controls->Add(this->label4);
			this->Controls->Add(this->label3);
			this->Controls->Add(this->label2);
			this->Controls->Add(this->label1);
			this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::FixedDialog;
			this->Icon = (cli::safe_cast<System::Drawing::Icon^  >(resources->GetObject(L"$this.Icon")));
			this->MaximizeBox = false;
			this->MinimizeBox = false;
			this->Name = L"PicasaPhotoForm";
			this->Text = L"Photo Info";
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion
	
    public: property Photo ^photo {

				Photo ^get() { return(_photo); }
				void set(Photo ^photo) { 

					_photo = photo;

					if(_photo == nullptr) return;

					photoTitleTextBox->Text = photo->Title;
					photoSummaryTextBox->Text = photo->Summary;
					photoAuthorTextBox->Text = photo->Author;
					photoUrlTextBox->Text = photo->PhotoUri->AbsoluteUri;
					photoWidthTextBox->Text = Convert::ToString(photo->Width);
					photoHeightTextBox->Text = Convert::ToString(photo->Height);
					photoSizeTextBox->Text = Convert::ToString(photo->Size / 1024);

					unsigned __int64 ticks = DateTime(1970,1,1).Ticks + photo->Timestamp * 10000;

					photoDateTimePicker->Value = DateTime(ticks);

				}
			}


	public: property String ^photoTitle {

			   void set(String ^photoTitle) {

				   photoTitleTextBox->Text = photoTitle;
			   }

			   String ^get() {

				   return(photoTitleTextBox->Text);
			   }
		   }

	public: property String ^photoSummary {

			   void set(String ^photoSummary) {

				   photoSummaryTextBox->Text = photoSummary;
			   }

			   String ^get() {

				   return(photoSummaryTextBox->Text);
			   }
		   }

	public: property String ^photoAuthor {

			   void set(String ^photoAuthor) {

				   photoAuthorTextBox->Text = photoAuthor;
			   }

			   String ^get() {

				   return(photoAuthorTextBox->Text);
			   }
		   }


	public: property bool editable {

			   void set(bool editable) {

				   _editable = editable;

				   if(editable == true) {

					   okButton->Text = "Modify";
					   cancelButton->Visible = true;
					   photoTitleTextBox->ReadOnly = false;
					   photoSummaryTextBox->ReadOnly = false;

				   } else {

					   okButton->Text = "Ok";
					   cancelButton->Visible = false;
					   photoTitleTextBox->ReadOnly = true;
					   photoSummaryTextBox->ReadOnly = true;
				   }
			   }

			   bool get() {

				   return(_editable);
			   }
		   }

	
	private: System::Void copyUrlButton_Click(System::Object^  sender, System::EventArgs^  e) {

				  Clipboard::SetText(photoUrlTextBox->Text);
			 }


};
}
