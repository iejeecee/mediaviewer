using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using YoutubePlugin.YoutubeChannelBrowser;

namespace YoutubePlugin.Properties {
    
    
    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
    internal sealed partial class Settings {

        public Settings()
        {
            // // To add event handlers for saving and changing settings, uncomment the lines below:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //
            /*if (IsUpgradeRequired)
            {
                Upgrade();
                IsUpgradeRequired = false;
            }*/

            SettingsLoaded += Settings_SettingsLoaded;           
        }

        void Settings_SettingsLoaded(object sender, System.Configuration.SettingsLoadedEventArgs e)
        {
            
            if (YoutubeChannels == null)
            {
                YoutubeChannels = new List<YoutubeChannelNodeState>();
            }

            if (FixedDownloadPathHistory == null)
            {
                FixedDownloadPathHistory = new ObservableCollection<string>();
            }
           
        }
        
        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
            // Add code to handle the SettingChangingEvent event here.
        }
        
        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
            // Add code to handle the SettingsSaving event here.
        }


        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public List<YoutubeChannelNodeState> YoutubeChannels
        {
            get
            {
                return (List<YoutubeChannelNodeState>)(this["YoutubeChannels"]);
            }
            set
            {
                this["YoutubeChannels"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public ObservableCollection<String> FixedDownloadPathHistory
        {
            get
            {
                return ((ObservableCollection<String>)(this["FixedDownloadPathHistory"]));
            }
            set
            {
                this["FixedDownloadPathHistory"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public MediaViewer.Infrastructure.Constants.SaveLocation VideoSaveMode
        {
            get
            {
                Object value = this["VideoSaveMode"];

                if (value == null)
                {
                    return MediaViewer.Infrastructure.Constants.SaveLocation.Ask;
                }
                else
                {
                    return (MediaViewer.Infrastructure.Constants.SaveLocation)value;
                }

            }
            set
            {
                this["VideoSaveMode"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public YoutubePlugin.Settings.Constants.VideoResolution MaxPlaybackResolution
        {
            get
            {
                Object value = this["MaxPlaybackResolution"];

                if (value == null)
                {
                    return YoutubePlugin.Settings.Constants.VideoResolution.r2160p;
                }
                else
                {
                    return (YoutubePlugin.Settings.Constants.VideoResolution)value;
                }

            }
            set
            {
                this["MaxPlaybackResolution"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public YoutubePlugin.Settings.Constants.VideoResolution MaxDownloadResolution
        {
            get
            {
                Object value = this["MaxDownloadResolution"];

                if (value == null)
                {
                    return YoutubePlugin.Settings.Constants.VideoResolution.r2160p;
                }
                else
                {
                    return (YoutubePlugin.Settings.Constants.VideoResolution)value;
                }

            }
            set
            {
                this["MaxDownloadResolution"] = value;
            }
        }
        
    }


}
