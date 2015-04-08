using MediaViewer.Model.Collections;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using mshtml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GeoTagPlugin
{
  
    [System.Runtime.InteropServices.ComVisibleAttribute(true)] 
    public class GoogleEarthScriptInterface {
	
	    WebBrowser browser;

        public event EventHandler<GeoTagFileItem> PlaceMarkClicked;
        public event EventHandler<GeoTagFileItem> PlaceMarkMoved;
        public event EventHandler<GeoTagFileItem> EndPlaceMarkMoved; 

        public bool IsInitialized { get; private set; }
        public bool IsError { get; private set; }
        SemaphoreSlim InitializingSemaphore { get; set; }
      
        LockedObservableCollection<GeoTagFileItem> Items { get; set; }
      
        public GoogleEarthScriptInterface(WebBrowser browser)
        {
            this.browser = browser;
            Items = new LockedObservableCollection<GeoTagFileItem>();
            Items.ItemPropertyChanged += Items_ItemPropertyChanged;

            InitializingSemaphore = new SemaphoreSlim(0, 1);
            IsInitialized = false;
            IsError = false;
           
        }

        private async void Items_ItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            GeoTagFileItem item = (GeoTagFileItem)sender;

            if (e.PropertyName.Equals("Name") && item.PlaceMarkIndex != -1)
            {
                await updatePlaceMark(item);
            }
            else if (e.PropertyName.Equals("HasGeoTag"))
            {
                if (item.HasGeoTag && item.PlaceMarkIndex == -1)
                {
                    await createPlaceMark(item, false);
                }
                else if (!item.HasGeoTag && item.PlaceMarkIndex != -1)
                {
                    await deletePlaceMark(item);
                }
                else if (item.HasGeoTag)
                {
                    await updatePlaceMark(item);
                }
            }

        }

        public async void initializeSuccess()
        {
            IsInitialized = true;
            IsError = false;
            InitializingSemaphore.Release();

            await setBorders(false);
            await setRoads(false);
            await setTerrain(false);

        }

        public void initializeFailure(String errorString)
        {
            IsInitialized = false;
            IsError = true;
            InitializingSemaphore.Release();

            MessageBox.Show(errorString, "Error");
            
        }

        public void error(String errorString)
        {
            MessageBox.Show(errorString, "Error");
        }

        async Task<Object> invokeScript(String functionName, Object[] args = null)
        {
            Object result = null;

            await InitializingSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (IsError == true) return(null);

                Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (args == null)
                        {
                            result = browser.InvokeScript(functionName);
                        }
                        else
                        {
                            result = browser.InvokeScript(functionName, args);
                        }
                    }));

                return (result);
            }
            finally
            {
                InitializingSemaphore.Release();
            }                                
        }

	    GeoTagFileItem get(int placeMarkIndex) {

		    GeoTagFileItem data = null;

            Items.EnterReaderLock();
            try
            {
                foreach (GeoTagFileItem item in Items)
                {
                    if (item.PlaceMarkIndex == placeMarkIndex)
                    {
                        data = item;
                        break;
                    }
                }

                return (data);
            }
            finally
            {
                Items.ExitReaderLock();                
            }
	    }

        public GeoTagFileItem get(MediaFileItem mediaItem)
        {
            Items.EnterReaderLock();
            try
            {
                foreach (GeoTagFileItem geoTagItem in Items)
                {
                    if (geoTagItem.MediaFileItem.Equals(mediaItem))
                    {
                        return (geoTagItem);
                    }
                }

                return (null);
            }
            finally
            {
                Items.ExitReaderLock();
            }
        }

        public async Task add(GeoTagFileItem item)
        {
            if (item.HasGeoTag)
            {
                await createPlaceMark(item, false);
            }

            Items.EnterWriteLock();
            try
            {  
                Items.Add(item);
            }
            finally
            {
                Items.ExitWriteLock();
            }
        }

     

        public async Task remove(GeoTagFileItem item)
        {
            if (item.HasGeoTag)
            {
                await deletePlaceMark(item);
            }

            Items.EnterWriteLock();
            try
            { 
                Items.Remove(item);
            }
            finally
            {
                Items.ExitWriteLock();
            }

        }

        public async Task clearAll()
        {
            Items.EnterWriteLock();
            try
            {
                foreach (GeoTagFileItem item in Items)
                {
                    await deletePlaceMark(item);
                }

                Items.Clear();
            }
            finally
            {
                Items.ExitWriteLock();
            }
        }
	    
	    public async Task createPlaceMark(GeoTagFileItem item, bool useViewportCenter) {
		
		    if(useViewportCenter == true) {

			    String latlngStr = (String)await invokeScript("getViewPortCenter");
			    String lat = latlngStr.Substring(0, latlngStr.IndexOf(' '));
			    String lng = latlngStr.Substring(latlngStr.IndexOf(' ') + 1);

			    item.GeoTag.Latitude.Decimal = Convert.ToDouble(lat, CultureInfo.InvariantCulture);
			    item.GeoTag.Longitude.Decimal = Convert.ToDouble(lng, CultureInfo.InvariantCulture);

			    item.IsModified = true;
		    }
                      
            String placemarkName = item.Name;
            String imagePath = "";
            String imageName = "";
            double latitude = item.GeoTag.Latitude.Decimal;
            double longitude = item.GeoTag.Longitude.Decimal;
           
		    Object[] args = new Object[5];

		    args[0] = latitude;
		    args[1] = longitude;
		    args[2] = placemarkName;
		    args[3] = imagePath;
		    args[4] = imageName;

		    item.PlaceMarkIndex = (int)await invokeScript("createPlaceMark", args);

		    item.HasGeoTag = true;
	
	    }

	    public async Task deletePlaceMark(GeoTagFileItem item) {

            if (item.PlaceMarkIndex == -1) return;
		
		    Object[] args = new Object[1];
		    args[0] = item.PlaceMarkIndex;

		    await invokeScript("deletePlaceMark", args);

		    item.HasGeoTag = false;
		    item.IsModified = true;
		    item.PlaceMarkIndex = -1;
		    item.GeoTag.Latitude.Decimal = 0;
		    item.GeoTag.Longitude.Decimal = 0;
            item.GeoTagLocation = "";

	    }

        async Task updatePlaceMark(GeoTagFileItem item)
        {
            if (item.PlaceMarkIndex == -1) return;

            Object[] args = new Object[4];

            args[0] = item.PlaceMarkIndex;
            args[1] = System.IO.Path.GetFileName(item.Name);
            args[2] = item.GeoTag.Latitude.Decimal;
            args[3] = item.GeoTag.Longitude.Decimal;

            await invokeScript("updatePlaceMark", args);
        }

	    public async Task lookAtPlaceMark(GeoTagFileItem item) {

		    if(item.PlaceMarkIndex == -1) return;

		    Object[] args = new Object[1];
		    args[0] = item.PlaceMarkIndex;

		    await invokeScript("lookAtPlaceMark", args);
	    }

	    public void placeMarkClicked(int index) {
		
		    GeoTagFileItem item = get(index);

            if (item != null && PlaceMarkClicked != null)
            {
                PlaceMarkClicked(this, item);
            }
	    }

	    public void placeMarkMoved(int index, double latitude, double longitude) {

		    GeoTagFileItem item = get(index);

            if (item == null) return;

		    item.GeoTag.Latitude.Decimal = latitude;
		    item.GeoTag.Longitude.Decimal = longitude;

            if (PlaceMarkMoved != null)
            {
                PlaceMarkMoved(this, item);
            }
	    }

	    public async void endPlaceMarkMoved(int index) {

		    GeoTagFileItem item = get(index);

            if (item == null) return;

            await reverseGeoCodePlaceMark(item);

            if (EndPlaceMarkMoved != null)
            {
                EndPlaceMarkMoved(this, item);
            }

	    }

	    public async Task flyTo(String location) {

		    Object[] args = new Object[1];
		    args[0] = location;

		    await invokeScript("lookAtQuery", args);

	    }

	    public async Task reverseGeoCodePlaceMark(GeoTagFileItem item) {
		 
		    Object[] args = new Object[1];
		    args[0] = item.PlaceMarkIndex;

		    await invokeScript("reverseGeoCodePlaceMark", args);

	    }

        public void reverseGeoCodeFinished(int placeMarkId, String address)
        {

            GeoTagFileItem item = get(placeMarkId);

            if (item != null)
            {
                item.GeoTagLocation = address;
            }

        }
	
	    public async Task setRoads(bool isVisible) {

		    Object[] args = new Object[1];
		    args[0] = isVisible;

		    await invokeScript("setRoads", args);
	    }

	    public async Task setBorders(bool isVisible) {

		    Object[] args = new Object[1];
		    args[0] = isVisible;

		    await invokeScript("setBorders", args);
	    }

	    public async Task setTerrain(bool isVisible) {

		    Object[] args = new Object[1];
		    args[0] = isVisible;

		    await invokeScript("setTerrain", args);
	    }

	    public async Task setBuildings(bool isVisible, bool isLowRes) {

            Object[] args = new Object[2];
		    args[0] = isVisible;
		    args[1] = isLowRes;
     
		    await invokeScript("setTerrain", args);

	    }

	    
    }

}
