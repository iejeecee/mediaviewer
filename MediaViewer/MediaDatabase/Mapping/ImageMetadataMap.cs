using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace MediaViewer.MediaDatabase.Mapping
{
    public class ImageMetadataMap : EntityTypeConfiguration<ImageMetadata>
    {
        public ImageMetadataMap()
        {
            // Primary Key
            /*this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.PixelFormat)
                .IsRequired();

            this.Property(t => t.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("ImageMetadata");
            this.Property(t => t.Width).HasColumnName("Width");
            this.Property(t => t.Height).HasColumnName("Height");
            this.Property(t => t.LightSource).HasColumnName("LightSource");
            this.Property(t => t.MeteringMode).HasColumnName("MeteringMode");
            this.Property(t => t.Saturation).HasColumnName("Saturation");
            this.Property(t => t.SceneCaptureType).HasColumnName("SceneCaptureType");
            this.Property(t => t.SensingMethod).HasColumnName("SensingMethod");
            this.Property(t => t.Sharpness).HasColumnName("Sharpness");
            this.Property(t => t.SubjectDistance).HasColumnName("SubjectDistance");
            this.Property(t => t.ShutterSpeedValue).HasColumnName("ShutterSpeedValue");
            this.Property(t => t.SubjectDistanceRange).HasColumnName("SubjectDistanceRange");
            this.Property(t => t.WhiteBalance).HasColumnName("WhiteBalance");
            this.Property(t => t.FlashFired).HasColumnName("FlashFired");
            this.Property(t => t.FlashMode).HasColumnName("FlashMode");
            this.Property(t => t.FlashReturn).HasColumnName("FlashReturn");
            this.Property(t => t.CameraMake).HasColumnName("CameraMake");
            this.Property(t => t.CameraModel).HasColumnName("CameraModel");
            this.Property(t => t.Lens).HasColumnName("Lens");
            this.Property(t => t.SerialNumber).HasColumnName("SerialNumber");
            this.Property(t => t.ExposureTime).HasColumnName("ExposureTime");
            this.Property(t => t.FNumber).HasColumnName("FNumber");
            this.Property(t => t.ExposureBiasValue).HasColumnName("ExposureBiasValue");
            this.Property(t => t.ExposureProgram).HasColumnName("ExposureProgram");
            this.Property(t => t.FocalLength).HasColumnName("FocalLength");
            this.Property(t => t.ISOSpeedRating).HasColumnName("ISOSpeedRating");
            this.Property(t => t.Contrast).HasColumnName("Contrast");
            this.Property(t => t.Orientation).HasColumnName("Orientation");
            this.Property(t => t.PixelFormat).HasColumnName("PixelFormat");
            this.Property(t => t.BitsPerPixel).HasColumnName("BitsPerPixel");
            this.Property(t => t.ImageContainer).HasColumnName("ImageContainer");
            this.Property(t => t.Id).HasColumnName("Id");*/

            // Relationships
            //this.HasRequired(t => t.BaseMetadata)
            //    .WithOptional(t => t.ImageMetadata);

        }
    }
}
