using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Add/Attach and Entity States
//http://msdn.microsoft.com/en-us/data/jj592676.aspx

namespace MediaViewer.MetaData
{
   
    class LinkedTagEditorViewModel : ObservableObject 
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public static TagCategory NullCategory = new TagCategory() { Name = "None", Id = -1 };

        public LinkedTagEditorViewModel()
        {          
            ChildTags = new ObservableCollection<Tag>();

            using (TagDbCommands tc = new TagDbCommands())
            {
                Categories = new ObservableCollection<TagCategory>(tc.getAllCategories());              
                Tags = new ObservableCollection<Tag>(tc.getAllTags());             
            }

            SelectTagCommand = new Command(new Action(selectTag));
            SelectTagCommand.CanExecute = false;

            ClearTagCommand = new Command(new Action(clearTag));        

            CreateTagCommand = new Command(new Action(createTag));
            CreateTagCommand.CanExecute = false;

            UpdateTagCommand = new Command(new Action(updateTag));
            UpdateTagCommand.CanExecute = false;

            DeleteTagCommand = new Command(new Action(deleteTag));
            DeleteTagCommand.CanExecute = false;

            AddChildTagCommand = new Command(new Action(() =>
            {
                Utils.Misc.insertIntoSortedCollection<Tag>(ChildTags, NewChildTag);     
            }));
            AddChildTagCommand.CanExecute = false;

            RemoveChildTagCommand = new Command(new Action(() =>
                {
                    ChildTags.Remove(SelectedChildTag);
                }));
            RemoveChildTagCommand.CanExecute = false;

            CreateCategoryCommand = new Command(new Action(createCategory));
            CreateCategoryCommand.CanExecute = false;

            DeleteCategoryCommand = new Command(new Action(deleteCategory));
            DeleteCategoryCommand.CanExecute = false;
        
            TagName = "";
            NewCategoryName = "";
            
        }

        ObservableCollection<Tag> tags;

        public ObservableCollection<Tag> Tags
        {
            get { return tags; }
            set { tags = value;
            NotifyPropertyChanged();
            }
        }

        Tag selectedTag;

        public Tag SelectedTag
        {
            get { return selectedTag; }
            set
            {
                selectedTag = value;

                if (selectedTag != null)
                {
                    SelectTagCommand.CanExecute = true;                   
                    selectTag();
                }
                else
                {
                    SelectTagCommand.CanExecute = false;
                    UpdateTagCommand.CanExecute = false;
                }
                
                NotifyPropertyChanged();
            }
        }

        Command selectTagCommand;

        public Command SelectTagCommand
        {
            get { return selectTagCommand; }
            set { selectTagCommand = value; }
        }

        void selectTag()
        {
                   
            using (var tc = new TagDbCommands())
            {
                Tag tag = tc.getTagById(SelectedTag.Id);

                TagName = tag.Name;

                ChildTags.Clear();

                foreach (Tag childTag in tag.ChildTags)
                {
                    ChildTags.Add(childTag);
                }

                TagCategory = tag.TagCategory;
            }
          
            CreateTagCommand.CanExecute = true;
            UpdateTagCommand.CanExecute = true;
            DeleteTagCommand.CanExecute = true;
                          
        }

        String tagName;

        public String TagName
        {
            get { return tagName; }
            set
            {
                tagName = value;

                if (String.IsNullOrEmpty(tagName) || String.IsNullOrWhiteSpace(tagName))
                {                    
                   CreateTagCommand.CanExecute = false;                  
                   UpdateTagCommand.CanExecute = false;                    
                }
                else
                {                    
                   CreateTagCommand.CanExecute = true;                    
                   UpdateTagCommand.CanExecute = true;                   
                }

                NotifyPropertyChanged();
            }
        }

        ObservableCollection<Tag> childTags;

        public ObservableCollection<Tag> ChildTags
        {
            get { return childTags; }
            set
            {
                childTags = value;
                NotifyPropertyChanged();
            }
        }

        TagCategory tagCategory;

        public TagCategory TagCategory
        {
            get { return tagCategory; }
            set
            {
                tagCategory = value;
                NotifyPropertyChanged();
            }
        }

        Command createTagCommand;

        public Command CreateTagCommand
        {
            get { return createTagCommand; }
            set { createTagCommand = value; }
        }

        void createTag()
        {
            Tag tag = new Tag();
            tag.Name = TagName;
            
            foreach (Tag childTag in ChildTags)
            {
                tag.ChildTags.Add(childTag);
            }

            tag.TagCategory = TagCategory;

            using (var tc = new TagDbCommands())
            {
                try
                {                   
                    tag = tc.createTag(tag);

                    if (tag != null)
                    {
                        insertTagIntoTagsCollection(tag, false);                      
                        clearTag();
                    }
                }
                catch (Exception e)
                {
                    log.Error("Could not add tag to database: " + TagName, e);
                }
            }
        }

        Command updateTagCommand;

        public Command UpdateTagCommand
        {
            get { return updateTagCommand; }
            set { updateTagCommand = value; }
        }

        void updateTag()
        {
            if (SelectedTag == null) return;
          
            using (var tc = new TagDbCommands())
            {
                try
                {
                    Tag tag = new Tag();

                    tag.Id = SelectedTag.Id;
                    tag.Name = TagName;
                    tag.TagCategory = TagCategory;
                  
                    foreach (Tag childTag in ChildTags)
                    {                       
                        tag.ChildTags.Add(childTag);
                    }
                          
                    tag = tc.updateTag(tag);
                    Tags.Remove(SelectedTag);
                    insertTagIntoTagsCollection(tag, true);                 
                
                }
                catch (Exception e)
                {
                    log.Error("Could not update tag in database: " + TagName, e);
                }
            }

        }

