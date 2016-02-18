using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace MediaViewer.MediaDatabase.Mapping
{
    public class PresetMetadataMap : EntityTypeConfiguration<PresetMetadata>
    {
        public PresetMetadataMap()
        {
            // Primary Key
            /*this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Name)
                .IsRequired();

            this.Property(t => t.TimeStamp)
                .IsRequired()
                .IsFixedLength()
                .HasMaxLength(8)
                .IsRowVersion();

            // Table & Column Mappings
            this.ToTable("PresetMetadata");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Title).HasColumnName("Title");
            this.Property(t => t.Rating).HasColumnName("Rating");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.Author).HasColumnName("Author");
            this.Property(t => t.Copyright).HasColumnName("Copyright");
            this.Property(t => t.IsNameEnabled).HasColumnName("IsNameEnabled");
            this.Property(t => t.IsTitleEnabled).HasColumnName("IsTitleEnabled");
            this.Property(t => t.IsRatingEnabled).HasColumnName("IsRatingEnabled");
            this.Property(t => t.IsDescriptionEnabled).HasColumnName("IsDescriptionEnabled");
            this.Property(t => t.IsAuthorEnabled).HasColumnName("IsAuthorEnabled");
            this.Property(t => t.IsCopyrightEnabled).HasColumnName("IsCopyrightEnabled");
            this.Property(t => t.CreationDate).HasColumnName("CreationDate");
            this.Property(t => t.IsCreationDateEnabled).HasColumnName("IsCreationDateEnabled");
            this.Property(t => t.TimeStamp).HasColumnName("TimeStamp");*/

            // Relationships
            this.HasMany(t => t.Tags)
                .WithMany(t => t.PresetMetadatas)
                .Map(m =>
                    {
                        m.ToTable("PresetMetadataTag");
                        m.MapLeftKey("PresetMetadataTag_Tag_Id");
                        m.MapRightKey("Tags_Id");
                    });


        }
    }
}
