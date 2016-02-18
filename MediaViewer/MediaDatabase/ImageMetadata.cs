using MediaViewer.Model.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;

namespace MediaViewer.MediaDatabase
{
    public partial class ImageMetadata : BaseMetadata
    {
        public ImageMetadata()
        {
        }

        public ImageMetadata(String location, Stream data) : base(location, data)  
        {
            
        }

        public override string DefaultFormatCaption
        {
            get
            {
                if (MetadataReadError != null)
                {
                    return MetadataReadError.Message;
                }

                StringBuilder sb = new StringBuilder();

                sb.AppendLine(Path.GetFileName(Location));
                sb.AppendLine();

                sb.AppendLine("Mime type:");
                sb.Append(MimeType);
                sb.AppendLine();
                sb.AppendLine();

                sb.AppendLine("Resolution:");
                sb.Append(Width);
                sb.Append("x");
                sb.Append(Height);
                sb.AppendLine();
                sb.AppendLine();

                sb.AppendLine("Size:");
                sb.Append(MiscUtils.formatSizeBytes(SizeBytes));
               
                return (sb.ToString());
            }
        }
    

        public int Width { get; set; }
        public int Height { get; set; }
        public Nullable<short> LightSource { get; set; }
        public Nullable<short> MeteringMode { get; set; }
        public Nullable<short> Saturation { get; set; }
        public Nullable<short> SceneCaptureType { get; set; }
        public Nullable<short> SensingMethod { get; set; }
        public Nullable<short> Sharpness { get; set; }
        public Nullable<double> SubjectDistance { get; set; }
        public Nullable<double> ShutterSpeedValue { get; set; }
        public Nullable<short> SubjectDistanceRange { get; set; }
        public Nullable<short> WhiteBalance { get; set; }
        public Nullable<bool> FlashFired { get; set; }
        public Nullable<short> FlashMode { get; set; }
        public Nullable<short> FlashReturn { get; set; }
        public string CameraMake { get; set; }
        public string CameraModel { get; set; }
        public string Lens { get; set; }
        public string SerialNumber { get; set; }
        public Nullable<double> ExposureTime { get; set; }
        public Nullable<double> FNumber { get; set; }
        public Nullable<double> ExposureBiasValue { get; set; }
        public Nullable<short> ExposureProgram { get; set; }
        public Nullable<double> FocalLength { get; set; }
        public Nullable<int> ISOSpeedRating { get; set; }
        public Nullable<short> Contrast { get; set; }
        public Nullable<short> Orientation { get; set; }

        [Required]
        public string PixelFormat { get; set; }

        public short BitsPerPixel { get; set; }
        public string ImageContainer { get; set; }
        //public int Id { get; set; }
        //public virtual BaseMetadata BaseMetadata { get; set; }
    }
}
