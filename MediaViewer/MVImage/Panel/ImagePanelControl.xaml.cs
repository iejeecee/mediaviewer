using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
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
using MediaViewer.MediaFileObject;
using MediaViewer.Utils;

namespace MediaViewer.MVImage.Panel
{
    /// <summary>
    /// Interaction logic for ImagePanelControl.xaml
    /// </summary>
    public partial class ImagePanelControl : UserControl, INotifyPropertyChanged
    {

        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Matrix transform;
        private double scaleX, scaleY;

        public double ScaleY
        {
            get { return scaleY; }
            set
            {
                scaleY = value;
                Transform = buildTransform();
            }
        }

        public double ScaleX
        {
            get { return scaleX; }
            set
            {
                scaleX = value;
                Transform = buildTransform();
            }
        }
        private bool flipX, flipY;

        public bool FlipY
        {
            get { return flipY; }
            set
            {
                flipY = value;
                Transform = buildTransform();
            }
        }

        public bool FlipX
        {
            get { return flipX; }
            set
            {
                flipX = value;
                Transform = buildTransform();
            }
        }
        private double rotation;

        public double Rotation
        {
            get { return rotation; }
            set
            {
                rotation = value;
                Transform = buildTransform();
            }
        }

        private MediaFileFactory mediaFileFactory;
        private MediaFile media;

        private BitmapImage sourceImage;
        private ImageFormat imageFormat;

        private DisplayModeState displayMode;

        private bool isLeftMouseButtonDown;
        private bool isModified;

        private Point mouseStartPosition;
        private Point scrollbarStartPosition;

        private delegate void loadImageDelegate(MediaFile media);

        public event EventHandler<EventArgs> LoadImageFinished;

        public ImagePanelControl()
        {
            transform = Matrix.Identity;
            scaleX = scaleY = 1;
            rotation = 0;
            flipX = false;
            flipY = false;

            InitializeComponent();

            mediaFileFactory = new MediaFileFactory();
            mediaFileFactory.OpenFinished += new EventHandler<MediaFile>(mediaFileFactory_OpenFinished);
            media = null;

            displayMode = DisplayModeState.NORMAL;

        }

        public enum DisplayModeState
        {
            NORMAL,
            SCALED
        };

        public enum CropStageState
        {
            DISABLED,
            ENABLED,
            START_PRESSED,
            START_RELEASED
        };

        Matrix buildTransform()
        {
            Matrix transformMatrix = Matrix.Identity;
            if (flipX == true)
            {
                transformMatrix.M11 *= -1;
            }

            if (flipY == true)
            {
                transformMatrix.M22 *= -1;
            }
            transformMatrix.Scale(scaleX, scaleY);
            transformMatrix.Rotate(rotation);

            return (transformMatrix);
        }

        private void mediaFileFactory_OpenFinished(System.Object sender, MediaFile media)
        {

            Object[] args = new Object[1];
            args[0] = media;

            Dispatcher.Invoke(new loadImageDelegate(loadImage), args);

        }

        private void loadImage(MediaFile media)
        {

            try
            {

                this.media = media;

                if (string.IsNullOrEmpty(media.Location) ||
                    media.MediaFormat != MediaFile.MediaType.IMAGE)
                {

                    clearImage();
                    return;

                }
                else if (!media.OpenSuccess)
                {

                    MessageBox.Show("Failed to open:\n\n" + media.Location + "\n\n" + media.OpenError.Message, "Error");
                    log.Error("Failed to open image: " + media.Location, media.OpenError);
                    return;
                }

                imageFormat = MediaFormatConvert.mimeTypeToImageFormat(media.MimeType);

                sourceImage = new BitmapImage();

                sourceImage.BeginInit();
                sourceImage.CacheOption = BitmapCacheOption.OnLoad;
                sourceImage.StreamSource = media.Data;
                sourceImage.EndInit();

                displayAndCenterImage(sourceImage);

                log.Info("Loaded image: " + media.Location);
                //LoadImageFinished(this, EventArgs.Empty);

            }
            catch (Exception e)
            {

                log.Error("Error reading:" + media.Location, e);
                MessageBox.Show("Error reading:\n\n" + media.Location + "\n\n" + e.Message, "Error");

            }
            finally
            {

                media.close();
                mediaFileFactory.releaseNonBlockingOpenLock();
            }

        }

