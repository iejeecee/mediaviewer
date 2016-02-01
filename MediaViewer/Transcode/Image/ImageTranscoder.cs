using MediaViewer.Infrastructure;
using MediaViewer.Infrastructure.Utils;
using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.File.Metadata;
using MediaViewer.Model.Utils;
using MediaViewer.Progress;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;


namespace MediaViewer.Transcode.Image
{
    class ImageTranscoder
    {
        public static void writeImage(String outputPath, BitmapSource image, Dictionary<String, Object> options = null, ImageMetadata metaData = null, CancellableOperationProgressBase progress = null)
        {            
            int width = image.PixelWidth;
            int height = image.PixelHeight;

            float scale = ImageUtils.resizeRectangle(width, height, Constants.MAX_THUMBNAIL_WIDTH, Constants.MAX_THUMBNAIL_HEIGHT);

            TransformedBitmap thumbnail = new TransformedBitmap(image, new System.Windows.Media.ScaleTransform(scale, scale));
          
            if (options != null)
            {                
                if (options.ContainsKey("Width"))
                {
                    width = (int)options["Width"];

                    if (!options.ContainsKey("Height"))
                    {
                        height = (int)(((float)width / image.PixelWidth) * image.PixelHeight);
                    }

                } 
                
                if (options.ContainsKey("Height")) {

                    height = (int)options["Height"];

                    if (!options.ContainsKey("Width"))
                    {
                        width = (int)(((float)height / image.PixelHeight) * image.PixelWidth);
                    }
                }
            }

            BitmapSource outImage = image;

            if (width != image.PixelWidth || height != image.PixelHeight)
            {
                scale = ImageUtils.resizeRectangle(image.PixelWidth, image.PixelHeight, width, height);
                outImage = new TransformedBitmap(image, new System.Windows.Media.ScaleTransform(scale, scale));
            }
          
            ImageFormat format = MediaFormatConvert.fileNameToImageFormat(outputPath);

            BitmapEncoder encoder = null;

            if (format == ImageFormat.Jpeg)
            {
                encoder = configureJpeg(options, ref thumbnail);
            }
            else if (format == ImageFormat.Png)
            {
                encoder = configurePng(options);                
            }
            else if (format == ImageFormat.Gif)
            {
                encoder = new GifBitmapEncoder();               
            }
            else if (format == ImageFormat.Bmp)
            {
                encoder = new BmpBitmapEncoder();
            }
            else if (format == ImageFormat.Tiff)
            {
                encoder = configureTiff(options);                
            }

            encoder.Frames.Add(BitmapFrame.Create(outImage, thumbnail, null, null));

            FileStream outputFile = new FileStream(outputPath, FileMode.Create);

            encoder.Save(outputFile);

            outputFile.Close();

            if (metaData != null)
            {
                metaData.Location = outputPath;
                ImageFileMetadataWriter metadataWriter = new ImageFileMetadataWriter();
                metadataWriter.writeMetadata(metaData, progress);
            }
        }

        static BitmapEncoder configureJpeg(Dictionary<String, Object> options, ref TransformedBitmap thumbnail)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();

            if (options != null)
            {
                if (options.ContainsKey("QualityLevel"))
                {
                    encoder.QualityLevel = (int)options["QualityLevel"];
                }

                if (options.ContainsKey("Rotation"))
                {
                    encoder.Rotation = (Rotation)options["Rotation"];

                    if (encoder.Rotation != Rotation.Rotate0)
                    {
                        thumbnail = new TransformedBitmap(thumbnail, new System.Windows.Media.RotateTransform((int)encoder.Rotation * 90));
                    }
                }

                if (options.ContainsKey("FlipHorizontal"))
                {
                    encoder.FlipHorizontal = (bool)options["FlipHorizontal"];

                    if (encoder.FlipHorizontal)
                    {
                        System.Windows.Media.Matrix transform = System.Windows.Media.Matrix.Identity;

                        transform.M11 *= -1;
                                   
                        thumbnail = new TransformedBitmap(thumbnail, new System.Windows.Media.MatrixTransform(transform));
                    }
                }

                if (options.ContainsKey("FlipVertical"))
                {
                    encoder.FlipVertical = (bool)options["FlipVertical"];

                    if (encoder.FlipVertical)
                    {
                        System.Windows.Media.Matrix transform = System.Windows.Media.Matrix.Identity;

                        transform.M22 *= -1;

                        thumbnail = new TransformedBitmap(thumbnail, new System.Windows.Media.MatrixTransform(transform));
                    }
                }
            }

            return (encoder);
        }

        static BitmapEncoder configurePng(Dictionary<String, Object> options)
        {
            PngBitmapEncoder encoder = new PngBitmapEncoder();

            if (options != null)
            {
                if (options.ContainsKey("Interlace"))
                {
                    encoder.Interlace = (PngInterlaceOption)options["Interlace"];
                }
               
            }

            return (encoder);
    
        }

        static BitmapEncoder configureTiff(Dictionary<String, Object> options)
        {
            TiffBitmapEncoder encoder = new TiffBitmapEncoder();

            if (options != null)
            {
                if (options.ContainsKey("TiffCompression"))
                {
                    encoder.Compression = (TiffCompressOption)options["TiffCompression"];
                }
               
            }
            
            return (encoder);
        }

        
    }
}
