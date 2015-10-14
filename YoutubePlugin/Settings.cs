using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
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

        
    }


}
