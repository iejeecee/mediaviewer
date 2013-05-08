using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace MediaViewer
{
    /// <summary>
    /// Interaction logic for ImagePanelControl.xaml
    /// </summary>
    public partial class ImagePanelControl : UserControl
    {

        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private MediaFileFactory mediaFileFactory;
        private MediaFile media;

        private BitmapImage sourceImage;
        private ImageFormat imageFormat;

        private DisplayModeState displayMode;

        private bool leftMouseButtonDown;
        private bool isModified;

        private System.Drawing.Point startMouseLocation;
        private System.Drawing.Point scrollbarPosition;

        private delegate void loadImageDelegate(MediaFile media);

        public event EventHandler<EventArgs> LoadImageFinished;

        public ImagePanelControl()
        {
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
                else if(!media.OpenSuccess)
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
                //LoadImageFinished(this, EventArgs.Empty);
                log.Info("Loaded image: " + media.Location);

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
    }
}
