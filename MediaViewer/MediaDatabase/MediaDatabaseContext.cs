using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//http://msdn.microsoft.com/en-us/data/jj193542.aspx

namespace MediaViewer.MediaDatabase
{
    class MediaDatabaseContext : DbContext
    {

        public DbSet<TagCategory> TagCategories { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
           
            modelBuilder.Entity<Tag>()
                .HasMany(t => t.LinkedTags);
        }
    }

}
