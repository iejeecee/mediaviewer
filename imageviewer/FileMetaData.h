#pragma once

#include "XMP_Const.h"
#include "MetaDataTree.h"
#include "GeoTagCoordinatePair.h"

namespace imageviewer {

using namespace System;
using namespace System::Drawing;
using namespace System::IO;
using namespace XMPLib;
namespace Data = MediaDatabase;

public ref class MetaDataThumb
{
private:

	Image ^image;
	MemoryStream ^data;
	int width;
	int height;
	bool modified;

public:

	MetaDataThumb(Image ^image) {

		ThumbImage = image;

	}

	MetaDataThumb(MemoryStream ^data) {

		this->data = data;
		image = gcnew Bitmap(data);
		width = image->Width;
		height = image->Height;
		modified = false;
	}

	property Image ^ThumbImage {

		void set(Image ^image) {

			this->image = image;
		
			width = image->Width;
			height = image->Height;

			modified = true;
		}

		Image ^get() {

			return(image);
		}
	}

	property int Width {

		int get() {

			return(width);
		}

		void set(int width) {

			this->width = width;
		}
	}
	
	property int Height {

		int get() {

			return(height);
		}

		void set(int height) {

			this->height = height;
		}
	}

	property MemoryStream ^Data {

		MemoryStream ^get() {

			if(modified == true) {
				
				data = gcnew MemoryStream();
			
				image->Save(data, Imaging::ImageFormat::Jpeg);

				modified = false;
			}

			return(data);
		}

		void set(MemoryStream ^data) {

			this->data = data;
		}
	}

};

public ref class FileMetaData : public EventArgs
{
private:

	String ^filePath;
	String ^title;
	String ^description;
	String ^creator;
	String ^creatorTool;
	String ^copyright;
	List<MetaDataThumb ^> ^thumbnail;
	List<String ^> ^tags;
	DateTime creationDate;
	DateTime modifiedDate;
	DateTime metaDataDate;

	static String ^latString = "geo:lat=";
	static String ^lonString = "geo:lon=";
	GeoTagCoordinatePair ^geoTag;
	bool hasGeoTag;

	MetaData ^metaData;
	MetaDataTreeNode ^tree;

	void deleteThumbNail() {

		if(thumbnail != nullptr) {

			delete thumbnail;
		}

		thumbnail = nullptr;

	}

	void readThumbnails() {

		thumbnail->Clear();

		int nrThumbs = metaData->countArrayItems(kXMP_NS_XMP, "Thumbnails");

		for(int thumbNr = 1; thumbNr <= nrThumbs; thumbNr++) {

			String ^fullPath;

			MetaData::composeArrayItemPath(kXMP_NS_XMP, "Thumbnails", thumbNr, fullPath);
			MetaData::composeStructFieldPath(kXMP_NS_XMP, fullPath, kXMP_NS_XMP_Image, "image", fullPath);

			String ^encodedData;
			bool success = metaData->getProperty(kXMP_NS_XMP, fullPath, encodedData);

			if(!success) continue;
				
			cli::array<unsigned char> ^decodedData = Convert::FromBase64String(encodedData);
		
			MemoryStream ^stream = gcnew MemoryStream();
			stream->Write(decodedData, 0, decodedData->Length);
			stream->Seek(0, SeekOrigin::Begin);

			thumbnail->Add(gcnew MetaDataThumb(stream));
		}
	}

	void writeThumbnails() {

		int nrThumbs = metaData->countArrayItems(kXMP_NS_XMP, "Thumbnails");

		for(int i = nrThumbs; i > 0; i--) {

			metaData->deleteArrayItem(kXMP_NS_XMP, "Thumbnails", i);
		}

		for(int i = 0; i < thumbnail->Count; i++) {

			cli::array<unsigned char> ^decodedData = thumbnail[i]->Data->ToArray();

			String ^encodedData = Convert::ToBase64String(decodedData);

			String ^path;

			metaData->appendArrayItem(kXMP_NS_XMP, "Thumbnails", kXMP_PropValueIsArray, nullptr, kXMP_PropValueIsStruct);
			MetaData::composeArrayItemPath(kXMP_NS_XMP, "Thumbnails", kXMP_ArrayLastItem, path);
			
			metaData->setStructField(kXMP_NS_XMP, path, kXMP_NS_XMP_Image, "image", encodedData, 0);
			metaData->setStructField(kXMP_NS_XMP, path, kXMP_NS_XMP_Image, "format", "JPEG", 0);
			metaData->setStructField(kXMP_NS_XMP, path, kXMP_NS_XMP_Image, "width", Convert::ToString(thumbnail[i]->Width), 0);
			metaData->setStructField(kXMP_NS_XMP, path, kXMP_NS_XMP_Image, "height", Convert::ToString(thumbnail[i]->Height), 0);
		}
	}

	void initialize(String ^filePath) {

		this->filePath = filePath;
		title = "";
		description = "";
		creator = "";
		creatorTool = "";
		copyright = "";
		thumbnail = gcnew List<MetaDataThumb ^>();
		tags = gcnew List<String ^>();
		creationDate = DateTime::MinValue;
		modifiedDate = DateTime::MinValue;
		metaDataDate = DateTime::MinValue;

		geoTag = gcnew GeoTagCoordinatePair();
		hasGeoTag = false;

		tree = nullptr;
		metaData = nullptr;

	}

	void initVarsFromDatabaseItem(Data::Media ^item) {

		FilePath = item->Location;

		if(item->Title != nullptr) {

			Title = item->Title;
		}

		if(item->Description != nullptr) {

			Description = item->Description;
		}

		if(item->Author != nullptr) {

			Creator = item->Author;
		}

		if(item->Copyright != nullptr) {

			Copyright = item->Copyright;
		}

		if(item->CreatorTool != nullptr) {

			CreatorTool = item->CreatorTool;
		}

		if(item->MetaDataModified.HasValue) {

			ModifiedDate = item->MetaDataModified.Value;
		}

		if(item->MetaDataDate.HasValue) {

			MetaDataDate = item->MetaDataDate.Value;
		}
		
		if(item->MetaDataCreated.HasValue) {

			CreationDate = item->MetaDataCreated.Value;
		}

		if(item->Longitude != nullptr && item->Latitude != nullptr) {

			hasGeoTag = true;
			GeoTag->longitude->Coord = item->Longitude;
			GeoTag->latitude->Coord = item->Latitude;
		}

		for each(Data::MediaTag ^mediaTag in item->MediaTag) {

			Tags->Add(mediaTag->Tag);
		}

		for each(Data::MediaTag ^mediaTag in item->MediaTag) {

			Tags->Add(mediaTag->Tag);
		}

		for each(Data::MediaThumb ^mediaThumb in item->MediaThumb) {
			
			MemoryStream ^stream = gcnew MemoryStream(mediaThumb->ImageData->ToArray());

			MetaDataThumb ^thumb = gcnew MetaDataThumb(stream);

			Thumbnail->Add(thumb);
		}
	}

public:

	FileMetaData() {		

		initialize("");
	}

	FileMetaData(Data::Media ^media) {		

		initialize("");

		initVarsFromDatabaseItem(media);
	}


	void load(String ^filePath) {

		initialize(filePath);

		Data::MediaTable ^mediaTable = gcnew Data::MediaTable();

		Data::Media ^item = mediaTable->getMediaByLocation(filePath);

		FileInfo ^file = gcnew FileInfo(filePath);

		if(item == nullptr || item->LastWriteTime < file->LastWriteTime.Ticks) {

			loadFromDisk(filePath);

		} else {

			initVarsFromDatabaseItem(item);
		}
		
	}

	bool loadFromDataBase(String ^filePath) {

		initialize(filePath);

		Data::MediaTable ^mediaTable = gcnew Data::MediaTable();

		Data::Media ^item = mediaTable->getMediaByLocation(filePath);

		if(item == nullptr) return(false);

		initVarsFromDatabaseItem(item);
		
		return(true);
	}

	void loadFromDisk(String ^filePath) {

		initialize(filePath);

		metaData = gcnew MetaData();
				
		if(metaData->open(filePath, kXMPFiles_OpenForRead) == false) {

			throw gcnew Exception("Cannot open metadata for: " + filePath);

		} 
		
		readThumbnails();
	
		String ^temp = "";

		bool exists = metaData->getLocalizedText(kXMP_NS_DC, "title", "en", "en-US", temp);
		if(exists) {

			Title = temp;
		}

		exists = metaData->getLocalizedText(kXMP_NS_DC, "description", "en", "en-US", temp);
		if(exists) {

			Description = temp;
		}

		exists = metaData->getArrayItem(kXMP_NS_DC, "creator", 1, temp);
		if(exists) {

			Creator = temp;
		}

		exists = metaData->getArrayItem(kXMP_NS_DC, "rights", 1, temp);
		if(exists) {

			Copyright = temp;
		}

		exists = metaData->getProperty(kXMP_NS_XMP, "CreatorTool", temp);
		if(exists) {

			CreatorTool = temp;
		}

		DateTime propValue;

		exists = metaData->getProperty_Date(kXMP_NS_XMP, "MetadataDate", propValue);
		if(exists) {

			MetaDataDate = propValue;
		} 

		exists = metaData->getProperty_Date(kXMP_NS_XMP, "CreateDate", propValue);
		if(exists) {

			CreationDate = propValue;
		} 

		exists = metaData->getProperty_Date(kXMP_NS_XMP, "ModifyDate", propValue);
		if(exists) {

			ModifiedDate = propValue;
		} 

		if(metaData->doesPropertyExists(kXMP_NS_EXIF, "GPSLatitude") && metaData->doesPropertyExists(kXMP_NS_EXIF, "GPSLongitude"))
		{
			String ^latitude;
			String ^longitude;

			metaData->getProperty(kXMP_NS_EXIF, "GPSLatitude", latitude);
			metaData->getProperty(kXMP_NS_EXIF, "GPSLongitude", longitude);

			geoTag->longitude->Coord = longitude;
			geoTag->latitude->Coord = latitude;

			hasGeoTag = true;

		} 
		
		bool hasLat = false;
		bool hasLon = false;
				
		tags = gcnew List<String ^>();

		int nrTags = metaData->countArrayItems(kXMP_NS_DC, "subject");
		
		for(int i = 1; i <= nrTags; i++) {

			String ^tag;
			exists = metaData->getArrayItem(kXMP_NS_DC, "subject", i, tag);

			if(exists) {

				tags->Add(tag);

				if(tag->StartsWith(latString)) {

					geoTag->latitude->Decimal = Convert::ToDouble(tag->Substring(latString->Length), CultureInfo::InvariantCulture);
					hasLat = true;

				} else if(tag->StartsWith(lonString)) {

					geoTag->longitude->Decimal = Convert::ToDouble(tag->Substring(lonString->Length), CultureInfo::InvariantCulture);
					hasLon = true;
				}
			}
		}

		if(hasLat && hasLon) {
					
			hasGeoTag = true;
		}
		
	}

	~FileMetaData() {

		closeFile();
	}

	property String ^FilePath {

		String ^get() {

			return(filePath);
		}

		void set(String ^filePath) {

			this->filePath = filePath;
		}
	}

	property GeoTagCoordinatePair ^GeoTag {

		GeoTagCoordinatePair ^get() {

			return(geoTag);
		}

		void set(GeoTagCoordinatePair ^geoTag) {

			this->geoTag = geoTag;
		}
	}

	property bool HasGeoTag {

		bool get() {

			return(hasGeoTag);
		}

		void set(bool hasGeoTag) {

			this->hasGeoTag = hasGeoTag;
		}
	}

	property String ^Title {

		String ^get() {

			return(title);
		}

		void set(String ^title) {

			this->title = title;
		}
	}
	property String ^Description {

		String ^get() {

			return(description);
		}

		void set(String ^description) {

			this->description = description;
		}
	}
	property String ^Creator {

		String ^get() {

			return(creator);
		}

		void set(String ^creator) {

			this->creator = creator;
		}
	}
	property String ^CreatorTool {

		String ^get() {

			return(creatorTool);
		}

		void set(String ^creatorTool) {

			this->creatorTool = creatorTool;
		}
	}
	property String ^Copyright {

		String ^get() {

			return(copyright);
		}

		void set(String ^copyright) {

			this->copyright = copyright;
		}
	}
	property List<String ^> ^Tags {

		List<String ^> ^get() {

			return(tags);
		}

		void set(List<String ^> ^tags) {

			this->tags = tags;
		}
	}
	property DateTime CreationDate {

		DateTime get() {

			return(creationDate);
		}

		void set(DateTime creationDate) {

			this->creationDate = creationDate;
		}
	}
	property DateTime ModifiedDate {

		DateTime get() {

			return(modifiedDate);
		}

		void set(DateTime modifiedDate) {

			this->modifiedDate = modifiedDate;
		}
	}
	property DateTime MetaDataDate {

		DateTime get() {

			return(metaDataDate);
		}

		void set(DateTime metaDataDate) {

			this->metaDataDate = metaDataDate;
		}
	}

	property MetaDataTreeNode ^Tree {

		MetaDataTreeNode ^get() {

			if(tree == nullptr && metaData != nullptr) {

				tree = MetaDataTree::create(metaData);
			}

			return(tree);
		}
	}

	property List<MetaDataThumb ^> ^Thumbnail {

		void set(List<MetaDataThumb ^> ^thumbnail) {

			this->thumbnail = thumbnail;
		}

		List<MetaDataThumb ^> ^get() {

			return(thumbnail);
		}
	}

	void save() {

		saveToDatabase();
		saveToDisk();
	}

	virtual void saveToDatabase() {

		bool insert = false;

		Data::MediaTable ^mediaTable = gcnew Data::MediaTable();

		Data::Media ^item = mediaTable->getMediaByLocation(filePath);

		if(item == nullptr) {

			insert = true;
			item = gcnew Data::Media();
			item->Location = FilePath;
			item->CanStoreMetaData = 1;

			FileInfo ^info = gcnew FileInfo(FilePath);
			item->LastWriteTime = info->LastWriteTime.Ticks;
		}


		if(!String::IsNullOrEmpty(Title)) {

			item->Title = Title;

		} else {

			item->Title = nullptr;
		}

		if(!String::IsNullOrEmpty(Description)) {

			item->Description = Description;

		} else {

			item->Description = nullptr;
		}

		if(!String::IsNullOrEmpty(CreatorTool)) {

			item->CreatorTool = CreatorTool;

		} else {

			item->CreatorTool = nullptr;
		}

		if(!String::IsNullOrEmpty(Creator)) {

			item->Author = Creator;

		} else {

			item->Author = nullptr;
		}

		if(!String::IsNullOrEmpty(Copyright)) {
			
			item->Copyright = Creator;

		} else {

			item->Copyright = nullptr;
		}

		if(CreationDate != DateTime::MinValue) {

			item->MetaDataCreated = CreationDate;

		} else {

			item->MetaDataCreated = Nullable<DateTime>();
		}

		if(ModifiedDate != DateTime::MinValue) {

			item->MetaDataModified = ModifiedDate;

		} else {

			item->MetaDataModified = Nullable<DateTime>();
		}

		if(HasGeoTag == true) {
		
			item->Longitude = GeoTag->longitude->Coord;
			item->Latitude = GeoTag->latitude->Coord;
		}

		item->MetaDataDate = DateTime::Now;
	
		item->MediaTag->Clear();

		for each(String ^tag in Tags) {

			Data::MediaTag ^mediaTag = gcnew Data::MediaTag();
			mediaTag->Tag = tag;

			item->MediaTag->Add(mediaTag);
		}

		item->MediaThumb->Clear();

		int i = 0;

		for each(MetaDataThumb ^thumb in Thumbnail) {

			Data::MediaThumb ^mediaThumb = gcnew Data::MediaThumb();
			mediaThumb->ImageData = thumb->Data->ToArray();
			mediaThumb->ThumbNr = i;
	
			item->MediaThumb->Add(mediaThumb);
			i++;
		}

		if(insert == true) {

			mediaTable->insertOnSubmit(item);
		}

		mediaTable->submitChanges();
	}

	virtual void saveToDisk() {

		metaData = gcnew MetaData();

		if(metaData->open(filePath, kXMPFiles_OpenForUpdate) == false) {

			throw gcnew Exception("Cannot open metadata for: " + filePath);

		} 

		writeThumbnails();

		if(!String::IsNullOrEmpty(Title)) {

			metaData->setLocalizedText(kXMP_NS_DC,"title","en", "en-US",Title);

		}

		if(!String::IsNullOrEmpty(Description)) {

			if(metaData->doesArrayItemExist(kXMP_NS_DC, "description", 1)) {

				metaData->setArrayItem(kXMP_NS_DC, "description", 1, Description, 0);

			} else {

				metaData->appendArrayItem(kXMP_NS_DC, "description",  kXMP_PropArrayIsOrdered, Description, 0);
			}

		}

		if(!String::IsNullOrEmpty(CreatorTool)) {

			metaData->setProperty(kXMP_NS_XMP, "CreatorTool", CreatorTool, kXMP_DeleteExisting);
		}

		if(!String::IsNullOrEmpty(Creator)) {

			if(metaData->doesArrayItemExist(kXMP_NS_DC, "creator", 1)) {

				metaData->setArrayItem(kXMP_NS_DC, "creator", 1, Creator, 0);

			} else {

				metaData->appendArrayItem(kXMP_NS_DC, "creator",  kXMP_PropArrayIsOrdered, Creator, 0);
			}

		}

		if(!String::IsNullOrEmpty(Copyright)) {

			if(metaData->doesArrayItemExist(kXMP_NS_DC, "rights", 1)) {

				metaData->setArrayItem(kXMP_NS_DC, "rights", 1, Copyright, 0);

			} else {

				metaData->appendArrayItem(kXMP_NS_DC, "rights",  kXMP_PropArrayIsOrdered, Copyright, 0);
			}

		}

		metaData->setProperty_Date(kXMP_NS_XMP, "MetadataDate", DateTime::Now);

		List<String ^> ^tags = Tags;
		int nrTags = metaData->countArrayItems(kXMP_NS_DC, "subject");
		
		for(int i = nrTags; i > 0; i--) {

			metaData->deleteArrayItem(kXMP_NS_DC, "subject", i);
		}

		for each(String ^tag in tags) {

			metaData->appendArrayItem(kXMP_NS_DC, "subject", kXMP_PropArrayIsUnordered, tag, 0);
		}

		if(HasGeoTag == true)
		{
			String ^latitude = geoTag->latitude->Coord;
			String ^longitude = geoTag->longitude->Coord;

			metaData->setProperty(kXMP_NS_EXIF, "GPSLatitude", latitude, 0);
			metaData->setProperty(kXMP_NS_EXIF, "GPSLongitude", longitude, 0);

		} else {

			// remove a potentially existing geotag
			if(metaData->doesPropertyExists(kXMP_NS_EXIF, "GPSLatitude") && metaData->doesPropertyExists(kXMP_NS_EXIF, "GPSLongitude")) {

				metaData->deleteProperty(kXMP_NS_EXIF, "GPSLatitude");
				metaData->deleteProperty(kXMP_NS_EXIF, "GPSLongitude");
			}

			String ^latString = "geo:lat=";
			String ^lonString = "geo:lon=";

			for(int i = metaData->countArrayItems(kXMP_NS_DC, "subject"); i > 0 ; i--) {

				String ^value = "";

				metaData->getArrayItem(kXMP_NS_DC, "subject", i, value);

				if(value->StartsWith(latString) || value->StartsWith(lonString)) {

					metaData->deleteArrayItem(kXMP_NS_DC, "subject", i);						
				} 
			}
		}

		if(metaData->canPutXMP()) {

			metaData->putXMP();

		} else {

			closeFile();
			throw gcnew Exception("Cannot write metadata for: " + filePath);
		}

		closeFile();
	}

	void closeFile() {

		if(metaData != nullptr) {

			delete metaData;
			metaData = nullptr;
		}
	}
};

}