#pragma once

#include "MediaFileFactory.h"
#include "ImageUtils.h"
#include "Util.h"
#include "ImageStream.h"
#include "MediaFormatConvert.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;


namespace imageviewer {

	/// <summary>
	/// Summary for ImagePanelControl
	/// </summary>
	public ref class ImagePanelControl : public System::Windows::Forms::UserControl
	{
	public:
		ImagePanelControl(void)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//

			leftMouseButtonDown = false;
			isModified = false;

			mediaFileFactory = gcnew MediaFileFactory();
			mediaFileFactory->OpenFinished += gcnew EventHandler<MediaFile ^>(this, &ImagePanelControl::mediaFileFactory_OpenFinished);
			media = nullptr;

			displayMode = DisplayModeState::NORMAL;
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~ImagePanelControl()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::Panel^  panel;
	protected: 

	private: System::Windows::Forms::PictureBox^  pictureBox;
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
			this->panel = (gcnew System::Windows::Forms::Panel());
			this->pictureBox = (gcnew System::Windows::Forms::PictureBox());
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
			this->panel->Size = System::Drawing::Size(578, 498);
			this->panel->TabIndex = 0;
			this->panel->MouseMove += gcnew System::Windows::Forms::MouseEventHandler(this, &ImagePanelControl::imagePanel_MouseMove);
			this->panel->MouseDown += gcnew System::Windows::Forms::MouseEventHandler(this, &ImagePanelControl::imagePanel_MouseDown);
			this->panel->MouseUp += gcnew System::Windows::Forms::MouseEventHandler(this, &ImagePanelControl::imagePanel_MouseUp);
			// 
			// pictureBox
			// 
			this->pictureBox->Location = System::Drawing::Point(0, 0);
			this->pictureBox->Name = L"pictureBox";
			this->pictureBox->Size = System::Drawing::Size(578, 498);
			this->pictureBox->SizeMode = System::Windows::Forms::PictureBoxSizeMode::AutoSize;
			this->pictureBox->TabIndex = 0;
			this->pictureBox->TabStop = false;
			this->pictureBox->MouseMove += gcnew System::Windows::Forms::MouseEventHandler(this, &ImagePanelControl::imagePanel_MouseMove);
			this->pictureBox->MouseDown += gcnew System::Windows::Forms::MouseEventHandler(this, &ImagePanelControl::imagePanel_MouseDown);
			this->pictureBox->MouseUp += gcnew System::Windows::Forms::MouseEventHandler(this, &ImagePanelControl::imagePanel_MouseUp);
			// 
			// ImagePanelControl
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->Controls->Add(this->panel);
			this->Name = L"ImagePanelControl";
			this->Size = System::Drawing::Size(578, 498);
			this->panel->ResumeLayout(false);
			this->panel->PerformLayout();
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->pictureBox))->EndInit();
			this->ResumeLayout(false);

		}
#pragma endregion

public:

	enum class DisplayModeState {
		NORMAL,
		SCALED
	};

	enum class CropStageState {
		DISABLED,
		ENABLED,
		START_PRESSED,
		START_RELEASED
	};

