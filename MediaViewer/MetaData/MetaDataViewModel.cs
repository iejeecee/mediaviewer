using MediaViewer.DirectoryPicker;
using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Progress;
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
using System.ComponentModel.Composition;
using MediaViewer.Model.Settings;
using Microsoft.Practices.Prism.Regions;
using MediaViewer.Model.Utils;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Commands;
using MediaViewer.Model.Mvvm;
using MediaViewer.Infrastructure.Logging;
using MediaViewer.Model.Media.Base.Item;
using Microsoft.Practices.ServiceLocation;
using MediaViewer.UserControls.Layout;
using MediaViewer.Model.Global.Events;
using MediaViewer.Properties;
using MediaViewer.Model.Media.File.Metadata;

namespace MediaViewer.MetaData
{

    public class MetaDataViewModel : BindableBase
    {             
        
        
        void clear()
        {
            Filename = "";
            ReplaceFilename = "";
            Location = "";
            
            Rating = 0;
            Title = "";
            Description = "";
            Author = "";
            Copyright = "";
            Creation = null;
            IsImported = false;
            Geotag = new GeoTagCoordinatePair();

            lock (itemsLock)
            {
                DynamicProperties.Clear();
            }

            SelectedMetaDataPreset = noPresetMetaData;

            lock (tagsLock)
            {
                Tags.Clear();
            }
            lock (addTagsLock)
            {
                AddTags.Clear();
            }
            lock (removeTagsLock)
            {
                RemoveTags.Clear();
            }
            
        }

        IEventAggregator EventAggregator { get; set; }

        public MetaDataViewModel(MediaFileWatcher mediaFileWatcher, IEventAggregator eventAggregator)
        {            
            //Items = new ObservableCollection<MediaFileItem>();
            itemsLock = new Object();

            DynamicProperties = new ObservableCollection<Tuple<string, string>>();
            BindingOperations.EnableCollectionSynchronization(DynamicProperties, itemsLock);

            EventAggregator = eventAggregator;

            Tags = new ObservableCollection<Tag>();
            tagsLock = new Object();
            BindingOperations.EnableCollectionSynchronization(Tags, tagsLock);      

            AddTags = new ObservableCollection<Tag>();
            addTagsLock = new Object();
            BindingOperations.EnableCollectionSynchronization(AddTags, addTagsLock);

            RemoveTags = new ObservableCollection<Tag>();
            removeTagsLock = new Object();
            BindingOperations.EnableCollectionSynchronization(RemoveTags, removeTagsLock);

            MetaDataPresets = new ObservableCollection<PresetMetadata>();

            loadMetaDataPresets();

            clear();
            BatchMode = false;
            IsEnabled = false;
            IsReadOnly = true;

            WriteMetaDataCommand = new AsyncCommand(async () =>
            {
                CancellableOperationProgressView metaDataUpdateView = new CancellableOperationProgressView();
                MetaDataUpdateViewModel vm = new MetaDataUpdateViewModel(mediaFileWatcher, EventAggregator);
                metaDataUpdateView.DataContext = vm;
                metaDataUpdateView.Show();             
                await vm.writeMetaDataAsync(new MetaDataUpdateViewModelAsyncState(this));

            });

            FilenamePresetsCommand = new Command(() =>
            {
                FilenameRegexView filenamePreset = new FilenameRegexView();
                FilenameRegexViewModel vm = (FilenameRegexViewModel)filenamePreset.DataContext;

                if (filenamePreset.ShowDialog() == true)
                {
                    if (!vm.SelectedRegex.IsEmpty)
                    {
                        Filename = vm.SelectedRegex.Regex;
                        ReplaceFilename = vm.SelectedRegex.Replace;
                    }
                }

            }); 

            DirectoryPickerCommand = new Command(() => 
            {
                DirectoryPickerView directoryPicker = new DirectoryPickerView();
                DirectoryPickerViewModel vm = (DirectoryPickerViewModel)directoryPicker.DataContext;
                vm.SelectedPath = String.IsNullOrEmpty(Location) ? mediaFileWatcher.Path : Location;
                lock (Items)
                {
                    vm.SelectedItems = new List<MediaFileItem>(Items);
                }
                vm.PathHistory = Settings.Default.MetaDataUpdateDirectoryHistory;
              
                if (directoryPicker.ShowDialog() == true)
                {                    
                    Location = vm.SelectedPath;                    
                }

            });

            MetaDataPresetsCommand = new Command(() =>
                {
                    MetaDataPresetsView metaDataPresets = new MetaDataPresetsView();
                    metaDataPresets.ShowDialog();
                    loadMetaDataPresets();                    

                });

            ClearRatingCommand = new Command(() =>
                {
                    Rating = null;
                });

           
         

            mediaFileWatcher.MediaFileState.ItemPropertyChanged += MediaState_ItemPropertiesChanged;
          
            FilenameHistory = Settings.Default.FilenameHistory;
            ReplaceFilenameHistory = Settings.Default.ReplaceFilenameHistory;

            MovePathHistory = Settings.Default.MetaDataUpdateDirectoryHistory;

            FavoriteLocations = Settings.Default.FavoriteLocations;

            IsRegexEnabled = false;
            ReplaceFilename = "";
        }

