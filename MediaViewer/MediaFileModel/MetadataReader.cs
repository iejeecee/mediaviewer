using MediaViewer.MediaDatabase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPLib;

namespace MediaViewer.MediaFileModel
{    

    abstract class MetadataReader
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected const int MAX_THUMBNAIL_WIDTH = 160;
        protected const int MAX_THUMBNAIL_HEIGHT = 160;
        protected static DateTime sqlMinDate = new DateTime(1753, 1, 1);     

        public virtual void readMetadata(Stream data, MediaFactory.ReadOptions options, Media media)
        {

            XMPLib.MetaData.ErrorCallbackDelegate errorCallbackDelegate = new XMPLib.MetaData.ErrorCallbackDelegate(errorCallback);

            //XMPLib.MetaData xmpMetaDataReader = new XMPLib.MetaData(errorCallbackDelegate, null);
            XMPLib.MetaData xmpMetaDataReader = new XMPLib.MetaData(null, null);

            try
            {
                FileInfo info = new FileInfo(media.Location);
                info.Refresh();
                media.LastModifiedDate = info.LastWriteTime < sqlMinDate ? sqlMinDate : info.LastWriteTime;
                media.FileDate = info.CreationTime < sqlMinDate ? sqlMinDate : info.CreationTime;

                if (media.SupportsXMPMetadata == false) return;

                xmpMetaDataReader.open(media.Location, Consts.OpenOptions.XMPFiles_OpenForRead);
                                    
                readXMPMetadata(xmpMetaDataReader, media);
                
            }
            catch (Exception e)
            {
                log.Error("Cannot read XMP metadata for: " + media.Location, e);
                media.MetadataReadError = e;

            } finally {
          
                xmpMetaDataReader.Dispose();
                xmpMetaDataReader = null;
            }
        }

        private bool errorCallback(string filePath, byte errorSeverity, System.UInt32 cause, string message)
        {
            log.Error("MetadataReader (XMP Error): " + filePath + " - " + message + " - error severity: " + errorSeverity.ToString());
            return (true);
        }
      
        protected virtual void readXMPMetadata(XMPLib.MetaData xmpMetaDataReader, Media media)
        {
                     
            string title = "";

            xmpMetaDataReader.getLocalizedText(Consts.XMP_NS_DC, "title", "en", "en-US", ref title);

            media.Title = title;

            string description = "";

            xmpMetaDataReader.getLocalizedText(Consts.XMP_NS_DC, "description", "en", "en-US", ref description);
           
            media.Description = description;

            string author = "";

            xmpMetaDataReader.getArrayItem(Consts.XMP_NS_DC, "creator", 1, ref author);

            media.Author = author;

            string copyright = "";

            xmpMetaDataReader.getLocalizedText(Consts.XMP_NS_DC, "rights", "en", "en-US", ref copyright);
                     
            media.Copyright = copyright;

            string software = "";

            xmpMetaDataReader.getProperty(Consts.XMP_NS_XMP, "CreatorTool", ref software);
                       
            media.Software = software;
         
            Nullable<double> rating = new Nullable<double>();

            xmpMetaDataReader.getProperty_Float(Consts.XMP_NS_XMP, "Rating", ref rating);

            media.Rating = rating;

            Nullable<DateTime> date = new Nullable<DateTime>();

            xmpMetaDataReader.getProperty_Date(Consts.XMP_NS_XMP, "MetadataDate", ref date);
            if (date != null && date < sqlMinDate)
            {               
                media.MetadataDate = null;               
            }
            else
            {
                media.MetadataDate = date;
            }

            xmpMetaDataReader.getProperty_Date(Consts.XMP_NS_XMP, "CreateDate", ref date);
            if (date != null && date < sqlMinDate)
            {
                media.CreationDate = null;
            }
            else
            {
                media.CreationDate = date;
            }
          
            xmpMetaDataReader.getProperty_Date(Consts.XMP_NS_XMP, "ModifyDate", ref date);
            if (date != null && date < sqlMinDate)
            {
                media.MetadataModifiedDate = null;
            }
            else
            {
                media.MetadataModifiedDate = date;
            }

            string longitude = null;
            string latitude = null;

            xmpMetaDataReader.getProperty(Consts.XMP_NS_EXIF, "GPSLatitude", ref latitude);
            xmpMetaDataReader.getProperty(Consts.XMP_NS_EXIF, "GPSLongitude", ref longitude);

            media.Longitude = longitude;
            media.Latitude = latitude;
       
/*
            List<MetaDataProperty> tiffProps = new List<MetaDataProperty>();

            metaDataReader.iterate(Consts.XMP_NS_TIFF, Consts.IterOptions.XMP_IterProperties, ref tiffProps);
            miscProps.AddRange(tiffProps);

            List<MetaDataProperty> exifProps = new List<MetaDataProperty>();

            metaDataReader.iterate(Consts.XMP_NS_EXIF, Consts.IterOptions.XMP_IterProperties, ref exifProps);
            miscProps.AddRange(exifProps);

            List<MetaDataProperty> exifAuxProps = new List<MetaDataProperty>();

            metaDataReader.iterate(Consts.XMP_NS_EXIF_Aux, Consts.IterOptions.XMP_IterProperties, ref exifAuxProps);
            miscProps.AddRange(exifAuxProps);

            List<MetaDataProperty> xmpProps = new List<MetaDataProperty>();

            metaDataReader.iterate(Consts.XMP_NS_XMP, Consts.IterOptions.XMP_IterProperties, ref xmpProps);
            miscProps.AddRange(xmpProps);

            bool hasLat = false;
            bool hasLon = false;

            tags = new List<string>();
*/
            media.Tags.Clear();
            
            int nrTags = xmpMetaDataReader.countArrayItems(Consts.XMP_NS_DC, "subject");

            for (int i = 1; i <= nrTags; i++)
            {

                string tagName = "";
                xmpMetaDataReader.getArrayItem(Consts.XMP_NS_DC, "subject", i, ref tagName);

                if (tagName != null)
                {

                    Tag newTag = new Tag();
                    newTag.Name = tagName;

                    media.Tags.Add(newTag);
/*
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
 */
                }
            }
/*
            if (hasLat && hasLon)
            {

                hasGeoTag = true;
            }          
 */
        }
    }
}
