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
        public static List<LocationResult> findLocations(String location, LocationRect usermapView, String sessionKey)
        {
            List<LocationResult> result = new List<LocationResult>();

            if (String.IsNullOrEmpty(location) || String.IsNullOrWhiteSpace(location)) return (result);

            String query = "?query=" + location.Trim();

            String culture = "&culture=en-GB";

            String output = "&o=xml";

            string userMapView = "&usermapView=" +
                usermapView.South.ToString(CultureInfo.CreateSpecificCulture("en-GB")) + "," +
                usermapView.West.ToString(CultureInfo.CreateSpecificCulture("en-GB")) + "," +
                usermapView.North.ToString(CultureInfo.CreateSpecificCulture("en-GB")) + "," +
                usermapView.East.ToString(CultureInfo.CreateSpecificCulture("en-GB"));

            string userLocation = "&userLocation=" +
                usermapView.Center.Latitude.ToString(CultureInfo.CreateSpecificCulture("en-GB")) + "," +
                usermapView.Center.Longitude.ToString(CultureInfo.CreateSpecificCulture("en-GB"));
           
            string key = "&key=" + sessionKey;

            string geocodeRequest = "http://dev.virtualearth.net/REST/v1/Locations" + query + culture + output + userMapView + key;

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
                    result.Add(new LocationResult(resource, nsmgr, usermapView.Center));
                }

                result.Sort(new Comparison<LocationResult>((a, b) => a.SqrdDistToMapCenter.CompareTo(b.SqrdDistToMapCenter)));
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
