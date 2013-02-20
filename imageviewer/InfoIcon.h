#pragma once

using namespace System;


namespace imageviewer {


public ref class InfoIcon 
{
public:

	enum class IconType {
		ERROR = 0,
		AVI,
		MOV,
		MP4,
		WMV,
		ASF,
		BMP,
		GIF,
		JPG,
		PNG,
		TIFF,
		GEOTAG,
		COMMENTS,
		MUTE
	};

private:

	String ^caption;
	IconType iconType;


	IconType mimeTypeToIconType(String ^mimeType) {

		if(mimeType->Equals("image/tiff")) {

			return(IconType::TIFF);

		} else if(mimeType->Equals("image/gif")) {

			return(IconType::GIF);

		} else if(mimeType->Equals("image/png")) {

			return(IconType::PNG);

		} else if(mimeType->Equals("image/jpeg")) {

			return(IconType::JPG);

		} else if(mimeType->Equals("image/bmp")) {

			return(IconType::BMP);

		} else if(mimeType->Equals("video/x-ms-asf")) {

			return(IconType::ASF);
		
		} else if(mimeType->Equals("video/x-ms-wmv")) {

			return(IconType::WMV);
		
		} else if(mimeType->Equals("video/x-flv")) {

			return(IconType::MP4);
		
		} else if(mimeType->Equals("video/avi") || 
			mimeType->Equals("video/vnd.avi") ||
			mimeType->Equals("video/msvideo") ||
			mimeType->Equals("video/x-msvideo")) 
		{
		
			return(IconType::AVI);

		} else if(mimeType->Equals("video/mp4")) {

			return(IconType::MP4);
		
		} else if(mimeType->Equals("video/quicktime")) {

			return(IconType::MOV);

		} else if(mimeType->Equals("video/x-matroska")) {

			return(IconType::MP4);

		} else if(mimeType->Equals("video/x-m4v")) {

			return(IconType::MP4);

		} else {

			return(IconType::JPG);
		}

	}
	

public:

	InfoIcon(String ^mimeType) {

		iconType = mimeTypeToIconType(mimeType);
	}

	InfoIcon(IconType iconType) {

		this->iconType = iconType;
	}

	property IconType IconImageType {

		IconType get() {

			return(iconType);
		}
	}

	property String ^Caption {

		String ^get() {

			return(caption);
		}

		void set(String ^caption) {

			this->caption = caption;
		}
	}
};

}