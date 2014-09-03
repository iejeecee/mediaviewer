using MvvmFoundation.Wpf;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.MediaFileModel.Watcher
{
    class MediaFileItemLoader : ObservableObject
    {
        List<MediaFileItem> queuedItems;
        int maxLoadingTasks;
        int nrLoadingTasks;
        CancellationTokenSource tokenSource = new CancellationTokenSource();

        public event EventHandler ItemFinishedLoading;

        public MediaFileItemLoader()
        {
            queuedItems = new List<MediaFileItem>();
            maxLoadingTasks = 5;
            nrLoadingTasks = 0;
            
            tokenSource = new CancellationTokenSource();
           
        }
       
        public void addRange(IEnumerable<MediaFileItem> itemList)
        {
            Monitor.Enter(queuedItems);
            try
            {
                foreach (MediaFileItem item in itemList)
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

        public void add(MediaFileItem item)
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

        public void remove(MediaFileItem item)
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

                    MediaFileItem item = queuedItems[0];
                    queuedItems.RemoveAt(0);

                    // don't reload already loaded items
                    if (item.ItemState == MediaFileItemState.LOADED) continue;
              
                    // wait until we have a thread available to load the item
                    while(nrLoadingTasks == maxLoadingTasks)
                    {
                        Monitor.Wait(queuedItems);
                    }

                    nrLoadingTasks++;

                    Task.Factory.StartNew(() =>
                    {
                        item.readMetaData(MediaFactory.ReadOptions.AUTO |
                                MediaFactory.ReadOptions.GENERATE_THUMBNAIL, tokenSource.Token);

                    }).ContinueWith((result) =>
                    {                                                
                        Monitor.Enter(queuedItems);
                        nrLoadingTasks--;

                        if (item.ItemState == MediaFileItemState.TIMED_OUT)
                        {
                            // the item timed out, try loading it again later
                            queuedItems.Add(item);
                        }

                        Monitor.PulseAll(queuedItems);
                        Monitor.Exit(queuedItems);

                        if (ItemFinishedLoading != null && item.ItemState != MediaFileItemState.TIMED_OUT)
                        {
                            ItemFinishedLoading(item, EventArgs.Empty);
                        }

                    }).ConfigureAwait(false);
                }
            }
            finally
            {
                Monitor.Exit(queuedItems);
            }
        
        }
    }
}
