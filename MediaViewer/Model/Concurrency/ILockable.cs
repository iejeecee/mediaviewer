using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Concurrency
{
    public interface ILockable
    {
        void EnterWriteLock();
        void ExitWriteLock(bool fireQueuedEvents = true);
        void EnterReadLock();
        void ExitReadLock();
        void EnterUpgradeableReadLock();
        void ExitUpgradeableReadLock(bool fireQueuedEvents = true);

        void FireQueuedEvents();
    }
}
