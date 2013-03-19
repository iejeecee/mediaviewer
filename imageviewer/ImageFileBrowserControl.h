#pragma once

#include "MediaFileWatcher.h"
#include "ImageGridControl.h"
#include "ImageGridPagerControl.h"
#include "ImageGridToolStripMenuItem.h"
#include "DirectoryBrowserControl.h"
#include "MediaInfoForm.h"
#include "GeoTagForm.h"
#include "Util.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;
using namespace System::IO;


namespace imageviewer {

	/// <summary>
	/// Summary for ImageFileBrowserControl
	/// </summary>
	public ref class ImageFileBrowserControl : public System::Windows::Forms::UserControl
	{
	public: delegate void ImageFileBrowserEventHandler(System::Object^ sender, ImageGridMouseEventArgs^ e);
	public: event ImageFileBrowserEventHandler ^OnViewEvent; 
	public: delegate void ChangeBrowseDirectoryEventHandler(System::Object^ sender, EventArgs^ e);
	public: event ChangeBrowseDirectoryEventHandler ^OnChangeBrowseDirectory; 
	private: MediaFileWatcher ^mediaFileWatcher;
	private: delegate void imageFileWatcherEventDelegate(List<ImageGridItem ^> ^imageData);	
	private: delegate void imageFileWatcherRenamedEventDelegate(System::IO::RenamedEventArgs ^e);	
	public:
		ImageFileBrowserControl(void)
		{
			
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
			imageGrid->setNrImagePanels(5 * 5, true);

			mediaFileWatcher = gcnew MediaFileWatcher();
			mediaFileWatcher->MediaDeleted += gcnew FileSystemEventHandler(this, &ImageFileBrowserControl::ImageFileWatcherThread_MediaDeleted);
			mediaFileWatcher->MediaChanged += gcnew FileSystemEventHandler(this, &ImageFileBrowserControl::ImageFileWatcherThread_MediaChanged);
			mediaFileWatcher->MediaCreated += gcnew FileSystemEventHandler(this, &ImageFileBrowserControl::ImageFileWatcherThread_MediaCreated);
			mediaFileWatcher->MediaRenamed += gcnew RenamedEventHandler(this, &ImageFileBrowserControl::ImageFileWatcherThread_MediaRenamed);
			mediaFileWatcher->CurrentMediaChanged += gcnew EventHandler<FileSystemEventArgs ^>(this, &ImageFileBrowserControl::imageFileWatcherThread_CurrentMediaChanged);

			pager->ImageGrid = imageGrid;
			
		}

	public: property ImageGridControl ^imageGrid {

				ImageGridControl ^get() {

					return(_imageGrid);
				}

			}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~ImageFileBrowserControl()
		{
			if (components)
			{
				delete components;
			}
		}
	private: imageviewer::ImageGridControl^  _imageGrid;
	private: System::Windows::Forms::SplitContainer^  splitContainer;
	private: imageviewer::ImageGridPagerControl^  pager;
	private: System::Windows::Forms::SplitContainer^  splitContainer1;
	private: imageviewer::DirectoryBrowserControl^  directoryBrowser;

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
			this->splitContainer = (gcnew System::Windows::Forms::SplitContainer());
			this->splitContainer1 = (gcnew System::Windows::Forms::SplitContainer());
			this->directoryBrowser = (gcnew imageviewer::DirectoryBrowserControl());
			this->_imageGrid = (gcnew imageviewer::ImageGridControl());
			this->pager = (gcnew imageviewer::ImageGridPagerControl());
			this->splitContainer->Panel1->SuspendLayout();
			this->splitContainer->Panel2->SuspendLayout();
			this->splitContainer->SuspendLayout();
			this->splitContainer1->Panel1->SuspendLayout();
			this->splitContainer1->Panel2->SuspendLayout();
			this->splitContainer1->SuspendLayout();
			this->SuspendLayout();
			// 
			// splitContainer
			// 
			this->splitContainer->Dock = System::Windows::Forms::DockStyle::Fill;
			this->splitContainer->FixedPanel = System::Windows::Forms::FixedPanel::Panel2;
			this->splitContainer->IsSplitterFixed = true;
			this->splitContainer->Location = System::Drawing::Point(0, 0);
			this->splitContainer->Name = L"splitContainer";
			this->splitContainer->Orientation = System::Windows::Forms::Orientation::Horizontal;
			// 
			// splitContainer.Panel1
			// 
			this->splitContainer->Panel1->Controls->Add(this->splitContainer1);
			// 
			// splitContainer.Panel2
			// 
			this->splitContainer->Panel2->Controls->Add(this->pager);
			this->splitContainer->Size = System::Drawing::Size(837, 477);
			this->splitContainer->SplitterDistance = 425;
			this->splitContainer->TabIndex = 2;
			// 
			// splitContainer1
			// 
			this->splitContainer1->Dock = System::Windows::Forms::DockStyle::Fill;
			this->splitContainer1->Location = System::Drawing::Point(0, 0);
			this->splitContainer1->Name = L"splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this->splitContainer1->Panel1->Controls->Add(this->directoryBrowser);
			// 
			// splitContainer1.Panel2
			// 
			this->splitContainer1->Panel2->Controls->Add(this->_imageGrid);
			this->splitContainer1->Size = System::Drawing::Size(837, 425);
			this->splitContainer1->SplitterDistance = 160;
			this->splitContainer1->TabIndex = 1;
			// 
			// directoryBrowser
			// 
			this->directoryBrowser->Dock = System::Windows::Forms::DockStyle::Fill;
			this->directoryBrowser->Location = System::Drawing::Point(0, 0);
			this->directoryBrowser->Name = L"directoryBrowser";
			this->directoryBrowser->Size = System::Drawing::Size(160, 425);
			this->directoryBrowser->TabIndex = 0;
			this->directoryBrowser->Renamed += gcnew System::IO::RenamedEventHandler(this, &ImageFileBrowserControl::directoryBrowser_Renamed);
			this->directoryBrowser->OnAfterSelect += gcnew imageviewer::DirectoryBrowserControl::AfterSelectDelegate(this, &ImageFileBrowserControl::directoryBrowserControl_AfterSelect);
			this->directoryBrowser->MouseEnter += gcnew System::EventHandler(this, &ImageFileBrowserControl::directoryBrowser_MouseEnter);
			// 
			// _imageGrid
			// 
			this->_imageGrid->Dock = System::Windows::Forms::DockStyle::Fill;
			this->_imageGrid->Location = System::Drawing::Point(0, 0);
			this->_imageGrid->Name = L"_imageGrid";
			this->_imageGrid->Size = System::Drawing::Size(673, 425);
			this->_imageGrid->TabIndex = 0;
			this->_imageGrid->MouseEnter += gcnew System::EventHandler(this, &ImageFileBrowserControl::imageGrid_MouseEnter);
			// 
			// pager
			// 
			this->pager->Anchor = System::Windows::Forms::AnchorStyles::None;
			this->pager->BeginButtonEnabled = true;
			this->pager->CurrentPage = 0;
			this->pager->EndButtonEnabled = true;
			this->pager->ImageGrid = nullptr;
			this->pager->Location = System::Drawing::Point(284, 3);
			this->pager->Name = L"pager";
			this->pager->NextButtonEnabled = true;
			this->pager->PrevButtonEnabled = true;
			this->pager->Size = System::Drawing::Size(284, 37);
			this->pager->TabIndex = 2;
			this->pager->TotalPages = 0;
			// 
			// ImageFileBrowserControl
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->Controls->Add(this->splitContainer);
			this->Name = L"ImageFileBrowserControl";
			this->Size = System::Drawing::Size(837, 477);
			this->splitContainer->Panel1->ResumeLayout(false);
			this->splitContainer->Panel2->ResumeLayout(false);
			this->splitContainer->ResumeLayout(false);
			this->splitContainer1->Panel1->ResumeLayout(false);
			this->splitContainer1->Panel2->ResumeLayout(false);
			this->splitContainer1->ResumeLayout(false);
			this->ResumeLayout(false);

		}
#pragma endregion
	
