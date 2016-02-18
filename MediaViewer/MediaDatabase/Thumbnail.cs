using MediaViewer.Infrastructure;
using MediaViewer.Model.Collections.Cache.LRUCache;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Windows.Media.Imaging;

namespace MediaViewer.MediaDatabase
{
    public class Thumbnail : BindableBase, IEquatable<Thumbnail>
    {
        static LRUCache<Guid, BitmapSource> thumbCache;

        static Thumbnail()
        {
            thumbCache = new LRUCache<Guid, BitmapSource>(100);           
        }

        [NotMapped]
        public BitmapSource Image
        {
            get
            {

                BitmapSource image;

                // get decoded image from the cache if possible
                if ((image = thumbCache.get(Guid)) != null)
                {
                    return (image);
                }
                else if (ImageData != null)
                {
                    image = decodeImage();
                    thumbCache.add(Guid, image);
                    return (image);
                }
                else
                {
                    return (null);
                }
            }

        }

        public Thumbnail()
        {
            Guid = Guid.NewGuid();
        }

        public Thumbnail(BitmapSource source)
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

            Guid = Guid.NewGuid();
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

        public bool Equals(Thumbnail other)
        {
            if (other == null)
            {
                throw new InvalidOperationException();
            }

            if (other.Id == Id) return (true);
            else return (false);
        }

        [NotMapped]
        public Guid Guid { get; protected set; }
       
        [Required]
        public byte[] ImageData { get; set; }
        public short Width { get; set; }
        public short Height { get; set; }
        public byte[] TimeStamp { get; set; }

        [Key,ForeignKey("BaseMetadata")]
        public int Id { get; set; }
        public virtual BaseMetadata BaseMetadata { get; set; }
    }
}
