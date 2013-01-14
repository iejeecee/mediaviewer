using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using System.Windows.Forms;
using System.Diagnostics;

namespace PicasaLib
{
    class PicasaService
    {
        private int timeOutSeconds;

        private AuthInfo _authInfo;

        public AuthInfo AuthInfo
        {
            get { return _authInfo; }
            set { _authInfo = value; }
        }

      
        private XDocument picasaGetRequest(String url)
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + AuthInfo.AccessToken);
            request.Headers.Add("GData-Version", "2");
            request.Timeout = timeOutSeconds * 1000;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            String responseText = HttpRequest.responseToString(response, timeOutSeconds);

            Stream responseStream = response.GetResponseStream();
            responseStream.ReadTimeout = timeOutSeconds * 1000;

            XmlTextReader reader = new XmlTextReader(responseStream);

            XDocument document = new XDocument(reader);

            return (document);

        }



        public PicasaService(AuthInfo authInfo) {

            AuthInfo = authInfo;
            timeOutSeconds = 60;
        }

        public void test()
        {

            try
            {

                //TextReader tr = new StreamReader("picasaalbums.xml");

                //XmlTextReader reader = new XmlTextReader(tr);

                XDocument picasaFeed = XDocument.Load("picasaalbums.xml");

                foreach (XElement elem in picasaFeed.Descendants())
                {

                    System.Diagnostics.Debug.Write(elem.Name + "\n");
                }

                IEnumerable<XElement> elements =  picasaFeed.Descendants("{http://www.w3.org/2005/Atom}entry");

                List<PicasaAlbum> albums = new List<PicasaAlbum>();

                foreach (XElement elem in elements)
                {
                   albums.Add(new PicasaAlbum(elem));
                }
/*
                IEnumerable<PicasaAlbum> albums =
                    from albumFeed in picasaFeed.Descendants("{http://www.w3.org/2005/Atom}entry")
                    select new PicasaAlbum(albumFeed);
*/

                int i = 0;
                i++;
            }
            catch (Exception e)
            {

                MessageBox.Show(e.Message);
            }

        }

        void getUserAlbums()
        {

            try
            {

                XDocument reader = picasaGetRequest("https://picasaweb.google.com/data/feed/api/user/" + "default");


            }
            catch (Exception e)
            {

                System.Diagnostics.Debug.Print(e.Message);

            }


        }

    }
}
