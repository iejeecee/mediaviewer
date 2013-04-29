#pragma once

#include "FileMetaData.h"
#include "MetaDataModel.h"
#include "TagEditorForm.h"
#include "GEventArgs.h"
#include "PagerControl.h"
#include "FileMetaData.h"
#include "WindowsUtils.h"
#include "MediaFileFactory.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Collections::Generic;
using namespace System::Collections::ObjectModel;
using namespace System::Windows::Forms;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;


using namespace Aga::Controls::Tree::NodeControls;
using namespace Aga::Controls::Tree;
using namespace Aga::Controls;


namespace imageviewer {

	/// <summary>
	/// Summary for MediaInfoForm
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class MediaInfoForm : public System::Windows::Forms::Form
	{
	public:
		MediaInfoForm(void)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
			boldFont = gcnew System::Drawing::Font(TreeViewAdv::DefaultFont, FontStyle::Bold);
			thumbnail = nullptr;

			media = gcnew List<MediaFile ^>();

			metaDataError = nullptr;
			useDefaultThumb = false;

			openFailure = nullptr;
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~MediaInfoForm()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::TabControl^  tabControl1;
	protected: 

	protected: 

	private: System::Windows::Forms::TabPage^  contentTabPage;
	private: System::Windows::Forms::TabPage^  authorTabPage;
	protected: 


	private: System::Windows::Forms::TextBox^  titleTextBox;

	private: System::Windows::Forms::Label^  label3;
	private: System::Windows::Forms::Label^  label2;
	private: System::Windows::Forms::ListBox^  tagsListBox;


	private: System::Windows::Forms::TextBox^  descriptionTextBox;

	private: System::Windows::Forms::Label^  label1;
	private: System::Windows::Forms::Button^  cancelButton;
	private: System::Windows::Forms::Button^  okButton;
	private: System::Windows::Forms::Label^  label4;
	private: System::Windows::Forms::TextBox^  addTagTextBox;
	private: System::Windows::Forms::Label^  label5;
	private: System::Windows::Forms::Label^  label6;
	private: System::Windows::Forms::TextBox^  creatorToolTextBox;
	private: System::Windows::Forms::TextBox^  authorTextBox;
	private: System::Windows::Forms::Label^  label7;
	private: System::Windows::Forms::DateTimePicker^  createDateDateTimePicker;
	private: System::Windows::Forms::TabPage^  fullTabPage;








	private: System::Windows::Forms::Label^  label9;
	private: System::Windows::Forms::DateTimePicker^  metaDataDateDateTimePicker;

	private: System::Windows::Forms::Label^  label8;
	private: System::Windows::Forms::DateTimePicker^  modifyDateDateTimePicker;

	private: System::Windows::Forms::TextBox^  copyrightTextBox;
	private: System::Windows::Forms::Label^  label10;
	private: Aga::Controls::Tree::TreeViewAdv^  miscTreeView;


	private: Aga::Controls::Tree::TreeColumn^  treeColumn1;
	private: Aga::Controls::Tree::TreeColumn^  treeColumn2;
	private: Aga::Controls::Tree::NodeControls::NodeTextBox^  nameNodeControl;
	private: Aga::Controls::Tree::NodeControls::NodeTextBox^  valueNodeControl;
	private: System::Windows::Forms::Button^  editTagsButton;

	private: System::Windows::Forms::Button^  addTagButton;
	private: System::Windows::Forms::ContextMenuStrip^  tagsContextMenuStrip;
	private: System::Windows::Forms::ToolStripMenuItem^  deleteToolStripMenuItem;
	private: System::Windows::Forms::TabPage^  content2TabPage;
	private: System::Windows::Forms::Button^  editTagsButton2;
	private: System::Windows::Forms::Button^  addTagButton2;
	private: System::Windows::Forms::TextBox^  addTagTextBox2;




	private: System::Windows::Forms::Label^  label12;
	private: System::Windows::Forms::Label^  label13;
	private: System::Windows::Forms::ListBox^  addTagsListBox;

	private: System::Windows::Forms::TextBox^  descriptionTextBox2;

	private: System::Windows::Forms::Label^  label14;
	private: System::Windows::Forms::TextBox^  titleTextBox2;
	private: System::Windows::Forms::Button^  removeTagButton;
	private: System::Windows::Forms::TextBox^  removeTagTextBox;



	private: System::Windows::Forms::Label^  label16;
	private: System::Windows::Forms::ListBox^  removeTagsListBox;
private: System::Windows::Forms::TabPage^  thumbTabPage;
private: System::Windows::Forms::TextBox^  thumbSizeTextBox;

private: System::Windows::Forms::Label^  label17;
private: System::Windows::Forms::TextBox^  thumbHeightTextBox;


private: System::Windows::Forms::Label^  label15;
private: System::Windows::Forms::TextBox^  thumbWidthTextBox;

private: System::Windows::Forms::Label^  label11;
private: System::Windows::Forms::PictureBox^  thumbPictureBox;

private: System::Windows::Forms::RadioButton^  deleteThumbRadioButton;

private: System::Windows::Forms::RadioButton^  defaultThumbRadioButton;
private: System::Windows::Forms::RadioButton^  browseThumbRadioButton;
private: System::Windows::Forms::RadioButton^  noChangeThumbRadioButton;
private: imageviewer::PagerControl^  pagerControl;














	private: System::ComponentModel::IContainer^  components;


























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
			System::ComponentModel::ComponentResourceManager^  resources = (gcnew System::ComponentModel::ComponentResourceManager(MediaInfoForm::typeid));
			this->tabControl1 = (gcnew System::Windows::Forms::TabControl());
			this->contentTabPage = (gcnew System::Windows::Forms::TabPage());
			this->editTagsButton = (gcnew System::Windows::Forms::Button());
			this->addTagButton = (gcnew System::Windows::Forms::Button());
			this->label4 = (gcnew System::Windows::Forms::Label());
			this->addTagTextBox = (gcnew System::Windows::Forms::TextBox());
			this->label3 = (gcnew System::Windows::Forms::Label());
			this->label2 = (gcnew System::Windows::Forms::Label());
			this->tagsListBox = (gcnew System::Windows::Forms::ListBox());
			this->tagsContextMenuStrip = (gcnew System::Windows::Forms::ContextMenuStrip(this->components));
			this->deleteToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->descriptionTextBox = (gcnew System::Windows::Forms::TextBox());
			this->label1 = (gcnew System::Windows::Forms::Label());
			this->titleTextBox = (gcnew System::Windows::Forms::TextBox());
			this->content2TabPage = (gcnew System::Windows::Forms::TabPage());
			this->removeTagButton = (gcnew System::Windows::Forms::Button());
			this->removeTagTextBox = (gcnew System::Windows::Forms::TextBox());
			this->label16 = (gcnew System::Windows::Forms::Label());
			this->removeTagsListBox = (gcnew System::Windows::Forms::ListBox());
			this->editTagsButton2 = (gcnew System::Windows::Forms::Button());
			this->addTagButton2 = (gcnew System::Windows::Forms::Button());
			this->addTagTextBox2 = (gcnew System::Windows::Forms::TextBox());
			this->label12 = (gcnew System::Windows::Forms::Label());
			this->label13 = (gcnew System::Windows::Forms::Label());
			this->addTagsListBox = (gcnew System::Windows::Forms::ListBox());
			this->descriptionTextBox2 = (gcnew System::Windows::Forms::TextBox());
			this->label14 = (gcnew System::Windows::Forms::Label());
			this->titleTextBox2 = (gcnew System::Windows::Forms::TextBox());
			this->authorTabPage = (gcnew System::Windows::Forms::TabPage());
			this->copyrightTextBox = (gcnew System::Windows::Forms::TextBox());
			this->label10 = (gcnew System::Windows::Forms::Label());
			this->label9 = (gcnew System::Windows::Forms::Label());
			this->metaDataDateDateTimePicker = (gcnew System::Windows::Forms::DateTimePicker());
			this->label8 = (gcnew System::Windows::Forms::Label());
			this->modifyDateDateTimePicker = (gcnew System::Windows::Forms::DateTimePicker());
			this->label7 = (gcnew System::Windows::Forms::Label());
			this->createDateDateTimePicker = (gcnew System::Windows::Forms::DateTimePicker());
			this->authorTextBox = (gcnew System::Windows::Forms::TextBox());
			this->label5 = (gcnew System::Windows::Forms::Label());
			this->label6 = (gcnew System::Windows::Forms::Label());
			this->creatorToolTextBox = (gcnew System::Windows::Forms::TextBox());
			this->thumbTabPage = (gcnew System::Windows::Forms::TabPage());
			this->pagerControl = (gcnew imageviewer::PagerControl());
			this->noChangeThumbRadioButton = (gcnew System::Windows::Forms::RadioButton());
			this->browseThumbRadioButton = (gcnew System::Windows::Forms::RadioButton());
			this->deleteThumbRadioButton = (gcnew System::Windows::Forms::RadioButton());
			this->defaultThumbRadioButton = (gcnew System::Windows::Forms::RadioButton());
			this->thumbSizeTextBox = (gcnew System::Windows::Forms::TextBox());
			this->label17 = (gcnew System::Windows::Forms::Label());
			this->thumbHeightTextBox = (gcnew System::Windows::Forms::TextBox());
			this->label15 = (gcnew System::Windows::Forms::Label());
			this->thumbWidthTextBox = (gcnew System::Windows::Forms::TextBox());
			this->label11 = (gcnew System::Windows::Forms::Label());
			this->thumbPictureBox = (gcnew System::Windows::Forms::PictureBox());
			this->fullTabPage = (gcnew System::Windows::Forms::TabPage());
			this->miscTreeView = (gcnew Aga::Controls::Tree::TreeViewAdv());
			this->treeColumn1 = (gcnew Aga::Controls::Tree::TreeColumn());
			this->treeColumn2 = (gcnew Aga::Controls::Tree::TreeColumn());
			this->nameNodeControl = (gcnew Aga::Controls::Tree::NodeControls::NodeTextBox());
			this->valueNodeControl = (gcnew Aga::Controls::Tree::NodeControls::NodeTextBox());
			this->cancelButton = (gcnew System::Windows::Forms::Button());
			this->okButton = (gcnew System::Windows::Forms::Button());
			this->tabControl1->SuspendLayout();
			this->contentTabPage->SuspendLayout();
			this->tagsContextMenuStrip->SuspendLayout();
			this->content2TabPage->SuspendLayout();
			this->authorTabPage->SuspendLayout();
			this->thumbTabPage->SuspendLayout();
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->thumbPictureBox))->BeginInit();
			this->fullTabPage->SuspendLayout();
			this->SuspendLayout();
			// 
			// tabControl1
			// 
			this->tabControl1->Controls->Add(this->contentTabPage);
			this->tabControl1->Controls->Add(this->content2TabPage);
			this->tabControl1->Controls->Add(this->authorTabPage);
			this->tabControl1->Controls->Add(this->thumbTabPage);
			this->tabControl1->Controls->Add(this->fullTabPage);
			this->tabControl1->Location = System::Drawing::Point(6, 12);
			this->tabControl1->Name = L"tabControl1";
			this->tabControl1->SelectedIndex = 0;
			this->tabControl1->Size = System::Drawing::Size(491, 516);
			this->tabControl1->TabIndex = 0;
			// 
			// contentTabPage
			// 
			this->contentTabPage->Controls->Add(this->editTagsButton);
			this->contentTabPage->Controls->Add(this->addTagButton);
			this->contentTabPage->Controls->Add(this->label4);
			this->contentTabPage->Controls->Add(this->addTagTextBox);
			this->contentTabPage->Controls->Add(this->label3);
			this->contentTabPage->Controls->Add(this->label2);
			this->contentTabPage->Controls->Add(this->tagsListBox);
			this->contentTabPage->Controls->Add(this->descriptionTextBox);
			this->contentTabPage->Controls->Add(this->label1);
			this->contentTabPage->Controls->Add(this->titleTextBox);
			this->contentTabPage->Location = System::Drawing::Point(4, 29);
			this->contentTabPage->Name = L"contentTabPage";
			this->contentTabPage->Padding = System::Windows::Forms::Padding(3);
			this->contentTabPage->Size = System::Drawing::Size(483, 483);
			this->contentTabPage->TabIndex = 0;
			this->contentTabPage->Text = L"Content";
			this->contentTabPage->UseVisualStyleBackColor = true;
			// 
			// editTagsButton
			// 
			this->editTagsButton->Location = System::Drawing::Point(385, 429);
			this->editTagsButton->Name = L"editTagsButton";
			this->editTagsButton->Size = System::Drawing::Size(92, 41);
			this->editTagsButton->TabIndex = 9;
			this->editTagsButton->Text = L"Edit Tags";
			this->editTagsButton->UseVisualStyleBackColor = true;
			this->editTagsButton->Click += gcnew System::EventHandler(this, &MediaInfoForm::editTagsButton_Click);
			// 
			// addTagButton
			// 
			this->addTagButton->Location = System::Drawing::Point(287, 429);
			this->addTagButton->Name = L"addTagButton";
			this->addTagButton->Size = System::Drawing::Size(92, 41);
			this->addTagButton->TabIndex = 8;
			this->addTagButton->Text = L"Add";
			this->addTagButton->UseVisualStyleBackColor = true;
			this->addTagButton->Click += gcnew System::EventHandler(this, &MediaInfoForm::addTagButton_Click);
			// 
			// label4
			// 
			this->label4->AutoSize = true;
			this->label4->Location = System::Drawing::Point(10, 413);
			this->label4->Name = L"label4";
			this->label4->Size = System::Drawing::Size(69, 20);
			this->label4->TabIndex = 7;
			this->label4->Text = L"Add Tag";
			// 
			// addTagTextBox
			// 
			this->addTagTextBox->Location = System::Drawing::Point(6, 436);
			this->addTagTextBox->Name = L"addTagTextBox";
			this->addTagTextBox->Size = System::Drawing::Size(275, 26);
			this->addTagTextBox->TabIndex = 6;
			this->addTagTextBox->KeyDown += gcnew System::Windows::Forms::KeyEventHandler(this, &MediaInfoForm::addTagTextBox_KeyDown);
			// 
			// label3
			// 
			this->label3->AutoSize = true;
			this->label3->Location = System::Drawing::Point(10, 222);
			this->label3->Name = L"label3";
			this->label3->Size = System::Drawing::Size(44, 20);
			this->label3->TabIndex = 5;
			this->label3->Text = L"Tags";
			// 
			// label2
			// 
			this->label2->AutoSize = true;
			this->label2->Location = System::Drawing::Point(10, 63);
			this->label2->Name = L"label2";
			this->label2->Size = System::Drawing::Size(89, 20);
			this->label2->TabIndex = 4;
			this->label2->Text = L"Description";
			// 
			// tagsListBox
			// 
			this->tagsListBox->ContextMenuStrip = this->tagsContextMenuStrip;
			this->tagsListBox->FormattingEnabled = true;
			this->tagsListBox->ItemHeight = 20;
			this->tagsListBox->Location = System::Drawing::Point(6, 245);
			this->tagsListBox->Name = L"tagsListBox";
			this->tagsListBox->SelectionMode = System::Windows::Forms::SelectionMode::MultiSimple;
			this->tagsListBox->Size = System::Drawing::Size(471, 164);
			this->tagsListBox->TabIndex = 3;
			// 
			// tagsContextMenuStrip
			// 
			this->tagsContextMenuStrip->Items->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(1) {this->deleteToolStripMenuItem});
			this->tagsContextMenuStrip->Name = L"tagsContextMenuStrip";
			this->tagsContextMenuStrip->Size = System::Drawing::Size(130, 30);
			// 
			// deleteToolStripMenuItem
			// 
			this->deleteToolStripMenuItem->Name = L"deleteToolStripMenuItem";
			this->deleteToolStripMenuItem->Size = System::Drawing::Size(129, 26);
			this->deleteToolStripMenuItem->Text = L"Delete";
			this->deleteToolStripMenuItem->Click += gcnew System::EventHandler(this, &MediaInfoForm::deleteToolStripMenuItem_Click);
			// 
			// descriptionTextBox
			// 
			this->descriptionTextBox->Location = System::Drawing::Point(6, 86);
			this->descriptionTextBox->Multiline = true;
			this->descriptionTextBox->Name = L"descriptionTextBox";
			this->descriptionTextBox->ScrollBars = System::Windows::Forms::ScrollBars::Vertical;
			this->descriptionTextBox->Size = System::Drawing::Size(471, 124);
			this->descriptionTextBox->TabIndex = 2;
			// 
			// label1
			// 
			this->label1->AutoSize = true;
			this->label1->Location = System::Drawing::Point(10, 10);
			this->label1->Name = L"label1";
			this->label1->Size = System::Drawing::Size(38, 20);
			this->label1->TabIndex = 1;
			this->label1->Text = L"Title";
			// 
			// titleTextBox
			// 
			this->titleTextBox->Location = System::Drawing::Point(6, 31);
			this->titleTextBox->Name = L"titleTextBox";
			this->titleTextBox->Size = System::Drawing::Size(471, 26);
			this->titleTextBox->TabIndex = 0;
			// 
			// content2TabPage
			// 
			this->content2TabPage->Controls->Add(this->removeTagButton);
			this->content2TabPage->Controls->Add(this->removeTagTextBox);
			this->content2TabPage->Controls->Add(this->label16);
			this->content2TabPage->Controls->Add(this->removeTagsListBox);
			this->content2TabPage->Controls->Add(this->editTagsButton2);
			this->content2TabPage->Controls->Add(this->addTagButton2);
			this->content2TabPage->Controls->Add(this->addTagTextBox2);
			this->content2TabPage->Controls->Add(this->label12);
			this->content2TabPage->Controls->Add(this->label13);
			this->content2TabPage->Controls->Add(this->addTagsListBox);
			this->content2TabPage->Controls->Add(this->descriptionTextBox2);
			this->content2TabPage->Controls->Add(this->label14);
			this->content2TabPage->Controls->Add(this->titleTextBox2);
			this->content2TabPage->Location = System::Drawing::Point(4, 29);
			this->content2TabPage->Name = L"content2TabPage";
			this->content2TabPage->Size = System::Drawing::Size(483, 483);
			this->content2TabPage->TabIndex = 3;
			this->content2TabPage->Text = L"Content";
			this->content2TabPage->UseVisualStyleBackColor = true;
			// 
			// removeTagButton
			// 
			this->removeTagButton->Location = System::Drawing::Point(287, 437);
			this->removeTagButton->Name = L"removeTagButton";
			this->removeTagButton->Size = System::Drawing::Size(92, 41);
			this->removeTagButton->TabIndex = 24;
			this->removeTagButton->Text = L"Remove";
			this->removeTagButton->UseVisualStyleBackColor = true;
			this->removeTagButton->Click += gcnew System::EventHandler(this, &MediaInfoForm::removeTagButton_Click);
			// 
			// removeTagTextBox
			// 
			this->removeTagTextBox->Location = System::Drawing::Point(7, 444);
			this->removeTagTextBox->Name = L"removeTagTextBox";
			this->removeTagTextBox->Size = System::Drawing::Size(275, 26);
			this->removeTagTextBox->TabIndex = 22;
			// 
			// label16
			// 
			this->label16->AutoSize = true;
			this->label16->Location = System::Drawing::Point(10, 344);
			this->label16->Name = L"label16";
			this->label16->Size = System::Drawing::Size(107, 20);
			this->label16->TabIndex = 21;
			this->label16->Text = L"Remove Tags";
			// 
			// removeTagsListBox
			// 
			this->removeTagsListBox->ContextMenuStrip = this->tagsContextMenuStrip;
			this->removeTagsListBox->FormattingEnabled = true;
			this->removeTagsListBox->ItemHeight = 20;
			this->removeTagsListBox->Location = System::Drawing::Point(6, 367);
			this->removeTagsListBox->Name = L"removeTagsListBox";
			this->removeTagsListBox->SelectionMode = System::Windows::Forms::SelectionMode::MultiSimple;
			this->removeTagsListBox->Size = System::Drawing::Size(471, 64);
			this->removeTagsListBox->TabIndex = 20;
			// 
			// editTagsButton2
			// 
			this->editTagsButton2->Location = System::Drawing::Point(385, 308);
			this->editTagsButton2->Name = L"editTagsButton2";
			this->editTagsButton2->Size = System::Drawing::Size(92, 41);
			this->editTagsButton2->TabIndex = 19;
			this->editTagsButton2->Text = L"Edit Tags";
			this->editTagsButton2->UseVisualStyleBackColor = true;
			// 
			// addTagButton2
			// 
			this->addTagButton2->Location = System::Drawing::Point(288, 308);
			this->addTagButton2->Name = L"addTagButton2";
			this->addTagButton2->Size = System::Drawing::Size(92, 41);
			this->addTagButton2->TabIndex = 18;
			this->addTagButton2->Text = L"Add";
			this->addTagButton2->UseVisualStyleBackColor = true;
			this->addTagButton2->Click += gcnew System::EventHandler(this, &MediaInfoForm::addTagButton2_Click);
			// 
			// addTagTextBox2
			// 
			this->addTagTextBox2->Location = System::Drawing::Point(7, 315);
			this->addTagTextBox2->Name = L"addTagTextBox2";
			this->addTagTextBox2->Size = System::Drawing::Size(275, 26);
			this->addTagTextBox2->TabIndex = 16;
			// 
			// label12
			// 
			this->label12->AutoSize = true;
			this->label12->Location = System::Drawing::Point(9, 216);
			this->label12->Name = L"label12";
			this->label12->Size = System::Drawing::Size(77, 20);
			this->label12->TabIndex = 15;
			this->label12->Text = L"Add Tags";
			// 
			// label13
			// 
			this->label13->AutoSize = true;
			this->label13->Location = System::Drawing::Point(10, 66);
			this->label13->Name = L"label13";
			this->label13->Size = System::Drawing::Size(89, 20);
			this->label13->TabIndex = 14;
			this->label13->Text = L"Description";
			// 
			// addTagsListBox
			// 
			this->addTagsListBox->ContextMenuStrip = this->tagsContextMenuStrip;
			this->addTagsListBox->FormattingEnabled = true;
			this->addTagsListBox->ItemHeight = 20;
			this->addTagsListBox->Location = System::Drawing::Point(6, 239);
			this->addTagsListBox->Name = L"addTagsListBox";
			this->addTagsListBox->SelectionMode = System::Windows::Forms::SelectionMode::MultiSimple;
			this->addTagsListBox->Size = System::Drawing::Size(471, 64);
			this->addTagsListBox->TabIndex = 13;
			// 
			// descriptionTextBox2
			// 
			this->descriptionTextBox2->Location = System::Drawing::Point(6, 89);
			this->descriptionTextBox2->Multiline = true;
			this->descriptionTextBox2->Name = L"descriptionTextBox2";
			this->descriptionTextBox2->ScrollBars = System::Windows::Forms::ScrollBars::Vertical;
			this->descriptionTextBox2->Size = System::Drawing::Size(471, 124);
			this->descriptionTextBox2->TabIndex = 12;
			// 
			// label14
			// 
			this->label14->AutoSize = true;
			this->label14->Location = System::Drawing::Point(10, 13);
			this->label14->Name = L"label14";
			this->label14->Size = System::Drawing::Size(38, 20);
			this->label14->TabIndex = 11;
			this->label14->Text = L"Title";
			// 
			// titleTextBox2
			// 
			this->titleTextBox2->Location = System::Drawing::Point(6, 34);
			this->titleTextBox2->Name = L"titleTextBox2";
			this->titleTextBox2->Size = System::Drawing::Size(471, 26);
			this->titleTextBox2->TabIndex = 10;
			// 
			// authorTabPage
			// 
			this->authorTabPage->Controls->Add(this->copyrightTextBox);
			this->authorTabPage->Controls->Add(this->label10);
			this->authorTabPage->Controls->Add(this->label9);
			this->authorTabPage->Controls->Add(this->metaDataDateDateTimePicker);
			this->authorTabPage->Controls->Add(this->label8);
			this->authorTabPage->Controls->Add(this->modifyDateDateTimePicker);
			this->authorTabPage->Controls->Add(this->label7);
			this->authorTabPage->Controls->Add(this->createDateDateTimePicker);
			this->authorTabPage->Controls->Add(this->authorTextBox);
			this->authorTabPage->Controls->Add(this->label5);
			this->authorTabPage->Controls->Add(this->label6);
			this->authorTabPage->Controls->Add(this->creatorToolTextBox);
			this->authorTabPage->Location = System::Drawing::Point(4, 29);
			this->authorTabPage->Name = L"authorTabPage";
			this->authorTabPage->Padding = System::Windows::Forms::Padding(3);
			this->authorTabPage->Size = System::Drawing::Size(483, 483);
			this->authorTabPage->TabIndex = 1;
			this->authorTabPage->Text = L"Author";
			this->authorTabPage->UseVisualStyleBackColor = true;
			// 
			// copyrightTextBox
			// 
			this->copyrightTextBox->Location = System::Drawing::Point(6, 140);
			this->copyrightTextBox->Multiline = true;
			this->copyrightTextBox->Name = L"copyrightTextBox";
			this->copyrightTextBox->ScrollBars = System::Windows::Forms::ScrollBars::Vertical;
			this->copyrightTextBox->Size = System::Drawing::Size(471, 170);
			this->copyrightTextBox->TabIndex = 16;
			// 
			// label10
			// 
			this->label10->AutoSize = true;
			this->label10->Location = System::Drawing::Point(6, 117);
			this->label10->Name = L"label10";
			this->label10->Size = System::Drawing::Size(76, 20);
			this->label10->TabIndex = 15;
			this->label10->Text = L"Copyright";
			// 
			// label9
			// 
			this->label9->AutoSize = true;
			this->label9->Location = System::Drawing::Point(7, 425);
			this->label9->Name = L"label9";
			this->label9->Size = System::Drawing::Size(116, 20);
			this->label9->TabIndex = 14;
			this->label9->Text = L"Metadata Date";
			// 
			// metaDataDateDateTimePicker
			// 
			this->metaDataDateDateTimePicker->CustomFormat = L"dddd d MMMM yyyy H:mm:ss";
			this->metaDataDateDateTimePicker->Enabled = false;
			this->metaDataDateDateTimePicker->Format = System::Windows::Forms::DateTimePickerFormat::Custom;
			this->metaDataDateDateTimePicker->Location = System::Drawing::Point(4, 448);
			this->metaDataDateDateTimePicker->Name = L"metaDataDateDateTimePicker";
			this->metaDataDateDateTimePicker->Size = System::Drawing::Size(318, 26);
			this->metaDataDateDateTimePicker->TabIndex = 13;
			// 
			// label8
			// 
			this->label8->AutoSize = true;
			this->label8->Location = System::Drawing::Point(6, 370);
			this->label8->Name = L"label8";
			this->label8->Size = System::Drawing::Size(94, 20);
			this->label8->TabIndex = 12;
			this->label8->Text = L"Modify Date";
			// 
			// modifyDateDateTimePicker
			// 
			this->modifyDateDateTimePicker->CustomFormat = L"dddd d MMMM yyyy H:mm:ss";
			this->modifyDateDateTimePicker->Enabled = false;
			this->modifyDateDateTimePicker->Format = System::Windows::Forms::DateTimePickerFormat::Custom;
			this->modifyDateDateTimePicker->Location = System::Drawing::Point(4, 393);
			this->modifyDateDateTimePicker->Name = L"modifyDateDateTimePicker";
			this->modifyDateDateTimePicker->Size = System::Drawing::Size(318, 26);
			this->modifyDateDateTimePicker->TabIndex = 11;
			// 
			// label7
			// 
			this->label7->AutoSize = true;
			this->label7->Location = System::Drawing::Point(6, 313);
			this->label7->Name = L"label7";
			this->label7->Size = System::Drawing::Size(96, 20);
			this->label7->TabIndex = 10;
			this->label7->Text = L"Create Date";
			// 
			// createDateDateTimePicker
			// 
			this->createDateDateTimePicker->CustomFormat = L"dddd d MMMM yyyy H:mm:ss";
			this->createDateDateTimePicker->Enabled = false;
			this->createDateDateTimePicker->Format = System::Windows::Forms::DateTimePickerFormat::Custom;
			this->createDateDateTimePicker->Location = System::Drawing::Point(3, 336);
			this->createDateDateTimePicker->Name = L"createDateDateTimePicker";
			this->createDateDateTimePicker->Size = System::Drawing::Size(318, 26);
			this->createDateDateTimePicker->TabIndex = 9;
			// 
			// authorTextBox
			// 
			this->authorTextBox->Location = System::Drawing::Point(5, 79);
			this->authorTextBox->Name = L"authorTextBox";
			this->authorTextBox->Size = System::Drawing::Size(471, 26);
			this->authorTextBox->TabIndex = 8;
			// 
			// label5
			// 
			this->label5->AutoSize = true;
			this->label5->Location = System::Drawing::Point(6, 56);
			this->label5->Name = L"label5";
			this->label5->Size = System::Drawing::Size(62, 20);
			this->label5->TabIndex = 7;
			this->label5->Text = L"Creator";
			// 
			// label6
			// 
			this->label6->AutoSize = true;
			this->label6->Location = System::Drawing::Point(6, 3);
			this->label6->Name = L"label6";
			this->label6->Size = System::Drawing::Size(96, 20);
			this->label6->TabIndex = 6;
			this->label6->Text = L"Creator Tool";
			// 
			// creatorToolTextBox
			// 
			this->creatorToolTextBox->Location = System::Drawing::Point(5, 24);
			this->creatorToolTextBox->Name = L"creatorToolTextBox";
			this->creatorToolTextBox->Size = System::Drawing::Size(471, 26);
			this->creatorToolTextBox->TabIndex = 5;
			// 
			// thumbTabPage
			// 
			this->thumbTabPage->Controls->Add(this->pagerControl);
			this->thumbTabPage->Controls->Add(this->noChangeThumbRadioButton);
			this->thumbTabPage->Controls->Add(this->browseThumbRadioButton);
			this->thumbTabPage->Controls->Add(this->deleteThumbRadioButton);
			this->thumbTabPage->Controls->Add(this->defaultThumbRadioButton);
			this->thumbTabPage->Controls->Add(this->thumbSizeTextBox);
			this->thumbTabPage->Controls->Add(this->label17);
			this->thumbTabPage->Controls->Add(this->thumbHeightTextBox);
			this->thumbTabPage->Controls->Add(this->label15);
			this->thumbTabPage->Controls->Add(this->thumbWidthTextBox);
			this->thumbTabPage->Controls->Add(this->label11);
			this->thumbTabPage->Controls->Add(this->thumbPictureBox);
			this->thumbTabPage->Location = System::Drawing::Point(4, 29);
			this->thumbTabPage->Name = L"thumbTabPage";
			this->thumbTabPage->Size = System::Drawing::Size(483, 483);
			this->thumbTabPage->TabIndex = 4;
			this->thumbTabPage->Text = L"Thumb";
			this->thumbTabPage->UseVisualStyleBackColor = true;
			// 
			// pagerControl
			// 
			this->pagerControl->BeginButtonEnabled = true;
			this->pagerControl->CurrentPage = 0;
			this->pagerControl->EndButtonEnabled = true;
			this->pagerControl->Location = System::Drawing::Point(101, 322);
			this->pagerControl->Name = L"pagerControl";
			this->pagerControl->NextButtonEnabled = true;
			this->pagerControl->PrevButtonEnabled = true;
			this->pagerControl->Size = System::Drawing::Size(280, 47);
			this->pagerControl->TabIndex = 12;
			this->pagerControl->TotalPages = 0;
			// 
			// noChangeThumbRadioButton
			// 
			this->noChangeThumbRadioButton->AutoSize = true;
			this->noChangeThumbRadioButton->Checked = true;
			this->noChangeThumbRadioButton->Location = System::Drawing::Point(319, 424);
			this->noChangeThumbRadioButton->Name = L"noChangeThumbRadioButton";
			this->noChangeThumbRadioButton->Size = System::Drawing::Size(104, 24);
			this->noChangeThumbRadioButton->TabIndex = 11;
			this->noChangeThumbRadioButton->TabStop = true;
			this->noChangeThumbRadioButton->Text = L"No change";
			this->noChangeThumbRadioButton->UseVisualStyleBackColor = true;
			this->noChangeThumbRadioButton->CheckedChanged += gcnew System::EventHandler(this, &MediaInfoForm::noChangeThumbRadioButton_CheckedChanged);
			// 
			// browseThumbRadioButton
			// 
			this->browseThumbRadioButton->AutoSize = true;
			this->browseThumbRadioButton->Location = System::Drawing::Point(153, 424);
			this->browseThumbRadioButton->Name = L"browseThumbRadioButton";
			this->browseThumbRadioButton->Size = System::Drawing::Size(80, 24);
			this->browseThumbRadioButton->TabIndex = 10;
			this->browseThumbRadioButton->Text = L"Browse";
			this->browseThumbRadioButton->UseVisualStyleBackColor = true;
			this->browseThumbRadioButton->CheckedChanged += gcnew System::EventHandler(this, &MediaInfoForm::browseThumbRadioButton_CheckedChanged);
			// 
			// deleteThumbRadioButton
			// 
			this->deleteThumbRadioButton->AutoSize = true;
			this->deleteThumbRadioButton->Location = System::Drawing::Point(239, 424);
			this->deleteThumbRadioButton->Name = L"deleteThumbRadioButton";
			this->deleteThumbRadioButton->Size = System::Drawing::Size(74, 24);
			this->deleteThumbRadioButton->TabIndex = 9;
			this->deleteThumbRadioButton->Text = L"Delete";
			this->deleteThumbRadioButton->UseVisualStyleBackColor = true;
			this->deleteThumbRadioButton->CheckedChanged += gcnew System::EventHandler(this, &MediaInfoForm::deleteThumbRadioButton_CheckedChanged);
			// 
			// defaultThumbRadioButton
			// 
			this->defaultThumbRadioButton->AutoSize = true;
			this->defaultThumbRadioButton->Location = System::Drawing::Point(68, 424);
			this->defaultThumbRadioButton->Name = L"defaultThumbRadioButton";
			this->defaultThumbRadioButton->Size = System::Drawing::Size(79, 24);
			this->defaultThumbRadioButton->TabIndex = 8;
			this->defaultThumbRadioButton->Text = L"Default";
			this->defaultThumbRadioButton->UseVisualStyleBackColor = true;
			this->defaultThumbRadioButton->CheckedChanged += gcnew System::EventHandler(this, &MediaInfoForm::defaultThumbRadioButton_CheckedChanged);
			// 
			// thumbSizeTextBox
			// 
			this->thumbSizeTextBox->Location = System::Drawing::Point(308, 254);
			this->thumbSizeTextBox->Name = L"thumbSizeTextBox";
			this->thumbSizeTextBox->ReadOnly = true;
			this->thumbSizeTextBox->Size = System::Drawing::Size(117, 26);
			this->thumbSizeTextBox->TabIndex = 6;
			// 
			// label17
			// 
			this->label17->AutoSize = true;
			this->label17->Location = System::Drawing::Point(304, 231);
			this->label17->Name = L"label17";
			this->label17->Size = System::Drawing::Size(40, 20);
			this->label17->TabIndex = 5;
			this->label17->Text = L"Size";
			// 
			// thumbHeightTextBox
			// 
			this->thumbHeightTextBox->Location = System::Drawing::Point(185, 254);
			this->thumbHeightTextBox->Name = L"thumbHeightTextBox";
			this->thumbHeightTextBox->ReadOnly = true;
			this->thumbHeightTextBox->Size = System::Drawing::Size(117, 26);
			this->thumbHeightTextBox->TabIndex = 4;
			// 
			// label15
			// 
			this->label15->AutoSize = true;
			this->label15->Location = System::Drawing::Point(182, 231);
			this->label15->Name = L"label15";
			this->label15->Size = System::Drawing::Size(56, 20);
			this->label15->TabIndex = 3;
			this->label15->Text = L"Height";
			// 
			// thumbWidthTextBox
			// 
			this->thumbWidthTextBox->Location = System::Drawing::Point(62, 254);
			this->thumbWidthTextBox->Name = L"thumbWidthTextBox";
			this->thumbWidthTextBox->ReadOnly = true;
			this->thumbWidthTextBox->Size = System::Drawing::Size(117, 26);
			this->thumbWidthTextBox->TabIndex = 2;
			// 
			// label11
			// 
			this->label11->AutoSize = true;
			this->label11->Location = System::Drawing::Point(58, 231);
			this->label11->Name = L"label11";
			this->label11->Size = System::Drawing::Size(50, 20);
			this->label11->TabIndex = 1;
			this->label11->Text = L"Width";
			// 
			// thumbPictureBox
			// 
			this->thumbPictureBox->BorderStyle = System::Windows::Forms::BorderStyle::FixedSingle;
			this->thumbPictureBox->Location = System::Drawing::Point(108, 15);
			this->thumbPictureBox->Name = L"thumbPictureBox";
			this->thumbPictureBox->Size = System::Drawing::Size(267, 195);
			this->thumbPictureBox->SizeMode = System::Windows::Forms::PictureBoxSizeMode::CenterImage;
			this->thumbPictureBox->TabIndex = 0;
			this->thumbPictureBox->TabStop = false;
			// 
			// fullTabPage
			// 
			this->fullTabPage->Controls->Add(this->miscTreeView);
			this->fullTabPage->Location = System::Drawing::Point(4, 29);
			this->fullTabPage->Name = L"fullTabPage";
			this->fullTabPage->Size = System::Drawing::Size(483, 483);
			this->fullTabPage->TabIndex = 2;
			this->fullTabPage->Text = L"Full";
			this->fullTabPage->UseVisualStyleBackColor = true;
			// 
			// miscTreeView
			// 
			this->miscTreeView->AutoRowHeight = true;
			this->miscTreeView->BackColor = System::Drawing::SystemColors::Window;
			this->miscTreeView->Columns->Add(this->treeColumn1);
			this->miscTreeView->Columns->Add(this->treeColumn2);
			this->miscTreeView->DefaultToolTipProvider = nullptr;
			this->miscTreeView->Dock = System::Windows::Forms::DockStyle::Fill;
			this->miscTreeView->DragDropMarkColor = System::Drawing::Color::Black;
			this->miscTreeView->FullRowSelect = true;
			this->miscTreeView->GridLineStyle = Aga::Controls::Tree::GridLineStyle::Vertical;
			this->miscTreeView->LineColor = System::Drawing::SystemColors::ControlDark;
			this->miscTreeView->Location = System::Drawing::Point(0, 0);
			this->miscTreeView->Model = nullptr;
			this->miscTreeView->Name = L"miscTreeView";
			this->miscTreeView->NodeControls->Add(this->nameNodeControl);
			this->miscTreeView->NodeControls->Add(this->valueNodeControl);
			this->miscTreeView->SelectedNode = nullptr;
			this->miscTreeView->ShowNodeToolTips = true;
			this->miscTreeView->Size = System::Drawing::Size(483, 483);
			this->miscTreeView->TabIndex = 0;
			this->miscTreeView->Text = L"treeViewAdv1";
			this->miscTreeView->UseColumns = true;
			// 
			// treeColumn1
			// 
			this->treeColumn1->Header = L"Name";
			this->treeColumn1->SortOrder = System::Windows::Forms::SortOrder::None;
			this->treeColumn1->TooltipText = nullptr;
			this->treeColumn1->Width = 250;
			// 
			// treeColumn2
			// 
			this->treeColumn2->Header = L"Value";
			this->treeColumn2->SortOrder = System::Windows::Forms::SortOrder::None;
			this->treeColumn2->TooltipText = nullptr;
			this->treeColumn2->Width = 229;
			// 
			// nameNodeControl
			// 
			this->nameNodeControl->DataPropertyName = L"Name";
			this->nameNodeControl->IncrementalSearchEnabled = true;
			this->nameNodeControl->LeftMargin = 3;
			this->nameNodeControl->ParentColumn = this->treeColumn1;
			// 
			// valueNodeControl
			// 
			this->valueNodeControl->DataPropertyName = L"Value";
			this->valueNodeControl->IncrementalSearchEnabled = true;
			this->valueNodeControl->LeftMargin = 3;
			this->valueNodeControl->ParentColumn = this->treeColumn2;
			// 
			// cancelButton
			// 
			this->cancelButton->DialogResult = System::Windows::Forms::DialogResult::Cancel;
			this->cancelButton->Location = System::Drawing::Point(102, 534);
			this->cancelButton->Name = L"cancelButton";
			this->cancelButton->Size = System::Drawing::Size(86, 41);
			this->cancelButton->TabIndex = 19;
			this->cancelButton->Text = L"Cancel";
			this->cancelButton->UseVisualStyleBackColor = true;
			this->cancelButton->Click += gcnew System::EventHandler(this, &MediaInfoForm::cancelButton_Click);
			// 
			// okButton
			// 
			this->okButton->DialogResult = System::Windows::Forms::DialogResult::OK;
			this->okButton->Location = System::Drawing::Point(10, 534);
			this->okButton->Name = L"okButton";
			this->okButton->Size = System::Drawing::Size(86, 41);
			this->okButton->TabIndex = 18;
			this->okButton->Text = L"Save";
			this->okButton->UseVisualStyleBackColor = true;
			this->okButton->Click += gcnew System::EventHandler(this, &MediaInfoForm::okButton_Click);
			// 
			// MediaInfoForm
			// 
			this->AcceptButton = this->okButton;
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->CancelButton = this->cancelButton;
			this->ClientSize = System::Drawing::Size(502, 587);
			this->Controls->Add(this->tabControl1);
			this->Controls->Add(this->okButton);
			this->Controls->Add(this->cancelButton);
			this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::FixedDialog;
			this->Icon = (cli::safe_cast<System::Drawing::Icon^  >(resources->GetObject(L"$this.Icon")));
			this->MaximizeBox = false;
			this->Name = L"MediaInfoForm";
			this->Text = L"MediaInfoForm";
			this->Shown += gcnew System::EventHandler(this, &MediaInfoForm::mediaInfoForm_Shown);
			this->FormClosing += gcnew System::Windows::Forms::FormClosingEventHandler(this, &MediaInfoForm::mediaInfoForm_FormClosing);
			this->tabControl1->ResumeLayout(false);
			this->contentTabPage->ResumeLayout(false);
			this->contentTabPage->PerformLayout();
			this->tagsContextMenuStrip->ResumeLayout(false);
			this->content2TabPage->ResumeLayout(false);
			this->content2TabPage->PerformLayout();
			this->authorTabPage->ResumeLayout(false);
			this->authorTabPage->PerformLayout();
			this->thumbTabPage->ResumeLayout(false);
			this->thumbTabPage->PerformLayout();
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->thumbPictureBox))->EndInit();
			this->fullTabPage->ResumeLayout(false);
			this->ResumeLayout(false);

		}
