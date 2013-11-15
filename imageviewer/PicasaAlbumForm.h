#pragma once

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;

using namespace Google::GData::Photos;
using namespace Google::Picasa;

namespace imageviewer {

	/// <summary>
	/// Summary for PicasaAlbumForm
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class PicasaAlbumForm : public System::Windows::Forms::Form
	{
	private: Album ^_album;
	private: bool _editable;

	public:

		property Album ^album {

			Album ^get() { return(_album); }
			void set(Album ^album) { 

				_album = album;

				if(_album == nullptr) return;

				albumTitleTextBox->Text = album->Title;
				albumDescriptionTextBox->Text = album->Summary;
				albumAccessComboBox->SelectedIndex = albumAccessComboBox->Items->IndexOf(album->Access);
			}
		}

		property String ^albumTitle {

			String ^get() {

				return(albumTitleTextBox->Text);
			}

			void set(String ^albumTitle) {

				albumTitleTextBox->Text = albumTitle;
			}
		}

		property String ^albumDescription {

			String ^get() {

				return(albumDescriptionTextBox->Text);
			}

			void set(String ^albumDescription) {

				albumDescriptionTextBox->Text = albumDescription;
			}
		}

		property String ^albumAccess {

			String ^get() {

				return(albumAccessComboBox->SelectedItem->ToString());
			}

			void set(String ^albumAccess) {

				albumAccessComboBox->SelectedIndex = albumAccessComboBox->Items->IndexOf(albumAccess);
			}
		}

		property bool editable {

			bool get() {

				return(_editable);
			}

			void set(bool editable) {

				_editable = editable;

				if(editable == true) {

					okButton->Text = "Modify";
					cancelButton->Visible = true;
					albumTitleTextBox->ReadOnly = false;
					albumDescriptionTextBox->ReadOnly = false;
				    albumAccessComboBox->Enabled = true;

				} else {

					okButton->Text = "Ok";
					cancelButton->Visible = false;
					albumTitleTextBox->ReadOnly = true;
					albumDescriptionTextBox->ReadOnly = true;
				    albumAccessComboBox->Enabled = false;
				}
			}
		}

		PicasaAlbumForm(void)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
			album = nullptr;

			albumAccessComboBox->SelectedIndex = 0;
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~PicasaAlbumForm()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::TextBox^  albumTitleTextBox;
	protected: 
	private: System::Windows::Forms::TextBox^  albumDescriptionTextBox;
	private: System::Windows::Forms::Label^  label1;
	private: System::Windows::Forms::Label^  label2;
	private: System::Windows::Forms::ComboBox^  albumAccessComboBox;

	private: System::Windows::Forms::Label^  label3;
	private: System::Windows::Forms::Button^  okButton;
	private: System::Windows::Forms::Button^  cancelButton;

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
			this->albumTitleTextBox = (gcnew System::Windows::Forms::TextBox());
			this->albumDescriptionTextBox = (gcnew System::Windows::Forms::TextBox());
			this->label1 = (gcnew System::Windows::Forms::Label());
			this->label2 = (gcnew System::Windows::Forms::Label());
			this->albumAccessComboBox = (gcnew System::Windows::Forms::ComboBox());
			this->label3 = (gcnew System::Windows::Forms::Label());
			this->okButton = (gcnew System::Windows::Forms::Button());
			this->cancelButton = (gcnew System::Windows::Forms::Button());
			this->SuspendLayout();
			// 
			// albumTitleTextBox
			// 
			this->albumTitleTextBox->Location = System::Drawing::Point(12, 35);
			this->albumTitleTextBox->Name = L"albumTitleTextBox";
			this->albumTitleTextBox->Size = System::Drawing::Size(527, 26);
			this->albumTitleTextBox->TabIndex = 0;
			// 
			// albumDescriptionTextBox
			// 
			this->albumDescriptionTextBox->Location = System::Drawing::Point(12, 87);
			this->albumDescriptionTextBox->Multiline = true;
			this->albumDescriptionTextBox->Name = L"albumDescriptionTextBox";
			this->albumDescriptionTextBox->ScrollBars = System::Windows::Forms::ScrollBars::Vertical;
			this->albumDescriptionTextBox->Size = System::Drawing::Size(527, 146);
			this->albumDescriptionTextBox->TabIndex = 1;
			// 
			// label1
			// 
			this->label1->AutoSize = true;
			this->label1->Location = System::Drawing::Point(8, 12);
			this->label1->Name = L"label1";
			this->label1->Size = System::Drawing::Size(38, 20);
			this->label1->TabIndex = 2;
			this->label1->Text = L"title:";
			// 
			// label2
			// 
			this->label2->AutoSize = true;
			this->label2->Location = System::Drawing::Point(8, 64);
			this->label2->Name = L"label2";
			this->label2->Size = System::Drawing::Size(90, 20);
			this->label2->TabIndex = 3;
			this->label2->Text = L"description:";
			// 
			// albumAccessComboBox
			// 
			this->albumAccessComboBox->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
			this->albumAccessComboBox->FormattingEnabled = true;
			this->albumAccessComboBox->Items->AddRange(gcnew cli::array< System::Object^  >(3) {L"protected", L"private", L"public"});
			this->albumAccessComboBox->Location = System::Drawing::Point(12, 259);
			this->albumAccessComboBox->Name = L"albumAccessComboBox";
			this->albumAccessComboBox->Size = System::Drawing::Size(252, 28);
			this->albumAccessComboBox->TabIndex = 4;
			// 
			// label3
			// 
			this->label3->AutoSize = true;
			this->label3->Location = System::Drawing::Point(12, 236);
			this->label3->Name = L"label3";
			this->label3->Size = System::Drawing::Size(63, 20);
			this->label3->TabIndex = 5;
			this->label3->Text = L"access:";
			// 
			// okButton
			// 
			this->okButton->DialogResult = System::Windows::Forms::DialogResult::OK;
			this->okButton->Location = System::Drawing::Point(12, 305);
			this->okButton->Name = L"okButton";
			this->okButton->Size = System::Drawing::Size(97, 36);
			this->okButton->TabIndex = 6;
			this->okButton->Text = L"Ok";
			this->okButton->UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this->cancelButton->DialogResult = System::Windows::Forms::DialogResult::Cancel;
			this->cancelButton->Location = System::Drawing::Point(115, 305);
			this->cancelButton->Name = L"cancelButton";
			this->cancelButton->Size = System::Drawing::Size(97, 36);
			this->cancelButton->TabIndex = 7;
			this->cancelButton->Text = L"Cancel";
			this->cancelButton->UseVisualStyleBackColor = true;
			// 
			// PicasaAlbumForm
			// 
			this->AcceptButton = this->okButton;
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->CancelButton = this->cancelButton;
			this->ClientSize = System::Drawing::Size(556, 355);
			this->Controls->Add(this->cancelButton);
			this->Controls->Add(this->okButton);
			this->Controls->Add(this->label3);
			this->Controls->Add(this->albumAccessComboBox);
			this->Controls->Add(this->label2);
			this->Controls->Add(this->label1);
			this->Controls->Add(this->albumDescriptionTextBox);
			this->Controls->Add(this->albumTitleTextBox);
			this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::FixedDialog;
			this->MaximizeBox = false;
			this->MinimizeBox = false;
			this->Name = L"PicasaAlbumForm";
			this->Text = L"Picasa Album";
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion

	
	};
}
