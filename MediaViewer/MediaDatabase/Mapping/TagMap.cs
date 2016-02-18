using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace MediaViewer.MediaDatabase.Mapping
{
    public class TagMap : EntityTypeConfiguration<Tag>
    {
        public TagMap()
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
            this.ToTable("Tag");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Used).HasColumnName("Used");
            this.Property(t => t.TimeStamp).HasColumnName("TimeStamp");*/

            // Relationships
            this.HasMany(t => t.ParentTags)
                .WithMany(t => t.ChildTags)
                .Map(m =>
                    {
                        m.ToTable("ParentTagChildTag");
                        m.MapLeftKey("ParentTag_Id");
                        m.MapRightKey("ChildTag_Id");
                    });


        }
    }
}
