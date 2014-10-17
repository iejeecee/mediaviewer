using MediaViewer.Model.Media.State;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaViewer.Model.Collections
{
    public class LockedObservableCollection<T> : ObservableCollection<T>, IDisposable where T : INotifyPropertyChanged 
    {
        public event EventHandler<PropertyChangedEventArgs> ItemPropertyChanged;

        protected ReaderWriterLockSlim rwLock;

        public LockedObservableCollection()
        {           
            rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);          

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

        protected override void ClearItems()
        {
            foreach (T item in this)
            {
                removeItemPropertyChangedListener(item);    
            }
            
            base.ClearItems();
          
        }

        protected override void RemoveItem(int index)
        {
            removeItemPropertyChangedListener(this[index]);  
           
            base.RemoveItem(index);            
        }

        protected override void InsertItem(int index, T newItem)
        {         
            if (Contains(newItem) == true)
            {
                throw new MediaStateException("Cannot add duplicate items to LockedCollection");
            }

            base.InsertItem(index,newItem);
               
            addItemPropertyChangedListener(newItem);                                          
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

        virtual protected void addItemPropertyChangedListener(T item)
        {
            item.PropertyChanged += item_PropertyChanged;
        }

        virtual protected void removeItemPropertyChangedListener(T item)
        {
            item.PropertyChanged -= item_PropertyChanged;
        }

    }
}
