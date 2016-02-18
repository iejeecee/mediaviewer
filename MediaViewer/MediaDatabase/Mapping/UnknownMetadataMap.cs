using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace MediaViewer.MediaDatabase.Mapping
{
    public class UnknownMetadataMap : EntityTypeConfiguration<UnknownMetadata>
    {
        public UnknownMetadataMap()
        {
            // Primary Key
            /*this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("UnknownMetadata");
            this.Property(t => t.Id).HasColumnName("Id");*/

            // Relationships
            //this.HasRequired(t => t.BaseMetadata)
            //    .WithOptional(t => t.UnknownMetadata);

        }
    }
}
