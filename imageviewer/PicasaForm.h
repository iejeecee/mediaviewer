#pragma once

#include "ImageGridControl.h"
#include "PicasaAlbumForm.h"
#include "PicasaPhotoForm.h"
#include "PicasaUser.h"
#include "ImageGridContextMenuStrip.h"
#include "ImageGridToolStripMenuItem.h"
#include "PagerControl.h"
#include "PicasaAsyncState.h"
#include "PicasaLoginDialog.h"
#include "PicasaPhotoMetaData.h"
#include "UploadImage.h"
#include "PicasaService2.h"


using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Collections::Generic;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;
using namespace System::Diagnostics;

using namespace Microsoft::Win32;

using namespace Google::GData;
using namespace Google::GData::Extensions;
using namespace Google::GData::Photos;
using namespace Google::GData::Client;
using namespace Google::Picasa;

namespace imageviewer {

	/// <summary>
	/// Summary for PicasaForm
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class PicasaForm : public System::Windows::Forms::Form
	{
	private: PicasaService ^service;
	private: PicasaService2 ^service2;
	private: PicasaFeed ^albumFeed;
	private: PicasaFeed ^photoFeed;
	private: int currentPage;
	private: String ^titleInfo;
	private: System::Windows::Forms::ContextMenuStrip^  albumContextMenuStrip;
	private: System::Windows::Forms::ToolStripMenuItem^  createAlbumToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  albumInfoToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  deleteAlbumToolStripMenuItem;
    private: System::Windows::Forms::Button^  loginButton;	
	private: System::Windows::Forms::ImageList^  imageList;
    private: imageviewer::PagerControl^  pager;
	private: System::Windows::Forms::ToolTip^  toolTip;
	private: System::Windows::Forms::ComboBox^  userComboBox;
	private: delegate void UploadNextPhotoEventHandler(System::Object^ sender, PicasaAsyncState^ e);
	private: event UploadNextPhotoEventHandler ^UploadNextPhotoEvent;

	public:

		property ImageGridControl ^imageGrid {

			ImageGridControl ^get() {

				return(_imageGrid);
			}

		}

		delegate void DownloadEventHandler(System::Object^ sender, List<ImageGridItem ^> ^items);
		event DownloadEventHandler ^OnDownloadEvent;

		delegate void ViewEventHandler(System::Object^ sender, ImageGridMouseEventArgs^ e);
		event ViewEventHandler ^OnViewEvent;

		delegate void UploadEventHandler(System::Object^ sender, EventArgs^ e);
		event UploadEventHandler ^OnUploadEvent;


		PicasaForm(void)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
			this->UploadNextPhotoEvent += gcnew UploadNextPhotoEventHandler(this, &PicasaForm::picasaForm_UploadNextPhotoEvent);
			this->KeyPreview = true;

			imageGrid->setNrImagePanels(16, true);
			imageGrid->ImageGridMouseDown += gcnew EventHandler<ImageGridMouseEventArgs^>(this, &PicasaForm::picasaImage_Click);
			imageGrid->UpdateImages += gcnew EventHandler<EventArgs^>(this, &PicasaForm::imageGrid_UpdatePhotosEvent);

			pager->imageGrid = imageGrid;

			service = gcnew PicasaService("exampleCo-exampleApp-1");
			service->AsyncOperationCompleted += gcnew AsyncOperationCompletedEventHandler(this, &PicasaForm::picasaService_asyncOperationCompleted);
			service->AsyncOperationProgress += gcnew AsyncOperationProgressEventHandler(this, &PicasaForm::picasaService_asyncOperationProgress);

			service2 = gcnew PicasaService2();
			service2->AsyncOperationCompleted += gcnew RunWorkerCompletedEventHandler(this, &PicasaForm::picasaService2_asyncOperationCompleted);
			service2->AsyncOperationProgress += gcnew ProgressChangedEventHandler(this, &PicasaForm::picasaService2_asyncOperationProgress);

			states = gcnew PicasaAsyncStateList();

			searchComboBox->SelectedIndex = 2;

			titleInfo = "";

			setTitle();
		
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~PicasaForm()
		{
			if (components)
			{
				delete components;
			}
		}
	private: imageviewer::ImageGridControl^  _imageGrid;
	protected: 
	private: System::Windows::Forms::ListBox^  albumListBox;
	private: System::Windows::Forms::Button^  downloadButton;


	private: System::Windows::Forms::Button^  uploadPhotoButton;
	private: System::Windows::Forms::Label^  label1;
	private: System::Windows::Forms::ComboBox^  searchComboBox;
	private: System::Windows::Forms::Button^  searchButton;
	private: System::Windows::Forms::TextBox^  searchTextBox;


	private: System::ComponentModel::IContainer^  components;
	private: PicasaAsyncStateList ^states;

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
			System::ComponentModel::ComponentResourceManager^  resources = (gcnew System::ComponentModel::ComponentResourceManager(PicasaForm::typeid));
			this->albumListBox = (gcnew System::Windows::Forms::ListBox());
			this->albumContextMenuStrip = (gcnew System::Windows::Forms::ContextMenuStrip(this->components));
			this->createAlbumToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->albumInfoToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->deleteAlbumToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->downloadButton = (gcnew System::Windows::Forms::Button());
			this->uploadPhotoButton = (gcnew System::Windows::Forms::Button());
			this->label1 = (gcnew System::Windows::Forms::Label());
			this->searchComboBox = (gcnew System::Windows::Forms::ComboBox());
			this->searchButton = (gcnew System::Windows::Forms::Button());
			this->searchTextBox = (gcnew System::Windows::Forms::TextBox());
			this->userComboBox = (gcnew System::Windows::Forms::ComboBox());
			this->loginButton = (gcnew System::Windows::Forms::Button());
			this->imageList = (gcnew System::Windows::Forms::ImageList(this->components));
			this->toolTip = (gcnew System::Windows::Forms::ToolTip(this->components));
			this->pager = (gcnew imageviewer::PagerControl());
			this->_imageGrid = (gcnew imageviewer::ImageGridControl());
			this->albumContextMenuStrip->SuspendLayout();
			this->SuspendLayout();
			// 
			// albumListBox
			// 
			this->albumListBox->ContextMenuStrip = this->albumContextMenuStrip;
			this->albumListBox->FormattingEnabled = true;
			this->albumListBox->ItemHeight = 20;
			this->albumListBox->Location = System::Drawing::Point(12, 46);
			this->albumListBox->Name = L"albumListBox";
			this->albumListBox->Size = System::Drawing::Size(230, 584);
			this->albumListBox->TabIndex = 17;
			this->albumListBox->SelectedIndexChanged += gcnew System::EventHandler(this, &PicasaForm::albumListBox_SelectedIndexChanged);
			// 
			// albumContextMenuStrip
			// 
			this->albumContextMenuStrip->Items->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(3) {this->createAlbumToolStripMenuItem, 
				this->albumInfoToolStripMenuItem, this->deleteAlbumToolStripMenuItem});
			this->albumContextMenuStrip->Name = L"albumContextMenuStrip";
			this->albumContextMenuStrip->Size = System::Drawing::Size(182, 82);
			// 
			// createAlbumToolStripMenuItem
			// 
			this->createAlbumToolStripMenuItem->Name = L"createAlbumToolStripMenuItem";
			this->createAlbumToolStripMenuItem->Size = System::Drawing::Size(181, 26);
			this->createAlbumToolStripMenuItem->Text = L"Create Album";
			this->createAlbumToolStripMenuItem->Click += gcnew System::EventHandler(this, &PicasaForm::createAlbumToolStripMenuItem_Click);
			// 
			// albumInfoToolStripMenuItem
			// 
			this->albumInfoToolStripMenuItem->Name = L"albumInfoToolStripMenuItem";
			this->albumInfoToolStripMenuItem->Size = System::Drawing::Size(181, 26);
			this->albumInfoToolStripMenuItem->Text = L"Album Info";
			this->albumInfoToolStripMenuItem->Click += gcnew System::EventHandler(this, &PicasaForm::abumInfoToolStripMenuItem_Click);
			// 
			// deleteAlbumToolStripMenuItem
			// 
			this->deleteAlbumToolStripMenuItem->Name = L"deleteAlbumToolStripMenuItem";
			this->deleteAlbumToolStripMenuItem->Size = System::Drawing::Size(181, 26);
			this->deleteAlbumToolStripMenuItem->Text = L"Delete Album";
			this->deleteAlbumToolStripMenuItem->Click += gcnew System::EventHandler(this, &PicasaForm::deleteAlbumToolStripMenuItem_Click);
			// 
			// downloadButton
			// 
			this->downloadButton->Location = System::Drawing::Point(776, 634);
			this->downloadButton->Name = L"downloadButton";
			this->downloadButton->Size = System::Drawing::Size(97, 36);
			this->downloadButton->TabIndex = 26;
			this->downloadButton->Text = L"Download";
			this->downloadButton->UseVisualStyleBackColor = true;
			this->downloadButton->Click += gcnew System::EventHandler(this, &PicasaForm::downloadButton_Click);
			// 
			// uploadPhotoButton
			// 
			this->uploadPhotoButton->Location = System::Drawing::Point(673, 634);
			this->uploadPhotoButton->Name = L"uploadPhotoButton";
			this->uploadPhotoButton->Size = System::Drawing::Size(97, 36);
			this->uploadPhotoButton->TabIndex = 25;
			this->uploadPhotoButton->Text = L"Upload";
			this->uploadPhotoButton->UseVisualStyleBackColor = true;
			this->uploadPhotoButton->Click += gcnew System::EventHandler(this, &PicasaForm::uploadPhotoButton_Click);
			// 
			// label1
			// 
			this->label1->AutoSize = true;
			this->label1->Location = System::Drawing::Point(444, 642);
			this->label1->Name = L"label1";
			this->label1->Size = System::Drawing::Size(21, 20);
			this->label1->TabIndex = 24;
			this->label1->Text = L"in";
			// 
			// searchComboBox
			// 
			this->searchComboBox->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
			this->searchComboBox->FormattingEnabled = true;
			this->searchComboBox->Items->AddRange(gcnew cli::array< System::Object^  >(3) {L"selected album", L"user albums", L"public albums"});
			this->searchComboBox->Location = System::Drawing::Point(471, 639);
			this->searchComboBox->Name = L"searchComboBox";
			this->searchComboBox->Size = System::Drawing::Size(196, 28);
			this->searchComboBox->TabIndex = 23;
			// 
			// searchButton
			// 
			this->searchButton->Location = System::Drawing::Point(341, 634);
			this->searchButton->Name = L"searchButton";
			this->searchButton->Size = System::Drawing::Size(97, 36);
			this->searchButton->TabIndex = 22;
			this->searchButton->Text = L"Search";
			this->searchButton->UseVisualStyleBackColor = true;
			this->searchButton->Click += gcnew System::EventHandler(this, &PicasaForm::searchButton_Click);
			// 
			// searchTextBox
			// 
			this->searchTextBox->Location = System::Drawing::Point(12, 639);
			this->searchTextBox->Name = L"searchTextBox";
			this->searchTextBox->Size = System::Drawing::Size(323, 26);
			this->searchTextBox->TabIndex = 21;
			this->searchTextBox->KeyDown += gcnew System::Windows::Forms::KeyEventHandler(this, &PicasaForm::searchTextBox_KeyDown);
			// 
			// userComboBox
			// 
			this->userComboBox->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
			this->userComboBox->FormattingEnabled = true;
			this->userComboBox->Location = System::Drawing::Point(12, 12);
			this->userComboBox->Name = L"userComboBox";
			this->userComboBox->Size = System::Drawing::Size(187, 28);
			this->userComboBox->TabIndex = 27;
			this->userComboBox->SelectedIndexChanged += gcnew System::EventHandler(this, &PicasaForm::userComboBox_SelectedIndexChanged);
			// 
			// loginButton
			// 
			this->loginButton->ImageIndex = 0;
			this->loginButton->ImageList = this->imageList;
			this->loginButton->Location = System::Drawing::Point(205, 12);
			this->loginButton->Name = L"loginButton";
			this->loginButton->Size = System::Drawing::Size(37, 28);
			this->loginButton->TabIndex = 29;
			this->toolTip->SetToolTip(this->loginButton, L"Login\r\n");
			this->loginButton->UseVisualStyleBackColor = true;
			this->loginButton->Click += gcnew System::EventHandler(this, &PicasaForm::loginButton_Click);
			// 
			// imageList
			// 
			this->imageList->ImageStream = (cli::safe_cast<System::Windows::Forms::ImageListStreamer^  >(resources->GetObject(L"imageList.ImageStream")));
			this->imageList->TransparentColor = System::Drawing::Color::Transparent;
			this->imageList->Images->SetKeyName(0, L"key_login.ico");
			this->imageList->Images->SetKeyName(1, L"lock.ico");
			this->imageList->Images->SetKeyName(2, L"icoEasyLogin.gif");
			this->imageList->Images->SetKeyName(3, L"Picasa.ico");
			// 
			// pager
			// 
			this->pager->imageGrid = nullptr;
			this->pager->Location = System::Drawing::Point(894, 634);
			this->pager->Name = L"pager";
			this->pager->Size = System::Drawing::Size(275, 38);
			this->pager->TabIndex = 28;
			// 
			// _imageGrid
			// 
			this->_imageGrid->Location = System::Drawing::Point(254, 12);
			this->_imageGrid->Name = L"_imageGrid";
			this->_imageGrid->Size = System::Drawing::Size(915, 616);
			this->_imageGrid->TabIndex = 18;
			// 
			// PicasaForm
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(1179, 678);
			this->Controls->Add(this->loginButton);
			this->Controls->Add(this->pager);
			this->Controls->Add(this->downloadButton);
			this->Controls->Add(this->userComboBox);
			this->Controls->Add(this->uploadPhotoButton);
			this->Controls->Add(this->label1);
			this->Controls->Add(this->searchComboBox);
			this->Controls->Add(this->searchButton);
			this->Controls->Add(this->searchTextBox);
			this->Controls->Add(this->_imageGrid);
			this->Controls->Add(this->albumListBox);
			this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::FixedDialog;
			this->Icon = (cli::safe_cast<System::Drawing::Icon^  >(resources->GetObject(L"$this.Icon")));
			this->MaximizeBox = false;
			this->Name = L"PicasaForm";
			this->Text = L"PicasaForm";
			this->KeyDown += gcnew System::Windows::Forms::KeyEventHandler(this, &PicasaForm::picasaForm_KeyDown);
			this->albumContextMenuStrip->ResumeLayout(false);
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion

