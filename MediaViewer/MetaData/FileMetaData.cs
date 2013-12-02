using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPLib;
using System.Globalization;
using MediaViewer.MetaData.Tree;


namespace MediaViewer.MetaData
{
    public class FileMetaData : EventArgs, IDisposable
    {

        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string filePath;
        private string title;
        private string description;
        private string creator;
        private string creatorTool;
        private string copyright;
        private List<MetaDataThumb> thumbnail;
        private List<string> tags;
        private DateTime creationDate;
        private DateTime modifiedDate;
        private DateTime metaDataDate;

        private const string latString = "geo:lat=";
        private const string lonString = "geo:lon=";
        private GeoTagCoordinatePair geoTag;
        private bool hasGeoTag;

        private XMPLib.MetaData metaData;   

        List<MetaDataProperty> miscProps;

        public List<MetaDataProperty> MiscProps
        {
            get { return miscProps; }          
        }

        private void deleteThumbNails()
        {

            if (thumbnail != null)
            {
             
                thumbnail.Clear();
            }

        }

        private void readThumbnails()
        {

            thumbnail.Clear();

            int nrThumbs = metaData.countArrayItems(Consts.XMP_NS_XMP, "Thumbnails");

            for (int thumbNr = 1; thumbNr <= nrThumbs; thumbNr++)
            {

                string fullPath = "";

                XMPLib.MetaData.composeArrayItemPath(Consts.XMP_NS_XMP, "Thumbnails", thumbNr, ref fullPath);
                XMPLib.MetaData.composeStructFieldPath(Consts.XMP_NS_XMP, fullPath, Consts.XMP_NS_XMP_Image, "image", ref fullPath);

                string encodedData = "";

                bool success = metaData.getProperty(Consts.XMP_NS_XMP, fullPath, ref encodedData);

                if (!success) continue;

                byte[] decodedData = Convert.FromBase64String(encodedData);

                MemoryStream stream = new MemoryStream();
                stream.Write(decodedData, 0, decodedData.Length);
                stream.Seek(0, SeekOrigin.Begin);

                thumbnail.Add(new MetaDataThumb(stream));
            }
        }

        private void writeThumbnails()
        {

            int nrThumbs = metaData.countArrayItems(Consts.XMP_NS_XMP, "Thumbnails");

            for (int i = nrThumbs; i > 0; i--)
            {

                metaData.deleteArrayItem(Consts.XMP_NS_XMP, "Thumbnails", i);
            }

            for (int i = 0; i < thumbnail.Count; i++)
            {

                byte[] decodedData = thumbnail[i].Data.ToArray();

                string encodedData = Convert.ToBase64String(decodedData);

                string path = "";

                metaData.appendArrayItem(Consts.XMP_NS_XMP, "Thumbnails", 
                    Consts.PropOptions.XMP_PropValueIsArray, null,
                    Consts.PropOptions.XMP_PropValueIsStruct);
                XMPLib.MetaData.composeArrayItemPath(Consts.XMP_NS_XMP, "Thumbnails",
                    Consts.XMP_ArrayLastItem, ref path);

                metaData.setStructField(Consts.XMP_NS_XMP, path, 
                    Consts.XMP_NS_XMP_Image, "image", encodedData, 0);
                metaData.setStructField(Consts.XMP_NS_XMP, path,
                    Consts.XMP_NS_XMP_Image, "format", "JPEG", 0);
                metaData.setStructField(Consts.XMP_NS_XMP, path, 
                    Consts.XMP_NS_XMP_Image, "width", Convert.ToString(thumbnail[i].Width), 0);
                metaData.setStructField(Consts.XMP_NS_XMP, path, 
                    Consts.XMP_NS_XMP_Image, "height", Convert.ToString(thumbnail[i].Height), 0);
            }
        }

        private void initialize(string filePath)
        {

            this.filePath = filePath;
            clear();

        }