	static log4net::ILog ^log = log4net::LogManager::GetLogger(System::Reflection::MethodBase::GetCurrentMethod()->DeclaringType);
	private: delegate void InvokeCurrentImageChangedDelegate(FileSystemEventArgs ^e);
	public: event EventHandler<FileSystemEventArgs ^> ^CurrentMediaChanged;

	private: ImageGridContextMenuStrip ^createContextMenu() {

				 ImageGridContextMenuStrip ^contextMenu = gcnew ImageGridContextMenuStrip(imageGrid);

				 ToolStripSeparator ^separator1 = gcnew ToolStripSeparator();
				 ToolStripSeparator ^separator2 = gcnew ToolStripSeparator();
				 ToolStripSeparator ^separator3 = gcnew ToolStripSeparator();
				 ToolStripSeparator ^separator4 = gcnew ToolStripSeparator();

				 ImageGridToolStripMenuItem ^viewItem = gcnew ImageGridToolStripMenuItem();
				 viewItem->Name = L"view";
				 viewItem->Text = L"View";
				 viewItem->OnGridMouseDown += gcnew ImageGridToolStripMenuItem::GridMouseDownEventHandler(this, &ImageFileBrowserControl::viewToolStripMenuItem_MouseDown);

				 ImageGridToolStripMenuItem ^metaData = gcnew ImageGridToolStripMenuItem();
				 metaData->Name = L"meta data";
				 metaData->Text = L"Meta Data";
				 metaData->OnGridMouseDown += gcnew ImageGridToolStripMenuItem::GridMouseDownEventHandler(this, &ImageFileBrowserControl::metaDataToolStripMenuItem_MouseDown);

				 ImageGridToolStripMenuItem ^geoTag = gcnew ImageGridToolStripMenuItem();
				 geoTag->Name = L"geo tag";
				 geoTag->Text = L"Geo Tag";
				 geoTag->OnGridMouseDown += gcnew ImageGridToolStripMenuItem::GridMouseDownEventHandler(this, &ImageFileBrowserControl::geoTagToolStripMenuItem_MouseDown);

				 ImageGridToolStripMenuItem ^cut = gcnew ImageGridToolStripMenuItem();
				 cut->Name = L"cut";
				 cut->Text = L"Cut";
				 cut->OnGridMouseDown += gcnew ImageGridToolStripMenuItem::GridMouseDownEventHandler(this, &ImageFileBrowserControl::cutToolStripMenuItem_MouseDown);

				 ImageGridToolStripMenuItem ^copy = gcnew ImageGridToolStripMenuItem();
				 copy->Name = L"copy";
				 copy->Text = L"Copy";
				 copy->OnGridMouseDown += gcnew ImageGridToolStripMenuItem::GridMouseDownEventHandler(this, &ImageFileBrowserControl::copyToolStripMenuItem_MouseDown);

				 ImageGridToolStripMenuItem ^selectAll = gcnew ImageGridToolStripMenuItem();
				 selectAll->Name = L"selectAll";
				 selectAll->Text = L"Select All";
				 selectAll->OnGridMouseDown += gcnew ImageGridToolStripMenuItem::GridMouseDownEventHandler(this, &ImageFileBrowserControl::selectAllToolStripMenuItem_MouseDown);

				 ImageGridToolStripMenuItem ^deselectAll = gcnew ImageGridToolStripMenuItem();
				 deselectAll->Name = L"deselectAll";
				 deselectAll->Text = L"Deselect All";
				 deselectAll->OnGridMouseDown += gcnew ImageGridToolStripMenuItem::GridMouseDownEventHandler(this, &ImageFileBrowserControl::deselectAllToolStripMenuItem_MouseDown);

				 ImageGridToolStripMenuItem ^renameImage = gcnew ImageGridToolStripMenuItem();
				 renameImage->Name = L"renameImage";
				 renameImage->Text = L"Rename";
				 renameImage->OnGridMouseDown += gcnew ImageGridToolStripMenuItem::GridMouseDownEventHandler(this, &ImageFileBrowserControl::renameImageToolStripMenuItem_MouseDown);

				 ImageGridToolStripMenuItem ^deleteImage = gcnew ImageGridToolStripMenuItem();
				 deleteImage->Name = L"deleteImage";
				 deleteImage->Text = L"Delete";
				 deleteImage->OnGridMouseDown += gcnew ImageGridToolStripMenuItem::GridMouseDownEventHandler(this, &ImageFileBrowserControl::deleteImageToolStripMenuItem_MouseDown);

				 contextMenu->Items->Add(viewItem);
				 contextMenu->Items->Add(separator1);
				 contextMenu->Items->Add(metaData);
				 contextMenu->Items->Add(geoTag);
				 contextMenu->Items->Add(separator2);
				 contextMenu->Items->Add(cut);
				 contextMenu->Items->Add(copy);			
				 contextMenu->Items->Add(separator3);
				 contextMenu->Items->Add(selectAll);
				 contextMenu->Items->Add(deselectAll);
				 contextMenu->Items->Add(separator4);
				 contextMenu->Items->Add(renameImage);	
				 contextMenu->Items->Add(deleteImage);	

				 return(contextMenu);

			 }

