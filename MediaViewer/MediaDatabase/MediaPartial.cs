using MediaViewer.Utils;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase
{
    partial class Media : ObservableObject
    {
        
        protected Media(String location, Stream data)
        {
            Location = location;
            Data = data;
            MimeType = MediaFormatConvert.fileNameToMimeType(location);

            Tags = new HashSet<Tag>();
            IsImported = false;
            metadataReadError = null;
        }

        Stream data;

        public Stream Data
        {
            set { data = value; }
            get { return data; }           
        }

        Exception metadataReadError;

        public Exception MetadataReadError
        {
            get { return metadataReadError; }
            set { metadataReadError = value; }
        }

        bool isImported;

        public bool IsImported
        {
            get { return isImported; }
            set { isImported = value;
            NotifyPropertyChanged();
            }
        }

        public abstract String DefaultFormatCaption
        {
            get;            
        }

        public virtual void clear()
        {
            Title = null;
            Description = null;
            Author = null;
            Software = null;
            Copyright = null;
            Rating = null;
            Thumbnail = null;
            Tags = new HashSet<Tag>();
            CreationDate = null;
            MetadataModifiedDate = null;
            MetadataDate = null;       
        }

        public virtual void close()
        {
            if (data != null)
            {
                data.Close();
                data = null;
            }
        }

      
    }
}