#pragma endregion

private:
	
	static log4net::ILog ^log = log4net::LogManager::GetLogger(System::Reflection::MethodBase::GetCurrentMethod()->DeclaringType);

	MetaDataThumb ^thumbnail;
	bool isBatch;
	System::Drawing::Font ^boldFont;

	MediaFileFactory ^mediaFileFactory;
	List<MediaFile ^> ^media;

	String ^metaDataError;
	bool useDefaultThumb;
	bool deleteThumbs;
	bool useBrowsedThumb;

	Exception ^openFailure;

	

	MetaDataThumb ^generateThumbnail(String ^path) {

		Image ^fullImage = gcnew Bitmap(path);

		int resizedHeight, resizedWidth;

		ImageUtils::resizeRectangle(fullImage->Width, fullImage->Height, 160, 160, resizedHeight, resizedWidth);
	
		Image ^thumbnail = ImageUtils::resizeImage(fullImage, resizedHeight, resizedWidth);

		delete fullImage;

		return(gcnew MetaDataThumb(thumbnail));
	}

public:

	property List<String ^> ^FileNames {

		void set(List<String ^> ^fileNames) { 

			media->Clear();
			openFailure = nullptr;

			for each(String ^fileName in fileNames) {

				MediaFile ^mediaFile = MediaFileFactory::openBlocking(fileName,
					MediaFile::MetaDataMode::LOAD_FROM_DISK);				
				if(mediaFile->OpenSuccess == false) {

					openFailure = mediaFile->OpenError;
					return;

				} else if(mediaFile->MetaDataError) {

					openFailure = mediaFile->MetaDataError;
					return;
				}

				media->Add(mediaFile);

			}

			if(media->Count == 1) {

				Text = Path::GetFileName(fileNames[0]) + " - Metadata";
				IsBatch = false;

				if(media[0]->MetaData->Thumbnail->Count > 0) {

					Thumbnail = media[0]->MetaData->Thumbnail[0];
				}

			} else {

				Text = Convert::ToString(media->Count) + " files - Combined Metadata";
				IsBatch = true;
			}


			MetaDataTreeNode ^tree = media[0]->MetaData->Tree;

			for(int i = 1; i < media->Count; i++) {

				MetaDataTreeNode ^tempTree = media[i]->MetaData->Tree;

				tree = tree->intersection(tempTree);

			}			

			MetaDataTreeArray ^arr = dynamic_cast<MetaDataTreeArray ^>(tree->getNode("dc:title"));
			if(arr != nullptr && arr->Count > 0) {
				Title = arr[0]->Data;
			}

			arr = dynamic_cast<MetaDataTreeArray ^>(tree->getNode("dc:description"));
			if(arr != nullptr && arr->Count > 0) {
				Description = arr[0]->Data;
			}

			arr = dynamic_cast<MetaDataTreeArray ^>(tree->getNode("dc:creator"));
			if(arr != nullptr && arr->Count > 0) {
				Author = arr[0]->Data;
			}

			arr = dynamic_cast<MetaDataTreeArray ^>(tree->getNode("dc:rights"));
			if(arr != nullptr && arr->Count > 0) {
				Copyright = arr[0]->Data;
			}

			MetaDataTreeProperty ^prop = dynamic_cast<MetaDataTreeProperty ^>(tree->getNode("xmp:CreatorTool"));
			if(prop != nullptr) {
				CreatorTool = prop->Value;
			}

			prop = dynamic_cast<MetaDataTreeProperty ^>(tree->getNode("xmp:MetadataDate"));
			if(prop != nullptr) {
				MetaDataDate = MetaData::convertToDate(prop->Value);
			} else {
				MetaDataDate = DateTime::MinValue;
			}

			prop = dynamic_cast<MetaDataTreeProperty ^>(tree->getNode("xmp:CreateDate"));
			if(prop != nullptr) {
				CreateDate = MetaData::convertToDate(prop->Value);
			} else {
				CreateDate = DateTime::MinValue;
			}

			prop = dynamic_cast<MetaDataTreeProperty ^>(tree->getNode("xmp:ModifyDate"));
			if(prop != nullptr) {
				ModifyDate = MetaData::convertToDate(prop->Value);
			} else {
				ModifyDate = DateTime::MinValue;
			}

			List<String ^> ^tags = gcnew List<String ^>();

			arr = dynamic_cast<MetaDataTreeArray ^>(tree->getNode("dc:subject"));
			if(arr != nullptr) {

				for each(MetaDataTreeNode ^n in arr->Child) {

					tags->Add(n->Data);
				}
			}

			Misc = tree;				
			Tags = tags;

		}

		List<String ^> ^get() {

			List<String ^> ^fileNames = gcnew List<String ^>();

			for each(MediaFile ^mediaFile in media) {

				fileNames->Add(mediaFile->Location);
			}

			return(fileNames);

		}
	}

	property bool IsBatch {

			void set(bool isBatch) {

				this->isBatch = isBatch;

				if(isBatch == false) {

					tabControl1->TabPages->Remove(content2TabPage);

				} else {

					tabControl1->TabPages->Remove(contentTabPage);
				}
			}

			bool get() {

				return(isBatch);
			}
		}

	property String ^Title 
			{

				void set(String ^title) {

					titleTextBox->Text = title;
					titleTextBox2->Text = title;
				}

				String ^get() {

					if(IsBatch == false) {

						return(titleTextBox->Text);

					} else {

						return(titleTextBox2->Text);
					}
				}
			}


	property String ^Description 
			{

				void set(String ^description) {

					descriptionTextBox->Text = description;
					descriptionTextBox2->Text = description;
				}

				String ^get() {

					if(IsBatch == false) {

						return(descriptionTextBox->Text);

					} else {

						return(descriptionTextBox2->Text);
					}

				}
			}

	property List<String ^> ^Tags
			{

				void set(List<String ^> ^tags) {

					tagsListBox->Items->Clear();

					for each(String ^tag in tags) {

						tagsListBox->Items->Add(tag);
					}
				}

				List<String ^> ^get() {

					List<String ^> ^tags = gcnew List<String ^>();

					for each(Object ^item in tagsListBox->Items) {

						tags->Add(dynamic_cast<String ^>(item));
					}

					return(tags);
				}
			}

	property List<String ^> ^AddTags
			{

				void set(List<String ^> ^tags) {

					addTagsListBox->Items->Clear();

					for each(String ^tag in tags) {

						addTagsListBox->Items->Add(tag);
					}
				}

				List<String ^> ^get() {

					List<String ^> ^tags = gcnew List<String ^>();

					for each(Object ^item in addTagsListBox->Items) {

						tags->Add(dynamic_cast<String ^>(item));
					}

					return(tags);
				}
			}

	property List<String ^> ^RemoveTags
			{

				void set(List<String ^> ^tags) {

					removeTagsListBox->Items->Clear();

					for each(String ^tag in tags) {

						removeTagsListBox->Items->Add(tag);
					}
				}

				List<String ^> ^get() {

					List<String ^> ^tags = gcnew List<String ^>();

					for each(Object ^item in removeTagsListBox->Items) {

						tags->Add(dynamic_cast<String ^>(item));
					}

					return(tags);
				}
			}
	property String ^Author 
			{

				void set(String ^author) {

					authorTextBox->Text = author;
				}

				String ^get() {

					return(authorTextBox->Text);
				}
			}

	property String ^CreatorTool
			{

				void set(String ^creatorTool) {

					creatorToolTextBox->Text = creatorTool;
				}

				String ^get() {

					return(creatorToolTextBox->Text);
				}
			}
	property String ^Copyright
			{

				void set(String ^copyright) {

					copyrightTextBox->Text = copyright;
				}

				String ^get() {

					return(copyrightTextBox->Text);
				}
			}
	property DateTime MetaDataDate
			{

				void set(DateTime metaDataDate) {

					if(metaDataDate.Equals(DateTime::MinValue)) {

						metaDataDateDateTimePicker->CustomFormat = " ";

					} else {

						metaDataDateDateTimePicker->Value = metaDataDate;
					}
				}

				DateTime get() {

					return(metaDataDateDateTimePicker->Value);
				}
			}
	property DateTime CreateDate
			{

				void set(DateTime createDate) {

					if(createDate.Equals(DateTime::MinValue)) {

						createDateDateTimePicker->CustomFormat = " ";

					} else {

						createDateDateTimePicker->Value = createDate;
					}
				}

				DateTime get() {

					return(createDateDateTimePicker->Value);
				}
			}
	property DateTime ModifyDate
			{

				void set(DateTime modifyDate) {

					if(modifyDate.Equals(DateTime::MinValue)) {

						modifyDateDateTimePicker->CustomFormat = " ";

					} else {

						modifyDateDateTimePicker->Value = modifyDate;
					}
				}

				DateTime get() {

					return(modifyDateDateTimePicker->Value);
				}
			}

	property MetaDataThumb ^Thumbnail
			{
				void set(MetaDataThumb ^thumbnail) {

					this->thumbnail = thumbnail;

					if(thumbnail != nullptr) {

						thumbPictureBox->Image = thumbnail->ThumbImage;
						thumbWidthTextBox->Text = Convert::ToString(thumbnail->Width);
						thumbHeightTextBox->Text = Convert::ToString(thumbnail->Height);
						thumbSizeTextBox->Text = Convert::ToString(thumbnail->Data->Length / 1024) + " kb";

					} else {

						thumbPictureBox->Image = nullptr;
						thumbWidthTextBox->Text = "";
						thumbHeightTextBox->Text = "";
						thumbSizeTextBox->Text = "";
					}
				}

				MetaDataThumb ^get() {

					return(thumbnail);
				}
			}
	property MetaDataTreeNode ^Misc
			{

				void set(MetaDataTreeNode ^node) {

					MetaDataModel ^model = gcnew MetaDataModel(node);
					nameNodeControl->DrawText += gcnew System::EventHandler<DrawEventArgs ^>(this, &MediaInfoForm::miscTreeView_DrawNode);
					//miscTreeView->Font = gcnew Drawing::Font(TreeView::DefaultFont, FontStyle::Regular);
					miscTreeView->Model = model;


					miscTreeView->ExpandAll();
				}

			}

	private: System::Void okButton_Click(System::Object^  sender, System::EventArgs^  e) {

				 try {

					 for each(MediaFile ^mediaFile in media) {

						 mediaFile->MetaData->Creator = Author;
						 mediaFile->MetaData->CreatorTool = CreatorTool;
						 mediaFile->MetaData->Title = Title;
						 mediaFile->MetaData->Description = Description;
						 mediaFile->MetaData->Copyright = Copyright;

						 List<MetaDataThumb ^> ^thumbs = gcnew List<MetaDataThumb ^>();

						 if(useDefaultThumb) {

							 thumbs = mediaFile->getThumbnails();

						 } else if(useBrowsedThumb) {

							 thumbs->Add(Thumbnail);

						 }

						 mediaFile->MetaData->Thumbnail = thumbs;

						 if(IsBatch) {

							 for each(String ^tag in AddTags) {

								 if(!mediaFile->MetaData->Tags->Contains(tag)) {

									 mediaFile->MetaData->Tags->Add(tag);
								 }
							 }

							 for each(String ^tag in RemoveTags) {

								 if(mediaFile->MetaData->Tags->Contains(tag)) {

									 mediaFile->MetaData->Tags->Remove(tag);
								 }
							 }

						 } else {

							 mediaFile->MetaData->Tags = Tags;
						 }

						 // close mediafile so it can obtain write access
						 // before saving
						 mediaFile->close();

						 mediaFile->MetaData->save();
					 }					 

				 } catch (Exception ^e) {

					 log->Error("Error while saving metadata", e);
					 MessageBox::Show(e->Message, "Error");

				 } finally {

					 Form::Close();
				 }
			 }
	private: System::Void miscTreeView_DrawNode(System::Object^  sender, DrawEventArgs ^e) {

				 MetaDataItem ^temp = (MetaDataItem ^)e->Node->Tag;

				 if(temp->Node->NodeType == MetaDataTreeNode::Type::NAMESPACE) {
					
					 e->TextColor = Color::Blue;
					 e->Font = boldFont;

				 }  
			}

