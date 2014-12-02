using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.ComponentModel.Composition;

//https://github.com/JakeGinnivan/SettingsProvider.net
namespace MediaViewer.Settings
{
   
    public class AppSettings
    {
        public AppSettings()
        {            
            MetaDataUpdateDirectoryHistory = new ObservableCollection<string>();
            FilenamePresets = new ObservableCollection<string>();
            FilenameHistory = new ObservableCollection<string>();
            CreateDirectoryHistory = new ObservableCollection<string>();
            TorrentAnnounceHistory = new ObservableCollection<string>();
            TranscodeOutputDirectoryHistory = new ObservableCollection<string>();
        }

        protected static void load()
        {
            var settingsProvider = new SettingsProvider(); //By default uses IsolatedStorage for storage
            instance = settingsProvider.GetSettings<AppSettings>();
            if (instance == null)
            {
                instance = new AppSettings();
            }

        }

        public void save()
        {
            if (instance == null)
            {
                throw new System.InvalidOperationException("There is no settings instance to save");
            }

            var settingsProvider = new SettingsProvider();

            settingsProvider.SaveSettings(instance);
        }

        static AppSettings instance;

        public static AppSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    load();
                }

                return (instance);
            }

        }

        public ObservableCollection<String> FilenamePresets { get; set; }
        public ObservableCollection<String> MetaDataUpdateDirectoryHistory { get; set; }
        public ObservableCollection<String> FilenameHistory { get; set; }
        public ObservableCollection<String> CreateDirectoryHistory { get; set;}
        public ObservableCollection<String> TorrentAnnounceHistory { get; set; }
        public String VideoScreenShotLocation { get; set; }
        public ObservableCollection<string> TranscodeOutputDirectoryHistory { get; set; }

        public void clearHistory()
        {
            FilenameHistory.Clear();
            MetaDataUpdateDirectoryHistory.Clear();
            CreateDirectoryHistory.Clear();
            TorrentAnnounceHistory.Clear();
            TranscodeOutputDirectoryHistory.Clear();
        }
            
    }
}
