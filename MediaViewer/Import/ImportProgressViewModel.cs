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
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Concurrency;

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

            TotalProgressMax = 0;
            ItemProgressMax = 1;
        }

        public async Task importAsync(ObservableCollection<ScanLocation> includeLocations, ObservableCollection<ScanLocation> excludeLocations)
        {                        
            TotalProgress = 0;

            await Task.Factory.StartNew(() =>
            {
                import(includeLocations, excludeLocations);

            }, CancellationToken, TaskCreationOptions.None, PriorityScheduler.BelowNormal);

            TotalProgressMax = TotalProgress;      

            OkCommand.IsExecutable = true;
            CancelCommand.IsExecutable = false;

        }

        private void importItem(FileInfo info)
        {
            try
            {
                
                ItemProgress = 0;

                MediaFileItem mediaFile = MediaFileItem.Factory.create(info.FullName);

                ItemInfo = "Importing: " + mediaFile.Location;

                MediaFileState.import(mediaFile, CancellationToken);

                ItemProgress = 1;
                TotalProgress++;
                InfoMessages.Add("Imported: " + mediaFile.Location);
            }
            catch (Exception e)
            {
                ItemInfo = "Error importing file: " + info.FullName;
                InfoMessages.Add("Error importing file: " + info.FullName + " " + e.Message);
                Logger.Log.Error("Error importing file: " + info.FullName, e);
                MessageBox.Show("Error importing file: " + info.FullName + "\n\n" + e.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;

            }

        }

        private bool iterateFiles(System.IO.DirectoryInfo location, object state)
        {
            if (CancellationToken.IsCancellationRequested)
            {
                return (false);
            }

            Tuple<ScanLocation, ObservableCollection<ScanLocation>> locationArgs = state as Tuple<ScanLocation, ObservableCollection<ScanLocation>>;

            ScanLocation scanLocation = locationArgs.Item1;
            ObservableCollection<ScanLocation> excludeLocations = locationArgs.Item2;
                    
            FileInfo[] files = null;
                    
            try
            {
                files = location.GetFiles("*.*");
            }       
            catch (UnauthorizedAccessException e)
            {           
                Logger.Log.Warn(e.Message);
            }
            catch (DirectoryNotFoundException e)
            {
                Logger.Log.Warn(e.Message);
            }

            List<BaseMetadata> staleItems = new List<BaseMetadata>();

            using(MetadataDbCommands metadataCommands = new MetadataDbCommands()) 
            {
                staleItems = metadataCommands.getAllMetadataInDirectory(location.FullName);            
            }

            foreach (FileInfo info in files)
            {
                if (CancellationToken.IsCancellationRequested) return false;

                if (MediaViewer.Model.Utils.MediaFormatConvert.isMediaFile(info.Name))
                {
                    staleItems.RemoveAll(x => x.Name.Equals(info.Name));
                }

                String addItem = null;

                switch (scanLocation.MediaType)
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

                    foreach (ScanLocation excludeLocation in excludeLocations)
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
                            if (path.Equals(excludeLocation.Location))
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

                    if (!excluded)
                    {
                        importItem(info);
                    }
                }
            }

            if(staleItems.Count > 0) {

                using (MetadataDbCommands metadataCommands = new MetadataDbCommands())
                {
                    foreach (BaseMetadata staleItem in staleItems)
                    {
                        metadataCommands.delete(staleItem);
                    }
                }

                Logger.Log.Info("Removed " + staleItems.Count + " stale media items from " + location.FullName);
                InfoMessages.Add("Removed " + staleItems.Count + " stale media items from " + location.FullName);
            }

            return (true);
        }

        void import(ObservableCollection<ScanLocation> includeLocations, ObservableCollection<ScanLocation> excludeLocations)
        {
            List<String> items = new List<String>();

            foreach (ScanLocation location in includeLocations)
            {
                if (CancellationToken.IsCancellationRequested) return;
                ItemInfo = "Importing files in: " + location.Location;

                Tuple<ScanLocation, ObservableCollection<ScanLocation>> locationArgs = new Tuple<ScanLocation, ObservableCollection<ScanLocation>>(location, excludeLocations);

                FileUtils.iterateDirectories(new DirectoryInfo(location.Location), iterateFiles, locationArgs);
                
            }            
           
        }
      
        
    }
}
