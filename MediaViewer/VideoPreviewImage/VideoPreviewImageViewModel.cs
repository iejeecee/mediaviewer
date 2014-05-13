using MediaViewer.DirectoryPicker;
using MediaViewer.MediaDatabase;
using MediaViewer.MediaFileModel.Watcher;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MediaViewer.VideoPreviewImage
{
    class VideoPreviewImageViewModel : CloseableObservableObject
    {
        protected static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        List<MediaFileItem> media;

        public List<MediaFileItem> Media
        {
            get { return media; }
            set { media = value; }
        }

        public VideoPreviewImageViewModel()
        {
            videoPreview = new VideoLib.VideoPreview();
            NrColumns = 4;
            NrRows = 16;
            MaxPreviewImageWidth = 1280;
            IsCaptureIntervalSecondsEnabled = true;
            CaptureIntervalSeconds = 30;
            OutputPath = MediaFileWatcher.Instance.Path;
            OutputPathHistory = new ObservableCollection<string>();
            OutputPathHistory.Insert(0, OutputPath);
            IsAddTags = true;
            IsCommentEnabled = false;

            directoryPickerCommand = new Command(new Action(() =>
            {
                DirectoryPickerView directoryPicker = new DirectoryPickerView();
                DirectoryPickerViewModel vm = (DirectoryPickerViewModel)directoryPicker.DataContext;
                vm.MovePath = OutputPath;
                vm.MovePathHistory = OutputPathHistory;

                if (directoryPicker.ShowDialog() == true)
                {
                    OutputPath = vm.MovePath;
                }

            }));

            OkCommand = new Command(() =>
            {
                generatePreviews();
                OnClosingRequest();
            });
            CancelCommand = new Command(() =>
            {
                OnClosingRequest();
            });
        }

        VideoLib.VideoPreview videoPreview;

        private VideoLib.VideoPreview VideoPreview
        {
            get { return videoPreview; }
            set { videoPreview = value; }
        }

        int maxPreviewImageWidth;

        public int MaxPreviewImageWidth
        {
            get { return maxPreviewImageWidth; }
            set { maxPreviewImageWidth = value;
            NotifyPropertyChanged();
            }
        }

        int nrRows;

        public int NrRows
        {
            get { return nrRows; }
            set { nrRows = value;
            NotifyPropertyChanged();
            }
        }

        bool isNrRowsEnabled;

        public bool IsNrRowsEnabled
        {
            get { return isNrRowsEnabled; }
            set
            {
                isNrRowsEnabled = value;

                if (IsNrRowsEnabled == true && IsCaptureIntervalSecondsEnabled == true)
                {
                    IsCaptureIntervalSecondsEnabled = false;
                }
                else if (IsNrRowsEnabled == false && IsCaptureIntervalSecondsEnabled == false)
                {
                    IsCaptureIntervalSecondsEnabled = true;
                }               
                
                NotifyPropertyChanged();
            }
        }

        int nrColumns;

        public int NrColumns
        {
            get { return nrColumns; }
            set { nrColumns = value;
            NotifyPropertyChanged();
            }
        }

        int jpegQuality;

        public int JpegQuality
        {
            get { return jpegQuality; }
            set { jpegQuality = value;
            NotifyPropertyChanged();
            }
        }

        int captureIntervalSeconds;

        public int CaptureIntervalSeconds
        {
            get { return captureIntervalSeconds; }
            set { captureIntervalSeconds = value;
            NotifyPropertyChanged();
            }
        }

        bool isCaptureIntervalSecondsEnabled;

        public bool IsCaptureIntervalSecondsEnabled
        {
            get { return isCaptureIntervalSecondsEnabled; }
            set
            {
                isCaptureIntervalSecondsEnabled = value;

                if (isCaptureIntervalSecondsEnabled == true && IsNrRowsEnabled == true)
                {
                    IsNrRowsEnabled = false;
                }
                else if (isCaptureIntervalSecondsEnabled == false && IsNrRowsEnabled == false)
                {
                    IsNrRowsEnabled = true;
                }
                                
                NotifyPropertyChanged();
            }
        }

        bool isAddTags;

        public bool IsAddTags
        {
            get { return isAddTags; }
            set { isAddTags = value;
            NotifyPropertyChanged();
            }
        }

        String comment;

        public String Comment
        {
            get { return comment; }            
            set { comment = value;
            NotifyPropertyChanged();
            }
        }
        bool isCommentEnabled;

        public bool IsCommentEnabled
        {
            get { return isCommentEnabled; }
            set { isCommentEnabled = value;
            NotifyPropertyChanged();
            }
        }

        string outputPath;

        public string OutputPath
        {
            get { return outputPath; }
            set { outputPath = value;
            NotifyPropertyChanged();
            }
        }

        ObservableCollection<String> outputPathHistory;

        public ObservableCollection<String> OutputPathHistory
        {
            get { return outputPathHistory; }
            set { outputPathHistory = value; }
        }

        Command okCommand;

        public Command OkCommand
        {
            get { return okCommand; }
            set { okCommand = value; }
        }

        Command directoryPickerCommand;

        public Command DirectoryPickerCommand
        {
            get { return directoryPickerCommand; }
            set { directoryPickerCommand = value; }
        }

        Command cancelCommand;

        public Command CancelCommand
        {
            get { return cancelCommand; }
            set { cancelCommand = value; }
        }

        void generatePreviews()
        {

            foreach (MediaFileItem item in Media)
            {                              
                generatePreview(item);                
            }
           
        }

        int calcNrRowsNrColumns(int nrFrames)
        {           
            if (nrFrames == 0)
            {
                // make sure to grab atleast one frame
                nrFrames = 1;
            }

            if (nrFrames < nrColumns)
            {

                nrColumns = nrFrames;
            }

            if (nrFrames % nrColumns != 0)
            {

                nrFrames -= (nrFrames % nrColumns);
            }

            nrRows = nrFrames / nrColumns;

            return (nrFrames);
        }

        void generatePreview(MediaFileItem item)      
        {

            FileStream outputFile = null;

            videoPreview.open(item.Location);
            try
            {

                int nrFrames = 0;

                if (IsCaptureIntervalSecondsEnabled == false)
                {

                    nrFrames = NrRows * NrColumns;

                }
                else
                {
                    nrFrames = videoPreview.DurationSeconds / CaptureIntervalSeconds;
                    nrFrames = calcNrRowsNrColumns(nrFrames);
                }

                int thumbWidth = MaxPreviewImageWidth / NrColumns;

                List<BitmapSource> thumbs = videoPreview.grabThumbnails(thumbWidth,
                       IsCaptureIntervalSecondsEnabled ? CaptureIntervalSeconds : -1, nrFrames, 0.01);

                if (thumbs.Count == 0) return;

                nrFrames = Math.Min(thumbs.Count, nrFrames);

                if (IsCaptureIntervalSecondsEnabled == true)
                {
                    nrFrames = calcNrRowsNrColumns(nrFrames);
                }

                GridImage gridImage = new GridImage(item.Media as VideoMedia,this, thumbs[0].PixelWidth * nrColumns,
                    thumbs[0].PixelHeight * nrRows, nrRows, nrColumns);

                for (int i = 0; i < nrFrames; i++)
                {
                    gridImage.addSubImage(thumbs[i], i);
                }         

                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
               
                encoder.Frames.Add(BitmapFrame.Create(gridImage.Image));

                String outputFileName = Path.GetFileNameWithoutExtension(item.Location) + ".jpg";

                outputFile = new FileStream(OutputPath + "/" + outputFileName, FileMode.Create);
                encoder.Save(outputFile);

            }
            catch (Exception e)
            {
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
    }
}
