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

//https://github.com/JakeGinnivan/SettingsProvider.net
namespace MediaViewer.Settings
{

    class AppSettings
    {

        public AppSettings()
        {
            instance = null;
            metaDataUpdateDirectoryHistory = new ObservableCollection<string>();
            filenamePresets = new ObservableCollection<string>();
            filenameHistory = new ObservableCollection<string>();
            createDirectoryHistory = new ObservableCollection<string>();

        }

        public static void load()
        {
            var settingsProvider = new SettingsProvider(); //By default uses IsolatedStorage for storage
            instance = settingsProvider.GetSettings<AppSettings>();
            if (instance == null)
            {
                instance = new AppSettings();
            }

        }

        public static void save()
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

                return (instance);
            }

        }

        ObservableCollection<String> filenamePresets;

        public ObservableCollection<String> FilenamePresets
        {
            get { return filenamePresets; }
            set { filenamePresets = value; }
        }


        ObservableCollection<String> metaDataUpdateDirectoryHistory;

        public ObservableCollection<String> MetaDataUpdateDirectoryHistory
        {
            get { return metaDataUpdateDirectoryHistory; }
            set { metaDataUpdateDirectoryHistory = value; }
        }


        ObservableCollection<String> filenameHistory;

        public ObservableCollection<String> FilenameHistory
        {
            get { return filenameHistory; }
            set { filenameHistory = value; }
        }

        ObservableCollection<String> createDirectoryHistory;

        public ObservableCollection<String> CreateDirectoryHistory
        {
            get { return createDirectoryHistory; }
            set { createDirectoryHistory = value; }
        }

        public void clearHistory()
        {

            FilenameHistory.Clear();
            MetaDataUpdateDirectoryHistory.Clear();
            CreateDirectoryHistory.Clear();
       
        }


        String videoScreenShotLocation;

        public String VideoScreenShotLocation
        {
            get { return videoScreenShotLocation; }
            set { videoScreenShotLocation = value; }
        }
    }
}
