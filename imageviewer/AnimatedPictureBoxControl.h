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

	protected: 

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
			this->SuspendLayout();
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
        }

		if(currentlyAnimating == true) {

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
        // Force a call to the Paint event handler. 
        this->Invalidate();
    }

	void centerImageOffset(int destWidth, int destHeight, int %destX, int %destY) {

		int tempX = Width;
		int tempY = Height;

		if(destWidth < Width) {

			destX = (Width - destWidth) / 2;
		}

		if(destHeight < Height) {

			destY = (Height - destHeight) / 2;
		}

	}

protected:

	virtual void OnPaint(PaintEventArgs^ e) override
	{
		// Begin the animation.
		if(currentImage == nullptr) return;

		animateImage();

		int destX = 0;
		int destY = 0;
		int destWidth = currentImage->Width;
		int destHeight = currentImage->Height;

		if(sizeMode == PictureBoxSizeMode::Zoom) {

			ImageUtils::stretchRectangle(currentImage->Width, currentImage->Height, Width, Height, destWidth, destHeight);
			centerImageOffset(destWidth, destHeight, destX, destY);

		} else if(sizeMode == PictureBoxSizeMode::CenterImage) {

			centerImageOffset(destWidth, destHeight, destX, destY);

		} else if(sizeMode == PictureBoxSizeMode::StretchImage) {

			destWidth = Width;
			destHeight = Height;
		}

		Rectangle destRect = Rectangle(destX, destY, destWidth, destHeight);

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

		

		//e->Graphics->DrawImage( currentImage, Point(0,0));
	

	}
public:

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

	};
}
