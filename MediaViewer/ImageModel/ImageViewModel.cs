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
using System.Threading;

namespace MediaViewer.ImageModel
{
    class ImageViewModel : ObservableObject
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        CancellationTokenSource openMediaTokenSource;
        Task<MediaFile> openMediaTask;
     
        ImageFile imageFile;       

        public ImageViewModel()
        {
            openMediaTask = null;
            openMediaTokenSource = new CancellationTokenSource();

            loadImageAsyncCommand = new Command(new Action<object>((fileName) =>
            {
                loadImageAsync((String)fileName);
                
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

        private async void loadImageAsync(String fileName)
        {
            // cancel previously running load requests
            if (openMediaTask != null && openMediaTask.IsCompleted == false)
            {
                openMediaTokenSource.Cancel();
                await openMediaTask;
            }

            // async load media
            openMediaTask = MediaFileFactory.openAsync((String)fileName, MediaFile.MetaDataMode.AUTO, openMediaTokenSource.Token);
            MediaFile media = await openMediaTask;

            BitmapImage loadedImage = null;

            try
            {
               
                if (media.OpenSuccess &&
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
              
                Image = loadedImage;
                setIdentityTransform();
               
            }
            catch (Exception e)
            {
                log.Error("Error decoding image:" + media.Location, e);
            }
            finally
            {
                media.close();             
            }
          
        }        
    }
}