        private void MediaState_ItemPropertiesChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (Items == null || Items.Count == 0) return;

            if (e.PropertyName.Equals("Location"))
            {
                lock(itemsLock)
                {
                    if (BatchMode == false && Items.Count > 0)
                    {
                        MediaFileItem modifiedItem = sender as MediaFileItem;

                        if (Items.ElementAt(0).Equals(modifiedItem))
                        {
                            Filename = Path.GetFileNameWithoutExtension(modifiedItem.Location);
                            Location = FileUtils.getPathWithoutFileName(modifiedItem.Location);
                        }
                    }
                }

            }
            else if (e.PropertyName.Equals("Metadata"))
            {
                lock (itemsLock)
                {
                    if (BatchMode == false && Items.Count > 0)
                    {
                        MediaFileItem modifiedItem = sender as MediaFileItem;

                        if (Items.ElementAt(0).Equals(modifiedItem))
                        {
                            grabData();
                        }
                    }
                }

            }
            else if (e.PropertyName.Equals("IsReadOnly"))
            {
                lock (itemsLock)
                {
                    if (BatchMode == false && Items.Count > 0)
                    {
                        IsReadOnly = Items.ElementAt(0).IsReadOnly;
                    }
                }
            }
        }

        Object itemsLock;
        ICollection<MediaFileItem> items;

        public ICollection<MediaFileItem> Items
        {
            get { return items; }
            set
            {
                lock (itemsLock)
                {
                    items = value;

                    if (items != null)
                    {
                        grabData();
                    }
                }
              
            }
        }

        String filename;

        public string Filename
        {
            get { return filename; }
            set
            {               
                SetProperty(ref filename, value);
            }
        }

        String replaceFilename;

        public String ReplaceFilename
        {
            get { return replaceFilename; }
            set { SetProperty(ref replaceFilename, value); }
        }

        bool isRegexEnabled;

        public bool IsRegexEnabled
        {
            get { return isRegexEnabled; }
            set { SetProperty(ref isRegexEnabled, value); }
        }


        String location;

        public String Location
        {
            get { return location; }
            set
            {
                SetProperty(ref location, value);
            }
        }

        public AsyncCommand WriteMetaDataCommand { get; set; }
        public Command ClearRatingCommand { get; set; }
       

        public Command FilenamePresetsCommand { get; set; }
        public Command DirectoryPickerCommand { get; set; }
        public Command MetaDataPresetsCommand { get; set; }

        static PresetMetadata noPresetMetaData = new PresetMetadata() { Name = "None", Id = -1 };

        public ObservableCollection<PresetMetadata> MetaDataPresets { get; set; }

        PresetMetadata selectedMetaDataPreset;

        public PresetMetadata SelectedMetaDataPreset
        {
            get { return selectedMetaDataPreset; }
            set
            {
                bool dontGrabData = false;

                if (value != null && selectedMetaDataPreset != null)
                {
                    if (value.Id == -1 && selectedMetaDataPreset.Id == -1)
                    {
                        dontGrabData = true;
                    }
                }

                SetProperty(ref selectedMetaDataPreset, value);
                           
                if (selectedMetaDataPreset == null)
                {

                }
                else if (selectedMetaDataPreset.Id == -1)
                {
                    if (items != null && items.Count == 1 && dontGrabData == false)
                    {
                        grabData();
                    }
                }
                else
                {
                 
                    if (selectedMetaDataPreset.IsTitleEnabled)
                    {
                        TitleEnabled = true;
                        Title = selectedMetaDataPreset.Title;
                    }
                    if (selectedMetaDataPreset.IsRatingEnabled)
                    {
                        RatingEnabled = true;
                        Rating = (float)selectedMetaDataPreset.Rating;
                    }
                    if (selectedMetaDataPreset.IsDescriptionEnabled)
                    {
                        DescriptionEnabled = true;
                        Description = selectedMetaDataPreset.Description;
                    }
                    if (selectedMetaDataPreset.IsAuthorEnabled)
                    {
                        AuthorEnabled = true;
                        Author = selectedMetaDataPreset.Author;
                    }

                    if (selectedMetaDataPreset.IsCreationDateEnabled)
                    {
                        CreationEnabled = true;
                        Creation = selectedMetaDataPreset.CreationDate;
                    }

                    if (selectedMetaDataPreset.IsCopyrightEnabled)
                    {
                        CopyrightEnabled = true;
                        Copyright = selectedMetaDataPreset.Copyright;
                    }

                }
               
            }

        }

