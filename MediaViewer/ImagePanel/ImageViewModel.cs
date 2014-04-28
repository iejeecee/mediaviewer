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
using MediaViewer.Utils;
using System.Collections.Specialized;
using MediaViewer.MediaDatabase;

namespace MediaViewer.ImagePanel
{
    public class ImageViewModel : ObservableObject, IPageable
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        CancellationTokenSource loadImageCTS;

        ImageMedia imageFile;

        MediaState MediaState
        {
            get
            {
                return (MediaFileWatcher.Instance.MediaState);
            }
        }


        public ImageViewModel()
        {

            loadImageCTS = new CancellationTokenSource();
            isLoading = false;

            loadImageAsyncCommand = new Command(new Action<object>((fileName) =>
            {
                loadImageAsync((String)fileName);

            }));


            MediaState.NrItemsInStateChanged += new NotifyCollectionChangedEventHandler((s, e) =>
            {

                if (imageFile == null)
                {
                    IsPagingEnabled = false;
                    return;
                }

                int index = getImageFileIndex(imageFile.Location);

                if (index == -1)
                {
                    IsPagingEnabled = false;
                }
                else
                {
                    IsPagingEnabled = true;
                    NrPages = getNrImageFiles();
                    CurrentPage = index + 1;
                }                                                        
                
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
            resetScaleCommand = new Command(() => { CustomScale = 1; });

            setIdentityTransform();
            SelectedScaleMode = ScaleMode.NONE;
            CustomScale = 1;
            SelectedRotationMode = RotationMode.NONE;
            CustomRotation = 0;
                  
            SetCustomScaleCommand = new Command(() => {

                SelectedScaleMode = ScaleMode.CUSTOM;            
                ScaleView custom = new ScaleView();
                custom.DataContext = this;
                custom.ShowDialog();
            });

            SetCustomRotationCommand = new Command(() =>
            {
                SelectedRotationMode = RotationMode.CUSTOM;
                RotationView custom = new RotationView();
                custom.DataContext = this;
                custom.ShowDialog();
            });

            SetBrightnessCommand = new Command(() =>
                {
                    BrightnessView brightnessView = new BrightnessView();
                    brightnessView.DataContext = this;
                    brightnessView.ShowDialog();
                });

            ResetBrightnessCommand = new Command(() =>
                {
                    Brightness = 0;
                });

            SetContrastCommand = new Command(() =>
                {
                    ContrastView contrastView = new ContrastView();
                    contrastView.DataContext = this;
                    contrastView.ShowDialog();

                });
            ResetContrastCommand = new Command(() =>
                {
                    Contrast = 1;
                });


            Brightness = 0;
            Contrast = 1;
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
        Command setBrightnessCommand;

        public Command SetBrightnessCommand
        {
            get { return setBrightnessCommand; }
            set { setBrightnessCommand = value;
            NotifyPropertyChanged();
            }
        }
        Command resetBrightnessCommand;

        public Command ResetBrightnessCommand
        {
            get { return resetBrightnessCommand; }
            set { resetBrightnessCommand = value;
            NotifyPropertyChanged();
            }
            
        }

        Command setContrastCommand;

        public Command SetContrastCommand
        {
            get { return setContrastCommand; }
            set { setContrastCommand = value;
            NotifyPropertyChanged();
            }
        }
        Command resetContrastCommand;

        public Command ResetContrastCommand
        {
            get { return resetContrastCommand; }
            set { resetContrastCommand = value;
            NotifyPropertyChanged();
            }
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

        double brightness;

        public double Brightness
        {
            get { return brightness; }
            set { brightness = value;
            NotifyPropertyChanged();
            }
        }
        double contrast;

        public double Contrast
        {
            get { return contrast; }
            set { contrast = value;
            NotifyPropertyChanged();
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

            Media media = null;

            try
            {
                IsLoading = true;

                media = await Task<Media>.Run(() => MediaFactory.read((String)fileName, 
                    MediaFactory.ReadOptions.AUTO | MediaFactory.ReadOptions.LEAVE_STREAM_OPENED_AFTER_READ, 
                    loadImageCTS.Token), loadImageCTS.Token);
          
                BitmapImage loadedImage = null;

                if (media is ImageMedia && media.Data != null)
                {
                    loadedImage = new BitmapImage();

                    loadedImage.BeginInit();
                    loadedImage.CacheOption = BitmapCacheOption.OnLoad;
                    loadedImage.StreamSource = media.Data;
                    loadedImage.EndInit();

                    loadedImage.Freeze();

                    imageFile = (ImageMedia)media;

                    MediaState.UIMediaCollection.EnterReaderLock();

                    int index = getImageFileIndex(imageFile.Location);

                    if (index == -1)
                    {
                        IsPagingEnabled = false;
                    }
                    else
                    {
                        IsPagingEnabled = true;
                        NrPages = getNrImageFiles();
                        CurrentPage = index + 1;
                    }

                    MediaState.UIMediaCollection.ExitReaderLock();

                    log.Info("Image loaded: " + media.Location);
                }
                

                Image = loadedImage;

                IsLoading = false;
                //setIdentityTransform();

            }
            catch (TaskCanceledException)
            {
                log.Info("Cancelled loading image:" + (String)fileName);
            }
            catch (Exception e)
            {
                log.Error("Error decoding image:" + (String)fileName, e);
                MessageBox.Show("Error loading image: " + fileName + "\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Image = null;
            }
            finally
            {
                if (media != null)
                {
                    media.close();
                }
                IsLoading = false;
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
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        NextPageCommand.CanExecute = false;
                        PrevPageCommand.CanExecute = false;
                        FirstPageCommand.CanExecute = false;
                        LastPageCommand.CanExecute = false;
                    }));

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

                MediaState.UIMediaCollection.EnterReaderLock();

                String location = getImageFileByIndex(value - 1);

                if (location != null && imageFile != null && !location.ToLower().Equals(imageFile.Location.ToLower()))
                {
                    //GlobalMessenger.Instance.NotifyColleagues("MainWindowViewModel.ViewMediaCommand", location);
                    loadImageAsyncCommand.DoExecute(location);
                }

                MediaState.UIMediaCollection.ExitReaderLock();

                currentPage = value;
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                  {
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
                  }));

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

        int getNrImageFiles()
        {           
            int count = 0;

            foreach (MediaFileItem item in MediaState.UIMediaCollection.Items)
            {                  
                if (MediaFormatConvert.isImageFile(item.Location))
                {
                    count++;
                }                  
            }

            return (count);            
        }

        string getImageFileByIndex(int index)
        {

            int i = 0;

            foreach (MediaFileItem item in MediaState.UIMediaCollection.Items)
            {

                if (MediaFormatConvert.isImageFile(item.Location))
                {
                    if (index == i)
                    {
                        return (item.Location);
                    }

                    i++;
                }

            }

            return (null);
        }

        int getImageFileIndex(string location)
        {

            int i = 0;

            foreach (MediaFileItem item in MediaState.UIMediaCollection.Items)
            {

                if (MediaFormatConvert.isImageFile(item.Location))
                {
                    if (item.Location.ToLower().Equals(location.ToLower()))
                    {
                        return (i);
                    }

                    i++;
                }

            }

            return (-1);
        }

        public enum ScaleMode
        {
            NONE,
            AUTO,
            FIT_HEIGHT,
            FIT_WIDTH,
            CUSTOM
        }

        ScaleMode selectedScaleMode;

        public ScaleMode SelectedScaleMode
        {
            get { return selectedScaleMode; }
            set
            {
                selectedScaleMode = value;

                if (selectedScaleMode == ScaleMode.CUSTOM)
                {
                    Scale = CustomScale;
                }
                else
                {
                    Scale = 1;
                }

                NotifyPropertyChanged();
            }
        }

        float customScale;

        public float CustomScale
        {
            get { return customScale; }
            set { customScale = value;
            Scale = customScale;
            NotifyPropertyChanged();
            }
        }
        
        Command setCustomScaleCommand;

        public Command SetCustomScaleCommand
        {
            get { return setCustomScaleCommand; }
            set { setCustomScaleCommand = value; }
        }

        public enum RotationMode
        {
            NONE,
            CW_90,
            CW_180,
            CCW_90,
            CUSTOM
        }

        RotationMode selectedRotationMode;

        public RotationMode SelectedRotationMode
        {
            get { return selectedRotationMode; }
            set
            {
                selectedRotationMode = value;

                switch (selectedRotationMode)
                {
                    case RotationMode.NONE:
                        {
                            RotationDegrees = 0;
                            break;
                        }
                    case RotationMode.CW_90:
                        {
                            RotationDegrees = 90;
                            break;
                        }
                    case RotationMode.CW_180:
                        {
                            RotationDegrees = 180;
                            break;
                        }
                    case RotationMode.CCW_90:
                        {
                            RotationDegrees = -90;
                            break;
                        }
                    case RotationMode.CUSTOM:
                        {
                            RotationDegrees = CustomRotation;
                            break;
                        }

                }
            
                NotifyPropertyChanged();
            }
        }


        float customRotation;

        public float CustomRotation
        {
            get { return customRotation; }
            set
            {
                customRotation = value;
                RotationDegrees = customRotation;
                NotifyPropertyChanged();
            }
        }
        
        Command setCustomRotationCommand;

        public Command SetCustomRotationCommand
        {
            get { return setCustomRotationCommand; }
            set { setCustomRotationCommand = value; }
        }
    }

}
