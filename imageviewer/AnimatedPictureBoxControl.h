#pragma once

#include "ImageUtils.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;
using namespace System::Drawing::Imaging;


namespace imageviewer {

	/// <summary>
	/// Summary for AnimatedPictureBoxControl
	/// </summary>
	public ref class AnimatedPictureBoxControl : public System::Windows::Forms::UserControl
	{
	public:
		AnimatedPictureBoxControl(void)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
			currentlyAnimating = false;
			currentImage = nullptr;

			sizeMode = PictureBoxSizeMode::Normal;
			transparencyEnabled = false;

			imageAttr = gcnew ImageAttributes();

			lowerColor = Color::FromArgb( 255, 255, 255 );
			upperColor = Color::FromArgb( 255, 255, 255 );

			infoIcon = gcnew array<int>(nrIcons);
			clearInfoIcons();
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~AnimatedPictureBoxControl()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::ImageList^  imageList;
	protected: 

	protected: 
	private: System::ComponentModel::IContainer^  components;

	protected: 

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
			System::ComponentModel::ComponentResourceManager^  resources = (gcnew System::ComponentModel::ComponentResourceManager(AnimatedPictureBoxControl::typeid));
			this->imageList = (gcnew System::Windows::Forms::ImageList(this->components));
			this->SuspendLayout();
			// 
			// imageList
			// 
			this->imageList->ImageStream = (cli::safe_cast<System::Windows::Forms::ImageListStreamer^  >(resources->GetObject(L"imageList.ImageStream")));
			this->imageList->TransparentColor = System::Drawing::Color::Transparent;
			this->imageList->Images->SetKeyName(0, L"264.png");
			this->imageList->Images->SetKeyName(1, L"AVI.ico");
			this->imageList->Images->SetKeyName(2, L"MOV.ico");
			this->imageList->Images->SetKeyName(3, L"MP4.ico");
			this->imageList->Images->SetKeyName(4, L"WMV.ico");
			this->imageList->Images->SetKeyName(5, L"ASF.ico");
			this->imageList->Images->SetKeyName(6, L"BMP.ico");
			this->imageList->Images->SetKeyName(7, L"GIF.ico");
			this->imageList->Images->SetKeyName(8, L"JPG.ico");
			this->imageList->Images->SetKeyName(9, L"PNG.ico");
			this->imageList->Images->SetKeyName(10, L"TIFF.ico");
			// 
			// AnimatedPictureBoxControl
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->DoubleBuffered = true;
			this->Name = L"AnimatedPictureBoxControl";
			this->Size = System::Drawing::Size(371, 336);
			this->ResumeLayout(false);

		}
#pragma endregion
	
private:

	bool currentlyAnimating;
	System::Drawing::Image ^currentImage;
	PictureBoxSizeMode sizeMode;
	bool transparencyEnabled;
	ImageAttributes^ imageAttr;

	Color lowerColor;
	Color upperColor;

	const static int nrIcons = 6;
	array<int> ^infoIcon;

    void animateImage() 
    {
        // Begin the animation only once. 
        // Make sure to animate only if animatedImage was 
        // successfully initialised. 
		if (currentImage != nullptr && !currentlyAnimating)
        {
			if(!ImageAnimator::CanAnimate(currentImage)) return;

			ImageAnimator::Animate(currentImage,
                gcnew EventHandler(this, &AnimatedPictureBoxControl::onFrameChanged));
            currentlyAnimating = true;

        } else if(currentlyAnimating == true) {

			// Get the next frame ready for rendering.
			ImageAnimator::UpdateFrames();
		}
    }

	void stopAnimateImage() {

		if(currentImage != nullptr && currentlyAnimating) {

			ImageAnimator::StopAnimate(currentImage,
                gcnew EventHandler(this, &AnimatedPictureBoxControl::onFrameChanged));
			currentlyAnimating = false;
		}

	}

    void onFrameChanged(Object^ , EventArgs^ ) 
    {
        this->Invalidate();
    }

	Rectangle getIconCanvas(int iconNr) {

		int width = Width / nrIcons;
		int height = Height / nrIcons;

		int xPos = width * iconNr;
		int yPos = Height - height;

		Rectangle dest = Rectangle(xPos, yPos, width, height);
		Rectangle canvas;

		canvas.Width = int(dest.Width  * 0.9);
		canvas.Height = int(dest.Height * 0.9);
		
		canvas = ImageUtils::centerRectangle(dest, canvas);

		return(canvas);
	}

protected: 

