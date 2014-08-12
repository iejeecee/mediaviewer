using MediaViewer.MediaFileModel.Watcher;
using mshtml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PluginTest
{
  
    [System.Runtime.InteropServices.ComVisibleAttribute(true)] 
    public class GeoTagScriptInterface {
	
	    WebBrowser browser;
        List<GeoTagFileData> geoTagData;

        public List<GeoTagFileData> GeoTagData
        {
            get { return geoTagData; }          
        }

	    GeoTagFileData getGeoTagFileData(int placeMarkIndex) {

		    GeoTagFileData data = null;

		    foreach(GeoTagFileData image in GeoTagData) {

			    if(image.PlaceMarkIndex == placeMarkIndex) {

				    data = image;
				    break;
			    }
		    }

		    return(data);
	    }
	
	    public event EventHandler Initialized;
	    public event EventHandler<GeoTagFileData> PlaceMarkClicked;
	    public event EventHandler<GeoTagFileData> PlaceMarkMoved;
	    public event EventHandler<GeoTagFileData> EndPlaceMarkMoved;
	    public event EventHandler<String> AddressUpdate;
	
	    public GeoTagScriptInterface(WebBrowser browser, List<GeoTagFileData> geoTagFileItems) {

		    this.browser = browser;

            geoTagData = geoTagFileItems;
		               	    
	    }

	    public void initialize()
        {
		    foreach(GeoTagFileData image in geoTagData) {

			    if(image.HasGeoTag) {

				    createPlaceMark(image, false);
			    }
		    }

            setBorders(false);
            setRoads(false);
            setTerrain(false);

            if (Initialized != null)
            {
                Initialized(this, EventArgs.Empty);
            }
        }

	    public void failure(String errorString) {

		    MessageBox.Show(errorString, "Error");
	    }

	    public void createPlaceMark(GeoTagFileData image, bool useViewportCenter) {
		
		    if(useViewportCenter == true) {

			    String latlngStr = (String)browser.InvokeScript("getViewPortCenter");
			    String lat = latlngStr.Substring(0, latlngStr.IndexOf(' '));
			    String lng = latlngStr.Substring(latlngStr.IndexOf(' ') + 1);

			    image.GeoTag.Latitude.Decimal = Convert.ToDouble(lat, CultureInfo.InvariantCulture);
			    image.GeoTag.Longitude.Decimal = Convert.ToDouble(lng, CultureInfo.InvariantCulture);

			    image.IsModified = true;
		    }

		    String placemarkName = image.FileName;
		    String imagePath = image.FileUrl;
            String imageName = image.FileName;
		    double latitude = image.GeoTag.Latitude.Decimal;
		    double longitude = image.GeoTag.Longitude.Decimal; 

		    Object[] args = new Object[5];

		    args[0] = latitude;
		    args[1] = longitude;
		    args[2] = placemarkName;
		    args[3] = imagePath;
		    args[4] = imageName;

		    image.PlaceMarkIndex = (int)browser.InvokeScript("createPlaceMark", args);

		    image.HasGeoTag = true;
	
	    }

	    public void deletePlaceMark(GeoTagFileData image) {

		    if(image.HasGeoTag == false) return;
		
		    Object[] args = new Object[1];
		    args[0] = image.PlaceMarkIndex;

		    browser.InvokeScript("deletePlaceMark", args);

		    image.HasGeoTag = false;
		    image.IsModified = true;
		    image.PlaceMarkIndex = -1;
		    image.GeoTag.Latitude.Decimal = 0;
		    image.GeoTag.Longitude.Decimal = 0;

	    }

	    public void lookAtPlaceMark(GeoTagFileData image) {

		    if(image.PlaceMarkIndex == -1) return;

		    Object[] args = new Object[1];
		    args[0] = image.PlaceMarkIndex;

		    browser.InvokeScript("lookAtPlaceMark", args);
	    }

	    public void placeMarkClicked(int index) {
		
		    GeoTagFileData clickedImage = getGeoTagFileData(index);

            if (PlaceMarkClicked != null)
            {
                PlaceMarkClicked(this, clickedImage);
            }
	    }

	    public void placeMarkMoved(int index, double latitude, double longitude) {

		    GeoTagFileData movedImage = getGeoTagFileData(index);

		    movedImage.GeoTag.Latitude.Decimal = latitude;
		    movedImage.GeoTag.Longitude.Decimal = longitude;

            if (PlaceMarkMoved != null)
            {
                PlaceMarkMoved(this, movedImage);
            }
	    }

	    public void endPlaceMarkMoved(int index) {

		    GeoTagFileData movedImage = getGeoTagFileData(index);
		    movedImage.IsModified = true;

            if (EndPlaceMarkMoved != null)
            {
                EndPlaceMarkMoved(this, movedImage);
            }

	    }

	    public void flyTo(String location) {

		    Object[] args = new Object[1];
		    args[0] = location;

		    browser.InvokeScript("lookAtQuery", args);

	    }

	    public void reverseGeoCodePlaceMark(GeoTagFileData image) {

		    if(image.HasGeoTag == false) {
			
			    addressUpdate("unknown address");
			    return;
		    }

		    Object[] args = new Object[1];
		    args[0] = image.PlaceMarkIndex;

		    browser.InvokeScript("reverseGeoCodePlaceMark", args);

	    }
	
	    public void setRoads(bool isVisible) {

		    Object[] args = new Object[1];
		    args[0] = isVisible;

		    browser.InvokeScript("setRoads", args);
	    }

	    public void setBorders(bool isVisible) {

		    Object[] args = new Object[1];
		    args[0] = isVisible;

		    browser.InvokeScript("setBorders", args);
	    }

	    public void setTerrain(bool isVisible) {

		    Object[] args = new Object[1];
		    args[0] = isVisible;

		    browser.InvokeScript("setTerrain", args);
	    }

	    public void setBuildings(bool isVisible, bool isLowRes) {

            Object[] args = new Object[2];
		    args[0] = isVisible;
		    args[1] = isLowRes;
     
		    browser.InvokeScript("setTerrain", args);

	    }

	    public void addressUpdate(String address) {

            if (AddressUpdate != null)
            {
                AddressUpdate(this, address);
            }
	    }
    }

}
