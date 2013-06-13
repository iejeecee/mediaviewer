#pragma once

#include "ResizeUpdateEventArgs.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;


namespace imageviewer {

	/// <summary>
	/// Summary for ResizeForm
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class ResizeForm : public System::Windows::Forms::Form
	{

		float widthScale;
		float heightScale;

		bool disableResolutionChangedHandler;

	public:
		
		// add a delegate
		delegate void ResizeUpdateHandler(System::Object^ sender, ResizeUpdateEventArgs^ e);
		// add an event of the delegate type
		event ResizeUpdateHandler ^OnResize;

		ResizeForm(Drawing::Size imageSize)
		{
			InitializeComponent();

			heightScale = imageSize.Height / float(imageSize.Width);
			widthScale = imageSize.Width / float(imageSize.Height);

			disableResolutionChangedHandler = true;

			numericUpDown1->Value = imageSize.Width;
			numericUpDown2->Value = imageSize.Height;

			disableResolutionChangedHandler = false;

			//
			//TODO: Add the constructor code here
			//
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~ResizeForm()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::NumericUpDown^  numericUpDown1;
	private: System::Windows::Forms::NumericUpDown^  numericUpDown2;
	private: System::Windows::Forms::Label^  label1;
	private: System::Windows::Forms::Label^  label2;
	private: System::Windows::Forms::Button^  button1;
	private: System::Windows::Forms::Button^  button2;
	private: System::Windows::Forms::CheckBox^  checkBox1;
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
			System::ComponentModel::ComponentResourceManager^  resources = (gcnew System::ComponentModel::ComponentResourceManager(ResizeForm::typeid));
			this->numericUpDown1 = (gcnew System::Windows::Forms::NumericUpDown());
			this->numericUpDown2 = (gcnew System::Windows::Forms::NumericUpDown());
			this->label1 = (gcnew System::Windows::Forms::Label());
			this->label2 = (gcnew System::Windows::Forms::Label());
			this->button1 = (gcnew System::Windows::Forms::Button());
			this->button2 = (gcnew System::Windows::Forms::Button());
			this->checkBox1 = (gcnew System::Windows::Forms::CheckBox());
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->numericUpDown1))->BeginInit();
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->numericUpDown2))->BeginInit();
			this->SuspendLayout();
			// 
			// numericUpDown1
			// 
			this->numericUpDown1->Location = System::Drawing::Point(25, 25);
			this->numericUpDown1->Maximum = System::Decimal(gcnew cli::array< System::Int32 >(4) {262144, 0, 0, 0});
			this->numericUpDown1->Name = L"numericUpDown1";
			this->numericUpDown1->Size = System::Drawing::Size(120, 26);
			this->numericUpDown1->TabIndex = 0;
			this->numericUpDown1->ValueChanged += gcnew System::EventHandler(this, &ResizeForm::widthChanged);
			// 
			// numericUpDown2
			// 
			this->numericUpDown2->Location = System::Drawing::Point(25, 57);
			this->numericUpDown2->Maximum = System::Decimal(gcnew cli::array< System::Int32 >(4) {262144, 0, 0, 0});
			this->numericUpDown2->Name = L"numericUpDown2";
			this->numericUpDown2->Size = System::Drawing::Size(120, 26);
			this->numericUpDown2->TabIndex = 1;
			this->numericUpDown2->ValueChanged += gcnew System::EventHandler(this, &ResizeForm::heightChanged);
			// 
			// label1
			// 
			this->label1->AutoSize = true;
			this->label1->Location = System::Drawing::Point(156, 27);
			this->label1->Name = L"label1";
			this->label1->Size = System::Drawing::Size(50, 20);
			this->label1->TabIndex = 2;
			this->label1->Text = L"Width";
			// 
			// label2
			// 
			this->label2->AutoSize = true;
			this->label2->Location = System::Drawing::Point(156, 63);
			this->label2->Name = L"label2";
			this->label2->Size = System::Drawing::Size(56, 20);
			this->label2->TabIndex = 3;
			this->label2->Text = L"Height";
			// 
			// button1
			// 
			this->button1->Location = System::Drawing::Point(25, 146);
			this->button1->Name = L"button1";
			this->button1->Size = System::Drawing::Size(97, 44);
			this->button1->TabIndex = 4;
			this->button1->Text = L"Ok";
			this->button1->UseVisualStyleBackColor = true;
			this->button1->Click += gcnew System::EventHandler(this, &ResizeForm::button1_Click);
			// 
			// button2
			// 
			this->button2->Location = System::Drawing::Point(128, 146);
			this->button2->Name = L"button2";
			this->button2->Size = System::Drawing::Size(97, 44);
			this->button2->TabIndex = 5;
			this->button2->Text = L"Cancel";
			this->button2->UseVisualStyleBackColor = true;
			this->button2->Click += gcnew System::EventHandler(this, &ResizeForm::button2_Click);
			// 
			// checkBox1
			// 
			this->checkBox1->AutoSize = true;
			this->checkBox1->Checked = true;
			this->checkBox1->CheckState = System::Windows::Forms::CheckState::Checked;
			this->checkBox1->Location = System::Drawing::Point(25, 101);
			this->checkBox1->Name = L"checkBox1";
			this->checkBox1->Size = System::Drawing::Size(181, 24);
			this->checkBox1->TabIndex = 6;
			this->checkBox1->Text = L"Constrain Proportions";
			this->checkBox1->UseVisualStyleBackColor = true;
			// 
			// ResizeForm
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(255, 212);
			this->ControlBox = false;
			this->Controls->Add(this->checkBox1);
			this->Controls->Add(this->button2);
			this->Controls->Add(this->button1);
			this->Controls->Add(this->label2);
			this->Controls->Add(this->label1);
			this->Controls->Add(this->numericUpDown2);
			this->Controls->Add(this->numericUpDown1);
			this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::FixedDialog;
			this->Icon = (cli::safe_cast<System::Drawing::Icon^  >(resources->GetObject(L"$this.Icon")));
			this->MaximumSize = System::Drawing::Size(261, 254);
			this->MinimumSize = System::Drawing::Size(261, 254);
			this->Name = L"ResizeForm";
			this->Text = L"Resize";
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->numericUpDown1))->EndInit();
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->numericUpDown2))->EndInit();
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion
	private: System::Void button2_Click(System::Object^  sender, System::EventArgs^  e) {
				 Form::Close();
			 }
	private: System::Void button1_Click(System::Object^  sender, System::EventArgs^  e) {

				 int width = Decimal::ToInt32(numericUpDown1->Value);
				 int height = Decimal::ToInt32(numericUpDown2->Value);

				 ResizeUpdateEventArgs ^args = gcnew ResizeUpdateEventArgs(width, height);

				 OnResize(this, args);		
				 Form::Close();
			 }
	private: System::Void widthChanged(System::Object^  sender, System::EventArgs^  e) {

				 if(disableResolutionChangedHandler == true) return;

				 int width = CLAMP(Decimal::ToInt32(numericUpDown1->Value), 1, MAX_IMAGE_SIZE);

				 if(checkBox1->Checked == true) {

					 disableResolutionChangedHandler = true;
					 numericUpDown2->Value = CLAMP(int(width * heightScale), 1, MAX_IMAGE_SIZE);
					 disableResolutionChangedHandler = false;
				 }
			 }
	private: System::Void heightChanged(System::Object^  sender, System::EventArgs^  e) {

				 if(disableResolutionChangedHandler == true) return;

				 int height = CLAMP(Decimal::ToInt32(numericUpDown2->Value), 1, MAX_IMAGE_SIZE);

				 if(checkBox1->Checked == true) {

					 disableResolutionChangedHandler = true;
					 numericUpDown1->Value = CLAMP(int(height * widthScale), 1, MAX_IMAGE_SIZE);
					 disableResolutionChangedHandler = false;
				 }
			 }
	};
}
