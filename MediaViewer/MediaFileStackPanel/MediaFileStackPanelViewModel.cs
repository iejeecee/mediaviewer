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
                MediaStateCollectionView.Media.EnterWriteLock();
                try
                {
                    int nrItems = MediaStateCollectionView.Media.Count();

                    if (nrItems > 0)
                    {
                        MediaItem selectedItem;

                        MediaStateCollectionView.getSelectedItem(out selectedItem);

                        if(selectedItem != null) 
                        {
                            int index = MediaStateCollectionView.Media.IndexOf(new SelectableMediaItem(selectedItem));

                            if (index + 1 < nrItems)
                            {
                                MediaStateCollectionView.selectExclusive(MediaStateCollectionView.Media[index + 1].Item);
                            }
                        }
                    }

                }
                finally
                {
                    MediaStateCollectionView.Media.ExitWriteLock();
                }
            });

            PrevPageCommand = new Command(() =>
            {
                MediaStateCollectionView.Media.EnterWriteLock();
                try
                {
                    int nrItems = MediaStateCollectionView.Media.Count();

                    if (nrItems > 0)
                    {
                        MediaItem selectedItem;

                        MediaStateCollectionView.getSelectedItem(out selectedItem);

                        if (selectedItem != null)
                        {
                            int index = MediaStateCollectionView.Media.IndexOf(new SelectableMediaItem(selectedItem));

                            if (index - 1 >= 0)
                            {
                                MediaStateCollectionView.selectExclusive(MediaStateCollectionView.Media[index - 1].Item);
                            }
                        }
                    }

                }
                finally
                {
                    MediaStateCollectionView.Media.ExitWriteLock();
                }
            });

            FirstPageCommand = new Command(() =>
            {
                MediaStateCollectionView.Media.EnterWriteLock();
                try
                {
                    if (MediaStateCollectionView.Media.Count() > 0)
                    {
                        MediaStateCollectionView.selectExclusive(MediaStateCollectionView.Media[0].Item);
                    }

                }
                finally
                {
                    MediaStateCollectionView.Media.ExitWriteLock();
                }

            });

            LastPageCommand = new Command(() =>
            {
                MediaStateCollectionView.Media.EnterWriteLock();
                try
                {
                    if (MediaStateCollectionView.Media.Count() > 0)
                    {
                        MediaStateCollectionView.selectExclusive(MediaStateCollectionView.Media[MediaStateCollectionView.Media.Count() - 1].Item);
                    }

                }
                finally
                {
                    MediaStateCollectionView.Media.ExitWriteLock();
                }
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
           

            MediaStateCollectionView.NrItemsInStateChanged += mediaState_NrItemsInStateChanged;
            MediaStateCollectionView.ItemResorted += mediaState_ItemResorted;
                                                         
        }

        

        void selectItems(MediaSelectionPayload selection)
        {
            ICollection<MediaItem> selectedItems = MediaStateCollectionView.getSelectedItems();
            if (Enumerable.SequenceEqual(selection.Items, selectedItems)) return;

            if (selection.Items.Count() == 0)
            {
                MediaStateCollectionView.deselectAll();
            }
            else
            {
                MediaStateCollectionView.selectExclusive(selection.Items.ElementAt(0));
            }
            
        }

        private void mediaState_ItemResorted(object sender, int[] index)
        {                
            if(Nullable<int>.Equals(CurrentPage,index[0] + 1)) {

                // current page has been resorted 
                // set to new index
                CurrentPage = index[1] + 1;
            }           
        }

        private void mediaState_SelectionChanged(object sender, EventArgs e)
        {           
          
            MediaStateCollectionView.Media.EnterReadLock();
            try
            {                
                //MediaStateCollectionView.getSelectedItem(out selectedItem);
                List<MediaItem> items = MediaStateCollectionView.getSelectedItems();

                EventAggregator.GetEvent<MediaSelectionEvent>().Publish(new MediaSelectionPayload(MediaStateCollectionView.Guid, items));
        
                if(items.Count == 0) {

                    CurrentPage = null;

                } else {

                    CurrentPage = MediaStateCollectionView.Media.IndexOf(new SelectableMediaItem(items[0])) + 1;
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

                 MediaItem selectedItem;

                 int index = MediaStateCollectionView.getSelectedItem(out selectedItem);

                 if (index != -1 && (index + 1) != CurrentPage)
                 {
                     CurrentPage = index + 1;                
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

        int? currentPage;

        public int? CurrentPage
        {
            get
            {
                return (currentPage);
            }
            set
            {
                SetProperty(ref currentPage, value);              
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
            MediaStateCollectionView.SelectionChanged -= mediaState_SelectionChanged;
            EventAggregator.GetEvent<MediaSelectionEvent>().Unsubscribe(selectItems);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {                   
            ICollection<MediaItem> selectedItems = MediaStateCollectionView.getSelectedItems();

            MediaStateCollectionView.SelectionChanged += mediaState_SelectionChanged;
            EventAggregator.GetEvent<MediaSelectionEvent>().Subscribe(selectItems, ThreadOption.UIThread, false, selection => selection.SenderId.Equals(MediaStateCollectionView.Guid));

            String location = (String)navigationContext.Parameters["location"];
            if (!String.IsNullOrEmpty(location))
            {
                MediaFileItem item = MediaFileItem.Factory.create(location);

                if (selectedItems.Count > 0 && selectedItems.ElementAt(0).Equals(item))
                {
                    // Send a selection event in the case the media is already selected
                    // to inform other views
                    EventAggregator.GetEvent<MediaSelectionEvent>().Publish(new MediaSelectionPayload(MediaStateCollectionView.Guid, item));
                }
                else
                {
                    selectItems(new MediaSelectionPayload(MediaStateCollectionView.Guid, item));
                }
            }
            else
            {
                //MediaStateCollectionView.refresh();
            }
        }

        
    }
}
