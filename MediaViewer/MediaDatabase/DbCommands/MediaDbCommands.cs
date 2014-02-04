using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.Search;
using MediaViewer.UserControls.Relation;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
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

            if (query.SearchType == MediaType.Video)
            {
                result = result.Where(m => m.MimeType.StartsWith("video"));
            }
            else if (query.SearchType == MediaType.Images)
            {
                result = result.Where(m => m.MimeType.StartsWith("image"));
            }

            if (query.DurationSeconds != null)
            {
                switch (query.DurationSecondsRelation)
                {
                    case RelationEnum.EQUAL:
                        {
                            result = result.OfType<VideoMedia>().Where(m => m.DurationSeconds == query.DurationSeconds.Value);
                            break;
                        }
                    case RelationEnum.GREATER_THAN_OR_EQUAL:
                        {
                            result = result.OfType<VideoMedia>().Where(m => m.DurationSeconds >= query.DurationSeconds.Value);
                            break;
                        }
                    case RelationEnum.LESS_THAN_OR_EQUAL:
                        {
                            result = result.OfType<VideoMedia>().Where(m => m.DurationSeconds <= query.DurationSeconds.Value);
                            break;
                        }
                }

            }

            if (query.VideoWidth != null)
            {
                switch (query.VideoWidthRelation)
                {
                    case RelationEnum.EQUAL:
                        {
                            result = result.OfType<VideoMedia>().Where(m => m.Width == query.VideoWidth.Value);
                            break;
                        }
                    case RelationEnum.GREATER_THAN_OR_EQUAL:
                        {
                            result = result.OfType<VideoMedia>().Where(m => m.Width >= query.VideoWidth.Value);
                            break;
                        }
                    case RelationEnum.LESS_THAN_OR_EQUAL:
                        {
                            result = result.OfType<VideoMedia>().Where(m => m.Width <= query.VideoWidth.Value);
                            break;
                        }
                }
            }

            if (query.VideoHeight != null)
            {
                switch (query.VideoHeightRelation)
                {
                    case RelationEnum.EQUAL:
                        {
                            result = result.OfType<VideoMedia>().Where(m => m.Height == query.VideoHeight.Value);
                            break;
                        }
                    case RelationEnum.GREATER_THAN_OR_EQUAL:
                        {
                            result = result.OfType<VideoMedia>().Where(m => m.Height >= query.VideoHeight.Value);
                            break;
                        }
                    case RelationEnum.LESS_THAN_OR_EQUAL:
                        {
                            result = result.OfType<VideoMedia>().Where(m => m.Height <= query.VideoHeight.Value);
                            break;
                        }
                }
              
            }

            if (query.FramesPerSecond != null)
            {
                switch (query.FramesPerSecondRelation)
                {
                    case RelationEnum.EQUAL:
                        {
                            result = result.OfType<VideoMedia>().Where(m => m.FramesPerSecond == query.FramesPerSecond.Value);
                            break;
                        }
                    case RelationEnum.GREATER_THAN_OR_EQUAL:
                        {
                            result = result.OfType<VideoMedia>().Where(m => m.FramesPerSecond >= query.FramesPerSecond.Value);
                            break;
                        }
                    case RelationEnum.LESS_THAN_OR_EQUAL:
                        {
                            result = result.OfType<VideoMedia>().Where(m => m.FramesPerSecond <= query.FramesPerSecond.Value);
                            break;
                        }
                }

            }

            return(result.OrderBy(m => m.Location).ToList());
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
      

            if (media.Thumbnail != null && media.Thumbnail.Id != 0)
            {
                //thumbnail already exists
                updateMedia.Thumbnail = Db.ThumbnailSet.FirstOrDefault(t => t.Id == media.Thumbnail.Id);
                updateMedia.Thumbnail.decodeImage();
            }
            else
            {

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
