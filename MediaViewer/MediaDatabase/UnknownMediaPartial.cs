using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase
{
    partial class UnknownMedia
    {
        public UnknownMedia()
        {

        }

        public UnknownMedia(String location) : base(location, null)
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

                return (Location);
            }
        }

    }
}
