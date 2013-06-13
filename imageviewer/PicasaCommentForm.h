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
	/// Summary for PicasaCommentForm
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class PicasaCommentForm : public System::Windows::Forms::Form
	{
	public:
		PicasaCommentForm(void)
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
		~PicasaCommentForm()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::TextBox^  inputTextBox;
	protected: 

	private: List<Comment ^> ^_comments;
	private: System::Windows::Forms::RichTextBox^  commentsTextBox;
	private: System::Windows::Forms::Label^  label1;
	private: System::Windows::Forms::Label^  label2;
	private: System::Windows::Forms::Button^  postButton;

	private: System::Windows::Forms::Button^  cancelButton;
	private: Photo ^_photo;


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
			System::ComponentModel::ComponentResourceManager^  resources = (gcnew System::ComponentModel::ComponentResourceManager(PicasaCommentForm::typeid));
			this->inputTextBox = (gcnew System::Windows::Forms::TextBox());
			this->commentsTextBox = (gcnew System::Windows::Forms::RichTextBox());
			this->label1 = (gcnew System::Windows::Forms::Label());
			this->label2 = (gcnew System::Windows::Forms::Label());
			this->postButton = (gcnew System::Windows::Forms::Button());
			this->cancelButton = (gcnew System::Windows::Forms::Button());
			this->SuspendLayout();
			// 
			// inputTextBox
			// 
			this->inputTextBox->Location = System::Drawing::Point(12, 320);
			this->inputTextBox->Multiline = true;
			this->inputTextBox->Name = L"inputTextBox";
			this->inputTextBox->Size = System::Drawing::Size(534, 128);
			this->inputTextBox->TabIndex = 0;
			this->inputTextBox->TextChanged += gcnew System::EventHandler(this, &PicasaCommentForm::inputTextBox_TextChanged);
			// 
			// commentsTextBox
			// 
			this->commentsTextBox->Location = System::Drawing::Point(12, 38);
			this->commentsTextBox->Name = L"commentsTextBox";
			this->commentsTextBox->ReadOnly = true;
			this->commentsTextBox->ScrollBars = System::Windows::Forms::RichTextBoxScrollBars::Vertical;
			this->commentsTextBox->Size = System::Drawing::Size(533, 258);
			this->commentsTextBox->TabIndex = 1;
			this->commentsTextBox->Text = L"";
			this->commentsTextBox->LinkClicked += gcnew System::Windows::Forms::LinkClickedEventHandler(this, &PicasaCommentForm::commentsTextBox_LinkClicked);
			// 
			// label1
			// 
			this->label1->AutoSize = true;
			this->label1->Location = System::Drawing::Point(12, 299);
			this->label1->Name = L"label1";
			this->label1->Size = System::Drawing::Size(74, 20);
			this->label1->TabIndex = 2;
			this->label1->Text = L"Respond";
			// 
			// label2
			// 
			this->label2->AutoSize = true;
			this->label2->Location = System::Drawing::Point(12, 15);
			this->label2->Name = L"label2";
			this->label2->Size = System::Drawing::Size(86, 20);
			this->label2->TabIndex = 3;
			this->label2->Text = L"Comments";
			// 
			// postButton
			// 
			this->postButton->DialogResult = System::Windows::Forms::DialogResult::OK;
			this->postButton->Enabled = false;
			this->postButton->Location = System::Drawing::Point(12, 459);
			this->postButton->Name = L"postButton";
			this->postButton->Size = System::Drawing::Size(97, 36);
			this->postButton->TabIndex = 27;
			this->postButton->Text = L"Post";
			this->postButton->UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this->cancelButton->DialogResult = System::Windows::Forms::DialogResult::Cancel;
			this->cancelButton->Location = System::Drawing::Point(115, 459);
			this->cancelButton->Name = L"cancelButton";
			this->cancelButton->Size = System::Drawing::Size(97, 36);
			this->cancelButton->TabIndex = 26;
			this->cancelButton->Text = L"Cancel";
			this->cancelButton->UseVisualStyleBackColor = true;
			// 
			// PicasaCommentForm
			// 
			this->AcceptButton = this->postButton;
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->CancelButton = this->cancelButton;
			this->ClientSize = System::Drawing::Size(558, 507);
			this->Controls->Add(this->postButton);
			this->Controls->Add(this->cancelButton);
			this->Controls->Add(this->label2);
			this->Controls->Add(this->label1);
			this->Controls->Add(this->commentsTextBox);
			this->Controls->Add(this->inputTextBox);
			this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::FixedDialog;
			this->Icon = (cli::safe_cast<System::Drawing::Icon^  >(resources->GetObject(L"$this.Icon")));
			this->MaximizeBox = false;
			this->MinimizeBox = false;
			this->Name = L"PicasaCommentForm";
			this->Text = L"Photo Comments";
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion

	public: property Photo ^photo {

				Photo ^get() { return(_photo); }
				void set(Photo ^photo) { 

					_photo = photo;

					this->Text = "Comments for: \"" + photo->Title + "\"";

					if(photo->CommentingEnabled == true) {

						inputTextBox->Enabled = true;

					} else {

						inputTextBox->Enabled = false;
						inputTextBox->Text = "Adding comments is disabled for this photo";
					}

				}
			}

	private: void appendText(String ^text, Color color) {

				 commentsTextBox->SelectionStart = commentsTextBox->TextLength;
				 commentsTextBox->SelectionLength = 0;

				 commentsTextBox->SelectionColor = color;
				 commentsTextBox->AppendText(text);
				 commentsTextBox->SelectionColor = commentsTextBox->ForeColor;
			 }

	private: String ^getTimeString(DateTime now, DateTime time) {

				 TimeSpan passed = now.Subtract(time);

				 String ^timeString;
				 int timeUnit;

				 if(passed.Days > 365) {

					 timeUnit = passed.Days / 365;

					 timeString = " year";

				 } else if(passed.Days > 0) {

					 if(passed.Days > 30) {

						 timeUnit = passed.Days / 30;
						 timeString = " month";

					 } else if(passed.Days > 7) {

						 timeUnit = passed.Days / 7;
						 timeString = " week";

					 } else {

						 timeUnit = passed.Days;
						 timeString = " day";

						} 

				 } else if(passed.Hours > 1) {

					 timeUnit = (int) passed.Hours;
					 timeString += " hour";

				 } else {

					 timeUnit = (int) passed.Minutes;
					 timeString += " minute";

				 }

				 String ^result = Convert::ToString(timeUnit) + timeString;
				 if(timeUnit > 1) {

					 result += "s";
				 }

				 result += " ago";

				 return(result);

			 }
	public: property List<Comment ^> ^comments {

				void set(List<Comment ^> ^comments) {

					_comments = comments;

					commentsTextBox->Clear();

					DateTime now = DateTime::Now;

					for each(Comment ^comment in comments) {

						String ^time = getTimeString(now, comment->Updated);

						appendText(comment->Title, Color::DarkBlue);
						appendText(" " + time + "\r\n", Color::Black);
						appendText(comment->Content + "\r\n\r\n", Color::Black);

					}

				}

				List<Comment ^> ^get() {

					return(_comments);
				}
			}

	public: property String ^commentsText {

				void set(String ^commentsText) {

					commentsTextBox->Clear();
					commentsTextBox->Text = commentsText;
				}

				String ^get() {

					return(commentsTextBox->Text);
				}
			}

	public: property String ^inputText {

				void set(String ^inputText) {

					inputTextBox->Clear();
					inputTextBox->Text = inputText;
				}

				String ^get() {

					return(inputTextBox->Text);
				}
			}

	private: System::Void commentsTextBox_LinkClicked(System::Object^  sender, System::Windows::Forms::LinkClickedEventArgs^  e) {

				 System::Diagnostics::Process::Start(e->LinkText);

			 }

	private: System::Void inputTextBox_TextChanged(System::Object^  sender, System::EventArgs^  e) {

				 if(inputTextBox->Text->Length == 0 || photo->CommentingEnabled == false) {

					 postButton->Enabled = false;

				 } else {

					 postButton->Enabled = true;
				 }
			 }
	};

}
