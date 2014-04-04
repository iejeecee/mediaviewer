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
        /// <summary>
        /// Remember to call unregistermessages before the tagtreepicker is in garbage collect mode
        /// otherwise "dead" tagtreepickers will respond to notify calls and mess up the treelist
        /// </summary>
        public TagTreePickerViewModel()
        {         

            GlobalMessenger.Instance.Register<TagCategory>("tagCategory_Created", addCategory);
            GlobalMessenger.Instance.Register<TagCategory>("tagCategory_Deleted", removeCategory);
            GlobalMessenger.Instance.Register<TagCategory>("tagCategory_Updated", updateCategory);

            GlobalMessenger.Instance.Register<Tag>("tag_Created", addTag);
            GlobalMessenger.Instance.Register<Tag>("tag_Deleted", removeTag);
            GlobalMessenger.Instance.Register<Tag>("tag_Updated", updateTag);
                   
        }

        public void unregisterMessages()
        {
            GlobalMessenger.Instance.UnRegister<TagCategory>("tagCategory_Created", addCategory);
            GlobalMessenger.Instance.UnRegister<TagCategory>("tagCategory_Deleted", removeCategory);
            GlobalMessenger.Instance.UnRegister<TagCategory>("tagCategory_Updated", updateCategory);

            GlobalMessenger.Instance.UnRegister<Tag>("tag_Created", addTag);
            GlobalMessenger.Instance.UnRegister<Tag>("tag_Deleted", removeTag);
            GlobalMessenger.Instance.UnRegister<Tag>("tag_Updated", updateTag);
        }

        void addCategory(TagCategory category)
        {
            CategoryItem newItem = new CategoryItem(category);

            insertTagTreePickerItem(root, newItem, 0, getNrCategories());
        }

        void removeCategory(TagCategory category)
        {
            for (int i = 0; i < getNrCategories(); i++)
            {
                CategoryItem item = root[i] as CategoryItem;

                if (item.Category.Id == category.Id)
                {
                    root.RemoveAt(i);
                    break;
                }
            }
        }

        void updateCategory(TagCategory category)
        {
            for (int i = 0; i < getNrCategories(); i++)
            {
                CategoryItem item = root[i] as CategoryItem;

                if (item.Category.Id == category.Id)
                {
                    item.Category = category;
                    item.Name = category.Name;
                    break;
                }

            }
        }

        void updateTag(Tag tag)
        {
            removeTag(tag);
            addTag(tag);
        }

        void removeTag(Tag tag)
        {
            for (int i = 0; i < getNrCategories(); i++)
            {
                CategoryItem categoryItem = root[i] as CategoryItem;

                if (categoryItem.Children != null)
                {
                    for (int j = 0; j < categoryItem.Children.Count; j++)
                    {
                        TagItem tagItem = categoryItem.Children[j] as TagItem;
                        if (tagItem.Tag.Id == tag.Id)
                        {
                            categoryItem.Children.RemoveAt(j);
                            return;
                        }
                    }
                }
            }
         
            for (int i = getNrCategories(); i < root.Count; i++)
            {
                TagItem item = root[i] as TagItem;

                if (item.Tag.Id == tag.Id)
                {
                    root.RemoveAt(i);
                    return;
                }
            }
            
        }

        void addTag(Tag tag)
        {
            TagItem newItem = new TagItem(tag);

            if (tag.TagCategory == null)
            {
                insertTagTreePickerItem(root, newItem, getNrCategories(), root.Count);
            }
            else
            {
                CategoryItem item = getCategoryById(tag.TagCategory.Id);

                if (item.Children != null)
                {
                    insertTagTreePickerItem(item.Children, newItem, 0, item.Children.Count);
                }
            }
        }

        int getNrCategories()
        {
            int count = 0;

            while (count < root.Count && root[count] is CategoryItem)
            {
                count++;
            }

            return (count);
        }

        void insertTagTreePickerItem(ObservableCollection<TagTreePickerItem> list, TagTreePickerItem item, int start, int end)
        {
            bool isInserted = false;

            for (int i = start; i < end; i++)
            {
                if (list[i].CompareTo(item) <= 0)
                {
                    list.Insert(i, item);
                    isInserted = true;
                    break;
                }                
            }

            if (isInserted == false)
            {
                list.Insert(end, item);
            }
        }

   

        CategoryItem getCategoryById(int id)
        {
            int i = 0;

            while (i < root.Count && root[i] is CategoryItem)
            {
                CategoryItem item = root[i] as CategoryItem;

                if (item.Category.Id == id)
                {
                    return (item);
                }

                i++;
            }

            return (null);
        }

    
        ObservableCollection<TagTreePickerItem> root;

        public System.Collections.IEnumerable GetChildren(object parent)
        {
            if (parent == null)
            {
                using (TagCategoryDbCommands categoryCommands = new TagCategoryDbCommands())
                {
                    List<TagCategory> categories = categoryCommands.getAllCategories();
                    root = new ObservableCollection<TagTreePickerItem>();
                
                    foreach (TagCategory category in categories)
                    {
                        root.Add(new CategoryItem(category));
                    }

                    List<Tag> tags = categoryCommands.getTagsWithoutCategory();

                    foreach (Tag tag in tags)
                    {
                        root.Add(new TagItem(tag));
                    }

                    return (root);
                }
            }
            else if (parent is CategoryItem)
            {
                CategoryItem category = parent as CategoryItem;
              
                using (TagCategoryDbCommands categoryCommands = new TagCategoryDbCommands())
                {
                    List<Tag> tags = categoryCommands.getTagsByCategory(category.Category);

                    category.Children = new ObservableCollection<TagTreePickerItem>();

                    foreach (Tag tag in tags)
                    {
                        category.Children.Add(new TagItem(tag));
                    }

                    return (category.Children);
                }
                
            }            

            return (null);
        }

        public bool HasChildren(object parent)
        {
            if (parent is TagItem)
            {
                return (false);
                
            }
            else if (parent is CategoryItem)
            {
                CategoryItem category = parent as CategoryItem;
              
                using (TagCategoryDbCommands categoryCommands = new TagCategoryDbCommands())
                {
                    int nrTags = categoryCommands.getNrTagsInCategory((parent as CategoryItem).Category);

                    if (nrTags == 0) return (false);
                    else return (true);

                }
                
            }

            return (false);
        }

      
    }
}
