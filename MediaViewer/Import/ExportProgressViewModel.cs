using MediaViewer.Infrastructure.Logging;
using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
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
using MediaViewer.Model.Media.Base.Metadata;

namespace MediaViewer.Import
{

    class ExportProgressViewModel : CancellableOperationProgressBase
    {
             
        MediaFileState MediaFileState
        {
            get;
            set;
        }
      
        public ExportProgressViewModel(MediaFileState mediaFileState)
        {
            MediaFileState = mediaFileState;

            WindowTitle = "Exporting Media";
            WindowIcon = "pack://application:,,,/Resources/Icons/export.ico";
         
            ItemInfo = "";

            OkCommand.IsExecutable = false;
            CancelCommand.IsExecutable = true;
        }

        public async Task exportAsync(ObservableCollection<ScanLocation> includeLocations, ObservableCollection<ScanLocation> excludeLocations)
        {
            
            TotalProgress = 0;

            await Task.Factory.StartNew(() =>
            {
                export(includeLocations, excludeLocations);

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

            ScanLocation location = (state as Tuple<ScanLocation, ObservableCollection<ScanLocation>, List<String>>).Item1;
            ObservableCollection<ScanLocation> excludeLocations = (state as Tuple<ScanLocation, ObservableCollection<ScanLocation>, List<String>>).Item2;
            List<String> items = (state as Tuple<ScanLocation, ObservableCollection<ScanLocation>, List<String>>).Item3;

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

                if (!items.Contains(addItem) && !excluded)
                {
                    items.Add(addItem);
                }
            }

            return (true);
        }

        void export(ObservableCollection<ScanLocation> includeLocations, ObservableCollection<ScanLocation> excludeLocations)
        {
            List<String> items = new List<String>();

            foreach (ScanLocation location in includeLocations)
            {
                if (CancellationToken.IsCancellationRequested) return;
                ItemInfo = "Searching files in: " + location.Location;

                Tuple<ScanLocation, ObservableCollection<ScanLocation>, List<String>> state =
                    new Tuple<ScanLocation, ObservableCollection<ScanLocation>, List<String>>(location, excludeLocations, items);

                FileUtils.iterateFilesInDirectory(new DirectoryInfo(location.Location),
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

                    ItemInfo = "Exporting: " + mediaFile.Location;

                    MediaFileState.export(mediaFile, CancellationToken);

                    ItemProgress = 100;
                    TotalProgress++;
                    InfoMessages.Add("Exported: " + mediaFile.Location);
                }
                catch (Exception e)
                {
                    ItemInfo = "Error exporting file: " + item;
                    InfoMessages.Add("Error exporting file: " + item + " " + e.Message);
                    Logger.Log.Error("Error exporting file: " + item, e);
                    MessageBox.Show("Error exporting file: " + item + "\n\n" + e.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;

                }
            }
        }
      
        public async Task exportAsync(ICollection<MediaFileItem> items)
        {
            TotalProgressMax = items.Count;
            TotalProgress = 0;


            await Task.Factory.StartNew(() =>
            {
                export(items);

            }, CancellationToken);

            OkCommand.IsExecutable = true;
            CancelCommand.IsExecutable = false;

        }

        void export(ICollection<MediaFileItem> items)
        {
            foreach (MediaFileItem item in items)
            {
                try
                {
                    if (CancellationToken.IsCancellationRequested) return;
                    ItemProgress = 0;

                    if (item.Metadata == null)
                    {
                        ItemInfo = "Reading Metadata: " + item.Location;

                        item.ExitUpgradeableReadLock();
                        try
                        {
                            item.readMetadata_URLock(MetadataFactory.ReadOptions.AUTO, CancellationToken);
                            if (item.Metadata == null || item.Metadata is UnknownMetadata)
                            {
                                ItemInfo = "Could not open file and/or read it's metadata: " + item.Location;
                                InfoMessages.Add("Could not open file and/or read it's metadata: " + item.Location);
                                Logger.Log.Error("Could not open file and/or read it's metadata: " + item.Location);
                            }
                        }
                        finally
                        {
                            item.ExitUpgradeableReadLock();
                        }
                    }

                    if (item.Metadata.IsImported == false)
                    {
                        InfoMessages.Add("Skipping non-imported file: " + item.Location);
                        ItemProgress = 100;
                        TotalProgress++;
                        continue;
                    }

                    ItemInfo = "Exporting: " + item.Location;

                    MediaFileState.export(item, CancellationToken);

                    ItemProgress = 100;
                    TotalProgress++;
                    InfoMessages.Add("Exported: " + item.Location);
                }
                catch (Exception e)
                {
                    ItemInfo = "Error exporting file: " + item.Location;
                    InfoMessages.Add("Error exporting file: " + item.Location + " " + e.Message);
                    Logger.Log.Error("Error exporting file: " + item.Location, e);
                    MessageBox.Show("Error exporting file: " + item.Location + "\n\n" + e.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;

                }
            }
        }
                    
    }


}
