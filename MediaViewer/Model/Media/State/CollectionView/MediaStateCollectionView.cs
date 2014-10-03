using MediaViewer.Model.Collections.Sort;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace MediaViewer.Model.Media.State.CollectionView
{
    public class MediaStateCollectionView : ObservableObject {
        
        public event EventHandler<MediaStateChangedEventArgs> NrItemsInStateChanged;
        public event EventHandler<MediaStateChangedEventArgs> ItemPropertiesChanged;
        public event EventHandler SelectionChanged;
        public event EventHandler Cleared;
      
        int sortedItemEnd;

        public MediaStateCollectionView(IMediaState mediaState)
        {                      
            MediaState = mediaState;

            Media = new ObservableCollection<SelectableMediaFileItem>();                      
            BindingOperations.EnableCollectionSynchronization(Media, Media);
         
            MediaState.NrItemsInStateChanged += MediaState_NrItemsInStateChanged;
            MediaState.ItemPropertiesChanged += MediaState_ItemPropertiesChanged;

            sortedItemEnd = 0;

            sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>((a, b) =>
            {
                return (a.ToString().CompareTo(b.ToString()));
            });

            filter = new Func<SelectableMediaFileItem,bool>((a) => {

                return (true);
            });
            
        }

        public virtual void detachFromMediaState()
        {
            MediaState.NrItemsInStateChanged -= new EventHandler<MediaStateChangedEventArgs>(MediaState_NrItemsInStateChanged);
            MediaState.ItemPropertiesChanged -= MediaState_ItemPropertiesChanged;
        }

        public ObservableCollection<SelectableMediaFileItem> Media { get; protected set; }

        public void lockMedia() {

            Monitor.Enter(Media);
        }

        public void unlockMedia()
        {
            Monitor.Exit(Media);
        }
     
        public IMediaState MediaState { get; protected set; }

        protected Func<SelectableMediaFileItem, SelectableMediaFileItem, int> sortFunc;

        public Func<SelectableMediaFileItem, SelectableMediaFileItem, int> SortFunc
        {
            get { return sortFunc; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("SortFunc");
                }

                sortFunc = value;

                refresh();
            }
        }

        protected Func<SelectableMediaFileItem, bool> filter;

        public Func<SelectableMediaFileItem, bool> Filter
        {
            get { return filter; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Filter");
                }

                filter = value;

                refresh();
            }
        }

        public List<MediaFileItem> getSelectedItems()
        {
            List<MediaFileItem> selectedItems = new List<MediaFileItem>();

            lock (Media)
            {
                foreach (SelectableMediaFileItem item in Media)
                {
                    if (item.IsSelected)
                    {
                        selectedItems.Add(item.Item);
                    }
                }
            }

            return (selectedItems);
        }
      
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
                SelectableMediaFileItem selectableItem = new SelectableMediaFileItem(item);

                int index = Media.IndexOf(selectableItem);

                if (index >= 0)
                {
                    selectableItem = Media[index];

                    Media.RemoveAt(index);
                    if (index < sortedItemEnd)
                    {
                        sortedItemEnd -= 1;
                    }

                    if (Filter(selectableItem))
                    {
                        insertSorted(selectableItem);
                    }   
                }
              
            }
        }

        public void selectAll()
        {
            bool selectionChanged = false;

            lock (Media)
            {
                foreach (SelectableMediaFileItem selectableItem in Media)
                {
                    selectableItem.SelectionChanged -= selectableItem_SelectionChanged;

                    if (selectableItem.IsSelected == false)
                    {
                        selectionChanged = true;
                    }

                    selectableItem.IsSelected = true;

                    selectableItem.SelectionChanged += selectableItem_SelectionChanged;
                }

            }

            if (selectionChanged)
            {
                OnSelectionChanged();
            }
        }
       
        public void deselectAll()
        {
            bool selectionChanged = false;

            lock (Media)
            {
                foreach (SelectableMediaFileItem selectableItem in Media)
                {
                    selectableItem.SelectionChanged -= selectableItem_SelectionChanged;

                    if (selectableItem.IsSelected == true)
                    {
                        selectionChanged = true;
                    }

                    selectableItem.IsSelected = false;

                    selectableItem.SelectionChanged += selectableItem_SelectionChanged;
                }

            }

            if (selectionChanged)
            {
                OnSelectionChanged();
            }
        }

        public int getIndexOf(MediaFileItem item) {

            SelectableMediaFileItem temp = new SelectableMediaFileItem(item);

            lock (Media)
            {
                for(int i = 0; i < Media.Count; i++)
                {
                    if (Media[i].Equals(temp))
                    {
                        return (i);
                    }

                }

            }

            return (-1);
        }

        public void setIsSelected(MediaFileItem item, bool isSelected = true)
        {
           
            SelectableMediaFileItem temp = new SelectableMediaFileItem(item);

            lock (Media)
            {
                foreach (SelectableMediaFileItem selectableItem in Media)
                {                    
                    if (selectableItem.Equals(temp))
                    {
                        selectableItem.IsSelected = isSelected;
                    }
                                       
                }

            }
            
        }

        protected void remove(MediaFileItem item)
        {
            bool selectionChanged = false;
            bool isItemRemoved = false;

            lock (Media)
            {
                SelectableMediaFileItem selectableItem = new SelectableMediaFileItem(item);

                int index = Media.IndexOf(selectableItem);

                if (index >= 0)
                {
                    if (Media[index].IsSelected)
                    {
                        selectionChanged = true;
                    }

                    Media.RemoveAt(index);
                    if (index < sortedItemEnd)
                    {
                        sortedItemEnd -= 1;
                    }

                    selectableItem.SelectionChanged -= selectableItem_SelectionChanged;
                    isItemRemoved = true;
                }
            }

            if (isItemRemoved)
            {
                OnNrItemsInStateChanged(new MediaStateChangedEventArgs(MediaStateChangedAction.Remove, item));
            }

            if (selectionChanged)
            {
                OnSelectionChanged();
            }
            
        }



        protected void add(MediaFileItem item)
        {
            bool isItemAdded = false;

            lock (Media)
            {
                SelectableMediaFileItem selectableItem = new SelectableMediaFileItem(item);

                if (Filter(selectableItem) && !Media.Contains(selectableItem))
                {
                    insertSorted(selectableItem);
                    isItemAdded = true;
                }

                selectableItem.SelectionChanged += selectableItem_SelectionChanged;
            }

            if (isItemAdded)
            {
                OnNrItemsInStateChanged(new MediaStateChangedEventArgs(MediaStateChangedAction.Add, item));
            }

        }

        


        void insertSorted(SelectableMediaFileItem selectableItem)
        {
            lock (Media)
            {
                CollectionsSort.insertIntoSortedCollection<SelectableMediaFileItem>(Media, selectableItem, SortFunc, 0, sortedItemEnd);

                sortedItemEnd++;                               
            }

        }

        protected void clear()
        {
            bool selectionChanged = false;           

            lock (Media)
            {
                foreach (SelectableMediaFileItem selectableItem in Media)
                {
                    selectableItem.SelectionChanged -= selectableItem_SelectionChanged;

                    if (selectableItem.IsSelected)
                    {
                        selectionChanged = true;
                    }
                }

                Media.Clear();
                sortedItemEnd = 0;          
            }

            if (selectionChanged)
            {
                OnSelectionChanged();
            }

            OnNrItemsInStateChanged(new MediaStateChangedEventArgs(MediaStateChangedAction.Clear));

        }

        private void selectableItem_SelectionChanged(object sender, EventArgs e)
        {
            OnSelectionChanged();
        }

        void OnSelectionChanged()
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, EventArgs.Empty);
            }
        }

        private void OnNrItemsInStateChanged(MediaStateChangedEventArgs args)
        {
            if (NrItemsInStateChanged != null)
            {
                NrItemsInStateChanged(this, args);
            }
        }
               
    }

}