        private void displayAndCenterImage(BitmapImage image)
        {

            pictureBox.Source = image;

            var source = PresentationSource.FromVisual(pictureBox);
            Matrix transformToDevice = source.CompositionTarget.TransformToDevice;
            transformToDevice.Invert();
            var actualSize = (Size)transformToDevice.Transform(new Vector(ImageSize.Width, ImageSize.Height));
            scaleX = actualSize.Width / image.Width;
            scaleY = actualSize.Height / image.Height;

            Transform = buildTransform();

            scrollViewer.ScrollToHorizontalOffset(0);
            scrollViewer.ScrollToVerticalOffset(0);
            /*
                        Drawing.Size maxDim = panel.Size;
                        Drawing.Size imageSize = image.Size;

                        if (DisplayMode == DisplayModeState.SCALED)
                        {

                            int scaledWidth, scaledHeight;
                            ImageUtils.resizeRectangle(image.Width, image.Height, maxDim.Width, maxDim.Height, scaledWidth, scaledHeight);

                            imageSize.Width = scaledWidth;
                            imageSize.Height = scaledHeight;
                        }

                        Drawing.Size autoScrollMinSize;

                        if (imageSize.Width > maxDim.Width)
                        {

                            autoScrollMinSize.Width = imageSize.Width + 1;
                        }

                        if (imageSize.Height > maxDim.Height)
                        {

                            autoScrollMinSize.Height = imageSize.Height + 1;
                        }

                        panel.AutoScrollMinSize = autoScrollMinSize;

                        get;
                        get;

                        panel.AutoScroll = false;
                        vscroll.Value = 0;
                        hscroll.Value = 0;
                        panel.AutoScroll = true;

                        int offsetX = 0;
                        int offsetY = 0;

                        if (imageSize.Width < maxDim.Width)
                        {

                            offsetX = (maxDim.Width - imageSize.Width) / 2;
                        }

                        if (imageSize.Height < maxDim.Height)
                        {

                            offsetY = (maxDim.Height - imageSize.Height) / 2;
                        }

                        pictureBox.Location = Drawing.Point(offsetX, offsetY);
                        pictureBox.Image = ImageUtils.resizeImage(image, imageSize.Width, imageSize.Height);
             */
        }



        private void clearImage()
        {

            if (sourceImage != null)
            {
                sourceImage = null;
            }

            if (pictureBox.Source != null)
            {
                pictureBox.Source = null;
            }

        }

        public void loadImage(string fileLocation)
        {

            clearImage();

            mediaFileFactory.openNonBlockingAndCancelPending(fileLocation,
                MediaFile.MetaDataMode.AUTO);

        }

        private void gridContainer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isLeftMouseButtonDown == true)
            {
                Point mousePos = e.GetPosition(scrollViewer);

                Vector ratio = new Vector(scrollViewer.ScrollableWidth / scrollViewer.ViewportWidth,
                    scrollViewer.ScrollableHeight / scrollViewer.ViewportHeight);

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
        public Matrix Transform
        {
            get
            {
                return transform;
            }
            set
            {
                transform = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Transform"));
            }
        }

        public Stretch Stretch
        {
            get
            {
                return (pictureBox.Stretch);
            }
            set
            {
                if (value != Stretch.None)
                {
                    scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                }
                else
                {
                    scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                }

                pictureBox.Stretch = value;

            }
        }

        public bool IsEmpty
        {

            get
            {

                return (pictureBox.Source == null ? true : false);
            }
        }

        public Size ImageSize
        {

            get
            {

                if (IsEmpty)
                {

                    return (new Size(0, 0));
                }

                Size imageSize = new Size(sourceImage.PixelWidth, sourceImage.PixelHeight);

                return (imageSize);
            }

        }



        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion


    }

}
