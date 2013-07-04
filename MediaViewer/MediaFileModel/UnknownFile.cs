using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaViewer.MetaData;

namespace MediaViewer.MediaFileModel
{
    class UnknownFile : MediaFile
    {

        public UnknownFile(String location, Stream data)
            : base(location, null, data, MetaDataMode.AUTO)
        {

        }
        
        public override void generateThumbnails(int nrThumbnails)
        {

            Thumbnail = null;

            base.generateThumbnails();
        }

        public override MediaType MediaFormat
        {
            get
            {

                return (MediaType.UNKNOWN);
            }
        }

    }
}
