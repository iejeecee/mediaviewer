using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;

namespace PicasaLib
{
    class PicasaAlbum
    {
        public DateTime Published { get; set; }
        public DateTime Updated { get; set; }

        public String Title { get; set; }
        public String Summary { get; set; }
        public String Rights { get; set; }
        public String AlbumId { get; set; }
        public String Access { get; set; }

        public int NumPhotos { get; set; }
        public int NumPhotosRemaining { get; set; }
        public int BytesUsed { get; set; }

        public String User { get; set; }
        public String NickName { get; set; }


        private static string GetElementValue(XContainer element, string name)
        {
            if ((element == null) || (element.Element(name) == null))
                return String.Empty;
            return element.Element(name).Value;
        }

        public PicasaAlbum(XContainer album)
        {
            // Get the string properties from the post's element values
            Title = GetElementValue(album, "{http://www.w3.org/2005/Atom}title");
            Summary = GetElementValue(album, "{http://www.w3.org/2005/Atom}summary");
            Rights = GetElementValue(album, "{http://www.w3.org/2005/Atom}rights");
            AlbumId = GetElementValue(album,
                "{http://schemas.google.com/photos/2007}id");
            Access = GetElementValue(album,
                "{http://schemas.google.com/photos/2007}access");

            // The Date property is a nullable DateTime? -- if the pubDate element
            // can't be parsed into a valid date, the Date property is set to null
            DateTime result;
            if (DateTime.TryParse(GetElementValue(album, "pubDate"), out result))
                Published = result;
        }

    }
}
