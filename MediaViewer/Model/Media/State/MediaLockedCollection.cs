using MediaViewer.Model.Collections;
using MediaViewer.Model.Media.File;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.State
{

    /// <summary>
    /// Concurrent Collection that supports multiple readers and a single writer
    /// Also makes sure the items in the collection are unique
    /// </summary>
    /// <typeparam name="MediaFileItem"></typeparam>
    public class MediaLockedCollection : LockedObservableCollection<MediaFileItem>
    {               
        MediaFileItemLoader itemLoader;
                              
        public MediaLockedCollection(bool autoLoadItems = false)
        {                             
            if (autoLoadItems == true)
            {
                itemLoader = new MediaFileItemLoader();
                Task.Factory.StartNew(() => itemLoader.loadLoop());

                itemLoader.ItemFinishedLoading += itemLoader_ItemFinishedLoading;
            }

            this.autoLoadItems = autoLoadItems;
            IsLoading = false;
            
        }

        void itemLoader_ItemFinishedLoading(object sender, EventArgs e)
        {
            rwLock.EnterWriteLock();
            try
            {
                // check to make sure the loaded item is actually in the current state
                if(Contains(sender as MediaFileItem)) {
                    NrLoadedItems++;

                    if (NrLoadedItems == Count)
                    {
                        IsLoading = false;
                    }
                    else
                    {
                        IsLoading = true;
                    }
                }                             
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
           
        }

        bool isLoading;

        public bool IsLoading
        {
            get { return isLoading; }
            set { isLoading = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsLoading"));           
            }
        }

        int nrLoadedItems;

        public int NrLoadedItems
        {
            get { return nrLoadedItems; }
            protected set { nrLoadedItems = value;
                OnPropertyChanged(new PropertyChangedEventArgs("NrLoadedItems"));           
            }
        }
        
        bool autoLoadItems;

        public bool AutoLoadItems
        {
            get { return autoLoadItems; }           
        }
          
        override protected void InsertItem(int index, MediaFileItem newItem)
        {
            base.InsertItem(index, newItem);

            if (AutoLoadItems)
            {
                if (newItem.ItemState == MediaFileItemState.LOADED)
                {
                    NrLoadedItems++;
                }
                else
                {
                    itemLoader.add(newItem);
                }
            }
                          
        }
               
        /// <summary>
        /// Remove all elements from the collection
        /// </summary>
        override protected void ClearItems()
        {        
            base.ClearItems();

            if (AutoLoadItems)
            {
                NrLoadedItems = 0;
                itemLoader.clear();
            }                
        }

        protected override void RemoveItem(int index)
        {                                     
            if (AutoLoadItems)
            {
                if (this[index].ItemState != MediaFileItemState.LOADING)
                {
                    NrLoadedItems--;
                }
                itemLoader.remove(this[index]);
            }

            base.RemoveItem(index);
                    
        }

        public void AddRange(IEnumerable<MediaFileItem> addItems)
        {
            foreach (MediaFileItem item in addItems)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Remove all elements contained in removeItems from the collection.   
        /// Returns the actually removed items
        /// </summary>
        /// <param name="removeItems"></param>
        public List<MediaFileItem> RemoveRange(IEnumerable<MediaFileItem> removeItems)
        {
            List<MediaFileItem> removed = new List<MediaFileItem>();
                                   
            foreach (MediaFileItem removeItem in removeItems)
            {
                if(Remove(removeItem)) {
                    removed.Add(removeItem);
                }
            }
                             
            return (removed);           
        }
       
        public bool RenameRange(IEnumerable<MediaFileItem> oldItems, IEnumerable<String> newLocations)
        {                      
            bool success = true;

            int nrOldItems = oldItems.Count();
            int nrNewItems = newLocations.Count();

            Debug.Assert(nrOldItems == nrNewItems);
              
            for (int i = 0; i < nrOldItems; i++)
            {                   
                MediaFileItem oldItem = this.FirstOrDefault((elem) => {
                    return(oldItems.ElementAt(i).Location.Equals(elem.Location));
                });

                if (oldItem == null)
                {
                    success = false;
                    continue;
                }

                oldItem.Location = newLocations.ElementAt(i);                       
            }
            
            return (success);
           
        }
                               
    }
}

