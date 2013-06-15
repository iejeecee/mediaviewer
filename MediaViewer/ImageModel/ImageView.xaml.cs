//http://bjorn.kuiper.nu/2011/05/11/tips-tricks-listening-to-dependency-property-change-notifications-of-a-given-element/
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
        static BitmapImage errorImage = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/error.png"));
        static BitmapImage loadingImage = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/loading1.gif"));

        private bool isLeftMouseButtonDown;
        //private bool isModified;

        private Point mouseStartPosition;
        private Point scrollbarStartPosition;        

        PropertyObserver<ImageViewModel> imageViewModelObserver;

        public ImageView()
        {
            InitializeComponent();

            DataContextChanged += new DependencyPropertyChangedEventHandler(imageView_DataContextChanged);
                     
        }

        void imageView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null) return;
            
            ImageViewModel imageViewModel = (ImageViewModel)e.NewValue;

            // notify when a loadimage command is executed
            // so a temporary loading image can be shown
            imageViewModel.LoadImageAsyncCommand.Executing += new MvvmFoundation.Wpf.Delegates.CancelCommandEventHandler(loadImage_Executing);

            // Due to the device independent nature of WPF, images are 
            // scaled according to the dpi value stored in the image file.
            // But since nobody ever bothers to set this correctly we
            // undo the scaling and display the image at it's pixel by pixel size
            imageViewModelObserver = new PropertyObserver<ImageViewModel>(imageViewModel);
            imageViewModelObserver.RegisterHandler(m => m.Image,
                m =>
                {         
                    if (m.Image != null)
                    {
                        pictureBox.SetCurrentValue(Image.LayoutTransformProperty, getDefaultScaleMatrix(m.Image));
                        scrollViewer.ScrollToHorizontalOffset(0);
                        scrollViewer.ScrollToVerticalOffset(0);
                    }
                });

            // everytime the transform matrix is updated in the imageviewmodel
            // make sure we apply unscale the dpi as well
            imageViewModelObserver.RegisterHandler(m => m.Transform,
               m =>
               {
                
                   if (m.Image != null)
                   {
                       pictureBox.SetCurrentValue(Image.LayoutTransformProperty, new MatrixTransform(m.Transform * getDefaultScaleMatrix(m.Image).Matrix));
                   }
               });               
        }

        public MatrixTransform getDefaultScaleMatrix(BitmapImage image)
        {
            var source = PresentationSource.FromVisual(pictureBox);
            Matrix transformToDevice = source.CompositionTarget.TransformToDevice;
            transformToDevice.Invert();
            var actualSize = (Size)transformToDevice.Transform(new Vector(image.PixelWidth, image.PixelHeight));
            double scale = actualSize.Width / image.Width;

            Matrix scaleMatrix = new Matrix();
            scaleMatrix.Scale(scale, scale);

            return (new MatrixTransform(scaleMatrix));
        }



        void loadImage_Executing(object sender, CancelCommandEventArgs e)
        {
            
            pictureBox.SetCurrentValue(Image.SourceProperty, loadingImage); 
        }

        private void gridContainer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
           
            if (isLeftMouseButtonDown == true)
            {
                Point mousePos = e.GetPosition(scrollViewer);

                Vector ratio = new Vector(scrollViewer.ExtentWidth / scrollViewer.ViewportWidth,
                    scrollViewer.ExtentHeight / scrollViewer.ViewportHeight);

                Vector delta = mouseStartPosition - mousePos;

                double offsetX = scrollbarStartPosition.X + delta.X * ratio.X;
                double offsetY = scrollbarStartPosition.Y + delta.Y * ratio.Y;

                scrollViewer.ScrollToHorizontalOffset(offsetX);
                scrollViewer.ScrollToVerticalOffset(offsetY);
            }

        }

        private void gridContainer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isLeftMouseButtonDown = true;
            mouseStartPosition = e.GetPosition(scrollViewer);
            scrollbarStartPosition.X = scrollViewer.HorizontalOffset;
            scrollbarStartPosition.Y = scrollViewer.VerticalOffset;
        }

        private void gridContainer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isLeftMouseButtonDown = false;
        }

        private void gridContainer_MouseLeave(object sender, MouseEventArgs e)
        {
            isLeftMouseButtonDown = false;
        }

      
    }
}
