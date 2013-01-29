#pragma once
//http://www.leghumped.com/blog/2008/05/17/repeated-mousehover-events-in-c/
#include "ImageUtils.h"
#include "InfoIcon.h"
#include "Util.h"

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

			infoIcon = gcnew array<InfoIcon ^>(nrIcons);
			clearInfoIcons();

			infoIconsEnabled = false;

			activeToolTip = -1;
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
	private: System::Windows::Forms::ToolTip^  toolTip;
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
			this->toolTip = (gcnew System::Windows::Forms::ToolTip(this->components));
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
			this->imageList->Images->SetKeyName(11, L"geotag.ico");
			// 
			// AnimatedPictureBoxControl
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->DoubleBuffered = true;
			this->Name = L"AnimatedPictureBoxControl";
			this->Size = System::Drawing::Size(371, 336);
			this->MouseLeave += gcnew System::EventHandler(this, &AnimatedPictureBoxControl::animatedPictureBoxControl_MouseLeave);
			this->MouseHover += gcnew System::EventHandler(this, &AnimatedPictureBoxControl::animatedPictureBoxControl_MouseHover);
			this->ResumeLayout(false);

		}
#pragma endregion
	
private:

	bool currentlyAnimating;
	System::Drawing::Image ^currentImage;
	PictureBoxSizeMode sizeMode;
	bool transparencyEnabled;
	bool infoIconsEnabled;
	ImageAttributes^ imageAttr;

	Color lowerColor;
	Color upperColor;

	String ^caption;
	int activeToolTip;

	const static int nrIcons = 5;
	cli::array<InfoIcon ^> ^infoIcon;

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

	static UInt32 TME_HOVER = 0x00000001;
	static UInt32 TME_LEAVE = 0x00000002;
	static UInt32 HOVER_DEFAULT = 0xFFFFFFFF;

	[System::Runtime::InteropServices::StructLayoutAttribute(System::Runtime::InteropServices::LayoutKind::Sequential)]
	ref struct TRACKMOUSEEVENT {
		UInt32 cbSize;
		UInt32 dwFlags;
		IntPtr hwndTrack;
		UInt32 dwHoverTime;
	};

	[DllImport("user32.dll")]
	static int TrackMouseEvent(TRACKMOUSEEVENT ^lpEventTrack);

	void trackMouseEvent() {
		
		TRACKMOUSEEVENT ^trackMouseEvent = gcnew TRACKMOUSEEVENT();
		trackMouseEvent->hwndTrack = this->Handle;
		trackMouseEvent->dwFlags = TME_HOVER;
		trackMouseEvent->dwHoverTime = HOVER_DEFAULT;
		trackMouseEvent->cbSize = Marshal::SizeOf(trackMouseEvent);

		int result = TrackMouseEvent(trackMouseEvent);
	
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
		
		if(InfoIconsEnabled == false) return;

		for(int i = 0; i < nrIcons; i++) {

			if(infoIcon[i] == nullptr) continue;

			System::Drawing::Image ^icon = imageList->Images[(int)infoIcon[i]->IconImageType];

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

	virtual void OnMouseLeave(EventArgs ^e) override
	{

		if(this->ClientRectangle.Contains(PointToClient(MousePosition))) {

			
		} else {

			UserControl::OnMouseLeave(e);
		}
	
	}

	
	void showToolTip(int nr, Point point) {

		if(nr == activeToolTip) {

			return;

		} else if(nr == nrIcons) {

			toolTip->Show(caption, this, point, toolTip->AutoPopDelay);

		} else {

			toolTip->Show(infoIcon[nr]->Caption, this, point, toolTip->AutoPopDelay);
		}

		activeToolTip = nr;
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

	property bool InfoIconsEnabled {

		void set(bool infoIconsEnabled) {

			this->infoIconsEnabled = infoIconsEnabled;
		}

		bool get() {

			return(infoIconsEnabled);
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

	property String ^Caption {

		void set(String ^caption) {

			this->caption = caption;
		}

	}

	void addInfoIcon(InfoIcon ^icon) {

		for(int i = nrIcons - 1; i >= 0; i--) {

			if(infoIcon[i] == nullptr) {

				infoIcon[i] = icon;
				break;
			}
		}

	}

	void clearInfoIcons() {

		for(int i = 0; i < nrIcons; i++) {

			infoIcon[i] = nullptr;			
		}

	}


private: System::Void animatedPictureBoxControl_MouseHover(System::Object^  sender, System::EventArgs^  e) {

			trackMouseEvent();

			//Util::DebugOut("trackmouse event" + activeToolTip.ToString());

			Point point = this->PointToClient(MousePosition);

			 for(int i = 0; i < nrIcons; i++) {

				 if(infoIcon[i] == nullptr) continue;

				 Rectangle canvas = getIconCanvas(i);

				 if(canvas.Contains(point)) {

					 showToolTip(i, point);
					 return;
				 }

			 }

			 showToolTip(nrIcons, point);
			
				 
		 }
private: System::Void animatedPictureBoxControl_MouseLeave(System::Object^  sender, System::EventArgs^  e) {

			 //Util::DebugOut("mouse leave");
			 toolTip->Hide(this);
			 activeToolTip = -1;
			 
		 }

};
}
