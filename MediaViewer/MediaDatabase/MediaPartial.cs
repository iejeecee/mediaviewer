// Note that a custom SSDL to DDL script is used to generate timestamp columns for concurrency checks, see:
// http://msdn.microsoft.com/en-us/library/vstudio/dd560887%28v=vs.100%29.aspx
// http://www.undisciplinedbytes.com/2012/03/creating-a-timestamp-column-with-entity-framework/
// the script location is: 
// C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\Extensions\Microsoft\Entity Framework Tools\DBGen\SSDLToSQL10_CustomTimestamp.tt
using MediaViewer.Utils;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase
{
    [Serializable]
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
