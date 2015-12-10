using MediaViewer.Infrastructure.Logging;
using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Media.Base.State;
using MediaViewer.Model.Mvvm;
using MediaViewer.Model.Utils;
using MediaViewer.Progress;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MediaViewer.Import
{

    class ImportProgressViewModel : CancellableOperationProgressBase
    {
             
        MediaFileState MediaFileState
        {
            get;
            set;
        }
     
        public ImportProgressViewModel(MediaFileState mediaFileState) {

            MediaFileState = mediaFileState;

            WindowTitle = "Importing Media";
            WindowIcon = "pack://application:,,,/Resources/Icons/import.ico";

            ItemInfo = "";

            OkCommand.IsExecutable = false;
            CancelCommand.IsExecutable = true;
        }

        public async Task importAsync(ObservableCollection<ImportExportLocation> includeLocations, ObservableCollection<ImportExportLocation> excludeLocations)
        {
                        
            TotalProgress = 0;

            await Task.Factory.StartNew(() =>
            {
                import(includeLocations, excludeLocations);

            }, CancellationToken);

            OkCommand.IsExecutable = true;
            CancelCommand.IsExecutable = false;

        }

        private bool getMediaFiles(System.IO.FileInfo info, object state)
        {
            if (CancellationToken.IsCancellationRequested)
            {
                return (false);
            }

            ImportExportLocation location = (state as Tuple<ImportExportLocation, ObservableCollection<ImportExportLocation>, List<String>>).Item1;
            ObservableCollection<ImportExportLocation> excludeLocations = (state as Tuple<ImportExportLocation, ObservableCollection<ImportExportLocation>, List<String>>).Item2;
            List<String> items = (state as Tuple<ImportExportLocation, ObservableCollection<ImportExportLocation>, List<String>>).Item3;

            String addItem = null;

            switch (location.MediaType)
            {
                case Search.MediaType.All:
                    {
                        if (MediaViewer.Model.Utils.MediaFormatConvert.isMediaFile(info.Name))
                        {
                            addItem = info.FullName;
                        }
                        break;
                    }
                case Search.MediaType.Images:
                    {
                        if (MediaFormatConvert.isImageFile(info.Name))
                        {
                            addItem = info.FullName;
                        }
                        break;
                    }
                case Search.MediaType.Video:
                    {
                        if (MediaFormatConvert.isVideoFile(info.Name))
                        {
                            addItem = info.FullName;
                        }
                        break;
                    }
                case Search.MediaType.Audio:
                    {
                        if (MediaFormatConvert.isAudioFile(info.Name))
                        {
                            addItem = info.FullName;
                        }
                        break;
                    }
            }

            if (addItem != null)
            {
                String path = FileUtils.getPathWithoutFileName(addItem);

                bool excluded = false;

                foreach (ImportExportLocation excludeLocation in excludeLocations)
                {
                    if (excludeLocation.IsRecursive)
                    {
                        if (path.StartsWith(excludeLocation.Location))
                        {
                            if (excludeLocation.MediaType == Search.MediaType.All ||
                                (excludeLocation.MediaType == Search.MediaType.Images && MediaFormatConvert.isImageFile(addItem)) ||
                                 (excludeLocation.MediaType == Search.MediaType.Video && MediaFormatConvert.isVideoFile(addItem)))
                            {
                                excluded = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if(path.Equals(excludeLocation.Location)) 
                        {
                            if (excludeLocation.MediaType == Search.MediaType.All ||
                                (excludeLocation.MediaType == Search.MediaType.Images && MediaFormatConvert.isImageFile(addItem)) ||
                                 (excludeLocation.MediaType == Search.MediaType.Video && MediaFormatConvert.isVideoFile(addItem)))
                            {
                                excluded = true;
                                break;
                            }
                        }
                    }
                }

                if (!items.Contains(addItem) && !excluded)
                {
                    items.Add(addItem);
                }
            }

            return (true);
        }

        void import(ObservableCollection<ImportExportLocation> includeLocations, ObservableCollection<ImportExportLocation> excludeLocations)
        {
            List<String> items = new List<String>();

            foreach (ImportExportLocation location in includeLocations)
            {
                if (CancellationToken.IsCancellationRequested) return;
                ItemInfo = "Searching files in: " + location.Location;

                Tuple<ImportExportLocation, ObservableCollection<ImportExportLocation>, List<String>> state = 
                    new Tuple<ImportExportLocation, ObservableCollection<ImportExportLocation>, List<String>>(location, excludeLocations, items);

                FileUtils.walkDirectoryTree(new DirectoryInfo(location.Location),
                    getMediaFiles, state, location.IsRecursive);

                InfoMessages.Add("Completed searching files in: " + location.Location);
            }

            TotalProgressMax = items.Count;
            ItemProgressMax = 100;

            foreach (String item in items)
            {
                try
                {
                    if (CancellationToken.IsCancellationRequested) return;
                    ItemProgress = 0;
                    
                    MediaFileItem mediaFile = MediaFileItem.Factory.create(item);

                    ItemInfo = "Importing: " + mediaFile.Location;

                    MediaFileState.import(mediaFile, CancellationToken);

                    ItemProgress = 100;
                    TotalProgress++;
                    InfoMessages.Add("Imported: " + mediaFile.Location);
                }
                catch (Exception e)
                {
                    ItemInfo = "Error importing file: " + item;
                    InfoMessages.Add("Error importing file: " + item + " " + e.Message);
                    Logger.Log.Error("Error importing file: " + item, e);
                    MessageBox.Show("Error importing file: " + item + "\n\n" + e.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;

                }
            }
        }
      
        
    }
}
