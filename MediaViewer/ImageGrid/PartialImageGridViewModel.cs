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
    class PartialImageGridViewModel : ObservableObject
    {

        int startItem;
        int maxItems;

        public int MaxItems
        {
            get { return maxItems; }
            set { maxItems = value; }
        }

        ImageGridViewModel imageGridViewModel;

        public PartialImageGridViewModel(ImageGridViewModel imageGridViewModel)
        {
            this.imageGridViewModel = imageGridViewModel;
            startItem = 0;
            maxItems = 25;

            media = new ObservableCollection<MediaFile>();

            for (int i = 0; i < maxItems; i++)
            {
                media.Add(null);

            }

            imageGridViewModel.Locations.CollectionChanged += new NotifyCollectionChangedEventHandler(imageGridViewModel_CollectionChanged);

            FirstPage = new Command(new Action(() =>
            {
                startItem = 0;
                loadItemsAsync();
            }));

            NextPage = new Command(new Action(() =>
            {
                startItem = startItem + maxItems;
                loadItemsAsync();
            }));

            PrevPage = new Command(new Action(() =>
            {
                startItem = startItem - maxItems;
                loadItemsAsync();
            }));
           
        }

        Command nextPage;

        public Command NextPage
        {
            get { return nextPage; }
            set { nextPage = value; }
        }
        Command prevPage;

        public Command PrevPage
        {
            get { return prevPage; }
            set { prevPage = value; }
        }
        Command firstPage;

        public Command FirstPage
        {
            get { return firstPage; }
            set { firstPage = value; }
        }
        Command lastPage;

        public Command LastPage
        {
            get { return lastPage; }
            set { lastPage = value; }
        }

        ObservableCollection<MediaFile> media;

        public ObservableCollection<MediaFile> Media
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
                        FirstPage.DoExecute();
                        break;
                    }
            }
        }

        void loadItemsAsync()
        {

            int startIndex = startItem;
            int endIndex = Math.Min(startIndex + maxItems, imageGridViewModel.Locations.Count);

            for(int i = startIndex, j = 0; j < maxItems; i++, j++) {

                if (i < endIndex)
                {

                    Task<MediaFile> task = MediaFileFactory.openAsync(imageGridViewModel.Locations[i], MediaFile.MetaDataMode.LOAD_FROM_DISK, CancellationToken.None, j);
                                     

                    task.ContinueWith(child =>
                    {

                        int index = (int)child.Result.UserState;

                        Media[index] = child.Result;

                    }, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.NotOnCanceled);
                }
                else
                {
                    Media[j] = null;
                }
               
            }
            
        }
        
    }
}
