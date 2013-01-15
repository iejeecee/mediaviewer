#pragma once

#include "XMP_Const.h"

namespace imageviewer {

using namespace System;
using namespace System::Globalization;
using namespace XMPLib;

public ref class GeoTagCoordinate
{
private:

	int degrees;
	int minutes;
	int seconds;
	int secondsFraction;
	System::Char direction;

	double decimal;

	bool isLat;

public:

	GeoTagCoordinate(bool isLat) {

		this->isLat = isLat;
	}

	property String ^Coord {

		void set(String ^coord) {

			degrees = 0;
			minutes = 0;
			seconds = 0;
			secondsFraction = 0;
			direction = '0';
			decimal = 0;

			int s1 = coord->IndexOf(",");
			int s2 = coord->LastIndexOf(",");
			int s3 = coord->IndexOf(".");

			int s4 = (s2 == -1 || s1 == s2) ? s3 : s2;

			degrees = Convert::ToInt32(coord->Substring(0, s1));
			minutes = Convert::ToInt32(coord->Substring(s1 + 1, s4 - s1 - 1));

			int fractLength = coord->Length - s4 - 2;
			int temp = Convert::ToInt32(coord->Substring(s4 + 1, fractLength));

			if(s2 == -1 || s1 == s2) {

				secondsFraction = temp;

				double d = Math::Pow(10, fractLength);

				seconds = int((secondsFraction / d) * 60);	

				decimal = degrees + ((minutes + secondsFraction / d) / 60);

			} else {

				seconds = temp;
				secondsFraction = 0;

				decimal = degrees + (minutes / 60.0) + (seconds / 3600.0);
			}

			direction = Char::ToUpper(coord[coord->Length - 1]);

			if(direction == 'W' || direction == 'S') {

				decimal *= -1;
			}

		}

		String ^get() {

			String ^result = String::Format("{0},{1}.{2}{3}",
				Convert::ToString(degrees),
				Convert::ToString(minutes),
				Convert::ToString(secondsFraction),
				direction);

			return(result);
		}
	}

	property bool IsLat {

		bool get() {

			return(isLat);
		}
	}

	property double Decimal {

		double get() {

			return(decimal);
		}

		void set(double decimal) {

			this->decimal = decimal;

			degrees = (int)Math::Truncate(Math::Abs(decimal));
			
			minutes = ((int)Math::Truncate(Math::Abs(decimal) * 60)) % 60;

			double fract = (Math::Abs(decimal) * 3600) / 60;
			fract = fract - Math::Floor(fract);

			seconds = (int)(fract * 60);
			secondsFraction = (int)(fract * 10000);
			
			if(decimal < 0) {

				if(isLat) {

					direction = 'S';

				} else {

					direction = 'W';
				}

			} else {

				if(isLat) {

					direction = 'N';

				} else {

					direction = 'E';
				}
			} 

		}
	}

};

public ref class GeoTagCoordinatePair {

public:

	GeoTagCoordinate ^latitude;
	GeoTagCoordinate ^longitude;

	GeoTagCoordinatePair() {

		latitude = gcnew GeoTagCoordinate(true);
		longitude = gcnew GeoTagCoordinate(false);
	}

};

public ref class GeoTagFileData : public EventArgs {

public:

	String ^filePath;
	String ^fileUrl;
	String ^fileName;
	GeoTagCoordinatePair ^coord;
	bool hasCoord;
	bool isModified;
	int placeMarkIndex;

	GeoTagFileData(String ^filePath) {

		this->filePath = filePath;
		this->fileName = System::IO::Path::GetFileName(filePath);		
		this->fileUrl = "file:///" + filePath->Replace('\\','/');
		MetaData ^metaData = gcnew MetaData();
		coord = gcnew GeoTagCoordinatePair();
		hasCoord = false;
		placeMarkIndex = -1;
		isModified = false;
		
		try {

			if(metaData->open(filePath, kXMPFiles_OpenForUpdate) == false) {

				throw gcnew Exception("Error opening metadata");
			}

			if(metaData->doesPropertyExists(kXMP_NS_EXIF, "GPSLatitude") && metaData->doesPropertyExists(kXMP_NS_EXIF, "GPSLongitude"))
			{
				String ^latitude;
				String ^longitude;

				metaData->getProperty(kXMP_NS_EXIF, "GPSLatitude", latitude);
				metaData->getProperty(kXMP_NS_EXIF, "GPSLongitude", longitude);

				coord->longitude->Coord = longitude;
				coord->latitude->Coord = latitude;

				hasCoord = true;

			} else if(metaData->doesArrayItemExist(kXMP_NS_DC, "subject", 1)) {

				// look for geotag information in the tags
				// picasa seems to do this

				String ^latString = "geo:lat=";
				String ^lonString = "geo:lon=";

				bool hasLat = false;
				bool hasLon = false;

				for(int i = 1; i <= metaData->countArrayItems(kXMP_NS_DC, "subject"); i++) {

					String ^value = "";

					metaData->getArrayItem(kXMP_NS_DC, "subject", i, value);

					if(value->StartsWith(latString)) {

						coord->latitude->Decimal = Convert::ToDouble(value->Substring(latString->Length), CultureInfo::InvariantCulture);
						hasLat = true;

					} else if(value->StartsWith(lonString)) {

						coord->longitude->Decimal = Convert::ToDouble(value->Substring(lonString->Length), CultureInfo::InvariantCulture);
						hasLon = true;
					}
				}

				if(hasLat && hasLon) {
					
					hasCoord = true;
				}
			} 

		} catch (Exception ^e) {

			throw gcnew Exception("Error in " + filePath + ": " + e->Message);			

		} finally {

			delete metaData;
		}
	}

	void saveToDisk() {

		if(isModified == false) return;

		MetaData ^metaData = gcnew MetaData();
		
		try {

			if(metaData->open(filePath, kXMPFiles_OpenForUpdate) == false) {

				throw gcnew Exception("Error opening metadata");
			}

			if(hasCoord == true)
			{
				String ^latitude = coord->latitude->Coord;
				String ^longitude = coord->longitude->Coord;

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

			throw gcnew Exception("Error in " + filePath + ": " + e->Message);			

		} finally {

			delete metaData;
		}

	}

	virtual String ^ToString() override {

		return(fileName);
	}

};


}