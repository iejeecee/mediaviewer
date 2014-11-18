using MediaViewer.Model.Collections;
using MediaViewer.Model.Collections.Sort;
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

        public event EventHandler<MediaStateChangedEventArgs> NrItemsInStateChanged;
        public event EventHandler<PropertyChangedEventArgs> ItemPropertyChanged;
        public event EventHandler<int> ItemResorted;
        public event EventHandler SelectionChanged;
        public event EventHandler Cleared;
      
        int sortedItemEnd;

        public MediaStateCollectionView(IMediaState mediaState)
        {                      
            MediaState = mediaState;

            Media = new SelectableMediaLockedCollection();                      
            Media.ItemPropertyChanged += Media_ItemPropertyChanged;          
         
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

        public int getSelectedItem(out MediaFileItem selectedItem)
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

        public List<MediaFileItem> getSelectedItems()
        {
            List<MediaFileItem> selectedItems = new List<MediaFileItem>();

            Media.EnterReaderLock();
            try
            {
                foreach (SelectableMediaFileItem item in Media)
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
            MediaFileItem item = sender as MediaFileItem;

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

        protected void reSort(MediaFileItem item)
        {

            int index = 0;
            int newIndex = 0;

            Media.EnterWriteLock();
            try
            {
                SelectableMediaFileItem selectableItem = new SelectableMediaFileItem(item);

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
            finally
            {
                Media.ExitWriteLock();
            }

            if (selectionChanged)
            {
                OnSelectionChanged();
            }
        }

        public void selectRange(MediaFileItem end)
        {
            bool selectionChanged = false;

            Media.EnterWriteLock();
            try
            {
                MediaFileItem start = null;

                int startIndex = getSelectedItem(out start);
                int endIndex = Media.IndexOf(new SelectableMediaFileItem(end));
                
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
            finally
            {
                Media.ExitWriteLock();
            }

            if (selectionChanged)
            {
                OnSelectionChanged();
            }
        }
        
        public void setIsSelected(MediaFileItem item, bool isSelected = true)
        {
           
            SelectableMediaFileItem temp = new SelectableMediaFileItem(item);

            Media.EnterWriteLock();
            try
            {

                foreach (SelectableMediaFileItem selectableItem in Media)
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

        protected void remove(IEnumerable<MediaFileItem> items)
        {
            Media.EnterWriteLock();
            try
            {
                foreach (MediaFileItem item in items)
                {
                    remove(item);
                }
            }
            finally
            {
                Media.ExitWriteLock();
            }
        }

        protected void remove(MediaFileItem item)
        {
            bool selectionChanged = false;
            bool isItemRemoved = false;

            Media.EnterWriteLock();
            try
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
            finally
            {
                Media.ExitWriteLock();
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

        protected void add(IEnumerable<MediaFileItem> items)
        {
            Media.EnterWriteLock();
            try
            {
                // Use a fast(er) path if we are just adding a batch of items to a empty list
                // instead of firing off a whole bunch of itemchanged events
                if (this.Media.Count == 0 && items.Count() > 1)
                {                   
                    List<SelectableMediaFileItem> temp = new List<SelectableMediaFileItem>();
                    List<MediaFileItem> filteredItems = new List<MediaFileItem>();

                    foreach (MediaFileItem item in items)
                    {
                        SelectableMediaFileItem selectableItem = new SelectableMediaFileItem(item);                        
                       
                        if (Filter(selectableItem))
                        {
                            selectableItem.SelectionChanged += selectableItem_SelectionChanged;
                            CollectionsSort.insertIntoSortedCollection(temp, selectableItem, sortFunc);

                            filteredItems.Add(item);
                        }
                    }

                    Media.AddRange(temp);
                    sortedItemEnd = Media.Count;

                    OnNrItemsInStateChanged(new MediaStateChangedEventArgs(MediaStateChangedAction.Add, filteredItems));
                }
                else
                {
                    foreach (MediaFileItem item in items)
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

        protected void add(MediaFileItem item)
        {
            bool isItemAdded = false;

            Media.EnterWriteLock();
            try
            {
                SelectableMediaFileItem selectableItem = new SelectableMediaFileItem(item);

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
                OnNrItemsInStateChanged(new MediaStateChangedEventArgs(MediaStateChangedAction.Add, item));
            }

        }
        
        int insertSorted(SelectableMediaFileItem selectableItem)
        {
            Media.EnterWriteLock();
            try
            {
                int newIndex = CollectionsSort.insertIntoSortedCollection<SelectableMediaFileItem>(Media, selectableItem, SortFunc, 0, sortedItemEnd);

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
            finally
            {
                Media.ExitWriteLock();
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
        private void OnItemPropertyChanged(Object sender, PropertyChangedEventArgs args)
        {
            if (ItemPropertyChanged != null)
            {
                ItemPropertyChanged(sender, args);
            }
        }

        private void OnNrItemsInStateChanged(MediaStateChangedEventArgs args)
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
