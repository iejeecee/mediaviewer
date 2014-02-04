using MediaViewer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase
{
    partial class ImageMedia
    {
        public ImageMedia()
        {

        }

        public ImageMedia(String location, Stream data) : base(location, data)  
        {
            
        }

        public override string DefaultFormatCaption
        {
            get
            {
                /*
                if (OpenError != null)
                {
                    return OpenError.Message;

                }
                */

                StringBuilder sb = new StringBuilder();

                sb.AppendLine(Path.GetFileName(Location));
                sb.AppendLine();

                sb.AppendLine("Mime type:");
                sb.Append(MimeType);
                sb.AppendLine();
                sb.AppendLine();

                sb.AppendLine("Resolution:");
                sb.Append(Width);
                sb.Append("x");
                sb.Append(Height);
                sb.AppendLine();
                sb.AppendLine();

                sb.AppendLine("Size:");
                sb.Append(Misc.formatSizeBytes(SizeBytes));

                return (sb.ToString());
            }
        }
    }
}
