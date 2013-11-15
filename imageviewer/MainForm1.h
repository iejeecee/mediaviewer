#pragma once
#include "About.h"
#include "ImagePanel.h"
#include "ResizeForm.h"
#include "UploadImage.h"
#include "UploadOutputForm.h"
#include "OpenURLForm.h"
#include "ImageSearchForm.h"
#include "PicasaForm.h"
#include "Util.h"
#include "ImageFileWatcher.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;
using namespace System::IO;

namespace imageviewer {

	/// <summary>
	/// Summary for MainForm1
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class MainForm1 : public System::Windows::Forms::Form
	{

	private: ImagePanel ^imagePanel;
	private: About^ aboutForm;
	private: ImageFileWatcher ^imageFileWatcher;
	private: OpenFileDialog^ openFileDialog;
	private: SaveFileDialog^ saveFileDialog;
	private: System::Windows::Forms::ToolStripMenuItem^  imageToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  resizeToolStripMenuItem;
	private: ResizeForm^ resizeForm;
	private: System::Windows::Forms::ToolStripMenuItem^  saveToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  flipXToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  flipYToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  rotate90ToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  rotate90ToolStripMenuItem1;
	private: System::Windows::Forms::ToolStripMenuItem^  deleteToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  uploadToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  freefapToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  dumppixToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  openURLToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  cropToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  searchToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  picasaToolStripMenuItem;













	private: String^ currentImagePath;
	private: String^ filesGridPath;
	private: System::Windows::Forms::ToolStripContainer^  toolStripContainer1;
	private: System::Windows::Forms::ToolStrip^  toolStrip1;
	private: System::Windows::Forms::ToolStripButton^  toolStripButton1;







	private: int filesImageGridCurrentPage;

	public:

		MainForm1(array<System::String ^> ^args)
		{
			InitializeComponent();

			//imagePanel->OnModified += gcnew ImagePanel::ModifiedEventHandler(this, &MainForm1::imageModified_Event);
			//imageTabPage->MouseWheel += gcnew MouseEventHandler(this, &MainForm1::imagePanelMouseWheel_Event);
			//filesTabPage->MouseWheel += gcnew MouseEventHandler(this, &MainForm1::filesGridMouseWheel_Event);

			//filesImageGrid->setNrImages(4 * 4, true);

			this->WindowState = FormWindowState::Maximized;
			this->Show();
			this->MinimumSize = Size;
			this->MaximumSize = Size;

			this->currentImagePath = nullptr;
			this->filesGridPath = L"";

			//
			//TODO: Add the constructor code here
			//		

			openFileDialog = gcnew OpenFileDialog;

			openFileDialog->Filter = L"Image Files|*.tif;*.jpg;*.jpeg;*.gif;*.png;*.bmp|"
				L"JPEG Files (*.jpg)|*.jpg;*.jpeg|"
				L"PNG Files (*.png)|*.png|"	                  
				L"GIF Files (*.gif)|*.gif|"
				L"TIFF Files (*.tif)|*.tif|"
				L"BMP File (*.bmp)|*.bmp";

			openFileDialog->FilterIndex = 1;

			saveFileDialog = gcnew SaveFileDialog;

			saveFileDialog->Filter = L"JPEG File (*.jpg)|*.jpg|"
				L"PNG File (*.png)|*.png|"	                  
				L"GIF File (*.gif)|*.gif|"
				L"TIFF File (*.tif)|*.tif|"
				L"BMP File (*.bmp)|*.bmp";

			saveFileDialog->FilterIndex = 1;

			imageFileWatcher = gcnew ImageFileWatcher();

			if(args->Length != 0) {

				loadImage(args[0]);
			}

		}

		String ^getTitleText() {

			return(currentImagePath + L" - Image Viewer");
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~MainForm1()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::MenuStrip^  menuStrip1;
	private: System::Windows::Forms::ToolStripMenuItem^  fileToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  openToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  exitToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  helpToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  aboutToolStripMenuItem;



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
			System::ComponentModel::ComponentResourceManager^  resources = (gcnew System::ComponentModel::ComponentResourceManager(MainForm1::typeid));
			this->menuStrip1 = (gcnew System::Windows::Forms::MenuStrip());
			this->fileToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->openToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->openURLToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->picasaToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->searchToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->saveToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->deleteToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->exitToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->imageToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->cropToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->resizeToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->flipXToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->flipYToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->rotate90ToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->rotate90ToolStripMenuItem1 = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->uploadToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->freefapToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->dumppixToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->helpToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->aboutToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->toolStripContainer1 = (gcnew System::Windows::Forms::ToolStripContainer());
			this->toolStrip1 = (gcnew System::Windows::Forms::ToolStrip());
			this->toolStripButton1 = (gcnew System::Windows::Forms::ToolStripButton());
			this->menuStrip1->SuspendLayout();
			this->toolStripContainer1->TopToolStripPanel->SuspendLayout();
			this->toolStripContainer1->SuspendLayout();
			this->toolStrip1->SuspendLayout();
			this->SuspendLayout();
			// 
			// menuStrip1
			// 
			this->menuStrip1->Items->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(3) {this->fileToolStripMenuItem, 
				this->imageToolStripMenuItem, this->helpToolStripMenuItem});
			this->menuStrip1->Location = System::Drawing::Point(0, 0);
			this->menuStrip1->Name = L"menuStrip1";
			this->menuStrip1->Size = System::Drawing::Size(493, 29);
			this->menuStrip1->TabIndex = 0;
			this->menuStrip1->Text = L"menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this->fileToolStripMenuItem->DropDownItems->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(7) {this->openToolStripMenuItem, 
				this->openURLToolStripMenuItem, this->picasaToolStripMenuItem, this->searchToolStripMenuItem, this->saveToolStripMenuItem, this->deleteToolStripMenuItem, 
				this->exitToolStripMenuItem});
			this->fileToolStripMenuItem->Name = L"fileToolStripMenuItem";
			this->fileToolStripMenuItem->Size = System::Drawing::Size(48, 25);
			this->fileToolStripMenuItem->Text = L"File";
			// 
			// openToolStripMenuItem
			// 
			this->openToolStripMenuItem->Name = L"openToolStripMenuItem";
			this->openToolStripMenuItem->Size = System::Drawing::Size(165, 26);
			this->openToolStripMenuItem->Text = L"Open";
			this->openToolStripMenuItem->Click += gcnew System::EventHandler(this, &MainForm1::openFile);
			// 
			// openURLToolStripMenuItem
			// 
			this->openURLToolStripMenuItem->Name = L"openURLToolStripMenuItem";
			this->openURLToolStripMenuItem->Size = System::Drawing::Size(165, 26);
			this->openURLToolStripMenuItem->Text = L"Open URL";
			this->openURLToolStripMenuItem->Click += gcnew System::EventHandler(this, &MainForm1::openURLToolStripMenuItem_Click);
			// 
			// picasaToolStripMenuItem
			// 
			this->picasaToolStripMenuItem->Name = L"picasaToolStripMenuItem";
			this->picasaToolStripMenuItem->Size = System::Drawing::Size(165, 26);
			this->picasaToolStripMenuItem->Text = L"Picasa Web";
			this->picasaToolStripMenuItem->Click += gcnew System::EventHandler(this, &MainForm1::picasaToolStripMenuItem_Click);
			// 
			// searchToolStripMenuItem
			// 
			this->searchToolStripMenuItem->Name = L"searchToolStripMenuItem";
			this->searchToolStripMenuItem->Size = System::Drawing::Size(165, 26);
			this->searchToolStripMenuItem->Text = L"Search";
			this->searchToolStripMenuItem->Click += gcnew System::EventHandler(this, &MainForm1::searchToolStripMenuItem_Click);
			// 
			// saveToolStripMenuItem
			// 
			this->saveToolStripMenuItem->Enabled = false;
			this->saveToolStripMenuItem->Name = L"saveToolStripMenuItem";
			this->saveToolStripMenuItem->Size = System::Drawing::Size(165, 26);
			this->saveToolStripMenuItem->Text = L"Save";
			this->saveToolStripMenuItem->Click += gcnew System::EventHandler(this, &MainForm1::saveToolStripMenuItem_Click);
			// 
			// deleteToolStripMenuItem
			// 
			this->deleteToolStripMenuItem->Enabled = false;
			this->deleteToolStripMenuItem->Name = L"deleteToolStripMenuItem";
			this->deleteToolStripMenuItem->Size = System::Drawing::Size(165, 26);
			this->deleteToolStripMenuItem->Text = L"Delete";
			this->deleteToolStripMenuItem->Click += gcnew System::EventHandler(this, &MainForm1::deleteToolStripMenuItem_Click);
			// 
			// exitToolStripMenuItem
			// 
			this->exitToolStripMenuItem->Name = L"exitToolStripMenuItem";
			this->exitToolStripMenuItem->Size = System::Drawing::Size(165, 26);
			this->exitToolStripMenuItem->Text = L"Exit";
			this->exitToolStripMenuItem->Click += gcnew System::EventHandler(this, &MainForm1::exitApplication);
			// 
			// imageToolStripMenuItem
			// 
			this->imageToolStripMenuItem->DropDownItems->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(7) {this->cropToolStripMenuItem, 
				this->resizeToolStripMenuItem, this->flipXToolStripMenuItem, this->flipYToolStripMenuItem, this->rotate90ToolStripMenuItem, this->rotate90ToolStripMenuItem1, 
				this->uploadToolStripMenuItem});
			this->imageToolStripMenuItem->Name = L"imageToolStripMenuItem";
			this->imageToolStripMenuItem->Size = System::Drawing::Size(69, 25);
			this->imageToolStripMenuItem->Text = L"Image";
			// 
			// cropToolStripMenuItem
			// 
			this->cropToolStripMenuItem->Enabled = false;
			this->cropToolStripMenuItem->Name = L"cropToolStripMenuItem";
			this->cropToolStripMenuItem->Size = System::Drawing::Size(167, 26);
			this->cropToolStripMenuItem->Text = L"Crop";
			this->cropToolStripMenuItem->Click += gcnew System::EventHandler(this, &MainForm1::cropToolStripMenuItem_Click);
			// 
			// resizeToolStripMenuItem
			// 
			this->resizeToolStripMenuItem->Enabled = false;
			this->resizeToolStripMenuItem->Name = L"resizeToolStripMenuItem";
			this->resizeToolStripMenuItem->Size = System::Drawing::Size(167, 26);
			this->resizeToolStripMenuItem->Text = L"Resize";
			this->resizeToolStripMenuItem->Click += gcnew System::EventHandler(this, &MainForm1::resizeToolStripMenuItem_Click);
			// 
			// flipXToolStripMenuItem
			// 
			this->flipXToolStripMenuItem->Enabled = false;
			this->flipXToolStripMenuItem->Name = L"flipXToolStripMenuItem";
			this->flipXToolStripMenuItem->Size = System::Drawing::Size(167, 26);
			this->flipXToolStripMenuItem->Text = L"Flip X";
			this->flipXToolStripMenuItem->Click += gcnew System::EventHandler(this, &MainForm1::flipXToolStripMenuItem_Click);
			// 
			// flipYToolStripMenuItem
			// 
			this->flipYToolStripMenuItem->Enabled = false;
			this->flipYToolStripMenuItem->Name = L"flipYToolStripMenuItem";
			this->flipYToolStripMenuItem->Size = System::Drawing::Size(167, 26);
			this->flipYToolStripMenuItem->Text = L"Flip Y";
			this->flipYToolStripMenuItem->Click += gcnew System::EventHandler(this, &MainForm1::flipYToolStripMenuItem_Click);
			// 
			// rotate90ToolStripMenuItem
			// 
			this->rotate90ToolStripMenuItem->Enabled = false;
			this->rotate90ToolStripMenuItem->Name = L"rotate90ToolStripMenuItem";
			this->rotate90ToolStripMenuItem->Size = System::Drawing::Size(167, 26);
			this->rotate90ToolStripMenuItem->Text = L"Rotate 90°";
			this->rotate90ToolStripMenuItem->Click += gcnew System::EventHandler(this, &MainForm1::rotate90ToolStripMenuItem_Click);
			// 
			// rotate90ToolStripMenuItem1
			// 
			this->rotate90ToolStripMenuItem1->Enabled = false;
			this->rotate90ToolStripMenuItem1->Name = L"rotate90ToolStripMenuItem1";
			this->rotate90ToolStripMenuItem1->Size = System::Drawing::Size(167, 26);
			this->rotate90ToolStripMenuItem1->Text = L"Rotate -90°";
			this->rotate90ToolStripMenuItem1->Click += gcnew System::EventHandler(this, &MainForm1::rotate90ToolStripMenuItem1_Click);
			// 
			// uploadToolStripMenuItem
			// 
			this->uploadToolStripMenuItem->DropDownItems->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(2) {this->freefapToolStripMenuItem, 
				this->dumppixToolStripMenuItem});
			this->uploadToolStripMenuItem->Enabled = false;
			this->uploadToolStripMenuItem->Name = L"uploadToolStripMenuItem";
			this->uploadToolStripMenuItem->Size = System::Drawing::Size(167, 26);
			this->uploadToolStripMenuItem->Text = L"Upload";
			// 
			// freefapToolStripMenuItem
			// 
			this->freefapToolStripMenuItem->Name = L"freefapToolStripMenuItem";
			this->freefapToolStripMenuItem->Size = System::Drawing::Size(145, 26);
			this->freefapToolStripMenuItem->Text = L"Freefap";
			this->freefapToolStripMenuItem->Click += gcnew System::EventHandler(this, &MainForm1::freefapToolStripMenuItem_Click);
			// 
			// dumppixToolStripMenuItem
			// 
			this->dumppixToolStripMenuItem->Name = L"dumppixToolStripMenuItem";
			this->dumppixToolStripMenuItem->Size = System::Drawing::Size(145, 26);
			this->dumppixToolStripMenuItem->Text = L"Dumppix";
			this->dumppixToolStripMenuItem->Click += gcnew System::EventHandler(this, &MainForm1::dumppixToolStripMenuItem_Click);
			// 
			// helpToolStripMenuItem
			// 
			this->helpToolStripMenuItem->DropDownItems->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(1) {this->aboutToolStripMenuItem});
			this->helpToolStripMenuItem->Name = L"helpToolStripMenuItem";
			this->helpToolStripMenuItem->Size = System::Drawing::Size(55, 25);
			this->helpToolStripMenuItem->Text = L"Help";
			// 
			// aboutToolStripMenuItem
			// 
			this->aboutToolStripMenuItem->Name = L"aboutToolStripMenuItem";
			this->aboutToolStripMenuItem->Size = System::Drawing::Size(124, 26);
			this->aboutToolStripMenuItem->Text = L"About";
			this->aboutToolStripMenuItem->Click += gcnew System::EventHandler(this, &MainForm1::showAboutForm);
			// 
			// toolStripContainer1
			// 
			// 
			// toolStripContainer1.ContentPanel
			// 
			this->toolStripContainer1->ContentPanel->Size = System::Drawing::Size(493, 352);
			this->toolStripContainer1->Dock = System::Windows::Forms::DockStyle::Fill;
			this->toolStripContainer1->Location = System::Drawing::Point(0, 29);
			this->toolStripContainer1->Name = L"toolStripContainer1";
			this->toolStripContainer1->Size = System::Drawing::Size(493, 377);
			this->toolStripContainer1->TabIndex = 1;
			this->toolStripContainer1->Text = L"toolStripContainer1";
			// 
			// toolStripContainer1.TopToolStripPanel
			// 
			this->toolStripContainer1->TopToolStripPanel->Controls->Add(this->toolStrip1);
			// 
			// toolStrip1
			// 
			this->toolStrip1->Dock = System::Windows::Forms::DockStyle::None;
			this->toolStrip1->Items->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(1) {this->toolStripButton1});
			this->toolStrip1->Location = System::Drawing::Point(3, 0);
			this->toolStrip1->Name = L"toolStrip1";
			this->toolStrip1->Size = System::Drawing::Size(64, 25);
			this->toolStrip1->TabIndex = 0;
			// 
			// toolStripButton1
			// 
			this->toolStripButton1->DisplayStyle = System::Windows::Forms::ToolStripItemDisplayStyle::Image;
			this->toolStripButton1->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"toolStripButton1.Image")));
			this->toolStripButton1->ImageTransparentColor = System::Drawing::Color::Magenta;
			this->toolStripButton1->Name = L"toolStripButton1";
			this->toolStripButton1->Size = System::Drawing::Size(23, 22);
			this->toolStripButton1->Text = L"toolStripButton1";
			// 
			// MainForm1
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->AutoScroll = true;
			this->ClientSize = System::Drawing::Size(493, 406);
			this->Controls->Add(this->toolStripContainer1);
			this->Controls->Add(this->menuStrip1);
			this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::FixedSingle;
			this->MainMenuStrip = this->menuStrip1;
			this->Name = L"MainForm1";
			this->Text = L"Image Viewer";
			this->WindowState = System::Windows::Forms::FormWindowState::Maximized;
			this->menuStrip1->ResumeLayout(false);
			this->menuStrip1->PerformLayout();
			this->toolStripContainer1->TopToolStripPanel->ResumeLayout(false);
			this->toolStripContainer1->TopToolStripPanel->PerformLayout();
			this->toolStripContainer1->ResumeLayout(false);
			this->toolStripContainer1->PerformLayout();
			this->toolStrip1->ResumeLayout(false);
			this->toolStrip1->PerformLayout();
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion
	private: System::Void openFile(System::Object^  sender, System::EventArgs^  e) {

				 if(openFileDialog->ShowDialog() == ::DialogResult::OK)
					{
						loadImage(openFileDialog->FileName);						
					}
			 }

	private: System::Void exitApplication(System::Object^  sender, System::EventArgs^  e) {

				 Close();
			 }

	private: System::Void showAboutForm(System::Object^  sender, System::EventArgs^  e) {
				 aboutForm = gcnew About();	
				 aboutForm->Show();
			 }
	private: System::Void imagePanelKeyDownHandler(System::Object^  sender, System::Windows::Forms::KeyEventArgs^  e) {

				 if(e->KeyCode == Keys::Right) {

					 loadImage(imageFileWatcher->getNextImageFile());

				 } else if(e->KeyCode == Keys::Left) {

					 loadImage(imageFileWatcher->getPrevImageFile());

				 } else if(e->KeyCode == Keys::Add) {

					 imagePanel->zoomImage(1.5f);

				 } else if(e->KeyCode == Keys::Subtract) {

					 imagePanel->zoomImage(0.667f);

				 } else if(e->KeyCode == Keys::Delete) {

					 deleteFile();
				 }
			 }

	private: System::Void loadImage(String ^fileName) {

				 try {

					 currentImagePath = fileName;

					 imagePanel->loadImage(fileName, false);
					 this->Text = getTitleText();				

					 this->resizeToolStripMenuItem->Enabled = true;
					 this->saveToolStripMenuItem->Enabled = true;
					 this->flipXToolStripMenuItem->Enabled = true;
					 this->flipYToolStripMenuItem->Enabled = true;
					 this->rotate90ToolStripMenuItem->Enabled = true;
					 this->rotate90ToolStripMenuItem1->Enabled = true;
					 this->cropToolStripMenuItem->Enabled = true;
					 this->uploadToolStripMenuItem->Enabled = true;

					 if(imagePanel->isWebImage) {

						 this->deleteToolStripMenuItem->Enabled = false;

					 } else {

						 this->deleteToolStripMenuItem->Enabled = true;

						 imageFileWatcher->Path = currentImagePath;
					 }

				 } catch(Exception ^e) {

					 MessageBox::Show(e->Message);
				 }
			 }

	
	private: System::Void resizeToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

				 resizeForm = gcnew ResizeForm(imagePanel->getImageSize());
				 resizeForm->OnResize += gcnew ResizeForm::ResizeUpdateHandler(this, &MainForm1::resizeImageEvent);
				 resizeForm->Show();
			 }

	private: System::Void resizeImageEvent(System::Object^  sender, ResizeUpdateEventArgs^  e) {

				 imagePanel->resizeImage(e->width, e->height);
				 this->Refresh();	
			 }
	private: System::Void saveToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

				 saveFileDialog->FileName = imagePanel->name;

				 if(saveFileDialog->ShowDialog() == ::DialogResult::OK)
					{
						try {

							imagePanel->saveImage(saveFileDialog->FileName);
							loadImage(saveFileDialog->FileName);

						} catch(Exception ^e) {

							MessageBox::Show(e->Message);
						}
					}
			 }
	private: System::Void flipXToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {
				 imagePanel->rotateFlipImage(System::Drawing::RotateFlipType::RotateNoneFlipX);
			 }
	private: System::Void flipYToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {
				 imagePanel->rotateFlipImage(System::Drawing::RotateFlipType::RotateNoneFlipY);
			 }
	private: System::Void rotate90ToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {
				 imagePanel->rotateFlipImage(System::Drawing::RotateFlipType::Rotate90FlipNone);
			 }
	private: System::Void rotate90ToolStripMenuItem1_Click(System::Object^  sender, System::EventArgs^  e) {
				 imagePanel->rotateFlipImage(System::Drawing::RotateFlipType::Rotate270FlipNone);
			 }
	private: System::Void autoScaleToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

			 }
	private: System::Void imagePanelMouseWheel_Event(System::Object^  sender, MouseEventArgs^  e) {

				 if(e->Delta > 0) {

					 loadImage(imageFileWatcher->getPrevImageFile());

				 } else {

					 loadImage(imageFileWatcher->getNextImageFile());
				 }
			 }
    private: System::Void filesGridMouseWheel_Event(System::Object^  sender, MouseEventArgs^  e) {

				 if(e->Delta > 0) {

					 //filesImageGrid->displayPrevPage();

				 } else {

					 //filesImageGrid->displayNextPage();
				 }
			 }
	private: System::Void deleteToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {
				 deleteFile();
			 }

	private: void deleteFile() {

				 if(currentImagePath == nullptr || imagePanel->isWebImage) return;

				 try {

					 if(MessageBox::Show(L"Are you sure you want to permanently delete this file?\n\n" + currentImagePath,
						 "Delete File", MessageBoxButtons::YesNo, MessageBoxIcon::Question)
						 == Windows::Forms::DialogResult::Yes) 
					 {

						 String^ deleteImage = (String ^)currentImagePath->Clone();
						 loadImage(imageFileWatcher->getNextImageFile());
						 System::IO::File::Delete(deleteImage);

					 }

				 } catch(Exception ^e) {

					 MessageBox::Show(e->Message);
				 }

			 }
	private: System::Void freefapToolStripMenuItem_Click(System::Object^ sender, System::EventArgs^  e) {

				 try {

					 UploadImage ^uploadImage = gcnew UploadImage();

					 List<ImageStream ^> ^imageStream = gcnew List<ImageStream ^>();

					 imageStream->Add( imagePanel->saveImageToImageStream() );

					 List<HttpWebResponse ^> ^response = uploadImage->freefap(imageStream);

					 for(int i = 0; i < response->Count; i++) {

						 String ^text = L"Freefap: ";

						 UploadOutputForm ^output = gcnew UploadOutputForm(response[i], text);
						 output->Show();
					 }


				 } catch(Exception ^e) {

					 MessageBox::Show(e->Message);
				 }

			 }
	private: System::Void dumppixToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

				 try {

					 UploadImage ^uploadImage = gcnew UploadImage();

					 List<ImageStream ^> ^imageStream = gcnew List<ImageStream ^>();

					 imageStream->Add( imagePanel->saveImageToImageStream() );

					 List<HttpWebResponse ^> ^response = uploadImage->dumppix(imageStream);

					 for(int i = 0; i < response->Count; i++) {

						 String ^text = L"Dumppix: ";

						 UploadOutputForm ^output = gcnew UploadOutputForm(response[i], text);
						 output->Show();
					 }


				 } catch(Exception ^e) {

					 MessageBox::Show(e->Message);
				 }
			 }

	private: System::Void imageModified_Event(System::Object^  sender, ImageModifiedEventArgs^  e) {

				 if(e->modified == false) {

					 this->Text = getTitleText();	

				 } else {

					 this->Text = getTitleText() + L" *";	
				 }
			 }
	private: System::Void openURLToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

				 String ^currentURL = L"";

				 if(currentImagePath != nullptr && Util::isUrl(currentImagePath)) {

					 currentURL = currentImagePath;
				 }

				 OpenURLForm ^openURLForm = gcnew OpenURLForm(currentURL);
				 if(openURLForm->ShowDialog() == ::DialogResult::OK) {

					 loadImage(openURLForm->getURL());
				 }

			 }
	private: System::Void cropToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

				 imagePanel->cropStage = ImagePanel::CropStage::ENABLED;
			 }
	private: System::Void searchToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

				 ImageSearchForm ^imageSearch = gcnew ImageSearchForm();
				 imageSearch->OnImageSearchMouseDown += gcnew ImageSearchForm::ImageSearchMouseDownEventHandler(this, &MainForm1::searchImage_Click);
				 imageSearch->Show();
			 }
	private: System::Void searchImage_Click(System::Object^  sender, ImageGridMouseEventArgs^  e) {

				 if(e->Button == System::Windows::Forms::MouseButtons::Left) {

					 IImageResult ^imageInfo = dynamic_cast<IImageResult ^>(e->item->data);

					 this->loadImage(imageInfo->Url);
				 }
			 }
	private: System::Void picasaToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

				 PicasaForm ^picasa = gcnew PicasaForm();
				 picasa->authenticate("i.coronel7@gmail.com", "2bGreat?");
				 picasa->OnDownloadEvent += gcnew PicasaForm::DownloadEventHandler(this, &MainForm1::picasaDownload_Event);
				 picasa->OnUploadEvent += gcnew PicasaForm::UploadEventHandler(this, &MainForm1::picasaUpload_Event);
				 picasa->Show();


			 }
	private: System::Void picasaDownload_Event(System::Object^  sender, ImageGridMouseEventArgs^  e) {

				 Google::Picasa::Photo ^photo = dynamic_cast<Google::Picasa::Photo ^>(e->item->data);

				 this->loadImage(photo->PhotoUri->AbsoluteUri);

			 }
	
	private: void picasaUpload_Event(System::Object^  sender, System::EventArgs^  e) {

				 PicasaForm ^picasaForm = dynamic_cast<PicasaForm ^>(sender);

				 List<ImageStream ^> ^imageStream = gcnew List<ImageStream ^>();

				 imageStream->Add( imagePanel->saveImageToImageStream() );

				 picasaForm->uploadImages(imageStream);

				 //imageData->Close();

			 }
	
