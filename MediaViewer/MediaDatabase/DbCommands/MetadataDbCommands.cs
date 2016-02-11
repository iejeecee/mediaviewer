using MediaViewer.Infrastructure.Logging;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Search;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase.DbCommands
{
    class MetadataDbCommands : DbCommands<BaseMetadata>
    {
        
        public MetadataDbCommands(MediaDatabaseContext existingContext = null) :
            base(existingContext)
        {
            
        }

        public List<BaseMetadata> getAllMetadata()
        {
            return(Db.BaseMetadataSet.ToList());
        }

        public int getNrMetadata()
        {
            int result = Db.BaseMetadataSet.Count();

            return (result);
        }

        public int getNrImageMetadata()
        {
            int result = Db.BaseMetadataSet.OfType<ImageMetadata>().Count();

            return (result);
        }

        public int getNrVideoMetadata()
        {
            int result = Db.BaseMetadataSet.OfType<VideoMetadata>().Count();

            return (result);
        }

        public int getNrMetadataInLocation(String location)
        {
            int result = Db.BaseMetadataSet.Where(m => m.Location.StartsWith(location)).Count();

            return (result);
        }

        public List<BaseMetadata> getMetadataInLocation(String location)
        {
            List<BaseMetadata> result = Db.BaseMetadataSet.Where(m => m.Location.StartsWith(location)).ToList();

            return (result);
        }

        public BaseMetadata findMetadataByLocation(String location)
        {            
            BaseMetadata result = Db.BaseMetadataSet.Include(m => m.Tags).Include(m => m.Thumbnail).FirstOrDefault(m => m.Location.Equals(location));
           
            return (result);
        }

        public List<BaseMetadata> findMetadataByQuery(SearchQuery query)
        {
            IQueryable<BaseMetadata> result = textQuery(query);

            result = tagQuery(result, query);

            if (result == null)
            {
                result = allItems(query);
            }

            // media creation date

            if (query.CreationStart != null && query.CreationEnd == null)
            {
                result = result.Where(m => m.CreationDate >= query.CreationStart.Value);
            }
            else if (query.CreationStart == null && query.CreationEnd != null)
            {
                result = result.Where(m => m.CreationDate <= query.CreationEnd.Value);
            }
            else if (query.CreationStart != null && query.CreationEnd != null)
            {
                result = result.Where(m => (m.CreationDate >= query.CreationStart.Value) && (m.CreationDate <= query.CreationEnd.Value));
            }

            // file creation date

            if (query.FileDateStart != null && query.FileDateEnd == null)
            {
                result = result.Where(m => m.FileDate >= query.FileDateStart.Value);
            }
            else if (query.FileDateStart == null && query.FileDateEnd != null)
            {
                result = result.Where(m => m.FileDate <= query.FileDateEnd.Value);
            }
            else if (query.FileDateStart != null && query.FileDateEnd != null)
            {
                result = result.Where(m => (m.FileDate >= query.FileDateStart.Value) && (m.FileDate <= query.FileDateEnd.Value));
            }

            // rating

            if (query.RatingStart != null && query.RatingEnd == null)
            {
                result = result.Where(m => m.Rating >= query.RatingStart.Value * 5);
            }
            else if (query.RatingStart == null && query.RatingEnd != null)
            {
                result = result.Where(m => m.Rating <= query.RatingEnd.Value * 5);
            }
            else if (query.RatingStart != null && query.RatingEnd != null)
            {
                result = result.Where(m => (m.Rating >= query.RatingStart.Value * 5) && (m.Rating <= query.RatingEnd.Value * 5));
            }

            // nr tags

            if (query.NrTagsStart != null && query.NrTagsEnd == null)
            {
                result = result.Where(m => m.Tags.Count >= query.NrTagsStart.Value);
            }
            else if (query.NrTagsStart == null && query.NrTagsEnd != null)
            {
                result = result.Where(m => m.Tags.Count <= query.NrTagsEnd.Value);
            }
            else if (query.NrTagsStart != null && query.NrTagsEnd != null)
            {
                result = result.Where(m => (m.Tags.Count >= query.NrTagsStart.Value) && (m.Tags.Count <= query.NrTagsEnd.Value));
            }

            // is inside geo location rectangle
            if (query.GeoLocationRect != null)
            {
                result = result.Where(m =>
                    (m.Latitude <= query.GeoLocationRect.North) &&
                    (m.Latitude >= query.GeoLocationRect.South) &&
                    (m.Longitude >= query.GeoLocationRect.West) &&
                    (m.Longitude <= query.GeoLocationRect.East));
            }


            if (query.SearchType == MediaType.Video)
            {               
                result = videoQueryFilter(result, query);
            }
            else if (query.SearchType == MediaType.Images)
            {                
                result = imageQueryFilter(result, query);
            }
            else if (query.SearchType == MediaType.Audio)
            {
                result = audioQueryFilter(result, query);
            }
                                   
            return(result.Include(m => m.Tags).Include(m => m.Thumbnail).ToList());
        }

        IQueryable<BaseMetadata> allItems(SearchQuery query)
        {
            IQueryable<BaseMetadata> result = null;

            if (query.SearchType == MediaType.All)
            {
                result = Db.BaseMetadataSet;
            }
            else if (query.SearchType == MediaType.Images)
            {
                result = Db.BaseMetadataSet.OfType<ImageMetadata>();
            }
            else if (query.SearchType == MediaType.Video)
            {
                result = Db.BaseMetadataSet.OfType<VideoMetadata>();
            }
            else if (query.SearchType == MediaType.Audio)
            {
                result = Db.BaseMetadataSet.OfType<AudioMetadata>();
            }

            return (result);
        }

        IQueryable<BaseMetadata> textQuery(SearchQuery query)
        {
            IQueryable<BaseMetadata> result = null;

            if (String.IsNullOrEmpty(query.Text) || String.IsNullOrWhiteSpace(query.Text))
            {
                return (result);
            }
           
            if (query.SearchType == MediaType.All)
            {

                result = Db.BaseMetadataSet.Where(m =>
                       (m.Location.Contains(query.Text)) ||
                       (!String.IsNullOrEmpty(m.Title) && m.Title.Contains(query.Text)) ||
                       (!String.IsNullOrEmpty(m.Description) && m.Description.Contains(query.Text)) ||
                       (!String.IsNullOrEmpty(m.Author) && m.Author.Contains(query.Text)) ||
                       (!String.IsNullOrEmpty(m.Copyright) && m.Copyright.Contains(query.Text)) ||
                       (!String.IsNullOrEmpty(m.Software) && m.Software.Contains(query.Text))
                       );
            }
            else if (query.SearchType == MediaType.Images)
            {
                result = Db.BaseMetadataSet.OfType<ImageMetadata>().Where(m =>
                       (m.Location.Contains(query.Text)) ||
                       (!String.IsNullOrEmpty(m.Title) && m.Title.Contains(query.Text)) ||
                       (!String.IsNullOrEmpty(m.Description) && m.Description.Contains(query.Text)) ||
                       (!String.IsNullOrEmpty(m.Author) && m.Author.Contains(query.Text)) ||
                       (!String.IsNullOrEmpty(m.Copyright) && m.Copyright.Contains(query.Text)) ||
                       (!String.IsNullOrEmpty(m.Software) && m.Software.Contains(query.Text)) ||
                       (!String.IsNullOrEmpty(m.CameraMake) && m.CameraMake.Contains(query.Text)) ||
                       (!String.IsNullOrEmpty(m.CameraModel) && m.CameraModel.Contains(query.Text)) ||
                       (!String.IsNullOrEmpty(m.Lens) && m.Lens.Contains(query.Text))
                       );
            }
            else if (query.SearchType == MediaType.Video)
            {              
                result = Db.BaseMetadataSet.OfType<VideoMetadata>().Where(m =>
                        (m.Location.Contains(query.Text)) ||
                        (!String.IsNullOrEmpty(m.Title) && m.Title.Contains(query.Text)) ||
                        (!String.IsNullOrEmpty(m.Description) && m.Description.Contains(query.Text)) ||
                        (!String.IsNullOrEmpty(m.Author) && m.Author.Contains(query.Text)) ||
                        (!String.IsNullOrEmpty(m.Copyright) && m.Copyright.Contains(query.Text)) ||
                        (!String.IsNullOrEmpty(m.Software) && m.Software.Contains(query.Text)) ||
                        (!String.IsNullOrEmpty(m.AudioCodec) && m.AudioCodec.Contains(query.Text)) ||
                        (!String.IsNullOrEmpty(m.VideoCodec) && m.VideoCodec.Contains(query.Text)) ||
                        (!String.IsNullOrEmpty(m.VideoContainer) && m.VideoContainer.Contains(query.Text))
                        );
               
            }
            else if (query.SearchType == MediaType.Audio)
            {
                result = Db.BaseMetadataSet.OfType<AudioMetadata>().Where(m =>
                        (m.Location.Contains(query.Text)) ||
                        (!String.IsNullOrEmpty(m.Title) && m.Title.Contains(query.Text)) ||
                        (!String.IsNullOrEmpty(m.Description) && m.Description.Contains(query.Text)) ||
                        (!String.IsNullOrEmpty(m.Author) && m.Author.Contains(query.Text)) ||
                        (!String.IsNullOrEmpty(m.Copyright) && m.Copyright.Contains(query.Text)) ||
                        (!String.IsNullOrEmpty(m.Software) && m.Software.Contains(query.Text)) ||
                        (!String.IsNullOrEmpty(m.AudioCodec) && m.AudioCodec.Contains(query.Text)) ||
                        (!String.IsNullOrEmpty(m.Genre) && m.Genre.Contains(query.Text)) ||
                        (!String.IsNullOrEmpty(m.Album) && m.Album.Contains(query.Text))
                        );
            }

            return (result);
        }

        IQueryable<BaseMetadata> tagQuery(IQueryable<BaseMetadata> result, SearchQuery query)
        {
            if (query.Tags.Count == 0)
            {
                return(result);
            }
            List<int> tagIds = new List<int>();
            foreach (Tag tag in query.Tags)
            {
                tagIds.Add(tag.Id);
            }

            if (result == null)
            {
                if (query.SearchType == MediaType.All)
                {
                    result = Db.BaseMetadataSet.Where(m => m.Tags.Select(t => t.Id).Intersect(tagIds).Count() == tagIds.Count);
                    
                }
                else if (query.SearchType == MediaType.Video)
                {
                    result = Db.BaseMetadataSet.OfType<VideoMetadata>().Where(m => m.Tags.Select(t => t.Id).Intersect(tagIds).Count() == tagIds.Count);
                }
                else if (query.SearchType == MediaType.Images)
                {
                    result = Db.BaseMetadataSet.OfType<ImageMetadata>().Where(m => m.Tags.Select(t => t.Id).Intersect(tagIds).Count() == tagIds.Count);
                }
                else if (query.SearchType == MediaType.Audio)
                {
                    result = Db.BaseMetadataSet.OfType<AudioMetadata>().Where(m => m.Tags.Select(t => t.Id).Intersect(tagIds).Count() == tagIds.Count);
                }
            }
            else
            {
                result = result.Where(m => m.Tags.Select(t => t.Id).Intersect(tagIds).Count() == tagIds.Count);
            }
                          
            return (result);
        }


        IQueryable<BaseMetadata> imageQueryFilter(IQueryable<BaseMetadata> result, SearchQuery query)
        {
            // width

            if (query.ImageWidthStart != null && query.ImageWidthEnd == null)
            {
                result = result.OfType<ImageMetadata>().Where(m => m.Width >= query.ImageWidthStart.Value);
            }
            else if (query.ImageWidthStart == null && query.ImageWidthEnd != null)
            {
                result = result.OfType<ImageMetadata>().Where(m => m.Width <= query.ImageWidthEnd.Value);
            }
            else if (query.ImageWidthStart != null && query.ImageWidthEnd != null)
            {
                result = result.OfType<ImageMetadata>().Where(m => (m.Width >= query.ImageWidthStart.Value) && (m.Width <= query.ImageWidthEnd.Value));
            }

            // height

            if (query.ImageHeightStart != null && query.ImageHeightEnd == null)
            {
                result = result.OfType<ImageMetadata>().Where(m => m.Height >= query.ImageHeightStart.Value);
            }
            else if (query.ImageHeightStart == null && query.ImageHeightEnd != null)
            {
                result = result.OfType<ImageMetadata>().Where(m => m.Height <= query.ImageHeightEnd.Value);
            }
            else if (query.ImageHeightStart != null && query.ImageHeightEnd != null)
            {
                result = result.OfType<ImageMetadata>().Where(m => (m.Height >= query.ImageHeightStart.Value) && (m.Height <= query.ImageHeightEnd.Value));
            }           

            return (result);
        }

        IQueryable<BaseMetadata> videoQueryFilter(IQueryable<BaseMetadata> result, SearchQuery query)
        {
            // duration

            if (query.DurationSecondsStart != null && query.DurationSecondsEnd == null)
            {              
                result = result.OfType<VideoMetadata>().Where(m => m.DurationSeconds >= query.DurationSecondsStart.Value);
            }
            else if (query.DurationSecondsStart == null && query.DurationSecondsEnd != null)
            {               
                result = result.OfType<VideoMetadata>().Where(m => m.DurationSeconds <= query.DurationSecondsEnd.Value);
            }
            else if (query.DurationSecondsStart != null && query.DurationSecondsEnd != null)
            {
                result = result.OfType<VideoMetadata>().Where(m => (m.DurationSeconds >= query.DurationSecondsStart.Value) && (m.DurationSeconds <= query.DurationSecondsEnd.Value));
            }

            // width

            if (query.VideoWidthStart != null && query.VideoWidthEnd == null)
            {
                result = result.OfType<VideoMetadata>().Where(m => m.Width >= query.VideoWidthStart.Value);
            }
            else if (query.VideoWidthStart == null && query.VideoWidthEnd != null)
            {
                result = result.OfType<VideoMetadata>().Where(m => m.Width <= query.VideoWidthEnd.Value);
            }
            else if (query.VideoWidthStart != null && query.VideoWidthEnd != null)
            {
                result = result.OfType<VideoMetadata>().Where(m => (m.Width >= query.VideoWidthStart.Value) && (m.Width <= query.VideoWidthEnd.Value));
            }

            // height

            if (query.VideoHeightStart != null && query.VideoHeightEnd == null)
            {
                result = result.OfType<VideoMetadata>().Where(m => m.Height >= query.VideoHeightStart.Value);
            }
            else if (query.VideoHeightStart == null && query.VideoHeightEnd != null)
            {
                result = result.OfType<VideoMetadata>().Where(m => m.Height <= query.VideoHeightEnd.Value);
            }
            else if (query.VideoHeightStart != null && query.VideoHeightEnd != null)
            {
                result = result.OfType<VideoMetadata>().Where(m => (m.Height >= query.VideoHeightStart.Value) && (m.Height <= query.VideoHeightEnd.Value));
            }

            // frames per second

            if (query.FramesPerSecondStart != null && query.FramesPerSecondEnd == null)
            {
                result = result.OfType<VideoMetadata>().Where(m => m.FramesPerSecond >= query.FramesPerSecondStart.Value);
            }
            else if (query.FramesPerSecondStart == null && query.FramesPerSecondEnd != null)
            {
                result = result.OfType<VideoMetadata>().Where(m => m.FramesPerSecond <= query.FramesPerSecondEnd.Value);
            }
            else if (query.FramesPerSecondStart != null && query.FramesPerSecondEnd != null)
            {
                result = result.OfType<VideoMetadata>().Where(m => (m.FramesPerSecond >= query.FramesPerSecondStart.Value) && (m.FramesPerSecond <= query.FramesPerSecondEnd.Value));
            }

            // nr channels
            if (query.NrChannelsStart != null && query.NrChannelsEnd == null)
            {
                result = result.OfType<VideoMetadata>().Where(m => m.NrChannels >= query.NrChannelsStart.Value);
            }
            else if (query.NrChannelsStart == null && query.NrChannelsEnd != null)
            {
                result = result.OfType<VideoMetadata>().Where(m => m.NrChannels <= query.NrChannelsEnd.Value);
            }
            else if (query.NrChannelsStart != null && query.NrChannelsEnd != null)
            {
                result = result.OfType<VideoMetadata>().Where(m => (m.NrChannels >= query.NrChannelsStart.Value) && (m.NrChannels <= query.NrChannelsEnd.Value));
            }

            return result;
        }

        IQueryable<BaseMetadata> audioQueryFilter(IQueryable<BaseMetadata> result, SearchQuery query)
        {
            // duration
            if (query.DurationSecondsStart != null && query.DurationSecondsEnd == null)
            {
                result = result.OfType<AudioMetadata>().Where(m => m.DurationSeconds >= query.DurationSecondsStart.Value);
            }
            else if (query.DurationSecondsStart == null && query.DurationSecondsEnd != null)
            {
                result = result.OfType<AudioMetadata>().Where(m => m.DurationSeconds <= query.DurationSecondsEnd.Value);
            }
            else if (query.DurationSecondsStart != null && query.DurationSecondsEnd != null)
            {
                result = result.OfType<AudioMetadata>().Where(m => (m.DurationSeconds >= query.DurationSecondsStart.Value) && (m.DurationSeconds <= query.DurationSecondsEnd.Value));
            }

            // nr channels
            if (query.NrChannelsStart != null && query.NrChannelsEnd == null)
            {
                result = result.OfType<AudioMetadata>().Where(m => m.NrChannels >= query.NrChannelsStart.Value);
            }
            else if (query.NrChannelsStart == null && query.NrChannelsEnd != null)
            {
                result = result.OfType<AudioMetadata>().Where(m => m.NrChannels <= query.NrChannelsEnd.Value);
            }
            else if (query.NrChannelsStart != null && query.NrChannelsEnd != null)
            {
                result = result.OfType<AudioMetadata>().Where(m => (m.NrChannels >= query.NrChannelsStart.Value) && (m.NrChannels <= query.NrChannelsEnd.Value));
            }

            return result;
        }

    
        protected override BaseMetadata createFunc(BaseMetadata metadata)
        {
            if (String.IsNullOrEmpty(metadata.Location) || String.IsNullOrWhiteSpace(metadata.Location))
            {
                throw new DbEntityValidationException("Error creating metadata, location cannot be null, empty or whitespace");
            }

            if (Db.BaseMetadataSet.Any(t => t.Location == metadata.Location))
            {
                throw new DbEntityValidationException("Cannot create metadata with duplicate location: " + metadata.Location);
            }
           
            BaseMetadata newMetadata = null;
            
            if (metadata is VideoMetadata)
            {
                newMetadata = createVideoMetadata(metadata as VideoMetadata);
            }
            else if(metadata is ImageMetadata)
            {
                ImageMetadata imageMetadata = new ImageMetadata(metadata.Location, null);
                Db.BaseMetadataSet.Add(imageMetadata);

                Db.Entry<ImageMetadata>(imageMetadata).CurrentValues.SetValues(metadata);
                newMetadata = imageMetadata;
            }
            else if (metadata is AudioMetadata)
            {
                AudioMetadata audioMetadata = new AudioMetadata(metadata.Location, null);
                Db.BaseMetadataSet.Add(audioMetadata);

                Db.Entry<AudioMetadata>(audioMetadata).CurrentValues.SetValues(metadata);
                newMetadata = audioMetadata;
            }
          
            FileInfo info = new FileInfo(metadata.Location);
            info.Refresh();

            if (info.LastWriteTime < (DateTime)SqlDateTime.MinValue)
            {

                Logger.Log.Warn("LastWriteTime for " + metadata.Location + " smaller as SqlDateTime.MinValue");
                newMetadata.LastModifiedDate = (DateTime)SqlDateTime.MinValue;

            } else {

                newMetadata.LastModifiedDate =  info.LastWriteTime;
            }
                       
            newMetadata.Id = 0;
           
            if(metadata.Thumbnail != null)
            {
                if (metadata.Thumbnail.Id != 0)
                {
                    //thumbnail already exists
                    Thumbnail existing = Db.ThumbnailSet.FirstOrDefault(t => t.Id == metadata.Thumbnail.Id);             
                    newMetadata.Thumbnail = existing;
                }
                else
                {
                    Db.ThumbnailSet.Add(metadata.Thumbnail);
                    newMetadata.Thumbnail = metadata.Thumbnail;
                }
            }            
             
            TagDbCommands tagCommands = new TagDbCommands(Db);

            foreach (Tag tag in metadata.Tags)
            {               
                Tag result = tagCommands.getTagByName(tag.Name);

                if (result == null)
                {
                    result = tagCommands.create(tag);                    
                }
               
                result.Used += 1;                   
                newMetadata.Tags.Add(result);                                                  
            }
           
            int maxRetries = 15;

            do
            {
                try
                {
                    Db.SaveChanges();
                    maxRetries = 0;
                }
                catch (DbUpdateConcurrencyException e)
                {                    
                    if (--maxRetries == 0)
                    {
                        throw;
                    }

                    foreach (DbEntityEntry conflictingEntity in e.Entries)
                    {
                        if (conflictingEntity.Entity is Tag)
                        {
                            // reload the conflicting tag (database wins)
                            conflictingEntity.Reload();
                            (conflictingEntity.Entity as Tag).Used += 1;
                        }
                        else
                        {
                            throw;
                        }
                    }    
                
                    Random random = new Random();

                    Thread.Sleep(random.Next(50,100));
                }

            } while (maxRetries > 0);

            newMetadata.IsImported = true;

            return (newMetadata);
        }

        protected override BaseMetadata updateFunc(BaseMetadata metadata)
        {
            if (String.IsNullOrEmpty(metadata.Location) || String.IsNullOrWhiteSpace(metadata.Location))
            {
                throw new DbEntityValidationException("Error updating metadata, location cannot be null, empty or whitespace");
            }
            
            BaseMetadata updateMetadata = Db.BaseMetadataSet.FirstOrDefault(t => t.Id == metadata.Id);
            if (updateMetadata == null)
            {
                throw new DbEntityValidationException("Cannot update non existing metadata: " + metadata.Id.ToString());
            }

            if (metadata is VideoMetadata)
            {
                updateVideoMetadata(updateMetadata as VideoMetadata, metadata as VideoMetadata);
            }
            else if(metadata is ImageMetadata)
            {
                Db.Entry<ImageMetadata>(updateMetadata as ImageMetadata).CurrentValues.SetValues(metadata);      
            }
            else if (metadata is AudioMetadata)
            {
                Db.Entry<AudioMetadata>(updateMetadata as AudioMetadata).CurrentValues.SetValues(metadata);   
            }

            FileInfo info = new FileInfo(metadata.Location);
            info.Refresh();
            updateMetadata.LastModifiedDate = info.LastWriteTime;

            if (updateMetadata.Thumbnail != null)
            {                
                if ((metadata.Thumbnail != null && updateMetadata.Thumbnail.Id != metadata.Thumbnail.Id) ||
                    metadata.Thumbnail == null)
                {
                    Db.ThumbnailSet.Remove(updateMetadata.Thumbnail);                                  
                }               
            }

            if (metadata.Thumbnail != null)
            {
                if (metadata.Thumbnail.Id != 0)
                {
                    //thumbnail already exists
                    Thumbnail existing = Db.ThumbnailSet.FirstOrDefault(t => t.Id == metadata.Thumbnail.Id);
                    updateMetadata.Thumbnail = existing;
                }
                else
                {
                    Db.ThumbnailSet.Add(metadata.Thumbnail);
                    updateMetadata.Thumbnail = metadata.Thumbnail;
                }
            }            

                        
            TagDbCommands tagCommands = new TagDbCommands(Db);

            // remove tags
            for(int i = updateMetadata.Tags.Count - 1; i >= 0; i--)
            {
                Tag tag = updateMetadata.Tags.ElementAt(i);

                if (!metadata.Tags.Contains(tag, EqualityComparer<Tag>.Default))
                {                    
                    updateMetadata.Tags.Remove(tag);
                    tag.Used -= 1;
                }
            }
            
            // add tags
            foreach (Tag tag in metadata.Tags)
            {
                Tag result = tagCommands.getTagByName(tag.Name);

                if (result == null)
                {
                    result = tagCommands.create(tag);                  
                }

                if (!updateMetadata.Tags.Contains(result, EqualityComparer<Tag>.Default))
                {
                    updateMetadata.Tags.Add(result);
                    result.Used += 1;
                }
            }
                       
            Db.SaveChanges();
                      
            updateMetadata.IsImported = true;

            return (updateMetadata);
        }
        
        protected override void deleteFunc(BaseMetadata metadata)
        {

            if (String.IsNullOrEmpty(metadata.Location) || String.IsNullOrWhiteSpace(metadata.Location))
            {
                throw new DbEntityValidationException("Error deleting metadata, location cannot be null, empty or whitespace");
            }

            BaseMetadata deleteMedia = findMetadataByLocation(metadata.Location);
            if (deleteMedia == null)
            {
                throw new DbEntityValidationException("Cannot delete non existing metadata: " + metadata.Location);
            }

            if (deleteMedia is VideoMetadata)
            {
                deleteVideoMetadata(deleteMedia as VideoMetadata);
            }

            foreach (Tag tag in deleteMedia.Tags)
            {
                tag.Used -= 1;
            }

            if(deleteMedia.Thumbnail != null)
            {
                Db.ThumbnailSet.Remove(deleteMedia.Thumbnail);                      
            }

            deleteMedia.Thumbnail = null;

            Db.BaseMetadataSet.Remove(deleteMedia);
            Db.SaveChanges();

            metadata.Id = 0;
            /*if (metadata.Thumbnail != null)
            {
                metadata.Thumbnail.Id = 0;
                // make sure there is no lingering connection to the removed metadata entity
                // otherwise when we attach this thumbnail to a new entity
                // the framework will bring in the (cached?) dead entity and mess things up
                metadata.Thumbnail.BaseMetadata = null;
            }*/
          

            metadata.IsImported = false;
        }

        public override void clearAll()
        {
            throw new NotImplementedException();

            /*String[] tableNames = new String[] {"ThumbnailSet", "MediaTag", "MediaSet_ImageMetadata",
                "MediaSet_VideoMetadata","MediaSet_UnknownMetadata","BaseMetadataSet"};

            for (int i = 0; i < tableNames.Count(); i++)
            {
                Db.Database.ExecuteSqlCommand("TRUNCATE TABLE [" + tableNames[i] + "]");
            }*/
           
        }

        VideoMetadata createVideoMetadata(VideoMetadata metadata)
        {
            VideoMetadata videoMetadata = new VideoMetadata(metadata.Location, null);
            Db.BaseMetadataSet.Add(videoMetadata);

            Db.Entry<VideoMetadata>(videoMetadata).CurrentValues.SetValues(metadata);

            // add new or existing thumbnails
            foreach (VideoThumbnail thumb in metadata.VideoThumbnails)
            {
                if (thumb.Id != 0)
                {
                    //thumbnail already exists
                    VideoThumbnail existing = Db.VideoThumbnailSet.FirstOrDefault(t => t.Id == thumb.Id);
                    videoMetadata.VideoThumbnails.Add(existing);
                }
                else
                {
                    // new thumbnail
                    Db.VideoThumbnailSet.Add(thumb);
                    videoMetadata.VideoThumbnails.Add(thumb);
                }
            }

            return (videoMetadata);
        }

        void updateVideoMetadata(VideoMetadata updateMetadata, VideoMetadata metadata)
        {
            Db.Entry<VideoMetadata>(updateMetadata).CurrentValues.SetValues(metadata);

            // remove unused thumbnails
            for (int i = updateMetadata.VideoThumbnails.Count - 1; i >= 0; i--)
            {
                VideoThumbnail thumb = updateMetadata.VideoThumbnails.ElementAt(i);

                if (!metadata.VideoThumbnails.Any(t => t.Id == thumb.Id))
                {
                    Db.VideoThumbnailSet.Remove(thumb);                                 
                }
            }

            updateMetadata.VideoThumbnails.Clear();

            // add new or existing thumbnails
            foreach (VideoThumbnail thumb in metadata.VideoThumbnails)
            {
                if (thumb.Id != 0)
                {
                    //thumbnail already exists
                    VideoThumbnail existing = Db.VideoThumbnailSet.FirstOrDefault(t => t.Id == thumb.Id);
                    updateMetadata.VideoThumbnails.Add(existing);
                }
                else
                {
                    // new thumbnail
                    Db.VideoThumbnailSet.Add(thumb);
                    updateMetadata.VideoThumbnails.Add(thumb);
                }
            }
        }

        void deleteVideoMetadata(VideoMetadata metadata)
        {
            // remove unused thumbnails
            for (int i = metadata.VideoThumbnails.Count - 1; i >= 0; i--)
            {
                Db.VideoThumbnailSet.Remove(metadata.VideoThumbnails.ElementAt(i));                                
            }

            metadata.VideoThumbnails.Clear();
        }
       
    }
}
