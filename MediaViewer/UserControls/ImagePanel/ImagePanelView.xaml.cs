using MediaViewer.Infrastructure.Global.Events;
using MediaViewer.Infrastructure.Logging;
using MediaViewer.Infrastructure.Utils;
using MediaViewer.Model.Utils;
using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using VideoLib;

namespace MediaViewer.UserControls.ImagePanel
{
    /// <summary>
    /// Interaction logic for ImagePanelView.xaml
    /// </summary>
  

    public partial class ImagePanelView : UserControl
    {
       
        const int MAX_IMAGE_SIZE_PIXELS_X = 8096;
        const int MAX_IMAGE_SIZE_PIXELS_Y = 16192;

        private bool isLeftMouseButtonDown;
        //private bool isModified;

        private Point mouseStartPosition;
        private Point scrollbarStartPosition;

        Task<BitmapImage> ImageLoadingTask { get; set; }
        CancellationTokenSource TokenSource { get; set; }
        CancellationToken Token {get; set;}

        public ImagePanelView()
        {
            InitializeComponent();

            ImageLoadingTask = null;
   
        }

        public String Location
        {
            get { return (String)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Location.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LocationProperty =
            DependencyProperty.Register("Location", typeof(String), typeof(ImagePanelView), new PropertyMetadata(null,locationChangedCallback));

        private static async void locationChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImagePanelView view = (ImagePanelView)d;

            // cancel previously running load requests 
            if (view.ImageLoadingTask != null && !view.ImageLoadingTask.IsCompleted)
            {               
                view.TokenSource.Cancel();
                try
                {
                    await view.ImageLoadingTask;
                }
                catch (Exception)
                {

                }
            } 
         
            view.TokenSource = new CancellationTokenSource();
            view.Token = view.TokenSource.Token;

            String location = (String)e.NewValue;

            try
            {
                view.IsLoading = true;
                //view.ImageLoadingTask = Task<BitmapSource>.Factory.StartNew(() => loadImage(location, view.Token), view.Token);
                view.ImageLoadingTask = Task<BitmapImage>.Factory.StartNew(() => ImageUtils.loadImage(location, view.Token), view.Token);

                await view.ImageLoadingTask;
                         
                view.pictureBox.Source = view.ImageLoadingTask.Result;
                view.scrollViewer.ScrollToHorizontalOffset(0);
                view.scrollViewer.ScrollToVerticalOffset(0);
                view.setScaleMode();
            }
            catch (OperationCanceledException)
            {
                Logger.Log.Info("Cancelled loading image:" + (String)location);
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error loading image:" + (String)location, ex);
                MessageBox.Show("Error loading image: " + location + "\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);               
            }
            finally
            {
                view.IsLoading = false;
            }
        }

        /*static BitmapSource loadImage(string location, CancellationToken token)
        {
            VideoLib.MediaProbe mediaProbe = new VideoLib.MediaProbe();

            try
            {
                mediaProbe.open(location, token);
                List<MediaThumb> image = mediaProbe.grabThumbnails(0, 0, 0, 1, 0, token, 0, null);

                if (image.Count > 0)
                {
                    return (image[0].Thumb);
                }
            }
            finally
            {
                mediaProbe.close();
                mediaProbe.Dispose();
            }

            return (null);
        } */       

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsLoading.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(ImagePanelView), new PropertyMetadata(false, isLoadingChanged));

        private static void isLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImagePanelView view = (ImagePanelView)d;

            bool isLoading = (bool)e.NewValue;

            if (isLoading)
            {
                view.scrollViewer.Visibility = Visibility.Hidden;
                view.loadingView.VisibilityAndAnimate = Visibility.Visible;
            }
            else
            {
                view.scrollViewer.Visibility = Visibility.Visible;
                view.loadingView.VisibilityAndAnimate = Visibility.Collapsed;
            }

        }

        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Scale.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), typeof(ImagePanelView), new FrameworkPropertyMetadata(1.0,FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ,scaleChanged));

        private static void scaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImagePanelView view = (ImagePanelView)d;

            view.setLayoutTransform();
                        
        }

        public double RotationDegrees
        {
            get { return (double)GetValue(RotationDegreesProperty); }
            set { SetValue(RotationDegreesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Rotation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RotationDegreesProperty =
            DependencyProperty.Register("RotationDegrees", typeof(double), typeof(ImagePanelView), new PropertyMetadata(0.0, rotationDegreesChanged));

        private static void rotationDegreesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImagePanelView view = (ImagePanelView)d;

            if (view.ImageScaleMode != ScaleMode.UNSCALED)
            {
                view.setScaleMode();
            }
            else
            {
                view.setLayoutTransform();
            }
        }

        public bool FlipX
        {
            get { return (bool)GetValue(FlipXProperty); }
            set { SetValue(FlipXProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FlipX.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FlipXProperty =
            DependencyProperty.Register("FlipX", typeof(bool), typeof(ImagePanelView), new PropertyMetadata(false, flipXChanged));

        private static void flipXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImagePanelView view = (ImagePanelView)d;

            view.setLayoutTransform();
        }

        public bool FlipY
        {
            get { return (bool)GetValue(FlipYProperty); }
            set { SetValue(FlipYProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FlipY.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FlipYProperty =
            DependencyProperty.Register("FlipY", typeof(bool), typeof(ImagePanelView), new PropertyMetadata(false, flipYChanged));

        private static void flipYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImagePanelView view = (ImagePanelView)d;

            view.setLayoutTransform();
        }

        void setLayoutTransform()
        {
            Matrix transform = Matrix.Identity;

            if (FlipX == true)
            {
                transform.M11 *= -1;
            }

            if (FlipY == true)
            {
                transform.M22 *= -1;
            }

            transform.Scale(Scale, Scale);
            transform.Rotate(RotationDegrees);

            pictureBox.LayoutTransform = new MatrixTransform(transform);
        }

        public ScaleMode ImageScaleMode
        {
            get { return (ScaleMode)GetValue(ImageScaleModeProperty); }
            set { SetValue(ImageScaleModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageScaleMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageScaleModeProperty =
            DependencyProperty.Register("ImageScaleMode", typeof(ScaleMode), typeof(ImagePanelView), new PropertyMetadata(ScaleMode.UNSCALED, scaleModeChanged));

        private static void scaleModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImagePanelView view = (ImagePanelView)d;

            view.setScaleMode();

        }

        void setScaleMode()
        {
            BitmapImage image = pictureBox.Source as BitmapImage;

            if (image == null) return;

            double unscaled = getUnscaledScale();
            double fitHeightAndWidthScale = getFitWidthAndHeightScale();

            MinScale = Math.Min(unscaled, fitHeightAndWidthScale);
            MaxScale = ((double)MAX_IMAGE_SIZE_PIXELS_X) / image.PixelWidth;

            switch (ImageScaleMode)
            {
                case ScaleMode.UNSCALED:
                    {
                        Scale = unscaled;
                        break;
                    }
                case ScaleMode.FIT_HEIGHT_AND_WIDTH:
                    {
                        Scale = fitHeightAndWidthScale;
                        break;
                    }
                case ScaleMode.FIT_HEIGHT:
                    Scale = (scrollViewer.ActualHeight - SystemParameters.ScrollHeight) / getTransformedExtends().Y;
                    break;
                case ScaleMode.FIT_WIDTH:
                    Scale = (scrollViewer.ActualWidth - SystemParameters.ScrollWidth) / getTransformedExtends().X;
                    break;                    
                default:
                    break;
            }

            
        }

        double getUnscaledScale()
        {
            BitmapImage image = pictureBox.Source as BitmapImage;

            var source = PresentationSource.FromVisual(pictureBox);

            if (source != null)
            {
                Matrix transformToDevice = source.CompositionTarget.TransformToDevice;
                transformToDevice.Invert();
                Size actualSize = (Size)transformToDevice.Transform(new Vector(image.PixelWidth, image.PixelHeight));
                return (actualSize.Width / image.Width);
            }
            else
            {
                return (1);
            }
        }

        Vector getTransformedExtends()
        {
            BitmapImage image = pictureBox.Source as BitmapImage;

            Matrix rotation = new Matrix();
            rotation.Rotate(RotationDegrees);

            Vector[] imageRect = new Vector[4];

            double x = image.Width / 2;
            double y = image.Height / 2;

            // calculate bounding box of (potentially) rotated image
            imageRect[0] = new Vector(-x, y);
            imageRect[1] = new Vector(x, y);
            imageRect[2] = new Vector(-x, -y);
            imageRect[3] = new Vector(x, -y);

            double xMin = Double.MaxValue, xMax = double.MinValue, yMin = double.MaxValue, yMax = double.MinValue;

            for (int i = 0; i < 4; i++)
            {
                imageRect[i] = rotation.Transform(imageRect[i]);

                if (imageRect[i].X < xMin) xMin = imageRect[i].X;
                if (imageRect[i].X > xMax) xMax = imageRect[i].X;
                if (imageRect[i].Y < yMin) yMin = imageRect[i].Y;
                if (imageRect[i].Y > yMax) yMax = imageRect[i].Y;
            }

            Vector extends = new Vector(xMax - xMin, yMax - yMin);

            return (extends);
        }

        double getFitWidthAndHeightScale()
        {
            Vector extends = getTransformedExtends();

            double widthScale = (scrollViewer.ActualWidth) / extends.X;
            double heightScale = (scrollViewer.ActualHeight) / extends.Y;

            return(Math.Min(widthScale, heightScale));
        }

        private void gridContainer_PreviewMouseMove(object sender, MouseEventArgs e)
        {

            if (isLeftMouseButtonDown != true) return;
            
            Point mousePos = e.GetPosition(scrollViewer);

            Vector ratio = new Vector(scrollViewer.ExtentWidth / scrollViewer.ViewportWidth,
                scrollViewer.ExtentHeight / scrollViewer.ViewportHeight);

            Vector delta = mouseStartPosition - mousePos;

            double offsetX = scrollbarStartPosition.X + delta.X * ratio.X;
            double offsetY = scrollbarStartPosition.Y + delta.Y * ratio.Y;

            scrollViewer.ScrollToHorizontalOffset(offsetX);
            scrollViewer.ScrollToVerticalOffset(offsetY);
            

        }

        private void gridContainer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
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

        private void scrollViewer_SizeChanged(object sender, EventArgs e)
        {
            if (ImageScaleMode == ScaleMode.FIT_HEIGHT_AND_WIDTH)
            {
                setScaleMode();                               
            }
        }

        public bool IsZoomable
        {
            get { return (bool)GetValue(IsZoomableProperty); }
            set { SetValue(IsZoomableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsZoomable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsZoomableProperty =
            DependencyProperty.Register("IsZoomable", typeof(bool), typeof(ImagePanelView), new PropertyMetadata(true));
        
        private void gridContainer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (IsZoomable == false) return;

            e.Handled = true;

            // zoom to mouse pointer
            double scaleDelta = Math.Sign(e.Delta) * 0.1;

            if (Scale + scaleDelta > MaxScale)
            {
                scaleDelta = MaxScale - Scale;
            }
            else if (Scale + scaleDelta < MinScale)
            {
                scaleDelta = MinScale - Scale;
                
            }

            Point mouse = getMouseLocation(e);

            Scale = Scale + scaleDelta;
           
            Vector oldPos = new Vector(mouse.X, mouse.Y);
            Vector newPos = oldPos * (1 + scaleDelta);

            Vector result = newPos - oldPos;
                    
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + result.X);
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + result.Y);

            if (Scale <= getFitWidthAndHeightScale())
            {
                scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            }
            else
            {
                scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
        }

        Point getMouseLocation(MouseWheelEventArgs e)
        {
            Point mouse = e.GetPosition(pictureBox);
            /*BitmapImage image = pictureBox.Source as BitmapImage;

            System.Drawing.RectangleF screenRect = new System.Drawing.RectangleF(0,0,(float)(scrollViewer.ActualWidth + scrollViewer.ScrollableWidth),(float)(scrollViewer.ActualHeight + scrollViewer.ScrollableHeight));
            System.Drawing.RectangleF imageRect = new System.Drawing.RectangleF(0, 0, (float)(pictureBox.ActualWidth * Scale), (float)(pictureBox.ActualHeight * Scale));

            System.Drawing.RectangleF imageCenterRect = ImageUtils.centerRectangle(screenRect, imageRect);

            mouse.X += imageCenterRect.X;
            mouse.Y += imageCenterRect.Y;*/

            return (mouse);
        }


        public double MaxScale
        {
            get { return (double)GetValue(MaxScaleProperty); }
            set { SetValue(MaxScaleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxScale.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxScaleProperty =
            DependencyProperty.Register("MaxScale", typeof(double), typeof(ImagePanelView), new FrameworkPropertyMetadata(1.0,FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double MinScale
        {
            get { return (double )GetValue(MinScaleProperty); }
            set { SetValue(MinScaleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinScale.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinScaleProperty =
            DependencyProperty.Register("MinScale", typeof(double ), typeof(ImagePanelView), new FrameworkPropertyMetadata(1.0,FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
 
    }
}
