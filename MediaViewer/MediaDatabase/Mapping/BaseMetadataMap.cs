using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase.Mapping
{
    public class BaseMetadataMap : EntityTypeConfiguration<BaseMetadata>
    {
        public BaseMetadataMap()
        {
            this.Property(t => t.LocationHash).HasColumnName("LocationHash")
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_LocationHash")));

            this.Property(t => t.LocationNameHash).HasColumnName("LocationNameHash")
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_LocationNameHash")));
        }
    }
}
