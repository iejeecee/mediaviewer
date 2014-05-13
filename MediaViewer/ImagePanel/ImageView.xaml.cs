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
        const int MAX_IMAGE_SIZE_PIXELS = 8096;

        private bool isLeftMouseButtonDown;
        //private bool isModified;

        private Point mouseStartPosition;
        private Point scrollbarStartPosition;

        double fitHeightScale;
        double fitWidthScale;    
        double autoFitScale;  
        double normalScale;

        public ImageView()
        {
            InitializeComponent();

            DataContextChanged += new DependencyPropertyChangedEventHandler(imageView_DataContextChanged);

            pictureBox.Stretch = Stretch.None;

            scrollViewer.SizeChanged += new SizeChangedEventHandler(scrollViewer_SizeChanged);

           
        }

        private void scrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (pictureBox.Source != null)
            {
                ImageViewModel vm = (ImageViewModel)DataContext;

                calcScale(pictureBox.Source as BitmapImage, vm);

                if (vm.SelectedScaleMode == ImageViewModel.ScaleMode.AUTO ||
                    vm.SelectedScaleMode == ImageViewModel.ScaleMode.FIT_HEIGHT ||
                    vm.SelectedScaleMode == ImageViewModel.ScaleMode.FIT_WIDTH ||
                    vm.SelectedScaleMode == ImageViewModel.ScaleMode.RELATIVE) 
                {
                    setScale(vm, false);
                }

            }
        }

        void imageView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(e.NewValue is ImageViewModel)) return;
            
            ImageViewModel imageViewModel = (ImageViewModel)e.NewValue;

            WeakEventManager<ImageViewModel,  System.ComponentModel.PropertyChangedEventArgs>.RemoveHandler(
                   imageViewModel, "PropertyChanged", imageViewModel_PropertyChanged);

            WeakEventManager<ImageViewModel, System.ComponentModel.PropertyChangedEventArgs>.AddHandler(
                   imageViewModel, "PropertyChanged", imageViewModel_PropertyChanged);

            WeakEventManager<ImageViewModel, EventArgs>.RemoveHandler(
                 imageViewModel, "SetNormalScaleEvent", imageViewModel_SetNormalScale);

            WeakEventManager<ImageViewModel, EventArgs>.AddHandler(
                  imageViewModel, "SetNormalScaleEvent", imageViewModel_SetNormalScale);

            WeakEventManager<ImageViewModel, EventArgs>.RemoveHandler(
            imageViewModel, "SetBestFitScaleEvent", imageViewModel_SetBestFitScale);

            WeakEventManager<ImageViewModel, EventArgs>.AddHandler(
                  imageViewModel, "SetBestFitScaleEvent", imageViewModel_SetBestFitScale);
                        
        }

        private void imageViewModel_SetBestFitScale(object sender, EventArgs e)
        {
            ImageViewModel imageViewModel = (ImageViewModel)sender;
            imageViewModel.Scale = autoFitScale;
        }

        private void imageViewModel_SetNormalScale(object sender, EventArgs e)
        {
            ImageViewModel imageViewModel = (ImageViewModel)sender;
            imageViewModel.Scale = normalScale;
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
                    scrollViewer.ScrollToHorizontalOffset(0);
                    scrollViewer.ScrollToVerticalOffset(0);

                    calcScale(imageViewModel.Image, imageViewModel);
                    setScale(imageViewModel, true);
                }
            }
            else if (e.PropertyName.Equals("SelectedScaleMode"))
            {
                setScale(imageViewModel, true);
            }
            
        }
       
        private void calcScale(BitmapImage image, ImageViewModel vm)
        {
            // fitheightscale
            if (image == null) return;

            fitHeightScale = fitWidthScale = 1;

            double heightScale = scrollViewer.ActualHeight / image.Height;

            if (image.Width * heightScale > scrollViewer.ActualWidth)
            {
                fitHeightScale = (scrollViewer.ActualHeight - SystemParameters.ScrollHeight) / image.Height;             
            }

            // fitwidthscale
            double widthScale = scrollViewer.ActualWidth / image.Width;

            if (image.Height * fitWidthScale > scrollViewer.ActualHeight)
            {
                fitWidthScale = (scrollViewer.ActualWidth - SystemParameters.ScrollWidth) / image.Width;              
            }

            // autofitscale
            widthScale = (scrollViewer.ActualWidth - 3) / image.Width;
            heightScale = (scrollViewer.ActualHeight - 3) / image.Height;

            autoFitScale = Math.Min(widthScale, heightScale);

            // normalscale
            var source = PresentationSource.FromVisual(pictureBox);
            Matrix transformToDevice = source.CompositionTarget.TransformToDevice;
            transformToDevice.Invert();
            Size actualSize = (Size)transformToDevice.Transform(new Vector(image.PixelWidth, image.PixelHeight));
            normalScale = actualSize.Width / image.Width;

            // max/minscale
            double maxWidth = ((double)MAX_IMAGE_SIZE_PIXELS) / image.PixelWidth;
            double maxHeight = ((double)MAX_IMAGE_SIZE_PIXELS) / image.PixelHeight;

            vm.MinScale = Math.Min(autoFitScale, normalScale);
            vm.MaxScale = normalScale * Math.Min(maxWidth, maxHeight);
        }

        void setScale(ImageViewModel vm, bool init)
        {
            switch (vm.SelectedScaleMode)
            {
                case ImageViewModel.ScaleMode.NONE:
                    if (init == true)
                    {
                        vm.Scale = normalScale;
                    }
                    break;
                case ImageViewModel.ScaleMode.RELATIVE:                    
                    if (init == true)
                    {
                        vm.Scale = autoFitScale;
                    }                                                                                   
                    break;
                case ImageViewModel.ScaleMode.AUTO:
                    vm.Scale = autoFitScale;
                    break;
                case ImageViewModel.ScaleMode.FIT_HEIGHT:
                    vm.Scale = fitHeightScale;
                    break;
                case ImageViewModel.ScaleMode.FIT_WIDTH:
                    vm.Scale = fitWidthScale;
                    break;
                default:
                    break;
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
