#pragma once

#include "About.h"
#include "ImagePanelControl.h"
#include "ResizeForm.h"
#include "UploadImage.h"
#include "UploadOutputForm.h"
#include "ImageSearchForm.h"
#include "PicasaForm.h"
#include "ImageFileBrowserControl.h"
#include "DownloadProgressForm.h"
#include "CommentForm.h"
#include "Tags.h"
#include "ImageUtils.h"
#include "ImageFile.h"
#include "HRTimerTest.h"
#include "VideoPanelControl.h"
#include "Settings.h"
#include "YoutubeForm.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;

[assembly: log4net::Config::XmlConfigurator(ConfigFile = "log4net.config",Watch=true)];

namespace imageviewer {

	/// <summary>
	/// Summary for MainForm
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class MainForm : public System::Windows::Forms::Form
	{
	private: System::Windows::Forms::ToolStripButton^  imageToolStripButton;
	private: System::Windows::Forms::ToolStripButton^  imageFileBrowserToolStripButton;

	private: System::Windows::Forms::ToolStripButton^  imageSearchToolStripButton;
	private: imageviewer::ImagePanelControl^  imagePanel;
	private: imageviewer::ImageFileBrowserControl^  imageFileBrowser;
	private: System::Windows::Forms::ToolStripSeparator^  toolStripSeparator1;
	private: System::Windows::Forms::ToolStripButton^  saveImageToolStripButton;
	private: System::Windows::Forms::ToolStripButton^  resizeImageToolStripButton;
	private: System::Windows::Forms::ToolStripButton^  cropImageToolStripButton;
	private: System::Windows::Forms::ToolStripButton^  verticalMirrorImageToolStripButton;
	private: System::Windows::Forms::ToolStripButton^  horizontalMirrorImageToolStripButton;
	private: System::Windows::Forms::ToolStripButton^  rotateImageMinToolStripButton;
	private: System::Windows::Forms::ToolStripButton^  rotateImagePlusToolStripButton;
	private: PicasaForm ^picasa;
	private: ImageSearchForm ^imageSearch;
	private: System::Windows::Forms::ToolStripButton^  autoScaleToolStripButton;
	private: imageviewer::VideoPanelControl^  videoPanel;
	private: System::Windows::Forms::ToolStripButton^  videoToolStripButton;
	private: System::Windows::Forms::ToolStripSeparator^  toolStripSeparator3;
	private: System::Windows::Forms::ToolStripMenuItem^  settingsToolStripMenuItem;
	private: System::Windows::Forms::ToolStripSeparator^  toolStripSeparator2;
	private: System::Windows::Forms::ToolStripButton^  youtubeToolStripButton;
	private: System::Windows::Forms::ToolStripSeparator^  toolStripSeparator4;
	private: System::Windows::Forms::ToolStripMenuItem^  logToolStripMenuItem;










	private: System::Windows::Forms::ToolStripMenuItem^  openURLToolStripMenuItem;

	public:
		MainForm(array<System::String ^> ^args)
		{

			log->Info("Starting Application");
			
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
			this->KeyPreview = true;
			this->MouseWheel += gcnew MouseEventHandler(this, &MainForm::mainForm_MouseWheel);
			this->imageFileBrowser->imageGrid->ImageGridMouseDown += 
				gcnew EventHandler<ImageGridMouseEventArgs ^>(this, &MainForm::imageFileBrowser_MouseDown);

			this->imageFileBrowser->CurrentMediaChanged +=
				gcnew EventHandler<FileSystemEventArgs ^>(this, &MainForm::imageFileBrowser_CurrentMediaChanged);

			setTitle();

			System::Net::ServicePointManager::Expect100Continue = false;

			System::Diagnostics::Process::GetCurrentProcess()->PriorityClass = System::Diagnostics::ProcessPriorityClass::RealTime;

			//HRTimerTest::test();

			if(args->Length != 0) {

				loadMedia(args[0]);

			} 
	
			ActiveControl = imagePanel;

			
			
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~MainForm()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::MenuStrip^  menuStrip1;
	protected: 
	private: System::Windows::Forms::ToolStripMenuItem^  fileToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  openToolStripMenuItem;

	private: System::Windows::Forms::ToolStripMenuItem^  exitToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  aboutToolStripMenuItem;
	private: System::Windows::Forms::ToolStripContainer^  toolStripContainer1;
	private: System::Windows::Forms::ToolStrip^  toolStrip1;
	private: System::Windows::Forms::ToolStripButton^  picasaToolStripButton;


	private: System::Windows::Forms::ToolStripMenuItem^  aboutToolStripMenuItem1;

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
			System::ComponentModel::ComponentResourceManager^  resources = (gcnew System::ComponentModel::ComponentResourceManager(MainForm::typeid));
			this->menuStrip1 = (gcnew System::Windows::Forms::MenuStrip());
			this->fileToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->openToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->openURLToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->toolStripSeparator3 = (gcnew System::Windows::Forms::ToolStripSeparator());
			this->settingsToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->toolStripSeparator2 = (gcnew System::Windows::Forms::ToolStripSeparator());
			this->exitToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->aboutToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->aboutToolStripMenuItem1 = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->toolStripContainer1 = (gcnew System::Windows::Forms::ToolStripContainer());
			this->videoPanel = (gcnew imageviewer::VideoPanelControl());
			this->imagePanel = (gcnew imageviewer::ImagePanelControl());
			this->imageFileBrowser = (gcnew imageviewer::ImageFileBrowserControl());
			this->toolStrip1 = (gcnew System::Windows::Forms::ToolStrip());
			this->imageToolStripButton = (gcnew System::Windows::Forms::ToolStripButton());
			this->videoToolStripButton = (gcnew System::Windows::Forms::ToolStripButton());
			this->imageFileBrowserToolStripButton = (gcnew System::Windows::Forms::ToolStripButton());
			this->autoScaleToolStripButton = (gcnew System::Windows::Forms::ToolStripButton());
			this->youtubeToolStripButton = (gcnew System::Windows::Forms::ToolStripButton());
			this->picasaToolStripButton = (gcnew System::Windows::Forms::ToolStripButton());
			this->imageSearchToolStripButton = (gcnew System::Windows::Forms::ToolStripButton());
			this->toolStripSeparator1 = (gcnew System::Windows::Forms::ToolStripSeparator());
			this->saveImageToolStripButton = (gcnew System::Windows::Forms::ToolStripButton());
			this->resizeImageToolStripButton = (gcnew System::Windows::Forms::ToolStripButton());
			this->cropImageToolStripButton = (gcnew System::Windows::Forms::ToolStripButton());
			this->verticalMirrorImageToolStripButton = (gcnew System::Windows::Forms::ToolStripButton());
			this->horizontalMirrorImageToolStripButton = (gcnew System::Windows::Forms::ToolStripButton());
			this->rotateImageMinToolStripButton = (gcnew System::Windows::Forms::ToolStripButton());
			this->rotateImagePlusToolStripButton = (gcnew System::Windows::Forms::ToolStripButton());
			this->toolStripSeparator4 = (gcnew System::Windows::Forms::ToolStripSeparator());
			this->logToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->menuStrip1->SuspendLayout();
			this->toolStripContainer1->ContentPanel->SuspendLayout();
			this->toolStripContainer1->SuspendLayout();
			this->toolStrip1->SuspendLayout();
			this->SuspendLayout();
			// 
			// menuStrip1
			// 
			this->menuStrip1->Items->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(2) {this->fileToolStripMenuItem, 
				this->aboutToolStripMenuItem});
			this->menuStrip1->Location = System::Drawing::Point(0, 0);
			this->menuStrip1->Name = L"menuStrip1";
			this->menuStrip1->Size = System::Drawing::Size(668, 29);
			this->menuStrip1->TabIndex = 0;
			this->menuStrip1->Text = L"menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this->fileToolStripMenuItem->DropDownItems->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(7) {this->openToolStripMenuItem, 
				this->openURLToolStripMenuItem, this->toolStripSeparator3, this->settingsToolStripMenuItem, this->logToolStripMenuItem, this->toolStripSeparator2, 
				this->exitToolStripMenuItem});
			this->fileToolStripMenuItem->Name = L"fileToolStripMenuItem";
			this->fileToolStripMenuItem->Size = System::Drawing::Size(48, 25);
			this->fileToolStripMenuItem->Text = L"File";
			// 
			// openToolStripMenuItem
			// 
			this->openToolStripMenuItem->Name = L"openToolStripMenuItem";
			this->openToolStripMenuItem->Size = System::Drawing::Size(226, 26);
			this->openToolStripMenuItem->Text = L"Open";
			this->openToolStripMenuItem->Click += gcnew System::EventHandler(this, &MainForm::openToolStripMenuItem_Click);
			// 
			// openURLToolStripMenuItem
			// 
			this->openURLToolStripMenuItem->Name = L"openURLToolStripMenuItem";
			this->openURLToolStripMenuItem->Size = System::Drawing::Size(226, 26);
			this->openURLToolStripMenuItem->Text = L"Open URL";
			this->openURLToolStripMenuItem->Click += gcnew System::EventHandler(this, &MainForm::openURLToolStripMenuItem_Click);
			// 
			// toolStripSeparator3
			// 
			this->toolStripSeparator3->Name = L"toolStripSeparator3";
			this->toolStripSeparator3->Size = System::Drawing::Size(223, 6);
			// 
			// settingsToolStripMenuItem
			// 
			this->settingsToolStripMenuItem->Name = L"settingsToolStripMenuItem";
			this->settingsToolStripMenuItem->Size = System::Drawing::Size(226, 26);
			this->settingsToolStripMenuItem->Text = L"Settings";
			this->settingsToolStripMenuItem->Click += gcnew System::EventHandler(this, &MainForm::settingsToolStripMenuItem_Click);
			// 
			// toolStripSeparator2
			// 
			this->toolStripSeparator2->Name = L"toolStripSeparator2";
			this->toolStripSeparator2->Size = System::Drawing::Size(223, 6);
			// 
			// exitToolStripMenuItem
			// 
			this->exitToolStripMenuItem->Name = L"exitToolStripMenuItem";
			this->exitToolStripMenuItem->Size = System::Drawing::Size(226, 26);
			this->exitToolStripMenuItem->Text = L"Exit";
			this->exitToolStripMenuItem->Click += gcnew System::EventHandler(this, &MainForm::exitToolStripMenuItem_Click);
			// 
			// aboutToolStripMenuItem
			// 
			this->aboutToolStripMenuItem->DropDownItems->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(1) {this->aboutToolStripMenuItem1});
			this->aboutToolStripMenuItem->Name = L"aboutToolStripMenuItem";
			this->aboutToolStripMenuItem->Size = System::Drawing::Size(55, 25);
			this->aboutToolStripMenuItem->Text = L"Help";
			// 
			// aboutToolStripMenuItem1
			// 
			this->aboutToolStripMenuItem1->Name = L"aboutToolStripMenuItem1";
			this->aboutToolStripMenuItem1->Size = System::Drawing::Size(152, 26);
			this->aboutToolStripMenuItem1->Text = L"About";
			this->aboutToolStripMenuItem1->Click += gcnew System::EventHandler(this, &MainForm::aboutToolStripMenuItem1_Click);
			// 
			// toolStripContainer1
			// 
			// 
			// toolStripContainer1.ContentPanel
			// 
			this->toolStripContainer1->ContentPanel->Controls->Add(this->videoPanel);
			this->toolStripContainer1->ContentPanel->Controls->Add(this->imagePanel);
			this->toolStripContainer1->ContentPanel->Controls->Add(this->imageFileBrowser);
			this->toolStripContainer1->ContentPanel->Size = System::Drawing::Size(668, 458);
			this->toolStripContainer1->Dock = System::Windows::Forms::DockStyle::Fill;
			this->toolStripContainer1->Location = System::Drawing::Point(0, 54);
			this->toolStripContainer1->Name = L"toolStripContainer1";
			this->toolStripContainer1->Size = System::Drawing::Size(668, 483);
			this->toolStripContainer1->TabIndex = 1;
			this->toolStripContainer1->Text = L"toolStripContainer1";
			// 
			// videoPanel
			// 
			this->videoPanel->Dock = System::Windows::Forms::DockStyle::Fill;
			this->videoPanel->Location = System::Drawing::Point(0, 0);
			this->videoPanel->Name = L"videoPanel";
			this->videoPanel->Size = System::Drawing::Size(668, 458);
			this->videoPanel->TabIndex = 2;
			// 
			// imagePanel
			// 
			this->imagePanel->AutoScroll = true;
			this->imagePanel->CropStage = imageviewer::ImagePanelControl::CropStageState::DISABLED;
			this->imagePanel->Cursor = System::Windows::Forms::Cursors::Cross;
			this->imagePanel->DisplayMode = imageviewer::ImagePanelControl::DisplayModeState::NORMAL;
			this->imagePanel->Dock = System::Windows::Forms::DockStyle::Fill;
			this->imagePanel->Location = System::Drawing::Point(0, 0);
			this->imagePanel->Name = L"imagePanel";
			this->imagePanel->Size = System::Drawing::Size(668, 458);
			this->imagePanel->TabIndex = 0;
			this->imagePanel->PreviewKeyDown += gcnew System::Windows::Forms::PreviewKeyDownEventHandler(this, &MainForm::imagePanel_PreviewKeyDown);
			this->imagePanel->LoadImageFinished += gcnew System::EventHandler<System::EventArgs^ >(this, &MainForm::imagePanel_LoadImageFinished);
			// 
			// imageFileBrowser
			// 
			this->imageFileBrowser->Dock = System::Windows::Forms::DockStyle::Fill;
			this->imageFileBrowser->Location = System::Drawing::Point(0, 0);
			this->imageFileBrowser->Name = L"imageFileBrowser";
			this->imageFileBrowser->Size = System::Drawing::Size(668, 458);
			this->imageFileBrowser->TabIndex = 1;
			this->imageFileBrowser->OnChangeBrowseDirectory += gcnew imageviewer::ImageFileBrowserControl::ChangeBrowseDirectoryEventHandler(this, &MainForm::imageFileBrowser_ChangeBrowseDirectoryEvent);
			this->imageFileBrowser->OnViewEvent += gcnew imageviewer::ImageFileBrowserControl::ImageFileBrowserEventHandler(this, &MainForm::imageFileBrowser_ViewEvent);
			// 
			// toolStrip1
			// 
			this->toolStrip1->Items->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(16) {this->imageToolStripButton, 
				this->videoToolStripButton, this->imageFileBrowserToolStripButton, this->autoScaleToolStripButton, this->toolStripSeparator4, 
				this->youtubeToolStripButton, this->picasaToolStripButton, this->imageSearchToolStripButton, this->toolStripSeparator1, this->saveImageToolStripButton, 
				this->resizeImageToolStripButton, this->cropImageToolStripButton, this->verticalMirrorImageToolStripButton, this->horizontalMirrorImageToolStripButton, 
				this->rotateImageMinToolStripButton, this->rotateImagePlusToolStripButton});
			this->toolStrip1->Location = System::Drawing::Point(0, 29);
			this->toolStrip1->Name = L"toolStrip1";
			this->toolStrip1->Size = System::Drawing::Size(668, 25);
			this->toolStrip1->TabIndex = 2;
			this->toolStrip1->Text = L"toolStrip1";
			// 
			// imageToolStripButton
			// 
			this->imageToolStripButton->Checked = true;
			this->imageToolStripButton->CheckOnClick = true;
			this->imageToolStripButton->CheckState = System::Windows::Forms::CheckState::Checked;
			this->imageToolStripButton->DisplayStyle = System::Windows::Forms::ToolStripItemDisplayStyle::Image;
			this->imageToolStripButton->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"imageToolStripButton.Image")));
			this->imageToolStripButton->ImageScaling = System::Windows::Forms::ToolStripItemImageScaling::None;
			this->imageToolStripButton->ImageTransparentColor = System::Drawing::Color::Magenta;
			this->imageToolStripButton->Name = L"imageToolStripButton";
			this->imageToolStripButton->Size = System::Drawing::Size(23, 22);
			this->imageToolStripButton->Text = L"Image";
			this->imageToolStripButton->ToolTipText = L"Photo";
			this->imageToolStripButton->Click += gcnew System::EventHandler(this, &MainForm::imageToolStripButton_Click);
			// 
			// videoToolStripButton
			// 
			this->videoToolStripButton->CheckOnClick = true;
			this->videoToolStripButton->DisplayStyle = System::Windows::Forms::ToolStripItemDisplayStyle::Image;
			this->videoToolStripButton->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"videoToolStripButton.Image")));
			this->videoToolStripButton->ImageTransparentColor = System::Drawing::Color::Magenta;
			this->videoToolStripButton->Name = L"videoToolStripButton";
			this->videoToolStripButton->Size = System::Drawing::Size(23, 22);
			this->videoToolStripButton->Text = L"view video";
			this->videoToolStripButton->ToolTipText = L"Video";
			this->videoToolStripButton->Click += gcnew System::EventHandler(this, &MainForm::videoToolStripButton_Click);
			// 
			// imageFileBrowserToolStripButton
			// 
			this->imageFileBrowserToolStripButton->CheckOnClick = true;
			this->imageFileBrowserToolStripButton->DisplayStyle = System::Windows::Forms::ToolStripItemDisplayStyle::Image;
			this->imageFileBrowserToolStripButton->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"imageFileBrowserToolStripButton.Image")));
			this->imageFileBrowserToolStripButton->ImageScaling = System::Windows::Forms::ToolStripItemImageScaling::None;
			this->imageFileBrowserToolStripButton->ImageTransparentColor = System::Drawing::Color::Magenta;
			this->imageFileBrowserToolStripButton->Name = L"imageFileBrowserToolStripButton";
			this->imageFileBrowserToolStripButton->Size = System::Drawing::Size(23, 22);
			this->imageFileBrowserToolStripButton->Text = L"Browse Media";
			this->imageFileBrowserToolStripButton->Click += gcnew System::EventHandler(this, &MainForm::imageFileBrowserToolStripButton_Click);
			// 
			// autoScaleToolStripButton
			// 
			this->autoScaleToolStripButton->CheckOnClick = true;
			this->autoScaleToolStripButton->DisplayStyle = System::Windows::Forms::ToolStripItemDisplayStyle::Image;
			this->autoScaleToolStripButton->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"autoScaleToolStripButton.Image")));
			this->autoScaleToolStripButton->ImageTransparentColor = System::Drawing::Color::Magenta;
			this->autoScaleToolStripButton->Name = L"autoScaleToolStripButton";
			this->autoScaleToolStripButton->Size = System::Drawing::Size(23, 22);
			this->autoScaleToolStripButton->Text = L"toolStripButton1";
			this->autoScaleToolStripButton->ToolTipText = L"Auto Scale Image";
			this->autoScaleToolStripButton->Click += gcnew System::EventHandler(this, &MainForm::autoScaleToolStripButton_Click);
			// 
			// youtubeToolStripButton
			// 
			this->youtubeToolStripButton->DisplayStyle = System::Windows::Forms::ToolStripItemDisplayStyle::Image;
			this->youtubeToolStripButton->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"youtubeToolStripButton.Image")));
			this->youtubeToolStripButton->ImageTransparentColor = System::Drawing::Color::Magenta;
			this->youtubeToolStripButton->Name = L"youtubeToolStripButton";
			this->youtubeToolStripButton->Size = System::Drawing::Size(23, 22);
			this->youtubeToolStripButton->Text = L"toolStripButton1";
			this->youtubeToolStripButton->Click += gcnew System::EventHandler(this, &MainForm::youtubeToolStripButton_Click);
			// 
			// picasaToolStripButton
			// 
			this->picasaToolStripButton->CheckOnClick = true;
			this->picasaToolStripButton->DisplayStyle = System::Windows::Forms::ToolStripItemDisplayStyle::Image;
			this->picasaToolStripButton->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"picasaToolStripButton.Image")));
			this->picasaToolStripButton->ImageScaling = System::Windows::Forms::ToolStripItemImageScaling::None;
			this->picasaToolStripButton->ImageTransparentColor = System::Drawing::Color::Magenta;
			this->picasaToolStripButton->Name = L"picasaToolStripButton";
			this->picasaToolStripButton->Size = System::Drawing::Size(23, 22);
			this->picasaToolStripButton->Text = L"Picasa";
			this->picasaToolStripButton->Click += gcnew System::EventHandler(this, &MainForm::picasaToolStripButton_Click);
			// 
			// imageSearchToolStripButton
			// 
			this->imageSearchToolStripButton->CheckOnClick = true;
			this->imageSearchToolStripButton->DisplayStyle = System::Windows::Forms::ToolStripItemDisplayStyle::Image;
			this->imageSearchToolStripButton->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"imageSearchToolStripButton.Image")));
			this->imageSearchToolStripButton->ImageScaling = System::Windows::Forms::ToolStripItemImageScaling::None;
			this->imageSearchToolStripButton->ImageTransparentColor = System::Drawing::Color::Magenta;
			this->imageSearchToolStripButton->Name = L"imageSearchToolStripButton";
			this->imageSearchToolStripButton->Size = System::Drawing::Size(23, 22);
			this->imageSearchToolStripButton->Text = L"Search";
			this->imageSearchToolStripButton->Click += gcnew System::EventHandler(this, &MainForm::imageSearchToolStripButton_Click);
			// 
			// toolStripSeparator1
			// 
			this->toolStripSeparator1->Name = L"toolStripSeparator1";
			this->toolStripSeparator1->Size = System::Drawing::Size(6, 25);
			// 
			// saveImageToolStripButton
			// 
			this->saveImageToolStripButton->DisplayStyle = System::Windows::Forms::ToolStripItemDisplayStyle::Image;
			this->saveImageToolStripButton->Enabled = false;
			this->saveImageToolStripButton->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"saveImageToolStripButton.Image")));
			this->saveImageToolStripButton->ImageTransparentColor = System::Drawing::Color::Magenta;
			this->saveImageToolStripButton->Name = L"saveImageToolStripButton";
			this->saveImageToolStripButton->Size = System::Drawing::Size(23, 22);
			this->saveImageToolStripButton->Text = L"Save Image";
			this->saveImageToolStripButton->Click += gcnew System::EventHandler(this, &MainForm::saveImageToolStripButton_Click);
			// 
			// resizeImageToolStripButton
			// 
			this->resizeImageToolStripButton->DisplayStyle = System::Windows::Forms::ToolStripItemDisplayStyle::Image;
			this->resizeImageToolStripButton->Enabled = false;
			this->resizeImageToolStripButton->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"resizeImageToolStripButton.Image")));
			this->resizeImageToolStripButton->ImageTransparentColor = System::Drawing::Color::Magenta;
			this->resizeImageToolStripButton->Name = L"resizeImageToolStripButton";
			this->resizeImageToolStripButton->Size = System::Drawing::Size(23, 22);
			this->resizeImageToolStripButton->Text = L"Resize Image";
			this->resizeImageToolStripButton->Click += gcnew System::EventHandler(this, &MainForm::resizeImageToolStripButton_Click);
			// 
			// cropImageToolStripButton
			// 
			this->cropImageToolStripButton->DisplayStyle = System::Windows::Forms::ToolStripItemDisplayStyle::Image;
			this->cropImageToolStripButton->Enabled = false;
			this->cropImageToolStripButton->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"cropImageToolStripButton.Image")));
			this->cropImageToolStripButton->ImageTransparentColor = System::Drawing::Color::Magenta;
			this->cropImageToolStripButton->Name = L"cropImageToolStripButton";
			this->cropImageToolStripButton->Size = System::Drawing::Size(23, 22);
			this->cropImageToolStripButton->Text = L"Crop Image";
			this->cropImageToolStripButton->Click += gcnew System::EventHandler(this, &MainForm::cropImageToolStripButton_Click);
			// 
			// verticalMirrorImageToolStripButton
			// 
			this->verticalMirrorImageToolStripButton->DisplayStyle = System::Windows::Forms::ToolStripItemDisplayStyle::Image;
			this->verticalMirrorImageToolStripButton->Enabled = false;
			this->verticalMirrorImageToolStripButton->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"verticalMirrorImageToolStripButton.Image")));
			this->verticalMirrorImageToolStripButton->ImageTransparentColor = System::Drawing::Color::Magenta;
			this->verticalMirrorImageToolStripButton->Name = L"verticalMirrorImageToolStripButton";
			this->verticalMirrorImageToolStripButton->Size = System::Drawing::Size(23, 22);
			this->verticalMirrorImageToolStripButton->Text = L"Mirror Image Vertically";
			this->verticalMirrorImageToolStripButton->Click += gcnew System::EventHandler(this, &MainForm::verticalMirrorImageToolStripButton_Click);
			// 
			// horizontalMirrorImageToolStripButton
			// 
			this->horizontalMirrorImageToolStripButton->DisplayStyle = System::Windows::Forms::ToolStripItemDisplayStyle::Image;
			this->horizontalMirrorImageToolStripButton->Enabled = false;
			this->horizontalMirrorImageToolStripButton->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"horizontalMirrorImageToolStripButton.Image")));
			this->horizontalMirrorImageToolStripButton->ImageTransparentColor = System::Drawing::Color::Magenta;
			this->horizontalMirrorImageToolStripButton->Name = L"horizontalMirrorImageToolStripButton";
			this->horizontalMirrorImageToolStripButton->Size = System::Drawing::Size(23, 22);
			this->horizontalMirrorImageToolStripButton->Text = L"Mirror Image Horizontally";
			this->horizontalMirrorImageToolStripButton->Click += gcnew System::EventHandler(this, &MainForm::horizontalMirrorImageToolStripButton_Click);
			// 
			// rotateImageMinToolStripButton
			// 
			this->rotateImageMinToolStripButton->DisplayStyle = System::Windows::Forms::ToolStripItemDisplayStyle::Image;
			this->rotateImageMinToolStripButton->Enabled = false;
			this->rotateImageMinToolStripButton->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"rotateImageMinToolStripButton.Image")));
			this->rotateImageMinToolStripButton->ImageTransparentColor = System::Drawing::Color::Magenta;
			this->rotateImageMinToolStripButton->Name = L"rotateImageMinToolStripButton";
			this->rotateImageMinToolStripButton->Size = System::Drawing::Size(23, 22);
			this->rotateImageMinToolStripButton->Text = L"Rotate Image -90 Degrees";
			this->rotateImageMinToolStripButton->Click += gcnew System::EventHandler(this, &MainForm::rotateImageMinToolStripButton_Click);
			// 
			// rotateImagePlusToolStripButton
			// 
			this->rotateImagePlusToolStripButton->DisplayStyle = System::Windows::Forms::ToolStripItemDisplayStyle::Image;
			this->rotateImagePlusToolStripButton->Enabled = false;
			this->rotateImagePlusToolStripButton->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"rotateImagePlusToolStripButton.Image")));
			this->rotateImagePlusToolStripButton->ImageTransparentColor = System::Drawing::Color::Magenta;
			this->rotateImagePlusToolStripButton->Name = L"rotateImagePlusToolStripButton";
			this->rotateImagePlusToolStripButton->Size = System::Drawing::Size(23, 22);
			this->rotateImagePlusToolStripButton->Text = L"Rotate Image 90 Degrees";
			this->rotateImagePlusToolStripButton->Click += gcnew System::EventHandler(this, &MainForm::rotateImagePlusToolStripButton_Click);
			// 
			// toolStripSeparator4
			// 
			this->toolStripSeparator4->Name = L"toolStripSeparator4";
			this->toolStripSeparator4->Size = System::Drawing::Size(6, 25);
			// 
			// logToolStripMenuItem
			// 
			this->logToolStripMenuItem->Name = L"logToolStripMenuItem";
			this->logToolStripMenuItem->Size = System::Drawing::Size(154, 26);
			this->logToolStripMenuItem->Text = L"Log";
			this->logToolStripMenuItem->Click += gcnew System::EventHandler(this, &MainForm::logToolStripMenuItem_Click);
			// 
			// MainForm
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(668, 537);
			this->Controls->Add(this->toolStripContainer1);
			this->Controls->Add(this->toolStrip1);
			this->Controls->Add(this->menuStrip1);
			this->Icon = (cli::safe_cast<System::Drawing::Icon^  >(resources->GetObject(L"$this.Icon")));
			this->MainMenuStrip = this->menuStrip1;
			this->Name = L"MainForm";
			this->Text = L"MainForm";
			this->WindowState = System::Windows::Forms::FormWindowState::Maximized;
			this->FormClosing += gcnew System::Windows::Forms::FormClosingEventHandler(this, &MainForm::mainForm_FormClosing);
			this->menuStrip1->ResumeLayout(false);
			this->menuStrip1->PerformLayout();
			this->toolStripContainer1->ContentPanel->ResumeLayout(false);
			this->toolStripContainer1->ResumeLayout(false);
			this->toolStripContainer1->PerformLayout();
			this->toolStrip1->ResumeLayout(false);
			this->toolStrip1->PerformLayout();
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion

	static log4net::ILog ^log = log4net::LogManager::GetLogger(System::Reflection::MethodBase::GetCurrentMethod()->DeclaringType);

	private: System::Void exitToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

				 Close();
			 }
	private: System::Void aboutToolStripMenuItem1_Click(System::Object^  sender, System::EventArgs^  e) {

				 About ^aboutForm = gcnew About();	
				 aboutForm->Show();
			 }
	private: System::Void openToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

				 OpenFileDialog ^openFileDialog = ImageUtils::createOpenImageFileDialog();

				 if(openFileDialog->ShowDialog() == ::DialogResult::OK)
					{
						loadMedia(openFileDialog->FileName);						
					}
			 }
	private: System::Void openURLToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

				 String ^currentURL = L"";

				 if(imagePanel->IsWebImage) {

					 currentURL = imagePanel->ImageLocation;
				 }

				 InputDialog ^urlDialog = gcnew InputDialog();
				 urlDialog->Text = "Open URL";
				 if(urlDialog->ShowDialog() == ::DialogResult::OK) {

					 loadMedia(urlDialog->inputText);
				 }
			 }
	private: System::Void imageSearchToolStripButton_Click(System::Object^  sender, System::EventArgs^  e) {

				 if(imageSearch != nullptr) {

					 imageSearch->WindowState = FormWindowState::Normal;
					 imageSearch->BringToFront();
					 imageSearchToolStripButton->Checked = true;

				 } else {

					 imageSearch = gcnew ImageSearchForm();
					 imageSearch->OnViewImage += gcnew ImageSearchForm::viewImageEventHandler(this, &MainForm::imageSearch_ViewImageEvent);
					 imageSearch->OnDownload += gcnew ImageSearchForm::downloadEventHandler(this, &MainForm::imageSearch_DownloadEvent);
					 imageSearch->FormClosed += gcnew System::Windows::Forms::FormClosedEventHandler(this, &MainForm::imageSearch_FormClosedEvent);
					 imageSearch->Show();
				 }
			 }
	private: System::Void imageSearch_ViewImageEvent(System::Object^  sender, ImageGridMouseEventArgs^  e) {

				 IImageResult ^imageInfo = dynamic_cast<IImageResult ^>(e->item->Data);

				 this->loadMedia(imageInfo->Url);

				 showImagePanel();

			 }
	private: System::Void imageSearch_DownloadEvent(System::Object^  sender, List<ImageGridItem ^> ^items) {

				 DownloadProgressForm ^progressForm = gcnew DownloadProgressForm();

				 List<String ^> ^url = gcnew List<String ^>();

				 for(int i = 0; i < items->Count; i++) {

					 IImageResult ^imageInfo = dynamic_cast<IImageResult ^>(items[i]->Data);

					 url->Add(imageInfo->Url);
				 }

				 progressForm->Show();
				 progressForm->Text = "Download Google Search Images";
				 progressForm->download(url, imageFileBrowser->getBrowsePath());

			 }
	private: System::Void imageSearch_FormClosedEvent(System::Object^  sender, FormClosedEventArgs ^e) {

				 imageSearch = nullptr;
				 imageSearchToolStripButton->Checked = false;

			 }
	private: System::Void picasaToolStripButton_Click(System::Object^  sender, System::EventArgs^  e) {

				 if(picasa != nullptr) {

					 picasa->WindowState = FormWindowState::Normal;
					 picasa->BringToFront();
					 picasaToolStripButton->Checked = true;

				 } else {

					 picasa = gcnew PicasaForm();					 
					 picasa->OnViewEvent += gcnew PicasaForm::ViewEventHandler(this, &MainForm::picasaForm_ViewEvent);
					 picasa->OnDownloadEvent += gcnew PicasaForm::DownloadEventHandler(this, &MainForm::picasaForm_DownloadEvent);
					 picasa->OnUploadEvent += gcnew PicasaForm::UploadEventHandler(this, &MainForm::picasaForm_UploadEvent);
					 picasa->FormClosed += gcnew System::Windows::Forms::FormClosedEventHandler(this, &MainForm::picasaForm_FormClosedEvent);
					 picasa->Show();
				 }
			 }

	private: System::Void picasaForm_DownloadEvent(System::Object^  sender, List<ImageGridItem ^> ^items) {

				 DownloadProgressForm ^progressForm = gcnew DownloadProgressForm();

				 List<String ^> ^url = gcnew List<String ^>();

				 for(int i = 0; i < items->Count; i++) {

					 Photo ^photo = dynamic_cast<Photo ^>(items[i]->Data);

					 url->Add(getFullSizePicasaPhotoUrl(photo->PhotoUri->AbsoluteUri));
				 }

				 progressForm->Show();
				 progressForm->Text = "Download Picasa Images";
				 progressForm->download(url, imageFileBrowser->getBrowsePath());

			 }

    private: System::Void picasaForm_UploadEvent(System::Object^  sender, EventArgs ^e) {

				 List<ImageStream ^> ^imageStream = getSelectedImages();
				 
				 picasa->uploadImages(imageStream);

			 }
	private: System::Void picasaForm_FormClosedEvent(System::Object^  sender, FormClosedEventArgs ^e) {

				 picasa = nullptr;
				 picasaToolStripButton->Checked = false;

			 }


	private: System::Void loadMedia(String ^location) {

				 try {

					 clearTitle();

					 imagePanelButtonsEnabled(false);

					 if(MediaFormatConvert::isImageFile(location)) {

						 showImagePanel();
						 imagePanel->loadImage(location);

					 } else if(MediaFormatConvert::isVideoFile(location)) {

						 showVideoPanel();
						 videoPanel->open(location);
						 videoPanel->play();
						 setTitle();

					 } else if(Util::isUrl(location)) {

						 if(MessageBox::Show("Unknown media type, open as video?", "Warning",
							 MessageBoxButtons::OKCancel,MessageBoxIcon::Warning) == 
							 System::Windows::Forms::DialogResult::OK) 
						 {
							 showVideoPanel();
							 videoPanel->open(location);
							 videoPanel->play();
							 setTitle();
						 }

					 }

				 } catch(Exception ^e) {

					 log->Error("error loading media:" + location, e);
					 MessageBox::Show(e->Message);
				 }
			 }

	private: System::Void picasaForm_ViewEvent(System::Object^  sender, ImageGridMouseEventArgs^  e) {

				 Photo ^photo = dynamic_cast<Photo ^>(e->item->Data);

				 this->loadMedia(getFullSizePicasaPhotoUrl(photo->PhotoUri->AbsoluteUri));

				 showImagePanel();

			 }
	private: String ^getFullSizePicasaPhotoUrl(String ^url) {

				 String ^fUrl = url;
				 
				 fUrl = fUrl->Insert(url->LastIndexOf('/') + 1, "d/");

				 return(fUrl);
			 }
	private: System::Void imageFileBrowserToolStripButton_Click(System::Object^  sender, System::EventArgs^  e) {

				 showImageFileBrowser();

			 }

	private: System::Void imageFileBrowser_CurrentMediaChanged(System::Object^  sender, FileSystemEventArgs^  e) {

				 if(e->ChangeType == WatcherChangeTypes::Deleted) {
					
					 if(!imagePanel->IsModified) {
						 imagePanel->loadImage("");
					 }
				 
				 } else {

					loadMedia(e->FullPath);
				 }

			 }
	private: System::Void imageToolStripButton_Click(System::Object^  sender, System::EventArgs^  e) {

				 showImagePanel();

			 }

	private: System::Void mainForm_MouseWheel(System::Object^  sender, MouseEventArgs^  e) {

				 if(e->Delta < 0) {

					 if(imagePanel->Visible) {

						imageFileBrowser->setNextImageFile();
					 } else {

						 imageFileBrowser->imageGrid->displayNextPage();
					 }

				 } else {

					 if(imagePanel->Visible) {

						 imageFileBrowser->setPrevImageFile();
					 } else {

						 imageFileBrowser->imageGrid->displayPrevPage();
					 }					 
				 }
			 }
	private: System::Void imageFileBrowser_MouseDown(System::Object^  sender, ImageGridMouseEventArgs^  e) {

				 if(e->Button == Windows::Forms::MouseButtons::Left) {
					 imageFileBrowser->imageGrid->toggleImageSelected(e->imageNr);
				 }

			 }
    private: void showVideoPanel() {

				 imageFileBrowser->Visible = false;
				 videoPanel->Visible = true;
				 imagePanel->Visible = false;				 
				 ActiveControl = videoPanel;

				 imageToolStripButton->Checked = false;
				 videoToolStripButton->Checked = true;
				 imageFileBrowserToolStripButton->Checked = false;
				 imagePanelButtonsEnabled(false);
				 
				 setTitle();

			 }
	private: void showImagePanel() {

				 imageFileBrowser->Visible = false;
				 videoPanel->Visible = false;
				 imagePanel->Visible = true;				 
				 ActiveControl = imagePanel;

				 imageToolStripButton->Checked = true;
				 videoToolStripButton->Checked = false;
				 imageFileBrowserToolStripButton->Checked = false;

				 if(!imagePanel->IsEmpty) {

					 imagePanelButtonsEnabled(true);
				 }

				 setTitle();

			 }

	private: void imagePanelButtonsEnabled(bool state) {

				 this->saveImageToolStripButton->Enabled = state;
				 this->resizeImageToolStripButton->Enabled = state; 
				 this->cropImageToolStripButton->Enabled = state;
				 this->verticalMirrorImageToolStripButton->Enabled = state;
				 this->horizontalMirrorImageToolStripButton->Enabled = state;
				 this->rotateImageMinToolStripButton->Enabled = state;
				 this->rotateImagePlusToolStripButton->Enabled = state;
			 }


	private: void showImageFileBrowser() {

				 imagePanel->Visible = false;
				 videoPanel->Visible = false;
				 imageFileBrowser->Visible = true;	
				 ActiveControl = imageFileBrowser;

				 imageToolStripButton->Checked = false;
				 videoToolStripButton->Checked = false;
				 imageFileBrowserToolStripButton->Checked = true;

				 imagePanelButtonsEnabled(false);

				 setTitle();
			 }
	private: void clearTitle() {

				 this->Text = "Image Viewer v1.0";
			 }
	private: void setTitle() {

				 String ^title = L"";

				 if(videoPanel->Visible == true) {

					 title = videoPanel->VideoLocation;

				 } else if(imagePanel->Visible == true) {

					title = imagePanel->ImageLocation;
					if(imagePanel->IsModified) {

						title += " * "; 
					}
					
				 } else if(imageFileBrowser->Visible == true) {

					 title = imageFileBrowser->getBrowsePath();
				 }

				 if(!title->Equals("")) {

					 title += L" - ";
				 }

				 title += "Image Viewer v1.0";

				 this->Text = title;
			 }
	private: System::Void verticalMirrorImageToolStripButton_Click(System::Object^  sender, System::EventArgs^  e) {

				 imagePanel->rotateFlipImage(RotateFlipType::RotateNoneFlipX);
			 }
	private: System::Void horizontalMirrorImageToolStripButton_Click(System::Object^  sender, System::EventArgs^  e) {

				 imagePanel->rotateFlipImage(RotateFlipType::RotateNoneFlipY);
			 }
	private: System::Void rotateImageMinToolStripButton_Click(System::Object^  sender, System::EventArgs^  e) {

				 imagePanel->rotateFlipImage(RotateFlipType::Rotate270FlipNone);

			 }
	private: System::Void rotateImagePlusToolStripButton_Click(System::Object^  sender, System::EventArgs^  e) {

				 imagePanel->rotateFlipImage(RotateFlipType::Rotate90FlipNone);
			 }
	private: System::Void imagePanel_LoadImageFinished(System::Object^  sender, System::EventArgs^  e) {

				 if(imagePanel->Visible == true) {

					 imagePanelButtonsEnabled(true);
				 }

				 if(!imagePanel->IsWebImage) {

					 imageFileBrowser->setBrowsePath(imagePanel->ImageLocation);
				 }

				 setTitle();
			 }
	private: System::Void imagePanel_Modified(System::Object^  sender, GEventArgs<bool> ^e) {

				 setTitle();
			 }
	private: System::Void saveImageToolStripButton_Click(System::Object^  sender, System::EventArgs^  e) {

				 SaveFileDialog ^saveFileDialog = gcnew SaveFileDialog();

				 saveFileDialog->Filter = L"JPEG File (*.jpg)|*.jpg|"
					 L"PNG File (*.png)|*.png|"	                  
					 L"GIF File (*.gif)|*.gif|"
					 L"TIFF File (*.tif)|*.tif|"
					 L"BMP File (*.bmp)|*.bmp";

				 String ^ext = System::IO::Path::GetExtension(imagePanel->ImageLocation);
				 
				 if(ext->Equals(".png")) {

					saveFileDialog->FilterIndex = 2;

				 } else if(ext->Equals(".gif")) {

				    saveFileDialog->FilterIndex = 3;

				 } else if(ext->Equals(".tif")) {

					saveFileDialog->FilterIndex = 4;

				 } else if(ext->Equals(".bmp")) {

				    saveFileDialog->FilterIndex = 5;

				 } else {

					saveFileDialog->FilterIndex = 1;
				 }

				 String ^browsePath = imageFileBrowser->getBrowsePath();

				 if(!String::IsNullOrEmpty(browsePath)) {

					saveFileDialog->InitialDirectory = browsePath;

				 } else if(!imagePanel->IsWebImage) {

					 saveFileDialog->InitialDirectory = Path::GetDirectoryName(imagePanel->ImageLocation);

				 }

				 saveFileDialog->FileName = Path::GetFileName(imagePanel->ImageLocation);

				 if(saveFileDialog->ShowDialog() == ::DialogResult::OK)
					{
						try {

							imagePanel->saveImageToDisk(saveFileDialog->FileName);
							loadMedia(saveFileDialog->FileName);

						} catch(Exception ^e) {

							log->Error("error saving image:" + saveFileDialog->FileName, e);
							MessageBox::Show(e->Message);
						}
					}
			 }
	private: System::Void resizeImageToolStripButton_Click(System::Object^  sender, System::EventArgs^  e) {

				 ResizeForm	^resizeForm = gcnew ResizeForm(imagePanel->ImageSize);
				 resizeForm->OnResize += gcnew ResizeForm::ResizeUpdateHandler(this, &MainForm::resizeForm_Resize);
				 resizeForm->Show();
			 }

	private: System::Void resizeForm_Resize(System::Object^  sender, ResizeUpdateEventArgs^  e) {

				 imagePanel->resizeImage(e->width, e->height);
				 //this->Refresh();	
			 }
	private: System::Void cropImageToolStripButton_Click(System::Object^  sender, System::EventArgs^  e) {

				 imagePanel->CropStage = ImagePanelControl::CropStageState::ENABLED;
			 }

	private: List<ImageStream ^> ^getSelectedImages() {

				 List<ImageStream ^> ^images = gcnew List<ImageStream ^>();

				 if(imagePanel->Visible == true) {

					 images->Add( imagePanel->saveImageToImageStream() );

				 } else {

					 List<ImageGridItem ^> ^selected = imageFileBrowser->imageGrid->getSelectedImageData();

					 for(int i = 0; i < selected->Count; i++) {

						 String ^path = selected[i]->ImageLocation;

						 Stream ^data = File::OpenRead(path);
						 String ^mimeType = MediaFormatConvert::fileNameToMimeType(path);
						 String ^name = System::IO::Path::GetFileName(path);

						 ImageStream ^imageStream = gcnew ImageStream(name, mimeType, data);

						 images->Add(imageStream);
					 }

				 }

				 return(images);
			 }

	private: System::Void freefap() {

				 try {

					 UploadImage ^uploadImage = gcnew UploadImage();

					 List<ImageStream ^> ^imageStreams = getSelectedImages();

					 List<HttpWebResponse ^> ^response = uploadImage->freefap(imageStreams);

					 for(int i = 0; i < response->Count; i++) {

						 String ^text = L"Freefap: ";

						 UploadOutputForm ^output = gcnew UploadOutputForm(response[i], text);
						 output->Show();
					 }


				 } catch(Exception ^e) {

					 log->Error("Error uploading image(s) to freefap", e);
					 MessageBox::Show(e->Message);
				 }

			 }
	private: System::Void dumppix() {

				 try {

					 UploadImage ^uploadImage = gcnew UploadImage();

					 List<ImageStream ^> ^imageStreams = getSelectedImages();

					 List<HttpWebResponse ^> ^response = uploadImage->dumppix(imageStreams);

					 for(int i = 0; i < response->Count; i++) {

						 String ^text = L"Dumppix: ";

						 UploadOutputForm ^output = gcnew UploadOutputForm(response[i], text);
						 output->Show();
					 }


				 } catch(Exception ^e) {

					 log->Error("Error uploading image(s) to dumppix", e);
					 MessageBox::Show(e->Message);
				 }

			 }
	
	
