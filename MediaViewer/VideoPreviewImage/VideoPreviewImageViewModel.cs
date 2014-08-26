using MediaViewer.DirectoryPicker;
using MediaViewer.MediaDatabase;
using MediaViewer.MediaFileModel;
using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.Progress;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VideoLib;

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

        public VideoPreviewImageViewModel(MediaFileWatcher mediaFileWatcher)
        {
            setDefaults(mediaFileWatcher);
            
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

            OkCommand = new Command(async () =>
            {
                CancellableOperationProgressView progress = new CancellableOperationProgressView();
                VideoPreviewImageProgressViewModel vm = new VideoPreviewImageProgressViewModel();
                progress.DataContext = vm;
                vm.AsyncState = this;
                progress.Show();
                Task task = vm.generatePreviews();
                OnClosingRequest();
                await task;
            });
            CancelCommand = new Command(() =>
            {
                OnClosingRequest();
            });

            DefaultsCommand = new Command(() =>
                {
                    setDefaults(mediaFileWatcher);
                });
        }

        void setDefaults(MediaFileWatcher mediaFileWatcher)
        {
            NrColumns = 3;
            NrRows = 16;
            MaxPreviewImageWidth = 1280;
            IsCaptureIntervalSecondsEnabled = true;
            CaptureIntervalSeconds = 30;
            OutputPath = mediaFileWatcher.Path;
            OutputPathHistory = new ObservableCollection<string>();
            OutputPathHistory.Insert(0, OutputPath);
            IsAddTags = true;
            IsAddTimestamps = true;
            IsCommentEnabled = false;
            JpegQuality = 80;
            IsAddHeader = true;
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

        bool isAddHeader;

        public bool IsAddHeader
        {
            get { return isAddHeader; }
            set { isAddHeader = value;
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

        Command defaultsCommand;

        public Command DefaultsCommand
        {
            get { return defaultsCommand; }
            set { defaultsCommand = value; }
        }
        
        bool isAddTimeStamps;

        public bool IsAddTimestamps {
            get
            {
                return isAddTimeStamps;
            }
            set
            {
                isAddTimeStamps = value;
                NotifyPropertyChanged();
            }
        }
    }
}
