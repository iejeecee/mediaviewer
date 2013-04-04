#pragma once

#include "MediaFileFactory.h"
#include "AnimatedPictureBoxControl.h"
//#include "TransparentIconPanel.h"
#include "ImageGridItem.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;
using namespace System::Drawing::Imaging;


namespace imageviewer {

	/// <summary>
	/// Summary for MediaPreviewControl
	/// </summary>
	public ref class MediaPreviewControl : public System::Windows::Forms::UserControl
	{
	public:
		MediaPreviewControl(void)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
			mediaFileFactory = gcnew MediaFileFactory();
			mediaFileFactory->OpenFinished += gcnew EventHandler<MediaFile ^>(this, &MediaPreviewControl::mediaFileFactory_OpenFinished);
			media = nullptr;

			informImage = gcnew List<Image ^>();
			informImage->Add(gcnew Bitmap("C:\\game\\icons\\loading.gif"));
			informImage->Add(gcnew Bitmap("c:\\game\\icons\\error.png"));
			
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~MediaPreviewControl()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::Panel^  panel;
	protected: 


	private: System::Windows::Forms::ImageList^  imageList;

	private: imageviewer::AnimatedPictureBoxControl^  pictureBox;




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
			this->panel = (gcnew System::Windows::Forms::Panel());
			this->pictureBox = (gcnew imageviewer::AnimatedPictureBoxControl());
			this->panel->SuspendLayout();
			this->SuspendLayout();
			// 
			// panel
			// 
			this->panel->Controls->Add(this->pictureBox);
			this->panel->Dock = System::Windows::Forms::DockStyle::Fill;
			this->panel->Location = System::Drawing::Point(0, 0);
			this->panel->Name = L"panel";
			this->panel->Size = System::Drawing::Size(336, 348);
			this->panel->TabIndex = 0;
			// 
			// pictureBox
			// 
			this->pictureBox->Dock = System::Windows::Forms::DockStyle::Fill;
			this->pictureBox->Image = nullptr;
			this->pictureBox->Location = System::Drawing::Point(0, 0);
			this->pictureBox->LowerColor = System::Drawing::Color::FromArgb(static_cast<System::Int32>(static_cast<System::Byte>(200)), static_cast<System::Int32>(static_cast<System::Byte>(200)), 
				static_cast<System::Int32>(static_cast<System::Byte>(200)));
			this->pictureBox->Name = L"pictureBox";
			this->pictureBox->Size = System::Drawing::Size(336, 348);
			this->pictureBox->SizeMode = System::Windows::Forms::PictureBoxSizeMode::Zoom;
			this->pictureBox->TabIndex = 0;
			this->pictureBox->TransparencyEnabled = false;
			this->pictureBox->UpperColor = System::Drawing::Color::FromArgb(static_cast<System::Int32>(static_cast<System::Byte>(255)), static_cast<System::Int32>(static_cast<System::Byte>(255)), 
				static_cast<System::Int32>(static_cast<System::Byte>(255)));
			this->pictureBox->MouseDown += gcnew System::Windows::Forms::MouseEventHandler(this, &MediaPreviewControl::pictureBox_MouseDown);
			// 
			// MediaPreviewControl
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->Controls->Add(this->panel);
			this->Name = L"MediaPreviewControl";
			this->Size = System::Drawing::Size(336, 348);
			this->panel->ResumeLayout(false);
			this->ResumeLayout(false);

		}
#pragma endregion

private:

	static log4net::ILog ^log = log4net::LogManager::GetLogger(System::Reflection::MethodBase::GetCurrentMethod()->DeclaringType);

	enum class InformImage {

		LOADING_IMAGE = 0,
		ERROR_IMAGE = 1,		
	};
	
	MediaFileFactory ^mediaFileFactory;
	MediaFile ^media;
	List<Image ^> ^informImage;

	delegate void loadPreviewDelegate(MediaFile ^media, List<MetaDataThumb ^> ^thumbs);

	void mediaFileFactory_OpenFinished(System::Object ^sender, MediaFile ^media) {

		array<Object ^> ^args = gcnew array<Object ^>(2);

		args[0] = media;
		args[1] = gcnew List<MetaDataThumb ^>();

		try {

			// grab or generate thumbnail images
			List<MetaDataThumb ^> ^thumbs = nullptr;

			if(media->MetaData != nullptr && media->MetaData->Thumbnail->Count > 0) {

				thumbs = media->MetaData->Thumbnail;

			} else {

				thumbs = media->generateThumbnails();
			}

			args[1] = thumbs;			

		} catch (Exception ^e) {

			log->Error("Error generating thumbnails", e);
			media->OpenError = e;

		} finally {

			if(!this->IsDisposed) {

				this->Invoke(gcnew loadPreviewDelegate(this, &MediaPreviewControl::loadPreview), args);		
			}
		}

	}
	
