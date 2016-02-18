using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace MediaViewer.MediaDatabase.Mapping
{
    public class VideoThumbnailMap : EntityTypeConfiguration<VideoThumbnail>
    {
        public VideoThumbnailMap()
        {
            // Primary Key
            /*this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.ImageData)
                .IsRequired();

            this.Property(t => t.TimeStamp)
                .IsRequired()
                .IsFixedLength()
                .HasMaxLength(8)
                .IsRowVersion();

            // Table & Column Mappings
            this.ToTable("VideoThumbnail");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ImageData).HasColumnName("ImageData");
            this.Property(t => t.Width).HasColumnName("Width");
            this.Property(t => t.Height).HasColumnName("Height");
            this.Property(t => t.TimeSeconds).HasColumnName("TimeSeconds");
            this.Property(t => t.TimeStamp).HasColumnName("TimeStamp");
            this.Property(t => t.VideoMetadataId).HasColumnName("VideoMetadataId");*/

            // Relationships
            /*this.HasRequired(t => t.VideoMetadata)
                .WithMany(t => t.VideoThumbnails)
                .HasForeignKey(d => d.VideoMetadataId);*/

        }
    }
}
