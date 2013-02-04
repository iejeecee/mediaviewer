#pragma once

#include "MediaFile.h"
#include "Util.h"

namespace imageviewer {

using namespace System;
using namespace System::IO;
using namespace System::Collections::Generic;
using namespace VideoLib;
using namespace System::Runtime::InteropServices;


public ref class VideoFile : public MediaFile
{


private:

	static Image ^defaultVideoThumb;

	VideoPreview ^videoPreview;

	int durationSeconds;

	__int64 sizeBytes;

	int width;
	int height;

	String ^container;
	String ^videoCodecName;
	List<String ^> ^fsMetaData;

	float frameRate;

	bool videoSupportsXMPMetaData() {

		// XMP Metadata does not support matroska
		if(MimeType->Equals("video/x-matroska")) {
			
			return(false);

		// mp4 versions incompatible with XMP metadata
		} else if(mimeType->Equals("video/mp4")) {

			
			if(FSMetaData->Contains("major_brand: isom") &&
				FSMetaData->Contains("minor_version: 1")) 
			{
				return(false);
			}

			if(FSMetaData->Contains("major_brand: mp42") &&
				FSMetaData->Contains("minor_version: 0"))
			{

				if(FSMetaData->Contains("compatible_brands: isom")) {
					return(false);
				}

				if(FSMetaData->Contains("compatible_brands: 000000964375")) {
					return(false);
				}
			}

		} else if(mimeType->Equals("video/avi")) {

			if(VideoCodecName->Equals("mpeg2video")) {

				return(false);
			}
		}

		return(true);
	}
	
protected:

	virtual void readMetaData() override {

		if(videoPreview == nullptr) {

			videoPreview = gcnew VideoPreview();
		}

		try {

			videoPreview->open(Location);

			durationSeconds = videoPreview->DurationSeconds;
			sizeBytes = videoPreview->SizeBytes;

			width = videoPreview->Width;
			height = videoPreview->Height;

			container = videoPreview->Container;
			videoCodecName = videoPreview->VideoCodecName;
			fsMetaData = videoPreview->MetaData;

			frameRate = videoPreview->FrameRate;

			if(videoSupportsXMPMetaData()) {

				MediaFile::readMetaData();
			}

		} catch (Exception ^) {

			videoPreview->close();
		}
	}

public:

	static VideoFile() {

		defaultVideoThumb = gcnew Bitmap("C:\\game\\icons\\video.png");
	}

	VideoFile(String ^location, String ^mimeType, Stream ^data) : MediaFile(location, mimeType, data) {

		
	}

	property MediaType MediaFormat
	{
		virtual MediaType get() override {

			return(MediaType::VIDEO);
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

	property int DurationSeconds {

		int get() {

			return(durationSeconds);
		}
	}

	property __int64 SizeBytes {

		__int64 get() {

			return(sizeBytes);
		}
	}

	property String ^Container {

		String ^get() {

			return(container);
		}
	}

	property String ^VideoCodecName {

		String ^get() {

			return(videoCodecName);
		}
	}

	property List<String ^> ^FSMetaData {

		List<String ^> ^get() {

			return(fsMetaData);
		}
	}

	property float FrameRate {

		float get() {

			return(frameRate);
		}
	}

	virtual List<MetaDataThumb ^> ^generateThumbnails() override {

		List<MetaDataThumb ^> ^thumbs = gcnew List<MetaDataThumb ^>();

		List<Bitmap ^> ^thumbBitmaps = videoPreview->grabThumbnails(MAX_THUMBNAIL_WIDTH,
			MAX_THUMBNAIL_HEIGHT, -1, 1);

		for each(Bitmap ^bitmap in thumbBitmaps) {

			thumbs->Add(gcnew MetaDataThumb(bitmap));
		}

		return(thumbs);
	}

	virtual String ^getDefaultCaption() override {

		StringBuilder ^sb = gcnew StringBuilder();

		sb->AppendLine(Path::GetFileName(Location));
		sb->AppendLine();

		
/*
		for each(String ^info in FSMetaData) {

			sb->AppendLine(info);
		}
*/

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

		sb->AppendLine("Duration:");
		sb->AppendLine(Util::formatTimeSeconds(DurationSeconds));
		sb->AppendLine();

		sb->AppendLine("Size");
		sb->AppendLine(Util::formatSizeBytes(SizeBytes));
		sb->AppendLine();

		sb->AppendLine("Video Codec");
		sb->AppendLine(VideoCodecName);
	
		return(sb->ToString());
	}

	virtual void close() override {

		videoPreview->close();

		MediaFile::close();
	}
};

}