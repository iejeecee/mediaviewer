using MediaViewer.MediaFileModel;
using MediaViewer.Pager;
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
    class PagedImageGridViewModel : ImageGridViewModel, IPageable
    {

        public PagedImageGridViewModel()
        {


            maxItemsPerPage = 25;
            media = new ObservableCollection<ImageGridItem>();

            for (int i = 0; i < maxItemsPerPage; i++)
            {
                media.Add(null);

            }

            Items.CollectionChanged += new NotifyCollectionChangedEventHandler(imageGridViewModel_CollectionChanged);

            nextPageCommand = new Command(new Action(() =>
            {
                CurrentPage += 1;

            }));

            prevPageCommand = new Command(new Action(() =>
            {
                CurrentPage -= 1;


            }));

            firstPageCommand = new Command(new Action(() =>
            {
                CurrentPage = 0;


            }));

            lastPageCommand = new Command(new Action(() =>
            {
                CurrentPage = NrPages;


            }));

            CurrentPage = 0;
            NrPages = 0;
            isPagingEnabled = false;

        }

        int maxItemsPerPage;

        public int MaxItemsPerPage
        {
            get { return maxItemsPerPage; }
            set { maxItemsPerPage = value;
            NotifyPropertyChanged();
            }
        }

        bool isPagingEnabled;

        public bool IsPagingEnabled
        {
            get { return (isPagingEnabled); }
            set {
                isPagingEnabled = value;
                NotifyPropertyChanged();
            }
        }

        int currentPage;

        public int CurrentPage
        {
            get { return (currentPage); }
            set {
                        
               
                if (value >= 0 && value <= NrPages)                 
                {
                    if (value == 0 && NrPages > 0)
                    {
                        currentPage = 1;
                    }
                    else
                    {

                        currentPage = value;
                    }

                    loadItemsAsync();

                    setExecuteState();
     
                    NotifyPropertyChanged();
                }
              
            }
                
        }

        int nrPages;

        public int NrPages
        {
            get { return (nrPages); }
            set {
                nrPages = value;
                NotifyPropertyChanged();
            }
        }

        Command nextPageCommand;

        public Command NextPageCommand
        {
            get { return nextPageCommand; }
            set { nextPageCommand = value; }
        }
        Command prevPageCommand;

        public Command PrevPageCommand
        {
            get { return prevPageCommand; }
            set { prevPageCommand = value; }
        }
        Command firstPageCommand;

        public Command FirstPageCommand
        {
            get { return firstPageCommand; }
            set { firstPageCommand = value; }
        }
        Command lastPageCommand;

        public Command LastPageCommand
        {
            get { return lastPageCommand; }
            set { lastPageCommand = value; }
        }

        ObservableCollection<ImageGridItem> media;

        public ObservableCollection<ImageGridItem> Media
        {
            get { return media; }
            set { media = value; }
        }

        void setExecuteState()
        {
            if (CurrentPage + 1 <= NrPages)
            {
                nextPageCommand.CanExecute = true;
                lastPageCommand.CanExecute = true;
            }
            else
            {
                nextPageCommand.CanExecute = false;
                lastPageCommand.CanExecute = false;
            }

            if (CurrentPage - 1 >= 1)
            {
                prevPageCommand.CanExecute = true;
                firstPageCommand.CanExecute = true;
            }
            else
            {
                prevPageCommand.CanExecute = false;
                firstPageCommand.CanExecute = false;
            }
        }

        void imageGridViewModel_CollectionChanged(Object sender, NotifyCollectionChangedEventArgs e) {
             
            //int startIndex = CurrentPage * maxItemsPerPage;
            //int endIndex = startIndex + maxItemsPerPage;
            int totalPages = (int)Math.Ceiling(Items.Count / (float)MaxItemsPerPage);

            if(Items.Count == 0)
            {
                NrPages = 0;              
            }
            else if (totalPages == 0)
            {
                NrPages = 1;               
            }
            else
            {
                NrPages = totalPages;
            }

            if (NrPages <= 1)
            {
                IsPagingEnabled = false;
            }
            else
            {
                IsPagingEnabled = true;
            }
            
            if(CurrentPage == 0 && NrPages > 0) {

                CurrentPage = 1;

            }
            else if (CurrentPage > NrPages)
            {

                CurrentPage = NrPages;
            }
            else
            {
                loadItemsAsync();
            }

            setExecuteState();
           
/*
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    {
                        CurrentPage = 0;
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
                        if (CurrentPage > NrPages)
                        {
                            CurrentPage = NrPages;

                        }
                        else
                        {

                            loadItemsAsync();
                        }
                        break;
                    }             
            }

*/           
        }

        void loadItemsAsync()
        {

            int startItem = (CurrentPage > 0 ? CurrentPage - 1 : CurrentPage) * MaxItemsPerPage;

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
