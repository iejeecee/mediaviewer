#pragma once

//#include "XMP_Const.h"
//#include "GeoTagCoordinatePair.h"
#include "FileMetaData.h"

namespace imageviewer {

using namespace System;
using namespace System::Globalization;
using namespace XMPLib;

public ref class GeoTagFileData : public FileMetaData {

private:

	static log4net::ILog ^log = log4net::LogManager::GetLogger(System::Reflection::MethodBase::GetCurrentMethod()->DeclaringType);

public:

	String ^fileUrl;
	String ^fileName;

	bool isModified;
	int placeMarkIndex;

	GeoTagFileData(String ^filePath) : FileMetaData() {

		loadFromDisk(filePath);

		this->fileName = System::IO::Path::GetFileName(FilePath);		
		this->fileUrl = "file:///" + FilePath->Replace('\\','/');

		placeMarkIndex = -1;
		isModified = false;
	}

	virtual void saveToDisk() override {

		if(isModified == false) return;

		MetaData ^metaData = gcnew MetaData();
		
		try {

			if(metaData->open(FilePath, kXMPFiles_OpenForUpdate) == false) {

				throw gcnew Exception("Error opening metadata");
			}

			if(HasGeoTag == true)
			{
				String ^latitude = GeoTag->latitude->Coord;
				String ^longitude = GeoTag->longitude->Coord;

				metaData->setProperty(kXMP_NS_EXIF, "GPSLatitude", latitude, 0);
				metaData->setProperty(kXMP_NS_EXIF, "GPSLongitude", longitude, 0);

				metaData->putXMP();

			} else {
				
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

				metaData->putXMP();
			}

		} catch (Exception ^e) {

			log->Error("Error in " + FilePath, e);
			throw gcnew Exception("Error in " + FilePath + ": " + e->Message);			

		} finally {

			delete metaData;
		}

	}

	virtual String ^ToString() override {

		return(fileName);
	}

};


}