        ObservableCollection<TagCategory> categories;

        public ObservableCollection<TagCategory> Categories
        {
            get { return categories; }
            set { categories = value; }
        }

        Command deleteCategoryCommand;

        public Command DeleteCategoryCommand
        {
            get { return deleteCategoryCommand; }
            set { deleteCategoryCommand = value; }
        }
        Command createCategoryCommand;

        public Command CreateCategoryCommand
        {
            get { return createCategoryCommand; }
            set { createCategoryCommand = value; }
        }

        TagCategory selectedCategory;

        public TagCategory SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                selectedCategory = value;
                if (selectedCategory != null)
                {
                    DeleteCategoryCommand.CanExecute = true;
                }
                else
                {
                    DeleteCategoryCommand.CanExecute = false;
                }
                NotifyPropertyChanged();
            }
        }

        String newCategoryName;

        public String NewCategoryName
        {
            get { return newCategoryName; }
            set
            {
                newCategoryName = value;
                if (String.IsNullOrEmpty(newCategoryName) || String.IsNullOrWhiteSpace(newCategoryName))
                {
                    CreateCategoryCommand.CanExecute = false;
                }
                else
                {
                    CreateCategoryCommand.CanExecute = true;
                }
                NotifyPropertyChanged();
            }
        }
              
        Tag newChildTag;

        public Tag NewChildTag
        {
            get { return newChildTag; }
            set
            {
                newChildTag = value;

                if (value != null)
                {
                    AddChildTagCommand.CanExecute = true;
                }
                else
                {
                    AddChildTagCommand.CanExecute = false;
                }
                NotifyPropertyChanged();
            }
        }

        Tag selectedChildTag;

        public Tag SelectedChildTag
        {
            get { return selectedChildTag; }
            set
            {
                selectedChildTag = value;

                if (selectedChildTag != null)
                {
                    RemoveChildTagCommand.CanExecute = true;
                }
                else
                {
                    RemoveChildTagCommand.CanExecute = false;
                }
                NotifyPropertyChanged();
            }
        }

        TagCategory categoryFilter;

        public TagCategory CategoryFilter
        {
            get { return categoryFilter; }
            set
            {
                categoryFilter = value;

                using (TagDbCommands tagCommands = new TagDbCommands())
                {
                    try
                    {
                        List<Tag> result;

                        if (categoryFilter != null)
                        {
                            result = tagCommands.getTagsByCategory(categoryFilter);
                        }
                        else
                        {
                            result = tagCommands.getAllTags();
                        }

                        Tags.Clear();

                        foreach (Tag tag in result)
                        {
                            Tags.Add(tag);
                        }

                    }
                    catch (Exception e)
                    {
                        log.Error("Error setting category filter", e);
                    }
                }
                NotifyPropertyChanged();
            }
                    
        }

        Command addChildTagCommand;

        public Command AddChildTagCommand
        {
            get { return addChildTagCommand; }
            set { addChildTagCommand = value; }
        }

        Command removeChildTagCommand;

        public Command RemoveChildTagCommand
        {
            get { return removeChildTagCommand; }
            set { removeChildTagCommand = value; }
        }

        void createCategory()
        {
           
            using (var tc = new TagDbCommands())
            {
                try
                {
                    TagCategory newCategory = new TagCategory() { Name = NewCategoryName };

                    newCategory = tc.createTagCategory(newCategory);

                    if (newCategory != null)
                    {
                        Utils.Misc.insertIntoSortedCollection<TagCategory>(Categories, newCategory);                  
                        NewCategoryName = "";
                    }
                }
                catch (Exception e)
                {
                    log.Error("Could not create tag category in database: " + SelectedCategory.Name, e);
                }
            }

        }

        void deleteCategory()
        {
          
            using (var tc = new TagDbCommands())
            {
                try
                {
                    tc.deleteTagCategory(SelectedCategory);
                    Categories.Remove(SelectedCategory);

                    // update cached tags to reflect changes
                    CategoryFilter = null;
                    clearTag();
                   
                }
                catch (Exception e)
                {
                    log.Error("Could not remove tag category from database: " + SelectedCategory.Name, e);
                }
            }
        }

        Command deleteTagCommand;

        public Command DeleteTagCommand
        {
            get { return deleteTagCommand; }
            set { deleteTagCommand = value; }
        }

        void deleteTag()
        {
            using (var tc = new TagDbCommands())
            {
                try
                {
                  

                    tc.deleteTag(SelectedTag);
                    Tags.Remove(SelectedTag);
                }
                catch (Exception e)
                {
                    log.Error("Could not remove SelectedTag from database: " + SelectedTag.Name, e);
                }
            }
        }

        Command clearTagCommand;

        public Command ClearTagCommand
        {
            get { return clearTagCommand; }
            set { clearTagCommand = value; }
        }

        void clearTag()
        {
            
            TagName = "";
            TagCategory = null;
            SelectedTag = null;          
            ChildTags.Clear();

            CreateTagCommand.CanExecute = false;
            UpdateTagCommand.CanExecute = false;
            DeleteTagCommand.CanExecute = false;
        }

        void insertTagIntoTagsCollection(Tag tag, bool select)
        {
            if ((categoryFilter != null && tag.TagCategory == null) ||
                (categoryFilter != null && tag.TagCategory.Id != categoryFilter.Id))                
            {
               return;                             
            }

            Utils.Misc.insertIntoSortedCollection<Tag>(Tags, tag);

            if (select == true)
            {
                SelectedTag = tag;
            }
        }
    }
}
