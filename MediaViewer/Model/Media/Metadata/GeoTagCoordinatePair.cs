using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.Metadata
{
    public class GeoTagCoordinatePair 
    {
        public event EventHandler GeoTagChanged;

        GeoTagCoordinate Latitude { get; set;}
        GeoTagCoordinate Longitude { get; set; }

        public GeoTagCoordinatePair(double? latitude, double? longitude)
        {
            initialize();

            Latitude.Decimal = latitude;
            Longitude.Decimal = longitude;
        }

        public GeoTagCoordinatePair(string latitude, string longitude)
        {
            initialize();

            Latitude.Coord = latitude;
            Longitude.Coord = longitude;
        }

        public GeoTagCoordinatePair()
        {
            initialize();
        }

        void initialize()
        {
            Latitude = new GeoTagCoordinate(GeoTagCoordinate.CoordinateType.LATITUDE);
            Longitude = new GeoTagCoordinate(GeoTagCoordinate.CoordinateType.LONGITUDE);
        }

        public void set(double? latitude, double? longitude)
        {
            Latitude.Decimal = latitude;
            Longitude.Decimal = longitude;

            OnGeoTagChanged();
        }

        void OnGeoTagChanged()
        {
            if (GeoTagChanged != null)
            {
                GeoTagChanged(this, EventArgs.Empty);
            }
        }

        public String LatCoord
        {
            get
            {
                return (Latitude.Coord);
            }
        }

        public double? LatDecimal
        {
            get
            {
                return (Latitude.Decimal);
            }
        }

        public String LonCoord
        {
            get
            {
                return (Longitude.Coord);
            }
        }

        public double? LonDecimal
        {
            get
            {
                return (Longitude.Decimal);
            }
        }

    }
}
