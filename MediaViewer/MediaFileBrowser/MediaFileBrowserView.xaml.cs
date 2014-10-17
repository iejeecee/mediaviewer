using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
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
using MediaViewer.MediaGrid;
using MediaViewer.Input;
using MediaViewer.Model.Media.File;
using MediaViewer.Pager;
using VideoPlayerControl;
using MediaViewer.ImagePanel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Mvvm;
using MediaViewer.MetaData;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.ServiceLocation;
using MediaViewer.VideoPanel;
using MediaViewer.Model.GlobalEvents;
using MediaViewer.Model.Media.State.CollectionView;

namespace MediaViewer.MediaFileBrowser
{
    /// <summary>
    /// Interaction logic for MediaFileBrowserControl.xaml
    /// </summary>
    [Export]
    public partial class MediaFileBrowserView : UserControl, IRegionMemberLifetime, INavigationAware
    {

        static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MediaFileBrowserViewModel MediaFileBrowserViewModel
        {
            get;
            private set;
        }

        IRegionManager RegionManager { get; set; }
        IEventAggregator EventAggregator { get; set; }
    
        public MediaFileBrowserView()
        {
            InitializeComponent();
        }

        [ImportingConstructor]
        public MediaFileBrowserView(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            Microsoft.Practices.Prism.Regions.RegionManager.SetRegionManager(this, regionManager);

            InitializeComponent();

            RegionManager = regionManager;
            EventAggregator = eventAggregator;

            RegionManager.Regions[RegionNames.MediaFileBrowserContentRegion].NavigationService.Navigating += mediaFileBrowserContentRegion_Navigating;
            RegionManager.Regions[RegionNames.MediaFileBrowserContentRegion].NavigationService.Navigated += mediaFileBrowserContentRegion_Navigated;

            EventAggregator.GetEvent<ToggleFullScreenEvent>().Subscribe(toggleFullScreen);                                       
        }

        private void toggleFullScreen(bool isFullScreen)
        {
            if (isFullScreen)
            {
                browserTabControl.Visibility = System.Windows.Visibility.Collapsed;
                rightTabControl.Visibility = System.Windows.Visibility.Collapsed;
                miscOptionsGrid.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                browserTabControl.Visibility = System.Windows.Visibility.Visible;
                rightTabControl.Visibility = System.Windows.Visibility.Visible;
                miscOptionsGrid.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void mediaFileBrowserContentRegion_Navigating(object sender, RegionNavigationEventArgs e)
        {
            if (MediaFileBrowserViewModel == null || RegionManager.Regions[RegionNames.MediaFileBrowserContentRegion].NavigationService.Journal.CurrentEntry == null) return;

            Uri currentUri = RegionManager.Regions[RegionNames.MediaFileBrowserContentRegion].NavigationService.Journal.CurrentEntry.Uri;

            if (currentUri.ToString().StartsWith(typeof(VideoView).FullName))
            {
                MediaFileBrowserViewModel.NavigateBackCommand = MediaFileBrowserViewModel.NavigateToVideoViewCommand;
            }
            else if (currentUri.ToString().StartsWith(typeof(ImageView).FullName))
            {
                MediaFileBrowserViewModel.NavigateBackCommand = MediaFileBrowserViewModel.NavigateToImageViewCommand;
            }
            else
            {
                MediaFileBrowserViewModel.NavigateBackCommand = MediaFileBrowserViewModel.NavigateToImageGridCommand;
            }
        }

        private void mediaFileBrowserContentRegion_Navigated(object sender, RegionNavigationEventArgs e)
        {
            if (MediaFileBrowserViewModel == null) return;
          
            if (e.Uri.ToString().StartsWith(typeof(MediaGridView).FullName))
            {
                MediaFileBrowserViewModel.ExpandCommand.CanExecute = true;
                MediaFileBrowserViewModel.ContractCommand.CanExecute = false;
            }
            else
            {
                MediaFileBrowserViewModel.ExpandCommand.CanExecute = false;
                MediaFileBrowserViewModel.ContractCommand.CanExecute = true;
            }
        }

        private void selectedViewModelChanged(object sender, PropertyChangedEventArgs e)
        {
            //pager.DataContext = mediaFileBrowserViewModel.CurrentViewModel;
            //metaDataView.DataContext = mediaFileBrowserViewModel.CurrentViewModel;

        }

        private void mediaFileBrowser_ToggleFullScreen()
        {
            /*Visibility mode;

           if (pager.Visibility == System.Windows.Visibility.Visible)
            {
                mode = System.Windows.Visibility.Collapsed;
            }
            else
            {
                mode = System.Windows.Visibility.Visible;
            }

            pager.Visibility = mode;
            browserButtons.Visibility = mode;
            browserTabControl.Visibility = mode;
            metaDataTabControl.Visibility = mode;
            //imageOptionsGrid.Visibility = mode;
             */
        }

        private void selectedMediaDataGridView_RowDoubleClick(object sender, MediaFileItem e)
        {
            MediaFileBrowserViewModel.ExpandCommand.DoExecute(e);
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
            
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            MediaFileBrowserViewModel = (MediaFileBrowserViewModel)navigationContext.Parameters["viewModel"];
            DataContext = MediaFileBrowserViewModel;
            String title = "";
                      
            if (MediaFileBrowserViewModel.CurrentViewModel == null)
            {
                MediaFileBrowserViewModel.navigateToMetaData();
                MediaFileBrowserViewModel.navigateToMediaGrid();

                title = MediaFileBrowserViewModel.BrowsePath;
            }
            else
            {               
                if (MediaFileBrowserViewModel.CurrentViewModel is MediaGridViewModel)
                {
                    Shell.ShellViewModel.navigateToMediaStackPanelView(MediaFileBrowserViewModel.DummyMediaStackPanelViewModel);
                    title = MediaFileBrowserViewModel.BrowsePath;
                }
                else if(MediaFileBrowserViewModel.CurrentViewModel is ImageViewModel)
                {
                    Shell.ShellViewModel.navigateToMediaStackPanelView(MediaFileBrowserViewModel.ImageMediaStackPanelViewModel);
                    title = MediaFileBrowserViewModel.ImageViewModel.CurrentLocation;
                    if (title != null) title = System.IO.Path.GetFileName(title);
                }
                else if (MediaFileBrowserViewModel.CurrentViewModel is VideoViewModel)
                {
                    Shell.ShellViewModel.navigateToMediaStackPanelView(MediaFileBrowserViewModel.VideoMediaStackPanelViewModel);
                    title = MediaFileBrowserViewModel.VideoViewModel.CurrentLocation;
                    if (title != null) title = System.IO.Path.GetFileName(title);
                }
                                
            }

            EventAggregator.GetEvent<TitleChangedEvent>().Publish(title);
        }
    }
        
}
