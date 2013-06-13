using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MediaViewer.MVMediaFile;
using MvvmFoundation.Wpf;

namespace MediaViewer.ImageModel
{
    class ImageViewModel : ObservableObject
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        ImageFile imageFile;
        
        static BitmapImage errorImage = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/error.png"));

        public ImageViewModel()
        {

            loadImageAsyncCommand = new AsyncCommand(new Action<object>(loadImage));

            resetRotationDegreesCommand = new Command(() => { RotationDegrees = 0; });
            resetScaleCommand = new Command(() => { Scale = 1; });
        
            transform = new Matrix();
            rotationDegrees = 0;
            scale = 1;
            flipX = false;
            flipY = false;

        }

        bool flipX;

        public bool FlipX
        {
            get { return flipX; }
            set { flipX = value;
         
            NotifyPropertyChanged();
            updateTransform();
            }
        }

        bool flipY;

        public bool FlipY
        {
            get { return flipY; }
            set { flipY = value;
           
            NotifyPropertyChanged();
            updateTransform();
            }
        }

        BitmapImage image;

        public BitmapImage Image
        {
            get { return image; }
            set
            {
                image = value;
                NotifyPropertyChanged();
            }
        }

        Matrix transform;

        public Matrix Transform
        {
            get { return transform; }
            set
            {
                transform = value;
                NotifyPropertyChanged();
            }
        }

        double rotationDegrees;

        public double RotationDegrees
        {
            get { return rotationDegrees; }
            set
            {
                rotationDegrees = value;
                NotifyPropertyChanged();
                updateTransform();
            }
        }

        Command resetRotationDegreesCommand;

        public Command ResetRotationDegreesCommand
        {
            get { return resetRotationDegreesCommand; }           
        }

        double scale;

        public double Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                NotifyPropertyChanged();
                updateTransform();
            }
        }

        Command resetScaleCommand;

        public Command ResetScaleCommand
        {
            get { return resetScaleCommand; }            
        }
     
        AsyncCommand loadImageAsyncCommand;

        public AsyncCommand LoadImageAsyncCommand
        {
            get { return loadImageAsyncCommand; }
           
        }

        void updateTransform()
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
            transformMatrix.Scale(Scale, Scale);
            transformMatrix.Rotate(RotationDegrees);

            Transform = transformMatrix;
        }

        void loadImage(Object param)
        {
            string fileName = (string)param;
            MediaFile media = MediaFileFactory.openBlocking(fileName, MediaFile.MetaDataMode.AUTO);

            BitmapImage loadedImage = new BitmapImage();

            loadedImage.BeginInit();
            loadedImage.CacheOption = BitmapCacheOption.OnLoad;
            loadedImage.StreamSource = media.Data;
            loadedImage.EndInit();

            loadedImage.Freeze();

            imageFile = (ImageFile)media;

            LoadImageAsyncCommand.ReportProgress(() => { Image = loadedImage; });
                       
        }

        /*
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

                displayAndCenterSourceImage();

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
        */

    }
}
