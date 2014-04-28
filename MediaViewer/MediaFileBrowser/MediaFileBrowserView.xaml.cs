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
using MediaViewer.DirectoryBrowser;
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
            directoryBrowser.DataContext = DataContext;

            imageViewer.DataContext = mediaFileBrowserViewModel.ImageViewModel;
            
            browserGrid.DataContext = mediaFileBrowserViewModel.PagedImageGridViewModel;
            pager.DataContext = mediaFileBrowserViewModel.PagedImageGridViewModel;
           
            metaDataView.DataContext = mediaFileBrowserViewModel.PagedImageGridViewModel;      

            GlobalMessenger.Instance.Register("ToggleFullScreen", mediaFileBrowser_ToggleFullScreen);
            GlobalMessenger.Instance.Register("MediaFileBrowser_ShowBrowserGrid", mediaFileBrowser_ShowBrowserGrid);
            GlobalMessenger.Instance.Register<String>("MediaFileBrowser_ShowVideo", mediaFileBrowser_ShowVideo);
            GlobalMessenger.Instance.Register<String>("MediaFileBrowser_ShowImage", mediaFileBrowser_ShowImage);
          
        }

        private void mediaFileBrowser_ShowBrowserGrid()
        {
            browserGrid.Visibility = Visibility.Visible;
            imageViewer.Visibility = Visibility.Hidden;
            videoPlayer.Visibility = Visibility.Hidden;
            pager.DataContext = mediaFileBrowserViewModel.PagedImageGridViewModel;
            videoPlayer.ViewModel.CloseCommand.DoExecute();
        }

        private void mediaFileBrowser_ShowImage(string location)
        {
            browserGrid.Visibility = Visibility.Hidden;
            imageViewer.Visibility = Visibility.Visible;
            videoPlayer.Visibility = Visibility.Hidden;
            ImageViewModel viewModel = (ImageViewModel)imageViewer.DataContext;
            viewModel.SelectedScaleMode = ImageViewModel.ScaleMode.AUTO;
            viewModel.LoadImageAsyncCommand.DoExecute(location);
            pager.DataContext = mediaFileBrowserViewModel.ImageViewModel;
        }

        private void mediaFileBrowser_ShowVideo(string location)
        {
            browserGrid.Visibility = Visibility.Hidden;
            imageViewer.Visibility = Visibility.Hidden;
            videoPlayer.Visibility = Visibility.Visible;
            videoPlayer.ViewModel.OpenCommand.DoExecute(location);
            videoPlayer.ViewModel.PlayCommand.DoExecute();
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
