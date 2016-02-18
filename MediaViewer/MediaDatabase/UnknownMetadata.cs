using System;
using System.Collections.Generic;

namespace MediaViewer.MediaDatabase
{
    public partial class UnknownMetadata : BaseMetadata
    {
        public UnknownMetadata()
        {

        }

        public UnknownMetadata(String location)
            : base(location, null)
        {
            Location = location;
            MimeType = "application/octet-stream";
            SupportsXMPMetadata = false;

        }

        public override string DefaultFormatCaption
        {
            get
            {
                if (MetadataReadError != null)
                {
                    return MetadataReadError.Message;
                }

                return ("Unknown media");
            }
        }


        //public int Id { get; set; }
        //public virtual BaseMetadata BaseMetadata { get; set; }
    }
}
