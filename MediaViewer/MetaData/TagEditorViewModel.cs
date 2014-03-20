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
   
    class TagEditorViewModel : ObservableObject 
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public static TagCategory NullCategory = new TagCategory() { Name = "None", Id = -1 };

        public TagEditorViewModel()
        {          
            ChildTags = new ObservableCollection<Tag>();

            ClearTagCommand = new Command(new Action(clearTag));        

            CreateTagCommand = new Command(new Action(createTag));
            CreateTagCommand.CanExecute = false;

            UpdateTagCommand = new Command(new Action(updateTag));
            UpdateTagCommand.CanExecute = false;

            DeleteTagCommand = new Command(new Action(deleteTag));
            DeleteTagCommand.CanExecute = false;

            ClearCategoryCommand = new Command(new Action(() =>
            {
                SelectedCategory = null;
            }));

            CreateCategoryCommand = new Command(new Action(createCategory));
            CreateCategoryCommand.CanExecute = false;

            UpdateCategoryCommand = new Command(new Action(updateCategory));

            DeleteCategoryCommand = new Command(new Action(deleteCategory));
            DeleteCategoryCommand.CanExecute = false;
        
            TagName = "";
            CategoryName = "";
           
            using (var tagCategoryCommands = new TagCategoryDbCommands())
            {                
                 List<TagCategory> tempList = tagCategoryCommands.getAllCategories();

                 categories = new ObservableCollection<TagCategory>(tempList);                
            }

        }

        ObservableCollection<TagCategory> categories;

        public ObservableCollection<TagCategory> Categories
        {
            get { return categories; }
            set { categories = value; }
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
                                   
                    selectTag();
                }
                else
                {
                    ClearTagCommand.DoExecute();             
                    UpdateTagCommand.CanExecute = false;
                }
                
                NotifyPropertyChanged();
            }
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

            using (var tagCommands = new TagDbCommands())
            {
                try
                {                   
                    tag = tagCommands.create(tag);

                    if (tag != null)
                    {                                        
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
          
            using (var tagCommands = new TagDbCommands())
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
                          
                    tag = tagCommands.update(tag);                                
                
                }
                catch (Exception e)
                {
                    log.Error("Could not update tag in database: " + TagName, e);
                }
            }

        }

        Command updateCategoryCommand;

        public Command UpdateCategoryCommand
        {
            get { return updateCategoryCommand; }
            set { updateCategoryCommand = value; }
        }

        private void updateCategory()
        {
            using (var tagCategoryCommands = new TagCategoryDbCommands())
            {
                try
                {
                    SelectedCategory = tagCategoryCommands.update(SelectedCategory);

                }
                catch (Exception e)
                {
                    log.Error("Could not create tag category in database: " + SelectedCategory.Name, e);
                }
            }
        }

        Command deleteCategoryCommand;

        public Command DeleteCategoryCommand
        {
            get { return deleteCategoryCommand; }
            set { deleteCategoryCommand = value; }
        }
       
        void deleteCategory()
        {

            using (var tagCategoryCommands = new TagCategoryDbCommands())
            {
                try
                {
                    tagCategoryCommands.delete(SelectedCategory);

                    // update cached tags to reflect changes                
                    clearTag();

                }
                catch (Exception e)
                {
                    log.Error("Could not remove tag category from database: " + SelectedCategory.Name, e);
                }
            }
        }

        Command createCategoryCommand;

        public Command CreateCategoryCommand
        {
            get { return createCategoryCommand; }
            set { createCategoryCommand = value; }
        }

        void createCategory()
        {

            using (var tagCategoryCommands = new TagCategoryDbCommands())
            {
                try
                {
                    TagCategory newCategory = new TagCategory() { Name = CategoryName };

                    newCategory = tagCategoryCommands.create(newCategory);

                }
                catch (Exception e)
                {
                    log.Error("Could not create tag category in database: " + SelectedCategory.Name, e);
                }
            }

        }

        Command clearCategoryCommand;

        public Command ClearCategoryCommand
        {
            get { return clearCategoryCommand; }
            set { clearCategoryCommand = value; }
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
                    CategoryName = selectedCategory.Name;
                    DeleteCategoryCommand.CanExecute = true;
                }
                else
                {
                    CategoryName = "";
                    DeleteCategoryCommand.CanExecute = false;
                }
                NotifyPropertyChanged();
            }
        }

        String categoryName;

        public String CategoryName
        {
            get { return categoryName; }
            set
            {
                categoryName = value;
                if (String.IsNullOrEmpty(categoryName) || String.IsNullOrWhiteSpace(categoryName))
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
              
        Command deleteTagCommand;

        public Command DeleteTagCommand
        {
            get { return deleteTagCommand; }
            set { deleteTagCommand = value; }
        }

        void deleteTag()
        {
            using (var tagCommands = new TagDbCommands())
            {
                try
                {                  
                    tagCommands.delete(SelectedTag);         
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
            if(SelectedTag != null) SelectedTag = null;          
            ChildTags.Clear();

            CreateTagCommand.CanExecute = false;
            UpdateTagCommand.CanExecute = false;
            DeleteTagCommand.CanExecute = false;
        }



       
    }
}
