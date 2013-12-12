using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase
{
    class TagDbCommands : DbCommands
    {
       
        public TagDbCommands(MediaDatabaseContext existingContext) :
            base(existingContext)
        {
            
        }

        public List<TagCategory> getAllCategories()
        {
            List<TagCategory> categories = new List<TagCategory>();

            foreach (TagCategory category in Db.TagCategorySet.OrderBy(x => x.Name))
            {
                categories.Add(category);
            }

            return (categories);
           
        }

        public List<Tag> getAllTags()
        {
            List<Tag> tags = new List<Tag>();
         
            foreach (Tag tag in Db.TagSet.Where(x => x.ParentTagId == null).OrderBy(x => x.Name))
            {
                tags.Add(tag);
            }
          
            return (tags);
        }

        public Tag getTagByName(String name)
        {               
            List<Tag> result = (from b in Db.TagSet.Include("LinkedTags").Include("TagCategory")
                        where b.Name == name && b.ParentTagId == null
                        select b).ToList();

            if (result.Count == 0) return (null);
            else return (result[0]);                           
        }

        public List<Tag> getTagsByCategory(String categoryName)
        {
            List<Tag> result = (from b in Db.TagSet.Include("LinkedTags").Include("Category")
                                where b.TagCategory.Name == categoryName
                                select b).ToList();

            if (result == null) return (null);
            else return (result);
        }
       
    }
}
