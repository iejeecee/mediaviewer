using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaViewer.MetaData;
using MediaViewer.Utils;
using DB = MediaDatabase;

namespace MediaViewer.MediaFile
{
    abstract class MediaFileBase : EventArgs
    {
        public enum MetaDataMode
        {
            AUTO,
            LOAD_FROM_DISK,
            LOAD_FROM_DATABASE
        }

        protected static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected const int MAX_THUMBNAIL_WIDTH = 160;
        protected const int MAX_THUMBNAIL_HEIGHT = 160;

        protected string location;
        protected string name;

        protected string mimeType;
        protected FileMetaData metaData;

        protected Stream data;
        protected long sizeBytes;

        protected Exception openError;
        protected Exception metaDataError;

        protected Object userState;

        protected MetaDataMode mode;

        protected MediaFileBase()
        {

            location = null;
            data = null;
            metaData = null;
            mimeType = null;
            openError = null;
            metaDataError = null;
            userState = null;
            mode = MetaDataMode.AUTO;
        }

        protected MediaFileBase(string location, string mimeType, Stream data, MetaDataMode mode)
        {

            this.location = location;
            this.data = data;
            this.mimeType = mimeType;
            this.mode = mode;

            metaData = null;
            openError = null;
            metaDataError = null;
            userState = null;

            if (string.IsNullOrEmpty(Location) || MediaFormat == MediaType.UNKNOWN) return;

            readMetaData();
        }

        protected virtual void readMetaData()
        {

            try
            {

                if (FileUtils.isUrl(Location) == false)
                {

                    MetaData = new FileMetaData();

                    switch (mode)
                    {
                        case MetaDataMode.AUTO:
                            {

                                MetaData.load(Location);
                                break;
                            }
                        case MetaDataMode.LOAD_FROM_DATABASE:
                            {

                                MetaData.loadFromDataBase(Location);
                                break;
                            }
                        case MetaDataMode.LOAD_FROM_DISK:
                            {

                                MetaData.loadFromDisk(Location);
                                break;
                            }
                        default:
                            {
                                System.Diagnostics.Debug.Assert(false);
                                break;
                            }

                    }
                }

            }
            catch (Exception e)
            {

                log.Warn("Cannot read metadata: " + Location, e);
                MetaDataError = e;
            }
        }

        protected abstract List<MetaDataThumb> generateThumbnails();

        public enum MediaType
        {
            UNKNOWN,
            IMAGE,
            VIDEO
        }

        public string Location
        {

            set
            {

                this.location = value;
            }

            get
            {

                return (location);
            }
        }

        public string Name
        {

            get
            {

                return (Path.GetFileName(location));
            }
        }

        public string MimeType
        {

            get
            {

                return (mimeType);
            }

            set
            {

                this.mimeType = value;
            }
        }

        public FileMetaData MetaData
        {

            set
            {

                this.metaData = value;
            }

            get
            {

                return (metaData);
            }
        }

        public Stream Data
        {

            get
            {

                return (data);
            }

            set
            {

                this.data = value;

                if (data != null)
                {

                    SizeBytes = data.Length;
                }
            }
        }

        public Exception OpenError
        {

            get
            {

                return (openError);
            }

            set
            {

                this.openError = value;
            }
        }

        public Exception MetaDataError
        {

            get
            {

                return (metaDataError);
            }

            set
            {

                this.metaDataError = value;
            }
        }

        public bool OpenSuccess
        {

            get
            {

                return (OpenError != null ? false : true);
            }

        }

        public abstract MediaType MediaFormat
        {

            get;
        }

        public Object UserState
        {

            get
            {

                return (userState);
            }

            set
            {

                this.userState = value;
            }
        }

        public long SizeBytes
        {

            get
            {

                return (sizeBytes);
            }

            set
            {

                this.sizeBytes = value;
            }
        }

        public List<MetaDataThumb> getThumbnails()
        {

            List<MetaDataThumb> thumbs = new List<MetaDataThumb>();

            if (MediaFormat == MediaFileBase.MediaType.UNKNOWN)
            {

                return (thumbs);

            }
            else if (FileUtils.isUrl(Location))
            {

                thumbs = generateThumbnails();
                return (thumbs);
            }
            
            DB.Context ctx = new DB.Context();

            DB.Media mediaItem = ctx.getMediaByLocation(Location);

            if(mediaItem != null) {

                FileMetaData temp = new FileMetaData(mediaItem);

                ctx.close();
			
                if(temp.Thumbnail.Count == 0) {

                    thumbs = generateThumbnails();
                    MetaData.Thumbnail = thumbs;
                    MetaData.saveToDatabase();

                } else {

                    thumbs = temp.Thumbnail;
                }

            } else {

                ctx.close();

                thumbs = generateThumbnails();

                if(MetaDataError == null) {
				
                    MetaData.Thumbnail = thumbs;
                    MetaData.saveToDatabase();

                } else {

                    mediaItem = DB.Context.newMediaItem(new FileInfo(Location));
                    mediaItem.CanStoreMetaData = 0;

                    FileMetaData temp = new FileMetaData(mediaItem);
                    temp.Thumbnail = thumbs;

                    temp.saveToDatabase();
                }
            }
            
            return (thumbs);
        }

        public virtual string getDefaultCaption()
        {

            return ("");
        }

        public virtual string getDefaultFormatCaption()
        {

            return ("");
        }

        public virtual void close()
        {

            if (data != null)
            {

                data.Close();
            }

            if (metaData != null)
            {

                metaData.closeFile();
            }
        }
    }
}
