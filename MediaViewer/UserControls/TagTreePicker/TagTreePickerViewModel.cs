using Aga.Controls.Tree;
using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.UserControls.TagTreePicker
{
    class TagTreePickerViewModel : ObservableObject, ITreeModel
    {
        ObservableCollection<CategoryItem> root;

        public System.Collections.IEnumerable GetChildren(object parent)
        {
            if (parent == null)
            {
                using (TagCategoryDbCommands categoryCommands = new TagCategoryDbCommands())
                {
                    List<TagCategory> categories = categoryCommands.getAllCategories();
                    root = new ObservableCollection<CategoryItem>();

                    CategoryItem allTags = new CategoryItem(null);
                    allTags.IsAllTagsCategory = true;
                    allTags.Name = "All";
                    root.Add(allTags);

                    foreach (TagCategory category in categories)
                    {
                        root.Add(new CategoryItem(category));
                    }

                    return (root);
                }
            }
            else if (parent is CategoryItem)
            {
                CategoryItem category = parent as CategoryItem;

                if (category.IsAllTagsCategory)
                {
                    using (TagDbCommands tagCommands = new TagDbCommands())
                    {
                        List<Tag> tags = tagCommands.getAllTags();
                        List<TagItem> items = new List<TagItem>();

                        foreach (Tag tag in tags)
                        {
                            items.Add(new TagItem(tag, category));
                        }

                        return (items);   
                    }

                }
                else
                {

                    using (TagCategoryDbCommands categoryCommands = new TagCategoryDbCommands())
                    {
                        List<Tag> tags = categoryCommands.getTagsByCategory(category.Category);
                        List<TagItem> items = new List<TagItem>();

                        foreach (Tag tag in tags)
                        {
                            items.Add(new TagItem(tag, category));
                        }

                        return (items);
                    }
                }
            }
            else if (parent is TagItem)
            {
                using (TagDbCommands tagCommands = new TagDbCommands())
                {
                    Tag tag = tagCommands.getTagById((parent as TagItem).Tag.Id);
                    List<TagItem> items = new List<TagItem>();

                    foreach (Tag childTag in tag.ChildTags)
                    {
                        items.Add(new TagItem(childTag, parent as TagItem));
                    }

                    return(items);                   
                }
            }

            return (null);
        }

        public bool HasChildren(object parent)
        {
            if (parent is TagItem)
            {
                if ((parent as TagItem).Parent is TagItem)
                {
                    return (false);
                }
                else
                {
                    using (TagDbCommands tagCommands = new TagDbCommands())
                    {
                        int nrChildTags = tagCommands.getNrChildTags((parent as TagItem).Tag);

                        if (nrChildTags == 0) return (false);
                        else return (true);
                    }
                }
            }
            else if (parent is CategoryItem)
            {
                CategoryItem category = parent as CategoryItem;

                if (category.IsAllTagsCategory)
                {
                    using (TagDbCommands tagCommands = new TagDbCommands())
                    {
                        int nrTags = tagCommands.getNrTags();

                        if (nrTags == 0) return (false);
                        else return (true);
                    }
                }
                else
                {
                    using (TagCategoryDbCommands categoryCommands = new TagCategoryDbCommands())
                    {
                        int nrTags = categoryCommands.getNrTagsInCategory((parent as CategoryItem).Category);

                        if (nrTags == 0) return (false);
                        else return (true);

                    }
                }
            }

            return (false);
        }
    }
}
