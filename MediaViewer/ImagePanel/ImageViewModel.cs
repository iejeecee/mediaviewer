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
using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.Pager;

namespace MediaViewer.ImagePanel
{
    class ImageViewModel : ObservableObject
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        CancellationTokenSource loadImageCTS;

        ImageFile imageFile;

        public ImageViewModel()
        {

            loadImageCTS = new CancellationTokenSource();
            isLoading = false;

            loadImageAsyncCommand = new Command(new Action<object>((fileName) =>
            {
                loadImageAsync((String)fileName);

            }));


            MediaFileWatcher.Instance.MediaFiles.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler((s, e) =>
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {

                    if (imageFile == null)
                    {
                        IsPagingImagesEnabled = false;
                        return;
                    }

                    int index = MediaFileWatcher.Instance.getMediaFileIndex(imageFile.Location, MediaFileWatcher.MediaType.IMAGE);                                                                             

                    if (index == -1)
                    {
                        IsPagingImagesEnabled = false;                     
                    }
                    else
                    {
                        IsPagingImagesEnabled = true;
                        NrImages = MediaFileWatcher.Instance.getNrMediaFiles(MediaFileWatcher.MediaType.IMAGE);
                        CurrentImage = index + 1;                       
                    }                    
                     
                }));
            });

            nextImageCommand = new Command(new Action(() =>
            {

                CurrentImage += 1;

            }));

            prevImageCommand = new Command(new Action(() =>
            {

                CurrentImage -= 1;

            }));

            firstImageCommand = new Command(new Action(() =>
            {
                CurrentImage = 1;
            }));

            lastImageCommand = new Command(new Action(() =>
            {
                CurrentImage = NrImages;
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

        bool isLoading;

        public bool IsLoading
        {
            get { return isLoading; }
            set
            {
                isLoading = value;
                NotifyPropertyChanged();
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
            loadImageCTS.Cancel();
            loadImageCTS = new CancellationTokenSource();

            // async load media

            MediaFile media = null;

            try
            {
                IsLoading = true;

                media = await MediaFileFactory.openAsync((String)fileName, MediaFile.MetaDataMode.AUTO, loadImageCTS.Token);

                IsLoading = false;

                BitmapImage loadedImage = null;

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

                    int index = MediaFileWatcher.Instance.getMediaFileIndex(imageFile.Location, MediaFileWatcher.MediaType.IMAGE);

                    if (index == -1)
                    {
                        IsPagingImagesEnabled = false;                     
                    }
                    else
                    {
                        IsPagingImagesEnabled = true;
                        NrImages = MediaFileWatcher.Instance.getNrMediaFiles(MediaFileWatcher.MediaType.IMAGE);
                        CurrentImage = index + 1;
                    }

                    log.Info("Image loaded: " + media.Location);
                }

                Image = loadedImage;
                setIdentityTransform();

            }
            catch (TaskCanceledException)
            {
                log.Info("Cancelled loading image:" + (String)fileName);
            }
            catch (Exception e)
            {
                log.Error("Error decoding image:" + (String)fileName, e);
                Image = null;
            }
            finally
            {
                if (media != null)
                {
                    media.close();
                }
            }

        }

        bool isPagingImagesEnabled;

        public bool IsPagingImagesEnabled
        {
            get { return isPagingImagesEnabled; }
            set
            {
                isPagingImagesEnabled = value;
                if (value == false)
                {
                    NextImageCommand.CanExecute = false;
                    PrevImageCommand.CanExecute = false;
                    FirstImageCommand.CanExecute = false;
                    LastImageCommand.CanExecute = false;

                }
                NotifyPropertyChanged();
            }
        }


        int nrImages;

        public int NrImages
        {
            get
            {
                return nrImages;
            }
            set
            {
                nrImages = value;
                NotifyPropertyChanged();
            }
        }

        int currentImage;

        public int CurrentImage
        {
            get
            {
                return (currentImage);
            }
            set
            {
                if (value <= 0 || value > NrImages || IsPagingImagesEnabled == false) return;
              
                String newImage = MediaFileWatcher.Instance.getMediaFileByIndex(MediaFileWatcher.MediaType.IMAGE, value - 1);

                if (!String.IsNullOrEmpty(newImage) && imageFile != null && !newImage.Equals(imageFile.Location))
                {
                    GlobalMessenger.Instance.NotifyColleagues("MainWindowViewModel.ViewMediaCommand", newImage);
                }

                currentImage = value;

                if (CurrentImage + 1 <= NrImages)
                {
                    nextImageCommand.CanExecute = true;
                    lastImageCommand.CanExecute = true;
                }
                else
                {
                    nextImageCommand.CanExecute = false;
                    lastImageCommand.CanExecute = false;
                }

                if (CurrentImage - 1 > 0)
                {
                    prevImageCommand.CanExecute = true;
                    firstImageCommand.CanExecute = true;
                }
                else
                {
                    prevImageCommand.CanExecute = false;
                    firstImageCommand.CanExecute = false;
                }


                NotifyPropertyChanged();
            }
        }

        Command nextImageCommand;

        public Command NextImageCommand
        {
            get
            {
                return nextImageCommand;
            }
            set
            {
                nextImageCommand = value;
            }
        }

        Command prevImageCommand;

        public Command PrevImageCommand
        {
            get
            {
                return prevImageCommand;
            }
            set
            {
                prevImageCommand = value;
            }
        }

        Command firstImageCommand;

        public Command FirstImageCommand
        {
            get
            {
                return firstImageCommand;
            }
            set
            {
                firstImageCommand = value;
            }
        }

        Command lastImageCommand;

        public Command LastImageCommand
        {
            get
            {
                return lastImageCommand;
            }
            set
            {
                lastImageCommand = value;
            }
        }
    }
}