	private: ImageGridItem ^fileInfoToImageGridItem(String ^location) {
				
				 ImageGridItem ^item = gcnew ImageGridItem(location);

				 item->ContextMenu = createContextMenu();
				 item->InfoIconMode = ImageGridItem::InfoIconModes::DEFAULT_ICONS_ONLY;

				 return(item);
			 }

	public: void setBrowsePath(String ^path) {

				FileInfo ^fileInfo = gcnew FileInfo(path);

				String ^pathWithoutFileName;

				if((fileInfo->Attributes & FileAttributes::Directory) == FileAttributes::Directory) {

					pathWithoutFileName = path;
									
				} else {

					pathWithoutFileName = Util::getPathWithoutFileName(path);
					mediaFileWatcher->CurrentMediaFile = path;
				}

				if(mediaFileWatcher->Path->Equals(pathWithoutFileName)) {

					return;
				}

				mediaFileWatcher->Path = pathWithoutFileName;

				directoryBrowser->createDirectoryTreeViewFromPath(pathWithoutFileName);
				
				List<ImageGridItem ^> ^imageData = gcnew List<ImageGridItem ^>();

				for(int i = 0; i < mediaFileWatcher->MediaFiles->Count; i++) {

					ImageGridItem ^item = fileInfoToImageGridItem(mediaFileWatcher->MediaFiles[i]);				
					imageData->Add(item);

				}	

				imageGrid->initializeImageData(imageData);

				OnChangeBrowseDirectory(this, EventArgs::Empty);
			}

