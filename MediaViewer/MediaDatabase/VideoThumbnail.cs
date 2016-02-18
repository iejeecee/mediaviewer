using MediaViewer.Infrastructure;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Windows.Media.Imaging;

namespace MediaViewer.MediaDatabase
{
    public class VideoThumbnail : BindableBase
    {
        [NotMapped]
        public BitmapSource Image
        {
            get
            {

                BitmapSource image;

                // get decoded image from the cache if possible
                if (ImageData != null)
                {
                    image = decodeImage();
                    return (image);
                }
                else
                {
                    return (null);
                }
            }

        }

        public VideoThumbnail()
        {

        }

        
        public VideoThumbnail(BitmapSource source, double? timeSeconds)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            BitmapFrame outputFrame = BitmapFrame.Create(source, null, null, null);
            encoder.Frames.Add(outputFrame);
            encoder.QualityLevel = Constants.THUMBNAIL_QUALITY;

            MemoryStream stream = new MemoryStream();

            encoder.Save(stream);

            this.ImageData = stream.ToArray();
            this.Width = (short)source.PixelWidth;
            this.Height = (short)source.PixelHeight;

            TimeSeconds = timeSeconds;
        }

        BitmapImage decodeImage()
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(ImageData, 0, ImageData.Length);
            stream.Position = 0;

            BitmapImage thumb = new BitmapImage();
            thumb.BeginInit();
            thumb.CacheOption = BitmapCacheOption.OnLoad;
            thumb.StreamSource = stream;
            thumb.DecodePixelWidth = Width;
            thumb.DecodePixelHeight = Height;
            thumb.EndInit();

            thumb.Freeze();

            return (thumb);

        }

        [Key]
        public int Id { get; set; }

        [Required]
        public byte[] ImageData { get; set; }
        public short Width { get; set; }
        public short Height { get; set; }
        public Nullable<double> TimeSeconds { get; set; }

        [Timestamp]
        public byte[] TimeStamp { get; set; }

        [ForeignKey("VideoMetadata")]
        public int VideoMetadataId { get; set; }
        public virtual VideoMetadata VideoMetadata { get; set; }
    }
}
