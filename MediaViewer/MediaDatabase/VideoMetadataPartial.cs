using MediaViewer.Model.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase
{
    [Serializable]
    partial class VideoMetadata
    {
        public VideoMetadata()
        {

        }

        public VideoMetadata(String location, Stream data) : base(location, data)
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

                sb.Append("Video Codec (");
                sb.Append(VideoCodec);
                sb.AppendLine("):");
                sb.Append(Width);
                sb.Append("x");
                sb.Append(Height);
                sb.Append(", " + PixelFormat + ", " + FramesPerSecond.ToString("0.00") + " fps");
                sb.AppendLine();
                sb.AppendLine();

                if (AudioCodec != null)
                {

                    sb.Append("Audio Codec (");
                    sb.Append(AudioCodec);
                    sb.AppendLine("):");
                    sb.Append(SamplesPerSecond);
                    sb.Append("Hz, ");
                    sb.Append(BitsPerSample);
                    sb.Append("bit, ");
                    sb.Append(NrChannels);
                    sb.Append(" chan");
                    sb.AppendLine();
                    sb.AppendLine();
                }

                sb.AppendLine("Duration:");
                sb.AppendLine(MiscUtils.formatTimeSeconds(DurationSeconds));
                sb.AppendLine();

                sb.AppendLine("Size:");
                sb.Append(MiscUtils.formatSizeBytes(SizeBytes));         

                return (sb.ToString());
            }
        }
    }
}
