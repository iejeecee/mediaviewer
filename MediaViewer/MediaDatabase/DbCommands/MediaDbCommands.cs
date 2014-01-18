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
            Media result = Db.MediaSet.FirstOrDefault(m => m.Location.Equals(location));
           
            return (result);
        }

        public List<Media> findMediaByTags(List<Tag> tags)
        {

            List<int> tagIds = new List<int>();
            foreach (Tag tag in tags)
            {
                tagIds.Add(tag.Id);
            }
           
            var result = Db.MediaSet.Include("Thumbnail").Where(m => m.Tags.Select(t => t.Id).Intersect(tagIds).Count() == tagIds.Count);
            return(result.ToList());
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

            Media newMedia = new Media();
            Db.MediaSet.Add(newMedia);
            
            Db.Entry<Media>(newMedia).CurrentValues.SetValues(media);
            newMedia.Id = 0;
    
            if (media.Thumbnail != null && media.Thumbnail.Id != 0)
            {
                //thumbnail already exists
                newMedia.Thumbnail = Db.ThumbnailSet.FirstOrDefault(t => t.Id == media.Thumbnail.Id);
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

            return (newMedia);
        }
    }
}
