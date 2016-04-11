using MediaViewer.MetaData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Properties
{
    partial class Settings
    {
        public Settings()
        {
            // // To add event handlers for saving and changing settings, uncomment the lines below:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //
            this.SettingsLoaded += Settings_SettingsLoaded;

            /*if (IsUpgradeRequired)
            {
                Upgrade();
                IsUpgradeRequired = false;
            }*/
        }

        void Settings_SettingsLoaded(object sender, System.Configuration.SettingsLoadedEventArgs e)
        {
            if (MetaDataUpdateDirectoryHistory == null)
            {
                MetaDataUpdateDirectoryHistory = new ObservableCollection<string>();
            }

            if (FilenameRegexList == null)
            {
                FilenameRegexList = new ObservableCollection<FilenameRegex>();
            }

            if (FilenameHistory == null)
            {
                FilenameHistory = new ObservableCollection<string>();
            }

            if (ReplaceFilenameHistory == null)
            {
                ReplaceFilenameHistory = new ObservableCollection<string>();
            }

            if (CreateDirectoryHistory == null)
            {
                CreateDirectoryHistory = new ObservableCollection<string>();
            }

            if (TorrentAnnounceHistory == null)
            {
                TorrentAnnounceHistory = new ObservableCollection<string>();
            }

            if (TranscodeOutputDirectoryHistory == null)
            {
                TranscodeOutputDirectoryHistory = new ObservableCollection<string>();
            }

            if (ImageTranscodeOutputDirectoryHistory == null)
            {
                ImageTranscodeOutputDirectoryHistory = new ObservableCollection<string>();
            }

            if (VideoPreviewOutputDirectoryHistory == null)
            {
                VideoPreviewOutputDirectoryHistory = new ObservableCollection<string>();
            }

            if (VideoScreenShotLocationHistory == null)
            {
                VideoScreenShotLocationHistory = new ObservableCollection<string>();
            }

            if (ImageCollageOutputDirectoryHistory == null)
            {
                ImageCollageOutputDirectoryHistory = new ObservableCollection<string>();
            }

            if (BrowsePathHistory == null)
            {
                BrowsePathHistory = new ObservableCollection<string>();
            }

            if (FavoriteLocations == null)
            {
                FavoriteLocations = new ObservableCollection<string>();
            }

            if (VideoLocationHistory == null)
            {
                VideoLocationHistory = new ObservableCollection<string>();
            }

            if (AudioLocationHistory == null)
            {
                AudioLocationHistory = new ObservableCollection<string>();
            }
           
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]       
        public ObservableCollection<String> MetaDataUpdateDirectoryHistory
        {
            get
            {
                return ((ObservableCollection<String>)(this["MetaDataUpdateDirectoryHistory"]));
            }
            set
            {
                this["MetaDataUpdateDirectoryHistory"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public ObservableCollection<FilenameRegex> FilenameRegexList
        {
            get
            {
                return ((ObservableCollection<FilenameRegex>)(this["FilenameRegexList"]));
            }
            set
            {
                this["FilenameRegexList"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public ObservableCollection<String> FilenameHistory
        {
            get
            {
                return ((ObservableCollection<String>)(this["FilenameHistory"]));
            }
            set
            {
                this["FilenameHistory"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public ObservableCollection<String> ReplaceFilenameHistory
        {
            get
            {
                return ((ObservableCollection<String>)(this["ReplaceFilenameHistory"]));
            }
            set
            {
                this["ReplaceFilenameHistory"] = value;
            }
        }
      
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public ObservableCollection<String> CreateDirectoryHistory
        {
            get
            {
                return ((ObservableCollection<String>)(this["CreateDirectoryHistory"]));
            }
            set
            {
                this["CreateDirectoryHistory"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public ObservableCollection<String> TorrentAnnounceHistory
        {
            get
            {
                return ((ObservableCollection<String>)(this["TorrentAnnounceHistory"]));
            }
            set
            {
                this["TorrentAnnounceHistory"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public ObservableCollection<String> TranscodeOutputDirectoryHistory
        {
            get
            {
                return ((ObservableCollection<String>)(this["TranscodeOutputDirectoryHistory"]));
            }
            set
            {
                this["TranscodeOutputDirectoryHistory"] = value;
            }
        }
           
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public ObservableCollection<String> ImageTranscodeOutputDirectoryHistory
        {
            get
            {
                return ((ObservableCollection<String>)(this["ImageTranscodeOutputDirectoryHistory"]));
            }
            set
            {
                this["ImageTranscodeOutputDirectoryHistory"] = value;
            }
        }
           
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public ObservableCollection<String> VideoPreviewOutputDirectoryHistory
        {
            get
            {
                return ((ObservableCollection<String>)(this["VideoPreviewOutputDirectoryHistory"]));
            }
            set
            {
                this["VideoPreviewOutputDirectoryHistory"] = value;
            }
        }    
          
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public ObservableCollection<String> VideoScreenShotLocationHistory
        {
            get
            {
                return ((ObservableCollection<String>)(this["VideoScreenShotLocationHistory"]));
            }
            set
            {
                this["VideoScreenShotLocationHistory"] = value;
            }
        }   
 
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public ObservableCollection<String> ImageCollageOutputDirectoryHistory
        {
            get
            {
                return ((ObservableCollection<String>)(this["ImageCollageOutputDirectoryHistory"]));
            }
            set
            {
                this["ImageCollageOutputDirectoryHistory"] = value;
            }
        }    
         
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public ObservableCollection<String> BrowsePathHistory
        {
            get
            {
                return ((ObservableCollection<String>)(this["BrowsePathHistory"]));
            }
            set
            {
                this["BrowsePathHistory"] = value;
            }
        }    
           
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public ObservableCollection<String> FavoriteLocations
        {
            get
            {
                return ((ObservableCollection<String>)(this["FavoriteLocations"]));
            }
            set
            {
                this["FavoriteLocations"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public ObservableCollection<String> VideoLocationHistory
        {
            get
            {
                return ((ObservableCollection<String>)(this["VideoLocationHistory"]));
            }
            set
            {
                this["VideoLocationHistory"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public ObservableCollection<String> AudioLocationHistory
        {
            get
            {
                return ((ObservableCollection<String>)(this["AudioLocationHistory"]));
            }
            set
            {
                this["AudioLocationHistory"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public MediaViewer.Infrastructure.Constants.SaveLocation VideoScreenShotSaveMode
        {
            get
            {
                Object value = this["VideoScreenShotSaveMode"];

                if (value == null)
                {
                    return Infrastructure.Constants.SaveLocation.Ask;
                }
                else
                {
                    return (MediaViewer.Infrastructure.Constants.SaveLocation)value;
                }
            
            }
            set
            {
                this["VideoScreenShotSaveMode"] = value;
            }
        }

    }
}
