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
    class PagedImageGridViewModel : ImageGridViewModel
    {

        int startItem;
        int maxItemsPerPage;

        public int MaxItems
        {
            get { return maxItemsPerPage; }
            set { maxItemsPerPage = value; }
        }

        public PagedImageGridViewModel()
        {
       
            startItem = 0;
            maxItemsPerPage = 25;

            media = new ObservableCollection<ImageGridItem>();

            for (int i = 0; i < maxItemsPerPage; i++)
            {
                media.Add(null);

            }

            Items.CollectionChanged += new NotifyCollectionChangedEventHandler(imageGridViewModel_CollectionChanged);


            SetPageCommand = new Command(new Action<object>((param) =>
                {
                    int pageNr = (int)param;

                    int newStartItem = maxItemsPerPage * (pageNr - 1);

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
            int endIndex = startIndex + maxItemsPerPage;

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
                        if (startItem >= Items.Count)
                        {
                            startItem = Math.Max(Items.Count - 1, 0);
                        }

                        loadItemsAsync();
                        break;
                    }
            }
        }

        void loadItemsAsync()
        {

            int nrItems = startItem + maxItemsPerPage > Items.Count ? Items.Count - startItem : maxItemsPerPage;

            for (int i = 0; i < maxItemsPerPage; i++)
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
