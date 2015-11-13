using MediaViewer.Model.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase
{
    partial class AudioMetadata
    {
        public AudioMetadata()
        {
        }

        public AudioMetadata(String location, Stream data)
            : base(location, data)  
        {
            
        }

        public override string DefaultFormatCaption
        {
            get
            {
                if (MetadataReadError != null)
                {
                    return MetadataReadError.Message;
                }

                StringBuilder sb = new StringBuilder();

                sb.AppendLine(Path.GetFileName(Location));
                sb.AppendLine();

                sb.AppendLine("Mime type:");
                sb.Append(MimeType);
                sb.AppendLine();
                sb.AppendLine();

                sb.AppendLine("Duration:");
                sb.Append(MiscUtils.formatTimeSeconds(DurationSeconds));        
                sb.AppendLine();
                sb.AppendLine();

                sb.AppendLine("NrChannels:");
                sb.Append(NrChannels);
               
                return (sb.ToString());
            }
        }
    }
}
