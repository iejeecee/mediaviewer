using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.MediaFileModel.Watcher
{
    /// <summary>
    /// Concurrent Collection that supports multiple readers and a single writer
    /// Also makes sure the items in the collection are unique
    /// </summary>
    /// <typeparam name="MediaFileItem"></typeparam>
    public class MediaLockedCollection
    {

        // items in the current User Interface
        List<MediaFileItem> items;
     
        public ReadOnlyCollection<MediaFileItem> Items
        {
            get { return items.AsReadOnly(); }

        }

        protected ReaderWriterLockSlim rwLock;

        public MediaLockedCollection()
        {
            rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            items = new List<MediaFileItem>();
        }

        public void EnterReaderLock()
        {
            rwLock.EnterReadLock();
        }

        public void ExitReaderLock()
        {
            rwLock.ExitReadLock();
        }

        public void EnterWriteLock()
        {
            rwLock.EnterWriteLock();
        }

        public void ExitWriteLock()
        {
            rwLock.ExitWriteLock();
        }

        public int Count
        {
            get
            {
                rwLock.EnterReadLock();
                try
                {
                    return (items.Count);
                }
                finally
                {
                    rwLock.ExitReadLock();
                }
            }
        }
        virtual public bool Add(MediaFileItem newItem)
        {
            rwLock.EnterWriteLock();

            try
            {

                if (Contains(newItem) == true)
                {
                    return (false);
                }

                items.Add(newItem);
                items.Sort();
                return (true);

            }
            finally
            {
                rwLock.ExitWriteLock();
            }

        }
        /// <summary>
        /// Returns true when all items are added.
        /// Returns false when one or more newItems already exist in the list,
        /// none of the newItems will be added in this case
        /// </summary>
        /// <param name="newItems"></param>
        /// <returns></returns>
        virtual public bool AddRange(IEnumerable<MediaFileItem> newItems)
        {
            rwLock.EnterWriteLock();

            try {

                if (Contains(newItems) == true)
                {                    
                    return (false);
                }

                items.AddRange(newItems);
                items.Sort();
                return (true);
                        
            }
            finally
            {
                rwLock.ExitWriteLock();
            }

        }
        /// <summary>
        /// Remove all elements from the collection
        /// </summary>
        virtual public void Clear()
        {
            rwLock.EnterWriteLock();

            try
            {
                items.Clear();             
            }
            finally
            {
                rwLock.ExitWriteLock();
            }

        }

        virtual public void Remove(MediaFileItem removeItem)
        {
            rwLock.EnterWriteLock();
            try
            {             
                foreach (MediaFileItem item in items)
                {
                    if (removeItem.Equals(item))
                    {
                        items.Remove(item);
                        break;
                    }

                }
                
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Remove all elements contained in removeItems from the collection.   
        /// Returns the actually removed items
        /// </summary>
        /// <param name="removeItems"></param>
        virtual public List<MediaFileItem> RemoveAll(IEnumerable<MediaFileItem> removeItems)
        {
            List<MediaFileItem> removed = new List<MediaFileItem>();

            rwLock.EnterWriteLock();
            try
            {                     
                foreach (MediaFileItem removeItem in removeItems)
                {
                    foreach (MediaFileItem item in items)
                    {
                        if (removeItem.Equals(item))
                        {                            
                            items.Remove(item);
                            removed.Add(item);
                            break;
                        }

                    }

                }

                return (removed);
            }
            finally
            {               
                rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Remove oldItems and add newItems in a sequential order and a single operation
        /// For example:
        /// oldItem[0] is removed
        /// newItem[0] is added
        /// oldItem[1] is removed
        /// newItem[1] is added.... etc
        /// oldItems and newItems do not have to be of the same size
        /// </summary>
        /// <param name="oldItems"></param>
        /// <param name="newItems"></param>
        virtual public void ReplaceAll(IEnumerable<MediaFileItem> oldItems, IEnumerable<MediaFileItem> newItems)
        {

            rwLock.EnterWriteLock();
            try
            {
                int nrOldItems = oldItems.Count();
                int nrNewItems = newItems.Count();

                int iterations = Math.Max(nrOldItems, nrNewItems);

                for (int i = 0; i < iterations; i++)
                {
                    if (i < nrOldItems)
                    {
                        MediaFileItem oldItem = Find(oldItems.ElementAt(i).Location);
                        if (!object.Equals(oldItem, default(MediaFileItem)))
                        {
                            items.Remove(oldItem);
                        }

                    }

                    if (i < nrNewItems)
                    {
                        MediaFileItem newItem = Find(newItems.ElementAt(i).Location);
                        if (object.Equals(newItem, default(MediaFileItem)))
                        {
                            items.Add(newItems.ElementAt(i));
                        }
                    }

                }

                items.Sort();
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public virtual MediaFileItem Find(String location)
        {
            rwLock.EnterReadLock();
            try
            {
                foreach (MediaFileItem item in items)
                {
                    if (item.Location.Equals(location))
                    {
                        return (item);
                    }
                }

                return null;
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }

        public virtual bool Contains(MediaFileItem compareItem)
        {
            rwLock.EnterReadLock();

            try
            {
                foreach (MediaFileItem item in items)
                {
                    if (item.Equals(compareItem))
                    {
                        return (true);
                    }

                }

                return (false);
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }

        public virtual bool Contains(IEnumerable<MediaFileItem> compareItems)
        {
            rwLock.EnterWriteLock();
            try
            {
                foreach (MediaFileItem compareItem in compareItems)
                {
                    foreach (MediaFileItem item in items)
                    {
                        if (compareItem.Equals(item))
                        {
                            return (true);                            
                        }

                    }

                }

                return (false);
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }


        public bool RenameRange(IEnumerable<MediaFileItem> oldItems, IEnumerable<MediaFileItem> newItems)
        {
           
            rwLock.EnterWriteLock();
            try
            {
                bool success = true;

                int nrOldItems = oldItems.Count();
                int nrNewItems = newItems.Count();

                Debug.Assert(nrOldItems == nrNewItems);
              
                for (int i = 0; i < nrOldItems; i++)
                {                   
                    MediaFileItem oldItem = Find(oldItems.ElementAt(i).Location);
                    if (oldItem == null)
                    {
                        success = false;
                        continue;
                    }

                    oldItem.Location = newItems.ElementAt(i).Location;                       
                }

                items.Sort();
                return (success);
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }
    }
}