	public: String ^getBrowsePath() {

				return(mediaFileWatcher->Path);
			}
	public: void setNextImageFile() {

				mediaFileWatcher->setNextImageFile();
			}
	public: void setPrevImageFile() {

				mediaFileWatcher->setPrevImageFile();
			}
	    
	private: void imageFileWatcher_CurrentMediaChanged(FileSystemEventArgs ^e) {

				 CurrentMediaChanged(this, e);
			 }

	private: void imageFileWatcherThread_CurrentMediaChanged(Object ^sender, System::IO::FileSystemEventArgs ^e) {

			 cli::array<Object ^> ^args = gcnew cli::array<Object ^>(1);

			 args[0] = e;

			 this->Invoke(gcnew InvokeCurrentImageChangedDelegate(this, &ImageFileBrowserControl::imageFileWatcher_CurrentMediaChanged), args);
		 }

	private: void ImageFileWatcher_MediaDeleted(List<ImageGridItem ^> ^imageData) {

				 imageGrid->removeImageData(imageData);
			 }

	private: void ImageFileWatcherThread_MediaDeleted(Object ^sender, System::IO::FileSystemEventArgs ^e) {

				 List<ImageGridItem ^> ^imageData = gcnew List<ImageGridItem ^>();

				 imageData->Add( gcnew ImageGridItem(e->FullPath));

				 cli::array<Object ^> ^args = gcnew cli::array<Object ^>(1);

				 args[0] = imageData;

				 this->Invoke(gcnew imageFileWatcherEventDelegate(this, &ImageFileBrowserControl::ImageFileWatcher_MediaDeleted), args);
			 }

