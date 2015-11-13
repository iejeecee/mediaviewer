using MediaViewer.Model.Concurrency;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.metadata.Metadata;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.Base
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

        public void loadLoop()
        {
            Monitor.Enter(queuedItems);
            try
            {
                while (true)
                {
                    while(queuedItems.Count == 0)
                    {
                        Monitor.Wait(queuedItems);
                    }

                    MediaItem item = queuedItems[0];
                    queuedItems.RemoveAt(0);

                    // don't load already loaded/loading items
                    if (item.ItemState == MediaItemState.LOADED ||
                        item.ItemState == MediaItemState.LOADING) continue;
              
                    // wait until we have a thread available to load the item
                    while(nrLoadingTasks == maxLoadingTasks)
                    {
                        Monitor.Wait(queuedItems);
                    }

                    nrLoadingTasks++;

                    Task.Factory.StartNew(() =>
                    {
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

                    },tokenSource.Token,TaskCreationOptions.None, PriorityScheduler.BelowNormal).ContinueWith((result) =>
                    {                                                
                        Monitor.Enter(queuedItems);
                        nrLoadingTasks--;

                        if (item.ItemState == MediaItemState.TIMED_OUT)
                        {
                            // the item timed out, try loading it again later
                            queuedItems.Add(item);
                        }

                        Monitor.PulseAll(queuedItems);
                        Monitor.Exit(queuedItems);

                        if (isFinishedLoading(item))
                        {
                            OnItemFinishedLoading(item);
                        }

                    }).ConfigureAwait(false);
                }
            }
            finally
            {
                Monitor.Exit(queuedItems);
            }
        
        }

        bool isFinishedLoading(MediaItem item)
        {
            if (item.ItemState == MediaItemState.TIMED_OUT || item.ItemState == MediaItemState.LOADING)
            {
                return (false);
            }
            else
            {
                return (true);
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
