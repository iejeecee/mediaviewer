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
using MediaViewer.MediaPreview;
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

        PagedImageGridViewModel partialImageGridViewModel;
        DirectoryBrowserViewModel directoryBrowserViewModel;
        PagerViewModel pagerViewModel;

        public MediaFileBrowserView()
        {
            InitializeComponent();
       
            directoryBrowserViewModel = new DirectoryBrowserViewModel();
            directoryBrowser.DataContext = directoryBrowserViewModel;

            // recieve message when a path node is selected          
            GlobalMessenger.Instance.Register<PathModel>("PathModel_IsSelected", new Action<PathModel>(browsePath_IsSelected));

            partialImageGridViewModel = new PagedImageGridViewModel();
            imageGrid.DataContext = partialImageGridViewModel;

            pagerViewModel = new PagerViewModel();
            pager.DataContext = pagerViewModel;

            pagerViewModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler((s, e) =>
            {

                if (e.PropertyName.Equals("CurrentPage"))
                {
                    partialImageGridViewModel.SetPageCommand.DoExecute(pagerViewModel.CurrentPage);
                }
            });
           
            // this is not the way it "should" be done, but fuck it
            // There is not going to be a directorybrowser without these viewmodels
            Loaded += new RoutedEventHandler((o,e) => {

                MediaFileBrowserViewModel mediaFileBrowserViewModel = (MediaFileBrowserViewModel)DataContext;
                if (mediaFileBrowserViewModel == null) return;
                mediaFileBrowserViewModel.PagerViewModel = pagerViewModel;
                mediaFileBrowserViewModel.PartialImageGridViewModel = partialImageGridViewModel;
                mediaFileBrowserViewModel.DirectoryBrowserViewModel = directoryBrowserViewModel;
            });
          
        }

        private void browsePath_IsSelected(PathModel path)
        {

            if (path.GetType() == typeof(DummyPathModel)) return;

            string fullPath = path.getFullPath();

            MediaFileBrowserViewModel mediaFileBrowserViewModel = (MediaFileBrowserViewModel)DataContext;

            mediaFileBrowserViewModel.BrowsePath = fullPath;
        }
    }
        
}
