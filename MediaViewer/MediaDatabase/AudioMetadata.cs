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

                sb.Append("Audio Codec (");
                sb.Append(AudioCodec);
                sb.AppendLine("):");
                sb.Append(SamplesPerSecond);
                sb.Append("Hz, ");
                sb.Append(BitsPerSample);
                sb.Append("bit, ");
                sb.Append(NrChannels);
                sb.Append(" chan");

                if (BitRate.HasValue)
                {
                    sb.Append(", " + MiscUtils.formatSizeBytes(BitRate.Value / 8) + "/s");
                }

                sb.AppendLine();
                sb.AppendLine();

                sb.AppendLine("Duration:");
                sb.Append(MiscUtils.formatTimeSeconds(DurationSeconds));

                sb.AppendLine();
                sb.AppendLine();

                sb.AppendLine("Size:");
                sb.Append(MiscUtils.formatSizeBytes(SizeBytes));
                           
                return (sb.ToString());
            }
        }


        public int DurationSeconds { get; set; }
        public short NrChannels { get; set; }
        public int SamplesPerSecond { get; set; }
        public short BitsPerSample { get; set; }
        public Nullable<long> BitRate { get; set; }

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