private:

	MediaFileFactory ^mediaFileFactory;
	MediaFile ^media;

	Image ^sourceImage;
	ImageFormat ^imageFormat;

	DisplayModeState displayMode;

	bool leftMouseButtonDown;
	bool isModified;

	Point startMouseLocation;
	Point scrollbarPosition;

	delegate void loadImageDelegate(MediaFile ^media);

	void mediaFileFactory_OpenFinished(System::Object ^sender, MediaFile ^media) {

		array<Object ^> ^args = gcnew array<Object ^>(1);
		args[0] = media;

		this->Invoke(gcnew loadImageDelegate(this, &ImagePanelControl::loadImage), args);		

	}

	void loadImage(MediaFile ^media) {

		try {

			this->media = media;

			if(String::IsNullOrEmpty(media->Location) || 
				media->MediaFormat != MediaFile::MediaType::IMAGE) 
			{

				clearImage();
				return;

			} else if(!media->OpenSuccess) {

				MessageBox::Show("Failed to open:\n\n" + media->Location + "\n\n" + media->OpenError->Message, "Error");
				return;
			}

			imageFormat = MediaFormatConvert::mimeTypeToImageFormat(media->MimeType);

			sourceImage = gcnew Bitmap(media->Data);
			
			displayAndCenterImage(sourceImage);
			LoadImageFinished(this, EventArgs::Empty);

		} catch(Exception ^e) {

			MessageBox::Show("Error reading:\n\n" + media->Location + "\n\n" + e->Message, "Error");
			
		} finally {

			media->close();
			mediaFileFactory->releaseNonBlockingOpenLock();			
		}

	}

	void displayAndCenterImage(Image ^image) {

		Drawing::Size maxDim = panel->Size;		
		Drawing::Size imageSize = image->Size;

		if(DisplayMode == DisplayModeState::SCALED) {

			int scaledWidth, scaledHeight;
			ImageUtils::resizeRectangle(image->Width, image->Height, maxDim.Width, maxDim.Height, scaledWidth, scaledHeight);

			imageSize.Width = scaledWidth;
			imageSize.Height = scaledHeight;
		} 	

		Drawing::Size autoScrollMinSize;

		if(imageSize.Width > maxDim.Width) {

			autoScrollMinSize.Width = imageSize.Width + 1;
		}

		if(imageSize.Height > maxDim.Height) {

			autoScrollMinSize.Height = imageSize.Height + 1;
		}

		panel->AutoScrollMinSize = autoScrollMinSize;

		VScrollProperties ^vscroll = panel->VerticalScroll::get();
		HScrollProperties ^hscroll = panel->HorizontalScroll::get();

		panel->AutoScroll = false;
		vscroll->Value = 0;
		hscroll->Value = 0;
		panel->AutoScroll = true;

		int offsetX = 0;
		int offsetY = 0;

		if(imageSize.Width < maxDim.Width) {

			offsetX = (maxDim.Width - imageSize.Width) / 2;
		}

		if(imageSize.Height < maxDim.Height) {

			offsetY = (maxDim.Height - imageSize.Height) / 2;
		}

		pictureBox->Location = Drawing::Point(offsetX, offsetY);
		pictureBox->Image = ImageUtils::resizeImage(image, imageSize.Width, imageSize.Height);
	}

	void imagePanel_MouseDown(Object ^Sender, MouseEventArgs ^e) {

		if(e->Button == System::Windows::Forms::MouseButtons::Left) {
/*
			Control ^sender = dynamic_cast<Control ^>(Sender);

			if(cropStage == CropStage::ENABLED) {

				cropStart = sender->PointToScreen(e->Location);	
				cropStage = CropStage::START_PRESSED;
				return;

			} else if(cropStage == CropStage::START_RELEASED) {

				cropImage(getCropRectangle(cropStart, sender->PointToScreen(e->Location), true));
				cropStage = CropStage::DISABLED;
				return;
			}
*/
			leftMouseButtonDown = true;

			startMouseLocation = Cursor::get()->Position;

			VScrollProperties ^vscroll = panel->VerticalScroll::get();
			scrollbarPosition.Y = vscroll->Value;

			HScrollProperties ^hscroll = panel->HorizontalScroll::get();
			scrollbarPosition.X = hscroll->Value;

		}
/*
		if(e->Button == System::Windows::Forms::MouseButtons::Right) {

			if(cropStage == CropStage::START_PRESSED || cropStage == CropStage::START_RELEASED) {

				cropStage = CropStage::ENABLED;

			} else if(cropStage == CropStage::ENABLED) {

				cropStage = CropStage::DISABLED;
			}
		}
*/
	}

	void imagePanel_MouseMove(Object ^Sender, MouseEventArgs ^e) {

		if(leftMouseButtonDown) {

			float ratioY = ImageSize.Height / float(panel->Size.Height);
			float ratioX = ImageSize.Width / float(panel->Size.Width);

			Point imageLocation = Cursor::get()->Position;

			Point delta;
			delta.X = int((imageLocation.X - startMouseLocation.X) * -ratioX);
			delta.Y = int((imageLocation.Y - startMouseLocation.Y) * -ratioY);

			VScrollProperties ^vscroll = panel->VerticalScroll::get();
			HScrollProperties ^hscroll = panel->HorizontalScroll::get();

			if(vscroll->Visible == true) {

				int newPos = scrollbarPosition.Y + delta.Y;
				vscroll->Value = CLAMP(newPos, vscroll->Minimum, vscroll->Maximum);

			}

			if(hscroll->Visible == true) {

				int newPos = scrollbarPosition.X + delta.X;
				hscroll->Value = CLAMP(newPos, hscroll->Minimum, hscroll->Maximum);

			}

		}
/*
		if(cropStage == CropStage::START_PRESSED || cropStage == CropStage::START_RELEASED) {

			if(frame.X != -1) {

				ControlPaint::DrawReversibleFrame( frame, Color::White, FrameStyle::Dashed );
			}

			Control ^sender = dynamic_cast<Control ^>(Sender);

			Point end = sender->PointToScreen(e->Location);

			frame = getCropRectangle(cropStart, end, false);

			ControlPaint::DrawReversibleFrame( frame, Color::White, FrameStyle::Dashed );

		}
*/
	}

	void imagePanel_MouseUp(Object ^Sender, MouseEventArgs ^e) {

		if(e->Button == System::Windows::Forms::MouseButtons::Left) {

			leftMouseButtonDown = false;
/*
			if(cropStage == CropStage::START_PRESSED) {

				cropStage = CropStage::START_RELEASED;
			}
*/
		}
	}

	void clearImage() {

		if(sourceImage != nullptr) {

			delete sourceImage;
			sourceImage = nullptr;
		}

		if(pictureBox->Image != nullptr) {

			delete pictureBox->Image;
			pictureBox->Image = nullptr;
		}

		panel->VerticalScroll::get()->Visible = false;
		panel->HorizontalScroll::get()->Visible = false;
		
	}


