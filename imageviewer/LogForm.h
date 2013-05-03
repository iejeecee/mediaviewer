#pragma once

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections::Generic;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;
using namespace System::Threading;


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
			args = gcnew array<Object ^>(2);
			addTextDelegate = gcnew AddTextDelegate(this, &LogForm::addText);

			filterComboBox->SelectedIndex = 2;
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

	private: System::Windows::Forms::SplitContainer^  splitContainer1;
	private: System::Windows::Forms::Label^  label1;
	private: System::Windows::Forms::ComboBox^  filterComboBox;
	private: System::Windows::Forms::RichTextBox^  logTextBox;
	private: System::Windows::Forms::Button^  clearButton;

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
			System::ComponentModel::ComponentResourceManager^  resources = (gcnew System::ComponentModel::ComponentResourceManager(LogForm::typeid));
			this->splitContainer1 = (gcnew System::Windows::Forms::SplitContainer());
			this->logTextBox = (gcnew System::Windows::Forms::RichTextBox());
			this->clearButton = (gcnew System::Windows::Forms::Button());
			this->label1 = (gcnew System::Windows::Forms::Label());
			this->filterComboBox = (gcnew System::Windows::Forms::ComboBox());
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->splitContainer1))->BeginInit();
			this->splitContainer1->Panel1->SuspendLayout();
			this->splitContainer1->Panel2->SuspendLayout();
			this->splitContainer1->SuspendLayout();
			this->SuspendLayout();
			// 
			// splitContainer1
			// 
			this->splitContainer1->Dock = System::Windows::Forms::DockStyle::Fill;
			this->splitContainer1->FixedPanel = System::Windows::Forms::FixedPanel::Panel2;
			this->splitContainer1->IsSplitterFixed = true;
			this->splitContainer1->Location = System::Drawing::Point(0, 0);
			this->splitContainer1->Name = L"splitContainer1";
			this->splitContainer1->Orientation = System::Windows::Forms::Orientation::Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this->splitContainer1->Panel1->Controls->Add(this->logTextBox);
			// 
			// splitContainer1.Panel2
			// 
			this->splitContainer1->Panel2->Controls->Add(this->clearButton);
			this->splitContainer1->Panel2->Controls->Add(this->label1);
			this->splitContainer1->Panel2->Controls->Add(this->filterComboBox);
			this->splitContainer1->Size = System::Drawing::Size(906, 448);
			this->splitContainer1->SplitterDistance = 397;
			this->splitContainer1->TabIndex = 1;
			// 
			// logTextBox
			// 
			this->logTextBox->Dock = System::Windows::Forms::DockStyle::Fill;
			this->logTextBox->Location = System::Drawing::Point(0, 0);
			this->logTextBox->Name = L"logTextBox";
			this->logTextBox->ReadOnly = true;
			this->logTextBox->Size = System::Drawing::Size(906, 397);
			this->logTextBox->TabIndex = 0;
			this->logTextBox->Text = L"";
			this->logTextBox->WordWrap = false;
			// 
			// clearButton
			// 
			this->clearButton->Anchor = System::Windows::Forms::AnchorStyles::Right;
			this->clearButton->Location = System::Drawing::Point(818, 4);
			this->clearButton->Name = L"clearButton";
			this->clearButton->Size = System::Drawing::Size(85, 36);
			this->clearButton->TabIndex = 2;
			this->clearButton->Text = L"Clear";
			this->clearButton->UseVisualStyleBackColor = true;
			this->clearButton->Click += gcnew System::EventHandler(this, &LogForm::clearButton_Click);
			// 
			// label1
			// 
			this->label1->AutoSize = true;
			this->label1->Location = System::Drawing::Point(12, 12);
			this->label1->Name = L"label1";
			this->label1->Size = System::Drawing::Size(48, 20);
			this->label1->TabIndex = 1;
			this->label1->Text = L"Filter:";
			// 
			// filterComboBox
			// 
			this->filterComboBox->FormattingEnabled = true;
			this->filterComboBox->Items->AddRange(gcnew cli::array< System::Object^  >(3) {L"Info", L"Warnings", L"Errors"});
			this->filterComboBox->Location = System::Drawing::Point(66, 9);
			this->filterComboBox->Name = L"filterComboBox";
			this->filterComboBox->Size = System::Drawing::Size(179, 28);
			this->filterComboBox->TabIndex = 0;
			this->filterComboBox->SelectedIndexChanged += gcnew System::EventHandler(this, &LogForm::filterComboBox_SelectedIndexChanged);
			// 
			// LogForm
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(906, 448);
			this->Controls->Add(this->splitContainer1);
			this->Icon = (cli::safe_cast<System::Drawing::Icon^  >(resources->GetObject(L"$this.Icon")));
			this->Name = L"LogForm";
			this->Text = L"Log";
			this->FormClosing += gcnew System::Windows::Forms::FormClosingEventHandler(this, &LogForm::LogForm_FormClosing);
			this->splitContainer1->Panel1->ResumeLayout(false);
			this->splitContainer1->Panel2->ResumeLayout(false);
			this->splitContainer1->Panel2->PerformLayout();
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->splitContainer1))->EndInit();
			this->splitContainer1->ResumeLayout(false);
			this->ResumeLayout(false);

		}
