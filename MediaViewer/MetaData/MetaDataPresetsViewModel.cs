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
            Title = "";
            Description = "";
            Author = "";
            Copyright = "";
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
                    Title = selectedPreset.Title;
                    Description = selectedPreset.Description;
                    Author = selectedPreset.Author;
                    Copyright = selectedPreset.Copyright;

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
            preset.Title = Title;
            preset.Author = Author;
            preset.Description = Description;
            preset.Copyright = Copyright;

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
            SelectedPreset.Name = Name;
            SelectedPreset.Rating = Rating;
            SelectedPreset.Title = Title;
            SelectedPreset.Author = Author;
            SelectedPreset.Description = Description;
            SelectedPreset.Copyright = Copyright;

            using (PresetMetadataDbCommands metaDataCommands = new PresetMetadataDbCommands())
            {

                try
                {
                    PresetMetadata result = metaDataCommands.updatePresetMetadata(SelectedPreset);
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