	private: void ImageFileWatcher_MediaChanged(List<ImageGridItem ^> ^imageData) {

				 imageGrid->updateImageData(imageData);
			 }
	private: void ImageFileWatcherThread_MediaChanged(Object ^sender, System::IO::FileSystemEventArgs ^e) {

				 try {

					 List<ImageGridItem ^> ^imageData = gcnew List<ImageGridItem ^>();

					 imageData->Add(fileInfoToImageGridItem(e->FullPath));

					 cli::array<Object ^> ^args = gcnew cli::array<Object ^>(1);

					 args[0] = imageData;

					 this->Invoke(gcnew imageFileWatcherEventDelegate(this, &ImageFileBrowserControl::ImageFileWatcher_MediaChanged), args);

				 } catch (Exception ^ex) {

					 log->Error("Media Changed Error", ex);
				 }

			 }

	private: void ImageFileWatcher_MediaCreated(List<ImageGridItem ^> ^imageData) {

				 imageGrid->addImageData(imageData);
			 }

	private: void ImageFileWatcherThread_MediaCreated(Object ^sender, System::IO::FileSystemEventArgs ^e) {

				 try {

					 List<ImageGridItem ^> ^imageData = gcnew List<ImageGridItem ^>();

					 imageData->Add( fileInfoToImageGridItem(e->FullPath));

					 cli::array<Object ^> ^args = gcnew cli::array<Object ^>(1);

					 args[0] = imageData;

					 this->Invoke(gcnew imageFileWatcherEventDelegate(this, &ImageFileBrowserControl::ImageFileWatcher_MediaCreated), args);

				 } catch (Exception ^ex) {

					 log->Error("Media Created Error", ex);			
				 }

			 }

    private: void ImageFileWatcher_MediaRenamed(System::IO::RenamedEventArgs ^e) {

				 imageGrid->removeImageData(imageGrid->getImageData(e->OldFullPath));
				 imageGrid->addImageData(fileInfoToImageGridItem(e->FullPath));

			 }
	private: void ImageFileWatcherThread_MediaRenamed(Object ^sender, System::IO::RenamedEventArgs ^e) {
				
				 cli::array<Object ^> ^args = gcnew cli::array<Object ^>(1);

				 args[0] = e;

				 this->Invoke(gcnew imageFileWatcherRenamedEventDelegate(this, &ImageFileBrowserControl::ImageFileWatcher_MediaRenamed), args);

			 }
	
	private: System::Void viewToolStripMenuItem_MouseDown(System::Object^  sender, ImageGridMouseEventArgs ^e) {

				 OnViewEvent(this, e);
			 }
	
	private: System::Void metaDataToolStripMenuItem_MouseDown(System::Object^  sender, ImageGridMouseEventArgs ^e) {

				 List<ImageGridItem ^> ^selected = imageGrid->getSelectedImageData();

				 if(selected->Count == 0) {

					 selected->Add(e->item);
				 }

				 List<String ^> ^fileNames = gcnew List<String ^>();

				 for each(ImageGridItem ^item in selected) {

					 fileNames->Add(item->ImageLocation);
				 }

				 for each(Form ^form in Application::OpenForms) {

					 if(form->GetType() == MediaInfoForm::typeid) {

						 MediaInfoForm ^mediaInfo = dynamic_cast<MediaInfoForm ^>(form);

						 if(Util::listSortAndCompare<String ^>(mediaInfo->FileNames, fileNames)) {

							 mediaInfo->BringToFront();
							 return;
						 }
					 }
				 }
				 
				 try {

					 MediaInfoForm ^mediaInfo = gcnew MediaInfoForm();
					 mediaInfo->FileNames = fileNames;

					 mediaInfo->Show();					 

				 } catch (Exception ^e) {

					 log->Error("Error showing media info", e);
					 MessageBox::Show(e->Message, "Error");
				 }

			 }

