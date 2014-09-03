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

namespace MediaViewer.ImageGrid
{
    /// <summary>
    /// Interaction logic for ImageStackPanelView.xaml
    /// </summary>
    [Export]
    public partial class ImageStackPanelView : UserControl, INavigationAware
    {
        ImageGridViewModel ViewModel { get; set; }

        [ImportingConstructor]
        public ImageStackPanelView(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            itemsControl.Height = itemsControl.Height + SystemParameters.HorizontalScrollBarHeight;

            eventAggregator.GetEvent<GlobalEvents.MediaBrowserDisplayEvent>().Subscribe(imageStackPanelView_DisplayEvent, ThreadOption.UIThread);
        }

        private void imageStackPanelView_DisplayEvent(GlobalEvents.MediaBrowserDisplayOptions options)
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

            ViewModel.FilterMode = options.FilterMode;
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
