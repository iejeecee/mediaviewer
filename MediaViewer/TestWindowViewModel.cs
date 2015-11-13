using MediaViewer.Model.Mvvm;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaViewer
{
    class TestWindowViewModel : BindableBase
    {
        public Command AddCommand { get; set; }
        public Command ClearCommand { get; set; }
        public ObservableCollection<int> Observable { get; set; }
        ReaderWriterLockSlim RWLock { get; set; }

        public TestWindowViewModel()
        {
            RWLock = new ReaderWriterLockSlim();
            Observable = new ObservableCollection<int>();
            BindingOperations.EnableCollectionSynchronization(Observable, RWLock, new CollectionSynchronizationCallback(lockCollection));

            AddCommand = new Command(() =>
            {
                Task.Factory.StartNew(() =>
                {
                    RWLock.EnterWriteLock();
                    for (int i = 0; i < 10; i++)
                    {
                        Observable.Add(i);
                    }
                    RWLock.ExitWriteLock();
                });
            });

            ClearCommand = new Command(() =>
                {
                    Task.Factory.StartNew(() =>
                {
                    RWLock.EnterWriteLock();
                    Observable.Move(0,5);
                    RWLock.ExitWriteLock();

                    int k = 0;

                    for (int i = 0; i < 1000; i++)
                    {
                        k++;
                    }
                });
                });
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
    }
}
