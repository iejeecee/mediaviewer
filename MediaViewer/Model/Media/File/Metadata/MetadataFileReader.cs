using MediaViewer.Infrastructure.Logging;
using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XMPLib;
using MediaViewer.Model.Media.Base.Metadata;
using VideoLib;
using System.Windows.Media.Imaging;
using MediaViewer.Model.Utils;

namespace MediaViewer.Model.Media.File.Metadata
{    

    abstract class MetadataFileReader
    {
               
        protected static DateTime sqlMinDate = new DateTime(1753, 1, 1);     

        public virtual void readMetadata(MediaProbe mediaProbe, Stream data, MetadataFactory.ReadOptions options, BaseMetadata media, CancellationToken token, int timeoutSeconds)
        {

            XMPLib.MetaData.ErrorCallbackDelegate errorCallbackDelegate = new XMPLib.MetaData.ErrorCallbackDelegate(errorCallback);

            //XMPLib.MetaData xmpMetaDataReader = new XMPLib.MetaData(errorCallbackDelegate, null);
            XMPLib.MetaData xmpMetaDataReader = new XMPLib.MetaData(null, null);

            try
            {
                
                FileInfo info = new FileInfo(media.FullLocation);
                info.Refresh();
                media.LastModifiedDate = info.LastWriteTime < sqlMinDate ? sqlMinDate : info.LastWriteTime;
                media.FileDate = info.CreationTime < sqlMinDate ? sqlMinDate : info.CreationTime;
                media.MimeType = MediaFormatConvert.fileNameToMimeType(media.Name); 

                if (media.SupportsXMPMetadata == false) return;

                xmpMetaDataReader.open(media.FullLocation, Consts.OpenOptions.XMPFiles_OpenForRead);
                                    
                readXMPMetadata(xmpMetaDataReader, media);
                
            }
            catch (Exception e)
            {
                Logger.Log.Error("Cannot read XMP metadata for: " + media.Location, e);
                media.MetadataReadError = e;

            } finally {
          
                xmpMetaDataReader.Dispose();
                xmpMetaDataReader = null;
            }
        }

        private bool errorCallback(string filePath, byte errorSeverity, System.UInt32 cause, string message)
        {
            Logger.Log.Error("MetadataReader (XMP Error): " + filePath + " - " + message + " - error severity: " + errorSeverity.ToString());
            return (true);
        }
      
        protected virtual void readXMPMetadata(XMPLib.MetaData xmpMetaDataReader, BaseMetadata media)
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
           
            GeoTagCoordinatePair geoPair = new GeoTagCoordinatePair(latitude, longitude);
            media.Latitude = geoPair.LatDecimal;
            media.Longitude = geoPair.LonDecimal;
            
            media.Tags.Clear();
            
            int nrTags = xmpMetaDataReader.countArrayItems(Consts.XMP_NS_DC, "subject");

            for (int i = 1; i <= nrTags; i++)
            {

                string tagName = "";
                xmpMetaDataReader.getArrayItem(Consts.XMP_NS_DC, "subject", i, ref tagName);

                if (tagName != null)
                {

                    Tag newTag = new Tag();
                    newTag.Name = tagName.Trim();

                    media.Tags.Add(newTag);

                }
            }

            /*string imageData = "";

            bool exists = xmpMetaDataReader.getStructField(Consts.XMP_NS_XMP, "xmpGImg", Consts.XMP_NS_XMP_Image, "image", ref imageData);
            if (exists)
            {
                BitmapSource thumbnail = MediaViewer.Infrastructure.Utils.ImageUtils.jpegBase64StringToImage(imageData);
              
            }*/
            

        }
    }
}
