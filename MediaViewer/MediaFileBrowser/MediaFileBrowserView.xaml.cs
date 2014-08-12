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
            directoryPicker.DataContext = DataContext;
                                     
            metaDataView.DataContext = mediaFileBrowserViewModel.ImageGridViewModel;
            
            pager.DataContext = mediaFileBrowserViewModel.ImageGridViewModel;

            PropertyChangedEventManager.AddHandler(mediaFileBrowserViewModel, selectedViewModelChanged, "CurrentViewModel");
                               
        }

        private void selectedViewModelChanged(object sender, PropertyChangedEventArgs e)
        {
            pager.DataContext = mediaFileBrowserViewModel.CurrentViewModel;
            metaDataView.DataContext = mediaFileBrowserViewModel.CurrentViewModel;

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
            //imageOptionsGrid.Visibility = mode;
        }

        private void selectedMediaDataGridView_RowDoubleClick(object sender, MediaFileItem e)
        {
            mediaFileBrowserViewModel.ExpandCommand.DoExecute(e);
        }
        
       
    }
        
}
