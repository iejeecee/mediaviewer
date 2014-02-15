using MediaViewer.DirectoryPicker;
using MediaViewer.ImageGrid;
using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.MediaFileModel;
using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.Utils;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaViewer.MetaData
{
    class MetaDataPresetsViewModel : CloseableObservableObject
    {

        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public MetaDataPresetsViewModel()
        {
            Tags = new ObservableCollection<string>();
                        
            using (PresetMetadataDbCommands presetMetadataCommands = new PresetMetadataDbCommands())
            {
                MetadataPresets = new ObservableCollection<PresetMetadata>(presetMetadataCommands.getAllPresets());                                
            }

            createPresetCommand = new Command(new Action(createPreset));          
            createPresetCommand.CanExecute = false;

            updatePresetCommand = new Command(new Action(updatePreset));        
            updatePresetCommand.CanExecute = false;

            deletePresetCommand = new Command(new Action(deletePreset));
            deletePresetCommand.CanExecute = false;
            
            clearPresetCommand = new Command(new Action(() =>
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
                selectedPreset = value;

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

                    CreatePresetCommand.CanExecute = false;
                    UpdatePresetCommand.CanExecute = true;
                    DeletePresetCommand.CanExecute = true;
                }
                else
                {
                  
                    UpdatePresetCommand.CanExecute = false;
                    DeletePresetCommand.CanExecute = false;
                }

                NotifyPropertyChanged();
            }
        }

        String name;

        public String Name
        {
            get { return name; }
            set { name = value;

            if (String.IsNullOrEmpty(name) || String.IsNullOrWhiteSpace(name))
            {
                if (SelectedPreset == null)
                {
                    CreatePresetCommand.CanExecute = false;
                }
                else
                {
                    UpdatePresetCommand.CanExecute = false;
                }
            }
            else
            {
                if (SelectedPreset == null)
                {
                    CreatePresetCommand.CanExecute = true;
                }
                else
                {
                    UpdatePresetCommand.CanExecute = true;
                }
            }

            NotifyPropertyChanged();
            }
        }
           
        float rating;

        public float Rating
        {
            get { return rating; }
            set
            {
                rating = value;
                NotifyPropertyChanged();
            }
        }

        bool ratingEnabled;

        public bool RatingEnabled
        {
            get { return ratingEnabled; }
            set { ratingEnabled = value;
            NotifyPropertyChanged();
            }
        }

        string title;

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                NotifyPropertyChanged();
            }
        }

        bool titleEnabled;

        public bool TitleEnabled
        {
            get { return titleEnabled; }
            set { titleEnabled = value;
            NotifyPropertyChanged();
            }
        }

        string description;

        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                NotifyPropertyChanged();
            }
        }

        bool descriptionEnabled;

        public bool DescriptionEnabled
        {
            get { return descriptionEnabled; }
            set { descriptionEnabled = value;
            NotifyPropertyChanged();
            }
        }

        string author;

        public string Author
        {
            get { return author; }
            set
            {
                author = value;
                NotifyPropertyChanged();
            }
        }

        bool authorEnabled;

        public bool AuthorEnabled
        {
            get { return authorEnabled; }
            set { authorEnabled = value;
            NotifyPropertyChanged();
            }
        }

        string copyright;

        public string Copyright
        {
            get { return copyright; }

            set
            {
                copyright = value;
                NotifyPropertyChanged();
            }
        }

        bool copyrightEnabled;

        public bool CopyrightEnabled
        {
            get { return copyrightEnabled; }
            set { copyrightEnabled = value;
            NotifyPropertyChanged();
            }
        }

        Nullable<DateTime> creation;

        public Nullable<DateTime> Creation
        {
            get { return creation; }
            set { creation = value;
            NotifyPropertyChanged();
            }
        }

        bool creationEnabled;

        public bool CreationEnabled
        {
            get { return creationEnabled; }
            set { creationEnabled = value;
            NotifyPropertyChanged();
            }
        }

        ObservableCollection<String> tags;

        public ObservableCollection<String> Tags
        {
            get { return tags; }
            set
            {
                tags = value;
                NotifyPropertyChanged();
            }
        }

        Command createPresetCommand;

        public Command CreatePresetCommand
        {
            get { return createPresetCommand; }
            set { createPresetCommand = value; }
        }
        Command updatePresetCommand;

        public Command UpdatePresetCommand
        {
            get { return updatePresetCommand; }
            set { updatePresetCommand = value; }
        }
        Command deletePresetCommand;

        public Command DeletePresetCommand
        {
            get { return deletePresetCommand; }
            set { deletePresetCommand = value; }
        }

        Command clearPresetCommand;

        public Command ClearPresetCommand
        {
            get { return clearPresetCommand; }
            set { clearPresetCommand = value; }
        }

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

            using (PresetMetadataDbCommands metaDataCommands = new PresetMetadataDbCommands())
            {

                try
                {
                    PresetMetadata result = metaDataCommands.createPresetMetadata(preset);

                    Utils.Misc.insertIntoSortedCollection<PresetMetadata>(MetadataPresets, result);
                }
                catch (Exception e)
                {
                    log.Error("Error creating presetMetadata", e);
                }
            }
        }

        void deletePreset()
        {
            using (PresetMetadataDbCommands metaDataCommands = new PresetMetadataDbCommands())
            {

                try
                {
                     metaDataCommands.deletePresetMetadata(SelectedPreset);
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

            using (PresetMetadataDbCommands metaDataCommands = new PresetMetadataDbCommands())
            {

                try
                {
                    PresetMetadata result = metaDataCommands.updatePresetMetadata(preset);
                    MetadataPresets.Remove(SelectedPreset);
                    Utils.Misc.insertIntoSortedCollection<PresetMetadata>(MetadataPresets, result);
                   
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
