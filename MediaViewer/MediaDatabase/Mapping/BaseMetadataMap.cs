using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace MediaViewer.MediaDatabase.Mapping
{
    public class BaseMetadataMap : EntityTypeConfiguration<BaseMetadata>
    {
        public BaseMetadataMap()
        {
            // Primary Key
            /*this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Location)
                .IsRequired();

            this.Property(t => t.MimeType)
                .IsRequired();

            this.Property(t => t.TimeStamp)
                .IsRequired()
                .IsFixedLength()
                .HasMaxLength(8)
                .IsRowVersion();

            // Table & Column Mappings
            this.ToTable("BaseMetadata");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Location).HasColumnName("Location");
            this.Property(t => t.Title).HasColumnName("Title");
            this.Property(t => t.Rating).HasColumnName("Rating");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.Author).HasColumnName("Author");
            this.Property(t => t.Copyright).HasColumnName("Copyright");
            this.Property(t => t.LastModifiedDate).HasColumnName("LastModifiedDate");
            this.Property(t => t.CreationDate).HasColumnName("CreationDate");
            this.Property(t => t.MetadataModifiedDate).HasColumnName("MetadataModifiedDate");
            this.Property(t => t.MetadataDate).HasColumnName("MetadataDate");
            this.Property(t => t.MimeType).HasColumnName("MimeType");
            this.Property(t => t.SizeBytes).HasColumnName("SizeBytes");
            this.Property(t => t.Software).HasColumnName("Software");
            this.Property(t => t.SupportsXMPMetadata).HasColumnName("SupportsXMPMetadata");
            this.Property(t => t.TimeStamp).HasColumnName("TimeStamp");
            this.Property(t => t.Latitude).HasColumnName("Latitude");
            this.Property(t => t.Longitude).HasColumnName("Longitude");
            this.Property(t => t.FileDate).HasColumnName("FileDate");*/

            // Relationships
            /*this.HasMany(t => t.Tags)
                .WithMany(t => t.BaseMetadatas)
                .Map(m =>
                    {
                        m.ToTable("BaseMetadataTag");
                        m.MapLeftKey("BaseMetadatas_Id");
                        m.MapRightKey("Tags_Id");
                    });*/


        }
    }
}
