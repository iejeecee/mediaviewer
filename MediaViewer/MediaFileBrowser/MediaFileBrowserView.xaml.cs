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
using MediaViewer.ImageGrid;
using MediaViewer.Input;
using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.Utils;
using MediaViewer.Pager;
using VideoPlayerControl;
using MediaViewer.ImagePanel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Mvvm;
using MediaViewer.MetaData;
using Microsoft.Practices.Prism.PubSubEvents;
using MediaViewer.GlobalEvents;
using Microsoft.Practices.ServiceLocation;

namespace MediaViewer.MediaFileBrowser
{
    /// <summary>
    /// Interaction logic for MediaFileBrowserControl.xaml
    /// </summary>
    [Export]
    public partial class MediaFileBrowserView : UserControl, IRegionMemberLifetime, INavigationAware
    {

        static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        MediaFileBrowserViewModel MediaFileBrowserViewModel
        {
            get;
            set;
        }
    
        [ImportingConstructor]
        public MediaFileBrowserView(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            Microsoft.Practices.Prism.Regions.RegionManager.SetRegionManager(this, regionManager);

            InitializeComponent();

            //MediaFileBrowserViewModel = new MediaFileBrowserViewModel(MediaFileWatcher.Instance, regionManager);
            
            //DataContext = MediaFileBrowserViewModel;

            /*this.Loaded += (s, e) =>
            {
                MediaFileBrowserViewModel.navigateToMetaData();
                MediaFileBrowserViewModel.navigateToImageGrid();
            };*/
                                      
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

            if (MediaFileBrowserViewModel.CurrentViewModel == null)
            {
                MediaFileBrowserViewModel.navigateToMetaData();
                MediaFileBrowserViewModel.navigateToImageGrid();
            }

        }
    }
        
}
