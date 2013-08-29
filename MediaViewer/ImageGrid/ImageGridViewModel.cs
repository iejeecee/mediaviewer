using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaViewer.MediaFileModel;
using MvvmFoundation.Wpf;
using System.Windows.Data;
using System.Windows;
using System.Threading;
using System.Windows.Threading;

namespace MediaViewer.ImageGrid
{
    class ImageGridViewModel : ObservableObject
    {
       
        CancellationTokenSource loadItemsCTS;

        // maximum concurrently loading items
        int maxLoadingItems;
        // current concurrently loading items
        int nrLoadingItems;
        Object nrLoadingItemsLock;

        public ImageGridViewModel()
        {
            items = new ObservableRangeCollection<ImageGridItem>();

            nrLoadingItems = 0;
            maxLoadingItems = 25;

            nrLoadingItemsLock = new Object();
            loadItemsCTS = new CancellationTokenSource();
        }

        ObservableRangeCollection<ImageGridItem> items;

        public ObservableRangeCollection<ImageGridItem> Items
        {
            get { return items; }
            set { items = value; }
        }

       
        void loadItemCompleted()
        {

            lock (nrLoadingItemsLock)
            {
               
                nrLoadingItems = nrLoadingItems - 1;               

                Monitor.PulseAll(nrLoadingItemsLock);
            }
        }

        public void loadItemRangeAsync(int start, int nrItems)
        {
            // cancel any previously loading items           
            loadItemsCTS.Cancel();
            // create new cts for the items that need to be loaded
            loadItemsCTS = new CancellationTokenSource();

            for (int i = 0; i < nrItems; i++)
            {
                // don't reload already loaded items
                if (items[start + i].ItemState == ImageGridItemState.LOADED) continue;

                lock (nrLoadingItemsLock)
                {
                    while (nrLoadingItems == maxLoadingItems)
                    {
                        Monitor.Wait(nrLoadingItemsLock);
                    }

                    nrLoadingItems = nrLoadingItems + 1;
                }

                items[start + i].loadMediaFileAsync(loadItemsCTS.Token).ContinueWith(finishedTask =>
                {
                    loadItemCompleted();
                });
                
                            
            }
            

        }

        public void selectAll()
        {
            foreach (ImageGridItem item in Items)
            {
                item.IsSelected = true;
            }
        }

        public void deselectAll()
        {
            foreach (ImageGridItem item in Items)
            {
                item.IsSelected = false;
            }
        }

        public List<ImageGridItem> getSelectedItems()
        {
            List<ImageGridItem> selected = new List<ImageGridItem>();

            foreach (ImageGridItem item in Items)
            {
                if (item.IsSelected)
                {
                    selected.Add(item);
                }
            }

            return (selected);
        }
      
    }
}
