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

namespace MediaViewer.ImagePanel
{
    /// <summary>
    /// Interaction logic for ImageView.xaml
    /// </summary>
    public partial class ImageView : UserControl
    {
    
        private bool isLeftMouseButtonDown;
        //private bool isModified;

        private Point mouseStartPosition;
        private Point scrollbarStartPosition;        

        public ImageView()
        {
            InitializeComponent();

            DataContextChanged += new DependencyPropertyChangedEventHandler(imageView_DataContextChanged);

            pictureBox.Stretch = Stretch.None;
   
            scrollViewer.SizeChanged += new SizeChangedEventHandler((s, e) =>
            {
                if (pictureBox.Source != null)
                {
                    ImageViewModel imageViewModel = (ImageViewModel)DataContext;

                    pictureBox.SetCurrentValue(Image.LayoutTransformProperty,
                        new MatrixTransform(imageViewModel.Transform * getScaleMatrix(pictureBox.Source as BitmapImage, imageViewModel).Matrix));
                }

            });
        }

        void imageView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(e.NewValue is ImageViewModel)) return;
            
            ImageViewModel imageViewModel = (ImageViewModel)e.NewValue;

            WeakEventManager<ImageViewModel,  System.ComponentModel.PropertyChangedEventArgs>.RemoveHandler(
                   imageViewModel, "PropertyChanged", imageViewModel_PropertyChanged);

            WeakEventManager<ImageViewModel, System.ComponentModel.PropertyChangedEventArgs>.AddHandler(
                   imageViewModel, "PropertyChanged", imageViewModel_PropertyChanged);
                        
        }

        private void imageViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

            ImageViewModel imageViewModel = (ImageViewModel)sender;

            // Due to the device independent nature of WPF, images are 
            // scaled according to the dpi value stored in the image file.
            // But since nobody ever bothers to set this correctly 
            // undo the scaling and display the image at it's pixel by pixel size
            if (e.PropertyName.Equals("Image"))
            {
                if (imageViewModel.Image != null)
                {
                    pictureBox.SetCurrentValue(Image.LayoutTransformProperty, new MatrixTransform(imageViewModel.Transform * getScaleMatrix(imageViewModel.Image, imageViewModel).Matrix));
                    scrollViewer.ScrollToHorizontalOffset(0);
                    scrollViewer.ScrollToVerticalOffset(0);
                }
            }
            else if (e.PropertyName.Equals("Transform"))
            {
                // everytime the transform matrix is updated in the imageviewmodel
                // make sure to unscale the dpi as well
                if (imageViewModel.Image != null)
                {
                    pictureBox.SetCurrentValue(Image.LayoutTransformProperty, new MatrixTransform(imageViewModel.Transform * getScaleMatrix(imageViewModel.Image, imageViewModel).Matrix));
                }
            }
            else if (e.PropertyName.Equals("SelectedScaleMode"))
            {
                // update image whenever scalemode is changed
                if (imageViewModel.Image != null)
                {
                    pictureBox.SetCurrentValue(Image.LayoutTransformProperty, new MatrixTransform(imageViewModel.Transform * getScaleMatrix(imageViewModel.Image, imageViewModel).Matrix));
                }
            }
        }

        public MatrixTransform getScaleMatrix(BitmapImage image, ImageViewModel vm)
        {
          
            Matrix scaleMatrix = new Matrix();

            if (vm.SelectedScaleMode == ImageViewModel.ScaleMode.FIT_HEIGHT)
            {
                double heightScale = scrollViewer.ActualHeight / image.Height;

                if (image.Width * heightScale > scrollViewer.ActualWidth)
                {
                    heightScale = (scrollViewer.ActualHeight - SystemParameters.ScrollHeight) / image.Height; 
                }

                scaleMatrix.Scale(heightScale, heightScale);
            }
            else if (vm.SelectedScaleMode == ImageViewModel.ScaleMode.FIT_WIDTH)
            {
                double widthScale = scrollViewer.ActualWidth / image.Width;

                if (image.Height * widthScale > scrollViewer.ActualHeight)
                {
                    widthScale = (scrollViewer.ActualWidth - SystemParameters.ScrollWidth) / image.Width;
                }

                scaleMatrix.Scale(widthScale, widthScale);

            }
            else if (vm.SelectedScaleMode == ImageViewModel.ScaleMode.AUTO)
            {
                double widthScale = (scrollViewer.ActualWidth - 3) / image.Width;
                double heightScale = (scrollViewer.ActualHeight - 3) / image.Height;

                double scale = Math.Min(widthScale, heightScale);

                scaleMatrix.Scale(scale, scale);

            }
            else
            {
                var source = PresentationSource.FromVisual(pictureBox);
                Matrix transformToDevice = source.CompositionTarget.TransformToDevice;
                transformToDevice.Invert();
                Size actualSize = (Size)transformToDevice.Transform(new Vector(image.PixelWidth, image.PixelHeight));
                double scale = actualSize.Width / image.Width;

                scaleMatrix.Scale(scale, scale);

            }

            return (new MatrixTransform(scaleMatrix));
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
            if (e.ClickCount == 2)
            {             
                GlobalMessenger.Instance.NotifyColleagues("ToggleFullScreen");
            }
            else if (e.ClickCount == 1)
            {
                isLeftMouseButtonDown = true;
                mouseStartPosition = e.GetPosition(scrollViewer);
                scrollbarStartPosition.X = scrollViewer.HorizontalOffset;
                scrollbarStartPosition.Y = scrollViewer.VerticalOffset;
                Mouse.OverrideCursor = Cursors.Hand;
            }
        }

        private void gridContainer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isLeftMouseButtonDown = false;
            Mouse.OverrideCursor = null;
        }

        private void gridContainer_MouseLeave(object sender, MouseEventArgs e)
        {
            isLeftMouseButtonDown = false;
            Mouse.OverrideCursor = null;
        }

      
    }
}
