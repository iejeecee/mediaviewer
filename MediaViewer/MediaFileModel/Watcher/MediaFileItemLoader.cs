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
        List<MediaFileItem> items;
        int maxLoadingTasks;
        int nrLoadingTasks;
        CancellationTokenSource tokenSource = new CancellationTokenSource();

        public event EventHandler ItemFinishedLoading;

        public MediaFileItemLoader()
        {
            items = new List<MediaFileItem>();
            maxLoadingTasks = 5;
            nrLoadingTasks = 0;
            
            tokenSource = new CancellationTokenSource();
           
        }
       
        public void addRange(IEnumerable<MediaFileItem> itemList)
        {
            Monitor.Enter(items);
            try
            {
                foreach (MediaFileItem item in itemList)
                {
                    items.Add(item);
                }
                Monitor.PulseAll(items);
            }
            finally
            {
                Monitor.Exit(items);
            }
        }

        public void add(MediaFileItem item)
        {
            Monitor.Enter(items);
            try
            {
                items.Add(item);
                Monitor.PulseAll(items);
            }
            finally
            {
                Monitor.Exit(items);
            }
        }

        public void remove(MediaFileItem item)
        {
            Monitor.Enter(items);
            try
            {
                items.Remove(item);
            }
            finally
            {
                Monitor.Exit(items);
            }
        }

        public void clear()
        {

            Monitor.Enter(items);
            try
            {
                items.Clear();
                tokenSource.Cancel();
                tokenSource = new CancellationTokenSource();               
            }
            finally
            {
                Monitor.Exit(items);
            }
            
        }

        public void loadLoop()
        {
            Monitor.Enter(items);
            try
            {
                while (true)
                {
                    while(items.Count == 0)
                    {
                        Monitor.Wait(items);
                    }

                    MediaFileItem item = items[0];
                    items.RemoveAt(0);

                    // don't reload already loaded items
                    if (item.ItemState == MediaFileItemState.LOADED) continue;
              
                    while(nrLoadingTasks == maxLoadingTasks)
                    {
                        Monitor.Wait(items);
                    }

                    nrLoadingTasks++;

                    Task.Factory.StartNew(() =>
                    {
                        item.readMetaData(MediaFactory.ReadOptions.AUTO |
                                MediaFactory.ReadOptions.GENERATE_THUMBNAIL, tokenSource.Token);

                    }).ContinueWith((result) =>
                    {                        
                        Monitor.Enter(items);
                        nrLoadingTasks--;               
                        Monitor.PulseAll(items);
                        Monitor.Exit(items);

                        if (ItemFinishedLoading != null)
                        {
                            ItemFinishedLoading(this, EventArgs.Empty);
                        }

                    }).ConfigureAwait(false);
                }
            }
            finally
            {
                Monitor.Exit(items);
            }
        
        }
    }
}
