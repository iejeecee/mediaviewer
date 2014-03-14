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
        public event NotifyCollectionChangedEventHandler NrItemsInStateChanged;
        public event NotifyCollectionChangedEventHandler NrImportedItemsChanged;
        public event EventHandler ItemIsSelectedChanged;
        public event EventHandler ItemPropertiesChanged;

        // This collection contains all media that is currently visible/active in the user interface
        MediaLockedCollection mediaCollection;
        // This collection contains all media that is scheduled for a operation (e.g. importing, updating etc)
        MediaLockedCollection busyItems;
        
        public MediaLockedCollection MediaCollection
        {
            get { return mediaCollection; }
            set { mediaCollection = value; }
        }
       
        public MediaState()
        {
     
            mediaCollection = new MediaLockedCollection();
            busyItems = new MediaLockedCollection();             

        }

        public bool isInUse(MediaFileItem item)
        {
            return (busyItems.Contains(item));
        }

        public void add(IEnumerable<MediaFileItem> items)
        {

            if (items.Count() == 0) return;

            bool itemIsSelectedChanged = false;

            MediaCollection.EnterWriteLock();
            System.Diagnostics.Debug.WriteLine("begin add event: " + items.ElementAt(0).Location);

            try
            {

                bool success = MediaCollection.AddRange(items);

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
                System.Diagnostics.Debug.WriteLine("end add event: " + items.ElementAt(0).Location);
                MediaCollection.ExitWriteLock();
            }
        }

        public void rename(IEnumerable<MediaFileItem> oldItems, IEnumerable<MediaFileItem> newItems)
        {

            if (oldItems.Count() == 0 || newItems.Count() == 0) return;

            MediaCollection.EnterWriteLock();
            System.Diagnostics.Debug.WriteLine("begin rename event " + oldItems.ElementAt(0).Location + " " + newItems.ElementAt(0).Location);
            try
            {

                bool success = MediaCollection.RenameRange(oldItems, newItems);
               
                //fireEvents(NotifyCollectionChangedAction.Reset, null, false);

                if (success == false)
                {
                    throw new MediaStateException("Cannot rename non-existing items");
                }
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine("end rename event " + oldItems.ElementAt(0).Location + " " + newItems.ElementAt(0).Location);
                MediaCollection.ExitWriteLock();
            }
        }

        public void remove(IEnumerable<MediaFileItem> removeItems)
        {
            if (removeItems.Count() == 0) return;

            bool itemIsSelectedChanged = false;

            MediaCollection.EnterWriteLock();
            System.Diagnostics.Debug.WriteLine("begin remove event: " + removeItems.ElementAt(0).Location);

            try
            {
                List<MediaFileItem> removed = MediaCollection.RemoveAll(removeItems);

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
                System.Diagnostics.Debug.WriteLine("end remove event: " + removeItems.ElementAt(0).Location);
                MediaCollection.ExitWriteLock();
            }

        }

        public void clear()
        {
     
            bool itemIsSelectedChanged = false;

             MediaCollection.EnterWriteLock();

             try
             {
                 foreach (MediaFileItem item in MediaCollection.Items)
                 {                   
                     item.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(item_PropertyChanged);

                     if (item.IsSelected == true)
                     {
                         itemIsSelectedChanged = true;
                     }
                   
                 }

                 MediaCollection.Clear();

                 fireEvents(NotifyCollectionChangedAction.Reset, null, itemIsSelectedChanged);
             }
             finally
             {
                 MediaCollection.ExitWriteLock();
             }
        }

        public void delete(IEnumerable<MediaFileItem> removeItems, CancellationToken token)
        {
            List<MediaFileItem> deletedImportedItems = new List<MediaFileItem>();
            List<MediaFileItem> deletedItems = new List<MediaFileItem>();
            bool success = busyItems.AddRange(removeItems);
            
            if (success == false)
            {
                throw new MediaStateException("Cannot delete items, items already in use");
            }
           
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
                        deletedImportedItems.Add(item);
                    }

                    busyItems.Remove(item);
                    deletedItems.Add(item);
                }
                                                                                 
            }
            finally
            {
                if (MediaFileWatcher.Instance.IsWatcherEnabled == false)
                {
                    // if the watcher is not enabled remove the items from the state ourselves
                    remove(deletedItems);
                }

                if (deletedImportedItems.Count > 0)
                {
                    OnNrImportedItemsChanged(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove, deletedImportedItems));
                }

                busyItems.RemoveAll(removeItems);                                      
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

            bool success = busyItems.AddRange(items);
            if (success == false)
            {
                throw new MediaStateException("Cannot move items, items already in use");
            }

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

                    if (!item.Location.Equals(location))
                    {                     
                        bool isImported = item.move(location, progress);

                        if (isImported)
                        {
                            deletedImportedItems.Add(item);
                            addedImportedItems.Add(new MediaFileItem(location));
                        }
                    }

                    busyItems.Remove(item);
                }

            }
            finally
            {
                if (deletedImportedItems.Count > 0)
                {
                    OnNrImportedItemsChanged(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace, addedImportedItems, deletedImportedItems, 0));
                }

                busyItems.RemoveAll(items);
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

        public void selectAll()
        {
            MediaCollection.EnterReaderLock();
            try
            {
                bool isSelectionChanged = false;

                foreach (MediaFileItem item in MediaCollection.Items)
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
                MediaCollection.ExitReaderLock();
            }
        }
       
        public void deselectAll()
        {

            MediaCollection.EnterReaderLock();
            try
            {
                bool isSelectionChanged = false;

                foreach (MediaFileItem item in MediaCollection.Items)
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
                MediaCollection.ExitReaderLock();
            }
            
        }
      
        public List<MediaFileItem> getSelectedItems()
        {
            List<MediaFileItem> selected = new List<MediaFileItem>();

            MediaCollection.EnterReaderLock();

            try
            {
                foreach (MediaFileItem item in MediaCollection.Items)
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
                MediaCollection.ExitReaderLock();
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
                for (int i = 0; i < items.Count(); i++)
                {
                    MediaFileItem item = items.ElementAt(i);

                    if (token.IsCancellationRequested) return;

                    MediaCollection.EnterReaderLock();
                   
                    try
                    {                        
                        // If the mediaitem is in the current mediacollection use that instance
                        // to ensure changes are reflected in the user interface
                        MediaFileItem insertItem = MediaCollection.Find(item);

                        if (insertItem == null)
                        {
                            insertItem = item;
                        }
                       
                        
                        if (insertItem.Media == null)
                        {
                            readMetadata(insertItem, MediaFactory.ReadOptions.AUTO |
                                    MediaFactory.ReadOptions.GENERATE_THUMBNAIL, token);
                            if (insertItem.Media == null || insertItem.Media is UnknownMedia)
                            {
                                throw new MediaStateException("Error importing item, cannot read item metadata: " + item.Location);
                            }
                        }

                        bool success = busyItems.Add(item);
                        if (success == false)
                        {
                            throw new MediaStateException("Cannot import item, item already in use: " + item.Location);
                        }

                        success = insertItem.import(token);
                        if (success)
                        {
                            importedItems.Add(insertItem);
                        }

                        //item.Media = insertItem.Media;
                    }
                    finally
                    {
                        busyItems.Remove(item);
                        MediaCollection.ExitReaderLock();
                    }
                }
              
            }
            finally
            {
                if (importedItems.Count > 0)
                {                                      
                    OnNrImportedItemsChanged(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add, importedItems));
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

            bool success = busyItems.AddRange(items);
            if (success == false)
            {
                throw new MediaStateException("Cannot export items, items already in use");
            }

            try
            {
                foreach (MediaFileItem item in items)
                {
                    if (token.IsCancellationRequested) return;

                    success = item.export(token);
                    if (success)
                    {
                        exportedItems.Add(item);
                    }

                    busyItems.Remove(item);
                }

            }
            finally
            {
                if (exportedItems.Count > 0)
                {
                    OnNrImportedItemsChanged(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove, exportedItems));
                }

                busyItems.RemoveAll(items);
            }
        }

    
        public void readMetadataRangeAsync(int start, int nrItems, CancellationToken token)
        {
            MediaCollection.EnterReaderLock();

            for (int i = 0; i < nrItems; i++)
            {
                // don't load files in use by another operation, locking them for loading might mess up the operations lock(s)
                if (busyItems.Contains(MediaCollection.Items[start + i])) continue;
                // don't reload already loaded items
                if (MediaCollection.Items[start + i].ItemState == MediaFileItemState.LOADED) continue;

                MediaCollection.Items[start + i].readMetaDataAsync(
                    MediaFactory.ReadOptions.AUTO |
                    MediaFactory.ReadOptions.GENERATE_THUMBNAIL,
                    token).ContinueWith(finishedTask => { });

            }

            MediaCollection.ExitReaderLock();
        }

        public void writeMetadata(MediaFileItem item, MediaFactory.WriteOptions options, IProgress progress)
        {
            List<MediaFileItem> dummy = new List<MediaFileItem>();
            dummy.Add(item);
            writeMetadata(dummy, options, progress);
        }

        public void writeMetadata(IEnumerable<MediaFileItem> items, MediaFactory.WriteOptions options, IProgress progress)
        {
          
            bool success = busyItems.AddRange(items);
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
                    
                    busyItems.Remove(item);
                }

            }
            finally
            {
             
                busyItems.RemoveAll(items);
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
            MediaCollection.EnterReaderLock();

            try
            {
                foreach (MediaFileItem item in items)
                {

                    if (busyItems.Contains(item))
                    {
                        throw new MediaStateException("Cannot read metadata for item, already in use by another operation");
                    }
                
                    Task task = item.readMetaDataAsync(options, token);
                    task.Wait();
                
                }
            }
            finally
            {
                MediaCollection.ExitReaderLock();
            }
        }

        public void writeMetadata(IEnumerable<MediaFileItem> items)
        {
            throw new NotImplementedException();
        }
    }
}
