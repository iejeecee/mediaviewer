using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MediaViewer.UserControls.GeoTagEditor
{
    class BingMapsService
    {
        public static LocationResult findLocation(Location location, String authKey)
        {
            String point = location.Latitude.ToString(CultureInfo.CreateSpecificCulture("en-GB")) + "," +
                location.Longitude.ToString(CultureInfo.CreateSpecificCulture("en-GB"));

            String output = "?o=xml";

            string key = "&key=" + authKey;

            string geocodeRequest = "http://dev.virtualearth.net/REST/v1/Locations/" + point + output + key;

            //Make the request and get the response
            XmlDocument geocodeResponse = GetXmlResponse(geocodeRequest);

            XmlElement root = geocodeResponse.DocumentElement;

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(geocodeResponse.NameTable);
            nsmgr.AddNamespace("ns", "http://schemas.microsoft.com/search/local/ws/rest/v1");

            LocationResult result = null;

            if (int.Parse(root.SelectSingleNode("//ns:EstimatedTotal", nsmgr).InnerText) > 0)
            {
                XmlNodeList resources = root.SelectNodes("//ns:Location", nsmgr);
               
                result = new LocationResult(resources[0], nsmgr);                
            }

            return (result);
        }

        public static List<LocationResult> findLocations(String query, String authKey, LocationRect usermapView = null)
        {
            List<LocationResult> result = new List<LocationResult>();

            if (String.IsNullOrEmpty(query) || String.IsNullOrWhiteSpace(query)) return (result);

            String location = "?query=" + query.Trim();

            String culture = "&culture=en-GB";

            String output = "&o=xml";

            string userMapView = "";

            if (usermapView != null)
            {
                userMapView = "&usermapView=" +
                    usermapView.South.ToString(CultureInfo.CreateSpecificCulture("en-GB")) + "," +
                    usermapView.West.ToString(CultureInfo.CreateSpecificCulture("en-GB")) + "," +
                    usermapView.North.ToString(CultureInfo.CreateSpecificCulture("en-GB")) + "," +
                    usermapView.East.ToString(CultureInfo.CreateSpecificCulture("en-GB"));
            }
                       
            string key = "&key=" + authKey;

            string geocodeRequest = "http://dev.virtualearth.net/REST/v1/Locations" + location + culture + output + userMapView + key;

            //Make the request and get the response
            XmlDocument geocodeResponse = GetXmlResponse(geocodeRequest);

            XmlElement root = geocodeResponse.DocumentElement;

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(geocodeResponse.NameTable);
            nsmgr.AddNamespace("ns", "http://schemas.microsoft.com/search/local/ws/rest/v1");

            if (int.Parse(root.SelectSingleNode("//ns:EstimatedTotal", nsmgr).InnerText) > 0)
            {
                XmlNodeList resources = root.SelectNodes("//ns:Location", nsmgr);

                foreach (XmlNode resource in resources)
                {
                    result.Add(new LocationResult(resource, nsmgr));
                }

                if (usermapView != null)
                {
                    // sort on distance from usermap center
                    result.Sort(new Comparison<LocationResult>((a, b) => a.getSqrdDistToLocation(usermapView.Center).CompareTo(b.getSqrdDistToLocation(usermapView.Center))));
                }
            }
            else
            {
                // no results found
                SystemSounds.Hand.Play();
            }

            return (result);
        }

        private static XmlDocument GetXmlResponse(string requestUrl)
        {
            //System.Diagnostics.Trace.WriteLine("Request URL (XML): " + requestUrl);
            HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception(String.Format("Server error (HTTP {0}: {1}).",
                    response.StatusCode,
                    response.StatusDescription));
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(response.GetResponseStream());
                return xmlDoc;
            }
        }
    }
}
