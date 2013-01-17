#pragma once

#include "MediaFileFactory.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;


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

			caption = "";
			
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
	private: System::Windows::Forms::PictureBox^  pictureBox;
	private: System::Windows::Forms::ToolTip^  toolTip;
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
			this->panel = (gcnew System::Windows::Forms::Panel());
			this->pictureBox = (gcnew System::Windows::Forms::PictureBox());
			this->toolTip = (gcnew System::Windows::Forms::ToolTip(this->components));
			this->panel->SuspendLayout();
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->pictureBox))->BeginInit();
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
			this->pictureBox->Location = System::Drawing::Point(16, 22);
			this->pictureBox->Name = L"pictureBox";
			this->pictureBox->Size = System::Drawing::Size(305, 299);
			this->pictureBox->SizeMode = System::Windows::Forms::PictureBoxSizeMode::Zoom;
			this->pictureBox->TabIndex = 0;
			this->pictureBox->TabStop = false;
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
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->pictureBox))->EndInit();
			this->ResumeLayout(false);

		}
#pragma endregion

private:
	
	MediaFileFactory ^mediaFileFactory;
	MediaFile ^media;

	String ^caption;

	delegate void loadPreviewDelegate(MediaFile ^media, List<MetaDataThumb ^> ^thumbs);

	void mediaFileFactory_OpenFinished(System::Object ^sender, MediaFile ^media) {

		array<Object ^> ^args = gcnew array<Object ^>(2);

		args[0] = media;
		args[1] = gcnew List<MetaDataThumb ^>();

		try {

			// grab or generate thumbnail images
			List<MetaDataThumb ^> ^thumbs = nullptr;

			if(media->MetaData != nullptr) {

				if(media->MetaData->Thumbnail->Count > 0) {

					thumbs = media->MetaData->Thumbnail;

				} else {

					thumbs = media->generateThumbnails();
				}

			} else {

				thumbs = media->generateThumbnails();
			}

			args[1] = thumbs;			

		} catch (Exception ^) {

		} finally {

			this->Invoke(gcnew loadPreviewDelegate(this, &MediaPreviewControl::loadPreview), args);		
		}

	}
	
	void loadPreview(MediaFile ^media, List<MetaDataThumb ^> ^thumbs) {

		try {

			this->media = media;

			clearPictureBox();

			if(String::IsNullOrEmpty(media->Location) || !media->OpenSuccess ||
				media->MediaFormat == MediaFile::MediaType::UNKNOWN) 
			{
				return;
			} 

			if(thumbs->Count > 0) {

				pictureBox->Image = thumbs[0]->ThumbImage;

			} 
			
			if(String::IsNullOrEmpty(caption)) {

				setToolTip(media->getDefaultCaption());

			} else {

				setToolTip(caption);			
			}

			media->Data->Close();
			
		} catch(Exception ^) {

			
		} finally {

			// release the lock on opening of images
			mediaFileFactory->releaseOpenLock();
		}

	}

	void setToolTip(String ^text) {

		toolTip->SetToolTip(pictureBox, text);
	}

	void clearPictureBox() {

		if(pictureBox->Image != nullptr) {

			delete pictureBox->Image;
			pictureBox->Image = nullptr;
			setToolTip("");
		}		
	}
protected:

	
	 virtual void OnSizeChanged(EventArgs ^e) override {

		 Control::OnSizeChanged(e);

		 pictureBox->Width = int(panel->Width  * 0.9);
		 pictureBox->Height = int(panel->Height * 0.9);

		 int locationX = (panel->Width - pictureBox->Width) / 2;
		 int locationY = (panel->Height - pictureBox->Height) / 2;

		 pictureBox->Location =  System::Drawing::Point(locationX, locationY);

	 }


public:

	event EventHandler<MouseEventArgs ^> ^PreviewMouseDown;
	event EventHandler<MouseEventArgs ^> ^PreviewMouseDoubleClick;

	enum class DisplayMode {
		NORMAL,
		THUMBNAIL

	};

	property String ^ToolTip {

		void set(String ^caption) {
			
			this->caption = caption;
		}

	}

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

	void loadMedia(String ^fileLocation, DisplayMode mode) {
		
		clearPictureBox();

		mediaFileFactory->open(fileLocation, mode);

	}



private: System::Void pictureBox_MouseDown(System::Object^  sender, System::Windows::Forms::MouseEventArgs^  e) {

			 PreviewMouseDown(this, e);
			 

		 }

};
}
