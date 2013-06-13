using MvvmFoundation.Wpf;
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

namespace MediaViewer.ImageModel
{
    /// <summary>
    /// Interaction logic for ImageView.xaml
    /// </summary>
    public partial class ImageView : UserControl
    {
        static BitmapImage loadingImage = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/loading1.gif"));

        public ImageView()
        {
            InitializeComponent();

            DataContextChanged += new DependencyPropertyChangedEventHandler(imageView_DataContextChanged);
           
        }

        void imageView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == null && e.NewValue != null)
            {
                ImageViewModel imageViewModel = (ImageViewModel)e.NewValue;

                imageViewModel.LoadImageAsyncCommand.Executing += new MvvmFoundation.Wpf.Delegates.CancelCommandEventHandler(loadImage_Executing);
            }
        }

        void loadImage_Executing(object sender, CancelCommandEventArgs e)
        {
            
            pictureBox.SetCurrentValue(Image.SourceProperty, loadingImage); 
        }
    }
}
