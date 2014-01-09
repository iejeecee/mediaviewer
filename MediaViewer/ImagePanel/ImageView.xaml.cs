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
        public enum ScaleMode
        {
            NONE,
            FIT_TO_SCREEN,
            FIT_HEIGHT_TO_SCREEN,
            FIT_WIDTH_TO_SCREEN
        }

        ScaleMode mode;

        public ScaleMode Mode
        {
            get { return mode; }            
        }

        private bool isLeftMouseButtonDown;
        //private bool isModified;

        private Point mouseStartPosition;
        private Point scrollbarStartPosition;        

        PropertyObserver<ImageViewModel> imageViewModelObserver;

        public ImageView()
        {
            InitializeComponent();

            DataContextChanged += new DependencyPropertyChangedEventHandler(imageView_DataContextChanged);

            pictureBox.Stretch = Stretch.None;
            mode = ScaleMode.NONE;

            GlobalMessenger.Instance.Register<ScaleMode>("MainWindow_AutoScaleImageCheckBox_Click", new Action<ScaleMode>(setScaleMode));

            scrollViewer.SizeChanged += new SizeChangedEventHandler((s, e) =>
            {
                if (pictureBox.Source != null)
                {
                    ImageViewModel imageViewModel = (ImageViewModel)DataContext;

                    pictureBox.SetCurrentValue(Image.LayoutTransformProperty, 
                        new MatrixTransform(imageViewModel.Transform * getScaleMatrix(pictureBox.Source as BitmapImage).Matrix));
                }

            });
        }

        void imageView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null) return;
            
            ImageViewModel imageViewModel = (ImageViewModel)e.NewValue;
            pager.DataContext = imageViewModel;
         
            // Due to the device independent nature of WPF, images are 
            // scaled according to the dpi value stored in the image file.
            // But since nobody ever bothers to set this correctly 
            // undo the scaling and display the image at it's pixel by pixel size
            imageViewModelObserver = new PropertyObserver<ImageViewModel>(imageViewModel);
            imageViewModelObserver.RegisterHandler(m => m.Image,
                m =>
                {         
                    if (m.Image != null)
                    {
                                              
                        pictureBox.SetCurrentValue(Image.LayoutTransformProperty, getScaleMatrix(m.Image));
                        scrollViewer.ScrollToHorizontalOffset(0);
                        scrollViewer.ScrollToVerticalOffset(0);
                    }
                });

            // everytime the transform matrix is updated in the imageviewmodel
            // make sure to unscale the dpi as well
            imageViewModelObserver.RegisterHandler(m => m.Transform,
               m =>
               {                
                   if (m.Image != null)
                   {
                       pictureBox.SetCurrentValue(Image.LayoutTransformProperty, new MatrixTransform(m.Transform * getScaleMatrix(m.Image).Matrix));
                   }
               });               
        }

        public MatrixTransform getScaleMatrix(BitmapImage image)
        {
            Matrix scaleMatrix = new Matrix();

            if (Mode == ScaleMode.FIT_HEIGHT_TO_SCREEN)
            {
                double heightScale = scrollViewer.ActualHeight / image.Height;

                scaleMatrix.Scale(heightScale, heightScale);
            }
            else if (Mode == ScaleMode.FIT_WIDTH_TO_SCREEN)
            {
                double widthScale = scrollViewer.ActualWidth / image.Width;

                scaleMatrix.Scale(widthScale, widthScale);

            }
            else if (Mode == ScaleMode.FIT_TO_SCREEN)
            {
                double widthScale = scrollViewer.ActualWidth / image.Width;
                double heightScale = scrollViewer.ActualHeight / image.Height;

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

        void setScaleMode(ScaleMode mode)
        {
            this.mode = mode;

            if (pictureBox.Source != null)
            {
                ImageViewModel imageViewModel = (ImageViewModel)DataContext;

                pictureBox.SetCurrentValue(Image.LayoutTransformProperty,
                        new MatrixTransform(imageViewModel.Transform * getScaleMatrix(pictureBox.Source as BitmapImage).Matrix));
              
            }

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
            Mouse.OverrideCursor = Cursors.Hand;
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
