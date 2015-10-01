using MediaViewer.Infrastructure.Logging;
using MediaViewer.Model.Global.Events;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Settings;
using MediaViewer.Model.Utils;
using MediaViewer.Properties;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MediaViewer.MediaFileBrowser.DirectoryBrowser
{
    class MediaFileBrowserDirectoryBrowserViewModel : BindableBase
    {       
        MediaFileWatcher MediaFileWatcher { get; set; }
        IEventAggregator EventAggregator { get; set; }

        public MediaFileBrowserDirectoryBrowserViewModel(MediaFileWatcher mediaFileWatcher, IEventAggregator eventAggregator)
        {

            MediaFileWatcher = mediaFileWatcher;       
            EventAggregator = eventAggregator;
            BrowsePathHistory = Settings.Default.BrowsePathHistory;
            FavoriteLocations = Settings.Default.FavoriteLocations;

            EventAggregator.GetEvent<MediaBrowserPathChangedEvent>().Subscribe((location) =>
            {
                BrowsePath = location;
            });
                        
        }

        /*private void directoryBrowser_Renamed(System.Object sender, System.IO.RenamedEventArgs e)
        {
            BrowsePath = MediaFileWatcher.Path;
        }*/
      
        ObservableCollection<String> browsePathHistory;

        public ObservableCollection<String> BrowsePathHistory
        {
            get { return browsePathHistory; }
            set { SetProperty(ref browsePathHistory, value); }
        }

        ObservableCollection<String> favoriteLocations;

        public ObservableCollection<String> FavoriteLocations
        {
            get { return favoriteLocations; }
            set { SetProperty(ref favoriteLocations, value); }
        }

        public string BrowsePath
        {
            set
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(value);

                    string pathWithoutFileName;

                    if ((fileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        pathWithoutFileName = value;
                    }
                    else
                    {
                        pathWithoutFileName = FileUtils.getPathWithoutFileName(value);
                    }

                    if (MediaFileWatcher.Path.Equals(pathWithoutFileName))
                    {
                        return;
                    }

                    MediaFileWatcher.Path = pathWithoutFileName;

                    //Title = value;
                    EventAggregator.GetEvent<MediaBrowserPathChangedEvent>().Publish(value);

                    OnPropertyChanged("BrowsePath");
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error setting path\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Logger.Log.Error("error setting path", e);
                }
            }

            get
            {
                return (MediaFileWatcher.Path);
            }
        }
    }
}
