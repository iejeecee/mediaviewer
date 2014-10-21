// intresting stuff:
// Lazy<T> for lazy initialization
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.UserControls.Layout;
using MvvmFoundation.Wpf;
using Microsoft.Practices.Prism.Regions;
using System.ComponentModel.Composition;
using MediaViewer.MediaFileBrowser;
using MediaViewer.Model.Media.State.CollectionView;
using System.Windows.Controls.Primitives;
using Microsoft.Practices.Prism.PubSubEvents;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.GlobalEvents;

namespace MediaViewer.MediaGrid
{
    /// <summary>
    /// Interaction logic for ImageGridControl.xaml
    /// </summary>
    [Export]
    public partial class MediaGridView : UserControl, IRegionMemberLifetime, INavigationAware
    {
        VirtualizingTilePanel panel;
        
        IEventAggregator EventAggregator { get; set; }

        MediaGridViewModel ViewModel {get;set;}
     
     
        [ImportingConstructor]
        public MediaGridView(IRegionManager regionManager, IEventAggregator eventAggregator)
        {           
            InitializeComponent();

            EventAggregator = eventAggregator;

            panel = null;
           
        }

        void ImageGridView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is MediaGridViewModel)
            {
                MediaGridViewModel oldImageGridViewModel = e.OldValue as MediaGridViewModel;

                oldImageGridViewModel.MediaStateCollectionView.Cleared -= imageGridViewModel_Cleared;
                oldImageGridViewModel.MediaStateCollectionView.SelectionChanged -= imageGridViewModel_SelectionChanged;
                           
            }

            if (e.NewValue is MediaGridViewModel)
            {
                MediaGridViewModel imageGridViewModel = e.NewValue as MediaGridViewModel;

                imageGridViewModel.MediaStateCollectionView.Cleared += imageGridViewModel_Cleared;
                imageGridViewModel.MediaStateCollectionView.SelectionChanged += imageGridViewModel_SelectionChanged;

                //itemsControl.ItemsSource = MediaCollectionView.Media;
                //filterComboBox.ItemsSource = MediaCollectionView.FilterModes;               
                //sortComboBox.ItemsSource = MediaCollectionView.SortModes;
                                                       
            }
            
        }

        private void imageGridViewModel_SelectionChanged(object sender, EventArgs e)
        {
            List<MediaFileItem> selectedItems = ViewModel.MediaStateCollectionView.getSelectedItems();
           
            EventAggregator.GetEvent<MediaViewer.Model.GlobalEvents.MediaBatchSelectionEvent>().Publish(selectedItems);
        }

        private void imageGridViewModel_PrevPageCommand(object sender, EventArgs e)
        {
            if (panel != null)
            {
                panel.PageUp();
              
            }
        }

        private void imageGridViewModel_NextPageCommand(object sender, EventArgs e)
        {
            if (panel != null)
            {
                panel.PageDown();
            }
        }

        private void imageGridViewModel_FirstPageCommand(object sender, EventArgs e)
        {
            if (panel != null)
            {
                panel.SetHorizontalOffset(double.NegativeInfinity);
            }
        }

        private void imageGridViewModel_LastPageCommand(object sender, EventArgs e)
        {
            if (panel != null)
            {
                panel.SetHorizontalOffset(double.PositiveInfinity);
            }
        }

        void imageGridViewModel_Cleared(object sender, EventArgs e)
        {
            if (panel != null)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() => panel.ScrollOwner.ScrollToVerticalOffset(0)));
            }
        }

        private void virtualizingTilePanel_Loaded(object sender, RoutedEventArgs e)
        {
            panel = sender as VirtualizingTilePanel;
            
            
        }

        public bool KeepAlive
        {
            get { return(true); }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return (true);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
           
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            ViewModel = (MediaGridViewModel)navigationContext.Parameters["viewModel"];

            DataContext = ViewModel;

            EventAggregator.GetEvent<MediaBatchSelectionEvent>().Publish(ViewModel.MediaStateCollectionView.getSelectedItems());
                    
        }
    }
}
