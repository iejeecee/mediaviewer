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
using MediaViewer.Input;
using MediaViewer.Model.Media.File;
using MediaViewer.UserControls.Pager;
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
using MediaViewer.Model.Global.Events;
using MediaViewer.Model.Media.State.CollectionView;
using MediaViewer.Infrastructure;
using MediaViewer.Infrastructure.Global.Events;
using MediaViewer.UserControls.MediaGrid;
using MediaViewer.MediaFileGrid;

namespace MediaViewer.MediaFileBrowser
{
    /// <summary>
    /// Interaction logic for MediaFileBrowserControl.xaml
    /// </summary>
    [Export]
    public partial class MediaFileBrowserView : UserControl, IRegionMemberLifetime, INavigationAware
    {

        

        public MediaFileBrowserViewModel MediaFileBrowserViewModel
        {
            get;
            private set;
        }

        IRegionManager RegionManager { get; set; }
        IEventAggregator EventAggregator { get; set; }

        GridLength leftColumnWidth;
        GridLength rightColumnWidth;
    
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

            leftColumnWidth = mainGrid.ColumnDefinitions[0].Width;
            rightColumnWidth = mainGrid.ColumnDefinitions[2].Width;
        }
       
        private void toggleFullScreen(bool isFullScreen)
        {

            if (isFullScreen)
            {
                leftExpanderPanel.Visibility = System.Windows.Visibility.Collapsed;
                rightExpanderPanel.Visibility = System.Windows.Visibility.Collapsed;
                miscOptionsGrid.Visibility = System.Windows.Visibility.Collapsed;

                leftColumnWidth = mainGrid.ColumnDefinitions[0].Width;
                rightColumnWidth = mainGrid.ColumnDefinitions[2].Width;
                mainGrid.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Pixel);
                mainGrid.ColumnDefinitions[2].Width = new GridLength(0, GridUnitType.Pixel);

                leftGridSplitter.Visibility = System.Windows.Visibility.Collapsed;
                rightGridSplitter.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                leftExpanderPanel.Visibility = System.Windows.Visibility.Visible;
                rightExpanderPanel.Visibility = System.Windows.Visibility.Visible;
                miscOptionsGrid.Visibility = System.Windows.Visibility.Visible;

                leftGridSplitter.Visibility = System.Windows.Visibility.Visible;
                rightGridSplitter.Visibility = System.Windows.Visibility.Visible;
                mainGrid.ColumnDefinitions[0].Width = leftColumnWidth;
                mainGrid.ColumnDefinitions[2].Width = rightColumnWidth;
                
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
          
            if (e.Uri.ToString().StartsWith(typeof(MediaFileGridView).FullName))
            {
                MediaFileBrowserViewModel.ExpandCommand.IsExecutable = true;
                MediaFileBrowserViewModel.ContractCommand.IsExecutable = false;
            }
            else
            {
                MediaFileBrowserViewModel.ExpandCommand.IsExecutable = false;
                MediaFileBrowserViewModel.ContractCommand.IsExecutable = true;
            }
        }

        private void selectedViewModelChanged(object sender, PropertyChangedEventArgs e)
        {
            //pager.DataContext = mediaFileBrowserViewModel.CurrentViewModel;
            //metaDataView.DataContext = mediaFileBrowserViewModel.CurrentViewModel;

        }
        
        private void selectedMediaDataGridView_RowDoubleClick(object sender, MediaFileItem e)
        {
            MediaFileBrowserViewModel.ExpandCommand.Execute(e);
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
            MediaFileBrowserViewModel.OnNavigatedFrom(navigationContext);           
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            MediaFileBrowserViewModel = (MediaFileBrowserViewModel)navigationContext.Parameters["viewModel"];
            DataContext = MediaFileBrowserViewModel;

            MediaFileBrowserViewModel.OnNavigatedTo(navigationContext);

        }
    }
        
}
