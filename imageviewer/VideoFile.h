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

	static log4net::ILog ^log = log4net::LogManager::GetLogger(System::Reflection::MethodBase::GetCurrentMethod()->DeclaringType);

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

	String ^audioCodecName;
	int samplesPerSecond;
	int bytesPerSample;
	int nrChannels;

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

			audioCodecName = videoPreview->AudioCodecName;
			samplesPerSecond = videoPreview->SamplesPerSecond;
			bytesPerSample = videoPreview->BytesPerSample;
			nrChannels = videoPreview->NrChannels;

			if(videoSupportsXMPMetaData()) {

				MediaFile::readMetaData();

			} else {

				metaDataError = gcnew Exception("Metadata not supported for this format");
			}

		} catch (Exception ^e) {

			log->Error("Cannot read video meta data: " + Location, e);
			videoPreview->close();
		}
	}

public:

	static VideoFile() {

		defaultVideoThumb = gcnew Bitmap("C:\\game\\icons\\video.png");
	}

	VideoFile(String ^location, String ^mimeType, Stream ^data, MediaFile::MetaDataMode mode) 
		: MediaFile(location, mimeType, data, mode) 
	{


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

	property bool HasAudio {

		bool get() {

			return(!String::IsNullOrEmpty(AudioCodecName));
		}
	}

	property String ^AudioCodecName {

		String ^get() {

			return(audioCodecName);
		}
	}

	property int SamplesPerSecond {

		int get() {

			return(samplesPerSecond);
		}
	}

	property int BytesPerSample {

		int get() {

			return(bytesPerSample);
		}
	}

	property int NrChannels {

		int get() {

			return(nrChannels);
		}
	}


	virtual List<MetaDataThumb ^> ^generateThumbnails() override {

		List<MetaDataThumb ^> ^thumbs = gcnew List<MetaDataThumb ^>();

		List<Bitmap ^> ^thumbBitmaps = videoPreview->grabThumbnails(MAX_THUMBNAIL_WIDTH,
			MAX_THUMBNAIL_HEIGHT, -1, 1, 0.025);

		if(thumbBitmaps->Count == 0) {

			// possibly could not seek in video, try to get the first frame in the video
			thumbBitmaps = videoPreview->grabThumbnails(MAX_THUMBNAIL_WIDTH,
				MAX_THUMBNAIL_HEIGHT, -1, 1, 0);
		}

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

		sb->Append("Video Codec (");
		sb->Append(VideoCodecName);
		sb->AppendLine("):");
		sb->Append(width);
		sb->Append("x");
		sb->Append(height);
		sb->Append(", " + videoPreview->PixelFormat + ", " + videoPreview->FrameRate.ToString() + " fps");
		sb->AppendLine();
		sb->AppendLine();

		if(HasAudio == true) {

			sb->Append("Audio Codec (");
			sb->Append(AudioCodecName);
			sb->AppendLine("):");
			sb->Append(SamplesPerSecond);
			sb->Append("Hz, ");
			sb->Append(bytesPerSample * 8);
			sb->Append("bit, ");
			sb->Append(NrChannels);
			sb->Append(" chan");
			sb->AppendLine();
			sb->AppendLine();
		}

		sb->AppendLine("Duration:");
		sb->AppendLine(Util::formatTimeSeconds(DurationSeconds));
		sb->AppendLine();

		sb->AppendLine("Size");
		sb->AppendLine(Util::formatSizeBytes(SizeBytes));
		sb->AppendLine();



		return(sb->ToString());
	}

	virtual void close() override {

		videoPreview->close();

		MediaFile::close();
	}
};

}