	private: void addAuthInfoToEditUri(AtomEntry ^entry) {

				 String ^editUri = entry->EditUri->Content;

				 if(service2->authInfo->success == true) {

					 int argIndex = editUri->IndexOf("?");

					 if(argIndex != -1) {

						editUri = editUri->Remove(argIndex);
					 }

					 editUri += "?access_token=" + service2->authInfo->accessToken;
				 }

				 entry->EditUri = gcnew AtomUri(editUri);
			 }

	private: String ^createPicasaUri(String ^userId) {

				 String ^uri = PicasaQuery::CreatePicasaUri(userId);

				 if(service2->authInfo->success == true) {

					 uri += "?access_token=" + service2->authInfo->accessToken;
				 }

				 return(uri);
			 }
    private: String ^createPicasaUri(String ^userId, String ^albumId) {

				 String ^uri = PicasaQuery::CreatePicasaUri(userId, albumId);

				 if(service2->authInfo->success == true) {

					 uri += "?access_token=" + service2->authInfo->accessToken;
				 }

				 
				 return(uri);
			 }
   private: String ^createPicasaUri(String ^userId, String ^albumId, String ^photoId) {

				 String ^uri = PicasaQuery::CreatePicasaUri(userId, albumId, photoId);

				 if(service2->authInfo->success == true) {

					 uri += "?access_token=" + service2->authInfo->accessToken;
				 }

				 return(uri);
			 }

