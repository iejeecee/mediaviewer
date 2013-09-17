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
    class ImageViewModel : ObservableObject, IPageable
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
                        IsPagingEnabled = false;
                        return;
                    }

                    int index = MediaFileWatcher.Instance.getMediaFileIndex(imageFile.Location, MediaFileWatcher.MediaType.IMAGE);                                                                             

                    if (index == -1)
                    {
                        IsPagingEnabled = false;                     
                    }
                    else
                    {
                        IsPagingEnabled = true;
                        NrPages = MediaFileWatcher.Instance.getNrMediaFiles(MediaFileWatcher.MediaType.IMAGE);
                        CurrentPage = index + 1;                       
                    }                    
                     
                }));
            });

            nextPageCommand = new Command(new Action(() =>
            {

                CurrentPage += 1;

            }));

            prevPageCommand = new Command(new Action(() =>
            {

                CurrentPage -= 1;

            }));

            firstPageCommand = new Command(new Action(() =>
            {
                CurrentPage = 1;
            }));

            lastPageCommand = new Command(new Action(() =>
            {
                CurrentPage = NrPages;
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
                        IsPagingEnabled = false;                     
                    }
                    else
                    {
                        IsPagingEnabled = true;
                        NrPages = MediaFileWatcher.Instance.getNrMediaFiles(MediaFileWatcher.MediaType.IMAGE);
                        CurrentPage = index + 1;
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

        bool isPagingEnabled;

        public bool IsPagingEnabled
        {
            get { return isPagingEnabled; }
            set
            {
                isPagingEnabled = value;
                if (value == false)
                {
                    NextPageCommand.CanExecute = false;
                    PrevPageCommand.CanExecute = false;
                    FirstPageCommand.CanExecute = false;
                    LastPageCommand.CanExecute = false;

                }
                NotifyPropertyChanged();
            }
        }


        int nrPages;

        public int NrPages
        {
            get
            {
                return nrPages;
            }
            set
            {
                nrPages = value;
                NotifyPropertyChanged();
            }
        }

        int currentPage;

        public int CurrentPage
        {
            get
            {
                return (currentPage);
            }
            set
            {
                if (value <= 0 || value > NrPages || IsPagingEnabled == false) return;
              
                String newImage = MediaFileWatcher.Instance.getMediaFileByIndex(MediaFileWatcher.MediaType.IMAGE, value - 1);

                if (!String.IsNullOrEmpty(newImage) && imageFile != null && !newImage.Equals(imageFile.Location))
                {
                    GlobalMessenger.Instance.NotifyColleagues("MainWindowViewModel.ViewMediaCommand", newImage);
                }

                currentPage = value;

                if (CurrentPage + 1 <= NrPages)
                {
                    nextPageCommand.CanExecute = true;
                    lastPageCommand.CanExecute = true;
                }
                else
                {
                    nextPageCommand.CanExecute = false;
                    lastPageCommand.CanExecute = false;
                }

                if (CurrentPage - 1 > 0)
                {
                    prevPageCommand.CanExecute = true;
                    firstPageCommand.CanExecute = true;
                }
                else
                {
                    prevPageCommand.CanExecute = false;
                    firstPageCommand.CanExecute = false;
                }


                NotifyPropertyChanged();
            }
        }

        Command nextPageCommand;

        public Command NextPageCommand
        {
            get
            {
                return nextPageCommand;
            }
            set
            {
                nextPageCommand = value;
            }
        }

        Command prevPageCommand;

        public Command PrevPageCommand
        {
            get
            {
                return prevPageCommand;
            }
            set
            {
                prevPageCommand = value;
            }
        }

        Command firstPageCommand;

        public Command FirstPageCommand
        {
            get
            {
                return firstPageCommand;
            }
            set
            {
                firstPageCommand = value;
            }
        }

        Command lastPageCommand;

        public Command LastPageCommand
        {
            get
            {
                return lastPageCommand;
            }
            set
            {
                lastPageCommand = value;
            }
        }
    }
}
