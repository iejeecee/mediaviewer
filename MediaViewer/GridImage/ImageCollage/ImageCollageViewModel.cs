using MediaViewer.DirectoryPicker;
using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Mvvm;
using MediaViewer.Model.Settings;
using MediaViewer.Model.Utils;
using MediaViewer.Progress;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
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

namespace MediaViewer.GridImage.ImageCollage
{

    public class ImageCollageViewModel : CloseableBindableBase
    {
        AppSettings Settings { get; set; }

        ICollection<MediaFileItem> media;

        public ICollection<MediaFileItem> Media
        {
            get { return media; }
            set { media = value; }
        }

        public ImageCollageViewModel(MediaFileWatcher mediaFileWatcher, AppSettings settings)
        {
            Settings = settings;
           
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
                using (ImageCollageProgressViewModel vm = new ImageCollageProgressViewModel())
                {
                    progress.DataContext = vm;
                    vm.AsyncState = this;
                    progress.Show();
                    Task task = vm.generateImage();
                    OnClosingRequest();
                    await task;
                    MiscUtils.insertIntoHistoryCollection(OutputPathHistory, OutputPath);
                }
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
            NrColumns = 6;           
            MaxWidth = 1280;
            IsMaxGridHeightEnabled = true;
            MaxGridHeight = 256;            
            OutputPath = mediaFileWatcher.Path;
            OutputPathHistory = Settings.ImageCollageOutputDirectoryHistory;                 
            IsCommentEnabled = false;
            FontSize = 20;
            JpegQuality = 80;
            IsAddHeader = true;
            IsAddInfo = true;
            IsUseThumbs = true;
            Filename = String.IsNullOrEmpty(OutputPath) ? "collage" : Path.GetFileName(OutputPath);
        }

        string filename;

        public string Filename
        {
            get { return filename; }
            set { SetProperty(ref filename, value); }
        }

        int maxWidth;

        public int MaxWidth
        {
            get { return maxWidth; }
            set
            {
                SetProperty(ref maxWidth, value);
            }
        }

        bool isAddInfo;

        public bool IsAddInfo
        {
            get { return isAddInfo; }
            set { SetProperty(ref isAddInfo, value); }
        }

        bool isAddHeader;

        public bool IsAddHeader
        {
            get { return isAddHeader; }
            set
            {
                SetProperty(ref isAddHeader, value);
            }
        }

        int nrColumns;

        public int NrColumns
        {
            get { return nrColumns; }
            set
            {
                SetProperty(ref nrColumns, value);
            }
        }

        int jpegQuality;

        public int JpegQuality
        {
            get { return jpegQuality; }
            set
            {
                SetProperty(ref jpegQuality, value);
            }
        }

        int fontSize;

        public int FontSize
        {
            get { return fontSize; }
            set { SetProperty(ref fontSize, value); }
        }

        int maxGridHeight;

        public int MaxGridHeight
        {
            get { return maxGridHeight; }
            set
            {
                SetProperty(ref maxGridHeight, value);
            }
        }

        bool isUseThumbs;

        public bool IsUseThumbs
        {
            get { return isUseThumbs; }
            set 
            {
                SetProperty(ref isUseThumbs, value); 
            }
        }

        bool isMaxGridHeightEnabled;

        public bool IsMaxGridHeightEnabled
        {
            get { return isMaxGridHeightEnabled; }
            set
            {
                SetProperty(ref isMaxGridHeightEnabled, value);           
            }
        }
     
        String comment;

        public String Comment
        {
            get { return comment; }
            set
            {
                SetProperty(ref comment, value);
            }
        }
        bool isCommentEnabled;

        public bool IsCommentEnabled
        {
            get { return isCommentEnabled; }
            set
            {
                SetProperty(ref isCommentEnabled, value);
            }
        }

        string outputPath;

        public string OutputPath
        {
            get { return outputPath; }
            set
            {
                SetProperty(ref outputPath, value);
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

       
    }
}