	private: void albumQuery(String ^userName) {

				 try {

					  // stop any previous query's	
					 PicasaAsyncState ^oldState;

					 while((oldState = states->GetType(PicasaAsyncState::OperationType::ALBUM_QUERY)) != nullptr) {

							service->CancelAsync(oldState);
							// allow the main thread to handle the cancellation events
							// since the registered picasaservice events are run on the main thread
							Application::DoEvents();
					
					 }

					 String ^uri = createPicasaUri(userName);

					 AlbumQuery ^query = gcnew AlbumQuery(uri);

					 PicasaAsyncState ^state = gcnew PicasaAsyncState(PicasaAsyncState::OperationType::ALBUM_QUERY);
					 
					 states->Add(state);

					 service->QueryFeedAync(query->Uri, DateTime::MinValue, state);

				 } catch(Exception ^e) {

					 MessageBox::Show(e->Message);
				 }

			 }

	private: void photoQuery(String ^userName, String ^albumId, String ^queryString) {

				 try {
					
					 // stop any previous query's	
					 PicasaAsyncState ^oldState;

					 while((oldState = states->GetType(PicasaAsyncState::OperationType::PHOTO_QUERY)) != nullptr) {

							service->CancelAsync(oldState);
							// allow the main thread to handle the cancellation events
							// since the registered picasaservice events are run on the main thread
							Application::DoEvents();
					
					 }

					
					 // start new query
					 String ^uri;

					 if(userName != nullptr && albumId != nullptr) {

						 uri = createPicasaUri(userName, albumId);

					 } else if(userName != nullptr) {

						 uri = createPicasaUri(userName);

					 } else {

						 uri = L"https://picasaweb.google.com/data/feed/api/all";

					 }

					 PhotoQuery ^query = gcnew PhotoQuery(uri);

					 query->Query = queryString;
					 if(albumId != nullptr) {

						 query->NumberToRetrieve = 100000;

					 } else {

						 query->NumberToRetrieve = 1024;
					 }


					 PicasaAsyncState ^state = gcnew PicasaAsyncState(PicasaAsyncState::OperationType::PHOTO_QUERY);

					 states->Add(state);

					 service->QueryFeedAync(query->Uri, DateTime::MinValue, state);


				 } catch(Exception ^e) {

					 MessageBox::Show(e->Message);
				 }

			 }

