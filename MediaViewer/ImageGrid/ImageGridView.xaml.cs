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
using MediaViewer.MediaPreview;
using MediaViewer.Utils.WPF;

namespace MediaViewer.ImageGrid
{
    /// <summary>
    /// Interaction logic for ImageGridControl.xaml
    /// </summary>
    public partial class ImageGridView : UserControl
    {

        int currentPage;

        public ImageGridView()
        {
           
            InitializeComponent();

            currentPage = 0;
                      
        }
        
        public int CurrentPage
        {
            get
            {
                return (currentPage);
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            ImageGridViewModel imageGridViewModel = (ImageGridViewModel)DataContext;

            CollectionView cv = (CollectionView)CollectionViewSource.GetDefaultView(imageGridViewModel.Locations);

            cv.MoveCurrentToPosition(5);
        }
    }
}
