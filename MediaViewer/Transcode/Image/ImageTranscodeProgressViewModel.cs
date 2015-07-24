using MediaViewer.Infrastructure.Logging;
using MediaViewer.Infrastructure.Utils;
using MediaViewer.MediaDatabase;
using MediaViewer.Model.Concurrency;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Utils;
using MediaViewer.Progress;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MediaViewer.Transcode.Image
{
    class ImageTranscodeProgressViewModel : CancellableOperationProgressBase
    {
        ImageTranscodeViewModel State { get; set; }

        public ImageTranscodeProgressViewModel(ImageTranscodeViewModel vm)
        {
            WindowTitle = "Image Transcode Progress";
            WindowIcon = "pack://application:,,,/Resources/Icons/imagefile.ico";

            State = vm;

            ItemProgressMax = 100;
        }

        public async Task startTranscodeAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                startTranscode();

            }, CancellationToken, TaskCreationOptions.None, PriorityScheduler.BelowNormal);

            OkCommand.IsExecutable = true;
            CancelCommand.IsExecutable = false;
        }

        private void startTranscode()
        {
            Dictionary<String, Object> options = new Dictionary<string, object>();

            if (State.Width != null)
            {
                options.Add("Width", State.Width.Value);
            }

            if (State.Height != null)
            {
                options.Add("Height", State.Height.Value);
            }

            options.Add("QualityLevel", State.JpegQuality);
            options.Add("Rotation",Enum.Parse(typeof(Rotation),State.JpegRotationCollectionView.CurrentItem.ToString()));
            options.Add("Interlace", Enum.Parse(typeof(PngInterlaceOption), State.PngInterlacingCollectionView.CurrentItem.ToString()));
            options.Add("TiffCompression", Enum.Parse(typeof(TiffCompressOption), State.TiffCompressionCollectionView.CurrentItem.ToString()));
            options.Add("FlipHorizontal", State.FlipHorizontal);
            options.Add("FlipVertical", State.FlipVertical);

            TotalProgress = 0;
            TotalProgressMax = State.Items.Count;

            foreach (MediaFileItem item in State.Items)
            {
                if (CancellationToken.IsCancellationRequested) return;

                FileStream imageStream = null;

                item.RWLock.EnterReadLock();
                try
                {
                    ItemProgress = 0;

                    if (MediaFormatConvert.isImageFile(item.Location))
                    {
                        String outputPath = State.OutputPath + "\\" + Path.GetFileNameWithoutExtension(item.Location) + "." + ((String)State.OutputFormatCollectionView.CurrentItem).ToLower();

                        outputPath = FileUtils.getUniqueFileName(outputPath);

                        ItemInfo = "Loading image: " + item.Location;

                        imageStream = File.Open(item.Location, FileMode.Open, FileAccess.Read);
                        Rotation rotation = ImageUtils.getBitmapRotation(imageStream);
                        imageStream.Position = 0;

                        BitmapImage loadedImage = new BitmapImage();

                        loadedImage.BeginInit();           
                        loadedImage.CacheOption = BitmapCacheOption.OnLoad;
                        loadedImage.StreamSource = imageStream;
                        loadedImage.Rotation = rotation;
                        loadedImage.EndInit();

                        imageStream.Close();
                        imageStream = null;

                        ItemInfo = "Writing image: " + outputPath;

                        ImageTranscoder.writeImage(outputPath, loadedImage, options,
                            State.IsCopyMetadata ? item.Metadata as ImageMetadata : null, this);

                        InfoMessages.Add("Finished: " + item.Location + " -> " + outputPath);
                    }
                    else
                    {
                        InfoMessages.Add("Skipped: " + item.Location + " is not a image file");
                    }

                    TotalProgress++;
                    ItemProgress = 100;

                }
                catch (Exception e)
                {
                    InfoMessages.Add("Error: " + e.Message);
                    Logger.Log.Error("Error: " + e.Message);
                    return;
                }
                finally
                {                   
                    item.RWLock.ExitReadLock();
                    if (imageStream != null)
                    {
                        imageStream.Close();
                    }
                }
            }
        }
    }
}
