using MediaViewer.Infrastructure.Logging;
using MediaViewer.MediaDatabase;
using MediaViewer.Progress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPLib;

namespace MediaViewer.Model.Media.File.Metadata
{
    abstract class MetadataFileWriter 
    {
    
        protected CancellableOperationProgressBase Progress { get; set; }
        
        public virtual void writeMetadata(BaseMetadata media, CancellableOperationProgressBase progress = null)
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
            if (Progress == null) return (true);

            Progress.ItemProgress = (int)(Progress.ItemProgressMax * fractionDone);

            if (Progress.CancellationToken.IsCancellationRequested)
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
            Logger.Log.Error("MetadataWriter (XMP Error): " + filePath + " - " + message + " - error severity: " + errorSeverity.ToString());
            return (true);
        }

        protected virtual void write(XMPLib.MetaData xmpMetaDataWriter, BaseMetadata media) {
        
            if (media.Title != null)
            {
                xmpMetaDataWriter.setLocalizedText(Consts.XMP_NS_DC, "title", "en", "en-US", media.Title);
            }

            if (media.Rating != null)
            {
                xmpMetaDataWriter.setProperty_Float(Consts.XMP_NS_XMP, "Rating", media.Rating.Value);
                //xmpMetaDataWriter.setProperty(Consts.XMP_NS_XMP, "Rating", media.Rating.ToString(), Consts.PropOptions.XMP_DeleteExisting);
            }

            if (media.Description != null)
            {
                xmpMetaDataWriter.setLocalizedText(Consts.XMP_NS_DC, "description", "en", "en-US", media.Description);
            }

            if (media.Software != null)
            {

                xmpMetaDataWriter.setProperty(Consts.XMP_NS_XMP, "CreatorTool", media.Software,
                    Consts.PropOptions.XMP_DeleteExisting);
            }

            if (media.Author != null)
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

            if (media.Copyright != null)
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

            GeoTagCoordinatePair geoTag = new GeoTagCoordinatePair(media.Latitude, media.Longitude);

            if (!geoTag.IsEmpty)
            {
                xmpMetaDataWriter.setProperty(Consts.XMP_NS_EXIF, "GPSLatitude", geoTag.LatCoord, 0);
                xmpMetaDataWriter.setProperty(Consts.XMP_NS_EXIF, "GPSLongitude", geoTag.LonCoord, 0);
            }
            else
            {
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF, "GPSLatitude");
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF, "GPSLongitude");
            }
           
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