	virtual void OnPaint(PaintEventArgs^ e) override
	{		

		UserControl::OnPaint(e);

		if(currentImage == nullptr) return;

		e->Graphics->InterpolationMode = InterpolationMode::HighQualityBicubic;

		// Begin the animation.
		animateImage();

		Rectangle canvasRect, destRect;
		canvasRect.Width = int(Width  * 0.9);
		canvasRect.Height = int(Height * 0.9);
		
		canvasRect = ImageUtils::centerRectangle(Rectangle(0,0, Width, Height), canvasRect);

		if(sizeMode == PictureBoxSizeMode::Zoom) {

			int destWidth, destHeight;

			ImageUtils::stretchRectangle(currentImage->Width, currentImage->Height, 
				canvasRect.Width, canvasRect.Height, destWidth, destHeight);
			
			destRect = ImageUtils::centerRectangle(canvasRect, Rectangle(0,0, destWidth, destHeight));

		} else if(sizeMode == PictureBoxSizeMode::CenterImage) {

			destRect = ImageUtils::centerRectangle(canvasRect, Rectangle(0,0, currentImage->Width, currentImage->Height));

		} else if(sizeMode == PictureBoxSizeMode::StretchImage) {

			destRect = canvasRect;
		}

		// Draw the next frame in the animation.
		if(TransparencyEnabled == true) {
		
			imageAttr->SetColorKey( lowerColor, upperColor, ColorAdjustType::Default );
			
			e->Graphics->DrawImage( currentImage, destRect, 0, 0,
				currentImage->Width, currentImage->Height, GraphicsUnit::Pixel, 
				imageAttr );

		} else {

			//Pen^ blackPen = gcnew Pen( Color::Black,3.0f );
			//e->Graphics->DrawRectangle( blackPen, Rectangle(0,0,Width, Height));

			e->Graphics->DrawImage( currentImage, destRect, 0, 0,
				currentImage->Width, currentImage->Height, GraphicsUnit::Pixel);
					
		}

		//System::Drawing::Image ^test = gcnew Bitmap("C:\\game\\icons\\Button-Info-icon24.png");
		
		for(int i = 0; i < nrIcons; i++) {

			if(infoIcon[i] == -1) continue;

			System::Drawing::Image ^icon = imageList->Images[infoIcon[i]];

			canvasRect = getIconCanvas(i);

			int scaledWidth, scaledHeight;

			ImageUtils::stretchRectangle(icon->Width, icon->Height, canvasRect.Width,
				canvasRect.Height, scaledWidth, scaledHeight);

			destRect = ImageUtils::centerRectangle(canvasRect, Rectangle(0,0, scaledWidth, scaledHeight));

			imageAttr->SetColorKey( upperColor, upperColor, ColorAdjustType::Default );
	
			e->Graphics->DrawImage( icon, destRect, 0, 0,
				icon->Width, icon->Height, GraphicsUnit::Pixel);

			//e->Graphics->DrawIcon(, destRect);
		}

			

		//PaintFinished(this, gcnew EventArgs());

		//e->Graphics->DrawImage( currentImage, Point(0,0));
	

	}
public:

	event EventHandler<EventArgs ^> ^PaintFinished;

	property Image ^AnimatedPictureBoxControl::Image {

		void set(System::Drawing::Image ^image) {

			stopAnimateImage();
			currentImage = image;
			this->Invalidate();
		}

		System::Drawing::Image ^get() {

			return(currentImage);
		}
	}
	
	property PictureBoxSizeMode SizeMode {


		void set(PictureBoxSizeMode sizeMode) {

			this->sizeMode = sizeMode;
		}

		PictureBoxSizeMode get() {

			return(sizeMode);
		}
	}
		
	property bool TransparencyEnabled {


		void set(bool transparencyEnabled) {

			this->transparencyEnabled = transparencyEnabled;
		}

		bool get() {

			return(transparencyEnabled);
		}
	}

	property Color LowerColor {

		void set(Color lowerColor) {

			this->lowerColor = lowerColor;
		}

		Color get() {

			return(lowerColor);
		}
	}

	property Color UpperColor {

		void set(Color upperColor) {

			this->upperColor = upperColor;
		}

		Color get() {

			return(upperColor);
		}
	}

	void addInfoIcon(int iconNr) {

		for(int i = nrIcons -1; i >= 0; i--) {

			if(infoIcon[i] == -1) {

				infoIcon[i] = iconNr;
				break;
			}
		}

	}

	void clearInfoIcons() {

		for(int i = 0; i < nrIcons; i++) {

			infoIcon[i] = -1;
		}
	}

	};
}
