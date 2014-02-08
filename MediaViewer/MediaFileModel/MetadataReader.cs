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
          
            XMPLib.MetaData xmpMetaDataReader = new XMPLib.MetaData();

            try
            {
               
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

        protected virtual void readXMPMetadata(XMPLib.MetaData xmpMetaDataReader, Media media)
        {
                     
            string temp = "";

            bool exists = xmpMetaDataReader.getLocalizedText(Consts.XMP_NS_DC, "title", "en", "en-US", ref temp);
            if (exists)
            {

                media.Title = temp;
            }
            else
            {
                media.Title = null;
            }

            exists = xmpMetaDataReader.getLocalizedText(Consts.XMP_NS_DC, "description", "en", "en-US", ref temp);
            if (exists)
            {

                media.Description = temp;
            }
            else
            {
                media.Description = null;
            }

            exists = xmpMetaDataReader.getArrayItem(Consts.XMP_NS_DC, "creator", 1, ref temp);
            if (exists)
            {

                media.Author = temp;
            }
            else
            {
                media.Author = null;
            }

            exists = xmpMetaDataReader.getArrayItem(Consts.XMP_NS_DC, "rights", 1, ref temp);
            if (exists)
            {

                media.Copyright = temp;
            }
            else
            {
                media.Copyright = null;
            }

            exists = xmpMetaDataReader.getProperty(Consts.XMP_NS_XMP, "CreatorTool", ref temp);
            if (exists)
            {

                media.Software = temp;
            }
            else
            {
                media.Software = null;
            }

            exists = xmpMetaDataReader.getProperty(Consts.XMP_NS_XMP, "Rating", ref temp);
            if (exists)
            {
                try
                {
                    media.Rating = float.Parse(temp);
                }
                catch (Exception e)
                {
                    log.Error("Incorrect rating in " + media.Location, e);
                    media.Rating = null;
                }
            }
            else
            {
                media.Rating = null;
            }

            DateTime propValue = DateTime.MinValue;

            exists = xmpMetaDataReader.getProperty_Date(Consts.XMP_NS_XMP, "MetadataDate", ref propValue);
            if (exists)
            {
                if (propValue < sqlMinDate)
                {
                    media.MetadataDate = null;
                }
                else
                {
                    media.MetadataDate = propValue;
                }
            }
            else
            {
                media.MetadataDate = null;
            }

            exists = xmpMetaDataReader.getProperty_Date(Consts.XMP_NS_XMP, "CreateDate", ref propValue);
            if (exists)
            {
                if (propValue < sqlMinDate)
                {
                    media.CreationDate = null;
                }
                else
                {
                    media.CreationDate = propValue;                   
                }
            }
            else
            {
                media.CreationDate = null;
            }

            exists = xmpMetaDataReader.getProperty_Date(Consts.XMP_NS_XMP, "ModifyDate", ref propValue);
            if (exists)
            {

                if (propValue < sqlMinDate)
                {
                    media.MetadataModifiedDate = null;
                }
                else
                {
                    media.MetadataModifiedDate = propValue;                    
                }
            }
            else
            {
                media.MetadataModifiedDate = null;
            }
/*
            if (metaDataReader.doesPropertyExists(Consts.XMP_NS_EXIF, "GPSLatitude") && metaDataReader.doesPropertyExists(Consts.XMP_NS_EXIF, "GPSLongitude"))
            {
                string latitude = "";
                string longitude = "";

                metaDataReader.getProperty(Consts.XMP_NS_EXIF, "GPSLatitude", ref latitude);
                metaDataReader.getProperty(Consts.XMP_NS_EXIF, "GPSLongitude", ref longitude);

                geoTag.Longitude.Coord = longitude;
                geoTag.Latitude.Coord = latitude;

                hasGeoTag = true;

            }

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
                exists = xmpMetaDataReader.getArrayItem(Consts.XMP_NS_DC, "subject", i, ref tagName);

                if (exists)
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
