#pragma once

#include "MediaFile.h"
#include "ImageUtils.h"

namespace imageviewer {

using namespace System;
using namespace System::IO;
using namespace System::Drawing::Imaging;
using namespace System::Collections::Generic;
using namespace System::Text;


public ref class ImageFile : public MediaFile
{


private:

	static const int THUMBNAIL_DATA = 0x501B;
	

public:

	ImageFile(String ^location) : MediaFile(location) {

	}


	virtual List<MetaDataThumb ^> ^generateThumbnails() override {

		List<MetaDataThumb ^> ^thumbs = gcnew List<MetaDataThumb ^>();

		Image ^tempImage = nullptr;
		Image ^image = nullptr;

		try {

			image = Image::FromStream(Data, false, false);

			// GDI+ throws an error if we try to read a property when the image
			// doesn't have that property. Check to make sure the thumbnail property
			// item exists.
			bool propertyFound = false;

			for(int i = 0; i < image->PropertyIdList->Length; i++) {

				if(image->PropertyIdList[i] == THUMBNAIL_DATA)
				{
					propertyFound = true;
					break;
				}

			}

			if(propertyFound) {

				PropertyItem ^p = image->GetPropertyItem(THUMBNAIL_DATA);

				// The image data is in the form of a byte array. Write all 
				// the bytes to a stream and create a new image from that stream
				if(p->Value != nullptr) {

					cli::array<unsigned char> ^imageBytes = p->Value;

					MemoryStream ^stream = gcnew MemoryStream(imageBytes->Length);
					stream->Write(imageBytes, 0, imageBytes->Length);

					tempImage = Image::FromStream(stream);

				} else {

					tempImage = Image::FromStream(Data, MAX_THUMBNAIL_WIDTH, MAX_THUMBNAIL_HEIGHT);

				}

			} else {

				tempImage = Image::FromStream(Data, MAX_THUMBNAIL_WIDTH, MAX_THUMBNAIL_HEIGHT);
			}


			// scale thumbnail to the right size
			int thumbWidth;
			int thumbHeight;

			ImageUtils::resizeRectangle(tempImage->Width, tempImage->Height, 
				MAX_THUMBNAIL_WIDTH, MAX_THUMBNAIL_HEIGHT, thumbWidth, thumbHeight);

			Image ^thumbImage = nullptr;

			thumbImage = gcnew Bitmap(tempImage, thumbWidth, thumbHeight);

			thumbs->Add(gcnew MetaDataThumb(thumbImage));

			return(thumbs);

		} finally {

			if(tempImage != nullptr) {

				delete tempImage;
			}

			if(image != nullptr) {

				delete image;
			}
		}
	}

	virtual String ^getDefaultCaption() override {

		StringBuilder ^sb = gcnew StringBuilder();

		sb->AppendLine(Path::GetFileName(Location));
		sb->AppendLine();
		
		if(MetaData != nullptr) {

			if(MetaData->Description != nullptr) {

				sb->AppendLine("Description:");

				//String ^temp = System::Text::RegularExpressions::Regex::Replace(MetaData->Description,"(.{50}\\s)","$1`n");
				sb->AppendLine(MetaData->Description);
				sb->AppendLine();
			}

			if(MetaData->Creator != nullptr) {

				sb->AppendLine("Creator:");
				sb->AppendLine(MetaData->Creator);
				sb->AppendLine();

			}

			if(MetaData->CreationDate != DateTime::MinValue) {

				sb->AppendLine("Creation date:");
				sb->Append(MetaData->CreationDate);
				sb->AppendLine();
			}
		}

		return(sb->ToString());
	}

	property MediaType MediaFormat
	{
		virtual MediaType get() override {

			return(MediaType::IMAGE);
		}
	}
};

}