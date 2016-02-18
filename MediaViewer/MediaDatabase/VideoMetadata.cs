using MediaViewer.Model.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MediaViewer.MediaDatabase
{
    public partial class VideoMetadata : BaseMetadata
    {
        public VideoMetadata()
        {
            this.VideoThumbnails = new List<VideoThumbnail>();
        }

        public VideoMetadata(String location, Stream data) : base(location, data)
        {
            VideoThumbnails = new HashSet<VideoThumbnail>();
        }

        public override void clear()
        {
            base.clear();

            VideoThumbnails = new HashSet<VideoThumbnail>(); 
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
    

        public int Width { get; set; }
        public int Height { get; set; }
        public Nullable<short> BitsPerSample { get; set; }
        public int DurationSeconds { get; set; }
        public double FramesPerSecond { get; set; }
        public Nullable<short> NrChannels { get; set; }
        public string PixelFormat { get; set; }
        public Nullable<int> SamplesPerSecond { get; set; }
        public string VideoCodec { get; set; }
        public string VideoContainer { get; set; }
        public string AudioCodec { get; set; }
        public string MajorBrand { get; set; }
        public Nullable<int> MinorVersion { get; set; }
        public Nullable<bool> IsVariableBitRate { get; set; }
        public string WMFSDKVersion { get; set; }
        public Nullable<short> BitsPerPixel { get; set; }
        //public int Id { get; set; }
        //public virtual BaseMetadata BaseMetadata { get; set; }
        public virtual ICollection<VideoThumbnail> VideoThumbnails { get; set; }
    }
}
