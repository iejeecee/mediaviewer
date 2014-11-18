using MediaViewer.MediaDatabase;
using MediaViewer.Progress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPLib;

namespace MediaViewer.Model.Media.Metadata
{
    abstract class MetadataWriter 
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ICancellableOperationProgress progress;

        protected ICancellableOperationProgress Progress
        {
            get { return progress; }
            set { progress = value; }
        }

        public virtual void writeMetadata(BaseMedia media, ICancellableOperationProgress progress)
        {
            Progress = progress;

            XMPLib.MetaData.ErrorCallbackDelegate errorCallbackDelegate = new XMPLib.MetaData.ErrorCallbackDelegate(errorCallback);
            // bug in xmplib, crashes on write when video is mpg and a progresscallback is active
            XMPLib.MetaData.ProgressCallbackDelegate progressCallbackDelegate = media.MimeType.Equals("video/mpeg") ? null : new XMPLib.MetaData.ProgressCallbackDelegate(progressCallback);

            XMPLib.MetaData xmpMetaDataWriter = new XMPLib.MetaData(errorCallbackDelegate, progressCallbackDelegate);

            try
            {
               if (media.SupportsXMPMetadata)
               {
                   xmpMetaDataWriter.open(media.Location, Consts.OpenOptions.XMPFiles_OpenForUpdate);

                   write(xmpMetaDataWriter, media);
               }
               else
               {
                  throw new Exception("Format does not support XMP metadata");
               }

            }
            finally
            {
                xmpMetaDataWriter.Dispose();
                xmpMetaDataWriter = null;
            }
        }

        private bool progressCallback(float elapsedTime, float fractionDone, float secondsToGo)
        {
            progress.ItemProgress = (int)(progress.ItemProgressMax * fractionDone);

            if (progress.CancellationToken.IsCancellationRequested)
            {
                return (false);
            }
            else
            {
                return (true);
            }
        }

        private bool errorCallback(string filePath, byte errorSeverity, System.UInt32 cause, string message)
        {
            log.Error("MetadataWriter (XMP Error): " + filePath + " - " + message + " - error severity: " + errorSeverity.ToString());
            return (true);
        }

        protected virtual void write(XMPLib.MetaData xmpMetaDataWriter, BaseMedia media) {
        
            if (!string.IsNullOrEmpty(media.Title))
            {

                xmpMetaDataWriter.setLocalizedText(Consts.XMP_NS_DC, "title", "en", "en-US", media.Title);

            }

            if (media.Rating != null)
            {
                xmpMetaDataWriter.setProperty(Consts.XMP_NS_XMP, "Rating", media.Rating.ToString(), Consts.PropOptions.XMP_DeleteExisting);
            }

            if (!string.IsNullOrEmpty(media.Description))
            {
                xmpMetaDataWriter.setLocalizedText(Consts.XMP_NS_DC, "description", "en", "en-US", media.Description);
            }

            if (!string.IsNullOrEmpty(media.Software))
            {

                xmpMetaDataWriter.setProperty(Consts.XMP_NS_XMP, "CreatorTool", media.Software,
                    Consts.PropOptions.XMP_DeleteExisting);
            }

            if (!string.IsNullOrEmpty(media.Author))
            {

                if (xmpMetaDataWriter.doesArrayItemExist(Consts.XMP_NS_DC, "creator", 1))
                {

                    xmpMetaDataWriter.setArrayItem(Consts.XMP_NS_DC, "creator", 1, media.Author, 0);

                }
                else
                {

                    xmpMetaDataWriter.appendArrayItem(Consts.XMP_NS_DC, "creator",
                        Consts.PropOptions.XMP_PropArrayIsOrdered, media.Author, 0);
                }

            }

            if (!string.IsNullOrEmpty(media.Copyright))
            {

                xmpMetaDataWriter.setLocalizedText(Consts.XMP_NS_DC, "rights", "en", "en-US", media.Copyright);
          
            }

            if (media.CreationDate != null)
            {
                xmpMetaDataWriter.setProperty_Date(Consts.XMP_NS_XMP, "CreateDate", media.CreationDate.Value);
            }

            if (media.MetadataDate == null)
            {

                xmpMetaDataWriter.setProperty_Date(Consts.XMP_NS_XMP, "MetadataDate", DateTime.Now);
            }
            else
            {
                xmpMetaDataWriter.setProperty_Date(Consts.XMP_NS_XMP, "ModifyDate", DateTime.Now);
            }

           
            int nrTags = xmpMetaDataWriter.countArrayItems(Consts.XMP_NS_DC, "subject");

            for (int i = nrTags; i > 0; i--)
            {

                xmpMetaDataWriter.deleteArrayItem(Consts.XMP_NS_DC, "subject", i);
            }

            foreach (Tag tag in media.Tags)
            {
                xmpMetaDataWriter.appendArrayItem(Consts.XMP_NS_DC, "subject",
                    Consts.PropOptions.XMP_PropArrayIsUnordered, tag.Name, 0);
            }

            if (media.Latitude != null)
            {
                xmpMetaDataWriter.setProperty(Consts.XMP_NS_EXIF, "GPSLatitude", media.Latitude, 0);
            }
            else
            {
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF, "GPSLatitude");
            }

            if (media.Longitude != null)
            {
                xmpMetaDataWriter.setProperty(Consts.XMP_NS_EXIF, "GPSLongitude", media.Longitude, 0);
            }
            else
            {
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF, "GPSLongitude");
            }
/*
            if (HasGeoTag == true)
            {
                string latitude = geoTag.Latitude.Coord;
                string longitude = geoTag.Longitude.Coord;

                xmpMetaDataWriter.setProperty(Consts.XMP_NS_EXIF, "GPSLatitude", latitude, 0);
                xmpMetaDataWriter.setProperty(Consts.XMP_NS_EXIF, "GPSLongitude", longitude, 0);

            }
            else
            {

                //// remove a potentially existing geotag
                if (xmpMetaDataWriter.doesPropertyExists(Consts.XMP_NS_EXIF, "GPSLatitude") && xmpMetaDataWriter.doesPropertyExists(Consts.XMP_NS_EXIF, "GPSLongitude"))
                {

                    xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF, "GPSLatitude");
                    xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF, "GPSLongitude");
                }

                string latString = "geo:lat=";
                string lonString = "geo:lon=";

                for (int i = xmpMetaDataWriter.countArrayItems(Consts.XMP_NS_DC, "subject"); i > 0; i--)
                {

                    string value = "";

                    xmpMetaDataWriter.getArrayItem(Consts.XMP_NS_DC, "subject", i, ref value);

                    if (value.StartsWith(latString) || value.StartsWith(lonString))
                    {

                        xmpMetaDataWriter.deleteArrayItem(Consts.XMP_NS_DC, "subject", i);
                    }
                }
            }
*/
            if (xmpMetaDataWriter.canPutXMP())
            {

                xmpMetaDataWriter.putXMP();

            }
            else
            {              
                throw new Exception("Format does not support XMP metadata");
            }

          
        }

    }
}
