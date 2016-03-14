using MediaViewer.Model.Concurrency;
using MediaViewer.Model.Media.Base.Item;
using MediaViewer.Model.Media.Base.Metadata;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.Base.Item
{
    class MediaItemMetadataLoader : BindableBase, IDisposable
    {
        List<MediaItem> queuedItems;
        int maxLoadingTasks;
        int nrLoadingTasks;
        CancellationTokenSource tokenSource = new CancellationTokenSource();

        public event EventHandler ItemFinishedLoading;

        public MediaItemMetadataLoader()
        {
            queuedItems = new List<MediaItem>();
            maxLoadingTasks = 5;
            nrLoadingTasks = 0;
            
            tokenSource = new CancellationTokenSource();

            Task.Factory.StartNew(() => loadLoop(), TaskCreationOptions.LongRunning);           
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool safe)
        {
            if (safe)
            {
                if (tokenSource != null)
                {
                    tokenSource.Dispose();
                    tokenSource = null;
                }
            }
        }
       
        public void addRange(IEnumerable<MediaItem> itemList)
        {
            Monitor.Enter(queuedItems);
            try
            {
                foreach (MediaItem item in itemList)
                {
                    queuedItems.Add(item);
                }
                Monitor.PulseAll(queuedItems);
            }
            finally
            {
                Monitor.Exit(queuedItems);
            }
        }

        public void add(MediaItem item)
        {
            Monitor.Enter(queuedItems);
            try
            {
                queuedItems.Add(item);
                Monitor.PulseAll(queuedItems);
            }
            finally
            {
                Monitor.Exit(queuedItems);
            }
        }

        public void remove(MediaItem item)
        {
            Monitor.Enter(queuedItems);
            try
            {
                queuedItems.Remove(item);
            }
            finally
            {
                Monitor.Exit(queuedItems);
            }
        }

        public void clear()
        {

            Monitor.Enter(queuedItems);
            try
            {
                queuedItems.Clear();
                tokenSource.Cancel();
                tokenSource = new CancellationTokenSource();               
            }
            finally
            {
                Monitor.Exit(queuedItems);
            }
            
        }

        void loadLoop()
        {
                        
            while (true)
            {
                Monitor.Enter(queuedItems);
                try
                {
                    while (queuedItems.Count == 0)
                    {
                        Monitor.Wait(queuedItems);
                    }

                    MediaItem item = queuedItems[0];
                    queuedItems.RemoveAt(0);

                    // don't load already loaded/loading items
                    if (item.ItemState == MediaItemState.LOADED ||
                        item.ItemState == MediaItemState.LOADING) continue;

                    // wait until we have a thread available to load the item
                    while (nrLoadingTasks == maxLoadingTasks)
                    {
                        Monitor.Wait(queuedItems);
                    }

                    nrLoadingTasks++;

                    Task.Factory.StartNew(new Action<Object>(loadItem), item, tokenSource.Token, TaskCreationOptions.None,PriorityScheduler.BelowNormal);
                }
                finally
                {
                    Monitor.Exit(queuedItems);
                }
                                                        
            }
                                
        }

        void loadItem(Object itemObj)
        {
            MediaItem item = itemObj as MediaItem;

            item.EnterUpgradeableReadLock();
            try
            {
                item.readMetadata_URLock(MetadataFactory.ReadOptions.AUTO |
                        MetadataFactory.ReadOptions.GENERATE_THUMBNAIL, tokenSource.Token);
            }
            finally
            {
                item.ExitUpgradeableReadLock();
            }

            bool isFinishedLoading = true;

            Monitor.Enter(queuedItems);
            try
            {
                nrLoadingTasks--;
               
                if (item.ItemState == MediaItemState.TIMED_OUT)
                {
                    // the item timed out, try loading it again later
                    queuedItems.Add(item);
                    isFinishedLoading = false;
                }
            }
            finally
            {
                Monitor.PulseAll(queuedItems);
                Monitor.Exit(queuedItems);

                if (isFinishedLoading)
                {
                    OnItemFinishedLoading(item);
                }
            }            
        }
     

        void OnItemFinishedLoading(MediaItem item)
        {
            if (ItemFinishedLoading != null)
            {
                ItemFinishedLoading(item, EventArgs.Empty);
            }
        }
    }
}
