using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaViewer.MediaFileModel.Watcher
{
    class LockableObservableCollection<T> : ObservableCollection<T>
    {
        Object lockObject;

        public Object LockObject
        {
            get { return lockObject; }
            private set { lockObject = value; }
        }

        public LockableObservableCollection()
        {
            lockObject = new Object();

            BindingOperations.EnableCollectionSynchronization(this, lockObject);
        }
    }
}
