using MediaViewer.Filter;
using MediaViewer.Model.Collections;
using MediaViewer.Model.Collections.Sort;
using MediaViewer.Model.Concurrency;
using MediaViewer.Model.Media.Base.Item;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Utils;
using MediaViewer.UserControls.MediaGridItem;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections;
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
using System.Windows.Controls;
using System.Windows.Data;

namespace MediaViewer.Model.Media.Base.State.CollectionView
{
    /// <summary>
    /// Provides a sortable/filterable view on a MediaState bindable to the User Interface
    /// Functions marked with WLock or RLock need to be called while holding a write or read lock respectively
    /// </summary>
    public class MediaStateCollectionView : BindableBase, ILockable, IEnumerable<SelectableMediaItem>
    {

        ConcurrentQueue<Object> eventQueue;

        public event EventHandler<MediaStateCollectionViewChangedEventArgs> NrItemsInStateChanged;
        public event EventHandler<PropertyChangedEventArgs> ItemPropertyChanged;
        public event EventHandler<int[]> ItemResorted;
        public event EventHandler SelectionChanged;
        public event EventHandler Cleared;

        /// <summary>
        /// Only reason this is public is to make it bindable, 
        /// would be better to add INotifyCollectionChanged and make MediaStateCollectionView itself bindable
        /// </summary>
        public SelectableMediaLockedCollection MediaCollectionView { get; protected set; }

        public Guid Guid { get; private set; }
        public ListCollectionView FilterModes { get; set; }
        public ListCollectionView SortModes { get; set; }
        public InfoIconsCache InfoIconsCache { get; set; }
        public List<TagItem> TagFilter { get; set; }

        int sortedItemEnd;

        protected MediaStateCollectionView(MediaState mediaState = null)
        {
            eventQueue = new ConcurrentQueue<object>();

            MediaCollectionView = new SelectableMediaLockedCollection();
            this.MediaState = mediaState;

            sortedItemEnd = 0;

            SortFunc = new Func<SelectableMediaItem, SelectableMediaItem, int>((a, b) =>
            {
                return (a.ToString().CompareTo(b.ToString()));
            });

            Filter = new Func<SelectableMediaItem, bool>((a) =>
            {

                return (true);
            });

            SortDirection = ListSortDirection.Ascending;

            TagFilter = new List<TagItem>();

            Guid = Guid.NewGuid();
            
        }

        /// <summary>
        /// Number of items in the Media collection view, query under read lock
        /// </summary>
        public int Count
        {
            get
            {
                return (MediaCollectionView.Count);
            }
        }

        public bool IsAttached
        {
            get
            {
                return (MediaState != null);
            }
        }

        /// <summary>
        /// Returns index of Media item, query under read lock
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(SelectableMediaItem item)
        {
            return (MediaCollectionView.IndexOf(item));
        }

        private void MediaCollectionView_ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnItemPropertyChanged(sender, e);
        }

        MediaState mediaState;

        /// <summary>
        /// Source MediaState, set to null to de-attach 
        /// </summary>
        public MediaState MediaState
        {
            get
            {
                return (mediaState);
            }

            set
            {
                if (mediaState != null)
                {
                    mediaState.NrItemsInStateChanged -= new EventHandler<MediaStateChangedEventArgs>(MediaState_NrItemsInStateChanged);
                    mediaState.ItemPropertyChanged -= MediaState_ItemPropertyChanged;

                    MediaCollectionView.ItemPropertyChanged -= MediaCollectionView_ItemPropertyChanged;
                }

                SetProperty(ref mediaState, value);

                if (mediaState != null)
                {
                    mediaState.NrItemsInStateChanged += MediaState_NrItemsInStateChanged;
                    mediaState.ItemPropertyChanged += MediaState_ItemPropertyChanged;

                    MediaCollectionView.ItemPropertyChanged += MediaCollectionView_ItemPropertyChanged;

                    refresh();
                }
            }
        }

        Func<SelectableMediaItem, SelectableMediaItem, int> sortFunc, ascendingSortFunc, descendingSortFunc;

