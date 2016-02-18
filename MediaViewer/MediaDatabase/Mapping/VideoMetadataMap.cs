using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace MediaViewer.MediaDatabase.Mapping
{
    public class VideoMetadataMap : EntityTypeConfiguration<VideoMetadata>
    {
        public VideoMetadataMap()
        {
            // Primary Key
            /*this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("VideoMetadata");
            this.Property(t => t.Width).HasColumnName("Width");
            this.Property(t => t.Height).HasColumnName("Height");
            this.Property(t => t.BitsPerSample).HasColumnName("BitsPerSample");
            this.Property(t => t.DurationSeconds).HasColumnName("DurationSeconds");
            this.Property(t => t.FramesPerSecond).HasColumnName("FramesPerSecond");
            this.Property(t => t.NrChannels).HasColumnName("NrChannels");
            this.Property(t => t.PixelFormat).HasColumnName("PixelFormat");
            this.Property(t => t.SamplesPerSecond).HasColumnName("SamplesPerSecond");
            this.Property(t => t.VideoCodec).HasColumnName("VideoCodec");
            this.Property(t => t.VideoContainer).HasColumnName("VideoContainer");
            this.Property(t => t.AudioCodec).HasColumnName("AudioCodec");
            this.Property(t => t.MajorBrand).HasColumnName("MajorBrand");
            this.Property(t => t.MinorVersion).HasColumnName("MinorVersion");
            this.Property(t => t.IsVariableBitRate).HasColumnName("IsVariableBitRate");
            this.Property(t => t.WMFSDKVersion).HasColumnName("WMFSDKVersion");
            this.Property(t => t.BitsPerPixel).HasColumnName("BitsPerPixel");
            this.Property(t => t.Id).HasColumnName("Id");*/

            // Relationships
            //this.HasRequired(t => t.BaseMetadata)
            //    .WithOptional(t => t.VideoMetadata);

        }
    }
}