        public void clear()
        {
            title = "";
            description = "";
            creator = "";
            creatorTool = "";
            copyright = "";
            rating = 0;
            thumbnail = new List<MetaDataThumb>();
            tags = new List<string>();
            creationDate = DateTime.MinValue;
            modifiedDate = DateTime.MinValue;
            metaDataDate = DateTime.MinValue;

            geoTag = new GeoTagCoordinatePair();
            hasGeoTag = false;

            metaData = null;
            miscProps = new List<MetaDataProperty>();
        }


        public FileMetaData()
        {

            initialize("");
        }

   
        public void load(string filePath)
        {
           
            initialize(filePath);
            loadFromDisk(filePath);                                  
        }
        
        public void loadFromDisk(string filePath)
        {

            initialize(filePath);

            metaData = new XMPLib.MetaData();

            if (metaData.open(filePath, Consts.OpenOptions.XMPFiles_OpenForRead) == false)
            {

                throw new Exception("Cannot open metadata for: " + filePath);

            }

            readThumbnails();

            string temp = "";

            bool exists = metaData.getLocalizedText(Consts.XMP_NS_DC, "title", "en", "en-US", ref temp);
            if (exists)
            {

                Title = temp;
            }

            exists = metaData.getLocalizedText(Consts.XMP_NS_DC, "description", "en", "en-US",ref temp);
            if (exists)
            {

                Description = temp;
            }

            exists = metaData.getArrayItem(Consts.XMP_NS_DC, "creator", 1, ref temp);
            if (exists)
            {

                Creator = temp;
            }

            exists = metaData.getArrayItem(Consts.XMP_NS_DC, "rights", 1, ref temp);
            if (exists)
            {

                Copyright = temp;
            }

            exists = metaData.getProperty(Consts.XMP_NS_XMP, "CreatorTool", ref temp);
            if (exists)
            {

                CreatorTool = temp;
            }

            exists = metaData.getProperty(Consts.XMP_NS_XMP, "Rating", ref temp);
            if (exists)
            {
                try
                {
                    Rating = float.Parse(temp);
                }
                catch (Exception e)
                {
                    log.Error("Incorrect rating in " + filePath, e);
                    Rating = 0;
                }
            }

            DateTime propValue = DateTime.MinValue;

            exists = metaData.getProperty_Date(Consts.XMP_NS_XMP, "MetadataDate", ref propValue);
            if (exists)
            {

                MetaDataDate = propValue;
            }

            exists = metaData.getProperty_Date(Consts.XMP_NS_XMP, "CreateDate", ref propValue);
            if (exists)
            {

                CreationDate = propValue;
            }

            exists = metaData.getProperty_Date(Consts.XMP_NS_XMP, "ModifyDate", ref propValue);
            if (exists)
            {

                ModifiedDate = propValue;
            }

            if (metaData.doesPropertyExists(Consts.XMP_NS_EXIF, "GPSLatitude") && metaData.doesPropertyExists(Consts.XMP_NS_EXIF, "GPSLongitude"))
            {
                string latitude = "";
                string longitude = "";

                metaData.getProperty(Consts.XMP_NS_EXIF, "GPSLatitude", ref latitude);
                metaData.getProperty(Consts.XMP_NS_EXIF, "GPSLongitude", ref longitude);

                geoTag.Longitude.Coord = longitude;
                geoTag.Latitude.Coord = latitude;

                hasGeoTag = true;

            }

            List<MetaDataProperty> tiffProps = new List<MetaDataProperty>();

            metaData.iterate(Consts.XMP_NS_TIFF, Consts.IterOptions.XMP_IterProperties, ref tiffProps);
            miscProps.AddRange(tiffProps);

            List<MetaDataProperty> exifProps = new List<MetaDataProperty>();

            metaData.iterate(Consts.XMP_NS_EXIF, Consts.IterOptions.XMP_IterProperties, ref exifProps);
            miscProps.AddRange(exifProps);

            List<MetaDataProperty> exifAuxProps = new List<MetaDataProperty>();

            metaData.iterate(Consts.XMP_NS_EXIF_Aux, Consts.IterOptions.XMP_IterProperties, ref exifAuxProps);
            miscProps.AddRange(exifAuxProps);

            List<MetaDataProperty> xmpProps = new List<MetaDataProperty>();

            metaData.iterate(Consts.XMP_NS_XMP, Consts.IterOptions.XMP_IterProperties, ref xmpProps);
            miscProps.AddRange(xmpProps);
         
            bool hasLat = false;
            bool hasLon = false;

            tags = new List<string>();

            int nrTags = metaData.countArrayItems(Consts.XMP_NS_DC, "subject");

            for (int i = 1; i <= nrTags; i++)
            {

                string tag = "";
                exists = metaData.getArrayItem(Consts.XMP_NS_DC, "subject", i, ref tag);

                if (exists)
                {

                    tags.Add(tag);

                    if (tag.StartsWith(latString))
                    {

                        geoTag.Latitude.Decimal = Convert.ToDouble(tag.Substring(latString.Length), CultureInfo.InvariantCulture);
                        hasLat = true;

                    }
                    else if (tag.StartsWith(lonString))
                    {

                        geoTag.Longitude.Decimal = Convert.ToDouble(tag.Substring(lonString.Length), CultureInfo.InvariantCulture);
                        hasLon = true;
                    }
                }
            }

            if (hasLat && hasLon)
            {

                hasGeoTag = true;
            }          

        }

