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
    /// <typeparam name="T"></typeparam>
    class ReaderWriterLockedCollection<T> where T : System.IEquatable<T>
    {
        protected List<T> items;
     
        public ReadOnlyCollection<T> Items
        {
            get { return items.AsReadOnly(); }

        }

        protected ReaderWriterLockSlim rwLock;

        public ReaderWriterLockedCollection()
        {
            rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            items = new List<T>();
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
        /// <summary>
        /// Returns true when all items are added.
        /// Returns false when one or more newItems already exist in the list,
        /// none of the newItems will be added in this case
        /// </summary>
        /// <param name="newItems"></param>
        /// <returns></returns>
        virtual public bool AddRange(IEnumerable<T> newItems)
        {
            rwLock.EnterWriteLock();

            try {

                if (Contains(newItems) == true)
                {                    
                    return (false);
                }

                items.AddRange(newItems);
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
        /// <summary>
        /// Remove all elements contained in removeItems from the collection     
        /// </summary>
        /// <param name="removeItems"></param>
        virtual public void RemoveAll(IEnumerable<T> removeItems)
        {

            rwLock.EnterWriteLock();
            try
            {                     
                foreach (T removeItem in removeItems)
                {
                    foreach (T item in items)
                    {
                        if (removeItem.Equals(item))
                        {                            
                            items.Remove(item);
                            break;
                        }

                    }

                }
              
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
        virtual public void ReplaceAll(IEnumerable<T> oldItems, IEnumerable<T> newItems)
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
                        T oldItem = Find(oldItems.ElementAt(i));
                        if (!object.Equals(oldItem, default(T)))
                        {
                            items.Remove(oldItem);
                        }

                    }

                    if (i < nrNewItems)
                    {
                        T newItem = Find(newItems.ElementAt(i));
                        if (object.Equals(newItem, default(T)))
                        {
                            items.Add(newItems.ElementAt(i));
                        }
                    }

                }                                         

            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public virtual T Find(T findItem)
        {
            rwLock.EnterReadLock();
            try
            {
                foreach (T item in items)
                {
                    if (item.Equals(findItem))
                    {
                        return (item);
                    }
                }

                return default(T);
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }

        public virtual bool Contains(T compareItem)
        {
            rwLock.EnterReadLock();

            try
            {
                foreach (T item in items)
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

        public virtual bool Contains(IEnumerable<T> compareItems)
        {
            rwLock.EnterWriteLock();
            try
            {
                foreach (T compareItem in compareItems)
                {
                    foreach (T item in items)
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

    }
}