#pragma endregion

	public:

		enum class LogLevel
		{
			UNKNOWN,
			FATAL,
			ERROR,
			WARNING,			
			INFO,
			DEBUG
		};

	private:

		delegate void AddTextDelegate(LogLevel level, String ^text);
		AddTextDelegate ^addTextDelegate;
		array<Object ^> ^args;

		LogLevel filterLevel;
		
		void addText(LogLevel level, String ^text) {

			logTextBox->SelectionStart = logTextBox->TextLength;
			logTextBox->SelectionLength = 0;

			switch(level) {

			case LogLevel::UNKNOWN:
				{
					logTextBox->SelectionColor = Color::Yellow;
					break;
				}
			case LogLevel::DEBUG:
				{
					logTextBox->SelectionColor = Color::LightBlue;
					break;
				}
			case LogLevel::ERROR:
				{
					logTextBox->SelectionColor = Color::Red;
					break;
				}
			case LogLevel::FATAL:
				{
					logTextBox->SelectionColor = Color::DarkRed;
					break;
				}
			case LogLevel::INFO:
				{
					logTextBox->SelectionColor = Color::Blue;
					break;
				}
			case LogLevel::WARNING:
				{
					logTextBox->SelectionColor = Color::Yellow;
					break;
				}

			}
		
			logTextBox->AppendText(text);
			logTextBox->SelectionColor = logTextBox->ForeColor;

			logTextBox->ScrollToCaret();
		}

	public:

		void append(LogLevel level, String ^text) {

			if(filterLevel < level) return;

			Monitor::Enter(args);

			try {

				args[0] = level;
				args[1] = text;

				if(!this->IsHandleCreated)
				{
					this->CreateHandle();
				}

				if(this->InvokeRequired) {

					this->BeginInvoke(addTextDelegate, args);

				} else {

					addText(level, text);
				}

			} finally {

				Monitor::Exit(args);
			}

		}


	private: System::Void clearButton_Click(System::Object^  sender, System::EventArgs^  e) {

				 logTextBox->Clear();
			 }

private: System::Void filterComboBox_SelectedIndexChanged(System::Object^  sender, System::EventArgs^  e) {

			 if(filterComboBox->SelectedIndex == 0) {

				 filterLevel = LogLevel::INFO;

			 } else if(filterComboBox->SelectedIndex == 1) {

				 filterLevel = LogLevel::WARNING;

			 } else {

				 filterLevel = LogLevel::ERROR;
			 }
		 }

private: System::Void LogForm_FormClosing(System::Object^  sender, System::Windows::Forms::FormClosingEventArgs^  e) {
			 e->Cancel = true;
			 this->Hide();
		 }
};
}
