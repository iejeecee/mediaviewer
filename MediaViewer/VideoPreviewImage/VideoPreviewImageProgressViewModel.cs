using MediaViewer.MediaDatabase;
using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.Progress;
using MvvmFoundation.Wpf;
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

namespace MediaViewer.VideoPreviewImage
{
    public class VideoPreviewImageProgressViewModel : CloseableObservableObject, ICancellableOperationProgress
    {

        protected static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
                tokenSource.Cancel();
            });

            OkCommand.CanExecute = false;
            CancelCommand.CanExecute = true;
        }

        public async Task generatePreviews()
        {

            await Task.Run(() =>
            {
                TotalProgressMax = asyncState.Media.Count;
             
                for (TotalProgress = 0; TotalProgress < TotalProgressMax; TotalProgress++)
                {
                    MediaFileItem item = asyncState.Media[TotalProgress];

                    if (CancellationToken.IsCancellationRequested) return;
                    if (!Utils.MediaFormatConvert.isVideoFile(item.Location))
                    {
                        InfoMessages.Add("Skipping: " + item.Location + " is not a video file.");
                        continue;
                    }
                    if (item.Media == null)
                    {
                        item.readMetaData(MediaFileModel.MediaFactory.ReadOptions.AUTO, CancellationToken);
                        if (item.ItemState != MediaFileItemState.LOADED)
                        {
                            InfoMessages.Add("Skipping: " + item.Location + " could not read metadata.");
                            continue;
                        }
                    }

                    generatePreview(item);
             
                }

                App.Current.Dispatcher.Invoke(() =>
                {
                    OkCommand.CanExecute = true;
                    CancelCommand.CanExecute = false;
                });
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
                
                GridImage gridImage = new GridImage(item.Media as VideoMedia, asyncState, thumbs[0].Thumb.PixelWidth * asyncState.NrColumns,
                    thumbs[0].Thumb.PixelHeight * asyncState.NrRows, asyncState.NrRows, asyncState.NrColumns, thumbs);

                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                BitmapMetadata metaData = new BitmapMetadata("jpg");
                metaData.ApplicationName = App.getAppInfoString();
                //metaData.DateTaken = DateTime.Now.ToLongDateString();

                if (item.Media.Tags.Count > 0)
                {

                    List<String> tags = new List<string>();

                    foreach (Tag tag in item.Media.Tags)
                    {
                        tags.Add(tag.Name);
                    }

                    metaData.Keywords = new ReadOnlyCollection<string>(tags);
                }

                if (item.Media.Title != null)
                {
                    metaData.Title = item.Media.Title;
                }

                if (item.Media.Copyright != null)
                {
                    metaData.Copyright = item.Media.Copyright;
                }

                if (item.Media.Description != null)
                {
                    metaData.Subject = item.Media.Description;
                }

                if (item.Media.Author != null)
                {
                    List<String> author = new List<string>();
                    author.Add(item.Media.Author);

                    metaData.Author = new ReadOnlyCollection<string>(author);
                }

                if (item.Media.Rating != null)
                {
                    metaData.Rating = (int)item.Media.Rating.Value;
                }

                encoder.Frames.Add(BitmapFrame.Create(gridImage.Image, null, metaData, null));

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
                log.Error("Error creating preview image for: " + item.Location, e);
            }
            finally
            {
                if (outputFile != null)
                {
                    outputFile.Close();
                }

                videoPreview.close();
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


        Command okCommand;

        public Command OkCommand
        {
            get { return okCommand; }
            set
            {
                okCommand = value;
                NotifyPropertyChanged();
            }
        }

        Command cancelCommand;

        public Command CancelCommand
        {
            get { return cancelCommand; }
            set
            {
                cancelCommand = value;
                NotifyPropertyChanged();
            }
        }

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
                totalProgress = value;
                NotifyPropertyChanged();
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
                totalProgressMax = value;
                NotifyPropertyChanged();
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
                itemProgress = value;
                NotifyPropertyChanged();
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
                itemProgressMax = value;
                NotifyPropertyChanged();
            }
        }

        String itemInfo;

        public String ItemInfo
        {
            get { return itemInfo; }
            set
            {
                itemInfo = value;
                NotifyPropertyChanged();
            }
        }

        ObservableCollection<String> infoMessages;

        public ObservableCollection<String> InfoMessages
        {
            get { return infoMessages; }
            set
            {
                infoMessages = value;
                NotifyPropertyChanged();
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
                windowTitle = value;
                NotifyPropertyChanged();
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
                windowIcon = value;
                NotifyPropertyChanged();
            }
        }
    }
}