private: System::Void editTagsButton_Click(System::Object^  sender, System::EventArgs^  e) {

			 TagEditorForm ^tagEditor = gcnew TagEditorForm();

			 tagEditor->ShowDialog();
		 }
private: void addTag(String ^tag, ListBox ^listBox) {

			 if(String::IsNullOrEmpty(tag)) return;

			 if(listBox->Items->Contains(tag) == true) return;

			 listBox->Items->Add(tag);

			 // scroll tags listbox to bottom
			 int visibleItems = listBox->ClientSize.Height / listBox->ItemHeight;
			 listBox->TopIndex = Math::Max(listBox->Items->Count - visibleItems + 1, 0);

		 }

private: System::Void addTagButton_Click(System::Object^  sender, System::EventArgs^  e) {
	
			 addTag(addTagTextBox->Text, tagsListBox);	
			 addTagTextBox->Clear();
		 }

private: System::Void addTagTextBox_KeyDown(System::Object^  sender, System::Windows::Forms::KeyEventArgs^  e) {

			 if(e->KeyCode == Keys::Enter) {
				
				  addTag(addTagTextBox->Text, tagsListBox);
				  addTagTextBox->Clear();
			 }
		 }

private: System::Void deleteToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

			 while(tagsListBox->SelectedItems->Count > 0) {

				 tagsListBox->Items->Remove(tagsListBox->SelectedItems[0]);
			 }
		 }

