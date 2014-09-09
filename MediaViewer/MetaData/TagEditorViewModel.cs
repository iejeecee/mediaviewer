using AutoMapper;
using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DataTransferObjects;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Collections;
using MediaViewer.Progress;
using Microsoft.Win32;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

//Add/Attach and Entity States
//http://msdn.microsoft.com/en-us/data/jj592676.aspx

namespace MediaViewer.MetaData
{
   
    class TagEditorViewModel : CloseableObservableObject 
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public static TagCategory NullCategory = new TagCategory() { Name = "None", Id = -1 };

        public TagEditorViewModel()
        {          
            ChildTags = new ObservableRangeCollection<Tag>();
            AddChildTags = new ObservableRangeCollection<Tag>();
            RemoveChildTags = new ObservableRangeCollection<Tag>();

            ClearTagCommand = new Command(() => SelectedTags.Clear());        

            CreateTagCommand = new Command(new Action(createTag));
            CreateTagCommand.CanExecute = false;

            UpdateTagCommand = new Command(new Action(updateTag));
            UpdateTagCommand.CanExecute = false;

            DeleteTagCommand = new Command(new Action(deleteTag));
            DeleteTagCommand.CanExecute = false;

            ClearCategoryCommand = new Command(() => SelectedCategories.Clear());

            CreateCategoryCommand = new Command(new Action(createCategory));
            CreateCategoryCommand.CanExecute = false;

            UpdateCategoryCommand = new Command(new Action(updateCategory));

            DeleteCategoryCommand = new Command(new Action(deleteCategory));
            DeleteCategoryCommand.CanExecute = false;

            ImportCommand = new Command(new Action(import));
            ExportCommand = new Command(new Action(export));
        
            TagName = "";
            CategoryName = "";

            Categories = new ObservableRangeCollection<TagCategory>();

            loadCategories();

            CloseCommand = new Command(() => this.OnClosingRequest());

            SelectedTags = new ObservableRangeCollection<Tag>();
            SelectedTags.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(selectedTags_Changed);

            SelectedCategories = new ObservableRangeCollection<TagCategory>();
            SelectedCategories.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(selectedCategories_Changed);

            IsTagSelected = false;
            IsCategorySelected = false;
            IsTagBatchMode = false;
            IsTagCategoryEnabled = true;
        }

