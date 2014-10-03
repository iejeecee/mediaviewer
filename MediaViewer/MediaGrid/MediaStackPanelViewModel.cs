using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.State;
using MediaViewer.Model.Media.State.CollectionView;
using MediaViewer.Model.Utils;
using MediaViewer.Pager;
using Microsoft.Practices.Prism.PubSubEvents;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaGrid
{
    public class MediaStackPanelViewModel : MediaStateCollectionViewModel, IPageable
    {
        protected IEventAggregator EventAggregator { get; set; }

        public MediaStackPanelViewModel(IMediaState mediaState, IEventAggregator eventAggregator)
            : base(mediaState)
        {
            IsVisible = false;
            IsEnabled = true;

            NrPages = 0;
            CurrentPage = null;
            IsPagingEnabled = false;

            EventAggregator = eventAggregator;

            MediaStateCollectionView.NrItemsInStateChanged += mediaState_NrItemsInStateChanged;
            MediaStateCollectionView.SelectionChanged += mediaState_SelectionChanged;

            NextPageCommand = new Command(() =>
            {
                CurrentPage = CurrentPage + 1;
            });

            PrevPageCommand = new Command(() =>
            {
                CurrentPage = CurrentPage - 1;
            });

            FirstPageCommand = new Command(() =>
            {
                CurrentPage = 1;
               
            });

            LastPageCommand = new Command(() =>
            {
                MediaStateCollectionView.lockMedia();
                try
                {
                    CurrentPage = MediaStateCollectionView.Media.Count;
                }
                finally
                {
                    MediaStateCollectionView.unlockMedia();
                }
            });
        }

        private void mediaState_SelectionChanged(object sender, EventArgs e)
        {
            MediaStateCollectionView.lockMedia();
            try
            {
                ICollection<MediaFileItem> selectedItems = MediaStateCollectionView.getSelectedItems();

                if (selectedItems.Count > 0)
                {
                    MediaFileItem selectedItem = selectedItems.ElementAt(0);
                    CurrentPage = MediaStateCollectionView.getIndexOf(selectedItem) + 1;
                   
                }
                else
                {
                    CurrentPage = null;
                }
            }
            finally
            {
                MediaStateCollectionView.unlockMedia();
            }
        }

        private void mediaState_NrItemsInStateChanged(object sender, EventArgs e)
        {
            MediaStateCollectionView.lockMedia();
            NrPages = MediaStateCollectionView.Media.Count();
            MediaStateCollectionView.unlockMedia();
        }

        bool isVisible;

        public bool IsVisible
        {
            get { return isVisible; }
            set { isVisible = value;
            NotifyPropertyChanged();
            }
        }

        bool isEnabled;

        public bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value;

            if (isEnabled == false)
            {
                IsVisible = false;
            }
            NotifyPropertyChanged();
            }
        }

        int nrPages;

        public int NrPages
        {
            get
            {
                return (nrPages);
            }
            set
            {
                nrPages = value;

                if (nrPages > 0)
                {
                    IsPagingEnabled = true;
                }
                else
                {
                    IsPagingEnabled = false;
                }
                NotifyPropertyChanged();
            }
        }

        Nullable<int> currentPage;

        public Nullable<int> CurrentPage
        {
            get
            {
                return (currentPage);
            }
            set
            {
                currentPage = value;

                if (currentPage.HasValue)
                {
                    

                    MediaStateCollectionView.lockMedia();
                    try
                    {
                        currentPage = MiscUtils.clamp<int>(currentPage.Value, 1, MediaStateCollectionView.Media.Count);

                        int index = currentPage.Value - 1;

                        MediaFileItem item = MediaStateCollectionView.Media.ElementAt(index).Item;
                                           
                        EventAggregator.GetEvent<MediaViewer.Model.GlobalEvents.MediaSelectionEvent>().Publish(item);
                       
                    }
                    finally
                    {
                        MediaStateCollectionView.unlockMedia();
                    }

                }
                NotifyPropertyChanged();
            }
        }

        bool isPagingEnabled;

        public bool IsPagingEnabled
        {
            get
            {
                return (isPagingEnabled);
            }
            set
            {
                isPagingEnabled = value;
                NotifyPropertyChanged();
            }
        }

        public Command NextPageCommand { get; set; }
        public Command PrevPageCommand { get; set; }
        public Command FirstPageCommand { get; set; }
        public Command LastPageCommand { get; set; }
    }
}
