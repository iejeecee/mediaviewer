#pragma once

#include "CommentTreeView.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;


namespace imageviewer {

	/// <summary>
	/// Summary for CommentForm
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class CommentForm : public System::Windows::Forms::Form
	{
	public:
		CommentForm(void)
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
		~CommentForm()
		{
			if (components)
			{
				delete components;
			}
		}
	private: imageviewer::CommentTreeView^  commentTreeView1;
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
			this->commentTreeView1 = (gcnew imageviewer::CommentTreeView());
			this->SuspendLayout();
			// 
			// commentTreeView1
			// 
			this->commentTreeView1->Dock = System::Windows::Forms::DockStyle::Fill;
			this->commentTreeView1->DrawMode = System::Windows::Forms::TreeViewDrawMode::OwnerDrawText;
			this->commentTreeView1->Location = System::Drawing::Point(0, 0);
			this->commentTreeView1->Name = L"commentTreeView1";
			this->commentTreeView1->Size = System::Drawing::Size(290, 254);
			this->commentTreeView1->TabIndex = 0;
			// 
			// CommentForm
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(290, 254);
			this->Controls->Add(this->commentTreeView1);
			this->Name = L"CommentForm";
			this->Text = L"CommentForm";
			this->ResumeLayout(false);

		}
#pragma endregion
	};
}
