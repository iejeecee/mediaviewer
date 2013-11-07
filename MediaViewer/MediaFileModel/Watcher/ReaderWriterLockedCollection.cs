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
                return (items.Count);
            }
        }

        virtual public void AddRange(IEnumerable<T> newItems)
        {
            rwLock.EnterWriteLock();

            try {
                         
                items.AddRange(newItems);
                        
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

        virtual bool Contains(T item)
        {
            rwLock.EnterReadLock();

            try
            {

                foreach (T i in items)
                {
                    if (item.Equals(i))
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

    }
}

