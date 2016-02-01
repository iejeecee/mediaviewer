using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MediaViewer.UserControls.GeoTagEditor
{
    class LocationResult
    {
        public String Name { get; set; }

        public String AdminDistrict { get; set; }
        public String AdminDistrict2 { get; set; }
        public String CountryRegion { get; set; }
        public String FormattedAddress { get; set; }
        public String Locality { get; set; }

        public Location Geotag { get; set; }
        public LocationRect BoundingBox { get; set; }
        public String Confidence { get; set; }
        public String Matchcode { get; set; }
      
        public LocationResult(XmlNode location, XmlNamespaceManager nsmgr)
        {
           
            Name = location.SelectSingleNode("ns:Name",nsmgr).InnerText;
       
            string latStr = location.SelectSingleNode("ns:Point/ns:Latitude",nsmgr).InnerText;
            string lonStr = location.SelectSingleNode("ns:Point/ns:Longitude", nsmgr).InnerText;

            double lat = double.Parse(latStr, CultureInfo.InvariantCulture);
            double lon = double.Parse(lonStr, CultureInfo.InvariantCulture);
            
            Geotag = new Location(lat, lon);

            string southLat = location.SelectSingleNode("ns:BoundingBox/ns:SouthLatitude", nsmgr).InnerText;
            string westLon = location.SelectSingleNode("ns:BoundingBox/ns:WestLongitude", nsmgr).InnerText;
            string northLat = location.SelectSingleNode("ns:BoundingBox/ns:NorthLatitude", nsmgr).InnerText;
            string eastLon = location.SelectSingleNode("ns:BoundingBox/ns:EastLongitude", nsmgr).InnerText;

            Location corner1 = new Location(double.Parse(southLat, CultureInfo.InvariantCulture),double.Parse(westLon, CultureInfo.InvariantCulture));
            Location corner2 = new Location(double.Parse(northLat, CultureInfo.InvariantCulture),double.Parse(eastLon, CultureInfo.InvariantCulture));

            BoundingBox = new LocationRect(corner1,corner2);
           
        }

        public double getSqrdDistToLocation(Location loc)
        {
            return Math.Pow(Geotag.Latitude - loc.Latitude, 2) + Math.Pow(Geotag.Longitude - loc.Longitude, 2);
        }
    }
}
