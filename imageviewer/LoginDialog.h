#pragma once
#include "OAuth2.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;

using namespace Newtonsoft::Json;

namespace imageviewer {

	/// <summary>
	/// Summary for LoginDialog
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class LoginDialog : public System::Windows::Forms::Form
	{
	public:
		LoginDialog(void)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
			webBrowser->DocumentTitleChanged += gcnew EventHandler(this, &LoginDialog::webBrowser_DocumentTitleChanged);
			authInfo = gcnew AuthInfo();
			
		}



	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~LoginDialog()
		{
			if (components)
			{
				delete components;
			}
		}

	protected: 





	private: System::Windows::Forms::WebBrowser^  webBrowser;
	protected: OAuth2 ^authInstance;
	private: AuthInfo ^_authInfo;
	




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
			System::ComponentModel::ComponentResourceManager^  resources = (gcnew System::ComponentModel::ComponentResourceManager(LoginDialog::typeid));
			this->webBrowser = (gcnew System::Windows::Forms::WebBrowser());
			this->SuspendLayout();
			// 
			// webBrowser
			// 
			this->webBrowser->Location = System::Drawing::Point(12, 12);
			this->webBrowser->MinimumSize = System::Drawing::Size(20, 20);
			this->webBrowser->Name = L"webBrowser";
			this->webBrowser->Size = System::Drawing::Size(816, 491);
			this->webBrowser->TabIndex = 14;
			this->webBrowser->Url = (gcnew System::Uri(resources->GetString(L"webBrowser.Url"), System::UriKind::Absolute));
			// 
			// LoginDialog
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(840, 521);
			this->Controls->Add(this->webBrowser);
			this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::FixedDialog;
			this->Icon = (cli::safe_cast<System::Drawing::Icon^  >(resources->GetObject(L"$this.Icon")));
			this->MaximizeBox = false;
			this->MinimizeBox = false;
			this->Name = L"LoginDialog";
			this->Text = L"Login";
			this->ResumeLayout(false);

		}
#pragma endregion

	public: property AuthInfo ^authInfo {

			void set(AuthInfo ^authInfo) {

				_authInfo = authInfo;
			}

			AuthInfo ^get() {

				return(_authInfo);
			}
		}

	 private: System::Void webBrowser_DocumentTitleChanged(System::Object^  sender, System::EventArgs^  e) {

				 String ^successString = "Success code=";

				 String ^documentTitle = webBrowser->DocumentTitle;
			
				 if(documentTitle->StartsWith(successString)) {

					 String ^authCode = documentTitle->Substring(successString->Length);

					 try {

						 authInfo = authInstance->authenticate(authCode);
												
					 } catch (Exception ^ex) {

						 MessageBox::Show(ex->Message, "Authentication Error");
					 }
					 
					 this->Close();
				 
				 } else if(documentTitle->Equals("Denied error=access_denied")) {

					 this->Close();
				 }
				
			 }

};
}
