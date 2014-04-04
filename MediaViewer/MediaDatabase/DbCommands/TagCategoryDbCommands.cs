using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase.DbCommands
{
    class TagCategoryDbCommands : DbCommands<TagCategory>
    {
        public TagCategoryDbCommands(MediaDatabaseContext existingContext = null) :
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

        public List<Tag> getTagsByCategory(TagCategory category)
        {
            TagCategory c = getCategoryById(category.Id);

            if (c == null)
            {
                throw new DbEntityValidationException("Category does not exist: " + category.Id);
            }
          
            List<Tag> result = c.Tag.OrderBy(tag => tag.Name).ToList();

            return (result);
        }

        public List<Tag> getTagsWithoutCategory()
        {
            List<Tag> result = Db.TagSet.Where(tag => tag.TagCategory == null).OrderBy(tag => tag.Name).ToList();
      
            return (result);
        }

        public int getNrTagsInCategory(TagCategory category)
        {
            TagCategory c = getCategoryById(category.Id);

            if (c == null)
            {
                throw new DbEntityValidationException("Category does not exist: " + category.Id);
            }

            return (c.Tag.Count);
        }

        public TagCategory getCategoryById(int id)
        {
            TagCategory category = Db.TagCategorySet.FirstOrDefault<TagCategory>(c => c.Id == id);
            return (category);
        }

        public List<TagCategory> getCategoryAutocompleteMatches(String name)
        {
            List<TagCategory> result = Db.TagCategorySet.Where(t => t.Name.StartsWith(name)).OrderBy(t => t.Name).ToList();
            return (result);
        }

        protected override TagCategory createFunc(TagCategory tagCategory)
        {
            if (String.IsNullOrEmpty(tagCategory.Name) || String.IsNullOrWhiteSpace(tagCategory.Name))
            {
                return (null);
            }

            TagCategory result = null;

            if (Db.TagCategorySet.Any(t => t.Name == tagCategory.Name))
            {
                throw new DbEntityValidationException("Cannot create duplicate category: " + tagCategory.Name);
            }

            TagCategory newTagCategory = new TagCategory();
            Db.TagCategorySet.Add(newTagCategory);

            Db.Entry<TagCategory>(newTagCategory).CurrentValues.SetValues(tagCategory);
            newTagCategory.Id = 0;

            result = Db.TagCategorySet.Add(newTagCategory);
            Db.SaveChanges();

            return (result);
        }

        public override TagCategory update(TagCategory updateCategory)
        {

            TagCategory oldCategory = getCategoryById(updateCategory.Id);

            if (oldCategory == null)
            {
                throw new DbEntityValidationException("Cannot update non-existing category with id: " + updateCategory.Id.ToString());
            }

            Db.Entry<TagCategory>(oldCategory).CurrentValues.SetValues(updateCategory);

           
            Db.SaveChanges();

            return (oldCategory);
        }

       

        protected override void deleteFunc(TagCategory tagCategory)
        {
            if (String.IsNullOrEmpty(tagCategory.Name) || String.IsNullOrWhiteSpace(tagCategory.Name))
            {
                return;
            }

            TagCategory deleteCategory = getCategoryById(tagCategory.Id);
            if (deleteCategory == null)
            {
                throw new DbEntityValidationException("Cannot delete non-existent category: " + tagCategory.Id.ToString());
            }

            foreach (Tag tag in Db.TagSet.Where(t => t.TagCategoryId == deleteCategory.Id))
            {
                tag.TagCategoryId = null;
                tag.TagCategory = null;
            }

            Db.TagCategorySet.Remove(deleteCategory);

            Db.SaveChanges();
        }
       
    
    }
}
