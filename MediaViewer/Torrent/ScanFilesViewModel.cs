using MediaViewer.Infrastructure.Logging;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Utils;
using MediaViewer.Progress;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace MediaViewer.Torrent
{
    class ScanFilesViewModel : NonCancellableOperationProgressBase
    {
        public ScanFilesViewModel()
        {
            TotalProgressMax = 1;
            TotalProgress = 0;

            WindowTitle = "Scanning for media files";
            WindowIcon = "pack://application:,,,/Resources/Icons/torrent.ico";
        }

        
        public ObservableCollection<MediaFileItem> getInputMedia(string inputPath)
        {
            ObservableCollection<MediaFileItem> items = new ObservableCollection<MediaFileItem>();
                     
            FileUtils.walkDirectoryTree(new DirectoryInfo(inputPath), addInputMedia, items, true);
          
            TotalProgress = 1;
                        
            return (items);
        }

        private bool addInputMedia(FileInfo info, object state)
        {
            ObservableCollection<MediaFileItem> items = (ObservableCollection<MediaFileItem>)state;

            if (MediaFormatConvert.isMediaFile(info.FullName))
            {
                items.Add(MediaFileItem.Factory.create(info.FullName));
            }

            return (true);
        }
    }
}
