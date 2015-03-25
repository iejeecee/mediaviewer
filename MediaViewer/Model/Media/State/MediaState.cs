using MediaViewer.Infrastructure.Logging;
using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.metadata.Metadata;
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
    public enum MediaStateType
    {
        Directory,
        SearchResult,
        Other
    }
 
    public class MediaState : BindableBase, IDisposable
    {
        
        public event EventHandler<MediaStateChangedEventArgs> NrItemsInStateChanged;        
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

  
        public void addUIState(IEnumerable<MediaItem> items)
        {
            if (items.Count() == 0) return;
         
            UIMediaCollection.EnterWriteLock();

            if (DebugOutput) Logger.Log.Info("begin add event: " + items.ElementAt(0).Location);

            try
            {
                UIMediaCollection.AddRange(items);             
            
                NrItemsInState = UIMediaCollection.Count;
            }
            finally
            {

                if (DebugOutput) Logger.Log.Info("end add event: " + items.ElementAt(0).Location);

                UIMediaCollection.ExitWriteLock();
             
                fireEvents(MediaStateChangedAction.Add, items);                
            }
        }

        public void renameUIState(IEnumerable<MediaItem> oldItems, IEnumerable<String> newLocations)
        {

            if (oldItems.Count() == 0 || newLocations.Count() == 0) return;

            bool success = false;

            UIMediaCollection.EnterWriteLock();
            if (DebugOutput) Logger.Log.Info("begin rename event " + oldItems.ElementAt(0).Location + " " + newLocations.ElementAt(0));

            try
            {
                success = UIMediaCollection.RenameRange(oldItems, newLocations);                            
            }
            finally
            {
                if (DebugOutput) Logger.Log.Info("end rename event " + oldItems.ElementAt(0).Location + " " + newLocations.ElementAt(0));
                UIMediaCollection.ExitWriteLock();
              
                // redraw the UI since sorting order might have changed
                fireEvents(MediaStateChangedAction.Modified, null);                                  
            }
        }

        public void removeUIState(IEnumerable<MediaItem> removeItems)
        {
            if (removeItems.Count() == 0) return;
        
            List<MediaItem> removed = new List<MediaItem>();

            UIMediaCollection.EnterWriteLock();
            if (DebugOutput) Logger.Log.Info("begin remove event: " + removeItems.ElementAt(0).Location);

            try
            {
                removed = UIMediaCollection.RemoveRange(removeItems);
           
                NrItemsInState = UIMediaCollection.Count;
            }
            finally
            {
                if (DebugOutput) Logger.Log.Info("end remove event: " + removeItems.ElementAt(0).Location);
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

        protected void fireEvents(MediaStateChangedAction action, IEnumerable<MediaItem> files)
        {
            MediaStateChangedEventArgs args = new MediaStateChangedEventArgs(action, files);

            OnNrItemsInStateChanged(args);

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