        private void export()
        {
            SaveFileDialog saveTagsDialog = MediaViewer.Model.Utils.Windows.FileDialog.createSaveTagsFileDialog();
            if (saveTagsDialog.ShowDialog() == false) return;

            XmlTextWriter outFile = null;
            try
            {
                outFile = new XmlTextWriter(saveTagsDialog.FileName, Encoding.UTF8);
                outFile.Formatting = Formatting.Indented;            
                Type[] knownTypes = new Type[] {typeof(TagDTO),typeof(TagCategoryDTO)};

                DataContractSerializer tagSerializer = new DataContractSerializer(typeof(List<TagDTO>), knownTypes);

                using (var tagCommands = new TagDbCommands())                
                {
                    tagCommands.Db.Configuration.ProxyCreationEnabled = false;                   
                    List<Tag> tags = tagCommands.getAllTags(true);
                    List<TagDTO> tagsDTO = new List<TagDTO>();
                   
                    foreach (Tag tag in tags)
                    {
                        var tagDTO = Mapper.Map<Tag, TagDTO>(tag, new TagDTO());

                        tagsDTO.Add(tagDTO);                                                              
                    }

                    tagSerializer.WriteObject(outFile, tagsDTO);   
                }
               
            }
            catch (Exception e)
            {
                MessageBox.Show("Error exporting tags:\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (outFile != null)
                {
                    outFile.Dispose();
                }
            }
          
        }

 

        private void import()
        {
            OpenFileDialog loadTagsDialog = MediaViewer.Model.Utils.Windows.FileDialog.createLoadTagsFileDialog();
            if (loadTagsDialog.ShowDialog() == false) return;

            CancellableOperationProgressView importView = new CancellableOperationProgressView();
            TagEditorImportViewModel vm = new TagEditorImportViewModel();
            importView.DataContext = vm;
            Task.Run(() => vm.import(loadTagsDialog.FileName));
            importView.ShowDialog();
            
        }

        private void selectedCategories_Changed(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (selectedCategories.Count == 0)
            {
                CategoryName = "";

                CreateCategoryCommand.CanExecute = false;
                UpdateCategoryCommand.CanExecute = false;
                DeleteCategoryCommand.CanExecute = false;

                IsCategorySelected = false;
                IsCategoryBatchMode = false;
               
            }
            else if (selectedCategories.Count == 1)
            {
                if (selectedCategories[0] == null) return;

                CategoryName = selectedCategories[0].Name;
                CreateCategoryCommand.CanExecute = false;
                UpdateCategoryCommand.CanExecute = true;
                DeleteCategoryCommand.CanExecute = true;

                IsCategorySelected = true;
                IsCategoryBatchMode = false;

            }
            else
            {
                if (IsCategoryBatchMode == false)
                {
                    CategoryName = "";
                    CreateCategoryCommand.CanExecute = false;
                    UpdateCategoryCommand.CanExecute = false;
                    DeleteCategoryCommand.CanExecute = true;
                 
                    IsCategoryBatchMode = true;
                }
            }
           
        }

        private void selectedTags_Changed(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {            
            if (selectedTags.Count == 0)
            {
                TagName = "";
                TagCategory = null;           
                ChildTags.Clear();

                CreateTagCommand.CanExecute = false;
                UpdateTagCommand.CanExecute = false;
                DeleteTagCommand.CanExecute = false;

                IsTagSelected = false;
                IsTagBatchMode = false;
                IsTagCategoryEnabled = true;
                
            }
            else if(selectedTags.Count == 1)
            {
                selectTag(selectedTags[0]);

                CreateTagCommand.CanExecute = false;
                UpdateTagCommand.CanExecute = true;
                DeleteTagCommand.CanExecute = true;

                IsTagSelected = true;
                IsTagBatchMode = false;
                IsTagCategoryEnabled = true;
            }
            else
            {
                if (IsTagBatchMode == false)
                {
                    TagName = "";
                    this.TagCategory = null;                    
                    IsTagCategoryEnabled = false;
                    AddChildTags.Clear();
                    RemoveChildTags.Clear();

                    IsTagBatchMode = true;
                }
            }      
        }

        void loadCategories()
        {
            using (var tagCategoryCommands = new TagCategoryDbCommands())
            {
                List<TagCategory> tempList = tagCategoryCommands.getAllCategories();

                Categories.ReplaceRange(tempList);
            }
        }

        ObservableRangeCollection<TagCategory> categories;

        public ObservableRangeCollection<TagCategory> Categories
        {
            get { return categories; }
            set { categories = value;
            NotifyPropertyChanged();
            }
        }
      
        ObservableRangeCollection<Tag> selectedTags;

        public ObservableRangeCollection<Tag> SelectedTags
        {
            get { return selectedTags; }
            set
            {
                selectedTags = value;
                NotifyPropertyChanged();
            }
        }

        void selectTag(Tag tag)
        {                   
            using (var tc = new TagDbCommands())
            {
                Tag currentTag = tc.getTagById(tag.Id);

                TagName = currentTag.Name;

                ChildTags.Clear();

                foreach (Tag childTag in currentTag.ChildTags)
                {
                    ChildTags.Add(childTag);
                }

                TagCategory = currentTag.TagCategory;
            }                                         
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
                }
                else
                {                    
                   CreateTagCommand.CanExecute = true;                                                     
                }

                NotifyPropertyChanged();
            }
        }

        ObservableRangeCollection<Tag> childTags;

        public ObservableRangeCollection<Tag> ChildTags
        {
            get { return childTags; }
            set
            {
                childTags = value;
                NotifyPropertyChanged();
            }
        }

        ObservableRangeCollection<Tag> addChildTags;

        public ObservableRangeCollection<Tag> AddChildTags
        {
            get { return addChildTags; }
            set
            {
                addChildTags = value;
                NotifyPropertyChanged();
            }
        }

        ObservableRangeCollection<Tag> removeChildTags;