	public: void uploadImages(List<ImageStream ^> ^imageStream) {

				try {

					if(imageStream->Count == 0) {
						
						MessageBox::Show("No photo's selected for upload", "Error");
						return;
					}

					Album ^album = getSelectedAlbum();
					if(album == nullptr) {

						MessageBox::Show("No album selected", "Error");
						return;
					} 					

					PicasaAsyncState ^state = gcnew PicasaAsyncState(PicasaAsyncState::OperationType::UPLOAD_PHOTO);

					state->feedId = photoFeed->Id;
					state->uploadUri = gcnew Uri(photoFeed->Post + "?access_token=" + service2->authInfo->accessToken);
					state->uploadImages = imageStream;

					state->progressForm->totalProgressMaximum = imageStream->Count;
					state->progressForm->userState = state;
					state->progressForm->OnCancelEvent += gcnew ProgressForm::CancelEventHandler(this, &imageviewer::PicasaForm::progressForm_CancelUploadEvent);
					state->progressForm->Show();

					states->Add(state);

					UploadNextPhotoEvent(this, state);

				} catch(Exception ^e) {

					MessageBox::Show(e->Message);
				}
			}
	private: System::Void progressForm_CancelUploadEvent(System::Object^  sender, System::EventArgs^  e) {

				 ProgressForm ^progress = dynamic_cast<ProgressForm ^>(sender);

				 service->CancelAsync(progress->userState);

			 }
    private: System::Void progressForm_CancelEvent(System::Object^  sender, System::EventArgs^  e) {

				 ProgressForm ^progress = dynamic_cast<ProgressForm ^>(sender);

				 PicasaAsyncState ^state = dynamic_cast<PicasaAsyncState ^>(progress->userState);

				 state->cancel = true;

			 }

	private: System::Void picasaForm_UploadNextPhotoEvent(System::Object^  sender, PicasaAsyncState^  e) {

				 PicasaEntry ^entry = gcnew PhotoEntry();
				 
				 ImageStream ^s = e->uploadImages[e->uploadImageNr];
				 e->progressForm->totalProgressValue = e->uploadImageNr;
				 e->progressForm->itemInfo = s->name;
				 e->uploadImageNr++;
				 entry->MediaSource = gcnew MediaFileSource(s->data, s->name, s->mimeType);

				 service->InsertAsync(e->uploadUri, entry, e);
			 }

	private: System::Void picasaService_asyncOperationProgress(Object ^sender, AsyncOperationProgressEventArgs ^e) {

				 PicasaAsyncState ^state = dynamic_cast<PicasaAsyncState ^>(e->UserState);

				 if(state->type == PicasaAsyncState::OperationType::UPLOAD_PHOTO) {

					 state->progressForm->itemProgressValue = e->ProgressPercentage;
				 }
			 }

