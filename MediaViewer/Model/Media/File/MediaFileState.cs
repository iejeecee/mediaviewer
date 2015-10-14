using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Media.State;
using MediaViewer.Model.metadata.Metadata;
using MediaViewer.Model.Utils;
using MediaViewer.Progress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

                    bool isImported = item.delete();

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

                        bool isImported = item.move(location, progress);
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

        
        public void import(MediaFileItem item, CancellationToken token)
        {
            List<MediaFileItem> dummy = new List<MediaFileItem>();
            dummy.Add(item);
            import(dummy, token);

        }

        public void import(IEnumerable<MediaFileItem> items, CancellationToken token)
        {
            List<MediaFileItem> importedItems = new List<MediaFileItem>();

            try
            {
                foreach (MediaFileItem item in items)
                {
                    if (token.IsCancellationRequested) return;

                    //if (item.Metadata == null)
                    //{
                        item.readMetadata(MetadataFactory.ReadOptions.AUTO |
                                MetadataFactory.ReadOptions.GENERATE_MULTIPLE_THUMBNAILS, token);

                        if (item.Metadata == null || item.Metadata is UnknownMetadata)
                        {
                            throw new MediaStateException("Error importing item, cannot read item metadata: " + item.Location);
                        }
                    //}

                    bool success = item.import(token);
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
        }


        public void export(MediaFileItem item, CancellationToken token)
        {
            List<MediaFileItem> dummy = new List<MediaFileItem>();
            dummy.Add(item);
            export(dummy, token);
        }

        public void export(IEnumerable<MediaFileItem> items, CancellationToken token)
        {
            List<MediaFileItem> exportedItems = new List<MediaFileItem>();

            try
            {
                foreach (MediaFileItem item in items)
                {
                    if (token.IsCancellationRequested) return;

                    bool success = item.export(token);
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
        }
              
        void OnNrImportedItemsChanged(MediaStateChangedEventArgs args)
        {
            if (NrImportedItemsChanged != null)
            {
                NrImportedItemsChanged(this, args);
            }
        }
             
    }
}
