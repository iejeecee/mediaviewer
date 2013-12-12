using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

//https://github.com/JakeGinnivan/SettingsProvider.net
namespace MediaViewer.Settings
{
  
    class AppSettings
    {
        public AppSettings()
        {
            instance = null;
            metaDataUpdateDirectoryHistory = new List<string>();
            filenamePresets = new List<string>();
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
            get {

                return (instance);
            }
          
        }

        List<String> filenamePresets;

        public List<String> FilenamePresets
        {
            get { return filenamePresets; }
            set { filenamePresets = value; }
        }
      
        List<String> metaDataUpdateDirectoryHistory;

        public List<String> MetaDataUpdateDirectoryHistory
        {
            get { return metaDataUpdateDirectoryHistory; }
            set { metaDataUpdateDirectoryHistory = value; }
        }
    }
}
