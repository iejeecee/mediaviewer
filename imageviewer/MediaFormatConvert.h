#pragma once

namespace imageviewer {

using namespace System;
using namespace System::Drawing::Imaging;
using namespace System::Collections::Generic;
using namespace System::IO;

public ref class MediaFormatConvert {

public:

	static Dictionary<String ^, String ^> ^extToMimeType;
	static Dictionary<String ^, String ^> ^mimeTypeToExt;

	static MediaFormatConvert() {

		extToMimeType = gcnew Dictionary<String ^, String ^>();
		mimeTypeToExt = gcnew Dictionary<String ^, String ^>();

		extToMimeType["tif"] = "image/tiff";
		extToMimeType["tiff"] = "image/tiff";
		extToMimeType["gif"] = "image/gif";
		extToMimeType["png"] = "image/png";
		extToMimeType["jpg"] = "image/jpeg";
		extToMimeType["jpeg"] = "image/jpeg";
		extToMimeType["bmp"] = "image/bmp";
		extToMimeType["asf"] = "video/x-ms-asf";
		extToMimeType["wmv"] = "video/x-ms-wmv";
		extToMimeType["flv"] = "video/x-flv";
		extToMimeType["mov"] = "video/quicktime";
		extToMimeType["mp4"] = "video/mp4";
		extToMimeType["avi"] = "video/avi";
		extToMimeType["mpg"] = "video/mpeg";
		extToMimeType["mpeg"] = "video/mpeg";
		extToMimeType["m4v"] = "video/x-m4v";
		extToMimeType["mkv"] = "video/x-matroska";

		for each(KeyValuePair<String ^, String ^> ^pair in extToMimeType)
		{
			if(mimeTypeToExt->ContainsKey(pair->Value) == false) {
				mimeTypeToExt->Add(pair->Value, pair->Key);
			}
   
		}

		mimeTypeToExt["video/vnd.avi"] = "avi";
		mimeTypeToExt["video/msvideo"] = "avi";
		mimeTypeToExt["video/x-msvideo"] = "avi";
		mimeTypeToExt["video/mpeg"] = "mpg";
		mimeTypeToExt["video/x-mpg"] = "mpg";
		mimeTypeToExt["video/mpeg2"] = "mpg";

	}

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

			String ^ext = Path::GetExtension(fileName)->ToLower();

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

		String ^ext = Path::GetExtension(fileName)->ToLower()->Replace(".","");
		
		if(extToMimeType->ContainsKey(ext) == false) {

			return(nullptr);

		} else {

			return(extToMimeType[ext]);
		}
	
	}

	static String ^mimeTypeToExtension(String ^mimeType) {

		if(mimeTypeToExt->ContainsKey(mimeType) == false) {

			return(nullptr);

		} else {

			return(mimeTypeToExt[mimeType]);
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

}