private: System::Void imageFileBrowser_ChangeBrowseDirectoryEvent(System::Object^  sender, System::EventArgs^  e) {

			 setTitle();
		 }

private: System::Void imageFileBrowser_ViewEvent(System::Object^  sender, imageviewer::ImageGridMouseEventArgs^  e) {

			 loadMedia(e->item->ImageLocation);

		 }

private: System::Void autoScaleToolStripButton_Click(System::Object^  sender, System::EventArgs^  e) {

			 if(autoScaleToolStripButton->Checked) {

				imagePanel->DisplayMode = ImagePanelControl::DisplayModeState::SCALED;

			 } else {

				imagePanel->DisplayMode = ImagePanelControl::DisplayModeState::NORMAL;
			 }
		 }
private: System::Void imagePanel_PreviewKeyDown(System::Object^  sender, System::Windows::Forms::PreviewKeyDownEventArgs^  e) {

			 if(imagePanel->Visible == false) return;

			 if(e->KeyCode == Keys::PageDown) {

				 imageFileBrowser->setNextImageFile();

			 } else if(e->KeyCode == Keys::PageUp) {

				 imageFileBrowser->setPrevImageFile();

			 } else if(e->KeyCode == Keys::Add) {

				 imagePanel->zoomImage(1.5f);

			 } else if(e->KeyCode == Keys::Subtract) {

				 imagePanel->zoomImage(0.667f);

			 } else if(e->KeyCode == Keys::Space) {

				 autoScaleToolStripButton->PerformClick();

			 }  else if(e->KeyCode == Keys::F) {

				freefap();

			 } else if(e->KeyCode == Keys::D) {

				dumppix();

			 } else if(e->KeyCode == Keys::Delete) {

					 //deleteImages();
			 }
		 }
private: System::Void videoToolStripButton_Click(System::Object^  sender, System::EventArgs^  e) {

			showVideoPanel();
		 }

public: System::Void application_Idle(System::Object^  sender, System::EventArgs^  e) {

			//Util::DebugOut("idle");
		 }
private: System::Void mainForm_FormClosing(System::Object^  sender, System::Windows::Forms::FormClosingEventArgs^  e) {

			 videoPanel->close();
			 Settings::save();
			 log->Info("Closing Application");
		 }

private: System::Void settingsToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

		 }

private: System::Void youtubeToolStripButton_Click(System::Object^  sender, System::EventArgs^  e) {

			 YoutubeForm ^youtube = gcnew YoutubeForm();
			 youtube->Show();
		 }
private: System::Void logToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {
		 }
};
}
