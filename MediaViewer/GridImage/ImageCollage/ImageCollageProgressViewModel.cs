using MediaViewer.GridImage.ImageCollage;
using MediaViewer.Infrastructure.Logging;
using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Mvvm;
using MediaViewer.Model.Utils;
using MediaViewer.Progress;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using VideoLib;

namespace MediaViewer.GridImage.ImageCollage
{
    public class ImageCollageProgressViewModel : CloseableBindableBase, ICancellableOperationProgress, IDisposable
    {
           
        public ImageCollageViewModel AsyncState {get;set;}
       
        CancellationTokenSource tokenSource;

        public ImageCollageProgressViewModel()
        {
            WindowTitle = "Image Collage";
            WindowIcon = "pack://application:,,,/Resources/Icons/collage.ico";
          
            InfoMessages = new ObservableCollection<string>();
            tokenSource = new CancellationTokenSource();
            CancellationToken = tokenSource.Token;

            OkCommand = new Command(() =>
            {
                OnClosingRequest();
            });

            CancelCommand = new Command(() =>
            {
                if (tokenSource != null)
                {
                    tokenSource.Cancel();
                }
            });

            OkCommand.IsExecutable = false;
            CancelCommand.IsExecutable = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool cleanupManaged)
        {
            if (cleanupManaged)
            {          
                if (tokenSource != null)
                {
                    tokenSource.Dispose();
                    tokenSource = null;
                }
            }

        }

        public async Task generateImage()
        {          
            await Task.Factory.StartNew(() =>
            {               
                TotalProgressMax = 1;

                generateImageCollage();
                    
                App.Current.Dispatcher.Invoke(() =>
                {
                    OkCommand.IsExecutable = true;
                    CancelCommand.IsExecutable = false;
                });
                   
            });
            
        }
       
        void generateImageCollage()
        {
            ItemProgressMax = 100;
            ItemProgress = 0;
            //ItemInfo = "Creating video preview image for: " + System.IO.Path.GetFileName(item.Location);

            FileStream outputFile = null;
            RenderTargetBitmap bitmap = null;
                      
            try
            {                                                                        
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                BitmapMetadata metaData = new BitmapMetadata("jpg");
                metaData.ApplicationName = App.getAppInfoString();
                metaData.DateTaken = DateTime.Now.ToString("R");

                int? maxGridHeight = AsyncState.IsMaxGridHeightEnabled ? new Nullable<int>(AsyncState.MaxGridHeight) : null;

                App.Current.Dispatcher.Invoke(() =>
                {
                    PictureGridImage gridImage = new PictureGridImage(AsyncState, AsyncState.Media.ToList(), AsyncState.IsUseThumbs);
                    bitmap = gridImage.createGridImage(AsyncState.MaxWidth, maxGridHeight);

                });

                encoder.Frames.Add(BitmapFrame.Create(bitmap, null, metaData, null));

                String outputPath = AsyncState.OutputPath + "\\" + AsyncState.Filename + ".jpg";

                outputFile = new FileStream(outputPath, FileMode.Create);
                encoder.QualityLevel = AsyncState.JpegQuality;
                encoder.Save(outputFile);

                ItemProgress = ItemProgressMax;
                TotalProgress = TotalProgressMax;
                InfoMessages.Add("Finished image collage: " + outputPath);

            }
            catch (Exception e)
            {
                InfoMessages.Add("Error creating image collage: " + e.Message);
                Logger.Log.Error("Error creating image collage: ", e);
            }
            finally
            {               
                if (bitmap != null)
                {
                    bitmap.Clear();
                    bitmap = null;
                }

                if (outputFile != null)
                {
                    outputFile.Close();
                }

                // because of the extreme amount of memory used by rendertargetbitmap 
                // make sure we have it all back before moving on to prevent out of memory spikes
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
               
            }
        }

        private void grabThumbnails_UpdateProgressCallback(VideoThumb thumb)
        {
            ItemProgress += 1;
        }
     
        public Command OkCommand { get; set; }
        public Command CancelCommand { get; set; }

        CancellationToken cancellationToken;

        public CancellationToken CancellationToken
        {
            get { return cancellationToken; }
            set { cancellationToken = value; }
        }

        int totalProgress;

        public int TotalProgress
        {
            get
            {
                return (totalProgress);
            }
            set
            {
                SetProperty(ref totalProgress, value);
            }
        }

        int totalProgressMax;

        public int TotalProgressMax
        {
            get
            {
                return (totalProgressMax);
            }
            set
            {
                SetProperty(ref totalProgressMax, value);
            }
        }

        int itemProgress;

        public int ItemProgress
        {
            get
            {
                return (itemProgress);
            }
            set
            {
                SetProperty(ref itemProgress, value);
            }
        }

        int itemProgressMax;

        public int ItemProgressMax
        {
            get
            {
                return (itemProgressMax);
            }
            set
            {
                SetProperty(ref itemProgressMax, value);
            }
        }

        String itemInfo;

        public String ItemInfo
        {
            get { return itemInfo; }
            set
            {
                SetProperty(ref itemInfo, value);
            }
        }

        ObservableCollection<String> infoMessages;

        public ObservableCollection<String> InfoMessages
        {
            get { return infoMessages; }
            set
            {
                SetProperty(ref infoMessages, value);
            }
        }

        string windowTitle;

        public string WindowTitle
        {
            get
            {
                return (windowTitle);
            }
            set
            {
                SetProperty(ref windowTitle, value);
            }
        }

        string windowIcon;

        public string WindowIcon
        {
            get
            {
                return (windowIcon);
            }
            set
            {
                SetProperty(ref windowIcon, value);
            }
        }


    }
}
