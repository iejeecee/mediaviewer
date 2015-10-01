using MediaViewer.DirectoryPicker;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Mvvm;
using MediaViewer.Model.Settings;
using MediaViewer.Model.Utils;
using MediaViewer.Progress;
using MediaViewer.Properties;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MediaViewer.Transcode.Image
{
    public class ImageTranscodeViewModel : CloseableBindableBase
    {
        public Command DefaultsCommand { get; set; }
        public Command CancelCommand { get; set; }
        public Command OkCommand { get; set; }
        public Command DirectoryPickerCommand { get; set; }
        public ListCollectionView OutputFormatCollectionView { get; set; }
        public ListCollectionView JpegRotationCollectionView { get; set; }
        public ListCollectionView PngInterlacingCollectionView { get; set; }
        public ListCollectionView TiffCompressionCollectionView { get; set; }

        public ICollection<MediaFileItem> Items { get; set; }

        List<String> outFormats = new List<string>() { "JPG", "PNG", "BMP", "TIFF", "GIF" };
        

        public ImageTranscodeViewModel()
        {
            OkCommand = new Command(async () =>
            {
                CancellableOperationProgressView progress = new CancellableOperationProgressView();
                ImageTranscodeProgressViewModel vm = new ImageTranscodeProgressViewModel(this);
                progress.DataContext = vm;              

                Task task = vm.startTranscodeAsync();
                progress.Show();
                OnClosingRequest();
                await task;

                MiscUtils.insertIntoHistoryCollection(Settings.Default.ImageTranscodeOutputDirectoryHistory, OutputPath);
            });


            DefaultsCommand = new Command(() =>
            {
                setDefaults();
            });

            CancelCommand = new Command(() =>
            {
                OnClosingRequest();
            });

            DirectoryPickerCommand = new Command(() =>
            {
                DirectoryPickerView directoryPicker = new DirectoryPickerView();
                DirectoryPickerViewModel vm = (DirectoryPickerViewModel)directoryPicker.DataContext;
                vm.InfoString = "Select Transcode Output Path";
                vm.SelectedPath = OutputPath;
                vm.PathHistory = OutputPathHistory;

                if (directoryPicker.ShowDialog() == true)
                {
                    OutputPath = vm.SelectedPath;
                }

            });

            OutputPathHistory = Settings.Default.ImageTranscodeOutputDirectoryHistory;            
            OutputFormatCollectionView = new ListCollectionView(outFormats);
            JpegRotationCollectionView = new ListCollectionView(Enum.GetNames(typeof(Rotation)));
            PngInterlacingCollectionView = new ListCollectionView(Enum.GetNames(typeof(PngInterlaceOption)));
            TiffCompressionCollectionView = new ListCollectionView(Enum.GetNames(typeof(TiffCompressOption)));

            setDefaults();
        }

        private void setDefaults()
        {
            OutputFormatCollectionView.MoveCurrentToFirst();
            JpegRotationCollectionView.MoveCurrentToFirst();
            TiffCompressionCollectionView.MoveCurrentToFirst();
            PngInterlacingCollectionView.MoveCurrentToFirst();

            FlipHorizontal = false;
            FlipVertical = false;

            JpegQuality = 90;
            IsCopyMetadata = true;

            Width = null;
            Height = null;
        }

        ObservableCollection<String> outputPathHistory;

        public ObservableCollection<String> OutputPathHistory
        {
            get { return outputPathHistory; }
            set { SetProperty(ref outputPathHistory, value); }
        }

        String outputPath;

        public String OutputPath
        {
            get { return outputPath; }
            set
            {
                SetProperty(ref outputPath, value);
            }
        }

        bool isCopyMetadata;

        public bool IsCopyMetadata
        {
            get { return isCopyMetadata; }
            set { SetProperty(ref isCopyMetadata, value); }
        }

        int jpegQuality;

        public int JpegQuality
        {
            get { return jpegQuality; }
            set { SetProperty(ref jpegQuality, value); }
        }

        int? width;

        public int? Width
        {
            get { return width; }
            set { SetProperty(ref width, value); }
        }

        int? height;

        public int? Height
        {
            get { return height; }
            set { SetProperty(ref height, value); }
        }

        bool flipHorizontal;

        public bool FlipHorizontal
        {
            get { return flipHorizontal; }
            set { SetProperty(ref flipHorizontal, value); }
        }

        bool flipVertical;

        public bool FlipVertical
        {
            get { return flipVertical; }
            set { SetProperty(ref flipVertical, value); }
        }
    }
}
