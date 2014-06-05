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

namespace MediaViewer.MediaFileBrowser
{
    /// <summary>
    /// Interaction logic for MediaFileBrowserControl.xaml
    /// </summary>
    public partial class MediaFileBrowserView : UserControl
    {

        static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        MediaFileBrowserViewModel mediaFileBrowserViewModel;
        
     
        public MediaFileBrowserView()
        {
            InitializeComponent();
           
            DataContext = mediaFileBrowserViewModel = new MediaFileBrowserViewModel();
            mediaFileBrowserViewModel.VideoViewModel = videoPlayer.ViewModel;
            directoryPicker.DataContext = DataContext;

            imageViewer.DataContext = mediaFileBrowserViewModel.ImageViewModel;            
                
            metaDataView.DataContext = pager.DataContext = browserGrid.DataContext = mediaFileBrowserViewModel.PagedImageGridViewModel;      

            GlobalMessenger.Instance.Register("ToggleFullScreen", mediaFileBrowser_ToggleFullScreen);
            GlobalMessenger.Instance.Register("MediaFileBrowser_ShowBrowserGrid", mediaFileBrowser_ShowBrowserGrid);
            GlobalMessenger.Instance.Register<String>("MediaFileBrowser_ShowVideo", mediaFileBrowser_ShowVideo);
            GlobalMessenger.Instance.Register<String>("MediaFileBrowser_ShowImage", mediaFileBrowser_ShowImage);

            MediaFileModel.Watcher.MediaFileWatcher.Instance.MediaState.ItemIsSelectedChanged += new EventHandler((o, e) =>
            {              
                List<MediaFileItem> selected = MediaFileModel.Watcher.MediaFileWatcher.Instance.MediaState.getSelectedItemsUIState();

                if (selected.Count > 0)
                {
                    mediaFileBrowserViewModel.Title = mediaFileBrowserViewModel.BrowsePath + " - " + selected.Count.ToString() + " Item(s) Selected";
                }
                else
                {
                    mediaFileBrowserViewModel.Title = mediaFileBrowserViewModel.BrowsePath;
                }
              
            });
        }

        private void mediaFileBrowser_ShowBrowserGrid()
        {
            browserGrid.Visibility = Visibility.Visible;
            imageViewer.Visibility = Visibility.Collapsed;
            videoPlayer.Visibility = Visibility.Collapsed;
       
            imageOptionsGrid.Visibility = Visibility.Collapsed;        

            pager.DataContext = mediaFileBrowserViewModel.PagedImageGridViewModel;
            metaDataView.DataContext = mediaFileBrowserViewModel.PagedImageGridViewModel;
            videoPlayer.ViewModel.CloseCommand.DoExecute();

            GlobalMessenger.Instance.NotifyColleagues("MainWindow_SetTitle", mediaFileBrowserViewModel.Title);
        }

        private void mediaFileBrowser_ShowImage(string location)
        {
            browserGrid.Visibility = Visibility.Collapsed;
            imageViewer.Visibility = Visibility.Visible;
            videoPlayer.Visibility = Visibility.Collapsed;

            imageOptionsGrid.Visibility = Visibility.Visible;
                      
            metaDataView.DataContext = pager.DataContext = imageViewer.DataContext;      
        }

        private void mediaFileBrowser_ShowVideo(string location)
        {
            browserGrid.Visibility = Visibility.Collapsed;
            imageViewer.Visibility = Visibility.Collapsed;
            videoPlayer.Visibility = Visibility.Visible;
     
            metaDataView.DataContext = pager.DataContext = videoPlayer.DataContext;       
        }

        private void mediaFileBrowser_ToggleFullScreen()
        {
            Visibility mode;

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
        }
   
     
       
    }
        
}
