using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.Metadata
{
    public class GeoTagCoordinatePair
    {

        private GeoTagCoordinate latitude;

        public GeoTagCoordinate Latitude
        {
            get { return latitude; }
            set { latitude = value; }
        }
        private GeoTagCoordinate longitude;

        public GeoTagCoordinate Longitude
        {
            get { return longitude; }
            set { longitude = value; }
        }

        public GeoTagCoordinatePair()
        {

            latitude = new GeoTagCoordinate(GeoTagCoordinate.CoordinateType.LATITUDE);
            longitude = new GeoTagCoordinate(GeoTagCoordinate.CoordinateType.LONGITUDE);
        }

    }
}
