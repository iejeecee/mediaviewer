#pragma once
#include "MediaSearchState.h"
#include "SearchOptionsForm.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;


namespace imageviewer {

	/// <summary>
	/// Summary for SearchTextBoxControl
	/// </summary>
	public ref class SearchTextBoxControl : public System::Windows::Forms::UserControl
	{
	public:
		SearchTextBoxControl(void)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
			SetStyle(ControlStyles::SupportsTransparentBackColor, true);
			this->BackColor = Color::Transparent;

			firstEnter = true;
			searchState = gcnew MediaSearchState();
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~SearchTextBoxControl()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::TextBox^  queryTextBox;
	protected: 

	private: System::Windows::Forms::Button^  optionsButton;
	private: System::Windows::Forms::ContextMenuStrip^  contextMenuStrip;
	private: System::Windows::Forms::ToolStripMenuItem^  searchOptionsToolStripMenuItem;
	private: System::Windows::Forms::ToolTip^  toolTip;
	private: System::Windows::Forms::ToolStripMenuItem^  selectAllToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  clearToolStripMenuItem;


	private: System::Windows::Forms::ToolStripSeparator^  toolStripSeparator1;
	private: System::ComponentModel::IContainer^  components;
	protected: 

	private:
		/// <summary>
		/// Required designer variable.
		/// </summary>


#pragma region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent(void)
		{
			this->components = (gcnew System::ComponentModel::Container());
			System::ComponentModel::ComponentResourceManager^  resources = (gcnew System::ComponentModel::ComponentResourceManager(SearchTextBoxControl::typeid));
			this->queryTextBox = (gcnew System::Windows::Forms::TextBox());
			this->optionsButton = (gcnew System::Windows::Forms::Button());
			this->contextMenuStrip = (gcnew System::Windows::Forms::ContextMenuStrip(this->components));
			this->selectAllToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->clearToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->toolStripSeparator1 = (gcnew System::Windows::Forms::ToolStripSeparator());
			this->searchOptionsToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->toolTip = (gcnew System::Windows::Forms::ToolTip(this->components));
			this->contextMenuStrip->SuspendLayout();
			this->SuspendLayout();
			// 
			// queryTextBox
			// 
			this->queryTextBox->ContextMenuStrip = this->contextMenuStrip;
			this->queryTextBox->Location = System::Drawing::Point(0, 3);
			this->queryTextBox->Name = L"queryTextBox";
			this->queryTextBox->Size = System::Drawing::Size(200, 26);
			this->queryTextBox->TabIndex = 0;
			this->queryTextBox->Text = L"Search";
			this->queryTextBox->KeyDown += gcnew System::Windows::Forms::KeyEventHandler(this, &SearchTextBoxControl::queryTextBox_KeyDown);
			this->queryTextBox->Enter += gcnew System::EventHandler(this, &SearchTextBoxControl::queryTextBox_Enter);
			// 
			// optionsButton
			// 
			this->optionsButton->BackColor = System::Drawing::SystemColors::ButtonFace;
			this->optionsButton->ContextMenuStrip = this->contextMenuStrip;
			this->optionsButton->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"optionsButton.Image")));
			this->optionsButton->Location = System::Drawing::Point(206, 3);
			this->optionsButton->Name = L"optionsButton";
			this->optionsButton->Size = System::Drawing::Size(44, 26);
			this->optionsButton->TabIndex = 1;
			this->toolTip->SetToolTip(this->optionsButton, L"Search");
			this->optionsButton->UseVisualStyleBackColor = false;
			this->optionsButton->Click += gcnew System::EventHandler(this, &SearchTextBoxControl::optionsButton_Click);
			// 
			// contextMenuStrip
			// 
			this->contextMenuStrip->Items->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(4) {this->selectAllToolStripMenuItem, 
				this->clearToolStripMenuItem, this->toolStripSeparator1, this->searchOptionsToolStripMenuItem});
			this->contextMenuStrip->Name = L"contextMenuStrip";
			this->contextMenuStrip->Size = System::Drawing::Size(193, 88);
			// 
			// selectAllToolStripMenuItem
			// 
			this->selectAllToolStripMenuItem->Name = L"selectAllToolStripMenuItem";
			this->selectAllToolStripMenuItem->Size = System::Drawing::Size(192, 26);
			this->selectAllToolStripMenuItem->Text = L"Select All";
			this->selectAllToolStripMenuItem->Click += gcnew System::EventHandler(this, &SearchTextBoxControl::selectAllToolStripMenuItem_Click);
			// 
			// clearToolStripMenuItem
			// 
			this->clearToolStripMenuItem->Name = L"clearToolStripMenuItem";
			this->clearToolStripMenuItem->Size = System::Drawing::Size(192, 26);
			this->clearToolStripMenuItem->Text = L"Clear";
			this->clearToolStripMenuItem->Click += gcnew System::EventHandler(this, &SearchTextBoxControl::clearToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this->toolStripSeparator1->Name = L"toolStripSeparator1";
			this->toolStripSeparator1->Size = System::Drawing::Size(189, 6);
			// 
			// searchOptionsToolStripMenuItem
			// 
			this->searchOptionsToolStripMenuItem->Name = L"searchOptionsToolStripMenuItem";
			this->searchOptionsToolStripMenuItem->Size = System::Drawing::Size(192, 26);
			this->searchOptionsToolStripMenuItem->Text = L"Search Options";
			this->searchOptionsToolStripMenuItem->Click += gcnew System::EventHandler(this, &SearchTextBoxControl::searchOptionsToolStripMenuItem_Click);
			// 
			// SearchTextBoxControl
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->Controls->Add(this->optionsButton);
			this->Controls->Add(this->queryTextBox);
			this->Name = L"SearchTextBoxControl";
			this->Size = System::Drawing::Size(256, 34);
			this->contextMenuStrip->ResumeLayout(false);
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion

	private:

		bool firstEnter;
		MediaSearchState ^searchState;

	public:

		event EventHandler<MediaSearchState ^> ^DoSearch;

		property String ^Query
		{
			String ^get()
			{
				return queryTextBox->Text;
			}

			void set(String ^query)
			{
				queryTextBox->Text = query;
			}
		}

	private: System::Void optionsButton_Click(System::Object^  sender, System::EventArgs^  e) {
				 
				 if(firstEnter == true) return;

				 searchState->Query = Query;

				 DoSearch(this, searchState);
			 }
	private: System::Void queryTextBox_KeyDown(System::Object^  sender, System::Windows::Forms::KeyEventArgs^  e) {

				 if(e->KeyCode == Keys::Enter) {

					 searchState->Query = Query;

					 DoSearch(this, searchState);
				 }
			 }
	private: System::Void queryTextBox_Enter(System::Object^  sender, System::EventArgs^  e) {

				 if(firstEnter == true) {

					 queryTextBox->Text = "";

					 firstEnter = false;
				 }
			 }
private: System::Void searchOptionsToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

			 SearchOptionsForm ^searchOptions = gcnew SearchOptionsForm();

			 searchOptions->SearchFileNames = searchState->DoFileNameSearch;
			 searchOptions->SearchTags = searchState->DoTagSearch;
			 searchOptions->RecurseDirectories = searchState->RecurseDirectories;
			
			 if(searchOptions->ShowDialog() == System::Windows::Forms::DialogResult::OK) {

				 searchState->DoFileNameSearch = searchOptions->SearchFileNames;
				 searchState->DoTagSearch = searchOptions->SearchTags;
				 searchState->RecurseDirectories = searchOptions->RecurseDirectories;
			 }

		 }


private: System::Void selectAllToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

			 queryTextBox->SelectionStart = 0;
			 queryTextBox->SelectionLength = queryTextBox->Text->Length;
		 }
private: System::Void clearToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

			 queryTextBox->Text = "";
		 }
};
}
