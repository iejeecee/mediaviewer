using MediaViewer.MediaDatabase;
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

        ObservableCollection<Tag> tags;

        public ObservableCollection<Tag> Tags
        {
            get { return tags; }
            set { tags = value;
            NotifyPropertyChanged();
            }
        }

        ObservableCollection<Tag> linkedTags;

        public ObservableCollection<Tag> LinkedTags
        {
            get { return linkedTags; }
            set
            {
                linkedTags = value;
            NotifyPropertyChanged();
            }
        }

        public LinkedTagEditorViewModel()
        {
            NewTagName = "";
            Tags = new ObservableCollection<Tag>();
            LinkedTags = new ObservableCollection<Tag>();
            Categories = new ObservableCollection<TagCategory>();

            using (TagDbCommands tc = new TagDbCommands(null))
            {
                List<TagCategory> categories = tc.getAllCategories();

                foreach (TagCategory category in categories)
                {
                    Categories.Add(category);
                }

                List<Tag> temp = tc.getAllTags();

                foreach (Tag tag in temp)
                {
                    tags.Add(tag);
                }

            }
           
            AddTagCommand = new Command(new Action(addTag));
            RemoveTagCommand = new Command(new Action(removeTag));
            AddLinkedTagCommand = new Command(new Action(addLinkedTag));
            CreateCategoryCommand = new Command(new Action(createCategory));
            DeleteCategoryCommand = new Command(new Action(deleteCategory));
            
        }

        String newTagName;

        public String NewTagName
        {
            get { return newTagName; }
            set { newTagName = value;
            NotifyPropertyChanged();
            }
        }

        Command addTagCommand;

        public Command AddTagCommand
        {
            get { return addTagCommand; }
            set { addTagCommand = value; }
        }

        void addTag()
        {
            if (String.IsNullOrEmpty(NewTagName) || String.IsNullOrWhiteSpace(NewTagName)) return;

            using (var db = new MediaDatabaseContext())
            {
                try
                {
                    var newTag = new Tag { Name = NewTagName };
                    newTag = db.TagSet.Add(newTag);
                    db.SaveChanges();

                    Tags.Add(newTag);
                    NewTagName = "";
                }
                catch (Exception e)
                {
                    log.Error("Could not add tag to database: " + NewTagName, e);
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
            set { selectedCategory = value;
            NotifyPropertyChanged();
            }
        }

        String newCategoryName;

        public String NewCategoryName
        {
            get { return newCategoryName; }
            set { newCategoryName = value;
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
                NotifyPropertyChanged();

                if(selectedTag == null) return;

                using (TagDbCommands tc = new TagDbCommands(null))
                {
                    LinkedTags.Clear();

                    Tag item = tc.getTagByName(selectedTag.Name);
                                                                                  
                    foreach (Tag linkedTag in item.LinkedTags)
                    {
                        LinkedTags.Add(linkedTag);
                    }

                    SelectedTagCategory = item.TagCategory;         
                }
            }
        }

        TagCategory selectedTagCategory;

        public TagCategory SelectedTagCategory
        {
            get { return selectedTagCategory; }
            set { selectedTagCategory = value;
            NotifyPropertyChanged();
            updateTagCategory();
            }
        }

        Tag newLinkedTag;

        public Tag NewLinkedTag
        {
            get { return newLinkedTag; }
            set { newLinkedTag = value;
            NotifyPropertyChanged();
            }
        }

        Command addLinkedTagCommand;

        public Command AddLinkedTagCommand
        {
            get { return addLinkedTagCommand; }
            set { addLinkedTagCommand = value; }
        }

        void createCategory()
        {
            if (String.IsNullOrEmpty(NewCategoryName) || String.IsNullOrWhiteSpace(NewCategoryName)) return;

            using (var db = new MediaDatabaseContext())
            {
                try
                {
                    TagCategory newCategory = new TagCategory();
                    newCategory.Name = NewCategoryName;

                    db.TagCategorySet.Add(newCategory);                     

                    db.SaveChanges();
                    Categories.Add(newCategory);
                    NewCategoryName = "";
                }
                catch (Exception e)
                {
                    log.Error("Could not create tag category in database: " + SelectedCategory.Name, e);
                }
            }

        }

        void deleteCategory()
        {
            if (SelectedCategory == null) return;

            using (var db = new MediaDatabaseContext())
            {
                try
                {
                    db.TagCategorySet.Attach(SelectedCategory);

                    db.TagCategorySet.Remove(SelectedCategory);

                    db.SaveChanges();
                    Categories.Remove(SelectedCategory);
                }
                catch (Exception e)
                {
                    log.Error("Could not remove tag category from database: " + SelectedCategory.Name, e);
                }
            }
        }


        void addLinkedTag()
        {
            if (SelectedTag == null || NewLinkedTag == null) return;
  
            using (var db = new MediaDatabaseContext())
            {
                try
                {


                    db.TagSet.Attach(selectedTag);
                    // create a copy of newLinkedTag because db.SaveChanges() will modify
                    // newLinkedTag. 
                    Tag temp = new TagDbCommands(db).getTagByName(newLinkedTag.Name);               
                    selectedTag.LinkedTags.Add(temp);
                    db.Entry(temp).State = EntityState.Added;
                    /*db.TagSet.Attach(newLinkedTag);
                    selectedTag.LinkedTags.Add(newLinkedTag);
                    db.Entry(newLinkedTag).State = EntityState.Added;*/
                                
                    db.SaveChanges();
                   

                    LinkedTags.Add(temp);
                    NewLinkedTag = null;
                }
                catch (Exception e)
                {
                    log.Error("Could not update SelectedTag in database: " + SelectedTag.Name, e);
                }
            }
        }

        void updateTagCategory()
        {
            if (SelectedTag == null) return;
            if (SelectedTagCategory == null)
            {
                if (SelectedTag.TagCategory == null) return;
            }
            else
            {
                if (SelectedTag.TagCategory != null)
                {
                    if(SelectedTag.TagCategory.Equals(SelectedTagCategory)) return;
                }
            }

            using (var db = new MediaDatabaseContext())
            {
                try
                {
                    db.TagSet.Attach(SelectedTag);
                    db.TagCategorySet.Attach(selectedTagCategory);

                    SelectedTag.TagCategory = SelectedTagCategory;
                  
                    db.Entry(SelectedTag).State = EntityState.Modified;

                    db.SaveChanges();
                                 
                }
                catch (Exception e)
                {
                    log.Error("Could not update SelectedTag category in database: " + SelectedTag.Name, e);
                }
            }
        }

        Command removeTagCommand;

        public Command RemoveTagCommand
        {
            get { return removeTagCommand; }
            set { removeTagCommand = value; }
        }

        void removeTag()
        {
            if (SelectedTag == null) return;

            using (var db = new MediaDatabaseContext())
            {
                try
                {
                    db.TagSet.Attach(SelectedTag);

                    db.TagSet.Remove(SelectedTag);

                    db.SaveChanges();
                    Tags.Remove(SelectedTag);
                }
                catch (Exception e)
                {
                    log.Error("Could not remove SelectedTag from database: " + SelectedTag.Name, e);
                }
            }
        }
    }
}
