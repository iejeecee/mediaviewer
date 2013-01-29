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
	static const int IMAGE_WIDTH = 0xA002;
	static const int IMAGE_HEIGHT = 0x0101;

	int width;
	int height;

	__int64 sizeBytes;

	Image ^imageMetaData;

protected:

	virtual void readMetaData() override {

		imageMetaData = Image::FromStream(Data, false, false);
		
/*
		for(int i = 0; i < imageMetaData->PropertyIdList->Length; i++) {

			if(imageMetaData->PropertyIdList[i] == IMAGE_WIDTH)
			{
				PropertyItem ^p = imageMetaData->GetPropertyItem(IMAGE_WIDTH);
				if(p->Value != nullptr) {

					if(p->Len == 2) continue;

					IntPtr ptr = Marshal::UnsafeAddrOfPinnedArrayElement(p->Value, 0);
					
					cli::array<int> ^bla = gcnew cli::array<int>(p->Len / 4);

					Marshal::Copy(ptr, bla, 0, p->Len / 4);

					width = bla[0];
				}
	
			}

			if(imageMetaData->PropertyIdList[i] == IMAGE_HEIGHT)
			{
				PropertyItem ^p = imageMetaData->GetPropertyItem(IMAGE_HEIGHT);
				if(p->Value != nullptr) {

					IntPtr ptr = Marshal::UnsafeAddrOfPinnedArrayElement(p->Value, 0);
					
					cli::array<short> ^bla = gcnew cli::array<short>(p->Len / 2);

					Marshal::Copy(ptr, bla, 0, p->Len / 2);

					height = bla[0];
				}
	
			}

		}

*/
		MediaFile::readMetaData();
		
	}


public:

	ImageFile(String ^location, String ^mimeType, Stream ^data) : MediaFile(location, mimeType, data) {

		sizeBytes = data->Length;
	}

	virtual List<MetaDataThumb ^> ^generateThumbnails() override {

		List<MetaDataThumb ^> ^thumbs = gcnew List<MetaDataThumb ^>();

		Image ^tempImage = nullptr;

		try {

			// GDI+ throws an error if we try to read a property when the imageMetaData
			// doesn't have that property. Check to make sure the thumbnail property
			// item exists.
			bool propertyFound = false;

			for(int i = 0; i < imageMetaData->PropertyIdList->Length; i++) {

				if(imageMetaData->PropertyIdList[i] == THUMBNAIL_DATA)
				{
					propertyFound = true;
					break;
				}

			}

			if(propertyFound) {

				PropertyItem ^p = imageMetaData->GetPropertyItem(THUMBNAIL_DATA);

				// The imageMetaData data is in the form of a byte array. Write all 
				// the bytes to a stream and create a new imageMetaData from that stream
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

	virtual String ^getDefaultFormatCaption() override {

		StringBuilder ^sb = gcnew StringBuilder();

		sb->AppendLine(Path::GetFileName(Location));
		sb->AppendLine();
	
		sb->AppendLine("Mime type:");
		sb->Append(MimeType);
		sb->AppendLine();
		sb->AppendLine();

		sb->AppendLine("Resolution:");
		sb->Append(width);
		sb->Append("x");
		sb->Append(height);
		sb->AppendLine();
		sb->AppendLine();

		sb->AppendLine("Size:");
		sb->Append(Util::formatSizeBytes(sizeBytes));
	
		return(sb->ToString());
	}

	property MediaType MediaFormat
	{
		virtual MediaType get() override {

			return(MediaType::IMAGE);
		}
	}

	property int Width {

		int get() {

			return(width);
		}
	}

	property int Height {

		int get() {

			return(height);
		}
	}

	virtual void close() override {

		if(imageMetaData != nullptr) {

			delete imageMetaData;
			imageMetaData = nullptr;
		}

		MediaFile::close();
	}
};

}