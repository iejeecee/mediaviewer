using SettingsProviderNet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;


//https://github.com/JakeGinnivan/SettingsProvider.net
namespace MediaViewer.Model.Settings
{
   
    public class AppSettings
    {
        static String appName = "MediaViewer";

        public AppSettings()
        {            
            MetaDataUpdateDirectoryHistory = new ObservableCollection<string>();
            FilenamePresets = new ObservableCollection<string>();
            FilenameHistory = new ObservableCollection<string>();
            CreateDirectoryHistory = new ObservableCollection<string>();
            TorrentAnnounceHistory = new ObservableCollection<string>();
            TranscodeOutputDirectoryHistory = new ObservableCollection<string>();
            VideoScreenShotLocation = null;
            VideoPreviewOutputDirectoryHistory = new ObservableCollection<string>();
            VideoScreenShotLocationHistory = new ObservableCollection<string>();
            ImageCollageOutputDirectoryHistory = new ObservableCollection<string>();

            BrowsePathHistory = new ObservableCollection<string>();
            FavoriteLocations = new ObservableCollection<string>();
        }

        public static T getSettings<T>(String name) where T : new()
        {
            String fullName = appName + "." + name;

            var settingsProvider = new SettingsProvider(new RoamingAppDataStorage(fullName));

            T result = settingsProvider.GetSettings<T>();

            return (result);
        }

        public static void saveSettings<T>(String name, T settings) 
        {
            String fullName = appName + "." + name;

            var settingsProvider = new SettingsProvider(new RoamingAppDataStorage(fullName));

            settingsProvider.SaveSettings<T>(settings);            
        }

        protected static AppSettings load()
        {
            var settingsProvider = new SettingsProvider(new RoamingAppDataStorage(appName)); 
            //By default uses IsolatedStorage for storage
            AppSettings settings = settingsProvider.GetSettings<AppSettings>();

            setDefaults(settings);

            return (settings);

        }

        public void save()
        {
            if (instance == null)
            {
                throw new System.InvalidOperationException("There is no settings instance to save");
            }

            var settingsProvider = new SettingsProvider(new RoamingAppDataStorage(appName));

            settingsProvider.SaveSettings(instance);
        }

        static AppSettings instance;

        public static AppSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = load();                                        
                }

                return (instance);
            }

        }

        static void setDefaults(AppSettings settings) 
        {
            if (settings.VideoScreenShotLocation == null)
            {
                settings.VideoScreenShotLocation = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }       

        }

        public ObservableCollection<String> FilenamePresets { get; set; }
        public ObservableCollection<String> MetaDataUpdateDirectoryHistory { get; set; }
        public ObservableCollection<String> FilenameHistory { get; set; }
        public ObservableCollection<String> CreateDirectoryHistory { get; set;}
        public ObservableCollection<String> TorrentAnnounceHistory { get; set; }
        public String VideoScreenShotLocation { get; set; }
        public ObservableCollection<String> VideoScreenShotLocationHistory { get; set; }
        public ObservableCollection<String> TranscodeOutputDirectoryHistory { get; set; }
        public ObservableCollection<String> VideoPreviewOutputDirectoryHistory { get; set; }
        public ObservableCollection<String> ImageCollageOutputDirectoryHistory { get; set; }

        // mediafilebrowser
        public ObservableCollection<String> BrowsePathHistory { get; set; }
        public ObservableCollection<String> FavoriteLocations { get; set; }

        public void clearHistory()
        {
            FilenameHistory.Clear();
            MetaDataUpdateDirectoryHistory.Clear();
            CreateDirectoryHistory.Clear();
            TorrentAnnounceHistory.Clear();
            TranscodeOutputDirectoryHistory.Clear();
            VideoScreenShotLocation = null;
            VideoScreenShotLocationHistory.Clear();
            BrowsePathHistory.Clear();
            FavoriteLocations.Clear();
            VideoPreviewOutputDirectoryHistory.Clear();
            ImageCollageOutputDirectoryHistory.Clear();
        }
            
    }
}
