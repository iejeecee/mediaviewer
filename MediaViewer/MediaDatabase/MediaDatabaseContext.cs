using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using MediaViewer.MediaDatabase.Mapping;

namespace MediaViewer.MediaDatabase
{
    public partial class MediaDatabaseContext : DbContext
    {
        static MediaDatabaseContext()
        {
            //Database.SetInitializer<MediaDatabaseContext>(new CreateDatabaseIfNotExists<MediaDatabaseContext>());
            Database.SetInitializer<MediaDatabaseContext>(new DropCreateDatabaseIfModelChanges<MediaDatabaseContext>());
        }

        public MediaDatabaseContext()  : base("MediaDatabaseContext")        
        {                       
        }

        public DbSet<AudioMetadata> AudioMetadatas { get; set; }
        public DbSet<BaseMetadata> BaseMetadatas { get; set; }
        public DbSet<ImageMetadata> ImageMetadatas { get; set; }
        public DbSet<PresetMetadata> PresetMetadatas { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Thumbnail> Thumbnails { get; set; }
        public DbSet<UnknownMetadata> UnknownMetadatas { get; set; }
        public DbSet<VideoMetadata> VideoMetadatas { get; set; }
        //public DbSet<VideoThumbnail> VideoThumbnails { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {            
            modelBuilder.Configurations.Add(new AudioMetadataMap());
            modelBuilder.Configurations.Add(new BaseMetadataMap());
            modelBuilder.Configurations.Add(new ImageMetadataMap());
            modelBuilder.Configurations.Add(new PresetMetadataMap());
            modelBuilder.Configurations.Add(new TagMap());           
            modelBuilder.Configurations.Add(new ThumbnailMap());
            modelBuilder.Configurations.Add(new UnknownMetadataMap());
            modelBuilder.Configurations.Add(new VideoMetadataMap());
            //modelBuilder.Configurations.Add(new VideoThumbnailMap());
        }
    }
}
