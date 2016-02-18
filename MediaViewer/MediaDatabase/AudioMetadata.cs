using MediaViewer.Model.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;

namespace MediaViewer.MediaDatabase
{
    public partial class AudioMetadata : BaseMetadata
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


        public int DurationSeconds { get; set; }
        public short NrChannels { get; set; }
        public int SamplesPerSecond { get; set; }
        public short BitsPerSample { get; set; }

        [Required]
        public string AudioCodec { get; set; }

        public string Genre { get; set; }
        public string Album { get; set; }
        public Nullable<int> TrackNr { get; set; }
        public Nullable<int> TotalTracks { get; set; }
        public Nullable<int> DiscNr { get; set; }
        public Nullable<int> TotalDiscs { get; set; }
        public string AudioContainer { get; set; }      
        //public virtual BaseMetadata BaseMetadata { get; set; }
    }
}
