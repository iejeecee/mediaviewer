using MediaViewer.Infrastructure.Logging;
using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.metadata.Metadata;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VideoLib;

namespace MediaViewer.GridImage.VideoPreviewImage
{
    public class VideoPreviewImageProgressViewModel : CloseableBindableBase, ICancellableOperationProgress, IDisposable
    {
      
        VideoPreviewImageViewModel asyncState;

        internal VideoPreviewImageViewModel AsyncState
        {
            get { return asyncState; }
            set { asyncState = value; }
        }

        CancellationTokenSource tokenSource;

        public VideoPreviewImageProgressViewModel()
        {
            WindowTitle = "Video Preview Image";
            WindowIcon = "pack://application:,,,/Resources/Icons/preview.ico";

            videoPreview = new VideoLib.VideoPreview();

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
                if (videoPreview != null)
                {
                    videoPreview.Dispose();
                    videoPreview = null;
                }

                if (tokenSource != null)
                {
                    tokenSource.Dispose();
                    tokenSource = null;
                }
            }

        }

        public async Task generatePreviews()
        {
           
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    TotalProgressMax = asyncState.Media.Count;

                    for (TotalProgress = 0; TotalProgress < TotalProgressMax; TotalProgress++)
                    {
                        MediaFileItem item = asyncState.Media.ElementAt(TotalProgress);

                        if (CancellationToken.IsCancellationRequested) return;
                        if (!MediaFormatConvert.isVideoFile(item.Location))
                        {
                            InfoMessages.Add("Skipping: " + item.Location + " is not a video file.");
                            continue;
                        }
                        if (item.Metadata == null)
                        {
                            item.readMetadata(MetadataFactory.ReadOptions.AUTO, CancellationToken);
                            if (item.ItemState != MediaItemState.LOADED)
                            {
                                InfoMessages.Add("Skipping: " + item.Location + " could not read metadata.");
                                continue;
                            }
                        }

                        generatePreview(item);

                    }

                }
                finally
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        OkCommand.IsExecutable = true;
                        CancelCommand.IsExecutable = false;
                    });
                }
            });
        }

        int calcNrRowsNrColumns(int nrFrames)
        {
            if (nrFrames == 0)
            {
                // make sure to grab atleast one frame
                nrFrames = 1;
            }

            if (nrFrames < asyncState.NrColumns)
            {

                asyncState.NrColumns = nrFrames;
            }

            if (nrFrames % asyncState.NrColumns != 0)
            {

                nrFrames -= (nrFrames % asyncState.NrColumns);
            }

            asyncState.NrRows = nrFrames / asyncState.NrColumns;

            return (nrFrames);
        }
   
        void generatePreview(MediaFileItem item)
        {
            ItemProgressMax = 100;
            ItemProgress = 0;
            ItemInfo = "Creating video preview image for: " + System.IO.Path.GetFileName(item.Location);

            FileStream outputFile = null;
            RenderTargetBitmap bitmap = null;

            videoPreview.open(item.Location);
            try
            {

                int nrFrames = 0;

                if (asyncState.IsCaptureIntervalSecondsEnabled == false)
                {
                    nrFrames = asyncState.NrRows * asyncState.NrColumns;
                }
                else
                {
                    nrFrames = videoPreview.DurationSeconds / asyncState.CaptureIntervalSeconds;
                    nrFrames = calcNrRowsNrColumns(nrFrames);
                }

                int thumbWidth = asyncState.MaxPreviewImageWidth / asyncState.NrColumns;

                ItemProgressMax = nrFrames;

                List<VideoThumb> thumbs = videoPreview.grabThumbnails(thumbWidth,
                       asyncState.IsCaptureIntervalSecondsEnabled ? asyncState.CaptureIntervalSeconds : -1, nrFrames, 0.01, CancellationToken, grabThumbnails_UpdateProgressCallback);
             
                if (thumbs.Count == 0 || CancellationToken.IsCancellationRequested) return;

                nrFrames = Math.Min(thumbs.Count, nrFrames);

                if (asyncState.IsCaptureIntervalSecondsEnabled == true)
                {
                    nrFrames = calcNrRowsNrColumns(nrFrames);
                }                               

                JpegBitmapEncoder encoder = new JpegBitmapEncoder();                
                BitmapMetadata metaData = new BitmapMetadata("jpg");
                metaData.ApplicationName = App.getAppInfoString();
                metaData.DateTaken = DateTime.Now.ToString("R");

                if (item.Metadata.Tags.Count > 0)
                {

                    List<String> tags = new List<string>();

                    foreach (Tag tag in item.Metadata.Tags)
                    {
                        tags.Add(tag.Name);
                    }

                    metaData.Keywords = new ReadOnlyCollection<string>(tags);
                }

                if (item.Metadata.Title != null)
                {
                    metaData.Title = item.Metadata.Title;
                }

                if (item.Metadata.Copyright != null)
                {
                    metaData.Copyright = item.Metadata.Copyright;
                }

                if (item.Metadata.Description != null)
                {
                    metaData.Subject = item.Metadata.Description;
                }

                if (item.Metadata.Author != null)
                {
                    List<String> author = new List<string>();
                    author.Add(item.Metadata.Author);

                    metaData.Author = new ReadOnlyCollection<string>(author);
                }

                if (item.Metadata.Rating != null)
                {
                    metaData.Rating = (int)item.Metadata.Rating.Value;
                }

                // rendertargetbitmap has to be executed on the UI thread
                // if it's run on a non-UI thread there will be a memory leak
                App.Current.Dispatcher.Invoke(() =>
                    {
                        VideoGridImage gridImage = new VideoGridImage(item.Metadata as VideoMetadata, asyncState, thumbs);
                        bitmap = gridImage.createGridImage(asyncState.MaxPreviewImageWidth);
                    });

                BitmapFrame frame = BitmapFrame.Create(bitmap, null, metaData, null);

                encoder.Frames.Add(frame);               

                String outputFileName = Path.GetFileNameWithoutExtension(item.Location) + ".jpg";              

                outputFile = new FileStream(asyncState.OutputPath + "/" + outputFileName, FileMode.Create);
                encoder.QualityLevel = asyncState.JpegQuality;
                encoder.Save(outputFile);

                ItemProgressMax = nrFrames;
                ItemProgress = nrFrames;
                InfoMessages.Add("Finished video preview image: " + asyncState.OutputPath + "/" + outputFileName);
                                               
                
            }
            catch (Exception e)
            {
                InfoMessages.Add("Error creating video preview image for: " + item.Location + " " + e.Message);
                Logger.Log.Error("Error creating preview image for: " + item.Location, e);
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
                    outputFile.Dispose();
                }
                
                videoPreview.close();

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

        VideoLib.VideoPreview videoPreview;

        private VideoLib.VideoPreview VideoPreview
        {
            get { return videoPreview; }
            set { videoPreview = value; }
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
