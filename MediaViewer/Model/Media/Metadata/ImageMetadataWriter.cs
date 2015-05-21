using MediaViewer.MediaDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPLib;

namespace MediaViewer.Model.Media.Metadata
{
    class ImageMetadataWriter : MetadataWriter
    {
        protected override void write(XMPLib.MetaData xmpMetaDataWriter, MediaDatabase.BaseMetadata media)
        {
            ImageMetadata image = media as ImageMetadata;

            if (image.FlashFired != null)
            {
                xmpMetaDataWriter.setStructField(Consts.XMP_NS_EXIF, "Flash", "http://ns.adobe.com/exif/1.0/", "exif:Fired", image.FlashFired.Value.ToString(), 0);
            }
            else
            {
                xmpMetaDataWriter.deleteStructField(Consts.XMP_NS_EXIF, "Flash", "http://ns.adobe.com/exif/1.0/", "exif:Fired");
            }
       
            // integers

            if (image.LightSource != null)
            {
                xmpMetaDataWriter.setProperty_Int(Consts.XMP_NS_EXIF, "LightSource", image.LightSource.Value);
            }
            else
            {
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF, "LightSource");
            }

            if (image.MeteringMode != null)
            {
                xmpMetaDataWriter.setProperty_Int(Consts.XMP_NS_EXIF, "MeteringMode", image.MeteringMode.Value);
            }
            else
            {
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF, "MeteringMode");
            }