	void loadPreview(MediaFile ^media, List<MetaDataThumb ^> ^thumbs) {

		try {

			this->media = media;

			clearPictureBox();

			if(!media->OpenSuccess) {
			
				if(media->OpenError->GetType() != MediaFileException::typeid) {
					setPictureBoxInformImage(InformImage::ERROR_IMAGE);
				}

			} else if(String::IsNullOrEmpty(media->Location) || media->MediaFormat == MediaFile::MediaType::UNKNOWN) {
			
				// empty file
				return;
				
			} else if(thumbs->Count > 0) {

				setPictureBoxImage(thumbs[0]->ThumbImage);
				
			} 

			setPictureBoxFeatures(media);

		} catch(Exception ^e) {

			log->Error("Error opening preview", e);
			
		} finally {

			media->close();

			// release the lock on opening of images
			mediaFileFactory->releaseNonBlockingOpenLock();
		}

	}

	void setPictureBoxFeatures(MediaFile ^media) {

		ImageGridItem ^item = dynamic_cast<ImageGridItem ^>(media->UserState);

		if(item->InfoIconMode == ImageGridItem::InfoIconModes::DEFAULT_ICONS_ONLY ||
			item->InfoIconMode == ImageGridItem::InfoIconModes::SHOW_ALL_ICONS)
		{
			InfoIcon ^icon = gcnew InfoIcon(media->MimeType);
			icon->Caption = media->getDefaultFormatCaption();
			pictureBox->addInfoIcon(icon);

			if(media->MetaData == nullptr) {

				icon = gcnew InfoIcon(InfoIcon::IconType::ERROR);
				icon->Caption = "Cannot read metadata";

				pictureBox->addInfoIcon(icon);

			} else if(media->MetaData->HasGeoTag) {

				icon = gcnew InfoIcon(InfoIcon::IconType::GEOTAG);
				icon->Caption = "Geo Tag";
				pictureBox->addInfoIcon(icon);
			}

			// if media is a video and has no audio add a muted icon
			if(media->MediaFormat == MediaFile::MediaType::VIDEO) {

				VideoFile ^video = dynamic_cast<VideoFile ^>(media);

				if(video->HasAudio == false) {
					
					icon = gcnew InfoIcon(InfoIcon::IconType::MUTE);
					icon->Caption = "Video contains no audio";
					pictureBox->addInfoIcon(icon);
					
				}

			}
		}

		if(item->InfoIconMode == ImageGridItem::InfoIconModes::CUSTOM_ICONS_ONLY ||
			item->InfoIconMode == ImageGridItem::InfoIconModes::SHOW_ALL_ICONS)
		{

			for(int i = 0; i < item->InfoIcon->Count; i++) {

				pictureBox->addInfoIcon(item->InfoIcon[i]);
			}

		}

		if(String::IsNullOrEmpty(item->Caption)) {

			pictureBox->Caption = media->getDefaultCaption();

		} else {

			pictureBox->Caption = item->Caption;			
		}

		pictureBox->ContextMenuStrip = item->ContextMenu;

	}

	void setPictureBoxImage(Image ^image) {

		pictureBox->SizeMode = PictureBoxSizeMode::Zoom;
		pictureBox->TransparencyEnabled = false;
		pictureBox->Image = image;

	}

	void setPictureBoxInformImage(InformImage image) {

		clearPictureBox();

		pictureBox->SizeMode = PictureBoxSizeMode::CenterImage;
		pictureBox->TransparencyEnabled = true;
		pictureBox->Image = informImage[(int)image];

		
	}

	void clearPictureBox() {

		if(pictureBox->Image != nullptr) {

			if(!informImage->Contains(pictureBox->Image)) {

				delete pictureBox->Image;
			}
			pictureBox->Image = nullptr;
			pictureBox->clearInfoIcons();
			pictureBox->Caption = "";
			pictureBox->ContextMenuStrip = nullptr;
			
		}		
	}

	


protected:



public:

	event EventHandler<MouseEventArgs ^> ^PreviewMouseDown;
	event EventHandler<MouseEventArgs ^> ^PreviewMouseDoubleClick;


	property String ^Location {

		String ^get() {

			if(media == nullptr) return("");
			else return(media->Location);
		}

	}

	property bool IsEmpty {

		bool get() {

			return(pictureBox->Image == nullptr ? true : false);
		}

	}

	void loadMedia(String ^fileLocation, ImageGridItem ^data) {
		
		if(String::IsNullOrEmpty(fileLocation)) {

			clearPictureBox();

		} else {

			log->Info("Opening media: " + fileLocation);
			setPictureBoxInformImage(InformImage::LOADING_IMAGE);
		}
		//pictureBox->Image = miscImageList->Images[0];

		mediaFileFactory->openNonBlockingAndCancelPending(fileLocation, data);

	}



private: System::Void pictureBox_MouseDown(System::Object^  sender, System::Windows::Forms::MouseEventArgs^  e) {

			 PreviewMouseDown(this, e);
			 

		 }



};
}