private: System::Void addTagButton2_Click(System::Object^  sender, System::EventArgs^  e) {

			 addTag(addTagTextBox2->Text, addTagsListBox);	
			 addTagTextBox2->Clear();
		 }

private: System::Void removeTagButton_Click(System::Object^  sender, System::EventArgs^  e) {

			 addTag(removeTagTextBox->Text, removeTagsListBox);	
			 removeTagTextBox->Clear();
		 }

private: System::Void cancelButton_Click(System::Object^  sender, System::EventArgs^  e) {

			 Form::Close();
		 }

private: System::Void defaultThumbRadioButton_CheckedChanged(System::Object^  sender, System::EventArgs^  e) {

			 useDefaultThumb = defaultThumbRadioButton->Checked;

			 if(IsBatch == false && defaultThumbRadioButton->Checked == true) {

				 try {

					 List<MetaDataThumb ^> ^thumbs = media[0]->getThumbnails();

					 if(thumbs->Count > 0) {

						Thumbnail = thumbs[0];
					 }

				 } catch (Exception ^e) {

					 log->Error("Error generating thumbnails", e);
					 MessageBox::Show(e->Message);
				 }

			 }
		 }
private: System::Void deleteThumbRadioButton_CheckedChanged(System::Object^  sender, System::EventArgs^  e) {

			 deleteThumbs = deleteThumbRadioButton->Checked;

			 if(deleteThumbRadioButton->Checked == true) {

				 Thumbnail = nullptr;
			 }
		 }
