#pragma once

#include "MediaFormatConvert.h"
#include "HttpRequest.h"
#include "Util.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Collections::Generic;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;


namespace imageviewer {

	/// <summary>
	/// Summary for ProgressForm
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class ProgressForm : public System::Windows::Forms::Form
	{
	public:
	
		delegate void CancelEventHandler(System::Object^ sender, EventArgs^ e);
		event CancelEventHandler ^OnCancelEvent;

		ProgressForm(void)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
			abortAsyncAction = false;

		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~ProgressForm()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::ProgressBar^  totalProgressBar;
	protected: 

	private: System::Windows::Forms::Button^  cancelButton;
    private: delegate void setIntegerDelegate(int value);
    private: delegate void setStringDelegate(String ^value);
	protected: delegate void asyncAddInfoStringDelegate(String ^info);	
	protected: bool abortAsyncAction;
	private: System::Windows::Forms::ProgressBar^  itemProgressBar;
	protected: 

	private: System::Windows::Forms::Button^  okButton;
	private: System::Windows::Forms::Label^  itemLabel;

	private: System::Windows::Forms::Label^  progressLabel;
	private: System::Windows::Forms::TextBox^  infoTextBox;
	private: Object ^_userState;

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
			System::ComponentModel::ComponentResourceManager^  resources = (gcnew System::ComponentModel::ComponentResourceManager(ProgressForm::typeid));
			this->totalProgressBar = (gcnew System::Windows::Forms::ProgressBar());
			this->cancelButton = (gcnew System::Windows::Forms::Button());
			this->itemProgressBar = (gcnew System::Windows::Forms::ProgressBar());
			this->okButton = (gcnew System::Windows::Forms::Button());
			this->itemLabel = (gcnew System::Windows::Forms::Label());
			this->progressLabel = (gcnew System::Windows::Forms::Label());
			this->infoTextBox = (gcnew System::Windows::Forms::TextBox());
			this->SuspendLayout();
			// 
			// totalProgressBar
			// 
			this->totalProgressBar->Location = System::Drawing::Point(17, 21);
			this->totalProgressBar->Name = L"totalProgressBar";
			this->totalProgressBar->Size = System::Drawing::Size(386, 27);
			this->totalProgressBar->TabIndex = 0;
			// 
			// cancelButton
			// 
			this->cancelButton->Location = System::Drawing::Point(213, 253);
			this->cancelButton->Name = L"cancelButton";
			this->cancelButton->Size = System::Drawing::Size(97, 36);
			this->cancelButton->TabIndex = 23;
			this->cancelButton->Text = L"Cancel";
			this->cancelButton->UseVisualStyleBackColor = true;
			this->cancelButton->Click += gcnew System::EventHandler(this, &ProgressForm::cancelButton_Click);
			// 
			// itemProgressBar
			// 
			this->itemProgressBar->Location = System::Drawing::Point(17, 85);
			this->itemProgressBar->Name = L"itemProgressBar";
			this->itemProgressBar->Size = System::Drawing::Size(386, 27);
			this->itemProgressBar->TabIndex = 24;
			// 
			// okButton
			// 
			this->okButton->Enabled = false;
			this->okButton->Location = System::Drawing::Point(110, 253);
			this->okButton->Name = L"okButton";
			this->okButton->Size = System::Drawing::Size(97, 36);
			this->okButton->TabIndex = 25;
			this->okButton->Text = L"Ok";
			this->okButton->UseVisualStyleBackColor = true;
			this->okButton->Click += gcnew System::EventHandler(this, &ProgressForm::okButton_Click);
			// 
			// itemLabel
			// 
			this->itemLabel->Location = System::Drawing::Point(17, 115);
			this->itemLabel->Name = L"itemLabel";
			this->itemLabel->Size = System::Drawing::Size(386, 20);
			this->itemLabel->TabIndex = 26;
			this->itemLabel->TextAlign = System::Drawing::ContentAlignment::MiddleCenter;
			// 
			// progressLabel
			// 
			this->progressLabel->Location = System::Drawing::Point(21, 51);
			this->progressLabel->Name = L"progressLabel";
			this->progressLabel->Size = System::Drawing::Size(382, 20);
			this->progressLabel->TabIndex = 27;
			this->progressLabel->Text = L"Finished: 0 / 0";
			this->progressLabel->TextAlign = System::Drawing::ContentAlignment::MiddleCenter;
			// 
			// infoTextBox
			// 
			this->infoTextBox->Location = System::Drawing::Point(15, 149);
			this->infoTextBox->Multiline = true;
			this->infoTextBox->Name = L"infoTextBox";
			this->infoTextBox->ScrollBars = System::Windows::Forms::ScrollBars::Vertical;
			this->infoTextBox->Size = System::Drawing::Size(387, 87);
			this->infoTextBox->TabIndex = 28;
			// 
			// ProgressForm
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(415, 309);
			this->Controls->Add(this->infoTextBox);
			this->Controls->Add(this->progressLabel);
			this->Controls->Add(this->itemLabel);
			this->Controls->Add(this->okButton);
			this->Controls->Add(this->itemProgressBar);
			this->Controls->Add(this->cancelButton);
			this->Controls->Add(this->totalProgressBar);
			this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::FixedDialog;
			this->Icon = (cli::safe_cast<System::Drawing::Icon^  >(resources->GetObject(L"$this.Icon")));
			this->MaximizeBox = false;
			this->Name = L"ProgressForm";
			this->Text = L"ProgressForm";
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion

	protected: virtual void asyncAction(Object ^state) {

		}

	private: System::Void cancelButton_Click(System::Object^  sender, System::EventArgs^  e) {

				 abortAsyncAction = true;	
				 OnCancelEvent(this, gcnew EventArgs());
			 }

	private: System::Void okButton_Click(System::Object^  sender, System::EventArgs^  e) {

				 Form::Close();
			 }

	public: System::Void addInfoString(String ^info) {

				if(this->InvokeRequired) {

					cli::array<Object ^> ^infoArgs = gcnew cli::array<Object ^>(1);

					infoArgs[0] = info;
					asyncAddInfoStringDelegate ^addInfo = gcnew asyncAddInfoStringDelegate(this, &ProgressForm::asyncAddInfoStringInvoke);

					this->BeginInvoke(addInfo, infoArgs);

				} else {

					asyncAddInfoStringInvoke(info);
				}

			}
	private: System::Void asyncAddInfoStringInvoke(String ^info) {

				 infoTextBox->Text = infoTextBox->Text + info + L"\r\n\r\n";
				 infoTextBox->SelectionStart = infoTextBox->Text->Length;
				 infoTextBox->ScrollToCaret();
				 infoTextBox->Refresh();
			 }
	public: System::Void actionFinished() {

				if(this->InvokeRequired) {

					this->BeginInvoke(gcnew Action(this, &ProgressForm::asyncActionFinishedInvoke));

				} else {

					asyncActionFinishedInvoke();
				}
			}
	private: System::Void asyncActionFinishedInvoke(void) {

				 okButton->Enabled = true;
				 cancelButton->Enabled = false;
			 }

	public: property Object ^userState {

			void set(Object ^userState) {

				
				_userState = userState;
			}

			Object ^get() {

				return(_userState);
			}
		}
	

	public: property int totalProgressMaximum {

			void set(int maximum) {

				if(this->InvokeRequired) {

					cli::array<Object ^> ^args = gcnew cli::array<Object ^>(1);
					args[0] = maximum;	

					this->BeginInvoke(gcnew setIntegerDelegate(this, &ProgressForm::setTotalProgressMaximum), args);

				} else {

					setTotalProgressMaximum(maximum);
				}
				
			}

			int get() {

				return(totalProgressBar->Maximum);
			}
		}

	private: void setTotalProgressMaximum(int maximum) {

				 totalProgressBar->Maximum = maximum;
			 }

	public: property int itemProgressMaximum {

			void set(int maximum) {

				if(this->InvokeRequired) {

					cli::array<Object ^> ^args = gcnew cli::array<Object ^>(1);
					args[0] = maximum;	

					this->BeginInvoke(gcnew setIntegerDelegate(this, &ProgressForm::setItemProgressMaximum), args);

				} else {

					setItemProgressMaximum(maximum);
				}
			}

			int get() {

				return(itemProgressBar->Maximum);
			}
		}
	private: void setItemProgressMaximum(int maximum) {

				 itemProgressBar->Maximum = maximum;
			 }

	 
	public: property int itemProgressValue {

			void set(int value) {

				if(this->InvokeRequired) {

					cli::array<Object ^> ^args = gcnew cli::array<Object ^>(1);
					args[0] = value;	

					this->BeginInvoke(gcnew setIntegerDelegate(this, &ProgressForm::setItemProgressValue), args);

				} else {

					setItemProgressValue(value);
				}
			}

			int get() {

				return(itemProgressBar->Value);
			}
		}
	private: void setItemProgressValue(int value) {

				 itemProgressBar->Value = value;
			 }

	 public: property int totalProgressValue {

			void set(int value) {

				if(this->InvokeRequired) {

					cli::array<Object ^> ^args = gcnew cli::array<Object ^>(1);
					args[0] = value;	

					this->BeginInvoke(gcnew setIntegerDelegate(this, &ProgressForm::setTotalProgressValue), args);

				} else {

					setTotalProgressValue(value);
				}
			}

			int get() {

				return(totalProgressBar->Value);
			}
		}
	private: void setTotalProgressValue(int value) {

				 progressLabel->Text = "Finished: " + Convert::ToString(value) + " / " + Convert::ToString(totalProgressBar->Maximum);
				 totalProgressBar->Value = value;
			 }

	 public: property String ^itemInfo {

			void set(String ^value) {

				if(this->InvokeRequired) {

					cli::array<Object ^> ^args = gcnew cli::array<Object ^>(1);
					args[0] = value;	

					this->BeginInvoke(gcnew setStringDelegate(this, &ProgressForm::setItemInfo), args);

				} else {

					setItemInfo(value);
				}
			}

			String ^get() {

				return(itemLabel->Text);
			}
		}
	private: void setItemInfo(String ^value) {

				 itemLabel->Text = value;
				 itemLabel->TextAlign = System::Drawing::ContentAlignment::MiddleCenter;
			 }

	};
}
