using System;
using System.Collections.Generic;
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

namespace MediaViewer.ImagePanel
{
    /// <summary>
    /// Interaction logic for ImageWithPagerView.xaml
    /// </summary>
    public partial class ImageWithPagerView : UserControl
    {
        public ImageWithPagerView()
        {
            InitializeComponent();
            imageView.DataContextChanged += imageView_DataContextChanged;
            imagePagerView.DataContext = imageView.DataContext;
        }

        private void imageView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            imagePagerView.DataContext = e.NewValue;
        }
    }
}