public:

	event EventHandler<EventArgs ^> ^LoadImageFinished;
	event EventHandler<GEventArgs<bool> ^> ^Modified;

	property bool IsWebImage {

		bool get() {

			if(media == nullptr) return(false);
			
			return(Util::isUrl(media->Location));
		}
	}

	property String ^ImageLocation {

		String ^get() {

			if(IsEmpty) return("");
			
			return(media->Location);
		}
	}

	property bool IsModified {

		bool get() {

			return(false);
		}
	}

	property bool IsEmpty {

		bool get() {

			return(pictureBox->Image == nullptr ? true : false);
		}
	}

	property CropStageState CropStage {

		CropStageState get() {

			return(CropStageState::DISABLED);
		}

		void set(CropStageState stage) {

		}

	}

	property Drawing::Size ImageSize {

		Drawing::Size get() {

			if(IsEmpty) {
				
				return(Drawing::Size(0,0));
			}

			Drawing::Size imageSize(pictureBox->Image->Width, pictureBox->Image->Height);

			return(imageSize);
		}

	}

	property DisplayModeState DisplayMode {

		DisplayModeState get() {

			return(displayMode);
		}

		void set(DisplayModeState displayMode) {

			this->displayMode = displayMode;

			if(!IsEmpty) {

				displayAndCenterImage(sourceImage);
			}
		}

	}


	void loadImage(String ^fileLocation) {
		
		clearImage();

		mediaFileFactory->openNonBlockingAndCancelPending(fileLocation);

	}

	void rotateFlipImage(System::Drawing::RotateFlipType type) {

		Image^ temp = pictureBox->Image;

		sourceImage->RotateFlip(type);
		temp->RotateFlip(type);

		displayAndCenterImage(temp);

		//IsModified = true;
	}

	void saveImageToDisk(String ^fileName) {

		pictureBox->Image->Save(fileName);
	}

	ImageStream ^saveImageToImageStream() {

		Stream ^stream = gcnew MemoryStream();

		pictureBox->Image->Save(stream, imageFormat);

		stream->Seek(0, SeekOrigin::Begin);

		ImageStream ^imageStream = gcnew ImageStream(media->Name, media->MimeType, stream);

		return(imageStream);
	}

	
	void resizeImage(int width, int height) {

		if(sourceImage == nullptr) return;

/*
		if(width == -2 && height == -2) {

			width = sourceBitmap->Width;
			height = sourceBitmap->Height;

		} else if(width == -1 && height == -1) {

			scaleToFitPanel(sourceBitmap->Width, sourceBitmap->Height, 1, width, height);

		} else if(width == -1) {

			width = getScaledWidth(curSize.Width, curSize.Height, height);

		} else if(height == -1) {

			height = getScaledHeight(curSize.Width, curSize.Height, width);
		}
*/
		if(width != ImageSize.Width || height != ImageSize.Height) {

			Image ^resizedImage = ImageUtils::resizeImage(sourceImage, width, height);

			if(resizedImage != nullptr) {
				isModified = true;
				displayAndCenterImage(resizedImage);
			}
		}

		//centerImage();			

	}

	void zoomImage(float scale) {

		if(IsEmpty) return;

		Drawing::Size curSize = ImageSize;
		Drawing::Size newSize;

		newSize.Width = CLAMP(int(curSize.Width * scale), 1, MAX_IMAGE_SIZE);
		newSize.Height = CLAMP(int(curSize.Height * scale), 1, MAX_IMAGE_SIZE);

		resizeImage(newSize.Width, newSize.Height);

	}



};



}
