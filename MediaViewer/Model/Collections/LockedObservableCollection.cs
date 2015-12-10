using MediaViewer.Model.Concurrency;
using MediaViewer.Model.Media.Base.State;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaViewer.Model.Collections
{
    public class LockedObservableCollection<T> : ObservableCollection<T>, ILockable, IDisposable where T : INotifyPropertyChanged
    {        
        public event EventHandler<PropertyChangedEventArgs> ItemPropertyChanged;

        protected ConcurrentQueue<EventArgs> eventQueue;
        protected ReaderWriterLockSlim rwLock;

        public LockedObservableCollection()
        {
            eventQueue = new ConcurrentQueue<EventArgs>();
            rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            //rwLock = new ReaderWriterLockSlim();  

            //http://stackoverflow.com/questions/2091988/how-do-i-update-an-observablecollection-via-a-worker-thread
            BindingOperations.EnableCollectionSynchronization(this, rwLock, new CollectionSynchronizationCallback(lockCollection));
        }

        private void lockCollection(IEnumerable collection, object context, Action accessMethod, bool writeAccess)
        {
            ReaderWriterLockSlim lockObject = (ReaderWriterLockSlim)context;

            if (writeAccess)
            {
                lockObject.EnterWriteLock();
                try
                {
                    accessMethod();
                }
                finally
                {
                    lockObject.ExitWriteLock();
                }
            }
            else
            {
                lockObject.EnterReadLock();
                try
                {
                    accessMethod();
                }
                finally
                {
                    lockObject.ExitReadLock();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);            
        }

        protected virtual void Dispose(bool safe)
        {
            if (safe)
            {
                if (rwLock != null)
                {
                    rwLock.Dispose();
                    rwLock = null;
                }
            }
        }

       public void EnterReadLock()
        {
            rwLock.EnterReadLock();
        }

        public void ExitReadLock()
        {
            rwLock.ExitReadLock();
        }

        public void EnterWriteLock()
        {
            rwLock.EnterWriteLock();            
        }

        public void ExitWriteLock(bool fireQueuedEvents = true)
        {            
            rwLock.ExitWriteLock();
            if (fireQueuedEvents)
            {
                FireQueuedEvents();
            }
            
        }

        public void EnterUpgradeableReadLock()
        {
            rwLock.EnterUpgradeableReadLock();
        }

        public void ExitUpgradeableReadLock(bool fireQueuedEvents = true)
        {
            rwLock.ExitUpgradeableReadLock();
            if (fireQueuedEvents)
            {
                FireQueuedEvents();
            }
        }
     
        public virtual void AddRange(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");

            if (collection.Count() == 1)
            {
                Add(collection.First());
                return;
            }

            foreach (T item in collection)
            {
                Items.Add(item);
                afterItemAdded(item);    
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

   
        public virtual List<T> RemoveRange(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");
           
            List<T> removed = new List<T>();

            if (collection.Count() == 1)
            {
                if (Remove(collection.First()) == true)
                {
                    removed.Add(collection.First());                   
                }

                return (removed);
            }

            foreach (T item in collection)
            {
                if (Items.Remove(item))
                {
                    beforeItemRemoved(item);
                    removed.Add(item);
                }

                
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            return (removed);
        }

        protected override void ClearItems()
        {
            foreach (T item in this)
            {
                beforeItemRemoved(item);    
            }
                       
            base.ClearItems();
          
        }

        protected override void RemoveItem(int index)
        {
            beforeItemRemoved(this[index]);
           
            base.RemoveItem(index);            
        }

        protected override void InsertItem(int index, T newItem)
        {         
            if (Contains(newItem) == true)
            {
                throw new MediaStateException("Cannot add duplicate items to LockedCollection");
            }
            
            base.InsertItem(index,newItem);
               
            afterItemAdded(newItem);                                          
        }

        protected override void SetItem(int index, T item)
        {            
            base.SetItem(index, item);
        }

        protected void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ItemPropertyChanged != null)
            {
                ItemPropertyChanged(sender, e);
            }
        }

        virtual protected void afterItemAdded(T item)
        {
            item.PropertyChanged += item_PropertyChanged;
        }

        virtual protected void beforeItemRemoved(T item)
        {
            item.PropertyChanged -= item_PropertyChanged;
        }

        virtual public void FireQueuedEvents()
        {
            EventArgs eventArgs;

            while (eventQueue.TryDequeue(out eventArgs))
            {
                if (eventArgs is NotifyCollectionChangedEventArgs)
                {
                    base.OnCollectionChanged(eventArgs as NotifyCollectionChangedEventArgs);
                }
                else if (eventArgs is PropertyChangedEventArgs)
                {
                    base.OnPropertyChanged(eventArgs as PropertyChangedEventArgs);
                }
            }      

        }

        /*protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            eventQueue.Enqueue(e);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            eventQueue.Enqueue(e);            
        }*/
    }
}
