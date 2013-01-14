#pragma once

using namespace System;
using namespace System::Drawing::Imaging;

public ref class MediaFormatConvert {

public:

	static String ^imageFormatToMimeType(ImageFormat ^imageFormat) {

		if(imageFormat == ImageFormat::Tiff) {

			return("image/tiff");

		} else if(imageFormat == ImageFormat::Gif) {

			return("image/gif");

		} else if(imageFormat == ImageFormat::Png) {

			return("image/png");

		} else if(imageFormat == ImageFormat::Jpeg) {

			return("image/jpeg");

		} else if(imageFormat == ImageFormat::Bmp) {

			return("image/bmp");

		} else {

			return(nullptr);
		}

	}

	static ImageFormat ^mimeTypeToImageFormat(String ^mimeType) {

		if(mimeType->Equals("image/tiff")) {

			return(ImageFormat::Tiff);

		} else if(mimeType->Equals("image/gif")) {

			return(ImageFormat::Gif);

		} else if(mimeType->Equals("image/png")) {

			return(ImageFormat::Png);

		} else if(mimeType->Equals("image/jpeg")) {

			return(ImageFormat::Jpeg);

		} else if(mimeType->Equals("image/bmp")) {

			return(ImageFormat::Bmp);

		} else {

			return(nullptr);
		}
		
	}

	static ImageFormat ^fileNameToImageFormat(String ^fileName) {

			String ^ext = System::IO::Path::GetExtension(fileName)->ToLower();

			ImageFormat ^imageFormat;

			if(ext->Equals(".tif")) {

				imageFormat = ImageFormat::Tiff;

			} else if(ext->Equals(".gif")) {

				imageFormat = ImageFormat::Gif;

			} else if(ext->Equals(".png")) {

				imageFormat = ImageFormat::Png;

			} else if(ext->Equals(".bmp")) {

				imageFormat = ImageFormat::Bmp;

			} else {

				imageFormat = ImageFormat::Jpeg;
			}

			return(imageFormat);
		}

	static String ^fileNameToMimeType(String ^fileName) {

		String ^ext = System::IO::Path::GetExtension(fileName)->ToLower();

		if(ext->Equals(".tif")) {

			return("image/tiff");

		} else if(ext->Equals(".gif")) {

			return("image/gif");

		} else if(ext->Equals(".png")) {

			return("image/png");

		} else if(ext->Equals(".jpg") || ext->Equals(".jpeg")) {

			return("image/jpeg");

		} else if(ext->Equals(".bmp")) {

			return("image/bmp");

		} else if(ext->Equals(".asf")) {

			return("video/x-ms-asf");

		} else if(ext->Equals(".wmv")) {

			return("video/x-ms-wmv");

		} else if(ext->Equals(".flv")) {

			return("video/x-flv");

		} else if(ext->Equals(".mov")) {

			return("video/quicktime");

		} else if(ext->Equals(".mp4")) {
		
			return("video/mp4");

		} else if(ext->Equals(".avi")) {
	
			return("video/avi");

		} else {

			return(nullptr);
		}

	}

	static String ^mimeTypeToExtension(String ^mimeType) {

		if(mimeType->Equals("image/tiff")) {

			return("tif");

		} else if(mimeType->Equals("image/gif")) {

			return("gif");

		} else if(mimeType->Equals("image/png")) {

			return("png");

		} else if(mimeType->Equals("image/jpeg")) {

			return("jpg");

		} else if(mimeType->Equals("image/bmp")) {

			return("bmp");

		} else if(mimeType->Equals("video/x-ms-asf")) {

			return("asf");
		
		} else if(mimeType->Equals("video/x-ms-wmv")) {

			return("wmv");
		
		} else if(mimeType->Equals("video/x-flv")) {

			return("flv");
		
		} else if(mimeType->Equals("video/avi") || 
			mimeType->Equals("video/vnd.avi") ||
			mimeType->Equals("video/msvideo") ||
			mimeType->Equals("video/x-msvideo")) 
		{
		
			return("avi");

		} else if(mimeType->Equals("video/mp4")) {

			return("mp4");
		
		} else if(mimeType->Equals("video/quicktime")) {

			return("mov");
		
		} else {

			return("jpg");
		}

	}

	static bool isMediaFile(String ^fileName) {

		return(fileNameToMimeType(fileName) == nullptr ? false : true);
	}

	static bool isVideoFile(String ^fileName) {

		String ^mimeType = fileNameToMimeType(fileName);

		if(mimeType == nullptr) return(false);
		else if(mimeType->StartsWith("video")) return(true);
		
		return(false);
	}

	static bool isImageFile(String ^fileName) {

		String ^mimeType = fileNameToMimeType(fileName);

		if(mimeType == nullptr) return(false);
		else if(mimeType->StartsWith("image")) return(true);
		
		return(false);
	}

};