	private: System::Void picasaService_asyncOperationCompleted(Object ^sender, AsyncOperationCompletedEventArgs ^e) {

				 PicasaAsyncState ^state = dynamic_cast<PicasaAsyncState ^>(e->UserState);

				 states->Remove(state);

				 if(state->type == PicasaAsyncState::OperationType::UPLOAD_PHOTO) {

					 if(e->Error != nullptr) {

						 state->progressForm->addInfoString(e->Error->Message);

					 } else {

						 if(e->Entry != nullptr) {

							 if(state->feedId->Equals(photoFeed->Id)) {

								 photoFeed->Entries->Add(e->Entry);

								 Photo ^photo = gcnew Photo();

								 photo->AtomEntry = e->Entry;

								 ImageGridItem ^item = photoToImageGridItem(photo);

								 List<ImageGridItem ^> ^list = gcnew List<ImageGridItem ^>();
								 list->Add(item);

								 imageGrid->addImageData(list);

								 //displayPhotoFeed();
							 }
						 }
					 }

					 if(state->uploadImages->Count != state->uploadImageNr && e->Cancelled == false) {

						 this->UploadNextPhotoEvent(this, state);

					 } else {

						 state->progressForm->totalProgressValue = state->progressForm->totalProgressMaximum;
						 state->progressForm->actionFinished();
					 }
				 } else if(state->type == PicasaAsyncState::OperationType::PHOTO_QUERY) {

					 if(e->Error != nullptr) {

						 MessageBox::Show(e->Error->Message, L"Photo Query Error");

					 } else if(e->Cancelled == false && e->Feed != nullptr) {

						 photoFeed = dynamic_cast<PicasaFeed ^>(e->Feed);
						 displayPhotoFeed();

					 }
				 } else if(state->type == PicasaAsyncState::OperationType::ALBUM_QUERY) {
					 
					 if(e->Error != nullptr) {

						 MessageBox::Show(e->Error->Message, L"Album Query Error");

					 } else if(e->Cancelled == false && e->Feed != nullptr) {

						 albumFeed = dynamic_cast<PicasaFeed ^>(e->Feed);
						 displayAlbumFeed();

						 // cannot modify readonly albums 
						 if(albumFeed->Entries->Count > 0 && albumFeed->Entries[0]->ReadOnly == true) {

							 createAlbumToolStripMenuItem->Enabled = false;
							 deleteAlbumToolStripMenuItem->Enabled = false;

						 } else {

							 createAlbumToolStripMenuItem->Enabled = true;
							 deleteAlbumToolStripMenuItem->Enabled = true;
						
						 }

					 }

				 } else if(state->type == PicasaAsyncState::OperationType::COMMENT_QUERY) {

					 if(e->Error != nullptr) {

						 MessageBox::Show(e->Error->Message, L"Comment Query Error");

					 } else if(e->Cancelled == false && e->Feed != nullptr) {

						 List<Comment ^> ^comments = gcnew List<Comment ^>();

						 for each(PicasaEntry ^entry in e->Feed->Entries) {

							 Comment ^comment = gcnew Comment();

							 comment->AtomEntry = entry;

							 comments->Add(comment);
						 }

						 state->commentForm->comments = comments;

					 }
				 } else if(state->type == PicasaAsyncState::OperationType::ADD_COMMENT) {

					 if(e->Error != nullptr) {

						 MessageBox::Show(e->Error->Message, L"Adding Comment Error");
					 }
				 }
			 }

    
	private: System::Void picasaService2_asyncOperationProgress(Object ^sender, ProgressChangedEventArgs ^e) {
			
				 PicasaAsyncState ^state = dynamic_cast<PicasaAsyncState ^>(e->UserState);
				 
				 if(state->type == PicasaAsyncState::OperationType::DELETE_ALBUM) {

					 Album ^album = state->deleteAlbums[state->deletedEntry];

					 albumFeed->Entries->Remove(album->PicasaEntry);
					 displayAlbumFeed();
				
					 imageGrid->removeAllImageData();

				 } else if(state->type == PicasaAsyncState::OperationType::DELETE_PHOTO) {

					 Photo ^photo = state->deleteImages[state->deletedEntry];
					
					 state->progressForm->totalProgressValue = state->deletedEntry + 1;					 					
					 imageGrid->removeImageData(photoToImageGridItem(photo));

					 int nextPhotoNr = state->deletedEntry + 1;
					 if(nextPhotoNr == state->deleteImages->Count) nextPhotoNr--;

					 state->progressForm->itemInfo = state->deleteImages[nextPhotoNr]->Title;

				 }

			 }
    private: System::Void picasaService2_asyncOperationCompleted(Object ^sender, RunWorkerCompletedEventArgs ^e) {

				 PicasaAsyncState ^state = dynamic_cast<PicasaAsyncState ^>(e->Result);

				 if(state->type == PicasaAsyncState::OperationType::DELETE_PHOTO) {

					 state->progressForm->actionFinished();
				 }

			 }


	private: Album ^getAlbum(int index) {

				 Debug::Assert(index < albumListBox->Items->Count);

				 return(dynamic_cast<Album ^>(albumListBox->Items[index]));
			 }
	private: void openAlbum(String ^albumId) {

				 int albumIndex = -1;

				 for(int i = 0; i < albumListBox->Items->Count; i++) {

					 if(getAlbum(i)->Id->Equals(albumId)) {

						 albumIndex = i;
						 break;
					 }
				 }

				 if(albumIndex == -1) return;

				 albumListBox->SelectedIndex = -1;
				 albumListBox->SelectedIndex = albumIndex;

			 }

	private: ImageGridContextMenuStrip ^createContextMenu(Photo ^photo) {

				 ImageGridContextMenuStrip ^contextMenu = gcnew ImageGridContextMenuStrip(imageGrid);

				 ImageGridToolStripMenuItem ^viewItem = gcnew ImageGridToolStripMenuItem();
				 viewItem->Name = L"view";
				 viewItem->Text = L"View";
				 viewItem->OnGridMouseDown += gcnew ImageGridToolStripMenuItem::GridMouseDownEventHandler(this, &PicasaForm::viewToolStripMenuItem_MouseDown);

				 contextMenu->Items->Add(viewItem);

				 contextMenu->Items->Add(gcnew ToolStripSeparator());

				 if(albumListBox->SelectedIndex == -1) {

					 // there is no album currently selected
					 ImageGridToolStripMenuItem ^openAlbum = gcnew ImageGridToolStripMenuItem();
					 openAlbum->Name = L"openAlbum";
					 openAlbum->Text = L"Open Album";
					 openAlbum->OnGridMouseDown += gcnew ImageGridToolStripMenuItem::GridMouseDownEventHandler(this, &PicasaForm::openAlbumToolStripMenuItem_MouseDown);

					 contextMenu->Items->Add(openAlbum);

					 contextMenu->Items->Add(gcnew ToolStripSeparator());
				 }

				 ImageGridToolStripMenuItem ^imageInfo = gcnew ImageGridToolStripMenuItem();
				 imageInfo->Name = L"imageInfo";
				 imageInfo->Text = L"Info";
				 imageInfo->OnGridMouseDown += gcnew ImageGridToolStripMenuItem::GridMouseDownEventHandler(this, &PicasaForm::photoInfoToolStripMenuItem_MouseDown);

				 contextMenu->Items->Add(imageInfo);	

				 ImageGridToolStripMenuItem ^comments = gcnew ImageGridToolStripMenuItem();
				 comments->Name = L"comments";
				 comments->Text = L"Comments";
				 comments->OnGridMouseDown += gcnew ImageGridToolStripMenuItem::GridMouseDownEventHandler(this, &PicasaForm::commentsToolStripMenuItem_MouseDown);

				 contextMenu->Items->Add(comments);

				 contextMenu->Items->Add(gcnew ToolStripSeparator());

				 ImageGridToolStripMenuItem ^selectAll = gcnew ImageGridToolStripMenuItem();
				 selectAll->Name = L"selectAll";
				 selectAll->Text = L"Select All";
				 selectAll->OnGridMouseDown += gcnew ImageGridToolStripMenuItem::GridMouseDownEventHandler(this, &PicasaForm::selectAllToolStripMenuItem_MouseDown);

				 contextMenu->Items->Add(selectAll);	

				 ImageGridToolStripMenuItem ^deselectAll = gcnew ImageGridToolStripMenuItem();
				 deselectAll->Name = L"deselectAll";
				 deselectAll->Text = L"Deselect All";
				 deselectAll->OnGridMouseDown += gcnew ImageGridToolStripMenuItem::GridMouseDownEventHandler(this, &PicasaForm::deselectAllToolStripMenuItem_MouseDown);

				 contextMenu->Items->Add(deselectAll);	

				 if(photo->ReadOnly == false) {

					 contextMenu->Items->Add(gcnew ToolStripSeparator());

					 // there is no album currently selected
					 ImageGridToolStripMenuItem ^deletePhoto = gcnew ImageGridToolStripMenuItem();
					 deletePhoto->Name = L"deletePhoto";
					 deletePhoto->Text = L"Delete";
					 deletePhoto->OnGridMouseDown += gcnew ImageGridToolStripMenuItem::GridMouseDownEventHandler(this, &PicasaForm::deletePhotoToolStripMenuItem_MouseDown);

					 contextMenu->Items->Add(deletePhoto);

				 }

				 return(contextMenu);
			 }

