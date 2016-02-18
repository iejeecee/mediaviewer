using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace MediaViewer.MediaDatabase.Mapping
{
    public class ThumbnailMap : EntityTypeConfiguration<Thumbnail>
    {
        public ThumbnailMap()
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
            this.ToTable("Thumbnail");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ImageData).HasColumnName("ImageData");
            this.Property(t => t.Width).HasColumnName("Width");
            this.Property(t => t.Height).HasColumnName("Height");
            this.Property(t => t.TimeStamp).HasColumnName("TimeStamp");
            this.Property(t => t.BaseMetadataId).HasColumnName("BaseMetadata_Id");*/

            // Relationships
            /*this.HasRequired(t => t.BaseMetadata)
                .WithMany(t => t.Thumbnails)
                .HasForeignKey(d => d.BaseMetadataId);*/

        }
    }
}
