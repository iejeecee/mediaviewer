using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaViewer.MetaData;
using MediaViewer.Utils;
using DB = MediaDatabase;
using System.Threading;
using System.Windows.Media.Imaging;

namespace MediaViewer.MediaFileModel
{
    public abstract class MediaFile : EventArgs
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

        BitmapSource thumbnail;

        protected Stream data;
        protected long sizeBytes;

        protected Exception openError;

        protected Object userState;


        protected MediaFile()
        {

            location = null;
            name = null;
            data = null;
            metaData = null;
            mimeType = null;
            openError = null;
       
            userState = null;

            thumbnail = null;

        }

        protected MediaFile(string location, string mimeType, Stream data, MetaDataMode mode)
        {

            this.location = location;
            this.data = data;
            this.mimeType = mimeType;

            metaData = null;
            openError = null;
            userState = null;

            thumbnail = null;

            if (string.IsNullOrEmpty(Location) || MediaFormat == MediaType.UNKNOWN) return;
            
        }
     
        /*
         *   
         */

        public virtual void readMetaData()
        {
            if (!Utils.FileUtils.isUrl(Location))
            {
                MetaData = FileMetaDataFactory.read(Location, CancellationToken.None);

                if (MetaData != null && MetaData.Thumbnail.Count > 0)
                {
                    Thumbnail = MetaData.Thumbnail[0].ThumbImage;
                    Thumbnail.Freeze();
                }
            }

            if (Data != null)
            {
                Data.Position = 0;
            }
        }

        public virtual void generateThumbnails(int nrThumbnails = 1)
        {
            if (Thumbnail != null)
            {
                Thumbnail.Freeze();
            }

            if (Data != null)
            {
                Data.Position = 0;
            }
        }
     
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

        public BitmapSource Thumbnail
        {
            get { return thumbnail; }
            set { thumbnail = value; }
        }                               

        public virtual string DefaultCaption
        {
            get {

              
              return ("");
                
            
            }
        }

        public virtual string DefaultFormatCaption
        {

            get { return (""); }
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
