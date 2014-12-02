using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.State;
using MediaViewer.Model.Media.State.CollectionView;
using MediaViewer.Model.Mvvm;
using MediaViewer.Model.Utils;
using MediaViewer.UserControls.Pager;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.PubSubEvents;
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
        MediaFileItem selectedItem;

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
            MediaStateCollectionView.ItemResorted += mediaState_ItemResorted;

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
                CurrentPage = MediaStateCollectionView.Media.Count;               
            });

            selectedItem = null;
        }

        private void mediaState_ItemResorted(object sender, int newIndex)
        {
            MediaFileItem resortedItem = (MediaFileItem)sender;

            if (selectedItem != null && selectedItem.Equals(resortedItem))
            {
                currentPage = newIndex + 1;
                OnPropertyChanged("CurrentPage");
            }
        }

        private void mediaState_SelectionChanged(object sender, EventArgs e)
        {           
          
            MediaStateCollectionView.Media.EnterReaderLock();
            try
            {                
                MediaStateCollectionView.getSelectedItem(out selectedItem);

                if (selectedItem != null)
                {
                    CurrentPage = MediaStateCollectionView.Media.IndexOf(new SelectableMediaFileItem(selectedItem)) + 1;
                }
                else
                {
                    CurrentPage = null;
                }
     
            }
            finally
            {
                MediaStateCollectionView.Media.ExitReaderLock();
            }
                       
        }
     
        private void mediaState_NrItemsInStateChanged(object sender, EventArgs e)
        {       
             MediaStateCollectionView.Media.EnterReaderLock();
             try
             {
                 NrPages = MediaStateCollectionView.Media.Count();

                 // if the number of items in the state changed has changed
                 // the index of the currently selected item, update the index            
                 int index = MediaStateCollectionView.getSelectedItem(out selectedItem);

                 if (index != -1 && (index + 1) != CurrentPage)
                 {
                     currentPage = index + 1;
                     OnPropertyChanged("CurrentPage");
                 }
             }
             finally
             {
                 MediaStateCollectionView.Media.ExitReaderLock();
             }
        }

        bool isVisible;

        public bool IsVisible
        {
            get { return isVisible; }
            set {
                SetProperty(ref isVisible, value);  
        
            }
        }

        bool isEnabled;

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                SetProperty(ref isEnabled, value);

                if (isEnabled == false)
                {
                    IsVisible = false;
                }
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
                SetProperty(ref nrPages, value);
                
                if (nrPages > 0)
                {
                    IsPagingEnabled = true;
                }
                else
                {
                    IsPagingEnabled = false;
                }
               
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
                int? newPage = value;              

                if (newPage.HasValue) {
                                   
                    MediaStateCollectionView.Media.EnterReaderLock();
                    try
                    {
                        newPage = MiscUtils.clamp<int>(newPage.Value, 1, MediaStateCollectionView.Media.Count);

                        int index = newPage.Value - 1;

                        MediaFileItem item = MediaStateCollectionView.Media[index].Item;
                        
                        EventAggregator.GetEvent<MediaViewer.Model.Global.Events.MediaSelectionEvent>().Publish(item);                                       
                    }
                    finally
                    {
                        MediaStateCollectionView.Media.ExitReaderLock();
                    }

                }

                SetProperty(ref currentPage, newPage);
              
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
                SetProperty(ref isPagingEnabled, value);           
            }
        }

        public Command NextPageCommand { get; set; }
        public Command PrevPageCommand { get; set; }
        public Command FirstPageCommand { get; set; }
        public Command LastPageCommand { get; set; }
    }
}