        public Func<SelectableMediaItem, SelectableMediaItem, int> SortFunc
        {
            get { return sortFunc; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("SortFunc");
                }

                ascendingSortFunc = value;
                descendingSortFunc = new Func<SelectableMediaItem, SelectableMediaItem, int>((a, b) =>
                    {
                        return (ascendingSortFunc(a, b) * -1);
                    });

                if (SortDirection == ListSortDirection.Ascending)
                {
                    sortFunc = ascendingSortFunc;
                }
                else
                {
                    sortFunc = descendingSortFunc;
                }
            }
        }

        ListSortDirection sortDirection;

        public ListSortDirection SortDirection
        {
            get { return sortDirection; }
            set
            {

                if (value == ListSortDirection.Ascending)
                {
                    sortFunc = ascendingSortFunc;
                }
                else
                {
                    sortFunc = descendingSortFunc;
                }

                SetProperty(ref sortDirection, value);
            }
        }

        Func<SelectableMediaItem, bool> filter;

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
            }
        }


        public int getSelectedItem(out MediaItem selectedItem)
        {
            EnterReadLock();
            try
            {
                return (getSelectedItem_RLock(out selectedItem));
            }
            finally
            {
                ExitReadLock();
            }
        }

        /// <summary>
        /// Obtain the first selected item, query under read lock 
        /// </summary>
        /// <param name="selectedItem"></param>
        /// <returns>Index of selected item or -1</returns>
        public int getSelectedItem_RLock(out MediaItem selectedItem)
        {
            for (int i = 0; i < MediaCollectionView.Count; i++)
            {
                if (MediaCollectionView[i].IsSelected)
                {
                    selectedItem = MediaCollectionView[i].Item;
                    return (i);
                }
            }

            selectedItem = null;
            return (-1);

        }

        public virtual Object getExtraInfo(SelectableMediaItem item)
        {
            return (null);
        }


        public List<MediaItem> getSelectedItems()
        {
            EnterReadLock();
            try
            {
                return getSelectedItems_RLock();
            }
            finally
            {
                ExitReadLock();
            }
        }

        public List<MediaItem> getSelectedItems_RLock()
        {
            List<MediaItem> selectedItems = new List<MediaItem>();

            foreach (SelectableMediaItem item in MediaCollectionView)
            {
                if (item.IsSelected)
                {
                    selectedItems.Add(item.Item);
                }
            }

            return (selectedItems);

        }

        protected virtual void MediaState_ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            MediaItem item = sender as MediaItem;

            reSort(item);

        }

        private void MediaState_NrItemsInStateChanged(object sender, MediaStateChangedEventArgs e)
        {
            switch (e.Action)
            {
                case MediaStateChangedAction.Add:
                    {
                        EnterWriteLock();
                        try
                        {
                            add_WLock(e.NewItems);
                        }
                        finally
                        {
                            ExitWriteLock();
                        }
                        break;
                    }
                case MediaStateChangedAction.Remove:
                    {
                        EnterWriteLock();
                        try
                        {
                            remove_WLock(e.OldItems);
                        }
                        finally
                        {
                            ExitWriteLock();
                        }
                        break;
                    }
                case MediaStateChangedAction.Modified:
                    break;
                case MediaStateChangedAction.Clear:
                    {
                        EnterWriteLock();
                        try
                        {
                            clear_WLock();
                            QueueClearedEvent();
                        }
                        finally
                        {
                            ExitWriteLock();
                        }
                        break;
                    }
                default:
                    break;
            }

        }

        public void refresh()
        {
            if (IsAttached == false) return;

            EnterWriteLock();
            MediaState.UIMediaCollection.EnterReadLock();
            try
            {
                clear_WLock();
                add_WLock(MediaState.UIMediaCollection);
            }
            finally
            {
                MediaState.UIMediaCollection.ExitReadLock();
                ExitWriteLock();
            }

        }

        public void selectAll()
        {
            EnterWriteLock();
            try
            {
                bool selectionChanged = false;

                foreach (SelectableMediaItem selectableItem in MediaCollectionView)
                {
                    selectableItem.SelectionChanged -= selectableItem_SelectionChanged;

                    if (selectableItem.IsSelected == false)
                    {
                        selectionChanged = true;
                    }

                    selectableItem.IsSelected = true;

                    selectableItem.SelectionChanged += selectableItem_SelectionChanged;
                }

                if (selectionChanged)
                {
                    QueueSelectionChangedEvent();
                }

            }
            finally
            {
                ExitWriteLock();
            }

        }

        public void selectExclusive(MediaItem item)
        {
            EnterWriteLock();
            try
            {
                selectExclusive_WLock(item);
            }
            finally
            {
                ExitWriteLock();
            }
        }

        public void selectExclusive_WLock(MediaItem item)
        {           
            bool selectionChanged = false;

            SelectableMediaItem selectableItem = new SelectableMediaItem(item);

            foreach (SelectableMediaItem media in MediaCollectionView)
            {
                if (media.IsSelected)
                {
                    if (!media.Equals(selectableItem))
                    {

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

            if (selectionChanged)
            {
                QueueSelectionChangedEvent();
            }
            
        }

        public void selectRange(MediaItem end)
        {
            EnterWriteLock();
            try
            {
                bool selectionChanged = false;

                MediaItem start = null;

                int startIndex = getSelectedItem_RLock(out start);
                int endIndex = MediaCollectionView.IndexOf(new SelectableMediaItem(end));

                if (endIndex == -1)
                {

                    return;

                }
                else if (startIndex == -1)
                {

                    startIndex = 0;
                }

                if (endIndex < startIndex)
                {

                    int temp = startIndex;
                    startIndex = endIndex;
                    endIndex = temp;
                }

                for (int i = startIndex; i <= endIndex; i++)
                {
                    MediaCollectionView[i].SelectionChanged -= selectableItem_SelectionChanged;

                    if (MediaCollectionView[i].IsSelected == false)
                    {
                        selectionChanged = true;
                    }

                    MediaCollectionView[i].IsSelected = true;

                    MediaCollectionView[i].SelectionChanged += selectableItem_SelectionChanged;
                }

                if (selectionChanged)
                {
                    QueueSelectionChangedEvent();
                }

            }
            finally
            {
                ExitWriteLock();
            }

        }

        public void deselectAll()
        {
            EnterWriteLock();
            try
            {
                bool selectionChanged = false;

                foreach (SelectableMediaItem selectableItem in MediaCollectionView)
                {
                    selectableItem.SelectionChanged -= selectableItem_SelectionChanged;

                    if (selectableItem.IsSelected == true)
                    {
                        selectionChanged = true;
                    }

                    selectableItem.IsSelected = false;

                    selectableItem.SelectionChanged += selectableItem_SelectionChanged;
                }

                if (selectionChanged)
                {
                    QueueSelectionChangedEvent();
                }

            }
            finally
            {
                ExitWriteLock();
            }
        }

        public void setIsSelected(MediaItem item, bool isSelected = true)
        {
            EnterWriteLock();
            try
            {
                SelectableMediaItem temp = new SelectableMediaItem(item);

                foreach (SelectableMediaItem selectableItem in MediaCollectionView)
                {
                    if (selectableItem.Equals(temp))
                    {
                        if (selectableItem.IsSelected != isSelected)
                        {
                            selectableItem.SelectionChanged -= selectableItem_SelectionChanged;
                            selectableItem.IsSelected = isSelected;
                            selectableItem.SelectionChanged += selectableItem_SelectionChanged;

                            QueueSelectionChangedEvent();

                            return;
                        }
                    }
                }

            }
            finally
            {
                ExitWriteLock();
            }

        }

        protected void remove_WLock(IEnumerable<MediaItem> items)
        {           
            foreach (MediaItem item in items)
            {
                remove_WLock(item);
            }            
        }

        protected void remove_WLock(MediaItem item)
        {
            SelectableMediaItem selectableItem = new SelectableMediaItem(item);
                  
            int index = MediaCollectionView.IndexOf(selectableItem);

            removeAt_WLock(index);                                   
        }

        protected void removeAt_WLock(int index)
        {        
            SelectableMediaItem selectableItem = null;

            if (index >= 0)
            {
                selectableItem = MediaCollectionView[index];

                if (selectableItem.IsSelected)
                {
                    QueueSelectionChangedEvent();
                }

                MediaCollectionView.RemoveAt(index);
                if (index < sortedItemEnd)
                {
                    sortedItemEnd -= 1;
                }

                selectableItem.SelectionChanged -= selectableItem_SelectionChanged;

                QueueNrItemsInStateChangedEvent(new MediaStateCollectionViewChangedEventArgs(MediaStateChangedAction.Remove, selectableItem));
            }
                                 
        }

        protected void reSort(MediaItem item)
        {
            int oldIndex = 0;
            int newIndex = 0;
        
            EnterWriteLock();
            try
            {
                SelectableMediaItem selectableItem = new SelectableMediaItem(item);

                oldIndex = MediaCollectionView.IndexOf(selectableItem);
                newIndex = oldIndex;

                if (oldIndex != -1)
                {
                    selectableItem = MediaCollectionView[oldIndex];

                    // item is already in the list
                    if (Filter(selectableItem))
                    {
                        // remove and reinsert item
                        MediaCollectionView.RemoveAt(oldIndex);
                        if (oldIndex < sortedItemEnd)
                        {
                            sortedItemEnd -= 1;
                        }

                        newIndex = insertSorted_WLock(selectableItem);

                        if (newIndex != oldIndex)
                        {
                            QueueItemResortedEvent(item, oldIndex, newIndex);
                        }

                    }
                    else
                    {
                        // remove item from list
                        removeAt_WLock(oldIndex);
                    }
                }
                else
                {
                    // add item to list                    
                    add_WLock(item);
                }

            }
            finally
            {
                ExitWriteLock();
            }

        }

        protected void add_WLock(IEnumerable<MediaItem> items)
        {
            
            // Use a fast(er) path if we are just adding a batch of items to a empty list
            // instead of firing off a whole bunch of itemchanged events
            if (this.MediaCollectionView.Count == 0 && items.Count() > 1)
            {
                List<SelectableMediaItem> addedItems = new List<SelectableMediaItem>();

                foreach (MediaItem item in items)
                {
                    SelectableMediaItem selectableItem = new SelectableMediaItem(item);

                    if (Filter(selectableItem))
                    {
                        selectableItem.SelectionChanged += selectableItem_SelectionChanged;

                        if (selectableItem.Item.Metadata != null)
                        {
                            CollectionsSort.insertIntoSortedCollection(addedItems, selectableItem, sortFunc, 0, sortedItemEnd);
                            sortedItemEnd++;
                        }
                        else
                        {
                            addedItems.Add(selectableItem);
                        }
                    }

                }

                MediaCollectionView.AddRange(addedItems);

                QueueNrItemsInStateChangedEvent(new MediaStateCollectionViewChangedEventArgs(MediaStateChangedAction.Add, addedItems));
                // this is incorrect
                //sortedItemEnd = Media.Count;

            }
            else
            {
                foreach (MediaItem item in items)
                {                  
                    add_WLock(item);                    
                }
            }
           
        }

        protected void add_WLock(MediaItem item)
        {      
            SelectableMediaItem selectableItem = new SelectableMediaItem(item);

            if (Filter(selectableItem) && !MediaCollectionView.Contains(selectableItem))
            {
                if (item.Metadata != null)
                {
                    insertSorted_WLock(selectableItem);
                }
                else
                {
                    MediaCollectionView.Add(selectableItem);
                }

                QueueNrItemsInStateChangedEvent(new MediaStateCollectionViewChangedEventArgs(MediaStateChangedAction.Add, selectableItem));
            }

            selectableItem.SelectionChanged += selectableItem_SelectionChanged;
           
        }

        int insertSorted_WLock(SelectableMediaItem selectableItem)
        {
            int newIndex = CollectionsSort.insertIntoSortedCollection<SelectableMediaItem>(MediaCollectionView, selectableItem, SortFunc, 0, sortedItemEnd);

            sortedItemEnd++;

            return (newIndex);
        }

        protected void clear_WLock()
        {
            bool selectionChanged = false;

            foreach (SelectableMediaItem selectableItem in MediaCollectionView)
            {
                selectableItem.SelectionChanged -= selectableItem_SelectionChanged;

                if (selectableItem.IsSelected)
                {
                    selectionChanged = true;
                }
            }

            MediaCollectionView.Clear();
            sortedItemEnd = 0;

            if (selectionChanged)
            {
                QueueSelectionChangedEvent();
            }

            QueueNrItemsInStateChangedEvent(new MediaStateCollectionViewChangedEventArgs(MediaStateChangedAction.Clear));
            
        }

        protected bool tagFilter(SelectableMediaItem item)
        {
            MediaItem media = item.Item;

            if (TagFilter.Count == 0) return (true);
            if (media.ItemState == Base.Item.MediaItemState.LOADING) return (true);
            if (media.Metadata == null) return (false);

            foreach (TagItem tagItem in TagFilter)
            {
                bool containsTag = media.Metadata.Tags.Contains(tagItem.Tag);

                if (tagItem.IsIncluded && !containsTag)
                {
                    return (false);
                }
                else if (tagItem.IsExcluded && containsTag)
                {
                    return (false);
                }
            }

            return (true);
        }

        private void selectableItem_SelectionChanged(object sender, EventArgs e)
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

        protected void QueueClearedEvent()
        {
            eventQueue.Enqueue("Cleared");
        }

        protected virtual void QueueSelectionChangedEvent()
        {
            eventQueue.Enqueue("SelectionChanged");
        }
        
        protected virtual void QueueNrItemsInStateChangedEvent(MediaStateCollectionViewChangedEventArgs args)
        {
            eventQueue.Enqueue(args);
        }

        protected virtual void QueueItemResortedEvent(Object item, int oldIndex, int newIndex)
        {
            eventQueue.Enqueue(new int[] { oldIndex, newIndex });
        }

        public void EnterWriteLock()
        {
            MediaCollectionView.EnterWriteLock();
        }

        public void ExitWriteLock(bool fireQueuedEvents = true)
        {
            MediaCollectionView.ExitWriteLock(fireQueuedEvents);
            if (fireQueuedEvents)
            {
                FireQueuedEvents();
            }
        }

        public void EnterReadLock()
        {
            MediaCollectionView.EnterReadLock();
        }

        public void ExitReadLock()
        {
            MediaCollectionView.ExitReadLock();
        }

        public void EnterUpgradeableReadLock()
        {
            MediaCollectionView.EnterUpgradeableReadLock();
        }

        public void ExitUpgradeableReadLock(bool fireQueuedEvents = true)
        {
            MediaCollectionView.ExitUpgradeableReadLock(fireQueuedEvents);
            if (fireQueuedEvents)
            {
                FireQueuedEvents();
            }
        }

        public void FireQueuedEvents()
        {
            Object args;

            while (eventQueue.TryDequeue(out args))
            {
                if (args is String)
                {
                    String eventName = args as String;

                    if (eventName.Equals("Cleared"))
                    {
                        if (Cleared != null)
                        {
                            Cleared(this, EventArgs.Empty);
                        }
                    }
                    else if (eventName.Equals("SelectionChanged"))
                    {
                        if (SelectionChanged != null)
                        {
                            SelectionChanged(this, EventArgs.Empty);
                        }
                    }

                }
                else if (args is MediaStateCollectionViewChangedEventArgs)
                {
                    if (NrItemsInStateChanged != null)
                    {
                        NrItemsInStateChanged(this, args as MediaStateCollectionViewChangedEventArgs);
                    }
                }
                else if (args is int[])
                {
                    if (ItemResorted != null)
                    {
                        ItemResorted(this, args as int[]);
                    }

                }
            }
        }

        public IEnumerator<SelectableMediaItem> GetEnumerator()
        {
            return (MediaCollectionView.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (MediaCollectionView.GetEnumerator());
        }
    }

}
