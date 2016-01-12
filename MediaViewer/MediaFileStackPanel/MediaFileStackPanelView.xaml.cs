using MediaViewer.Infrastructure.Global.Events;
using MediaViewer.Model.Global.Events;
using MediaViewer.Model.Media.Base.Item;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.Base.State.CollectionView;
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
           
            eventAggregator.GetEvent<ToggleFullScreenEvent>().Subscribe(toggleFullScreen, ThreadOption.UIThread);
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
            if (ViewModel != null)
            {
                ViewModel.MediaStateCollectionView.Cleared -= MediaStateCollectionView_Cleared;
                ViewModel.OnNavigatedFrom(navigationContext);
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {            
            ViewModel = (MediaFileStackPanelViewModel)navigationContext.Parameters["viewModel"];
            DataContext = ViewModel;

            if (ViewModel == null)
            {
                collapseableGrid.Visibility = System.Windows.Visibility.Collapsed;
                collapseableButtonGrid.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }
            else
            {
                Binding binding = new Binding();
                binding.Path = new PropertyPath("IsEnabled");
                binding.Converter = new BooleanToVisibilityConverter();
                binding.Source = ViewModel;  

                BindingOperations.SetBinding(collapseableButtonGrid, Grid.VisibilityProperty, binding);

                binding = new Binding();
                binding.Path = new PropertyPath("IsVisible");
                binding.Converter = new BooleanToVisibilityConverter();
                binding.Source = ViewModel;

                BindingOperations.SetBinding(collapseableGrid, Grid.VisibilityProperty, binding); 

            }

            if (ViewModel.IsEnabled == false) return;
                                       
            ViewModel.MediaStateCollectionView.Cleared += MediaStateCollectionView_Cleared;
            ViewModel.OnNavigatedTo(navigationContext);   
            
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
