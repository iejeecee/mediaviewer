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

public: 
	
	static void resizeRectangle(int width, int height, int maxWidth, int maxHeight, int %scaledWidth, int %scaledHeight) {
			
			float widthScale = 1;
			float heightScale = 1;

			if(width > maxWidth) {
				
				widthScale = maxWidth / (float)width;				
			}

			if(height > maxHeight) {
				
				heightScale = maxHeight / (float)height;
				
			}

			scaledWidth = int(width * Math::Min(widthScale, heightScale));
			scaledHeight = int(height * Math::Min(widthScale, heightScale));
		}

	static Image ^resizeImage(Image ^source, int width, int height) {

		if(source->Width == width && source->Height == height) {

			return(gcnew Bitmap(source));
		}

	    Image ^result = gcnew Bitmap(width, height, source->PixelFormat);
		Graphics^ g = Graphics::FromImage(result);

		g->CompositingQuality = CompositingQuality::HighQuality;
		g->SmoothingMode = SmoothingMode::HighQuality;
		g->InterpolationMode = InterpolationMode::HighQualityBicubic;

		g->DrawImage(source, Rectangle(0,0,width, height));

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