	private: System::Void geoTagToolStripMenuItem_MouseDown(System::Object^  sender, ImageGridMouseEventArgs ^e) {

				 List<ImageGridItem ^> ^selected = imageGrid->getSelectedImageData();

				 if(selected->Count == 0) {

					 selected->Add(e->item);
				 }

				 List<String ^> ^fileNames = gcnew List<String ^>();

				 for each(ImageGridItem ^item in selected) {

					 fileNames->Add(item->ImageLocation);
				 }

				 try {
					
					 GeoTagForm ^geoTag = gcnew GeoTagForm(fileNames);

					 geoTag->Show();

				 } catch (Exception ^e) {

					 log->Error("Error opening geotag window", e);
					 MessageBox::Show("Error opening geotag window: " + e->Message, "error");
				 }
				 
			 }

	private: System::Void deleteImageToolStripMenuItem_MouseDown(System::Object^  sender, ImageGridMouseEventArgs ^e) {

				 e->item->ContextMenu->Hide();

				 List<ImageGridItem ^> ^selected = imageGrid->getSelectedImageData();

				 if(selected->Count == 0) {

					 selected->Add(e->item);
				 }
				
				 if(MessageBox::Show(L"Are you sure you want to permanently delete " + Convert::ToString(selected->Count) + " file(s)?",
					 L"Delete Images", MessageBoxButtons::YesNo, MessageBoxIcon::Question)
					 == Windows::Forms::DialogResult::Yes) 
				 {

					 try {

						 imageGrid->removeImageData(selected);

						 for(int i = 0; i < selected->Count; i++) {

							 System::IO::File::Delete(selected[i]->ImageLocation);
						 }

					 } catch(Exception ^e) {

						 log->Error("Error deleting file", e);
						 MessageBox::Show(e->Message);
					 }
				 }

			 }

