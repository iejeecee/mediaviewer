#pragma once

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;


namespace imageviewer {

	/// <summary>
	/// Summary for InputDialog
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class InputDialog : public System::Windows::Forms::Form
	{
	public:
		InputDialog(void)
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
		~InputDialog()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::Button^  copyButton;
	private: System::Windows::Forms::Button^  pasteButton;
	protected: 

	private: System::Windows::Forms::Button^  cancelButton;

	private: System::Windows::Forms::Button^  okButton;
	private: System::Windows::Forms::TextBox^  inputTextBox;





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
			System::ComponentModel::ComponentResourceManager^  resources = (gcnew System::ComponentModel::ComponentResourceManager(InputDialog::typeid));
			this->copyButton = (gcnew System::Windows::Forms::Button());
			this->pasteButton = (gcnew System::Windows::Forms::Button());
			this->cancelButton = (gcnew System::Windows::Forms::Button());
			this->okButton = (gcnew System::Windows::Forms::Button());
			this->inputTextBox = (gcnew System::Windows::Forms::TextBox());
			this->SuspendLayout();
			// 
			// copyButton
			// 
			this->copyButton->Location = System::Drawing::Point(192, 53);
			this->copyButton->Name = L"copyButton";
			this->copyButton->Size = System::Drawing::Size(86, 41);
			this->copyButton->TabIndex = 9;
			this->copyButton->Text = L"Copy";
			this->copyButton->UseVisualStyleBackColor = true;
			this->copyButton->Click += gcnew System::EventHandler(this, &InputDialog::copyButton_Click);
			// 
			// pasteButton
			// 
			this->pasteButton->Location = System::Drawing::Point(284, 53);
			this->pasteButton->Name = L"pasteButton";
			this->pasteButton->Size = System::Drawing::Size(86, 41);
			this->pasteButton->TabIndex = 8;
			this->pasteButton->Text = L"Paste";
			this->pasteButton->UseVisualStyleBackColor = true;
			this->pasteButton->Click += gcnew System::EventHandler(this, &InputDialog::pasteButton_Click);
			// 
			// cancelButton
			// 
			this->cancelButton->DialogResult = System::Windows::Forms::DialogResult::Cancel;
			this->cancelButton->Location = System::Drawing::Point(100, 53);
			this->cancelButton->Name = L"cancelButton";
			this->cancelButton->Size = System::Drawing::Size(86, 41);
			this->cancelButton->TabIndex = 7;
			this->cancelButton->Text = L"Cancel";
			this->cancelButton->UseVisualStyleBackColor = true;
			this->cancelButton->Click += gcnew System::EventHandler(this, &InputDialog::cancelButton_Click);
			// 
			// okButton
			// 
			this->okButton->DialogResult = System::Windows::Forms::DialogResult::OK;
			this->okButton->Location = System::Drawing::Point(8, 53);
			this->okButton->Name = L"okButton";
			this->okButton->Size = System::Drawing::Size(86, 41);
			this->okButton->TabIndex = 6;
			this->okButton->Text = L"Ok";
			this->okButton->UseVisualStyleBackColor = true;
			this->okButton->Click += gcnew System::EventHandler(this, &InputDialog::okButton_Click);
			// 
			// inputTextBox
			// 
			this->inputTextBox->Location = System::Drawing::Point(8, 11);
			this->inputTextBox->Name = L"inputTextBox";
			this->inputTextBox->Size = System::Drawing::Size(717, 26);
			this->inputTextBox->TabIndex = 5;
			// 
			// InputDialog
			// 
			this->AcceptButton = this->okButton;
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->CancelButton = this->cancelButton;
			this->ClientSize = System::Drawing::Size(733, 107);
			this->Controls->Add(this->copyButton);
			this->Controls->Add(this->pasteButton);
			this->Controls->Add(this->cancelButton);
			this->Controls->Add(this->okButton);
			this->Controls->Add(this->inputTextBox);
			this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::FixedSingle;
			this->Icon = (cli::safe_cast<System::Drawing::Icon^  >(resources->GetObject(L"$this.Icon")));
			this->MaximizeBox = false;
			this->MinimizeBox = false;
			this->Name = L"InputDialog";
			this->Text = L"InputDialog";
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion
	public: property String ^inputText {

				void set(String ^text) {
					inputTextBox->Text = text;
					inputTextBox->Select(0, text->Length);
				}

				String ^get() {
					return(inputTextBox->Text);
				}
			}
	private: System::Void okButton_Click(System::Object^  sender, System::EventArgs^  e) {

			 }
	private: System::Void cancelButton_Click(System::Object^  sender, System::EventArgs^  e) {
			 }

	private: System::Void pasteButton_Click(System::Object^  sender, System::EventArgs^  e) {

				 if(Clipboard::ContainsText()) {


					 inputTextBox->Text = Clipboard::GetText();
				 }
			 }
	private: System::Void copyButton_Click(System::Object^  sender, System::EventArgs^  e) {

				 if(!String::IsNullOrEmpty(inputTextBox->Text)) {

					Clipboard::SetText(inputTextBox->Text);
				 }
			 }
	};
}
