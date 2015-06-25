using MediaViewer.Infrastructure.Global.Events;
using MediaViewer.Model.Global.Events;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.State.CollectionView;
using MediaViewer.Model.Utils;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediaViewer.MediaFileStackPanel
{    
    [Export]
    public partial class MediaFileStackPanelView : UserControl, INavigationAware   
    {
        MediaFileStackPanelViewModel ViewModel { get; set; }
        IEventAggregator EventAggregator { get; set; }

        [ImportingConstructor]
        public MediaFileStackPanelView(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            EventAggregator = eventAggregator;

            eventAggregator.GetEvent<MediaBrowserDisplayEvent>().Subscribe(imageStackPanelView_DisplayEvent, ThreadOption.UIThread);
            eventAggregator.GetEvent<MediaSelectionEvent>().Subscribe(selectItem, ThreadOption.UIThread);
            eventAggregator.GetEvent<ToggleFullScreenEvent>().Subscribe(toggleFullScreen, ThreadOption.UIThread);
        }

        void selectItem(MediaItem item)
        {
            ICollection<MediaItem> selectedItems = ViewModel.MediaStateCollectionView.getSelectedItems();
            if (selectedItems.Count > 0 && selectedItems.ElementAt(0).Equals(item)) return;

            ViewModel.MediaStateCollectionView.deselectAll();
            ViewModel.MediaStateCollectionView.setIsSelected(item, true);
            
        }

        private void mediaStackPanel_MediaItemClick(Object sender, SelectableMediaItem selectableItem)
        {
            MediaItem item = selectableItem.Item;

            if (MediaFormatConvert.isImageFile(item.Location))
            {
                Shell.ShellViewModel.navigateToImageView(item);
            }
            else if (MediaFormatConvert.isVideoFile(item.Location))
            {
                Shell.ShellViewModel.navigateToVideoView(item);
            }
        }

        private void imageStackPanelView_DisplayEvent(MediaBrowserDisplayOptions options)
        {
            if (options.IsHidden != null)
            {
                if (options.IsHidden == true)
                {
                    if (collapseToggleButton.IsChecked.Value == false)
                    {
                        collapseToggleButton.IsChecked = true;
                    }
                }
                else
                {
                    if (collapseToggleButton.IsChecked.Value == true)
                    {
                        collapseToggleButton.IsChecked = false;
                    }
                }
            }

            if (options.IsEnabled != null)
            {
                if (options.IsEnabled == false)
                {
                    collapseToggleButton.IsChecked = true;
                    collapseToggleButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    collapseToggleButton.Visibility = Visibility.Visible;
                }
            }

            if (options.FilterMode != null)
            {
                ViewModel.MediaStateCollectionView.FilterModes.MoveCurrentTo(options.FilterMode);
            }

            if (options.SelectedItem != null)
            {
                selectItem(options.SelectedItem);

            }

            //ViewModel.FilterMode = options.FilterMode;
        }

        private void toggleFullScreen(bool isFullScreen)
        {
            if (isFullScreen)
            {
                mainGrid.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                mainGrid.Visibility = System.Windows.Visibility.Visible;
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return (true);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            ViewModel.MediaStateCollectionView.Cleared -= MediaStateCollectionView_Cleared;
            ViewModel.OnNavigatedFrom(navigationContext);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            ViewModel = (MediaFileStackPanelViewModel)navigationContext.Parameters["viewModel"];                          
            DataContext = ViewModel;

            if (ViewModel.IsEnabled == false) return;

            ViewModel.MediaStateCollectionView.Cleared += MediaStateCollectionView_Cleared;
            ViewModel.OnNavigatedTo(navigationContext);   

            ICollection<MediaItem> selectedItems = ViewModel.MediaStateCollectionView.getSelectedItems();

            String location = (String)navigationContext.Parameters["location"];
            if (!String.IsNullOrEmpty(location))
            {
                MediaFileItem item = MediaFileItem.Factory.create(location);

                if (selectedItems.Count > 0 && selectedItems.ElementAt(0).Equals(item))
                {
                    // Send a selection event in the case the media is already selected
                    // to inform other views
                    EventAggregator.GetEvent<MediaSelectionEvent>().Publish(item);
                }
                else
                {
                    selectItem(item);
                }
            }
            else
            {
                if (selectedItems.Count > 0)
                {
                    // Send a selection event to inform other views
                    EventAggregator.GetEvent<MediaSelectionEvent>().Publish(selectedItems.ElementAt(0));
                }
            }
        }

        void MediaStateCollectionView_Cleared(object sender, EventArgs e)
        {
            if (mediaStackPanel.scrollViewer != null)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() => mediaStackPanel.scrollViewer.ScrollToHorizontalOffset(0)));
            }
        }
    }
}
