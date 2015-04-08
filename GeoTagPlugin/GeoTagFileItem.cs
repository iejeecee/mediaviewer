using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.MetaData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.Metadata;
using Microsoft.Practices.Prism.Mvvm;
using System.Windows;
using System.ComponentModel;
using System.Threading;
using MediaViewer.Model.Media.Base;

namespace GeoTagPlugin
{
    public class GeoTagFileItem : BindableBase
    {
        public GeoTagFileItem(MediaFileItem item)
        {
            this.mediaFileItem = item;
            placeMarkIndex = -1;
            isModified = false;

            geoTag = new GeoTagCoordinatePair();
    
            WeakEventManager<MediaFileItem, PropertyChangedEventArgs>.AddHandler(mediaFileItem, "PropertyChanged", mediaFileItem_PropertyChanged);

            mediaFileItem.RWLock.EnterReadLock();
            try
            {
                Name = System.IO.Path.GetFileName(mediaFileItem.Location);
              
                if (mediaFileItem.ItemState == MediaItemState.LOADED)
                {
                    if (mediaFileItem.Metadata.Latitude != null && mediaFileItem.Metadata.Longitude != null)
                    {
                        geoTag.Latitude.Coord = mediaFileItem.Metadata.Latitude;
                        geoTag.Longitude.Coord = mediaFileItem.Metadata.Longitude;

                        HasGeoTag = true;
                    }
                    else
                    {
                        geoTag.Latitude.Decimal = 0;
                        geoTag.Longitude.Decimal = 0;

                        HasGeoTag = false;
                    }
                }
            }
            finally
            {
                mediaFileItem.RWLock.ExitReadLock();
            }
        }

        private void mediaFileItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            mediaFileItem.RWLock.EnterReadLock();
            try
            {
                if (e.PropertyName.Equals("Location"))
                {
                    Name = System.IO.Path.GetFileName(mediaFileItem.Location);
                }
                else if (e.PropertyName.Equals("HasGeoTag"))
                {
                    if (mediaFileItem.Metadata.Latitude != null && mediaFileItem.Metadata.Longitude != null)
                    {
                        geoTag.Latitude.Coord = mediaFileItem.Metadata.Latitude;
                        geoTag.Longitude.Coord = mediaFileItem.Metadata.Longitude;

                        HasGeoTag = true;
                    }
                    else
                    {
                        geoTag.Latitude.Decimal = 0;
                        geoTag.Longitude.Decimal = 0;

                        HasGeoTag = false;
                    }
                }
               
            }
            finally
            {
                mediaFileItem.RWLock.ExitReadLock();
            }         
        }

        String name;

        public String Name
        {
            get { return name; }
            set { 
                SetProperty(ref name, value);
            }
        }

        String geoTagLocation;

        public String GeoTagLocation
        {
            get { return geoTagLocation; }
            set { SetProperty(ref geoTagLocation, value); }
        }

        bool isModified;

        public bool IsModified
        {
            get { return isModified; }
            set { 

               isModified = value;
            }
        }
        int placeMarkIndex;

        public int PlaceMarkIndex
        {
            get { return placeMarkIndex; }
            set { placeMarkIndex = value; }
        }

        bool hasGeoTag;

        public bool HasGeoTag
        {
            get { return hasGeoTag; }
            set {

                SetProperty(ref hasGeoTag, value);
            }
        }

        MediaFileItem mediaFileItem;

        public MediaFileItem MediaFileItem
        {
            get { return mediaFileItem; }
           
        }

        GeoTagCoordinatePair geoTag;

        public GeoTagCoordinatePair GeoTag
        {
            get { return geoTag; }
            set { geoTag = value; }
        }

    }
}
