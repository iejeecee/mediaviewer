using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Utils;
using MvvmFoundation.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace MediaViewer.Model.Media.State.CollectionView
{
    class MediaStateCollectionView : ObservableObject {

        int sortedItemEnd;

        public MediaStateCollectionView(IMediaState mediaState)
        {
                      
            MediaState = mediaState;

            Media = new ObservableCollection<MediaFileItem>();                      
            BindingOperations.EnableCollectionSynchronization(Media, Media);

            SelectedMedia = new ObservableCollection<MediaFileItem>();
            BindingOperations.EnableCollectionSynchronization(SelectedMedia, SelectedMedia);

            MediaState.NrItemsInStateChanged += new EventHandler<MediaStateChangedEventArgs>(MediaState_NrItemsInStateChanged);
            MediaState.ItemPropertiesChanged += MediaState_ItemPropertiesChanged;

            sortedItemEnd = 0;
        }

        public virtual void detachFromMediaState()
        {
            MediaState.NrItemsInStateChanged -= new EventHandler<MediaStateChangedEventArgs>(MediaState_NrItemsInStateChanged);
            MediaState.ItemPropertiesChanged -= MediaState_ItemPropertiesChanged;
        }

        public ObservableCollection<MediaFileItem> Media { get; protected set; }
        public ObservableCollection<MediaFileItem> SelectedMedia { get; protected set; }

        public IMediaState MediaState { get; protected set; }
          
        Func<MediaFileItem, MediaFileItem, int> sortFunc;

        public Func<MediaFileItem, MediaFileItem, int> SortFunc
        {
            get { return sortFunc; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("SortFunc");
                }

                sortFunc = value;
            
            }
        }

        Func<MediaFileItem, bool> filter;

        public Func<MediaFileItem, bool> Filter
        {
            get { return filter; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Filter");
                }

                filter = value;
            }
        }

        public event EventHandler Cleared;

        protected void OnCleared()
        {
            if (Cleared != null)
            {
                Cleared(this, EventArgs.Empty);
            }
        }
    
        protected virtual void MediaState_ItemPropertiesChanged(object sender, PropertyChangedEventArgs e)
        {
            MediaFileItem item = sender as MediaFileItem;

            reSort(item);
     
        }

        private void MediaState_NrItemsInStateChanged(object sender, MediaStateChangedEventArgs e)
        {
            bool cleared = false;

            lock (Media)
            {
                switch (e.Action)
                {
                    case MediaStateChangedAction.Add:
                        foreach (MediaFileItem item in e.NewItems)
                        {
                            add(item);
                        }
                        break;
                    case MediaStateChangedAction.Remove:
                        foreach (MediaFileItem item in e.OldItems)
                        {
                            remove(item);
                        }
                        break;
                    case MediaStateChangedAction.Modified:
                        break;
                    case MediaStateChangedAction.Clear:
                        clear();
                        cleared = true;
                        break;
                    default:
                        break;
                }
            }

            if (cleared)
            {
                OnCleared();
            }
        }

        public void refresh()
        {
            MediaState.UIMediaCollection.EnterReaderLock();
            try
            {
                clear();

                foreach (MediaFileItem item in MediaState.UIMediaCollection.Items)
                {
                    add(item);
                }
            }
            finally
            {
                MediaState.UIMediaCollection.ExitReaderLock();
                OnCleared();
            }

        }

        protected void reSort(MediaFileItem item)
        {
            lock (Media)
            {
                remove(item);
                add(item);
            }
        }

        public void removeSelected(MediaFileItem item)
        {
            lock (SelectedMedia)
            {
                int index = SelectedMedia.IndexOf(item);

                if (index >= 0)
                {
                    SelectedMedia.RemoveAt(index);
                }
            }

        }

        protected void remove(MediaFileItem item)
        {
            lock (Media)
            {               
                int index = Media.IndexOf(item);

                if (index >= 0)
                {
                    Media.RemoveAt(index);
                    if (index < sortedItemEnd)
                    {
                        sortedItemEnd -= 1;
                    }
                }
            }

            removeSelected(item);
        }

        protected void add(MediaFileItem item)
        {
            lock (Media)
            {
                if (Filter(item) && !Media.Contains(item))
                {
                    insertSorted(item);
                }                
            }
        }

        public void addSelected(MediaFileItem item)
        {
            lock (SelectedMedia)
            {
                if (Filter(item) && !SelectedMedia.Contains(item))
                {
                    MiscUtils.insertIntoSortedCollection<MediaFileItem>(SelectedMedia, item, SortFunc);
                }
            }
        }

        void insertSorted(MediaFileItem item)
        {
            lock (Media)
            {                
                MiscUtils.insertIntoSortedCollection<MediaFileItem>(Media, item, SortFunc, 0, sortedItemEnd);

                sortedItemEnd++;                               
            }

        }

        protected void clear()
        {
            lock (Media)
            {
                Media.Clear();
                sortedItemEnd = 0;          
            }

            lock (SelectedMedia)
            {
                SelectedMedia.Clear();
            }

        }
               
    }

}
