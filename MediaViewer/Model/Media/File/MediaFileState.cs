using MediaViewer.Infrastructure.Logging;
using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.Base.Item;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Media.Base.State;
using MediaViewer.Model.Utils;
using MediaViewer.Progress;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaViewer.Model.Media.Base.Metadata;

namespace MediaViewer.Model.Media.File
{
    public class MediaFileState : MediaState
    {
        public event EventHandler<MediaStateChangedEventArgs> NrImportedItemsChanged;   

        public void delete(IEnumerable<MediaFileItem> removeItems, CancellationToken token)
        {
            List<String> deletedImportedLocations = new List<String>();
            List<MediaFileItem> deletedItems = new List<MediaFileItem>();

            try
            {
                foreach (MediaFileItem item in removeItems)
                {

                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    bool isImported = false;

                    item.EnterWriteLock();

                    try
                    {
                        isImported = item.delete_WLock();
                    }
                    finally
                    {
                        item.ExitWriteLock();
                    }

                    if (isImported)
                    {
                        deletedImportedLocations.Add(item.Location);
                    }

                    deletedItems.Add(item);
                }

            }
            finally
            {
                if (MediaFileWatcher.Instance.IsWatcherEnabled == false)
                {
                    // if the watcher is not enabled remove the items from the state ourselves
                    removeUIState(deletedItems);
                }

                if (deletedImportedLocations.Count > 0)
                {
                    OnNrImportedItemsChanged(new MediaStateChangedEventArgs(
                        MediaStateChangedAction.Remove, deletedImportedLocations));
                }

            }
        }

        public void move(MediaFileItem item, String location, CancellableOperationProgressBase progress)
        {
            List<MediaFileItem> dummy = new List<MediaFileItem>();
            dummy.Add(item);

            List<String> locationDummy = new List<string>();
            locationDummy.Add(location);

            move(dummy, locationDummy, progress);
        }

        public void move(IEnumerable<MediaFileItem> items, IEnumerable<String> locations,
            CancellableOperationProgressBase progress)
        {

            List<String> deletedImportedLocations = new List<String>();
            List<MediaFileItem> addedImportedItems = new List<MediaFileItem>();

            try
            {
                var itemsEnum = items.GetEnumerator();
                var locationsEnum = locations.GetEnumerator();

                while (itemsEnum.MoveNext() && locationsEnum.MoveNext())
                {
                    if (progress.CancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    MediaFileItem item = itemsEnum.Current;
                    String location = locationsEnum.Current;

                    if (!item.Location.Equals(location))
                    {
                        String oldLocation = item.Location;

                        bool isImported = false;

                        item.EnterUpgradeableReadLock();
                        try
                        {
                            isImported = item.move_URLock(location, progress);
                        }
                        finally
                        {
                            item.ExitUpgradeableReadLock();
                        }

                        if (MediaFileWatcher.Instance.IsWatcherEnabled &&
                            !FileUtils.getPathWithoutFileName(location).Equals(MediaFileWatcher.Instance.Path))
                        {
                            List<MediaFileItem> removeList = new List<MediaFileItem>();
                            removeList.Add(item);

                            removeUIState(removeList);
                        }

                        if (isImported)
                        {
                            deletedImportedLocations.Add(oldLocation);
                            addedImportedItems.Add(item);
                        }
                    }

                }

            }
            finally
            {
                if (deletedImportedLocations.Count > 0)
                {
                    OnNrImportedItemsChanged(new MediaStateChangedEventArgs(
                        MediaStateChangedAction.Replace, addedImportedItems, deletedImportedLocations));
                }

            }

        }

        // returns true on success
        public bool import(MediaFileItem item, CancellationToken token)
        {
            List<MediaFileItem> dummy = new List<MediaFileItem>();
            dummy.Add(item);
            int nrImported = import(dummy, token);

            return nrImported == 0 ? false : true;
        }

        // returns nr items successfully imported
        public int import(IEnumerable<MediaFileItem> items, CancellationToken token)
        {
            List<MediaFileItem> importedItems = new List<MediaFileItem>();

            try
            {
                foreach (MediaFileItem item in items)
                {
                    if (token.IsCancellationRequested) return(importedItems.Count);

                    bool success = false;
               
                    item.EnterUpgradeableReadLock();
                    try
                    {
                        if (item.Metadata != null && item.Metadata.IsImported == true)
                        {
                            // skip if item is already imported
                            continue;
                        }

                        item.readMetadata_URLock(MetadataFactory.ReadOptions.AUTO |
                                MetadataFactory.ReadOptions.GENERATE_THUMBNAIL, token);

                        if (item.Metadata == null || item.Metadata is UnknownMetadata)
                        {
                            throw new MediaStateException("Error importing item, cannot read item metadata: " + item.Location);
                        }
                    
                        item.EnterWriteLock();
                        try
                        {
                            success = item.import_WLock(token);
                        }
                        finally
                        {
                            item.ExitWriteLock(false);
                        }
                    }
                    finally
                    {
                        item.ExitUpgradeableReadLock();
                    }
                   
                    if (success)
                    {
                        importedItems.Add(item);
                    }

                }

            }
            finally
            {
                if (importedItems.Count > 0)
                {
                    OnNrImportedItemsChanged(new MediaStateChangedEventArgs(
                        MediaStateChangedAction.Add, importedItems));
                }
            }

            return (importedItems.Count);
        }

        // returns true on success
        public bool export(MediaFileItem item, CancellationToken token)
        {
            List<MediaFileItem> dummy = new List<MediaFileItem>();
            dummy.Add(item);
            int nrExported = export(dummy, token);

            return nrExported == 0 ? false : true;
        }

        // returns number of items exported
        public int export(IEnumerable<MediaFileItem> items, CancellationToken token)
        {
            List<MediaFileItem> exportedItems = new List<MediaFileItem>();

            try
            {
                foreach (MediaFileItem item in items)
                {
                    if (token.IsCancellationRequested) return(exportedItems.Count);

                    bool success = false;

                    item.EnterWriteLock();
                    try
                    {
                        success = item.export_WLock(token);
                    }
                    finally
                    {
                        item.ExitWriteLock();
                    }

                    if (success)
                    {
                        exportedItems.Add(item);
                    }

                }

            }
            finally
            {
                if (exportedItems.Count > 0)
                {
                    OnNrImportedItemsChanged(new MediaStateChangedEventArgs(
                        MediaStateChangedAction.Remove, exportedItems));
                }
            }

            return (exportedItems.Count);
        }
              
        void OnNrImportedItemsChanged(MediaStateChangedEventArgs args)
        {
            if (NrImportedItemsChanged != null)
            {
                NrImportedItemsChanged(this, args);
            }
        }

        public override void changedUIState(IEnumerable<MediaItem> changedItems)
        {
            foreach (MediaFileItem item in changedItems)
            {
                FileInfo info = new FileInfo(item.Location);
                info.Refresh();

                if (info.Exists)
                {
                    UIMediaCollection.EnterWriteLock();
                    try
                    {
                        UIMediaCollection.Reload(item);
                    }
                    finally
                    {
                        UIMediaCollection.ExitWriteLock();
                    }
                }
            }

        }
             
    }
}
