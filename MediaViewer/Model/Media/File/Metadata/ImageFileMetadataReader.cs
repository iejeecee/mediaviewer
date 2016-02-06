using MediaViewer.Infrastructure;
using MediaViewer.Infrastructure.Logging;
using MediaViewer.Infrastructure.Utils;
using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using XMPLib;
using MediaViewer.Model.Media.Base.Metadata;
using VideoLib;


namespace MediaViewer.Model.Media.File.Metadata
{
    class ImageFileMetadataReader : MetadataFileReader
    {

        public override void readMetadata(VideoPreview mediaPreview, Stream data, MetadataFactory.ReadOptions options, BaseMetadata media,
            CancellationToken token, int timeoutSeconds)
        {
            ImageMetadata image = media as ImageMetadata;
            image.SizeBytes = data.Length;

            if (FileUtils.isUrl(image.Location))
            {
                image.SupportsXMPMetadata = false;
            }
            else
            {
                image.SupportsXMPMetadata = true;
                base.readMetadata(mediaPreview, data, options, media, token, timeoutSeconds);
            }

            image.ImageContainer = mediaPreview.Container;
            image.Width = mediaPreview.Width;
            image.Height = mediaPreview.Height;
            image.PixelFormat = mediaPreview.PixelFormat;
            image.BitsPerPixel = (short)mediaPreview.BitsPerPixel;
           
            List<String> fsMetaData = mediaPreview.MetaData;
               
            try
            {                   
                if (options.HasFlag(MetadataFactory.ReadOptions.GENERATE_THUMBNAIL) ||
                    options.HasFlag(MetadataFactory.ReadOptions.GENERATE_MULTIPLE_THUMBNAILS))
                {
                    generateThumbnail(mediaPreview, image, token, timeoutSeconds, 1);
                }
                    
            }
            catch (Exception e)
            {
                Logger.Log.Error("Cannot create image thumbnail: " + image.Location, e);
                media.MetadataReadError = e;
            }
            
                        
        }

        public void generateThumbnail(VideoPreview mediaPreview, ImageMetadata image,
            CancellationToken token, int timeoutSeconds, int nrThumbnails)
        {
          
            // possibly could not seek in video, try to get the first frame in the video
            List<VideoThumb> thumbBitmaps = mediaPreview.grabThumbnails(Constants.MAX_THUMBNAIL_WIDTH,
                Constants.MAX_THUMBNAIL_HEIGHT, -1, 1, 0, token, timeoutSeconds);
           
            image.Thumbnails.Clear();

            foreach (VideoThumb imageThumb in thumbBitmaps)
            {
                BitmapSource thumb = imageThumb.Thumb;

                if(image.Orientation.HasValue &&  image.Orientation.Value != 1) {
                
                    System.Windows.Media.Transform transform = null;

                    switch(image.Orientation.Value)
                    {                        
                        case 2: 
                            {
                                //Mirror horizontal
                                transform = new System.Windows.Media.MatrixTransform(-1, 0, 0, 1, 0, 0);
                                break;
                            }
                   
                        case 3: 
                            {
                                //Rotate 180°
                                transform = new System.Windows.Media.RotateTransform(180);
                                break;
                            }
                        case 4: 
                            {
                                //Mirror vertical
                                transform = new System.Windows.Media.MatrixTransform(1, 0, 0, -1, 0, 0);
                                break;
                            }
                        case 5: 
                            {
                                //Mirror horizontal, rotate 270°
                                transform = new System.Windows.Media.MatrixTransform(0, -1, -1, 0, 0, 0);
                                break;
                            }
                        case 6: 
                            {
                                //Rotate 90°
                                transform = new System.Windows.Media.RotateTransform(90);
                                break;
                            }
                        case 7: 
                            {
                                //Mirror horizontal, rotate 90°
                                transform = new System.Windows.Media.MatrixTransform(0, 1, 1, 0, 0, 0);
                                break;
                            }
                        case 8: 
                            {
                                //Rotate 270°
                                transform = new System.Windows.Media.RotateTransform(270);
                                break;
                            }
                        default:
                            {
                                Logger.Log.Warn("Unknown orientation for image");
                                break;
                            }

                    }

                    if (transform != null)
                    {
                        thumb = new TransformedBitmap(thumb, transform);
                    }
                }

                image.Thumbnails.Add(new Thumbnail(thumb));
            }

        }

        Nullable<double> parseRational(String rational)
        {
            Nullable<double> result = new Nullable<double>();

            if (rational == null)
            {
                return (result);
            }

            try
            {
                char[] splitter = new char[] { '/' };

                string[] split = rational.Split(splitter);

                Int64 teller = 0;
                Int64 noemer = 0;
                double value = 0;

                if (split.Length > 0)
                {
                    teller = Int64.Parse(split[0]);
                    value = teller;
                }

                if (split.Length > 1)
                {
                    noemer = int.Parse(split[1]);
                    if (noemer != 0)
                    {
                        value = teller / (double)noemer;
                    }
                    else
                    {
                        value = 0;
                    }
                }

                result = value;
            }
            catch (Exception)
            {
                result = null;
            }

            return (result);
        }

