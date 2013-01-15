#pragma once

#include "GeoTagFileData.h"
#include "GEventArgs.h"

namespace imageviewer {

using namespace System;
using namespace System::Windows::Forms;
using namespace System::Collections::Generic;

[System::Runtime::InteropServices::ComVisibleAttribute(true)] 
public ref class GeoTagScriptInterface {

private:

	WebBrowser ^browser;
	List<GeoTagFileData ^> ^geoTagData;

	GeoTagFileData ^getGeoTagFileData(int placeMarkIndex) {

		GeoTagFileData ^data = nullptr;

		for each(GeoTagFileData ^image in GeoTagData) {

			if(image->placeMarkIndex == placeMarkIndex) {

				data = image;
				break;
			}
		}

		return(data);
	}

	
public:

	event EventHandler<EventArgs ^> ^Initialized;
	event EventHandler<GeoTagFileData ^> ^PlaceMarkClicked;
	event EventHandler<GeoTagFileData ^> ^PlaceMarkMoved;
	event EventHandler<GeoTagFileData ^> ^EndPlaceMarkMoved;
	event EventHandler<GEventArgs<String ^> ^> ^AddressUpdate;

	property List<GeoTagFileData ^> ^GeoTagData {

			 List<GeoTagFileData ^> ^get() {

				 return(geoTagData);
			 }

	}

	GeoTagScriptInterface(WebBrowser ^browser, List<String ^> ^fileNames) {

		this->browser = browser;
		
		geoTagData = gcnew List<GeoTagFileData ^>();

		try {

			for each(String ^fileName in fileNames) {

				GeoTagFileData ^newData = gcnew GeoTagFileData(fileName);
				geoTagData->Add(newData);
			}

		} catch(Exception ^e) {

			System::Diagnostics::Debug::Write(e->Message);
			throw;
		}

	}

	void initialize()
    {

		for each(GeoTagFileData ^image in geoTagData) {

			if(image->hasCoord) {

				createPlaceMark(image, false);
			}
		}

		Initialized(this, gcnew EventArgs());
    }

	void failure(String ^errorString) {

		MessageBox::Show(errorString, "Error");
	}

	void createPlaceMark(GeoTagFileData ^image, bool useViewportCenter) {
		
		if(useViewportCenter == true) {

			String ^latlngStr = (String ^)browser->Document->InvokeScript("getViewPortCenter");
			String ^lat = latlngStr->Substring(0, latlngStr->IndexOf(' '));
			String ^lng = latlngStr->Substring(latlngStr->IndexOf(' ') + 1);

			image->coord->latitude->Decimal = Convert::ToDouble(lat, CultureInfo::InvariantCulture);
			image->coord->longitude->Decimal = Convert::ToDouble(lng, CultureInfo::InvariantCulture);

			image->isModified = true;
		}

		String ^placemarkName = image->fileName;
		String ^imagePath = image->fileUrl; 
		String ^imageName = image->fileName;
		double latitude = image->coord->latitude->Decimal;
		double longitude = image->coord->longitude->Decimal; 

		cli::array<Object ^> ^args = gcnew cli::array<Object ^>(5);

		args[0] = latitude;
		args[1] = longitude;
		args[2] = placemarkName;
		args[3] = imagePath;
		args[4] = imageName;

		image->placeMarkIndex = (int)browser->Document->InvokeScript("createPlaceMark", args);

		image->hasCoord = true;
	
	}

	void deletePlaceMark(GeoTagFileData ^image) {

		if(image->hasCoord == false) return;
		
		cli::array<Object ^> ^args = gcnew cli::array<Object ^>(1);
		args[0] = image->placeMarkIndex;

		browser->Document->InvokeScript("deletePlaceMark", args);

		image->hasCoord = false;
		image->isModified = true;
		image->placeMarkIndex = -1;
		image->coord->latitude->Decimal = 0;
		image->coord->longitude->Decimal = 0;

	}

	void lookAtPlaceMark(GeoTagFileData ^image) {

		if(image->placeMarkIndex == -1) return;

		cli::array<Object ^> ^args = gcnew cli::array<Object ^>(1);
		args[0] = image->placeMarkIndex;

		browser->Document->InvokeScript("lookAtPlaceMark", args);
	}

	void placeMarkClicked(int index) {
		
		GeoTagFileData ^clickedImage = getGeoTagFileData(index);

		PlaceMarkClicked(this, clickedImage);
	}

	void placeMarkMoved(int index, double latitude, double longitude) {

		GeoTagFileData ^movedImage = getGeoTagFileData(index);

		movedImage->coord->latitude->Decimal = latitude;
		movedImage->coord->longitude->Decimal = longitude;

		PlaceMarkMoved(this, movedImage);

	}

	void endPlaceMarkMoved(int index) {

		GeoTagFileData ^movedImage = getGeoTagFileData(index);
		movedImage->isModified = true;

		EndPlaceMarkMoved(this, movedImage);

	}

	void flyTo(String ^location) {

		cli::array<Object ^> ^args = gcnew cli::array<Object ^>(1);
		args[0] = location;

		browser->Document->InvokeScript("lookAtQuery", args);

	}

	void reverseGeoCodePlaceMark(GeoTagFileData ^image) {

		if(image->hasCoord == false) {
			
			addressUpdate("unknown address");
			return;
		}

		cli::array<Object ^> ^args = gcnew cli::array<Object ^>(1);
		args[0] = image->placeMarkIndex;

		browser->Document->InvokeScript("reverseGeoCodePlaceMark", args);

	}
	
	void setRoads(bool isVisible) {

		cli::array<Object ^> ^args = gcnew cli::array<Object ^>(1);
		args[0] = isVisible;

		browser->Document->InvokeScript("setRoads", args);
	}

	void setBorders(bool isVisible) {

		cli::array<Object ^> ^args = gcnew cli::array<Object ^>(1);
		args[0] = isVisible;

		browser->Document->InvokeScript("setBorders", args);
	}

	void setTerrain(bool isVisible) {

		cli::array<Object ^> ^args = gcnew cli::array<Object ^>(1);
		args[0] = isVisible;

		browser->Document->InvokeScript("setTerrain", args);
	}

	void setBuildings(bool isVisible, bool isLowRes) {

		cli::array<Object ^> ^args = gcnew cli::array<Object ^>(2);
		args[0] = isVisible;
		args[1] = isLowRes;

		browser->Document->InvokeScript("setTerrain", args);

	}

	void addressUpdate(String ^address) {

		AddressUpdate(this, gcnew GEventArgs<String ^>(address));
	}
};

}