    private: System::Void renameImageToolStripMenuItem_MouseDown(System::Object^  sender, ImageGridMouseEventArgs ^e) {

				 InputDialog ^dialog = gcnew InputDialog();

				 List<ImageGridItem ^> ^selected = imageGrid->getSelectedImageData();

				 String ^info;

				 if(selected->Count == 0) {

					 selected->Add(e->item);
					 info = "Rename Image: " + e->item->ImageLocation;
					 dialog->inputText = System::IO::Path::GetFileNameWithoutExtension(e->item->ImageLocation);

				 } else {

					 info = "Rename " + Convert::ToString(selected->Count) + " images";
				 }
				
				 dialog->Text = info;
				
				 if(dialog->ShowDialog() == DialogResult::OK) {

					 try {

						 for(int i = 0; i < selected->Count; i++) {

							String ^directory = Path::GetDirectoryName(selected[i]->ImageLocation);
							String ^counter = selected->Count > 1 ? " (" + Convert::ToString(i + 1) + ")" : "";

							String ^name = Path::GetFileNameWithoutExtension(dialog->inputText);
							String ^ext = Path::GetExtension(selected[i]->ImageLocation);

							String ^newLocation = directory + "\\" + name + counter + ext;

							File::Move(selected[i]->ImageLocation, newLocation);
						 }

					 } catch(Exception ^e) {

						 log->Error("Error renaming file", e);
						 MessageBox::Show(e->Message);
					 }

				 }
				
			 }
	private: System::Void selectAllToolStripMenuItem_MouseDown(System::Object^  sender, ImageGridMouseEventArgs ^e) {

				 imageGrid->setSelectedForAllImages(true);
			 }
	private: System::Void deselectAllToolStripMenuItem_MouseDown(System::Object^  sender, ImageGridMouseEventArgs ^e) {

				 imageGrid->setSelectedForAllImages(false);
			 }
    private: System::Void cutToolStripMenuItem_MouseDown(System::Object^  sender, ImageGridMouseEventArgs ^e) {

				 List<ImageGridItem ^> ^selected = imageGrid->getSelectedImageData();

				 if(selected->Count == 0) {

					 selected->Add(e->item);
				 }

				 StringCollection ^files = gcnew StringCollection();

				 for each(ImageGridItem ^item in selected) {

					 files->Add(item->ImageLocation);
				 }

				 DirectoryBrowserControl::clipboardAction = DirectoryBrowserControl::ClipboardAction::CUT;

				 Clipboard::Clear();
				 if(files->Count > 0) {

					Clipboard::SetFileDropList(files);
				 }
			 }
    private: System::Void copyToolStripMenuItem_MouseDown(System::Object^  sender, ImageGridMouseEventArgs ^e) {

				 List<ImageGridItem ^> ^selected = imageGrid->getSelectedImageData();

				  if(selected->Count == 0) {

					 selected->Add(e->item);
				 }

				 StringCollection ^files = gcnew StringCollection();

				 for each(ImageGridItem ^item in selected) {

					 files->Add(item->ImageLocation);
				 }

				 DirectoryBrowserControl::clipboardAction = DirectoryBrowserControl::ClipboardAction::COPY;

				 Clipboard::Clear();
				 if(files->Count > 0) {

					Clipboard::SetFileDropList(files);
				 }
			 }
  

	private: System::Void moveButton_Click(System::Object^  sender, System::EventArgs^  e) {

				 List<ImageGridItem ^> ^images = imageGrid->getSelectedImageData();	
				 if(images->Count == 0) return;

				 FolderBrowserDialog ^dialog = gcnew FolderBrowserDialog();

				 dialog->RootFolder = Environment::SpecialFolder::MyComputer;
				 dialog->SelectedPath = mediaFileWatcher->Path;
				 dialog->Description = L"Move " + Convert::ToString(images->Count) + L" image(s) to";

				 if(dialog->ShowDialog() == DialogResult::OK) {

					 for(int i = 0; i < images->Count; i++) {

						 String ^fileName = Path::GetFileName(images[i]->ImageLocation);
						 String ^destFileName = dialog->SelectedPath + "\\" + fileName;

						 if(File::Exists(destFileName)) {

							 DialogResult result = MessageBox::Show(destFileName + L"\n already exists, do you want to overwrite?", "Overwrite File?",
								 MessageBoxButtons::YesNoCancel, MessageBoxIcon::Question);

							 if(result == DialogResult::Yes) {

								 File::Delete(destFileName);

							 } else if(result == DialogResult::No) {

								 continue;

							 } else if(result == DialogResult::Cancel) {

								 break;
							 }

						 }

						 File::Move(images[i]->ImageLocation, destFileName);

					 }

				 }
			 }
private: System::Void directoryBrowserControl_AfterSelect(System::Object^  sender, System::Windows::Forms::TreeViewEventArgs^  e) {

			 DirectoryInfo ^directory = dynamic_cast<DirectoryInfo ^>(e->Node->Tag);

			 setBrowsePath(directory->FullName);
		 }

private: System::Void imageGrid_MouseEnter(System::Object^  sender, System::EventArgs^  e) {


		 }
private: System::Void directoryBrowser_MouseEnter(System::Object^  sender, System::EventArgs^  e) {


		 }
private: System::Void directoryBrowser_Renamed(System::Object^  sender, System::IO::RenamedEventArgs^  e) {

			 // if(mediaFileWatcher->Path->Equals(e->OldFullPath)) {

			 setBrowsePath(e->FullPath);
			 //}
		 }
};
}
