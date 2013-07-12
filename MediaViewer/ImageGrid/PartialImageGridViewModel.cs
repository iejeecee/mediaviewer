using MediaViewer.MediaFileModel;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.ImageGrid
{
    class PartialImageGridViewModel : ImageGridViewModel
    {

        int startItem;
        int maxItems;

        public int MaxItems
        {
            get { return maxItems; }
            set { maxItems = value; }
        }

        public PartialImageGridViewModel()
        {
       
            startItem = 0;
            maxItems = 25;

            media = new ObservableCollection<ImageGridItem>();

            for (int i = 0; i < maxItems; i++)
            {
                media.Add(null);

            }

            Items.CollectionChanged += new NotifyCollectionChangedEventHandler(imageGridViewModel_CollectionChanged);


            SetPageCommand = new Command(new Action<object>((param) =>
                {
                    int pageNr = (int)param;

                    int newStartItem = maxItems * (pageNr - 1);

                    if (newStartItem != startItem)
                    {
                        startItem = newStartItem;
                        loadItemsAsync();
                    }
                }));
           
        }

        Command setPageCommand;

        public Command SetPageCommand
        {
            get { return setPageCommand; }
            set { setPageCommand = value; }
        }

        ObservableCollection<ImageGridItem> media;

        public ObservableCollection<ImageGridItem> Media
        {
            get { return media; }
            set { media = value; }
        }

        void imageGridViewModel_CollectionChanged(Object sender, NotifyCollectionChangedEventArgs e) {
             
            int startIndex = startItem;
            int endIndex = startIndex + maxItems;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    {
                        startItem = 0;
                        break;
                    }
                case NotifyCollectionChangedAction.Add:
                    {
                        if (e.NewStartingIndex < endIndex && e.NewStartingIndex >= startIndex)
                        {
                            loadItemsAsync();
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        
                        break;
                    }
            }
        }

        void loadItemsAsync()
        {

            int nrItems = startItem + maxItems > Items.Count ? Items.Count - startItem : maxItems;

            for (int i = 0; i < maxItems; i++)
            {
                if (i < nrItems)
                {
                    Media[i] = Items[startItem + i];
                }
                else
                {
                    Media[i] = null;
                }
            }

            this.loadItemRangeAsync(startItem, nrItems);
            
        }
        
    }
}
