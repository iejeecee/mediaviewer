using MediaViewer.MediaDatabase;
using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.Progress;
using MediaViewer.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.MediaFileModel
{
    public class MediaState : IMediaState
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public event NotifyCollectionChangedEventHandler NrItemsInStateChanged;
        public event NotifyCollectionChangedEventHandler NrImportedItemsChanged;
        public event EventHandler ItemIsSelectedChanged;
        public event EventHandler ItemPropertiesChanged;

        MediaLockedCollection uiMediaCollection;

        /// <summary>
        /// All the media (potentially) visible to the user in the user interface
        /// </summary>
        public MediaLockedCollection UIMediaCollection
        {
            get { return uiMediaCollection; }
            set { uiMediaCollection = value; }
        }

        MediaLockedCollection busyMediaCollection;

        /// <summary>
        /// All media that is scheduled for a operation (e.g. importing, updating etc)
        /// </summary>
        public MediaLockedCollection BusyMediaCollection
        {
            get { return busyMediaCollection; }
            set { busyMediaCollection = value; }
        }

        bool debugOutput;

        public bool DebugOutput
        {
            get { return debugOutput; }
            set { debugOutput = value; }
        }

        public MediaState()
        {

            uiMediaCollection = new MediaLockedCollection();
            busyMediaCollection = new MediaLockedCollection();

            debugOutput = false;
        }

        public bool isInUse(MediaFileItem item)
        {
            return (busyMediaCollection.Contains(item));
        }

        public void addUIState(IEnumerable<MediaFileItem> items)
        {

            if (items.Count() == 0) return;

            bool itemIsSelectedChanged = false;

            UIMediaCollection.EnterWriteLock();

            if (DebugOutput) log.Info("begin add event: " + items.ElementAt(0).Location);

            try
            {

                bool success = UIMediaCollection.AddRange(items);

                if (success == false)
                {
                    throw new MediaStateException("Cannot add duplicate items to state");
                }

                foreach (MediaFileItem item in items)
                {
                    item.PropertyChanged += new PropertyChangedEventHandler(item_PropertyChanged);
                    if (item.IsSelected == true)
                    {
                        itemIsSelectedChanged = true;
                    }

                }

                fireEvents(NotifyCollectionChangedAction.Add, items.ToList(), itemIsSelectedChanged);
            }
            finally
            {

                if (DebugOutput) log.Info("end add event: " + items.ElementAt(0).Location);

                UIMediaCollection.ExitWriteLock();
            }
        }

        public void renameUIState(IEnumerable<MediaFileItem> oldItems, IEnumerable<MediaFileItem> newItems)
        {

            if (oldItems.Count() == 0 || newItems.Count() == 0) return;

            UIMediaCollection.EnterWriteLock();
            if (DebugOutput) log.Info("begin rename event " + oldItems.ElementAt(0).Location + " " + newItems.ElementAt(0).Location);

            try
            {

                bool success = UIMediaCollection.RenameRange(oldItems, newItems);

                if (success == false)
                {
                    throw new MediaStateException("Cannot rename non-existing items");
                }

                // redraw the UI since sorting order might have changed
                fireEvents(NotifyCollectionChangedAction.Reset, null, false);
            }
            finally
            {
                if (DebugOutput) log.Info("end rename event " + oldItems.ElementAt(0).Location + " " + newItems.ElementAt(0).Location);
                UIMediaCollection.ExitWriteLock();
            }
        }

        public void removeUIState(IEnumerable<MediaFileItem> removeItems)
        {
            if (removeItems.Count() == 0) return;

            bool itemIsSelectedChanged = false;

            UIMediaCollection.EnterWriteLock();
            if (DebugOutput) log.Info("begin remove event: " + removeItems.ElementAt(0).Location);

            try
            {
                List<MediaFileItem> removed = UIMediaCollection.RemoveAll(removeItems);

                foreach (MediaFileItem item in removed)
                {
                    item.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(item_PropertyChanged);

                    if (item.IsSelected == true)
                    {
                        itemIsSelectedChanged = true;
                    }

                }

                fireEvents(NotifyCollectionChangedAction.Remove, removed, itemIsSelectedChanged);

            }
            finally
            {
                if (DebugOutput) log.Info("end remove event: " + removeItems.ElementAt(0).Location);
                UIMediaCollection.ExitWriteLock();
            }

        }

        public void clearUIState()
        {

            bool itemIsSelectedChanged = false;

            UIMediaCollection.EnterWriteLock();

            try
            {
                foreach (MediaFileItem item in UIMediaCollection.Items)
                {
                    item.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(item_PropertyChanged);

                    if (item.IsSelected == true)
                    {
                        itemIsSelectedChanged = true;
                    }

                }

                UIMediaCollection.Clear();

                fireEvents(NotifyCollectionChangedAction.Reset, null, itemIsSelectedChanged);
            }
            finally
            {
                UIMediaCollection.ExitWriteLock();
            }
        }

        public void delete(IEnumerable<MediaFileItem> removeItems, CancellationToken token)
        {
            List<MediaFileItem> deletedImportedItems = new List<MediaFileItem>();
            List<MediaFileItem> deletedItems = new List<MediaFileItem>();
           
            try
            {
                foreach (MediaFileItem item in removeItems)
                {
                    bool success = busyMediaCollection.Add(item);

                    if (success == false)
                    {
                        throw new MediaStateException("Cannot delete " + item.Location + ", item already in use");
                    }

                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    bool isImported = item.delete();

                    if (isImported)
                    {
                        deletedImportedItems.Add(item);
                    }

                    busyMediaCollection.Remove(item);
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

                if (deletedImportedItems.Count > 0)
                {
                    OnNrImportedItemsChanged(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove, deletedImportedItems));
                }

                busyMediaCollection.RemoveAll(removeItems);
            }
        }

        public void move(MediaFileItem item, String location, IProgress progress)
        {
            List<MediaFileItem> dummy = new List<MediaFileItem>();
            dummy.Add(item);

            List<String> locationDummy = new List<string>();
            locationDummy.Add(location);

            move(dummy, locationDummy, progress);
        }

        public void move(IEnumerable<MediaFileItem> items, IEnumerable<String> locations,
            IProgress progress)
        {
           
            List<MediaFileItem> deletedImportedItems = new List<MediaFileItem>();
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

                    bool success = busyMediaCollection.Add(item);
                    if (success == false)
                    {
                        throw new MediaStateException("Cannot move " + item.Location + ", item already in use");
                    }

                    if (!item.Location.Equals(location))
                    {
                        bool isImported = item.move(location, progress);

                        if (isImported)
                        {
                            deletedImportedItems.Add(item);
                            addedImportedItems.Add(MediaFileItem.Factory.create(location));
                        }
                    }

                    busyMediaCollection.Remove(item);
                }

            }
            finally
            {
                if (deletedImportedItems.Count > 0)
                {
                    OnNrImportedItemsChanged(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace, addedImportedItems, deletedImportedItems, 0));
                }

                busyMediaCollection.RemoveAll(items);
            }

        }

        void fireEvents(NotifyCollectionChangedAction action, List<MediaFileItem> files, bool itemIsSelectedChanged)
        {
            NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(action, files);

            OnNrItemsInStateChanged(args);

            if (itemIsSelectedChanged)
            {
                OnItemIsSelectedChanged();
            }

        }

        public void selectAllUIState()
        {
            UIMediaCollection.EnterReaderLock();
            try
            {
                bool isSelectionChanged = false;

                foreach (MediaFileItem item in UIMediaCollection.Items)
                {
                    item.PropertyChanged -= item_PropertyChanged;
                    if (item.IsSelected == false)
                    {
                        isSelectionChanged = true;
                    }
                    item.IsSelected = true;
                    item.PropertyChanged += item_PropertyChanged;
                }

                if (isSelectionChanged)
                {
                    OnItemIsSelectedChanged();
                }

            }
            finally
            {
                UIMediaCollection.ExitReaderLock();
            }
        }

        public void deselectAllUIState()
        {

            UIMediaCollection.EnterReaderLock();
            try
            {
                bool isSelectionChanged = false;

                foreach (MediaFileItem item in UIMediaCollection.Items)
                {
                    item.PropertyChanged -= item_PropertyChanged;
                    if (item.IsSelected == true)
                    {
                        isSelectionChanged = true;
                    }
                    item.IsSelected = false;
                    item.PropertyChanged += item_PropertyChanged;
                }

                if (isSelectionChanged)
                {
                    OnItemIsSelectedChanged();
                }

            }
            finally
            {
                UIMediaCollection.ExitReaderLock();
            }

        }

        public List<MediaFileItem> getSelectedItemsUIState()
        {
            List<MediaFileItem> selected = new List<MediaFileItem>();

            UIMediaCollection.EnterReaderLock();

            try
            {
                foreach (MediaFileItem item in UIMediaCollection.Items)
                {
                    if (item.IsSelected)
                    {
                        selected.Add(item);
                    }
                }

                return (selected);
            }
            finally
            {
                UIMediaCollection.ExitReaderLock();
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

                    bool success = busyMediaCollection.Add(item);
                    if (success == false)
                    {
                        throw new MediaStateException("Cannot import" + item.Location + ", item already in use");
                    }

                    if (item.Media == null)
                    {
                        Task task = item.readMetaDataAsync(MediaFactory.ReadOptions.AUTO |
                                MediaFactory.ReadOptions.GENERATE_THUMBNAIL, token);
                        task.Wait();
                        if (item.Media == null || item.Media is UnknownMedia)
                        {
                            throw new MediaStateException("Error importing item, cannot read item metadata: " + item.Location);
                        }
                    }

                    success = item.import(token);
                    if (success)
                    {
                        importedItems.Add(item);
                    }
                    

                    busyMediaCollection.Remove(item);
                }

            }
            finally
            {
                if (importedItems.Count > 0)
                {
                    OnNrImportedItemsChanged(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add, importedItems));
                }

                busyMediaCollection.RemoveAll(items);
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

                    bool success = busyMediaCollection.Add(item);
                    if (success == false)
                    {
                        throw new MediaStateException("Cannot export" + item.Location + ", item already in use");
                    }

                    success = item.export(token);
                    if (success)
                    {
                        exportedItems.Add(item);
                    }

                    busyMediaCollection.Remove(item);
                }

            }
            finally
            {
                if (exportedItems.Count > 0)
                {
                    OnNrImportedItemsChanged(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove, exportedItems));
                }

                busyMediaCollection.RemoveAll(items);
            }
        }


        public void readMetadataRangeAsync(int start, int nrItems, CancellationToken token)
        {
            UIMediaCollection.EnterReaderLock();

            for (int i = 0; i < nrItems; i++)
            {
                // don't load files in use by another operation, locking them for loading might mess up the operations lock(s)
                if (busyMediaCollection.Contains(UIMediaCollection.Items[start + i])) continue;
                // don't reload already loaded items
                if (UIMediaCollection.Items[start + i].ItemState == MediaFileItemState.LOADED) continue;

                UIMediaCollection.Items[start + i].readMetaDataAsync(
                    MediaFactory.ReadOptions.AUTO |
                    MediaFactory.ReadOptions.GENERATE_THUMBNAIL,
                    token).ContinueWith(finishedTask => { });

            }

            UIMediaCollection.ExitReaderLock();
        }

        public void writeMetadata(MediaFileItem item, MediaFactory.WriteOptions options, IProgress progress)
        {
            List<MediaFileItem> dummy = new List<MediaFileItem>();
            dummy.Add(item);
            writeMetadata(dummy, options, progress);
        }

        public void writeMetadata(IEnumerable<MediaFileItem> items, MediaFactory.WriteOptions options, IProgress progress)
        {

            bool success = busyMediaCollection.AddRange(items);
            if (success == false)
            {
                throw new MediaStateException("Cannot write items, items already in use");
            }

            try
            {
                foreach (MediaFileItem item in items)
                {
                    if (progress.CancellationToken.IsCancellationRequested) return;

                    item.writeMetaData(options, progress);

                    busyMediaCollection.Remove(item);
                }

            }
            finally
            {

                busyMediaCollection.RemoveAll(items);
            }

        }

        void item_PropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("IsSelected"))
            {
                OnItemIsSelectedChanged();
            }
        }

        void OnNrItemsInStateChanged(NotifyCollectionChangedEventArgs args)
        {
            if (NrItemsInStateChanged != null && args != null)
            {
                NrItemsInStateChanged(this, args);
            }
        }

        void OnItemIsSelectedChanged()
        {
            if (ItemIsSelectedChanged != null)
            {
                ItemIsSelectedChanged(this, EventArgs.Empty);
            }
        }

        void OnNrImportedItemsChanged(NotifyCollectionChangedEventArgs args)
        {
            if (NrImportedItemsChanged != null)
            {
                NrImportedItemsChanged(this, args);
            }
        }

        public void readMetadata(MediaFileItem item, MediaFactory.ReadOptions options, CancellationToken token)
        {
            List<MediaFileItem> dummy = new List<MediaFileItem>();
            dummy.Add(item);
            readMetadata(dummy, options, token);
        }

        public void readMetadata(IEnumerable<MediaFileItem> items, MediaFactory.ReadOptions options, CancellationToken token)
        {
            UIMediaCollection.EnterReaderLock();

            try
            {
                foreach (MediaFileItem item in items)
                {

                    if (busyMediaCollection.Contains(item))
                    {
                        throw new MediaStateException("Cannot read metadata for item, already in use by another operation");
                    }

                    Task task = item.readMetaDataAsync(options, token);
                    task.Wait();

                }
            }
            finally
            {
                UIMediaCollection.ExitReaderLock();
            }
        }

        public void writeMetadata(IEnumerable<MediaFileItem> items)
        {
            throw new NotImplementedException();
        }
    }
}
