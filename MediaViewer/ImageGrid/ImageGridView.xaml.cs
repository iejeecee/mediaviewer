// intresting stuff:
// Lazy<T> for lazy initialization
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using MediaViewer.Utils.WPF;
using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.UserControls.Layout;

namespace MediaViewer.ImageGrid
{
    /// <summary>
    /// Interaction logic for ImageGridControl.xaml
    /// </summary>
    public partial class ImageGridView : UserControl
    {
        VirtualizingTilePanel panel;

        public ImageGridView()
        {           
            InitializeComponent();

            panel = null;

            DataContextChanged += ImageGridView_DataContextChanged;
        }

        void ImageGridView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ImageGridViewModel)
            {
                ImageGridViewModel imageGridViewModel = e.NewValue as ImageGridViewModel;
                WeakEventManager<ImageGridViewModel,EventArgs>.AddHandler(imageGridViewModel,"Cleared",imageGridViewModel_Cleared);               
            } 

        }

        void imageGridViewModel_Cleared(object sender, EventArgs e)
        {
            if (panel != null)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() => panel.ScrollOwner.ScrollToVerticalOffset(0)));
            }
        }

        private void virtualizingTilePanel_Loaded(object sender, RoutedEventArgs e)
        {
            panel = sender as VirtualizingTilePanel;
            
        }

                           
    }
}
