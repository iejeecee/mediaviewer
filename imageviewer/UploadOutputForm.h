#pragma once

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;
using namespace System::Net;


namespace imageviewer {

	/// <summary>
	/// Summary for UploadOutputForm
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class UploadOutputForm : public System::Windows::Forms::Form
	{
	public:
		UploadOutputForm(HttpWebResponse ^htmlData, String ^text)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
			webBrowser1->DocumentStream = htmlData->GetResponseStream();		
			Text = text;
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~UploadOutputForm()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::WebBrowser^  webBrowser1;
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
			this->webBrowser1 = (gcnew System::Windows::Forms::WebBrowser());
			this->SuspendLayout();
			// 
			// webBrowser1
			// 
			this->webBrowser1->Dock = System::Windows::Forms::DockStyle::Fill;
			this->webBrowser1->Location = System::Drawing::Point(0, 0);
			this->webBrowser1->MinimumSize = System::Drawing::Size(20, 20);
			this->webBrowser1->Name = L"webBrowser1";
			this->webBrowser1->ScriptErrorsSuppressed = true;
			this->webBrowser1->Size = System::Drawing::Size(790, 440);
			this->webBrowser1->TabIndex = 0;
			// 
			// UploadOutputForm
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(790, 440);
			this->Controls->Add(this->webBrowser1);
			this->Name = L"UploadOutputForm";
			this->Text = L"UploadOutputForm";
			this->ResumeLayout(false);

		}
#pragma endregion

	};
}
