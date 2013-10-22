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
using System.Collections.Specialized;

namespace MediaViewer.ImageGrid
{
    class ImageGridViewModel : ObservableObject
    {
       
        CancellationTokenSource loadItemsCTS;

        // maximum concurrently loading items
        const int maxLoadingItems = 25;
        // current concurrently loading items
        int nrLoadingItems;
        Object nrLoadingItemsLock;

        public ImageGridViewModel()
        {
            SelectedItems = new ObservableRangeCollection<ImageGridItem>();
            Items = new ObservableRangeCollection<ImageGridItem>();
            Items.CollectionChanged += new NotifyCollectionChangedEventHandler(imageGridViewModel_CollectionChanged);           

            nrLoadingItems = 0;           

            nrLoadingItemsLock = new Object();
            loadItemsCTS = new CancellationTokenSource();
            
        }

        ObservableRangeCollection<ImageGridItem> items;

        public ObservableRangeCollection<ImageGridItem> Items
        {
            get { return items; }
            set { items = value; }
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
                    lock (nrLoadingItemsLock)
                    {
                        nrLoadingItems = nrLoadingItems - 1;

                        Monitor.PulseAll(nrLoadingItemsLock);
                    }
                });
                
                            
            }
            

        }

        public void selectAll()
        {
            SelectedItems.Clear();

            foreach (ImageGridItem item in Items)
            {
                item.PropertyChanged -= item_PropertyChanged;
                item.IsSelected = true;
            }

            SelectedItems.AddRange(Items);

            foreach (ImageGridItem item in Items)
            {
                item.PropertyChanged += item_PropertyChanged;               
            }
        }

        public void deselectAll()
        {
            foreach (ImageGridItem item in Items)
            {
                item.IsSelected = false;
            }
        }

        void imageGridViewModel_CollectionChanged(Object sender, NotifyCollectionChangedEventArgs e)
        {

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    {                   
                        break;
                    }
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (ImageGridItem item in e.NewItems)
                        {
                            item.PropertyChanged += item_PropertyChanged;
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (ImageGridItem item in e.OldItems)
                        {
                            item.PropertyChanged -= item_PropertyChanged;
                            SelectedItems.Remove(item);
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private void item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName.Equals("IsSelected"))
            {
                ImageGridItem item = (ImageGridItem)sender;

                if (item.IsSelected == true)
                {
                    SelectedItems.Add(item);
                }
                else
                {
                    SelectedItems.Remove(item);
                }
            }
        }

        ObservableRangeCollection<ImageGridItem> selectedItems;

        public ObservableRangeCollection<ImageGridItem> SelectedItems
        {
            set
            {
                selectedItems = value;
            }

            get
            {
                return (selectedItems);
            }
        }
        
      
    }
}
