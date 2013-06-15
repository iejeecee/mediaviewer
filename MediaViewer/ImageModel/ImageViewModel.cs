using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MediaViewer.MediaFileModel;
using MvvmFoundation.Wpf;
using System.Windows;

namespace MediaViewer.ImageModel
{
    class ImageViewModel : ObservableObject
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        MediaFileFactory mediaFileFactory;
        ImageFile imageFile;       

        public ImageViewModel()
        {

            mediaFileFactory = new MediaFileFactory();
            mediaFileFactory.OpenFinished += new EventHandler<MediaFile>(mediaFileFactory_OpenFinished);

            loadImageAsyncCommand = new AsyncCommand(new Action<object>((fileName) =>
            {
                mediaFileFactory.openNonBlockingAndCancelPending((String)fileName, MediaFile.MetaDataMode.AUTO);
            }));

            resetRotationDegreesCommand = new Command(() => { RotationDegrees = 0; });
            resetScaleCommand = new Command(() => { Scale = 1; });

            setIdentityTransform();

        }

        bool flipX;

        public bool FlipX
        {
            get { return flipX; }
            set
            {
                flipX = value;

                NotifyPropertyChanged();
                updateTransform();
            }
        }

        bool flipY;

        public bool FlipY
        {
            get { return flipY; }
            set
            {
                flipY = value;

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

        Command loadImageAsyncCommand;

        public Command LoadImageAsyncCommand
        {
            get { return loadImageAsyncCommand; }
        }

        void setIdentityTransform()
        {
            RotationDegrees = 0;
            Scale = 1;
            FlipX = false;
            FlipY = false;

            updateTransform();

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

        private void mediaFileFactory_OpenFinished(Object sender, MediaFile media)
        {

            BitmapImage loadedImage = null;

            try
            {
                if (!media.OpenSuccess)
                {

                    log.Error("Failed to open image: " + media.Location, media.OpenError);

                }
                else if (!string.IsNullOrEmpty(media.Location) &&
                      media.MediaFormat == MediaFile.MediaType.IMAGE)
                {
                    loadedImage = new BitmapImage();

                    loadedImage.BeginInit();
                    loadedImage.CacheOption = BitmapCacheOption.OnLoad;
                    loadedImage.StreamSource = media.Data;
                    loadedImage.EndInit();

                    loadedImage.Freeze();
                  
                    imageFile = (ImageFile)media;

                    log.Info("Image loaded: " + media.Location);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Image = loadedImage;
                    setIdentityTransform();
                });
            }
            catch (Exception e)
            {
                log.Error("Error reading:" + media.Location, e);
            }
            finally
            {                
                media.close();
                mediaFileFactory.releaseNonBlockingOpenLock();
            }

        }     
    }
}
