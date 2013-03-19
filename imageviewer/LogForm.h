#pragma once

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;


namespace imageviewer {

	/// <summary>
	/// Summary for LogForm
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class LogForm : public System::Windows::Forms::Form
	{
	public:
		LogForm(void)
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
		~LogForm()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::TextBox^  logTextBox;
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
			this->logTextBox = (gcnew System::Windows::Forms::TextBox());
			this->SuspendLayout();
			// 
			// logTextBox
			// 
			this->logTextBox->Location = System::Drawing::Point(12, 14);
			this->logTextBox->Multiline = true;
			this->logTextBox->Name = L"logTextBox";
			this->logTextBox->Size = System::Drawing::Size(882, 395);
			this->logTextBox->TabIndex = 0;
			// 
			// LogForm
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(906, 448);
			this->Controls->Add(this->logTextBox);
			this->Name = L"LogForm";
			this->Text = L"LogForm";
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion

		static log4net::ILog ^log = log4net::LogManager::GetLogger(System::Reflection::MethodBase::GetCurrentMethod()->DeclaringType);

		void test() {

			//log->
		}
	};
}