        public void Dispose()
        {
            closeFile();
        }

        public string FilePath
        {

            get
            {

                return (filePath);
            }

            set
            {

                this.filePath = value;
            }
        }

        public GeoTagCoordinatePair GeoTag
        {

            get
            {

                return (geoTag);
            }

            set
            {

                this.geoTag = value;
            }
        }

        public bool HasGeoTag
        {

            get
            {

                return (hasGeoTag);
            }

            set
            {

                this.hasGeoTag = value;
            }
        }

        public string Title
        {

            get
            {

                return (title);
            }

            set
            {

                this.title = value;
            }
        }

        float rating;

        public float Rating
        {
            get { return rating; }
            set { rating = value; }
        }

        public string Description
        {

            get
            {

                return (description);
            }

            set
            {

                this.description = value;
            }
        }

        public string Creator
        {

            get
            {

                return (creator);
            }

            set
            {

                this.creator = value;
            }
        }
        public string CreatorTool
        {

            get
            {

                return (creatorTool);
            }

            set
            {

                this.creatorTool = value;
            }
        }
        public string Copyright
        {

            get
            {

                return (copyright);
            }

            set
            {

                this.copyright = value;
            }
        }
        public List<string> Tags
        {

            get
            {

                return (tags);
            }

            set
            {

                this.tags = value;
            }
        }
        public DateTime CreationDate
        {

            get
            {

                return (creationDate);
            }

            set
            {

                this.creationDate = value;
            }
        }
        public DateTime ModifiedDate
        {

            get
            {

                return (modifiedDate);
            }

            set
            {

                this.modifiedDate = value;
            }
        }
        public DateTime MetaDataDate
        {

            get
            {

                return (metaDataDate);
            }

            set
            {

                this.metaDataDate = value;
            }
        }
      

        public List<MetaDataThumb> Thumbnail
        {

            set
            {

                this.thumbnail = value;
            }

            get
            {

                return (thumbnail);
            }
        }

