using MediaViewer.DirectoryPicker;
using MediaViewer.MediaGrid;
using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Collections.Sort;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Utils;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using MediaViewer.Model.Mvvm;
using Microsoft.Practices.Prism.Commands;

namespace MediaViewer.MetaData
{
    class MetaDataPresetsViewModel : CloseableBindableBase
    {

        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public MetaDataPresetsViewModel()
        {
            Tags = new ObservableCollection<string>();
                        
            using (PresetMetadataDbCommands presetMetadataCommands = new PresetMetadataDbCommands())
            {
                MetadataPresets = new ObservableCollection<PresetMetadata>(presetMetadataCommands.getAllPresets());                                
            }

            CreatePresetCommand = new Command(new Action(createPreset));          
            CreatePresetCommand.IsExecutable = false;

            UpdatePresetCommand = new Command(new Action(updatePreset));        
            UpdatePresetCommand.IsExecutable = false;

            DeletePresetCommand = new Command(new Action(deletePreset));
            DeletePresetCommand.IsExecutable = false;
            
            ClearPresetCommand = new Command(new Action(() =>
                {
                    clear();
                }));

            clear();
        }

        void clear()
        {
            SelectedPreset = null;
            Name = "";
            Rating = 0;
            RatingEnabled = false;
            Title = "";
            TitleEnabled = false;
            Description = "";
            DescriptionEnabled = false;
            Author = "";
            AuthorEnabled = false;
            Copyright = "";
            CopyrightEnabled = false;
            Creation = null;
            CreationEnabled = false;
            Tags.Clear();
        }

        ObservableCollection<PresetMetadata> metadataPresets;

        public ObservableCollection<PresetMetadata> MetadataPresets
        {
            get { return metadataPresets; }
            set { metadataPresets = value; }
        }

        PresetMetadata selectedPreset;

        public PresetMetadata SelectedPreset
        {
            get { return selectedPreset; }
            set
            {
                SetProperty(ref selectedPreset, value);
               
                if (selectedPreset != null)
                {
                    Rating = (float)selectedPreset.Rating;
                    RatingEnabled = selectedPreset.IsRatingEnabled;
                    Title = selectedPreset.Title;
                    TitleEnabled = selectedPreset.IsTitleEnabled;
                    Description = selectedPreset.Description;
                    DescriptionEnabled = selectedPreset.IsDescriptionEnabled;
                    Author = selectedPreset.Author;
                    AuthorEnabled = selectedPreset.IsAuthorEnabled;
                    Copyright = selectedPreset.Copyright;
                    CopyrightEnabled = selectedPreset.IsCopyrightEnabled;
                    Creation = selectedPreset.CreationDate;
                    CreationEnabled = selectedPreset.IsCreationDateEnabled;

                    CreatePresetCommand.IsExecutable = false;
                    UpdatePresetCommand.IsExecutable = true;
                    DeletePresetCommand.IsExecutable = true;
                }
                else
                {
                  
                    UpdatePresetCommand.IsExecutable = false;
                    DeletePresetCommand.IsExecutable = false;
                }

               
            }
        }

        String name;

        public String Name
        {
            get { return name; }
            set
            {
                SetProperty(ref name, value);

                if (String.IsNullOrEmpty(name) || String.IsNullOrWhiteSpace(name))
                {
                    if (SelectedPreset == null)
                    {
                        CreatePresetCommand.IsExecutable = false;
                    }
                    else
                    {
                        UpdatePresetCommand.IsExecutable = false;
                    }
                }
                else
                {
                    if (SelectedPreset == null)
                    {
                        CreatePresetCommand.IsExecutable = true;
                    }
                    else
                    {
                        UpdatePresetCommand.IsExecutable = true;
                    }
                }

            }
        }
           
        float rating;

        public float Rating
        {
            get { return rating; }
            set
            {              
                SetProperty(ref rating, value);
            }
        }

        bool ratingEnabled;

        public bool RatingEnabled
        {
            get { return ratingEnabled; }
            set { 
                SetProperty(ref ratingEnabled, value);
            }
        }

        string title;

        public string Title
        {
            get { return title; }
            set
            {             
                SetProperty(ref title, value);
            }
        }

        bool titleEnabled;

        public bool TitleEnabled
        {
            get { return titleEnabled; }
            set {  
                SetProperty(ref titleEnabled, value);
            }
        }

        string description;

