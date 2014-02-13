using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.Search;
using MediaViewer.UserControls.Relation;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase.DbCommands
{
    class MediaDbCommands : DbCommands
    {
        public MediaDbCommands(MediaDatabaseContext existingContext = null) :
            base(existingContext)
        {
            
        }

        public int getNrMediaInLocation(String location)
        {
            int result = Db.MediaSet.Where(m => m.Location.StartsWith(location)).Count();

            return (result);
        }

        public Media findMediaByLocation(String location)
        {
            Media result = Db.MediaSet.Include("Tags").FirstOrDefault(m => m.Location.Equals(location));
           
            return (result);
        }

        public List<Media> findMediaByQuery(SearchQuery query)
        {

            List<int> tagIds = new List<int>();
            foreach (Tag tag in query.Tags)
            {
                tagIds.Add(tag.Id);
            }

            var result = Db.MediaSet.Include("Tags").Where(m => m.Tags.Select(t => t.Id).Intersect(tagIds).Count() == tagIds.Count);

            // duration

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

            if (query.SearchType == MediaType.Video)
            {
                result = result.Where(m => m.MimeType.StartsWith("video"));

                result = videoQueryFilter(result, query);
            }
            else if (query.SearchType == MediaType.Images)
            {
                result = result.Where(m => m.MimeType.StartsWith("image"));
            }
           
            return(result.ToList());
        }

        IQueryable<Media> videoQueryFilter(IQueryable<Media> result, SearchQuery query)
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

    
        public Media createMedia(Media media)
        {
            if (String.IsNullOrEmpty(media.Location) || String.IsNullOrWhiteSpace(media.Location))
            {
                throw new DbEntityValidationException("Error creating media, location cannot be null, empty or whitespace");
            }

            if (Db.MediaSet.Any(t => t.Location == media.Location))
            {
                throw new DbEntityValidationException("Cannot create media with duplicate location: " + media.Location);
            }
            Media newMedia = null;
            
            if (media is VideoMedia)
            {
                VideoMedia video = new VideoMedia(media.Location, null);
                Db.MediaSet.Add(video);

                Db.Entry<VideoMedia>(video).CurrentValues.SetValues(media);
                newMedia = video;
            }
            else
            {
                ImageMedia image = new ImageMedia(media.Location, null);
                Db.MediaSet.Add(image);

                Db.Entry<ImageMedia>(image).CurrentValues.SetValues(media);
                newMedia = image;
            }

            FileInfo info = new FileInfo(media.Location);
            info.Refresh();
            newMedia.LastModifiedDate = info.LastWriteTime;
           
            newMedia.Id = 0;
    
            if (media.Thumbnail != null && media.Thumbnail.Id != 0)
            {
                //thumbnail already exists
                newMedia.Thumbnail = Db.ThumbnailSet.FirstOrDefault(t => t.Id == media.Thumbnail.Id);
                newMedia.Thumbnail.decodeImage();
            }
            else
            {

                newMedia.Thumbnail = media.Thumbnail;
            }

            TagDbCommands tagCommands = new TagDbCommands(Db);

            foreach (Tag tag in media.Tags)
            {
                Tag result = tagCommands.getTagByName(tag.Name);

                if (result == null)
                {
                    Tag newTag = tagCommands.createTag(tag);
                    newMedia.Tags.Add(newTag);
                }
                else
                {
                    newMedia.Tags.Add(result);
                }
            }
       
            Db.SaveChanges();

            newMedia.IsImported = true;

            return (newMedia);
        }

        public Media updateMedia(Media media)
        {
            if (String.IsNullOrEmpty(media.Location) || String.IsNullOrWhiteSpace(media.Location))
            {
                throw new DbEntityValidationException("Error updating media, location cannot be null, empty or whitespace");
            }
            
            Media updateMedia = Db.MediaSet.FirstOrDefault(t => t.Id == media.Id);
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

                updateMedia.Thumbnail = media.Thumbnail;
            }

            updateMedia.Tags.Clear();
            TagDbCommands tagCommands = new TagDbCommands(Db);

            foreach (Tag tag in media.Tags)
            {
                Tag result = tagCommands.getTagByName(tag.Name);

                if (result == null)
                {
                    Tag newTag = tagCommands.createTag(tag);
                    updateMedia.Tags.Add(newTag);
                }
                else
                {
                    updateMedia.Tags.Add(result);
                }
            }

            Db.SaveChanges();

            updateMedia.IsImported = true;

            return (updateMedia);
        }

        public void deleteMedia(Media media) {

            if (String.IsNullOrEmpty(media.Location) || String.IsNullOrWhiteSpace(media.Location))
            {
                throw new DbEntityValidationException("Error deleting media, location cannot be null, empty or whitespace");
            }

            Media deleteMedia = findMediaByLocation(media.Location);
            if (deleteMedia == null)
            {
                throw new DbEntityValidationException("Cannot delete non existing media: " + media.Location);
            }

            if(deleteMedia.Thumbnail != null) {
                Db.ThumbnailSet.Remove(deleteMedia.Thumbnail);
            }

            Db.MediaSet.Remove(deleteMedia);
            Db.SaveChanges();

            media.IsImported = false;
        }
    }
}