        public ObservableRangeCollection<Tag> RemoveChildTags
        {
            get { return removeChildTags; }
            set
            {
                removeChildTags = value;
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
                    GlobalMessenger.Instance.NotifyColleagues("tag_Created", tag);

                    SelectedTags.Replace(tag);
                  
                }
                catch (Exception e)
                {
                    log.Error("Could not add tag to database: " + TagName, e);
                    MessageBox.Show("Error creating tag: " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (SelectedTags.Count == 0) return;
          
            using (var tagCommands = new TagDbCommands())
            {
                try
                {
                    TagEditorState state = new TagEditorState();                   
                    state.SelectedTags.AddRange(SelectedTags);
                    state.ChildTags.AddRange(ChildTags);
                    state.AddChildTags.AddRange(AddChildTags);
                    state.RemoveChildTags.AddRange(RemoveChildTags);
                    state.TagCategory = TagCategory;
                    state.IsTagBatchMode = IsTagBatchMode;
                    state.IsTagCategoryEnabled = IsTagCategoryEnabled;

                    foreach(Tag selectedTag in state.SelectedTags)
                    {
                        Tag updateTag = new Tag();

                        updateTag.Name = selectedTag.Name;
                        updateTag.Id = selectedTag.Id;
                        updateTag.Used = selectedTag.Used;

                        if (state.IsTagBatchMode == false)
                        {
                            updateTag.TagCategory = state.TagCategory;

                            foreach (Tag childTag in state.ChildTags)
                            {
                                updateTag.ChildTags.Add(childTag);
                            }
                        }
                        else
                        {
                            if (state.IsTagCategoryEnabled)
                            {
                                updateTag.TagCategory = state.TagCategory;
                            }
                            else
                            {
                                updateTag.TagCategory = selectedTag.TagCategory;
                            }

                            Tag currentTag = tagCommands.getTagById(selectedTag.Id);

                            foreach (Tag tag in currentTag.ChildTags)
                            {
                                updateTag.ChildTags.Add(tag);
                            }

                            foreach (Tag tag in state.AddChildTags)
                            {
                                if (!updateTag.ChildTags.Contains(tag, EqualityComparer<Tag>.Default))
                                {
                                    updateTag.ChildTags.Add(tag);
                                }
                            }

                            foreach (Tag tag in state.RemoveChildTags)
                            {
                                Tag removeTag = updateTag.ChildTags.FirstOrDefault((t) => t.Name.Equals(tag.Name));

                                if (removeTag != null)
                                {
                                    updateTag.ChildTags.Remove(removeTag);                                 
                                }

                            }
                        }

                        updateTag = tagCommands.update(updateTag);

                        GlobalMessenger.Instance.NotifyColleagues("tag_Updated", updateTag);
                    }
                }
                catch (Exception e)
                {
                    log.Error("Could not update tag in database: " + TagName, e);
                    MessageBox.Show("Could not update tag in database: " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    for (int i = SelectedCategories.Count - 1; i >= 0; i--)
                    {
                        SelectedCategories[i].Name = CategoryName;

                        TagCategory updatedCategory = tagCategoryCommands.update(SelectedCategories[i]);

                        GlobalMessenger.Instance.NotifyColleagues("tagCategory_Updated", updatedCategory);
                    }

                    loadCategories();
                }
                catch (Exception e)
                {
                    log.Error("Could not update tag category in database", e);
                    MessageBox.Show("Could not update tag category in database: " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

            String info;

            if(SelectedCategories.Count == 1) {

                info = "Are you sure you want to remove category: \"" + SelectedCategories[0].Name + "\"?";

            } else {

                info = "Are you sure you want to remove " + SelectedCategories.Count.ToString() + " categories?";
            }

            if (MessageBox.Show(info, "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
            {
                return;
            }

            using (var tagCategoryCommands = new TagCategoryDbCommands())
            {
                try
                {
                    for (int i = SelectedCategories.Count - 1; i >= 0; i--)
                    {
                        tagCategoryCommands.delete(SelectedCategories[i]);

                        GlobalMessenger.Instance.NotifyColleagues("tagCategory_Deleted", SelectedCategories[i]);            
                    }

                    loadCategories();
                }
                catch (Exception e)
                {
                    log.Error("Could not remove tag category from database", e);
                    MessageBox.Show("Could not remove tag category from database: " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                    GlobalMessenger.Instance.NotifyColleagues("tagCategory_Created", newCategory);

                    loadCategories();
                }
                catch (Exception e)
                {
                    log.Error("Could not create tag category in database", e);
                    MessageBox.Show("Could not create tag category in database: " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }

        Command clearCategoryCommand;

        public Command ClearCategoryCommand
        {
            get { return clearCategoryCommand; }
            set { clearCategoryCommand = value; }
        }

        ObservableRangeCollection<TagCategory> selectedCategories;

        public ObservableRangeCollection<TagCategory> SelectedCategories
        {
            get { return selectedCategories; }
            set
            {
                selectedCategories = value;
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
                String info;

                if (SelectedTags.Count == 1)
                {

                    info = "Are you sure you want to delete tag: \"" + SelectedTags[0].Name + "\"?";

                }
                else
                {

                    info = "Are you sure you want to delete " + SelectedTags.Count.ToString() + " tags?";
                }

                if (MessageBox.Show(info, "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                {
                    return;
                }

                try
                {
                    for (int i = SelectedTags.Count - 1; i >= 0; i--)
                    {
                        tagCommands.delete(SelectedTags[i]);
                        GlobalMessenger.Instance.NotifyColleagues("tag_Deleted", SelectedTags[i]);
                    }

                    SelectedTags.Clear();
                }
                catch (Exception e)
                {
                    log.Error("Could not remove SelectedTags from database", e);
                    MessageBox.Show("Could not remove tag from database: " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        Command clearTagCommand;

        public Command ClearTagCommand
        {
            get { return clearTagCommand; }
            set { clearTagCommand = value; }
        }

        Command closeCommand;

        public Command CloseCommand
        {
            get { return closeCommand; }
            set { closeCommand = value; }
        }

        bool isTagSelected;

        public bool IsTagSelected
        {
            get { return isTagSelected; }
            set { isTagSelected = value;
            NotifyPropertyChanged();
            }
        }
        bool isCategorySelected;

        public bool IsCategorySelected
        {
            get { return isCategorySelected; }
            set { isCategorySelected = value;
            NotifyPropertyChanged();
            }
        }

        bool isTagBatchMode;

        public bool IsTagBatchMode
        {
            get { return isTagBatchMode; }
            set { isTagBatchMode = value;
            NotifyPropertyChanged();
            }
        }

        bool isCategoryBatchMode;

        public bool IsCategoryBatchMode
        {
            get { return isCategoryBatchMode; }
            set { isCategoryBatchMode = value;
            NotifyPropertyChanged();
            }
        }


        bool isTagCategoryEnabled;

        public bool IsTagCategoryEnabled
        {
            get { return isTagCategoryEnabled; }
            set { isTagCategoryEnabled = value;
            NotifyPropertyChanged();
            }
        }

        Command importCommand;

        public Command ImportCommand
        {
            get { return importCommand; }
            set { importCommand = value; }
        }
        Command exportCommand;

        public Command ExportCommand
        {
            get { return exportCommand; }
            set { exportCommand = value; }
        }
       
    }

    class TagEditorState
    {
        public TagEditorState()
        {
            SelectedTags = new ObservableRangeCollection<Tag>();
            ChildTags = new ObservableRangeCollection<Tag>();
            AddChildTags = new ObservableRangeCollection<Tag>();
            RemoveChildTags = new ObservableRangeCollection<Tag>();
            TagCategory = null;
            IsTagBatchMode = false;
            isTagCategoryEnabled = false;
        }

        ObservableRangeCollection<Tag> selectedTags;

        public ObservableRangeCollection<Tag> SelectedTags
        {
            get { return selectedTags; }
            set { selectedTags = value; }
        }
        ObservableRangeCollection<Tag> childTags;

        public ObservableRangeCollection<Tag> ChildTags
        {
            get { return childTags; }
            set { childTags = value; }
        }
        ObservableRangeCollection<Tag> addChildTags;

        public ObservableRangeCollection<Tag> AddChildTags
        {
            get { return addChildTags; }
            set { addChildTags = value; }
        }
        ObservableRangeCollection<Tag> removeChildTags;

        public ObservableRangeCollection<Tag> RemoveChildTags
        {
            get { return removeChildTags; }
            set { removeChildTags = value; }
        }
        TagCategory tagCategory;

        public TagCategory TagCategory
        {
            get { return tagCategory; }
            set { tagCategory = value; }
        }
        bool isTagBatchMode;

        public bool IsTagBatchMode
        {
            get { return isTagBatchMode; }
            set { isTagBatchMode = value; }
        }
        bool isTagCategoryEnabled;

        public bool IsTagCategoryEnabled
        {
            get { return isTagCategoryEnabled; }
            set { isTagCategoryEnabled = value; }
        }
    }
}
