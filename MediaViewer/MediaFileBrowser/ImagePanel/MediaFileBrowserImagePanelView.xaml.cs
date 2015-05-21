using MediaViewer.Infrastructure.Global.Events;
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

namespace MediaViewer.MediaFileBrowser.ImagePanel
{
    /// <summary>
    /// Interaction logic for MediaFileBrowserImagePanelView.xaml
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class MediaFileBrowserImagePanelView : UserControl, IRegionMemberLifetime, INavigationAware
    {
        IMediaFileBrowserContentViewModel ViewModel { get; set; }
        IEventAggregator EventAggregator { get; set; }

        [ImportingConstructor]
        public MediaFileBrowserImagePanelView(IEventAggregator eventAggregator)
        {
            InitializeComponent();
            EventAggregator = eventAggregator;

            IsFullScreen = false;

        }

        private void imagePanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                EventAggregator.GetEvent<ToggleFullScreenEvent>().Publish(IsFullScreen = !IsFullScreen);
            }
        }

        public bool KeepAlive
        {
            get { return (true); }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return (true);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            ViewModel.OnNavigatedFrom(navigationContext);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            ViewModel = (MediaFileBrowserImagePanelViewModel)navigationContext.Parameters["viewModel"];
            DataContext = ViewModel;

            ViewModel.OnNavigatedTo(navigationContext);
        }

        bool isFullScreen;

        bool IsFullScreen
        {
            get
            {
                return (isFullScreen);
            }

            set
            {
                if (value == true)
                {
                    controlsGrid.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    controlsGrid.Visibility = System.Windows.Visibility.Visible;
                }

                isFullScreen = value;
            }
        }
    }
}
