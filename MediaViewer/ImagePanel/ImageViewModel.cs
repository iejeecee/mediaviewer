using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MediaViewer.Model.Media.File;
using MvvmFoundation.Wpf;
using System.Windows;
using System.Threading;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Pager;
using System.Collections.Specialized;
using MediaViewer.MediaDatabase;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using MediaViewer.MetaData;
using MediaViewer.Model.Media.State;
using MediaViewer.Model.Utils;
using Microsoft.Practices.Prism.PubSubEvents;
using MediaViewer.Model.GlobalEvents;
using System.IO;

namespace MediaViewer.ImagePanel
{

    public class ImageViewModel : ObservableObject
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        CancellationTokenSource loadImageCTS; 
           
        public event EventHandler SetNormalScaleEvent;
        public event EventHandler SetBestFitScaleEvent;

        IEventAggregator EventAggregator { get; set; }
          
        public ImageViewModel(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;

            loadImageCTS = new CancellationTokenSource();
            isLoading = false;
            currentLocation = null;

            loadImageAsyncCommand = new Command(new Action<object>(async (fileName) =>
            {
                // cancel previously running load requests          
                loadImageCTS.Cancel();
                loadImageCTS = new CancellationTokenSource();

                try
                {
                    await Task.Factory.StartNew(() => loadImage((String)fileName), loadImageCTS.Token);
                }
                catch (TaskCanceledException)
                {
                
                }

            }));

        
            rotate90DegreesCommand = new Command(() =>
            {
                RotationDegrees += 90;
            });

            rotateMinus90DegreesCommand = new Command(() =>
            {
                RotationDegrees -= 90;
            });

            resetRotationDegreesCommand = new Command(() => { RotationDegrees = 0; });
      
            setIdentityTransform();
            SelectedScaleMode = ScaleMode.NONE;
  
            SelectedRotationMode = RotationMode.NONE;
            CustomRotation = 0;
                         
            SetCustomRotationCommand = new Command(() =>
            {
                SelectedRotationMode = RotationMode.CUSTOM;
                RotationView custom = new RotationView();
                custom.DataContext = this;
                custom.ShowDialog();
            });

            SetCustomScaleCommand = new Command(() =>
            {
                SelectedScaleMode = ScaleMode.CUSTOM;
                ScaleView custom = new ScaleView();
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

            SetNormalScaleCommand = new Command(() =>
            {
                if (SetNormalScaleEvent != null)
                {
                    SetNormalScaleEvent(this, EventArgs.Empty);
                }
            });

            SetBestFitScaleCommand = new Command(() =>
            {
                if (SetBestFitScaleEvent != null)
                {
                    SetBestFitScaleEvent(this, EventArgs.Empty);
                }
            });

            Brightness = 0;
            Contrast = 1;
            minScale = 0;
            maxScale = 1;
        
            IsEffectsEnabled = true;
            IsResetSettingsOnLoad = true;
         
        }

        void resetSettings()
        {
            Brightness = 0;
            Contrast = 1;
            RotationDegrees = 0;
            FlipX = false;
            FlipY = false;

            SelectedRotationMode = RotationMode.NONE;
            CustomRotation = 0;
        }

        bool isEffectsEnabled;

        public bool IsEffectsEnabled
        {
            get { return isEffectsEnabled; }
            set { isEffectsEnabled = value;
            NotifyPropertyChanged();
            }
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

                if (rotationDegrees < 0)
                {
                    rotationDegrees = 360 + (rotationDegrees + Math.Floor(rotationDegrees / 360) * 360);
                }
                else if (rotationDegrees >= 360)
                {
                    rotationDegrees = rotationDegrees - Math.Floor(rotationDegrees / 360) * 360;
                }

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

        Command rotate90DegreesCommand;

        public Command Rotate90DegreesCommand
        {
            get { return rotate90DegreesCommand; }
            set { rotate90DegreesCommand = value; }
        }
        Command rotateMinus90DegreesCommand;

        public Command RotateMinus90DegreesCommand
        {
            get { return rotateMinus90DegreesCommand; }
            set { rotateMinus90DegreesCommand = value; }
        }

        bool isResetSettingsOnLoad;

        public bool IsResetSettingsOnLoad
        {
            get { return isResetSettingsOnLoad; }
            set { isResetSettingsOnLoad = value;
            NotifyPropertyChanged();
            }
        }

        Command setCustomScaleCommand;

        public Command SetCustomScaleCommand
        {
            get { return setCustomScaleCommand; }
            set { setCustomScaleCommand = value; }
        }

        double scale;

        public double Scale
        {
            get { return scale; }
            set
            {
                scale = MiscUtils.clamp(value,MinScale,MaxScale);
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

        Command loadImageAsyncCommand;

        public Command LoadImageAsyncCommand
        {
            get { return loadImageAsyncCommand; }
        }

        Command setBestFitScaleCommand;

        public Command SetBestFitScaleCommand
        {
            get { return setBestFitScaleCommand; }
            set { setBestFitScaleCommand = value; }
        }

        Command setNormalScaleCommand;

        public Command SetNormalScaleCommand
        {
            get { return setNormalScaleCommand; }
            set { setNormalScaleCommand = value; }
        }

        String currentLocation;

        public String CurrentLocation
        {
            get { return currentLocation; }
            private set { currentLocation = value;
            NotifyPropertyChanged();
            }
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
              

        private void loadImage(String location)
        {
                    
            //BaseMedia media = null;
          
            try
            {
                IsLoading = true;

                //MediaFileItem item = MediaFileItem.Factory.create((String)location);
                
                //item.readMetaData(
                //    MediaFactory.ReadOptions.AUTO | MediaFactory.ReadOptions.LEAVE_STREAM_OPENED_AFTER_READ | MediaFactory.ReadOptions.GENERATE_THUMBNAIL, loadImageCTS.Token);

                if (loadImageCTS.IsCancellationRequested) return;

                //media = item.Media;

                BitmapImage loadedImage = null;

                //if (media is ImageMedia && media.Data != null)
                //{
                    loadedImage = new BitmapImage();

                    loadedImage.BeginInit();
                    loadedImage.CacheOption = BitmapCacheOption.OnLoad;
                    loadedImage.UriSource = new Uri(location);
                    loadedImage.EndInit();

                    loadedImage.Freeze();

                    //imageFile = (ImageMedia)media;

                    //updatePaging();

                    CurrentLocation = location;
                    log.Info("Image loaded: " + location);
                //}

                if (IsResetSettingsOnLoad)
                {
                    resetSettings();
                }

                Image = loadedImage;
          
                IsLoading = false;
               
            }
            catch (TaskCanceledException)
            {
                log.Info("Cancelled loading image:" + (String)location);
            }
            catch (Exception e)
            {
                CurrentLocation = null;
                log.Error("Error decoding image:" + (String)location, e);
                MessageBox.Show("Error loading image: " + location + "\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Image = null;
            }
            finally
            {
                //if (media != null)
                //{
                 //   media.close();
                //}
                EventAggregator.GetEvent<TitleChangedEvent>().Publish(location == null ? null : Path.GetFileName(CurrentLocation));
                IsLoading = false;
            }

        }

        public enum ScaleMode
        {
            NONE,  // display the image in 1 to 1 pixel ratio   
            CUSTOM,
            RELATIVE,
            AUTO, // automatically scale the image to fit it's surroundings
            FIT_HEIGHT, 
            FIT_WIDTH     
        }

        ScaleMode selectedScaleMode;

        public ScaleMode SelectedScaleMode
        {
            get { return selectedScaleMode; }
            set
            {
                selectedScaleMode = value;                                     
                NotifyPropertyChanged();
            }
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

        double minScale;

        public double MinScale
        {
            get { return minScale; }
            set { minScale = value;
            Scale = MiscUtils.clamp(Scale, MinScale, MaxScale);
            NotifyPropertyChanged();
            }
        }
        double maxScale;

        public double MaxScale
        {
            get { return maxScale; }
            set { maxScale = value;
            Scale = MiscUtils.clamp(Scale, MinScale, MaxScale);
            NotifyPropertyChanged();
            }
        }

        
    }

}
