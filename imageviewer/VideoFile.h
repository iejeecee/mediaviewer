#pragma once

#include "MediaFile.h"

namespace imageviewer {

using namespace System;
using namespace System::IO;
using namespace System::Collections::Generic;


public ref class VideoFile : public MediaFile
{


private:

	static Image ^defaultVideoThumb;

public:

	static VideoFile() {

		defaultVideoThumb = gcnew Bitmap("C:\\game\\icons\\video.png");
	}

	VideoFile(String ^location) : MediaFile(location) {

	}

	property MediaType MediaFormat
	{
		virtual MediaType get() override {

			return(MediaType::VIDEO);
		}
	}

	virtual List<MetaDataThumb ^> ^generateThumbnails() override {

		List<MetaDataThumb ^> ^thumbs = gcnew List<MetaDataThumb ^>();

		thumbs->Add(gcnew MetaDataThumb(gcnew Bitmap("C:\\game\\icons\\video.png")));

		return(thumbs);
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

};

}