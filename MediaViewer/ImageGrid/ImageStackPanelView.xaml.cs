using Microsoft.Practices.Prism.Regions;
using System;
using System.Collections.Generic;
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
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.PubSubEvents;
using MediaViewer.Model.GlobalEvents;
using MediaViewer.Model.Media.State.CollectionView;

namespace MediaViewer.ImageGrid
{
    /// <summary>
    /// Interaction logic for ImageStackPanelView.xaml
    /// </summary>
    [Export]
    public partial class ImageStackPanelView : UserControl, INavigationAware
    {
        DefaultMediaStateCollectionView MediaCollectionView;

        ImageGridViewModel ViewModel { get; set; }

        [ImportingConstructor]
        public ImageStackPanelView(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            itemsControl.Height = itemsControl.Height + SystemParameters.HorizontalScrollBarHeight;

            eventAggregator.GetEvent<MediaBrowserDisplayEvent>().Subscribe(imageStackPanelView_DisplayEvent, ThreadOption.UIThread);
        }

        private void imageStackPanelView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ImageGridViewModel)
            {
                ImageGridViewModel imageGridViewModel = e.OldValue as ImageGridViewModel;

                MediaCollectionView.detachFromMediaState();
            }

            if (e.NewValue is ImageGridViewModel)
            {
                ImageGridViewModel imageGridViewModel = e.NewValue as ImageGridViewModel;

                MediaCollectionView = new DefaultMediaStateCollectionView(imageGridViewModel.MediaState);
                itemsControl.ItemsSource = MediaCollectionView.Media;          
                sortComboBox.ItemsSource = MediaCollectionView.SortModes;

            }
        }

        private void imageStackPanelView_DisplayEvent(MediaBrowserDisplayOptions options)
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

            MediaCollectionView.FilterModes.MoveCurrentTo(options.FilterMode);

            //ViewModel.FilterMode = options.FilterMode;
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
            ViewModel = (ImageGridViewModel)navigationContext.Parameters["viewModel"];

            DataContext = ViewModel;
        }

        private void collapseToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            collapseableGrid.Visibility = Visibility.Collapsed;
        }

        private void collapseToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            collapseableGrid.Visibility = Visibility.Visible;
        }

      
    }
}