private: System::Void tabSelected(System::Object^  sender, System::Windows::Forms::TabControlEventArgs^  e) {

			 if(e->TabPageIndex == 1) {

				 if(!filesGridPath->Equals(imageFileWatcher->Path)) {

					 List<ImageGridItem ^> ^imageData = gcnew List<ImageGridItem ^>();

					 for(int i = 0; i < imageFileWatcher->imageFiles->Count; i++) {

					   imageData->Add( gcnew ImageGridItem(imageFileWatcher->imageFiles[i]));
							
					 }	

					 //filesImageGrid->loadImageData(imageData);				 

					 filesGridPath = imageFileWatcher->Path;

				 }
			 }
		 }

private: System::Void tabPage_MouseEnter(System::Object^  sender, System::EventArgs^  e) {

			 TabPage ^tabpage = dynamic_cast<TabPage ^>(sender);

			 tabpage->Focus();
		 }
private: System::Void filesGridDoubleClick(System::Object^  sender, imageviewer::ImageGridMouseEventArgs^  e) {

		
		 }
private: System::Void filesGridMouseDown(System::Object^  sender, imageviewer::ImageGridMouseEventArgs^  e) {

			 if(e->Button == Windows::Forms::MouseButtons::Left) {
				 //filesImageGrid->toggleImageSelected(e->globalImageNr);
			 }

			 if(e->Button == Windows::Forms::MouseButtons::Right) {

				loadImage(e->item->imageLocation);
				//tabControl->SelectedIndex = 0;
			 }
		 }
};
}