            if (image.Saturation != null)
            {
                xmpMetaDataWriter.setProperty_Int(Consts.XMP_NS_EXIF, "Saturation", image.Saturation.Value);
            }
            else
            {
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF, "Saturation");
            }

            if (image.SceneCaptureType != null)
            {
                xmpMetaDataWriter.setProperty_Int(Consts.XMP_NS_EXIF, "SceneCaptureType", image.SceneCaptureType.Value);
            }
            else
            {
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF, "SceneCaptureType");
            }

            if (image.SensingMethod != null)
            {
                xmpMetaDataWriter.setProperty_Int(Consts.XMP_NS_EXIF, "SensingMethod", image.SensingMethod.Value);
            }
            else
            {
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF, "SensingMethod");
            }

            if (image.Sharpness != null)
            {
                xmpMetaDataWriter.setProperty_Int(Consts.XMP_NS_EXIF, "Sharpness", image.Sharpness.Value);
            }
            else
            {
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF, "Sharpness");
            }

            if (image.Orientation != null)
            {
                xmpMetaDataWriter.setProperty_Int(Consts.XMP_NS_EXIF, "Orientation", image.Orientation.Value);
            }
            else
            {
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF, "Orientation");
            }

            if (image.Contrast != null)
            {
                xmpMetaDataWriter.setProperty_Int(Consts.XMP_NS_EXIF, "Contrast", image.Contrast.Value);
            }
            else
            {
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF, "Contrast");
            }

            if (image.SubjectDistanceRange != null)
            {
                xmpMetaDataWriter.setProperty_Int(Consts.XMP_NS_EXIF, "SubjectDistanceRange", image.SubjectDistanceRange.Value);
            }
            else
            {
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF, "SubjectDistanceRange");
            }

            if (image.WhiteBalance != null)
            {
                xmpMetaDataWriter.setProperty_Int(Consts.XMP_NS_EXIF, "WhiteBalance", image.WhiteBalance.Value);
            }
            else
            {
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF, "WhiteBalance");
            }

            if (image.ExposureProgram != null)
            {
                xmpMetaDataWriter.setProperty_Int(Consts.XMP_NS_EXIF, "ExposureProgram", image.ExposureProgram.Value);
            }
            else
            {
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF, "ExposureProgram");
            }

            // strings

            if (image.CameraMake != null)
            {
                xmpMetaDataWriter.setProperty(Consts.XMP_NS_TIFF, "Make", image.CameraMake, Consts.PropOptions.XMP_NoOptions);
            }
            else
            {
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_TIFF, "Make");
            }

            if (image.CameraModel != null)
            {
                xmpMetaDataWriter.setProperty(Consts.XMP_NS_TIFF, "Model", image.CameraModel, Consts.PropOptions.XMP_NoOptions);
            }
            else
            {
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_TIFF, "Model");
            }

            if (image.Lens != null)
            {
                xmpMetaDataWriter.setProperty(Consts.XMP_NS_EXIF_Aux, "Lens", image.Lens, Consts.PropOptions.XMP_NoOptions);
            }
            else
            {
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF_Aux, "Lens");
            }

            if (image.SerialNumber != null)
            {
                xmpMetaDataWriter.setProperty(Consts.XMP_NS_EXIF_Aux, "SerialNumber", image.SerialNumber, Consts.PropOptions.XMP_NoOptions);
            }
            else
            {
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF_Aux, "SerialNumber");
            }

            // fractions

            if (image.ExposureTime != null)
            {
                String result = approximateFraction(image.ExposureTime.Value);

                xmpMetaDataWriter.setProperty(Consts.XMP_NS_EXIF, "ExposureTime", result, 0);
            }
            else
            {
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF, "ExposureTime");
            }

            if (image.FNumber != null)
            {
                String result = approximateFraction(image.FNumber.Value);

                xmpMetaDataWriter.setProperty(Consts.XMP_NS_EXIF, "FNumber", result, 0);
            }
            else
            {
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF, "FNumber");
            }

            if (image.ExposureBiasValue != null)
            {
                String result = approximateFraction(image.ExposureBiasValue.Value);

                xmpMetaDataWriter.setProperty(Consts.XMP_NS_EXIF, "ExposureBiasValue", result, 0);
            }
            else
            {
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF, "ExposureBiasValue");
            }

            if (image.FocalLength != null)
            {
                String result = approximateFraction(image.FocalLength.Value);

                xmpMetaDataWriter.setProperty(Consts.XMP_NS_EXIF, "FocalLength", result, 0);
            }
            else
            {
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF, "FocalLength");
            }

            if (image.SubjectDistance != null)
            {
                String result = approximateFraction(image.SubjectDistance.Value);

                xmpMetaDataWriter.setProperty(Consts.XMP_NS_EXIF, "SubjectDistance", result, 0);
            }
            else
            {
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF, "SubjectDistance");
            }

            if(image.ShutterSpeedValue != null)
            {
                String result = approximateFraction(image.ShutterSpeedValue.Value);

                xmpMetaDataWriter.setProperty(Consts.XMP_NS_EXIF, "ShutterSpeedValue", result, 0);

            } else {

                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF, "ShutterSpeedValue");
            }

            if (image.ISOSpeedRating != null)
            {
                if (xmpMetaDataWriter.doesArrayItemExist(Consts.XMP_NS_EXIF, "ISOSpeedRatings", 1))
                {
                    xmpMetaDataWriter.setArrayItem(Consts.XMP_NS_EXIF, "ISOSpeedRatings", 1, image.ISOSpeedRating.Value.ToString(), 0);
                }
                else
                {
                    xmpMetaDataWriter.appendArrayItem(Consts.XMP_NS_EXIF, "ISOSpeedRatings",
                        Consts.PropOptions.XMP_PropArrayIsOrdered, image.ISOSpeedRating.Value.ToString(), 0);
                }               
            }
            else
            {
                xmpMetaDataWriter.deleteProperty(Consts.XMP_NS_EXIF, "ISOSpeedRatings");
            }
         
            base.write(xmpMetaDataWriter, media);
        }

        private static String approximateFraction(double value)
        {
            const double EPSILON = .000001d;

            int n = 1;  // numerator
            int d = 1;  // denominator
            double fraction = n / d;

            while (System.Math.Abs(fraction - value) > EPSILON)
            {
                if (fraction < value)
                {
                    n++;
                }
                else
                {
                    d++;
                    n = (int)System.Math.Round(value * d);
                }

                fraction = n / (double)d;
            }

            return (n.ToString() + "/" + d.ToString());
            
        }
    }
}