        protected override void readXMPMetadata(XMPLib.MetaData xmpMetaDataReader, BaseMetadata media)
        {

            base.readXMPMetadata(xmpMetaDataReader, media);

            ImageMetadata image = media as ImageMetadata;

            Nullable<int> intVal = new Nullable<int>();
            String temp = "";
            bool exists = false;

            exists = xmpMetaDataReader.getStructField(Consts.XMP_NS_EXIF, "Flash", "http://ns.adobe.com/exif/1.0/", "exif:Fired", ref temp);
            if (exists)
            {
                try
                {
                    image.FlashFired = Boolean.Parse(temp);
                }
                catch (Exception)
                {
                    image.FlashFired = null;
                }
            }
            else
            {
                image.FlashFired = null;
            }

            xmpMetaDataReader.getProperty_Int(Consts.XMP_NS_EXIF, "LightSource", ref intVal);
            if (intVal == null)
            {
                image.LightSource = null;
            }
            else
            {
                image.LightSource = (short)intVal;
            }

            xmpMetaDataReader.getProperty_Int(Consts.XMP_NS_EXIF, "MeteringMode", ref intVal);
            if (intVal == null)
            {
                image.MeteringMode = null;
            }
            else
            {
                image.MeteringMode = (short)intVal;
            }

            xmpMetaDataReader.getProperty_Int(Consts.XMP_NS_EXIF, "Saturation", ref intVal);
            if (intVal == null)
            {
                image.Saturation = null;
            }
            else
            {
                image.Saturation = (short)intVal;
            }


            xmpMetaDataReader.getProperty_Int(Consts.XMP_NS_EXIF, "SceneCaptureType", ref intVal);
            if (intVal == null)
            {
                image.SceneCaptureType = null;
            }
            else
            {
                image.SceneCaptureType = (short)intVal;
            }

            xmpMetaDataReader.getProperty_Int(Consts.XMP_NS_EXIF, "SensingMethod", ref intVal);
            if (intVal == null)
            {
                image.SensingMethod = null;
            }
            else
            {
                image.SensingMethod = (short)intVal;
            }

            xmpMetaDataReader.getProperty_Int(Consts.XMP_NS_EXIF, "Sharpness", ref intVal);
            if (intVal == null)
            {
                image.Sharpness = null;
            }
            else
            {
                image.Sharpness = (short)intVal;
            }

            xmpMetaDataReader.getProperty_Int(Consts.XMP_NS_TIFF, "Orientation", ref intVal);
            if (intVal == null)
            {
                image.Orientation = null;
            }
            else
            {
                image.Orientation = (short)intVal;
            }

            string subjectDistance = "";

            xmpMetaDataReader.getProperty(Consts.XMP_NS_EXIF, "SubjectDistance", ref subjectDistance);
            image.SubjectDistance = parseRational(subjectDistance);

            string shutterSpeedValue = "";

            xmpMetaDataReader.getProperty(Consts.XMP_NS_EXIF, "ShutterSpeedValue", ref shutterSpeedValue);
            image.ShutterSpeedValue = parseRational(shutterSpeedValue);

            xmpMetaDataReader.getProperty_Int(Consts.XMP_NS_EXIF, "SubjectDistanceRange", ref intVal);
            if (intVal == null)
            {
                image.SubjectDistanceRange = null;
            }
            else
            {
                image.SubjectDistanceRange = (short)intVal;
            }

            string isoSpeedRating = "";

            int nrSpeedRatings = xmpMetaDataReader.countArrayItems(Consts.XMP_NS_EXIF, "ISOSpeedRatings");
            if (nrSpeedRatings > 0)
            {
                xmpMetaDataReader.getArrayItem(Consts.XMP_NS_EXIF, "ISOSpeedRatings", 1, ref isoSpeedRating);
                int value = 0;
                if (int.TryParse(isoSpeedRating, out value))
                {
                    image.ISOSpeedRating = value;
                }
            }
            else
            {
                image.ISOSpeedRating = null;
            }

            xmpMetaDataReader.getProperty_Int(Consts.XMP_NS_EXIF, "WhiteBalance", ref intVal);
            if (intVal == null)
            {
                image.WhiteBalance = null;
            }
            else
            {
                image.WhiteBalance = (short)intVal;
            }

            String cameraMake = "";

            xmpMetaDataReader.getProperty(Consts.XMP_NS_TIFF, "Make", ref cameraMake);
            image.CameraMake = cameraMake;

            String cameraModel = "";

            xmpMetaDataReader.getProperty(Consts.XMP_NS_TIFF, "Model", ref cameraModel);
            image.CameraModel = cameraModel;

            String lens = "";

            xmpMetaDataReader.getProperty(Consts.XMP_NS_EXIF_Aux, "Lens", ref lens);
            image.Lens = lens;

            String serialNumber = "";

            xmpMetaDataReader.getProperty(Consts.XMP_NS_EXIF_Aux, "SerialNumber", ref serialNumber);
            image.SerialNumber = serialNumber;

            string exposureTime = "";

            xmpMetaDataReader.getProperty(Consts.XMP_NS_EXIF, "ExposureTime", ref exposureTime);
            image.ExposureTime = parseRational(exposureTime);

            string fnumber = "";

            xmpMetaDataReader.getProperty(Consts.XMP_NS_EXIF, "FNumber", ref fnumber);
            image.FNumber = parseRational(fnumber);

            string exposureBiasValue = "";

            xmpMetaDataReader.getProperty(Consts.XMP_NS_EXIF, "ExposureBiasValue", ref exposureBiasValue);
            image.ExposureBiasValue = parseRational(exposureBiasValue);

            xmpMetaDataReader.getProperty_Int(Consts.XMP_NS_EXIF, "ExposureProgram", ref intVal);
            if (intVal == null)
            {
                image.ExposureProgram = null;
            }
            else
            {
                image.ExposureProgram = (short)intVal;
            }

            string focalLength = "";

            xmpMetaDataReader.getProperty(Consts.XMP_NS_EXIF, "FocalLength", ref focalLength);
            image.FocalLength = parseRational(focalLength);

            xmpMetaDataReader.getProperty_Int(Consts.XMP_NS_EXIF, "Contrast", ref intVal);
            if (intVal == null)
            {
                image.Contrast = null;
            }
            else
            {
                image.Contrast = (short)intVal;
            }
        }

    }
}
