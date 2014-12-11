using MediaViewer.Infrastructure.Logging;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Search;
using MediaViewer.UserControls.Relation;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Validation;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase.DbCommands
{
    class MediaDbCommands : DbCommands<BaseMedia>
    {
        

        public MediaDbCommands(MediaDatabaseContext existingContext = null) :
            base(existingContext)
        {
            
        }

        public int getNrMediaInLocation(String location)
        {
            int result = Db.BaseMediaSet.Where(m => m.Location.StartsWith(location)).Count();

            return (result);
        }

        public List<BaseMedia> getMediaInLocation(String location)
        {
            List<BaseMedia> result = Db.BaseMediaSet.Where(m => m.Location.StartsWith(location)).ToList();

            return (result);
        }

        public BaseMedia findMediaByLocation(String location)
        {            
            BaseMedia result = Db.BaseMediaSet.Include("Tags").FirstOrDefault(m => m.Location.Equals(location));
           
            return (result);
        }

        public List<BaseMedia> findMediaByQuery(SearchQuery query)
        {
            IQueryable<BaseMedia> result = textQuery(query);

            result = tagQuery(result, query);

            if (result == null)
            {
                return (new List<BaseMedia>());
            }

            // creation

            if (query.CreationStart != null && query.CreationEnd == null)
            {
                result = result.Where(m => m.CreationDate >= query.CreationStart.Value);
            }
            else if (query.CreationStart == null && query.CreationEnd!= null)
            {
                result = result.Where(m => m.CreationDate <= query.CreationEnd.Value);
            }
            else if (query.CreationStart != null && query.CreationEnd != null)
            {
                result = result.Where(m => (m.CreationDate >= query.CreationStart.Value) && (m.CreationDate <= query.CreationEnd.Value));
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

            if (query.SearchType == MediaType.Video)
            {               
                result = videoQueryFilter(result, query);
            }
            else if (query.SearchType == MediaType.Images)
            {                
                result = imageQueryFilter(result, query);
            }
           
            return(result.ToList());
        }

        IQueryable<BaseMedia> textQuery(SearchQuery query)
        {
            IQueryable<BaseMedia> result = null;

            if (String.IsNullOrEmpty(query.Text) || String.IsNullOrWhiteSpace(query.Text))
            {
                return (result);
            }
           
            if (query.SearchType == MediaType.All)
            {

                result = Db.BaseMediaSet.Include("Tags").Where(m =>
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
                result = Db.BaseMediaSet.Include("Tags").OfType<ImageMedia>().Where(m =>
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
                result = Db.BaseMediaSet.Include("Tags").OfType<VideoMedia>().Where(m =>
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

            return (result);
        }

        IQueryable<BaseMedia> tagQuery(IQueryable<BaseMedia> result, SearchQuery query)
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
                    result = Db.BaseMediaSet.Include("Tags").Where(m => m.Tags.Select(t => t.Id).Intersect(tagIds).Count() == tagIds.Count);
                }
                else if (query.SearchType == MediaType.Video)
                {
                    result = Db.BaseMediaSet.Include("Tags").OfType<VideoMedia>().Where(m => m.Tags.Select(t => t.Id).Intersect(tagIds).Count() == tagIds.Count);
                }
                else if (query.SearchType == MediaType.Images)
                {
                    result = Db.BaseMediaSet.Include("Tags").OfType<ImageMedia>().Where(m => m.Tags.Select(t => t.Id).Intersect(tagIds).Count() == tagIds.Count);
                }
            }
            else
            {
                result = result.Where(m => m.Tags.Select(t => t.Id).Intersect(tagIds).Count() == tagIds.Count);
            }
                          
            return (result);
        }


        IQueryable<BaseMedia> imageQueryFilter(IQueryable<BaseMedia> result, SearchQuery query)
        {
            // width

            if (query.ImageWidthStart != null && query.ImageWidthEnd == null)
            {
                result = result.OfType<ImageMedia>().Where(m => m.Width >= query.ImageWidthStart.Value);
            }
            else if (query.ImageWidthStart == null && query.ImageWidthEnd != null)
            {
                result = result.OfType<ImageMedia>().Where(m => m.Width <= query.ImageWidthEnd.Value);
            }
            else if (query.ImageWidthStart != null && query.ImageWidthEnd != null)
            {
                result = result.OfType<ImageMedia>().Where(m => (m.Width >= query.ImageWidthStart.Value) && (m.Width <= query.ImageWidthEnd.Value));
            }

            // height

            if (query.ImageHeightStart != null && query.ImageHeightEnd == null)
            {
                result = result.OfType<ImageMedia>().Where(m => m.Height >= query.ImageHeightStart.Value);
            }
            else if (query.ImageHeightStart == null && query.ImageHeightEnd != null)
            {
                result = result.OfType<ImageMedia>().Where(m => m.Height <= query.ImageHeightEnd.Value);
            }
            else if (query.ImageHeightStart != null && query.ImageHeightEnd != null)
            {
                result = result.OfType<ImageMedia>().Where(m => (m.Height >= query.ImageHeightStart.Value) && (m.Height <= query.ImageHeightEnd.Value));
            }           

            return (result);
        }

        IQueryable<BaseMedia> videoQueryFilter(IQueryable<BaseMedia> result, SearchQuery query)
        {
            // duration

            if (query.DurationSecondsStart != null && query.DurationSecondsEnd == null)
            {              
                result = result.OfType<VideoMedia>().Where(m => m.DurationSeconds >= query.DurationSecondsStart.Value);
            }
            else if (query.DurationSecondsStart == null && query.DurationSecondsEnd != null)
            {               
                result = result.OfType<VideoMedia>().Where(m => m.DurationSeconds <= query.DurationSecondsEnd.Value);
            }
            else if (query.DurationSecondsStart != null && query.DurationSecondsEnd != null)
            {
                result = result.OfType<VideoMedia>().Where(m => (m.DurationSeconds >= query.DurationSecondsStart.Value) && (m.DurationSeconds <= query.DurationSecondsEnd.Value));
            }

            // width

            if (query.VideoWidthStart != null && query.VideoWidthEnd == null)
            {
                result = result.OfType<VideoMedia>().Where(m => m.Width >= query.VideoWidthStart.Value);
            }
            else if (query.VideoWidthStart == null && query.VideoWidthEnd != null)
            {
                result = result.OfType<VideoMedia>().Where(m => m.Width <= query.VideoWidthEnd.Value);
            }
            else if (query.VideoWidthStart != null && query.VideoWidthEnd != null)
            {
                result = result.OfType<VideoMedia>().Where(m => (m.Width >= query.VideoWidthStart.Value) && (m.Width <= query.VideoWidthEnd.Value));
            }

            // height

            if (query.VideoHeightStart != null && query.VideoHeightEnd == null)
            {
                result = result.OfType<VideoMedia>().Where(m => m.Height >= query.VideoHeightStart.Value);
            }
            else if (query.VideoHeightStart == null && query.VideoHeightEnd != null)
            {
                result = result.OfType<VideoMedia>().Where(m => m.Height <= query.VideoHeightEnd.Value);
            }
            else if (query.VideoHeightStart != null && query.VideoHeightEnd != null)
            {
                result = result.OfType<VideoMedia>().Where(m => (m.Height >= query.VideoHeightStart.Value) && (m.Height <= query.VideoHeightEnd.Value));
            }

            // frames per second

            if (query.FramesPerSecondStart != null && query.FramesPerSecondEnd == null)
            {
                result = result.OfType<VideoMedia>().Where(m => m.FramesPerSecond >= query.FramesPerSecondStart.Value);
            }
            else if (query.FramesPerSecondStart == null && query.FramesPerSecondEnd != null)
            {
                result = result.OfType<VideoMedia>().Where(m => m.FramesPerSecond <= query.FramesPerSecondEnd.Value);
            }
            else if (query.FramesPerSecondStart != null && query.FramesPerSecondEnd != null)
            {
                result = result.OfType<VideoMedia>().Where(m => (m.FramesPerSecond >= query.FramesPerSecondStart.Value) && (m.FramesPerSecond <= query.FramesPerSecondEnd.Value));
            }

            return result;
        }

    
        protected override BaseMedia createFunc(BaseMedia media)
        {
            if (String.IsNullOrEmpty(media.Location) || String.IsNullOrWhiteSpace(media.Location))
            {
                throw new DbEntityValidationException("Error creating media, location cannot be null, empty or whitespace");
            }

            if (Db.BaseMediaSet.Any(t => t.Location == media.Location))
            {
                throw new DbEntityValidationException("Cannot create media with duplicate location: " + media.Location);
            }
           

            BaseMedia newMedia = null;
            
            if (media is VideoMedia)
            {
                VideoMedia video = new VideoMedia(media.Location, null);
                Db.BaseMediaSet.Add(video);

                Db.Entry<VideoMedia>(video).CurrentValues.SetValues(media);
                newMedia = video;
            }
            else
            {
                ImageMedia image = new ImageMedia(media.Location, null);
                Db.BaseMediaSet.Add(image);

                Db.Entry<ImageMedia>(image).CurrentValues.SetValues(media);
                newMedia = image;
            }
          
            FileInfo info = new FileInfo(media.Location);
            info.Refresh();

            if (info.LastWriteTime < (DateTime)SqlDateTime.MinValue)
            {

                Logger.Log.Warn("LastWriteTime for " + media.Location + " smaller as SqlDateTime.MinValue");
                newMedia.LastModifiedDate = (DateTime)SqlDateTime.MinValue;

            } else {

                newMedia.LastModifiedDate =  info.LastWriteTime;
            }
            
           
            newMedia.Id = 0;
    
            if (media.Thumbnail != null && media.Thumbnail.Id != 0)
            {
                //thumbnail already exists
                newMedia.Thumbnail = Db.ThumbnailSet.FirstOrDefault(t => t.Id == media.Thumbnail.Id);
                newMedia.Thumbnail.decodeImage();
            }
            else
            {
                if (media.Thumbnail != null)
                {
                    Db.ThumbnailSet.Add(media.Thumbnail);
                }
                newMedia.Thumbnail = media.Thumbnail;               
            }          

            TagDbCommands tagCommands = new TagDbCommands(Db);

            foreach (Tag tag in media.Tags)
            {               
                Tag result = tagCommands.getTagByName(tag.Name);

                if (result == null)
                {
                    Tag newTag = tagCommands.create(tag);
                    newTag.Used = 1;
                    newMedia.Tags.Add(newTag);
                }
                else
                {
                    result.Used += 1;                   
                    newMedia.Tags.Add(result);
                   
                }
               
            }

            Db.SaveChanges();

            newMedia.IsImported = true;

            return (newMedia);
        }

        protected override BaseMedia updateFunc(BaseMedia media)
        {
            if (String.IsNullOrEmpty(media.Location) || String.IsNullOrWhiteSpace(media.Location))
            {
                throw new DbEntityValidationException("Error updating media, location cannot be null, empty or whitespace");
            }
            
            BaseMedia updateMedia = Db.BaseMediaSet.FirstOrDefault(t => t.Id == media.Id);
            if (updateMedia == null)
            {
                throw new DbEntityValidationException("Cannot update non existing media: " + media.Id.ToString());
            }

            if (media is VideoMedia)
            {
                Db.Entry<VideoMedia>(updateMedia as VideoMedia).CurrentValues.SetValues(media);                
            }
            else
            {
                Db.Entry<ImageMedia>(updateMedia as ImageMedia).CurrentValues.SetValues(media);      
            }

            FileInfo info = new FileInfo(media.Location);
            info.Refresh();
            updateMedia.LastModifiedDate = info.LastWriteTime;

            if (media.Thumbnail != null && media.Thumbnail.Id != 0)
            {
                //thumbnail already exists
                updateMedia.Thumbnail = Db.ThumbnailSet.FirstOrDefault(t => t.Id == media.Thumbnail.Id);
                updateMedia.Thumbnail.decodeImage();
            }
            else
            {
                if (updateMedia.Thumbnail != null)
                {
                    // remove old thumbnail
                    Db.ThumbnailSet.Remove(updateMedia.Thumbnail);
                }

                if (media.Thumbnail != null)
                {
                    Db.ThumbnailSet.Add(media.Thumbnail);
                }

                updateMedia.Thumbnail = media.Thumbnail;
            }

            TagDbCommands tagCommands = new TagDbCommands(Db);

            // remove tags
            for(int i = updateMedia.Tags.Count - 1; i >= 0; i--)
            {
                Tag tag = updateMedia.Tags.ElementAt(i);

                if (!media.Tags.Contains(tag, EqualityComparer<Tag>.Default))
                {                    
                    updateMedia.Tags.Remove(tag);
                    tag.Used -= 1;
                }
            }
            
            // add tags
            foreach (Tag tag in media.Tags)
            {
                Tag result = tagCommands.getTagByName(tag.Name);

                if (result == null)
                {
                    result = tagCommands.create(tag);                  
                }

                if (!updateMedia.Tags.Contains(result, EqualityComparer<Tag>.Default))
                {
                    updateMedia.Tags.Add(result);
                    result.Used += 1;
                }
            }
                       
            Db.SaveChanges();
                      
            updateMedia.IsImported = true;

            return (updateMedia);
        }

        protected override void deleteFunc(BaseMedia media)
        {

            if (String.IsNullOrEmpty(media.Location) || String.IsNullOrWhiteSpace(media.Location))
            {
                throw new DbEntityValidationException("Error deleting media, location cannot be null, empty or whitespace");
            }

            BaseMedia deleteMedia = findMediaByLocation(media.Location);
            if (deleteMedia == null)
            {
                throw new DbEntityValidationException("Cannot delete non existing media: " + media.Location);
            }
        
            foreach (Tag tag in deleteMedia.Tags)
            {
                tag.Used -= 1;
            }

            if(deleteMedia.Thumbnail != null) {
                Db.ThumbnailSet.Remove(deleteMedia.Thumbnail);
                deleteMedia.Thumbnail = null;
            }

            Db.BaseMediaSet.Remove(deleteMedia);
            Db.SaveChanges();

            media.Id = 0;
            if (media.Thumbnail != null)
            {
                media.Thumbnail.Id = 0;
                // make sure there is no lingering connection to the removed media entity
                // otherwise when we attach this thumbnail to a new entiy
                // the framework will bring in the (cached?) dead entity and mess things up
                media.Thumbnail.BaseMedia = null;
            }
          

            media.IsImported = false;
        }

        public override void clearAll()
        {
            String[] tableNames = new String[] {"ThumbnailSet", "MediaTag", "MediaSet_ImageMedia",
                "MediaSet_VideoMedia","MediaSet_UnknownMedia","BaseMediaSet"};

            for (int i = 0; i < tableNames.Count(); i++)
            {
                Db.Database.ExecuteSqlCommand("TRUNCATE TABLE [" + tableNames[i] + "]");
            }
           
        }
    }
}
