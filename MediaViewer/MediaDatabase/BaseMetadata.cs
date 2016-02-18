using MediaViewer.Model.Utils;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace MediaViewer.MediaDatabase
{
    public class BaseMetadata : BindableBase
    {
        public BaseMetadata()
        {           
            this.Tags = new List<Tag>();
        }

        protected BaseMetadata(String location, Stream data)
        {
            Tags = new List<Tag>();

            Location = location;
            Data = data;
            MimeType = MediaFormatConvert.fileNameToMimeType(location);            
            Thumbnail = null;

            isReadOnly = false;
            isImported = false;
            isModified = false;
            metadataReadError = null;
        }

        Stream data;

        [NotMapped]
        public Stream Data
        {
            set { data = value; }
            get { return data; }
        }

        Exception metadataReadError;

        [NotMapped]
        public Exception MetadataReadError
        {
            get { return metadataReadError; }
            set { metadataReadError = value; }
        }

        bool isImported;

        [NotMapped]
        public bool IsImported
        {
            get { return isImported; }
            set
            {

                SetProperty(ref isImported, value);
            }
        }

        bool isModified;

        [NotMapped]
        public bool IsModified
        {
            get { return isModified; }
            set
            {
                SetProperty(ref isModified, value);
            }
        }

        bool isReadOnly;

        [NotMapped]
        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set
            {
                SetProperty(ref isReadOnly, value);
            }

        }

        [NotMapped]
        public virtual String DefaultFormatCaption
        {
            get { return ""; }
        }

        public virtual void clear()
        {

            Author = null;
            Copyright = null;
            CreationDate = null;
            Description = null;

            Latitude = null;
            Longitude = null;

            MetadataDate = null;
            MetadataModifiedDate = null;

            Rating = null;

            Software = null;
            Tags = new HashSet<Tag>();
            Thumbnail = null;

            Title = null;

        }

        public virtual void close()
        {
            if (data != null)
            {
                data.Close();
                data = null;
            }
        }

        public int Id { get; set; }

        [Required]
        public string Location { get; set; }

        public string Title { get; set; }
        public Nullable<double> Rating { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Copyright { get; set; }
        public System.DateTime LastModifiedDate { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public Nullable<System.DateTime> MetadataModifiedDate { get; set; }
        public Nullable<System.DateTime> MetadataDate { get; set; }

        [Required]
        public string MimeType { get; set; }

        public long SizeBytes { get; set; }
        public string Software { get; set; }
        public bool SupportsXMPMetadata { get; set; }

        [Timestamp]
        public byte[] TimeStamp { get; set; }

        public Nullable<double> Latitude { get; set; }
        public Nullable<double> Longitude { get; set; }
        public System.DateTime FileDate { get; set; }        
        public virtual Thumbnail Thumbnail { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }

        /*public virtual AudioMetadata AudioMetadata { get; set; }
        public virtual ImageMetadata ImageMetadata { get; set; }
        public virtual UnknownMetadata UnknownMetadata { get; set; }
        public virtual VideoMetadata VideoMetadata { get; set; }
        */
    }
}
