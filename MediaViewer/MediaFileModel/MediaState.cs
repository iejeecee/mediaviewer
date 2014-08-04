using MediaViewer.MediaDatabase;
using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.Progress;
using MediaViewer.Utils;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.MediaFileModel
{
    public class MediaState : ObservableObject, IMediaState, IDisposable
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public event EventHandler<MediaStateChangedEventArgs> NrItemsInStateChanged;
        public event EventHandler<MediaStateChangedEventArgs> NrImportedItemsChanged;        
        public event EventHandler<PropertyChangedEventArgs> ItemPropertiesChanged;

        MediaLockedCollection uiMediaCollection;

        /// <summary>
        /// All (potentially) visible Media in the user interface
        /// </summary>
        public MediaLockedCollection UIMediaCollection
        {
            get { return uiMediaCollection; }
            set { uiMediaCollection = value; }
        }
     
        bool debugOutput;

        public bool DebugOutput
        {
            get { return debugOutput; }
            set { debugOutput = value; }
        }

        public MediaState()
        {

            uiMediaCollection = new MediaLockedCollection(true);
                   
            debugOutput = false;

            MediaStateInfo = "MediaStateInfo Not Set";
            MediaStateDateTime = DateTime.MinValue;
            MediaStateType = MediaStateType.Directory;

            NrItemsInState = 0;
            NrLoadedItemsInState = 0;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool safe)
        {
            if (safe)
            {
                if (uiMediaCollection != null)
                {
                    uiMediaCollection.Dispose();
                    uiMediaCollection = null;
                }
          
            }
        }

  
        public void addUIState(IEnumerable<MediaFileItem> items)
        {

            if (items.Count() == 0) return;

            bool success = false;  

            UIMediaCollection.EnterWriteLock();

            if (DebugOutput) log.Info("begin add event: " + items.ElementAt(0).Location);

            try
            {
                success = UIMediaCollection.AddRange(items);

                if (success == false)
                {
                    throw new MediaStateException("Cannot add duplicate items to state");
                }

                foreach (MediaFileItem item in items)
                {
                    item.PropertyChanged += new PropertyChangedEventHandler(item_PropertyChanged);             
                }

                NrItemsInState = UIMediaCollection.Count;
            }
            finally
            {

                if (DebugOutput) log.Info("end add event: " + items.ElementAt(0).Location);

                UIMediaCollection.ExitWriteLock();

                if (success)
                {
                    fireEvents(MediaStateChangedAction.Add, items);
                }
            }
        }

        public void renameUIState(IEnumerable<MediaFileItem> oldItems, IEnumerable<String> newLocations)
        {

            if (oldItems.Count() == 0 || newLocations.Count() == 0) return;

            bool success = false;

            UIMediaCollection.EnterWriteLock();
            if (DebugOutput) log.Info("begin rename event " + oldItems.ElementAt(0).Location + " " + newLocations.ElementAt(0));

            try
            {
                success = UIMediaCollection.RenameRange(oldItems, newLocations);                            
            }
            finally
            {
                if (DebugOutput) log.Info("end rename event " + oldItems.ElementAt(0).Location + " " + newLocations.ElementAt(0));
                UIMediaCollection.ExitWriteLock();

                //if (success)
                //{
                    // redraw the UI since sorting order might have changed
                fireEvents(MediaStateChangedAction.Modified, null);                    
                //}
            }
        }

        public void removeUIState(IEnumerable<MediaFileItem> removeItems)
        {
            if (removeItems.Count() == 0) return;
        
            List<MediaFileItem> removed = new List<MediaFileItem>();

            UIMediaCollection.EnterWriteLock();
            if (DebugOutput) log.Info("begin remove event: " + removeItems.ElementAt(0).Location);

            try
            {
                removed = UIMediaCollection.RemoveAll(removeItems);

                foreach (MediaFileItem item in removed)
                {
                    item.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(item_PropertyChanged);
             
                }

                NrItemsInState = UIMediaCollection.Count;
            }
            finally
            {
                if (DebugOutput) log.Info("end remove event: " + removeItems.ElementAt(0).Location);
                UIMediaCollection.ExitWriteLock();

                fireEvents(MediaStateChangedAction.Remove, removed);
            }

        }

        public void clearUIState(String stateInfo, DateTime stateDateTime, MediaStateType stateType) 
        {
                   
            UIMediaCollection.EnterWriteLock();

            try
            {
                foreach (MediaFileItem item in UIMediaCollection.Items)
                {
                    item.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(item_PropertyChanged);

                    if (item.IsSelected == true)
                    {
                        item.IsSelected = false;
                    }

                }

                UIMediaCollection.Clear();

                MediaStateInfo = stateInfo;
                MediaStateDateTime = stateDateTime;
                MediaStateType = stateType;

                NrItemsInState = 0;
                NrLoadedItemsInState = 0;
              
            }
            finally
            {
                UIMediaCollection.ExitWriteLock();

                fireEvents(MediaStateChangedAction.Clear, null);
                
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
               
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    bool isImported = item.delete();

                    if (isImported)
                    {
                        deletedImportedItems.Add(item);
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

                if (deletedImportedItems.Count > 0)
                {
                    OnNrImportedItemsChanged(new MediaStateChangedEventArgs(
                        MediaStateChangedAction.Remove, deletedImportedItems));
                }
             
            }
        }

        public void move(MediaFileItem item, String location, ICancellableOperationProgress progress)
        {
            List<MediaFileItem> dummy = new List<MediaFileItem>();
            dummy.Add(item);

            List<String> locationDummy = new List<string>();
            locationDummy.Add(location);

            move(dummy, locationDummy, progress);
        }

        public void move(IEnumerable<MediaFileItem> items, IEnumerable<String> locations,
            ICancellableOperationProgress progress)
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
             
                    if (!item.Location.Equals(location))
                    {                        
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
                            deletedImportedItems.Add(item);
                            addedImportedItems.Add(MediaFileItem.Factory.create(location));
                        }
                    }
             
                }

            }
            finally
            {
                if (deletedImportedItems.Count > 0)
                {
                    OnNrImportedItemsChanged(new MediaStateChangedEventArgs(
                        MediaStateChangedAction.Replace, addedImportedItems, deletedImportedItems));
                }

            }

        }

        void fireEvents(MediaStateChangedAction action, IEnumerable<MediaFileItem> files)
        {
            MediaStateChangedEventArgs args = new MediaStateChangedEventArgs(action, files);

            OnNrItemsInStateChanged(args);

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


        public void readMetadataRangeAsync(int start, int nrItems, CancellationToken token)
        {
            UIMediaCollection.EnterReaderLock();
            try
            {
                for (int i = 0; i < nrItems; i++)
                {
                    // don't reload already loaded items
                    if (UIMediaCollection.Items[start + i].ItemState == MediaFileItemState.LOADED) continue;

                    UIMediaCollection.Items[start + i].readMetaDataAsync(
                        MediaFactory.ReadOptions.AUTO |
                        MediaFactory.ReadOptions.GENERATE_THUMBNAIL,
                        token).ContinueWith(finishedTask => { });

                }
            }
            finally
            {
                UIMediaCollection.ExitReaderLock();
            }
        }

        public void writeMetadata(MediaFileItem item, MediaFactory.WriteOptions options, ICancellableOperationProgress progress)
        {
            List<MediaFileItem> dummy = new List<MediaFileItem>();
            dummy.Add(item);
            writeMetadata(dummy, options, progress);
        }

        public void writeMetadata(IEnumerable<MediaFileItem> items, MediaFactory.WriteOptions options, ICancellableOperationProgress progress)
        {                 
            foreach (MediaFileItem item in items)
            {
                if (progress.CancellationToken.IsCancellationRequested) return;

                item.writeMetaData(options, progress);                  
            }
           
        }

        void item_PropertyChanged(Object sender, PropertyChangedEventArgs e)
        {        
            if (ItemPropertiesChanged != null)
            {
                ItemPropertiesChanged(sender, e);
            }
        }

        void OnNrItemsInStateChanged(MediaStateChangedEventArgs args)
        {
            if (NrItemsInStateChanged != null && args != null)
            {
                NrItemsInStateChanged(this, args);
            }
        }
      

        void OnNrImportedItemsChanged(MediaStateChangedEventArgs args)
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
                    item.readMetaData(options, token);                    
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

        MediaStateType mediaStateType;

        public MediaStateType MediaStateType
        {
            get { return mediaStateType; }
            set
            {                               
                mediaStateType = value;             
                NotifyPropertyChanged();                
            }
        }

        String mediaStateInfo;

        public String MediaStateInfo
        {
            get { return mediaStateInfo; }
            set
            {              
                mediaStateInfo = value;
                NotifyPropertyChanged();                                   
            }
        }

        DateTime mediaStateDateTime;

        public DateTime MediaStateDateTime
        {
            get { return mediaStateDateTime; }
            set
            {                
                mediaStateDateTime = value;
                NotifyPropertyChanged();                 
            }
        }

        int nrLoadedItemsInState;

        public int NrLoadedItemsInState
        {
            protected set
            {
                nrLoadedItemsInState = value;
                NotifyPropertyChanged();
            }
            get { return (nrLoadedItemsInState); }
        }

        int nrItemsInState;

        public int NrItemsInState
        {
            protected set
            {
                nrItemsInState = value;
                NotifyPropertyChanged();
            }
            get { return (nrItemsInState); }
        }
    }
}
