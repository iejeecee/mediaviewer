#pragma once
//http://www.bobpowell.net/transcontrols.htm

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
	/// Summary for TransparentIconPanel
	/// </summary>
	public ref class TransparentIconPanel : public System::Windows::Forms::UserControl
	{
	public:
		TransparentIconPanel(void)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
			test = gcnew Bitmap("c:\\game\\icons\\error.png");
			imageAttr = gcnew ImageAttributes();
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~TransparentIconPanel()
		{
			if (components)
			{
				delete components;
			}
		}

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
			// TransparentIconPanel
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->Name = L"TransparentIconPanel";
			this->Size = System::Drawing::Size(392, 82);
			this->ResumeLayout(false);

		}
#pragma endregion

	private:

		const static int nrIcons = 8;
		Image ^test;
		ImageAttributes ^imageAttr;

		Rectangle getIconPosition(int iconNr) {

			int width = Width / 8;

			if(Parent != nullptr) {

				Height = Parent->Height / 8;
			}

			int height = Height;

			int xPos = width * iconNr;
			int yPos = 0;

			Rectangle dest = Rectangle(xPos, yPos, width, height);

			return(dest);
		}

	protected:

		property System::Windows::Forms::CreateParams ^CreateParams
		{

			virtual System::Windows::Forms::CreateParams ^get() override
			{

				System::Windows::Forms::CreateParams ^cp = UserControl::CreateParams;

				cp->ExStyle |= 0x00000020; //WS_EX_TRANSPARENT

				return cp;
			}
		}

		virtual void OnPaintBackground(PaintEventArgs ^pevent) override
		{

			//do not allow the background to be painted 

		}

		void invalidateParent()
		{
			if(Parent == nullptr) return;

			Rectangle rc = Rectangle(Location,Size);
			Parent->Invalidate(rc, true);
		}

	virtual void OnPaint(PaintEventArgs^ e) override
	{		
		
		//invalidateParent();

		Color lowerColor = Color::FromArgb(200,200,200);
		Color upperColor = Color::FromArgb(255,255,255);

		for(int i = 0; i < nrIcons; i++) {

			Rectangle destRect = getIconPosition(i);
			int scaledWidth, scaledHeight;

			ImageUtils::stretchRectangle(test->Width, test->Height, destRect.Width,
				destRect.Height, scaledWidth, scaledHeight);

			Rectangle imageDest = ImageUtils::centerRectangle(destRect, Rectangle(0,0, scaledWidth, scaledHeight));

			imageAttr->SetColorKey( lowerColor, upperColor, ColorAdjustType::Default );
			
			e->Graphics->DrawImage( test, imageDest, 0, 0,
				test->Width, test->Height, GraphicsUnit::Pixel, 
				imageAttr );
		}
/*
		

		// Draw the next frame in the animation.
		if(TransparencyEnabled == true) {
		
			

		} else {

			//Pen^ blackPen = gcnew Pen( Color::Black,3.0f );
			//e->Graphics->DrawRectangle( blackPen, Rectangle(0,0,Width, Height));

			e->Graphics->DrawImage( currentImage, destRect, 0, 0,
				currentImage->Width, currentImage->Height, GraphicsUnit::Pixel);
			
		}

*/		

		//e->Graphics->DrawImage( currentImage, Point(0,0));
	

	}


	};
}