        public string Description
        {
            get { return description; }
            set
            {                 
                SetProperty(ref description, value);
            }
        }

        bool descriptionEnabled;

        public bool DescriptionEnabled
        {
            get { return descriptionEnabled; }
            set { 
                SetProperty(ref descriptionEnabled, value);
            }
        }

        string author;

        public string Author
        {
            get { return author; }
            set
            {              
                SetProperty(ref author, value);
            }
        }

        bool authorEnabled;

        public bool AuthorEnabled
        {
            get { return authorEnabled; }
            set {  
                SetProperty(ref authorEnabled, value);
            }
        }

        string copyright;

        public string Copyright
        {
            get { return copyright; }

            set
            {               
                SetProperty(ref copyright, value);
            }
        }

        bool copyrightEnabled;

        public bool CopyrightEnabled
        {
            get { return copyrightEnabled; }
            set { 
                SetProperty(ref copyrightEnabled, value);
            }
        }

        Nullable<DateTime> creation;

        public Nullable<DateTime> Creation
        {
            get { return creation; }
            set {  
                SetProperty(ref creation, value);
            }
        }

        bool creationEnabled;

        public bool CreationEnabled
        {
            get { return creationEnabled; }
            set {  
                SetProperty(ref creationEnabled, value);
            }
        }

        ObservableCollection<String> tags;

        public ObservableCollection<String> Tags
        {
            get { return tags; }
            set
            {               
                SetProperty(ref tags, value);
            }
        }
       
        public Command CreatePresetCommand {get;set;}
        public Command UpdatePresetCommand {get;set;} 
        public Command DeletePresetCommand {get;set;} 
        public Command ClearPresetCommand {get;set;}
     
        void createPreset()
        {
            PresetMetadata preset = new PresetMetadata();
            preset.Name = Name;
            preset.Rating = Rating;
            preset.IsRatingEnabled = RatingEnabled;
            preset.Title = Title;
            preset.IsTitleEnabled = TitleEnabled;
            preset.Author = Author;
            preset.IsAuthorEnabled = AuthorEnabled;
            preset.Description = Description;
            preset.IsDescriptionEnabled = DescriptionEnabled;
            preset.Copyright = Copyright;
            preset.IsCopyrightEnabled = CopyrightEnabled;
            preset.CreationDate = Creation;
            preset.IsCreationDateEnabled = CreationEnabled;

            using (PresetMetadataDbCommands presetMetaDataCommands = new PresetMetadataDbCommands())
            {
                try
                {
                    PresetMetadata result = presetMetaDataCommands.create(preset);

                    CollectionsSort.insertIntoSortedCollection<PresetMetadata>(MetadataPresets, result);
                }
                catch (Exception e)
                {
                    log.Error("Error creating presetMetadata", e);
                }
            }
        }

        void deletePreset()
        {
            using (PresetMetadataDbCommands presetMetaDataCommands = new PresetMetadataDbCommands())
            {

                try
                {
                     presetMetaDataCommands.delete(SelectedPreset);
                     MetadataPresets.Remove(SelectedPreset);
                     clear();
                   
                }
                catch (Exception e)
                {
                    log.Error("Error deleting presetMetadata", e);
                }
            }
        }

        void updatePreset()
        {
            PresetMetadata preset = new PresetMetadata();
            preset.Id = selectedPreset.Id;
            preset.Name = Name;
            preset.Rating = Rating;
            preset.IsRatingEnabled = RatingEnabled;
            preset.Title = Title;
            preset.IsTitleEnabled = TitleEnabled;
            preset.Author = Author;
            preset.IsAuthorEnabled = AuthorEnabled;
            preset.Description = Description;
            preset.IsDescriptionEnabled = DescriptionEnabled;
            preset.Copyright = Copyright;
            preset.IsCopyrightEnabled = CopyrightEnabled;
            preset.CreationDate = Creation;
            preset.IsCreationDateEnabled = CreationEnabled;

            using (PresetMetadataDbCommands presetMetaDataCommands = new PresetMetadataDbCommands())
            {

                try
                {
                    PresetMetadata result = presetMetaDataCommands.update(preset);
                    MetadataPresets.Remove(SelectedPreset);
                    CollectionsSort.insertIntoSortedCollection<PresetMetadata>(MetadataPresets, result);
                   
                    clear();

                }
                catch (Exception e)
                {
                    log.Error("Error updating presetMetadata", e);
                }
            }
        }
    }
}