	private: ImageGridItem ^photoToImageGridItem(Photo ^photo) {

				 String ^thumbURL;

				 if(photo->PicasaEntry->Media->Thumbnails->Count == 0) {

					 thumbURL = L"http://www.blairlate.com/thumbnail.png";

				 } if(photo->PicasaEntry->Media->Thumbnails->Count == 1) {

					 thumbURL = photo->PicasaEntry->Media->Thumbnails[0]->Url;

				 } else if(photo->PicasaEntry->Media->Thumbnails->Count > 1) {

					 thumbURL = photo->PicasaEntry->Media->Thumbnails[1]->Url;
				 }

				 ImageGridItem ^item = gcnew ImageGridItem(thumbURL);
				 item->data = photo;						 

				 item->contextMenu = createContextMenu(photo);

				 String ^author = !String::IsNullOrEmpty(photo->Author) ? "Author: " + photo->Author + "\n" : "";
				 String ^summary = !String::IsNullOrEmpty(photo->Summary) ? photo->Summary + "\n" : "";

				 String ^comments = photo->CommentCount > 0 ? "Comments: " + Convert::ToString(photo->CommentCount) + "\n" : "";

				 item->caption = photo->Title + "\n" + author + comments + summary + Convert::ToString(photo->Width) + " x " + Convert::ToString(photo->Height);

				 return(item);
			 }

	private: void displayPhotoFeed() {

				 if(photoFeed == nullptr) return;

				 List<ImageGridItem ^> ^imageData = gcnew List<ImageGridItem ^>();

				 for each(PicasaEntry ^entry in photoFeed->Entries) {

					 Photo ^photo = gcnew Photo();
					 photo->AtomEntry = entry;

					 ImageGridItem ^item = photoToImageGridItem(photo);

					 imageData->Add(item);
				 }

				 imageGrid->initializeImageData(imageData);

			 }

	private: void displayAlbumFeed() {

				 albumListBox->Items->Clear();

				 for each (PicasaEntry ^entry in albumFeed->Entries)
				 {		
					 Album ^album = gcnew Album();
					 album->AtomEntry = entry;					

					 albumListBox->Items->Add(album);						
				 }

			 }

	private: System::Void albumListBox_SelectedIndexChanged(System::Object^  sender, System::EventArgs^  e) {

				 if(albumListBox->SelectedIndex == -1) return;

				 Album ^album = dynamic_cast<Album ^>(albumListBox->SelectedItem);			

				 String ^userName = L"default"; 

				 if(album->PicasaEntry->Authors->Count > 0) {

					 Google::GData::Client::AtomPerson ^person = album->PicasaEntry->Authors[0];

					 String ^uri = person->Uri->ToString();
					 String ^id = uri->Substring(uri->LastIndexOf('/') + 1);

					 if(!String::IsNullOrEmpty(id)) {

						 userName = id;
					 }
				 }

				 photoQuery(userName, album->Id, "");

				 titleInfo = L"- " + userComboBox->SelectedItem->ToString() + " : " + album->Title;

			 }
	private: System::Void userComboBox_SelectedIndexChanged(System::Object^  sender, System::EventArgs^  e) {

				 if(userComboBox->SelectedIndex == -1) return;

				 PicasaUser ^user = dynamic_cast<PicasaUser ^>(userComboBox->SelectedItem);

				 albumQuery(user->name);

			 }

	private: Album ^getSelectedAlbum() {

				 return(dynamic_cast<Album ^>(albumListBox->SelectedItem));
			 }
	private: PicasaUser ^getSelectedUser() {

				 return(dynamic_cast<PicasaUser ^>(userComboBox->SelectedItem));
			 }
	private: System::Void abumInfoToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

				 if(albumListBox->SelectedIndex == -1) return;

				 Album ^album = dynamic_cast<Album ^>(albumListBox->SelectedItem);

				 PicasaAlbumForm ^form = gcnew PicasaAlbumForm();

				 form->album = album;
				 form->Text = L"Picasa Album Info";
				 form->editable = !album->ReadOnly;
				 
