using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.UserControls.TagTreePicker
{
    class CategoryItem : TagTreePickerItem, IComparable<CategoryItem>, IEquatable<CategoryItem>
    {
        TagCategory category;

        public TagCategory Category
        {
            get { return category; }
            set
            {
                category = value;

                if (category != null)
                {
                    Name = category.Name;
                }
                else
                {
                    Name = "";
                }
            }
        }

        public CategoryItem(TagCategory category)
        {           
            Category = category;
                                
            LazyLoading = true;
        }

        public override object Text
        {           
            get
            {
                return Name;
            }
        }

        public override object Icon
        {
            get
            {
                return loadIcon("folder_back.ico");
            }
        }

        protected override void LoadChildren()
        {
            try
            {
                if (Category == null)
                {
                    using (TagCategoryDbCommands categoryCommands = new TagCategoryDbCommands())
                    {
                        List<TagCategory> categories = categoryCommands.getAllCategories();

                        foreach (TagCategory category in categories)
                        {
                            Children.Add(new CategoryItem(category));
                        }
                    }

                    using (TagCategoryDbCommands categoryCommands = new TagCategoryDbCommands())
                    {
                        List<Tag> tags = categoryCommands.getTagsWithoutCategory();

                        foreach (Tag tag in tags)
                        {
                            Children.Add(new TagItem(tag));
                        }
                    }
                }
                else
                {
                    using (TagCategoryDbCommands categoryCommands = new TagCategoryDbCommands())
                    {
                        List<Tag> tags = categoryCommands.getTagsByCategory(Category);

                        foreach (Tag tag in tags)
                        {
                            Children.Add(new TagItem(tag));
                        }
                    }
                }

                IsLoaded = true;
            }
            catch
            {
            }
        }

        public int CompareTo(CategoryItem other)
        {
            if (other == null)
            {
                throw new ArgumentException();
            }

            return (other.Name.CompareTo(Name));
        }


        public bool Equals(CategoryItem other)
        {
            if (other == null)
            {
                throw new ArgumentException();
            }

            return (other.Category.Id == Category.Id);
        }
    }
}
