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

namespace MediaViewer.ImageGrid
{
    /// <summary>
    /// Interaction logic for ImageGridControl.xaml
    /// </summary>
    public partial class ImageGridView : UserControl
    {
        public ImageGridView()
        {           
            InitializeComponent();

            this.DataContextChanged += ImageGridView_DataContextChanged;
        }

        void ImageGridView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ImageGridViewModel)
            {
                ImageGridViewModel vm = e.NewValue as ImageGridViewModel;

                collectionInfo.DataContext = vm.MediaState;
            }
        }
        
        
       
    }
}
