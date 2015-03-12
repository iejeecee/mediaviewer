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
    class ScanFilesViewModel : BindableBase, INonCancellableOperationProgress
    {
        public ScanFilesViewModel()
        {
            TotalProgressMax = 1;
            TotalProgress = 0;

            WindowTitle = "Scanning for media files";
            WindowIcon = "pack://application:,,,/Resources/Icons/torrent.ico";
        }

        string windowTitle;

        public string WindowTitle
        {
            get
            {
                return (windowTitle);
            }
            set
            {
                SetProperty(ref windowTitle, value);
            }
        }

        String windowIcon;

        public string WindowIcon
        {
            get
            {
                return(windowIcon);
            }
            set
            {
                SetProperty(ref windowIcon, value);
            }
        }

        int totalProgress;

        public int TotalProgress
        {
            get
            {
                return (totalProgress);   
            }
            set
            {
                SetProperty(ref totalProgress, value);
            }
        }

        int totalProgressMax;

        public int TotalProgressMax
        {
            get
            {
                return (totalProgressMax);   
            }
            set
            {
                SetProperty(ref totalProgressMax, value);
            }
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
