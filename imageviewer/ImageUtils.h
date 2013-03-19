#pragma once

namespace imageviewer {

using namespace System;
using namespace System::IO;
using namespace System::Collections::Generic;
using namespace System::Drawing;
using namespace System::Drawing::Imaging;
using namespace System::Drawing::Drawing2D;
using namespace System::Windows::Forms;
using namespace System::Runtime::InteropServices;


public ref class ImageUtils 
{
private:

	static log4net::ILog ^log = log4net::LogManager::GetLogger(System::Reflection::MethodBase::GetCurrentMethod()->DeclaringType);

public: 
	
	static Rectangle centerRectangle(Rectangle outer, Rectangle inner) {

		Rectangle center = Rectangle(outer.X, outer.Y, inner.Width, inner.Height);

		center.X += (outer.Width - inner.Width) / 2;
		center.Y += (outer.Height - inner.Height) / 2;
	
		return(center);
	}

	static void resizeRectangle(int width, int height, int maxWidth, int maxHeight, int %scaledWidth, int %scaledHeight) {
			
		float widthScale = 1;
		float heightScale = 1;

		if(width > maxWidth) {
			
			widthScale = maxWidth / (float)width;				
		}

		if(height > maxHeight) {
			
			heightScale = maxHeight / (float)height;
			
		}
		
		scaledWidth = int(Math::Round(width * Math::Min(widthScale, heightScale)));
		scaledHeight = int(Math::Round(height * Math::Min(widthScale, heightScale)));
	}

	static Rectangle stretchRectangle(Rectangle rec, Rectangle max) {
			
		float widthScale = 1;
		float heightScale = 1;

		widthScale = max.Width / (float)rec.Width;			
		heightScale = max.Height / (float)rec.Height;
		
		Rectangle stretched(rec.X, rec.Y, 0, 0);

		stretched.Width = int(Math::Round(rec.Width * Math::Min(widthScale, heightScale)));
		stretched.Height = int(Math::Round(rec.Height * Math::Min(widthScale, heightScale)));

		return(stretched);
	}

	static Image ^resizeImage(Image ^source, int width, int height) {

		Image ^result = nullptr;

		try {

			if(source->Width == width && source->Height == height) {

				return(gcnew Bitmap(source));
			}

			result = gcnew Bitmap(width, height, source->PixelFormat);
			Graphics^ g = Graphics::FromImage(result);

			g->CompositingQuality = CompositingQuality::HighQuality;
			g->SmoothingMode = SmoothingMode::HighQuality;
			g->InterpolationMode = InterpolationMode::HighQualityBicubic;

			g->DrawImage(source, Rectangle(0,0,width, height));

		} catch (Exception ^e) {

			log->Error("Error resizing image", e);
			MessageBox::Show(e->Message, "Error resizing image");

		} 

		return(result);

	}

	static OpenFileDialog ^createOpenImageFileDialog() {

		OpenFileDialog ^openFileDialog = gcnew OpenFileDialog();

		openFileDialog->Filter = L"Image Files|*.tif;*.jpg;*.jpeg;*.gif;*.png;*.bmp|"
			L"JPEG Files (*.jpg)|*.jpg;*.jpeg|"
			L"PNG Files (*.png)|*.png|"	                  
			L"GIF Files (*.gif)|*.gif|"
			L"TIFF Files (*.tif)|*.tif|"
			L"BMP File (*.bmp)|*.bmp";

		openFileDialog->FilterIndex = 1;

		return(openFileDialog);
	}

	static Image ^createImageFromArray(int width, int height, 
		Drawing::Imaging::PixelFormat format, cli::array<unsigned char> ^data) 
	{

		Bitmap ^bitmap = gcnew Bitmap(width, height, format);

		Rectangle rect = Rectangle(0, 0, width, height);

		BitmapData ^bmpData = bitmap->LockBits(rect, ImageLockMode::WriteOnly,
			format);

		IntPtr ptr = bmpData->Scan0;

		Marshal::Copy(data, 0, ptr, data->Length);

		bitmap->UnlockBits(bmpData);

		return(bitmap);
	}
};

}