        public void save()
        {

            saveToDisk();
       
        }

      
        public virtual void saveToDisk()
        {

            metaData = new XMPLib.MetaData();

            if (metaData.open(filePath, Consts.OpenOptions.XMPFiles_OpenForUpdate) == false)
            {

                throw new Exception("Cannot open metadata for: " + filePath);

            }

            writeThumbnails();

            if (!string.IsNullOrEmpty(Title))
            {

                metaData.setLocalizedText(Consts.XMP_NS_DC, "title", "en", "en-US", Title);

            }

            metaData.setProperty(Consts.XMP_NS_XMP, "Rating", Rating.ToString(), Consts.PropOptions.XMP_DeleteExisting);
         
            if (!string.IsNullOrEmpty(Description))
            {

                if (metaData.doesArrayItemExist(Consts.XMP_NS_DC, "description", 1))
                {

                    metaData.setArrayItem(Consts.XMP_NS_DC, "description", 1, Description, 0);

                }
                else
                {

                    metaData.appendArrayItem(Consts.XMP_NS_DC, "description",
                        Consts.PropOptions.XMP_PropArrayIsOrdered, Description, 0);
                }

            }

            if (!string.IsNullOrEmpty(CreatorTool))
            {

                metaData.setProperty(Consts.XMP_NS_XMP, "CreatorTool", CreatorTool, 
                    Consts.PropOptions.XMP_DeleteExisting);
            }

            if (!string.IsNullOrEmpty(Creator))
            {

                if (metaData.doesArrayItemExist(Consts.XMP_NS_DC, "creator", 1))
                {

                    metaData.setArrayItem(Consts.XMP_NS_DC, "creator", 1, Creator, 0);

                }
                else
                {

                    metaData.appendArrayItem(Consts.XMP_NS_DC, "creator", 
                        Consts.PropOptions.XMP_PropArrayIsOrdered, Creator, 0);
                }

            }

            if (!string.IsNullOrEmpty(Copyright))
            {

                if (metaData.doesArrayItemExist(Consts.XMP_NS_DC, "rights", 1))
                {

                    metaData.setArrayItem(Consts.XMP_NS_DC, "rights", 1, Copyright, 0);

                }
                else
                {

                    metaData.appendArrayItem(Consts.XMP_NS_DC, "rights",
                        Consts.PropOptions.XMP_PropArrayIsOrdered, Copyright, 0);
                }

            }

            metaData.setProperty_Date(Consts.XMP_NS_XMP, "MetadataDate", DateTime.Now);

            List<string> tags = Tags;
            int nrTags = metaData.countArrayItems(Consts.XMP_NS_DC, "subject");

            for (int i = nrTags; i > 0; i--)
            {

                metaData.deleteArrayItem(Consts.XMP_NS_DC, "subject", i);
            }

            foreach (string tag in tags)
            {

                metaData.appendArrayItem(Consts.XMP_NS_DC, "subject", 
                    Consts.PropOptions.XMP_PropArrayIsUnordered, tag, 0);
            }

            if (HasGeoTag == true)
            {
                string latitude = geoTag.Latitude.Coord;
                string longitude = geoTag.Longitude.Coord;

                metaData.setProperty(Consts.XMP_NS_EXIF, "GPSLatitude", latitude, 0);
                metaData.setProperty(Consts.XMP_NS_EXIF, "GPSLongitude", longitude, 0);

            }
            else
            {

                //// remove a potentially existing geotag
                if (metaData.doesPropertyExists(Consts.XMP_NS_EXIF, "GPSLatitude") && metaData.doesPropertyExists(Consts.XMP_NS_EXIF, "GPSLongitude"))
                {

                    metaData.deleteProperty(Consts.XMP_NS_EXIF, "GPSLatitude");
                    metaData.deleteProperty(Consts.XMP_NS_EXIF, "GPSLongitude");
                }

                string latString = "geo:lat=";
                string lonString = "geo:lon=";

                for (int i = metaData.countArrayItems(Consts.XMP_NS_DC, "subject"); i > 0; i--)
                {

                    string value = "";

                    metaData.getArrayItem(Consts.XMP_NS_DC, "subject", i, ref value);

                    if (value.StartsWith(latString) || value.StartsWith(lonString))
                    {

                        metaData.deleteArrayItem(Consts.XMP_NS_DC, "subject", i);
                    }
                }
            }

            if (metaData.canPutXMP())
            {

                metaData.putXMP();

            }
            else
            {

                closeFile();
                throw new Exception("Cannot write metadata for: " + filePath);
            }

            closeFile();
        }

        public void closeFile()
        {

            if (metaData != null)
            {

                metaData.Dispose();
                metaData = null;
            }
        }
    }
}
