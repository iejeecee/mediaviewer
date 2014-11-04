using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Utils;
using MediaViewer.Progress;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.State
{
 
    public class MediaState : BindableBase, IMediaState, IDisposable
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
            uiMediaCollection.ItemPropertyChanged += item_PropertyChanged;
                   
            debugOutput = false;

            MediaStateInfo = "Empty";
            MediaStateDateTime = DateTime.Now;
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
         
            UIMediaCollection.EnterWriteLock();

            if (DebugOutput) log.Info("begin add event: " + items.ElementAt(0).Location);

            try
            {
                UIMediaCollection.AddRange(items);             
            
                NrItemsInState = UIMediaCollection.Count;
            }
            finally
            {

                if (DebugOutput) log.Info("end add event: " + items.ElementAt(0).Location);

                UIMediaCollection.ExitWriteLock();
             
                fireEvents(MediaStateChangedAction.Add, items);                
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
                removed = UIMediaCollection.RemoveRange(removeItems);
           
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

        void fireEvents(MediaStateChangedAction action, IEnumerable<MediaFileItem> files)
        {
            MediaStateChangedEventArgs args = new MediaStateChangedEventArgs(action, files);

            OnNrItemsInStateChanged(args);

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
                SetProperty(ref mediaStateType, value);                
            }
        }

        String mediaStateInfo;

        public String MediaStateInfo
        {
            get { return mediaStateInfo; }
            set
            {                            
                SetProperty(ref mediaStateInfo, value);                                   
            }
        }

        DateTime mediaStateDateTime;

        public DateTime MediaStateDateTime
        {
            get { return mediaStateDateTime; }
            set
            {                              
                SetProperty(ref mediaStateDateTime, value);                 
            }
        }

        int nrLoadedItemsInState;

        public int NrLoadedItemsInState
        {
            protected set
            {
                SetProperty(ref nrLoadedItemsInState, value);
            }
            get { return (nrLoadedItemsInState); }
        }

        int nrItemsInState;

        public int NrItemsInState
        {
            protected set
            {              
                SetProperty(ref nrItemsInState, value);
            }
            get { return (nrItemsInState); }
        }
    }
}