				 if(form->ShowDialog() == Windows::Forms::DialogResult::OK) {

					 try {

						 if(!album->ReadOnly) {

							 album->Title = form->albumTitle;
							 album->Summary = form->albumDescription;
							 album->Access = form->albumAccess;

							 PicasaEntry ^entry = album->PicasaEntry;

							 addAuthInfoToEditUri(entry);
							 album->AtomEntry = entry->Update();
						 }

					 } catch(Exception ^ex) {

						 MessageBox::Show(ex->Message);

					 }
				 }
			 }
	
	private: System::Void createAlbumToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

				 PicasaAlbumForm ^form = gcnew PicasaAlbumForm();

				 Album ^album = gcnew Album();
				 album->AtomEntry = gcnew AlbumEntry();
				 album->Access = L"protected";

				 form->album = album;				
				 form->Text = L"Create Picasa Album";
				 form->editable = true;

				 if(form->ShowDialog() == Windows::Forms::DialogResult::OK) {

					 try {
					 
						 album->Title = form->albumTitle;
						 album->Summary = form->albumDescription;
						 album->Access = form->albumAccess;

						 PicasaEntry ^entry = album->PicasaEntry;

						 album->AtomEntry = service->Insert(createPicasaUri(L"default"), entry);
						 
						 albumFeed->Entries->Insert(0, album->AtomEntry);
						 displayAlbumFeed();
						 
					 } catch(Exception ^ex) {

						 MessageBox::Show(ex->Message);

					 }
				 }
			 }

	private: System::Void deleteAlbumToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

				 if(albumListBox->SelectedIndex == -1) return;

				 try {

					 Album ^album = dynamic_cast<Album ^>(albumListBox->SelectedItem);

					 if(MessageBox::Show(L"Are you sure you want to permanently delete the album\n\"" + album->Title +
						 L"\" containing " + album->NumPhotos + L" photos?",
						 L"Delete Album", MessageBoxButtons::YesNo, MessageBoxIcon::Question)
						 == Windows::Forms::DialogResult::Yes) 
					 {

						 PicasaAsyncState ^state = gcnew PicasaAsyncState(PicasaAsyncState::OperationType::DELETE_ALBUM);
						 state->deleteAlbums->Add(album);

						 service2->asyncAction(state);						 

					 }

				 } catch(Exception ^e) {

					 MessageBox::Show(e->Message);
				 }
			 }

	private: System::Void viewToolStripMenuItem_MouseDown(System::Object^  sender, ImageGridMouseEventArgs ^e) {

				 OnViewEvent(this, e);
			 }
	private: System::Void openAlbumToolStripMenuItem_MouseDown(System::Object^  sender, ImageGridMouseEventArgs ^e) {

				 Photo ^photo = dynamic_cast<Photo ^>(e->item->data);

				 if(photo->PicasaEntry->Authors->Count == 0) return;

				 Google::GData::Client::AtomPerson ^person = photo->PicasaEntry->Authors[0];

				 PicasaUser ^user = gcnew PicasaUser(person->Email, L"");

				 int index = userComboBox->Items->IndexOf(user);

				 if(index == -1) {

					 index = userComboBox->Items->Add(user);

				 }

				 userComboBox->SelectedIndex = index;

				 // wait until album query has finished
				 while(states->GetType(PicasaAsyncState::OperationType::ALBUM_QUERY) != nullptr) {

					 Application::DoEvents();
				 }
				 //photoQuery(person->Email, photo->AlbumId, "");
				 openAlbum(photo->AlbumId);
			 }
	private: void deletePhotos(List<ImageGridItem ^> ^selected) {

				 try {

					 List<Photo ^> ^deleteImages = gcnew List<Photo ^>();

					 for(int i = 0; i < selected->Count; i++) {

						 Photo ^photo = dynamic_cast<Photo ^>(selected[i]->data);

						 if(!photo->ReadOnly) {

							deleteImages->Add(photo);

						 } else {

							selected->Remove(selected[i]);
						 }
						
					 }

					 if(deleteImages->Count == 0) return;

					 if(MessageBox::Show(L"Are you sure you want to permanently delete " + selected->Count + L" photo(s)?",
						 L"Delete Photo(s)", MessageBoxButtons::YesNo, MessageBoxIcon::Question)
						 == Windows::Forms::DialogResult::Yes) 
					 {
						 
						 PicasaAsyncState ^state = gcnew PicasaAsyncState(PicasaAsyncState::OperationType::DELETE_PHOTO);
						 state->deleteImages = deleteImages;

						 if(deleteImages->Count > 1) {

							state->progressForm->OnCancelEvent += gcnew ProgressForm::CancelEventHandler(this, &PicasaForm::progressForm_CancelEvent);
							state->progressForm->Show();
						 }

						 state->progressForm->totalProgressMaximum = deleteImages->Count;
						 state->progressForm->itemInfo = deleteImages[0]->Title;

						 service2->asyncAction(state);

					 }

				 } catch(Exception ^e) {

					 MessageBox::Show(e->Message);
				 }

			 }
	
	private: System::Void photoInfoToolStripMenuItem_MouseDown(System::Object^  sender, ImageGridMouseEventArgs ^e) {

				 Photo ^photo = dynamic_cast<Photo ^>(e->item->data);

				 PicasaPhotoForm ^form = gcnew PicasaPhotoForm();
				 form->photo = photo;
				 form->editable = !photo->ReadOnly;

				 if(form->ShowDialog() == Windows::Forms::DialogResult::OK) {

					 try {

						 if(!photo->ReadOnly) {

							 PicasaEntry ^entry = photo->PicasaEntry;
							 entry->Title->Text = form->photoTitle;
							 entry->Summary->Text = form->photoSummary;

							 //entry->Location = gcnew Extensions::Location::GeoRssWhere();
							
							 addAuthInfoToEditUri(entry);
							 photo->AtomEntry = (PicasaEntry ^) entry->Update();
							 
							 ImageGridItem ^updatedPhoto = photoToImageGridItem(photo);

							 imageGrid->replaceImageData(e->imageNr, updatedPhoto);
						
						 }

					 } catch (Exception ^ex) {

						 MessageBox::Show(ex->Message, "Error modifying photo");
					 }
				 }
			 }

	private: System::Void commentsToolStripMenuItem_MouseDown(System::Object^  sender, ImageGridMouseEventArgs ^e) {

				 Photo ^photo = dynamic_cast<Photo ^>(e->item->data);

				 PicasaCommentForm ^commentForm = gcnew PicasaCommentForm();
				 commentForm->photo = photo;
				
				 if(photo->CommentCount > 0) {

					commentForm->commentsText = "Loading comments...";

					 // get photo comments
					String ^userId = getUserIdFromPhoto(photo);
					CommentsQuery ^query = gcnew CommentsQuery(createPicasaUri(userId, photo->AlbumId, photo->Id));

					PicasaAsyncState ^state = gcnew PicasaAsyncState(PicasaAsyncState::OperationType::COMMENT_QUERY);
					state->commentForm = commentForm;

					service->QueryFeedAync(query->Uri, DateTime::MinValue, state);
				 }

				 if(commentForm->ShowDialog() == Windows::Forms::DialogResult::OK) {

					 service2->postComment(commentForm->inputText, photo->AlbumId, photo->Id);
				 }
			 }
	private: String ^getUserIdFromPhoto(Photo ^photo) {

				 String ^id = "";

				 if(photo->PicasaEntry->Authors->Count > 0) {

					 id = photo->PicasaEntry->Authors[0]->Uri->ToString();

				 } else {

					 Album ^album = getSelectedAlbum();

					 if(album != nullptr && album->PicasaEntry->Authors->Count > 0) {

						 id = album->PicasaEntry->Authors[0]->Uri->ToString();
					 }
				 }
				
				 id = id->Substring(id->LastIndexOf('/') + 1);

				 return(id);
			 }
	private: System::Void selectAllToolStripMenuItem_MouseDown(System::Object^  sender, ImageGridMouseEventArgs ^e) {

				 imageGrid->setSelectedForAllImages(true);

			 }
	private: System::Void deselectAllToolStripMenuItem_MouseDown(System::Object^  sender, ImageGridMouseEventArgs ^e) {

				 imageGrid->setSelectedForAllImages(false);

			 }
	private: System::Void deletePhotoToolStripMenuItem_MouseDown(System::Object^  sender, ImageGridMouseEventArgs ^e) {

				 e->item->contextMenu->Hide();

				 List<ImageGridItem ^> ^selected = imageGrid->getSelectedImageData();

				 if(selected->Count == 0) {

					 selected->Add(e->item);
				 }

				 deletePhotos(selected);

			 }
	private: System::Void picasaImage_Click(System::Object^  sender, ImageGridMouseEventArgs^  e) {

				 if(e->Button == System::Windows::Forms::MouseButtons::Left) {

					 imageGrid->toggleImageSelected(e->imageNr);
				 }


			 }

	private: void doSearch() {

				 PicasaUser ^user = getSelectedUser();

				 if(searchComboBox->SelectedIndex == 0) {

					 Album ^album = getSelectedAlbum();

					 if(album == nullptr) return;

					 photoQuery(user->name, album->Id, searchTextBox->Text);

				 } else if(searchComboBox->SelectedIndex == 1) {

					 photoQuery(user->name, nullptr, searchTextBox->Text);

				 } else {

					 photoQuery(nullptr, nullptr, searchTextBox->Text);

				 }

				 titleInfo = L"- Search: " + searchTextBox->Text;

				 albumListBox->SelectedIndex = -1;

			 }

	private: System::Void searchButton_Click(System::Object^  sender, System::EventArgs^  e) {

				 doSearch();
			 }

	private: void setTitle() {

				 String ^title = "Picasa Web ";

				 if(imageGrid->getNrPages() > 0) {

					 title += "(" + Convert::ToString(imageGrid->getCurrentPage() + 1) + "/" + 
						 Convert::ToString(imageGrid->getNrPages()) + ") ";

				 }

				 title += titleInfo;

				 this->Text = title;
			 }


	private: System::Void searchTextBox_KeyDown(System::Object^  sender, System::Windows::Forms::KeyEventArgs^  e) {

				 if(e->KeyCode != System::Windows::Forms::Keys::Enter) return;

				 doSearch();
			 }

	private: System::Void uploadPhotoButton_Click(System::Object^  sender, System::EventArgs^  e) {


				OnUploadEvent(this, gcnew EventArgs());
				 
			 }

	private: System::Void downloadButton_Click(System::Object^  sender, System::EventArgs^  e) {

				 List<ImageGridItem ^> ^items = imageGrid->getSelectedImageData();

				 if(items->Count > 0) {

					 OnDownloadEvent(this, items);
				 }

				 imageGrid->setSelectedForAllImages(false);
			 }
	private: System::Void imageGrid_UpdatePhotosEvent(System::Object^  sender, System::EventArgs^  e) {

				 setTitle();
			 }
	private: System::Void picasaForm_KeyDown(System::Object^  sender, System::Windows::Forms::KeyEventArgs^  e) {

				 if(e->Control == true && e->KeyCode == Keys::A) {

					 imageGrid->setSelectedForAllImages(true);
				 }

				 if(e->KeyCode == Keys::Delete) {					

					 deletePhotos(imageGrid->getSelectedImageData());
				 }
			 }
	private: System::Void loginButton_Click(System::Object^  sender, System::EventArgs^  e) {

				 try {

					PicasaLoginDialog ^login = gcnew PicasaLoginDialog();

					if(login->ShowDialog() == Windows::Forms::DialogResult::Cancel) {

						if(login->authInfo->success == false) return;
						
						service2->authInfo = login->authInfo;

						PicasaUser ^user = gcnew PicasaUser("default", "");

						// if a user already exists with the same name remove it
						for(int i = 0; i < userComboBox->Items->Count; i++) {

							PicasaUser ^item = dynamic_cast<PicasaUser ^>(userComboBox->Items[i]);

							if(item->name->Equals(user->name)) {

								userComboBox->Items->Remove(item);
								break;
							}
						}

						userComboBox->Items->Insert(0, user);

						userComboBox->SelectedIndex = -1;
						userComboBox->SelectedIndex = 0;

					}

				}catch(Exception ^e) {

					MessageBox::Show(e->Message);
				}
			 }
};
}