        void loadMetaDataPresets()
        {           
            MetaDataPresets.Clear();

            MetaDataPresets.Add(noPresetMetaData);

            using (PresetMetadataDbCommands db = new PresetMetadataDbCommands())
            {
                foreach (PresetMetadata data in db.getAllPresets())
                {
                    MetaDataPresets.Add(data);
                }

            }
            SelectedMetaDataPreset = noPresetMetaData;
        }

        

        Nullable<double> rating;

        public Nullable<double> Rating
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
            set
            {               
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
            set
            {                
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
            set
            {                
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
            set
            {                
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
            set
            {               
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

        GeoTagCoordinatePair geotag;

        public GeoTagCoordinatePair Geotag
        {
            get { return geotag; }
            set { SetProperty(ref geotag, value); }
        }

        bool isGeotagEnabled;

        public bool IsGeotagEnabled
        {
            get { return isGeotagEnabled; }
            set { SetProperty(ref isGeotagEnabled,value); }
        }

        bool isImported;

        public bool IsImported
        {
            get { return isImported; }
            set {  
                SetProperty(ref isImported, value);
            }
        }

        bool importedEnabled;

        public bool ImportedEnabled
        {
            get { return importedEnabled; }
            set { 
                SetProperty(ref importedEnabled, value);
            }
        }

        bool isReadOnly;

        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set
            {
                SetProperty(ref isReadOnly, value);
            }
        }

        Object tagsLock;
        ObservableCollection<Tag> tags;

        public ObservableCollection<Tag> Tags
        {
            get { return tags; }
            set {  
                SetProperty(ref tags, value);
            }
        }

        Object addTagsLock;
        ObservableCollection<Tag> addTags;

        public ObservableCollection<Tag> AddTags
        {
            get { return addTags; }
            set { 
                SetProperty(ref addTags, value);
            }
        }

        Object removeTagsLock;
        ObservableCollection<Tag> removeTags;

        public ObservableCollection<Tag> RemoveTags
        {
            get { return removeTags; }
            set {  
                SetProperty(ref removeTags, value);
            }
        }

        public ObservableCollection<String> FilenameHistory { get; set; }
        public ObservableCollection<String> ReplaceFilenameHistory { get; set; }
        public ObservableCollection<String> MovePathHistory { get; set; }
        public ObservableCollection<String> FavoriteLocations { get; set; }

        bool isEnabled;

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {              
                SetProperty(ref isEnabled, value);

                if (isEnabled == false)
                {                   
                    RatingEnabled = false;
                    TitleEnabled = false;
                    DescriptionEnabled = false;
                    AuthorEnabled = false;
                    CopyrightEnabled = false;
                    CreationEnabled = false;
                    ImportedEnabled = false;
                    IsGeotagEnabled = false;

                }
            }
        }


        bool batchMode;

        public bool BatchMode
        {
            get { return batchMode; }
            set
            {                
                SetProperty(ref batchMode, value);

                if (batchMode == true && IsEnabled == true)
                {                   
                    RatingEnabled = false;
                    TitleEnabled = false;
                    DescriptionEnabled = false;
                    AuthorEnabled = false;
                    CopyrightEnabled = false;
                    CreationEnabled = false;
                    ImportedEnabled = false;
                    IsGeotagEnabled = false;
                }
                else if(IsEnabled == true)
                {               
                    RatingEnabled = true;
                    TitleEnabled = true;
                    DescriptionEnabled = true;
                    AuthorEnabled = true;
                    CopyrightEnabled = true;
                    CreationEnabled = true;
                    ImportedEnabled = true;
                    IsGeotagEnabled = true;
                }

            }
        }
     
        public ObservableCollection<Tuple<String, String>> DynamicProperties { get; private set; }
               
        void grabData()
        {                       
            if (items.Count == 1 && Items.ElementAt(0).Metadata != null)
            {
                MediaFileItem media = Items.ElementAt(0);
                BaseMetadata metadata = media.Metadata;

                if (metadata.SupportsXMPMetadata == false)
                {
                    clear();
                }

                Filename = Path.GetFileNameWithoutExtension(metadata.Name);
                Location = metadata.Path;

                DynamicProperties.Clear();

                if (metadata is ImageMetadata)
                {
                    getImageProperties(DynamicProperties, metadata as ImageMetadata);
                }
                if (metadata is VideoMetadata)
                {
                    getVideoProperties(DynamicProperties, metadata as VideoMetadata);
                }
                else if (metadata is AudioMetadata)
                {
                    getAudioProperties(DynamicProperties, metadata as AudioMetadata);
                }
                
                Rating = metadata.Rating == null ? null : new Nullable<double>(metadata.Rating.Value / 5);
                Title = metadata.Title;
                Description = metadata.Description;
                Author = metadata.Author;
                Copyright = metadata.Copyright;
                Creation = metadata.CreationDate;
                IsImported = metadata.IsImported;
                IsReadOnly = media.IsReadOnly || !metadata.SupportsXMPMetadata || metadata is UnknownMetadata;
             
                Geotag = new GeoTagCoordinatePair(metadata.Latitude, metadata.Longitude);                  
                
                lock (tagsLock)
                {
                    Tags.Clear();

                    foreach (Tag tag in metadata.Tags)
                    {
                        Tags.Add(tag);
                    }
                }

                getExifProperties(DynamicProperties, metadata);

                SelectedMetaDataPreset = noPresetMetaData;
                IsEnabled = true;
                BatchMode = false;

            }
            else if (items.Count > 1 && BatchMode == true)
            {


            }
            else
            {
              
                if (items.Count > 1)
                {
                    IsEnabled = true;
                    BatchMode = true;                                 
                    IsReadOnly = false;
                    clear();

                }
                else if (items.Count == 0)
                {

                    BatchMode = false;
                    IsEnabled = false;
                    IsReadOnly = true;
                    IsRegexEnabled = false;
                    clear();
                }

            }
                                               
        }

        private void getExifProperties(ObservableCollection<Tuple<string, string>> p, BaseMetadata media)
        {
            p.Add(new Tuple<string, string>("Size", MediaViewer.Model.Utils.MiscUtils.formatSizeBytes(media.SizeBytes)));

            if (media.FileDate != null)
            {
                p.Add(new Tuple<string, string>("File Date", media.FileDate.ToString()));
            }

            if (media.LastModifiedDate != null)
            {
                p.Add(new Tuple<string, string>("File Modified", media.LastModifiedDate.ToString()));
            }

            int nrProps = p.Count;
           
            if (media.Software != null)
            {
                p.Add(new Tuple<string,string>("Software", media.Software));
            }
            if(media.MetadataDate != null) {

                p.Add(new Tuple<string, string>("Metadata Date", media.MetadataDate.ToString()));
            }
            if (media.MetadataModifiedDate != null)
            {
                p.Add(new Tuple<string, string>("Metadata Modified", media.MetadataModifiedDate.ToString()));
            }
                      
            if (media is ImageMetadata)
            {
                foreach (Tuple<string, string> item in FormatMetaData.formatProperties(media as ImageMetadata))
                {
                    p.Add(item);
                }
            }
            else if (media is AudioMetadata)
            {
                AudioMetadata audio = media as AudioMetadata;

                if (audio.Genre != null)
                {
                    p.Add(new Tuple<string, string>("Genre", audio.Genre));
                }

                if (audio.Album != null)
                {
                    p.Add(new Tuple<string, string>("Album", audio.Album));
                }

                if (audio.TrackNr != null)
                {
                    String value = audio.TrackNr.ToString();

                    if (audio.TotalTracks != null)
                    {
                        value += " / " + audio.TotalTracks;
                    }

                    p.Add(new Tuple<string, string>("Track", value));
                }

                if (audio.DiscNr != null)
                {
                    String value = audio.DiscNr.ToString();

                    if (audio.TotalDiscs != null)
                    {
                        value += " / " + audio.TotalDiscs;
                    }

                    p.Add(new Tuple<string, string>("Disc", value));
                }
                                
            }

            
            if (media.Latitude != null)
            {
                p.Add(new Tuple<string, string>("GPS Latitude", media.Latitude.Value.ToString("0.00000")));
            }

            if (media.Longitude != null)
            {
                p.Add(new Tuple<string, string>("GPS Longitude", media.Longitude.Value.ToString("0.00000")));
            }
            
            if (p.Count > nrProps)
            {
                p.Insert(nrProps, new Tuple<string, string>("", "EXIF"));
            }
            
        }

        void getImageProperties(ObservableCollection<Tuple<String, String>> p, ImageMetadata image)
        {
            p.Add(new Tuple<string, string>("", "IMAGE"));

            p.Add(new Tuple<string, string>("Image Container", image.ImageContainer));
            p.Add(new Tuple<string, string>("Resolution", image.Width + " x " + image.Height));
            p.Add(new Tuple<string, string>("Pixel Format", image.PixelFormat));
            p.Add(new Tuple<string, string>("Bits Per Pixel", image.BitsPerPixel.ToString()));           
        }

        void getVideoProperties(ObservableCollection<Tuple<String, String>> p, VideoMetadata video)
        {
           
            p.Add(new Tuple<string, string>("", "VIDEO"));            
            p.Add(new Tuple<string, string>("Video Codec", video.VideoCodec));

            foreach (Tuple<string, string> item in FormatMetaData.formatProperties(video))
            {
                p.Add(item);
            }

            p.Add(new Tuple<string, string>("Resolution", video.Width.ToString() + " x " + video.Height.ToString()));                        
            p.Add(new Tuple<string, string>("Pixel Format", video.PixelFormat));
            p.Add(new Tuple<string, string>("Bits Per Pixel", video.BitsPerPixel.ToString()));
            p.Add(new Tuple<string, string>("Frames Per Second", video.FramesPerSecond.ToString("0.##")));

            if (video.VideoBitRate.HasValue)
            {
                p.Add(new Tuple<string, string>("Video Rate", MiscUtils.formatSizeBytes(video.VideoBitRate.Value / 8) + "/s")); 
            }

            if (!String.IsNullOrEmpty(video.AudioCodec))
            {
                p.Add(new Tuple<string, string>("", "AUDIO"));
                p.Add(new Tuple<string, string>("Audio Codec", video.AudioCodec));
                p.Add(new Tuple<string, string>("Bits Per Sample", video.BitsPerSample.ToString()));
                p.Add(new Tuple<string, string>("Samples Per Second", video.SamplesPerSecond.ToString()));
                p.Add(new Tuple<string, string>("Nr Channels", video.NrChannels.ToString()));

                if (video.AudioBitRate.HasValue)
                {
                    p.Add(new Tuple<string, string>("Audio Rate", MiscUtils.formatSizeBytes(video.AudioBitRate.Value / 8) + "/s"));
                }
            }

            p.Add(new Tuple<string, string>("", "FILE"));
            p.Add(new Tuple<string, string>("Container", video.VideoContainer));
            p.Add(new Tuple<string, string>("Duration", MiscUtils.formatTimeSeconds(video.DurationSeconds)));

        }

        void getAudioProperties(ObservableCollection<Tuple<String, String>> p, AudioMetadata audio)
        {
            p.Add(new Tuple<string, string>("", "AUDIO"));
            p.Add(new Tuple<string, string>("Audio Container", audio.AudioContainer));                               
            p.Add(new Tuple<string, string>("Duration", MiscUtils.formatTimeSeconds(audio.DurationSeconds)));           
            p.Add(new Tuple<string, string>("Audio Codec", audio.AudioCodec));
            p.Add(new Tuple<string, string>("Bits Per Sample", audio.BitsPerSample.ToString()));
            p.Add(new Tuple<string, string>("Samples Per Second", audio.SamplesPerSecond.ToString()));
            p.Add(new Tuple<string, string>("Nr Channels", audio.NrChannels.ToString()));

            if (audio.BitRate.HasValue)
            {
                p.Add(new Tuple<string, string>("Rate", MiscUtils.formatSizeBytes(audio.BitRate.Value / 8) + "/s"));
            }
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {          
            EventAggregator.GetEvent<MediaSelectionEvent>().Unsubscribe(globalMediaSelectionEvent);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            EventAggregator.GetEvent<MediaSelectionEvent>().Subscribe(globalMediaSelectionEvent);
        }

        private void globalMediaSelectionEvent(MediaSelectionPayload selection)
        {
            List<MediaFileItem> items = new List<MediaFileItem>();
            foreach (MediaItem item in selection.Items)
            {
                items.Add(item as MediaFileItem);
            }

            Items = items;
        }
                        
    }
   
}
