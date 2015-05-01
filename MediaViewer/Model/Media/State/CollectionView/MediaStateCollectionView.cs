using MediaViewer.Model.Collections;
using MediaViewer.Model.Collections.Sort;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Utils;
using Microsoft.Practices.Prism.Mvvm;
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
    public class MediaStateCollectionView : BindableBase {

        public event EventHandler<MediaStateCollectionViewChangedEventArgs> NrItemsInStateChanged;
        public event EventHandler<PropertyChangedEventArgs> ItemPropertyChanged;
        public event EventHandler<int> ItemResorted;
        public event EventHandler SelectionChanged;
        public event EventHandler Cleared;

        public ListCollectionView FilterModes { get; set; }
        public ListCollectionView SortModes { get; set; }

        int sortedItemEnd;

        protected MediaStateCollectionView(MediaState mediaState)
        {                      
            MediaState = mediaState;

            Media = new SelectableMediaLockedCollection();                      
            Media.ItemPropertyChanged += Media_ItemPropertyChanged;          
         
            MediaState.NrItemsInStateChanged += MediaState_NrItemsInStateChanged;
            MediaState.ItemPropertiesChanged += MediaState_ItemPropertiesChanged;

            sortedItemEnd = 0;

            sortFunc = new Func<SelectableMediaItem, SelectableMediaItem, int>((a, b) =>
            {
                return (a.ToString().CompareTo(b.ToString()));
            });

            filter = new Func<SelectableMediaItem,bool>((a) => {

                return (true);
            });
            
        }

    
        private void Media_ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {            
            OnItemPropertyChanged(sender, e);
        }

        public virtual void detachFromMediaState()
        {
            MediaState.NrItemsInStateChanged -= new EventHandler<MediaStateChangedEventArgs>(MediaState_NrItemsInStateChanged);
            MediaState.ItemPropertiesChanged -= MediaState_ItemPropertiesChanged;
        }

        public SelectableMediaLockedCollection Media { get; protected set; }
          
        public MediaState MediaState { get; protected set; }

        protected Func<SelectableMediaItem, SelectableMediaItem, int> sortFunc;

        public Func<SelectableMediaItem, SelectableMediaItem, int> SortFunc
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

        protected Func<SelectableMediaItem, bool> filter;

        public Func<SelectableMediaItem, bool> Filter
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

        public int getSelectedItem(out MediaItem selectedItem)
        {           
            Media.EnterReaderLock();
            try
            {
                for (int i = 0; i < Media.Count; i++)
                {
                    if (Media[i].IsSelected)
                    {
                        selectedItem = Media[i].Item;
                        return (i);
                    }
                }

                selectedItem = null;
                return (-1);
            }
            finally
            {
                Media.ExitReaderLock();
            }
        }

        public virtual Object getExtraInfo(SelectableMediaItem item)
        {
            return (null);
        }


        public List<MediaItem> getSelectedItems()
        {
            List<MediaItem> selectedItems = new List<MediaItem>();

            Media.EnterReaderLock();
            try
            {
                foreach (SelectableMediaItem item in Media)
                {
                    if (item.IsSelected)
                    {
                        selectedItems.Add(item.Item);
                    }
                }

                return (selectedItems);
            }
            finally
            {
                Media.ExitReaderLock();
            }
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
            MediaItem item = sender as MediaItem;

            reSort(item);
     
        }

        private void MediaState_NrItemsInStateChanged(object sender, MediaStateChangedEventArgs e)
        {
                     
            switch (e.Action)
            {
                case MediaStateChangedAction.Add:                      
                    add(e.NewItems);                       
                    break;
                case MediaStateChangedAction.Remove:                        
                    remove(e.OldItems);                        
                    break;
                case MediaStateChangedAction.Modified:
                    break;
                case MediaStateChangedAction.Clear:
                    clear();
                    OnCleared();
                    break;
                default:
                    break;
            }
                              
        }

        public void refresh()
        {
            MediaState.UIMediaCollection.EnterReaderLock();
            try
            {
                clear();             
                add(MediaState.UIMediaCollection);               
            }
            finally
            {
                MediaState.UIMediaCollection.ExitReaderLock();
                OnCleared();
            }

        }

        protected void reSort(MediaItem item)
        {

            int index = 0;
            int newIndex = 0;

            Media.EnterWriteLock();
            try
            {
                SelectableMediaItem selectableItem = new SelectableMediaItem(item);

                index = Media.IndexOf(selectableItem);
                newIndex = index;

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
                        newIndex = insertSorted(selectableItem);                        
                    }
                }

            }
            finally
            {
                Media.ExitWriteLock();
            }

            if (newIndex != index)
            {
                OnItemResorted(item, newIndex);
            }
         
        }

        public void selectAll()
        {
            bool selectionChanged = false;

            Media.EnterWriteLock();
            try
            {
                foreach (SelectableMediaItem selectableItem in Media)
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
            finally
            {
                Media.ExitWriteLock();
            }

            if (selectionChanged)
            {
                OnSelectionChanged();
            }
        }

        public void selectExclusive(MediaItem item)
        {
            bool selectionChanged = false;

            Media.EnterWriteLock();

            SelectableMediaItem selectableItem = new SelectableMediaItem(item);
            try
            {
                foreach (SelectableMediaItem media in Media)
                {
                    if (media.IsSelected)
                    {
                        if(!media.Equals(selectableItem)) {

                            media.SelectionChanged -= selectableItem_SelectionChanged;
                            media.IsSelected = false;
                            media.SelectionChanged += selectableItem_SelectionChanged;

                            selectionChanged = true;
                        }

                    }
                    else if (media.Equals(selectableItem))
                    {
                        media.SelectionChanged -= selectableItem_SelectionChanged;
                        media.IsSelected = true;
                        media.SelectionChanged += selectableItem_SelectionChanged;

                        selectionChanged = true;
                    }

                }
               
            }
            finally
            {
                Media.ExitWriteLock();
            }

            if (selectionChanged)
            {
                OnSelectionChanged();
            }
        }

        public void selectRange(MediaItem end)
        {
            bool selectionChanged = false;

            Media.EnterWriteLock();
            try
            {
                MediaItem start = null;

                int startIndex = getSelectedItem(out start);
                int endIndex = Media.IndexOf(new SelectableMediaItem(end));
                
                if(endIndex == -1) {

                    return;

                } else if(startIndex == -1) {

                    Media[endIndex].IsSelected = true;
                }

                if(endIndex < startIndex) {

                    int temp = startIndex;
                    startIndex = endIndex;
                    endIndex = temp;
                }
             
                for(int i = startIndex; i <= endIndex; i++)
                {  
                    Media[i].SelectionChanged -= selectableItem_SelectionChanged;

                    if (Media[i].IsSelected == false)
                    {
                        selectionChanged = true;
                    }

                    Media[i].IsSelected = true;

                    Media[i].SelectionChanged += selectableItem_SelectionChanged;
                }

            }
            finally
            {
                Media.ExitWriteLock();
            }

            if (selectionChanged)
            {
                OnSelectionChanged();
            }
        }

        public void deselectAll()
        {
            bool selectionChanged = false;

            Media.EnterWriteLock();
            try
            {
                foreach (SelectableMediaItem selectableItem in Media)
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
            finally
            {
                Media.ExitWriteLock();
            }

            if (selectionChanged)
            {
                OnSelectionChanged();
            }
        }
        
        public void setIsSelected(MediaItem item, bool isSelected = true)
        {
           
            SelectableMediaItem temp = new SelectableMediaItem(item);

            Media.EnterWriteLock();
            try
            {

                foreach (SelectableMediaItem selectableItem in Media)
                {
                    if (selectableItem.Equals(temp))
                    {
                        selectableItem.IsSelected = isSelected;
                    }

                }

            }
            finally
            {
                Media.ExitWriteLock();
            }
            
        }

        protected void remove(IEnumerable<MediaItem> items)
        {
            Media.EnterWriteLock();
            try
            {
                foreach (MediaItem item in items)
                {
                    remove(item);
                }
            }
            finally
            {
                Media.ExitWriteLock();
            }
        }

        protected void remove(MediaItem item)
        {
            bool selectionChanged = false;
            bool isItemRemoved = false;

            SelectableMediaItem selectableItem = new SelectableMediaItem(item);

            Media.EnterWriteLock();
            try
            {                
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
            finally
            {
                Media.ExitWriteLock();
            }

            if (isItemRemoved)
            {
                OnNrItemsInStateChanged(new MediaStateCollectionViewChangedEventArgs(MediaStateChangedAction.Remove, selectableItem));
            }

            if (selectionChanged)
            {
                OnSelectionChanged();
            }
            
        }

        protected void add(IEnumerable<MediaItem> items)
        {
            Media.EnterWriteLock();
            try
            {
                // Use a fast(er) path if we are just adding a batch of items to a empty list
                // instead of firing off a whole bunch of itemchanged events
                if (this.Media.Count == 0 && items.Count() > 1)
                {                   
                    List<SelectableMediaItem> filteredItems = new List<SelectableMediaItem>();
                    

                    foreach (MediaItem item in items)
                    {
                        SelectableMediaItem selectableItem = new SelectableMediaItem(item);                        
                       
                        if (Filter(selectableItem))
                        {
                            selectableItem.SelectionChanged += selectableItem_SelectionChanged;
                            CollectionsSort.insertIntoSortedCollection(filteredItems, selectableItem, sortFunc);                         
                        }
                    }

                    Media.AddRange(filteredItems);
                    sortedItemEnd = Media.Count;

                    OnNrItemsInStateChanged(new MediaStateCollectionViewChangedEventArgs(MediaStateChangedAction.Add, filteredItems));
                }
                else
                {
                    foreach (MediaItem item in items)
                    {
                        add(item);
                    }
                }
            }
            finally
            {
                Media.ExitWriteLock();
            }
        }

        protected void add(MediaItem item)
        {
            bool isItemAdded = false;

            SelectableMediaItem selectableItem = new SelectableMediaItem(item);

            Media.EnterWriteLock();
            try
            {                
                if (Filter(selectableItem) && !Media.Contains(selectableItem))
                {
                    insertSorted(selectableItem);
                    isItemAdded = true;
                }

                selectableItem.SelectionChanged += selectableItem_SelectionChanged;
            }
            finally
            {
                Media.ExitWriteLock();
            }

            if (isItemAdded)
            {
                OnNrItemsInStateChanged(new MediaStateCollectionViewChangedEventArgs(MediaStateChangedAction.Add, selectableItem));
            }

        }
        
        int insertSorted(SelectableMediaItem selectableItem)
        {
            Media.EnterWriteLock();
            try
            {
                int newIndex = CollectionsSort.insertIntoSortedCollection<SelectableMediaItem>(Media, selectableItem, SortFunc, 0, sortedItemEnd);

                sortedItemEnd++;

                return (newIndex);
            }
            finally
            {
                Media.ExitWriteLock();
            }

        }

        protected void clear()
        {
            bool selectionChanged = false;           

            Media.EnterWriteLock();
            try
            {
                foreach (SelectableMediaItem selectableItem in Media)
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
            finally
            {
                Media.ExitWriteLock();
            }

            if (selectionChanged)
            {
                OnSelectionChanged();
            }

            OnNrItemsInStateChanged(new MediaStateCollectionViewChangedEventArgs(MediaStateChangedAction.Clear));

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
        private void OnItemPropertyChanged(Object sender, PropertyChangedEventArgs args)
        {
            if (ItemPropertyChanged != null)
            {
                ItemPropertyChanged(sender, args);
            }
        }

        private void OnNrItemsInStateChanged(MediaStateCollectionViewChangedEventArgs args)
        {
            if (NrItemsInStateChanged != null)
            {
                NrItemsInStateChanged(this, args);
            }
        }

        private void OnItemResorted(Object item, int newIndex)
        {
            if (ItemResorted != null)
            {
                ItemResorted(item, newIndex);
            }
        }
               
    }

}
