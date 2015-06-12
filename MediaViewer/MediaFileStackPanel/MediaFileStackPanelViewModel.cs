using MediaViewer.Model.Global.Events;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.State;
using MediaViewer.Model.Media.State.CollectionView;
using MediaViewer.Model.Mvvm;
using MediaViewer.Model.Utils;
using MediaViewer.UserControls.Pager;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MediaViewer.MediaFileStackPanel
{
    public class MediaFileStackPanelViewModel : BindableBase, IPageable
    {
        MediaFileStateCollectionView mediaStateCollectionView;

        public MediaFileStateCollectionView MediaStateCollectionView
        {
            get { return mediaStateCollectionView; }
            protected set { SetProperty(ref mediaStateCollectionView, value); }
        }
     
        protected IEventAggregator EventAggregator { get; set; }
        MediaItem selectedItem;

        public MediaFileStackPanelViewModel(MediaFileState mediaState, IEventAggregator eventAggregator)
        {
            MediaStateCollectionView = new MediaFileStateCollectionView(mediaState);

            initialize(eventAggregator);
        }

        /// <summary>
        /// Use a existing mediaFileStateCollectionView
        /// </summary>
        /// <param name="mediaFileStateCollectionView"></param>
        /// <param name="eventAggregator"></param>
        public MediaFileStackPanelViewModel(MediaFileStateCollectionView mediaFileStateCollectionView, IEventAggregator eventAggregator)            
        {
            MediaStateCollectionView = mediaFileStateCollectionView;

            initialize(eventAggregator);
        }

        void initialize(IEventAggregator eventAggregator)
        {
            IsVisible = false;
            IsEnabled = true;

            NrPages = 0;
            CurrentPage = null;
            IsPagingEnabled = false;

            EventAggregator = eventAggregator;

           
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

            BrowseLocationCommand = new Command<SelectableMediaItem>((selectableItem) =>
            {
                MediaItem item = selectableItem.Item;

                String location = FileUtils.getPathWithoutFileName(item.Location);

                EventAggregator.GetEvent<MediaBrowserPathChangedEvent>().Publish(location);
            });

            OpenLocationCommand = new Command<SelectableMediaItem>((selectableItem) =>
            {
                MediaItem item = selectableItem.Item;

                String location = FileUtils.getPathWithoutFileName(item.Location);

                Process.Start(location);
            });

            DeleteCommand = new Command<SelectableMediaItem>((selectableItem) =>
            {
                if (MessageBox.Show("Delete:\n\n" + selectableItem.Item.Location, "Delete file", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                {

                    List<MediaFileItem> item = new List<MediaFileItem>();
                    item.Add(selectableItem.Item as MediaFileItem);
                    CancellationTokenSource tokenSource = new CancellationTokenSource();

                    (MediaStateCollectionView.MediaState as MediaFileState).delete(item, tokenSource.Token);

                }
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
          
            MediaStateCollectionView.Media.EnterReadLock();
            try
            {                
                MediaStateCollectionView.getSelectedItem(out selectedItem);

                if (selectedItem != null)
                {
                    CurrentPage = MediaStateCollectionView.Media.IndexOf(new SelectableMediaItem(selectedItem)) + 1;
                }
                else
                {
                    CurrentPage = null;
                }
     
            }
            finally
            {
                MediaStateCollectionView.Media.ExitReadLock();
            }
                       
        }
     
        private void mediaState_NrItemsInStateChanged(object sender, EventArgs e)
        {       
             MediaStateCollectionView.Media.EnterReadLock();
             try
             {
                 NrPages = MediaStateCollectionView.Media.Count();

                 // if the number of items in the state has changed
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
                 MediaStateCollectionView.Media.ExitReadLock();
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
                                   
                    MediaStateCollectionView.Media.EnterReadLock();
                    try
                    {
                        newPage = MiscUtils.clamp<int>(newPage.Value, 1, MediaStateCollectionView.Media.Count);

                        int index = newPage.Value - 1;

                        MediaItem item = MediaStateCollectionView.Media[index].Item;
                        
                        EventAggregator.GetEvent<MediaViewer.Model.Global.Events.MediaSelectionEvent>().Publish(item);                                       
                    }
                    finally
                    {
                        MediaStateCollectionView.Media.ExitReadLock();
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

        public Command<SelectableMediaItem> BrowseLocationCommand { get; set; }
        public Command<SelectableMediaItem> OpenLocationCommand { get; set; }
        public Command<SelectableMediaItem> DeleteCommand { get; set; }
        
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            MediaStateCollectionView.NrItemsInStateChanged -= mediaState_NrItemsInStateChanged;
            MediaStateCollectionView.SelectionChanged -= mediaState_SelectionChanged;
            MediaStateCollectionView.ItemResorted -= mediaState_ItemResorted;         
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            MediaStateCollectionView.NrItemsInStateChanged += mediaState_NrItemsInStateChanged;
            MediaStateCollectionView.SelectionChanged += mediaState_SelectionChanged;
            MediaStateCollectionView.ItemResorted += mediaState_ItemResorted;

            MediaStateCollectionView.refresh();
        }
    }
}
