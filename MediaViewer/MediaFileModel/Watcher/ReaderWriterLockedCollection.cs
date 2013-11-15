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

        virtual public bool ReplaceAll(IEnumerable<T> oldItems, IEnumerable<T> newItems)
        {

            rwLock.EnterWriteLock();
            try
            {
                if (Contains(newItems) == true)
                {
                    return (false);
                }

                foreach (T oldItem in oldItems)
                {
                    foreach (T item in items)
                    {
                        if (oldItem.Equals(item))
                        {
                            items.Remove(item);
                            break;
                        }

                    }

                }

                items.AddRange(newItems);
                return (true);

            }
            finally
            {
                rwLock.ExitWriteLock();
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

