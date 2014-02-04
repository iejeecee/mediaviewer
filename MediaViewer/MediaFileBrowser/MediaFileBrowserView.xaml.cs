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

namespace MediaViewer.MediaFileBrowser
{
    /// <summary>
    /// Interaction logic for MediaFileBrowserControl.xaml
    /// </summary>
    public partial class MediaFileBrowserView : UserControl
    {

        static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        PagedImageGridViewModel pagedImageGridViewModel;       
     
        public MediaFileBrowserView()
        {
            InitializeComponent();
           
            DataContext = new MediaFileBrowserViewModel();
            directoryBrowser.DataContext = DataContext;

            pagedImageGridViewModel = new PagedImageGridViewModel(MediaFileWatcher.Instance.MediaState);
            imageGrid.DataContext = pagedImageGridViewModel;
            metaDataView.DataContext = pagedImageGridViewModel;
            pager.DataContext = pagedImageGridViewModel;
           
            // this is not the way it "should" be done, but fuck it
            // There is not going to be a directorybrowser without these viewmodels
            Loaded += new RoutedEventHandler((o,e) => {

                MediaFileBrowserViewModel mediaFileBrowserViewModel = (MediaFileBrowserViewModel)DataContext;
                if (mediaFileBrowserViewModel == null) return;             
                mediaFileBrowserViewModel.PagedImageGridViewModel = pagedImageGridViewModel;
                
            });
          
        }
       
    }
        
}
