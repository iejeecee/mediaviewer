using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MediaViewer.Model.Media.File;
using Microsoft.Practices.Prism.Mvvm;
using System.Windows;
using System.Threading;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.UserControls.Pager;
using System.Collections.Specialized;
using MediaViewer.MediaDatabase;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using MediaViewer.MetaData;
using MediaViewer.Model.Media.State;
using MediaViewer.Model.Utils;
using Microsoft.Practices.Prism.PubSubEvents;
using MediaViewer.Model.Global.Events;
using System.IO;
using Microsoft.Practices.Prism.Commands;
using MediaViewer.Model.Mvvm;
using MediaViewer.MediaFileBrowser;
using MediaViewer.Infrastructure.Logging;

namespace MediaViewer.ImagePanel
{

    public class ImageViewModel : BindableBase, IMediaFileBrowserContentViewModel, IDisposable
    {
        

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

            loadImageAsyncCommand = new Command<String>(new Action<String>(async (fileName) =>
            {
                // cancel previously running load requests          
                loadImageCTS.Cancel();
                loadImageCTS = new CancellationTokenSource();

                try
                {
                    await Task.Factory.StartNew(() => loadImage(fileName), loadImageCTS.Token);
                }
                catch (TaskCanceledException)
                {
                
                }

            }));

        
            Rotate90DegreesCommand = new Command(() =>
            {
                RotationDegrees += 90;
            });

            RotateMinus90DegreesCommand = new Command(() =>
            {
                RotationDegrees -= 90;
            });

            ResetRotationDegreesCommand = new Command(() => { RotationDegrees = 0; });
      
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool safe)
        {
            if (safe)
            {
                if (loadImageCTS != null)
                {
                    loadImageCTS.Dispose();
                    loadImageCTS = null;
                }
            }
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
            set {  
            SetProperty(ref isEffectsEnabled, value);
            }
        }

        bool flipX;

        public bool FlipX
        {
            get { return flipX; }
            set
            {              
                SetProperty(ref flipX, value);
                updateTransform();
            }
        }

        bool flipY;

        public bool FlipY
        {
            get { return flipY; }
            set
            {            
                SetProperty(ref flipY, value);
                updateTransform();
            }
        }

        bool isLoading;

        public bool IsLoading
        {
            get { return isLoading; }
            set
            {               
                SetProperty(ref isLoading, value);
            }
        }

        BitmapImage image;

        public BitmapImage Image
        {
            get { return image; }
            set
            {                              
                SetProperty(ref image, value);
            }
        }

        Matrix transform;

        public Matrix Transform
        {
            get { return transform; }
            set
            {                
                SetProperty(ref transform, value);
            }
        }
      
    
        double rotationDegrees;

        public double RotationDegrees
        {
            get { return rotationDegrees; }
            set
            {
                double tempRotationDegrees = value;

                if (tempRotationDegrees < 0)
                {
                    tempRotationDegrees = 360 + (tempRotationDegrees + Math.Floor(tempRotationDegrees / 360) * 360);
                }
                else if (tempRotationDegrees >= 360)
                {
                    tempRotationDegrees = tempRotationDegrees - Math.Floor(tempRotationDegrees / 360) * 360;
                }

                SetProperty(ref rotationDegrees, tempRotationDegrees);
                updateTransform();
            }
        }

        public Command ResetRotationDegreesCommand { get; set; }              
        public Command SetBrightnessCommand {get; set;}
        public Command ResetBrightnessCommand { get; set; }       
        public Command SetContrastCommand { get; set; }           
        public Command ResetContrastCommand { get; set; }              
        public Command Rotate90DegreesCommand {get; set;}
        public Command RotateMinus90DegreesCommand { get; set; }
       
        bool isResetSettingsOnLoad;

        public bool IsResetSettingsOnLoad
        {
            get { return isResetSettingsOnLoad; }
            set {  
            SetProperty(ref isResetSettingsOnLoad, value);
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
                double clampedScale = MiscUtils.clamp(value,MinScale,MaxScale);
                SetProperty(ref scale, clampedScale);
                updateTransform();
            }
        }

        double brightness;

        public double Brightness
        {
            get { return brightness; }
            set { 
                SetProperty(ref brightness, value);
            }
        }
        double contrast;

        public double Contrast
        {
            get { return contrast; }
            set {  
                SetProperty(ref contrast, value);
            }
        }

        Command<String> loadImageAsyncCommand;

        public Command<String> LoadImageAsyncCommand
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
            private set {  
                SetProperty(ref currentLocation, value);
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
                    loadedImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    loadedImage.CacheOption = BitmapCacheOption.OnLoad;
                    loadedImage.UriSource = new Uri(location);
                    loadedImage.EndInit();

                    loadedImage.Freeze();

                    //imageFile = (ImageMedia)media;

                    //updatePaging();

                    CurrentLocation = location;
                    Logger.Log.Info("Image loaded: " + location);
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
                Logger.Log.Info("Cancelled loading image:" + (String)location);
            }
            catch (Exception e)
            {
                CurrentLocation = null;
                Logger.Log.Error("Error decoding image:" + (String)location, e);
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
                SetProperty(ref selectedScaleMode, value);
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
                SetProperty(ref selectedRotationMode, value);
             
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
            
                
            }
        }


        float customRotation;

        public float CustomRotation
        {
            get { return customRotation; }
            set
            {
                SetProperty(ref customRotation, value);               
                RotationDegrees = customRotation;                
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
            set
            {
                SetProperty(ref minScale, value);
                Scale = MiscUtils.clamp(Scale, MinScale, MaxScale);           
            }
        }
        double maxScale;

        public double MaxScale
        {
            get { return maxScale; }
            set
            {
                SetProperty(ref maxScale, value);
                Scale = MiscUtils.clamp(Scale, MinScale, MaxScale);           
            }
        }

        public void OnNavigatedTo(Microsoft.Practices.Prism.Regions.NavigationContext navigationContext)
        {
            String location = (String)navigationContext.Parameters["location"];

            if (!String.IsNullOrEmpty(location))
            {
                LoadImageAsyncCommand.Execute(location);
            }
            else
            {
                EventAggregator.GetEvent<TitleChangedEvent>().Publish(CurrentLocation == null ? "" : CurrentLocation);
            }

            EventAggregator.GetEvent<MediaSelectionEvent>().Subscribe(imageView_MediaSelectionEvent, ThreadOption.UIThread);
        }

        public void OnNavigatedFrom(Microsoft.Practices.Prism.Regions.NavigationContext navigationContext)
        {
            EventAggregator.GetEvent<MediaSelectionEvent>().Unsubscribe(imageView_MediaSelectionEvent);
        }

        private void imageView_MediaSelectionEvent(MediaFileItem item)
        {
            if (String.Equals(CurrentLocation, item.Location)) return;

            LoadImageAsyncCommand.Execute(item.Location);
        }
    }

}