private: System::Void browseThumbRadioButton_CheckedChanged(System::Object^  sender, System::EventArgs^  e) {

			 if((useBrowsedThumb = browseThumbRadioButton->Checked) == false) return;

			 OpenFileDialog ^openFileDialog = WindowsUtils::createOpenMediaFileDialog(true);

			 if(openFileDialog->ShowDialog() == ::DialogResult::OK)
			 {			
				 try {

					 Thumbnail = generateThumbnail(openFileDialog->FileName);					

				 } catch (Exception ^e) {

					 log->Error("Error generating thumbnail", e);
					 MessageBox::Show(e->Message);
				 }

			 } else {

				 Thumbnail = nullptr;
			 }
		 }
private: System::Void noChangeThumbRadioButton_CheckedChanged(System::Object^  sender, System::EventArgs^  e) {

			 if(noChangeThumbRadioButton->Checked == false) return;

			 if(IsBatch) {

				 Thumbnail = nullptr;

			 } else {

				 if(media[0]->MetaData->Thumbnail->Count > 0) {

					 Thumbnail = media[0]->MetaData->Thumbnail[0];

				 } else {

					 Thumbnail = nullptr;
				 }
			 }
			 
		 }
private: System::Void mediaInfoForm_Shown(System::Object^  sender, System::EventArgs^  e) {

			 if(openFailure != nullptr) {

				 MessageBox::Show(openFailure->Message, "Error");
				 Close();
			 }
		 }
private: System::Void mediaInfoForm_FormClosing(System::Object^  sender, System::Windows::Forms::FormClosingEventArgs^  e) {

			 for(int i = 0; i < media->Count; i++) {

				 media[i]->close();

			 }			

		 }
};
}
