using MediaViewer.MediaDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.File.Metadata
{

public class FormatMetaData
{
    

	// convert metadata properties to human readable strings
	class FormatInfo {

		public FormatInfo(String name, String type) {

			this.name = name;
			this.type = type;
			
			lookup = new Dictionary<String , String >();
			
		}

		public String name;
		public String type;
		public Dictionary<String , String > lookup;
	};

	static Dictionary<String , FormatInfo > format = null;

	static void initFormatDictionary() {

		if(format != null) return;

		format = new Dictionary<String , FormatInfo >();

		FormatInfo i = new FormatInfo("ColorMode", "");

		i.lookup["0"] = "Bitmap";
		i.lookup["1"] = "Gray scale";
		i.lookup["2"] = "Indexed colour";
		i.lookup["3"] = "RGB colour";
		i.lookup["4"] = "CMYK colour";
		i.lookup["7"] = "Multi-channel";
		i.lookup["8"] = "Duotone";
		i.lookup["9"] = "LAB colour";

		format.Add(i.name, i);

		i = new FormatInfo("Compression", "");
		i.lookup["1"] = "uncompressed";
		i.lookup["6"] = "jpeg";

		format.Add(i.name, i);

		i = new FormatInfo("ImageLength", "pixels");

		format.Add(i.name, i);

		i = new FormatInfo("ImageWidth", "pixels");

		format.Add(i.name, i);

		i = new FormatInfo("PhotometricInterpretation", "");;
		i.lookup["2"] = "RGB";
		i.lookup["6"] = "YCbCr";

		format.Add(i.name, i);

		i = new FormatInfo("PlanarConfiguration", "");
		i.lookup["1"] = "chunky";
		i.lookup["2"] = "planar";

		format.Add(i.name, i);

		i = new FormatInfo("ResolutionUnit", "");
		i.lookup["2"] = "inches";
		i.lookup["3"] = "centimeters";

		format.Add(i.name, i);

		i = new FormatInfo("XResolution", "pixels per unit");

		format.Add(i.name, i);

		i = new FormatInfo("YResolution", "pixels per unit");

		format.Add(i.name, i);

		i = new FormatInfo("YCbCrPositioning", "");
		i.lookup["1"] = "centered";
		i.lookup["2"] = "co-sited";

		format.Add(i.name, i);

		i = new FormatInfo("ApertureValue", "");

		format.Add(i.name, i);

		i = new FormatInfo("BrightnessValue", "");

		format.Add(i.name, i);

		i = new FormatInfo("ColorSpace", "");
		i.lookup["1"] = "sRGB";
		i.lookup["65535"] = "uncalibrated";

		format.Add(i.name, i);

		i = new FormatInfo("ComponentsConfiguration", "");
		i.lookup["0"] = "does not exist";
		i.lookup["1"] = "Y";
		i.lookup["2"] = "Cb";
		i.lookup["3"] = "Cr";
		i.lookup["4"] = "R";
		i.lookup["5"] = "G";
		i.lookup["6"] = "B";
		
		format.Add(i.name + "[]", i);
		/*format.Add(i.name + "[2]", i);
		format.Add(i.name + "[3]", i);
		format.Add(i.name + "[4]", i);*/

		i = new FormatInfo("Contrast", "");
		i.lookup["0"] = "Normal";
		i.lookup["1"] = "Soft";
		i.lookup["2"] = "Hard";

		format.Add(i.name, i);

		i = new FormatInfo("CustomRendered", "");
		i.lookup["0"] = "Normal process";
		i.lookup["1"] = "Custom process";

		format.Add(i.name, i);

		i = new FormatInfo("ExposureBiasValue", "EV");

		format.Add(i.name, i);

		i = new FormatInfo("ExposureMode", "");
		i.lookup["0"] = "Auto exposure";
		i.lookup["1"] = "Manual exposure";
		i.lookup["2"] = "Auto bracket";

		format.Add(i.name, i);

		i = new FormatInfo("ExposureProgram", "");

		i.lookup["0"] = "not defined";
		i.lookup["1"] = "Manual";
		i.lookup["2"] = "Normal program";
		i.lookup["3"] = "Aperture priority";
		i.lookup["4"] = "Shutter priority";
		i.lookup["5"] = "Creative program";
		i.lookup["6"] = "Action program";
		i.lookup["7"] = "Portrait mode";
		i.lookup["8"] = "Landscape mode";

		format.Add(i.name, i);

		i = new FormatInfo("ExposureTime", "seconds");

		format.Add(i.name, i);

		i = new FormatInfo("FileSource", "");
		i.lookup["3"] = "DSC";

		format.Add(i.name, i);

		i = new FormatInfo("FocalLength", "millimeters");

		format.Add(i.name, i);
		
		i = new FormatInfo("FocalPlaneResolutionUnit", "");
		i.lookup["2"] = "inches";
		i.lookup["3"] = "centimeters";

		format.Add(i.name, i);

		i = new FormatInfo("FocalPlaneXResolution", "pixels per unit");

		format.Add(i.name, i);

		i = new FormatInfo("FocalPlaneYResolution", "pixels per unit");

		format.Add(i.name, i);

		i = new FormatInfo("GainControl", "");

		i.lookup["0"] = "None";
		i.lookup["1"] = "Low gain up";
		i.lookup["2"] = "High gain up";
		i.lookup["3"] = "Low gain down";
		i.lookup["4"] = "High gain down";

		format.Add(i.name, i);

		i = new FormatInfo("GPSAltitudeRef", "");

		i.lookup["0"] = "Above sea level";
		i.lookup["1"] = "Below sea level";

		format.Add(i.name, i);

		i = new FormatInfo("GPSDestBearingRef", "");

		i.lookup["T"] = "true direction";
		i.lookup["M"] = "magnetic direction";

		format.Add(i.name, i);

		i = new FormatInfo("GPSDestDistanceRef", "");

		i.lookup["K"] = "kilometers per hour";
		i.lookup["M"] = "miles per hour";
		i.lookup["N"] = "knots";

		format.Add(i.name, i);

		i = new FormatInfo("GPSDifferential", "");

		i.lookup["0"] = "Without correction";
		i.lookup["1"] = "Correction applied";

		format.Add(i.name, i);

		i = new FormatInfo("GPSImgDirectionRef", "");

		i.lookup["T"] = "true direction";
		i.lookup["M"] = "magnetic direction";

		format.Add(i.name, i);

		i = new FormatInfo("GPSMeasureMode", "");

		i.lookup["2"] = "two-dimensional measurement";
		i.lookup["3"] = "three-dimensional measurement";

		format.Add(i.name, i);

		i = new FormatInfo("GPSTrackRef", "");

		i.lookup["T"] = "true direction";
		i.lookup["M"] = "magnetic direction";

		format.Add(i.name, i);

		i = new FormatInfo("GPSSpeedRef", "");

		i.lookup["K"] = "kilometers per hour";
		i.lookup["M"] = "miles per hour";
		i.lookup["N"] = "knots";

		format.Add(i.name, i);

		i = new FormatInfo("GPSStatus", "");

		i.lookup["A"] = "Measurement in progress";
		i.lookup["V"] = "Measurement is interoperability";

		format.Add(i.name, i);

		i = new FormatInfo("LightSource", "");

		i.lookup["0"] = "Unknown";
		i.lookup["1"] = "Daylight";
		i.lookup["2"] = "Fluorescent";
		i.lookup["3"] = "Tungsten";
		i.lookup["4"] = "Flash";
		i.lookup["9"] = "Fine weather";
		i.lookup["10"] = "Cloudy weather";
		i.lookup["11"] = "Shade";
		i.lookup["12"] = "Daylight fluorescent (D 5700 – 7100K)";
		i.lookup["13"] = "Day white fluorescent (N 4600 – 5400K)";
		i.lookup["14"] = "Cool white fluorescent (W 3900 – 4500K)";
		i.lookup["15"] = "White fluorescent (WW 3200 – 3700K)";
		i.lookup["17"] = "Standard light A";
		i.lookup["18"] = "Standard light B";
		i.lookup["19"] = "Standard light C";
		i.lookup["20"] = "D55";
		i.lookup["21"] = "D65";
		i.lookup["22"] = "D75";
		i.lookup["23"] = "D50";
		i.lookup["24"] = "ISO studio tungsten";
		i.lookup["255"] = "other";

		format.Add(i.name, i);

		i = new FormatInfo("MaxApertureValue", "");

		format.Add(i.name, i);

		i = new FormatInfo("MeteringMode", "");

		i.lookup["0"] = "Unknown";
		i.lookup["1"] = "Average";
		i.lookup["2"] = "CenterWeightedAverage";
		i.lookup["3"] = "Spot";
		i.lookup["4"] = "MultiSpot";
		i.lookup["5"] = "Pattern";
		i.lookup["6"] = "Partial";
		i.lookup["7"] = "Other";

		format.Add(i.name, i);

		i = new FormatInfo("PixelXDimension", "pixels");

		format.Add(i.name, i);

		i = new FormatInfo("PixelYDimension", "pixels");

		format.Add(i.name, i);

		i = new FormatInfo("Saturation", "");

		i.lookup["0"] = "Normal saturation";
		i.lookup["1"] = "Low saturation";
		i.lookup["2"] = "High saturation";

		format.Add(i.name, i);

		i = new FormatInfo("SceneCaptureType", "");

		i.lookup["0"] = "Standard";
		i.lookup["1"] = "Landscape";
		i.lookup["2"] = "Portrait";
		i.lookup["3"] = "Night scene";

		format.Add(i.name, i);

		i = new FormatInfo("SceneType", "");

		i.lookup["1"] = "Directly photographed image";
		
		format.Add(i.name, i);

		i = new FormatInfo("SensingMethod", "");

		i.lookup["1"] = "Not defined";
		i.lookup["2"] = "One-chip colour area sensor";
		i.lookup["3"] = "Two-chip colour area sensor";
		i.lookup["4"] = "Three-chip colour area sensor";
		i.lookup["5"] = "Colour sequential area sensor";
		i.lookup["7"] = "Trilinear sensor";
		i.lookup["8"] = "Colour sequential linear sensor";

		format.Add(i.name, i);

		i = new FormatInfo("Sharpness", "");

		i.lookup["0"] = "Normal";
		i.lookup["1"] = "Soft";
		i.lookup["2"] = "Hard";

		format.Add(i.name, i);

		i = new FormatInfo("ShutterSpeedValue", "");

		format.Add(i.name, i);

		i = new FormatInfo("SubjectDistance", "meters");

		format.Add(i.name, i);

		i = new FormatInfo("SubjectDistanceRange", "");

		i.lookup["0"] = "Unknown";
		i.lookup["1"] = "Macro";
		i.lookup["2"] = "Close view";
		i.lookup["3"] = "Distant view";

		format.Add(i.name, i);

		i = new FormatInfo("WhiteBalance", "");

		i.lookup["0"] = "Auto white balance";
		i.lookup["1"] = "Manual white balance";

		format.Add(i.name, i);

		i = new FormatInfo("FlashMode", "");

		i.lookup["0"] = "unknown";
		i.lookup["1"] = "compulsory flash firing";
		i.lookup["2"] = "compulsory flash suppression";
		i.lookup["3"] = "auto mode";

		format.Add(i.name, i);

		i = new FormatInfo("FlashReturn", "");

		i.lookup["0"] = "no strobe return detection";
		i.lookup["2"] = "strobe return light not detected";
		i.lookup["3"] = "strobe return light detected";
		
		format.Add(i.name, i);

        i = new FormatInfo("FlashFired", "");

        i.lookup["False"] = "Did not fire";
        i.lookup["True"] = "Fired";

        format.Add(i.name, i);

        i = new FormatInfo("Orientation", "");
        i.lookup["1"] = "Horizontal";
        i.lookup["2"] = "Mirror horizontal";
        i.lookup["3"] = "Rotate 180°";
        i.lookup["4"] = "Mirror vertical";
        i.lookup["5"] = "Mirror horizontal, rotate 270°";
        i.lookup["6"] = "Rotate 90°";
        i.lookup["7"] = "Mirror horizontal, rotate 90°";
        i.lookup["8"] = "Rotate 270°";

        format.Add(i.name, i);
	}

    private static XMPLib.MetaDataProperty findProp(String path, List<XMPLib.MetaDataProperty> props)
    {

       XMPLib.MetaDataProperty result = props.Find(p => p.path == path);
       return (result);
    }

    private static void addPropIfExists<T>(String path, T prop, List<Tuple<String, String>> propList)
    {       
        if (prop != null)
        {
            Tuple<String, String> tuple = 
                new Tuple<string, string>(formatPropertyName(path), formatPropertyValue(path, prop.ToString()));
            
            propList.Add(tuple);
        }
       
    }

    private static void addPropIfExists<T>(String path, T prop, List<Tuple<String, String>> propList,
        string overrideName)
    {
        if (prop != null)
        {
            Tuple<String, String> tuple =
                 new Tuple<string, string>(overrideName, formatPropertyValue(path, prop.ToString()));
          

            propList.Add(tuple);
        }
    }


    public static List<Tuple<String, String>> formatProperties(VideoMetadata video)
    {
        List<Tuple<String, String>> propList = new List<Tuple<string, string>>();
       
        addPropIfExists("Major Brand", video.MajorBrand, propList);
        addPropIfExists("Minor Version", video.MinorVersion, propList);
        addPropIfExists("WMF SDK Version", video.WMFSDKVersion, propList);
        addPropIfExists("Variable Bitrate", video.IsVariableBitRate, propList);

        return (propList);
    }

    public static List<Tuple<String, String>> formatProperties(ImageMetadata image)
    {
        // http://ptgmedia.pearsoncmg.com/images/art_evening_lrmetadata/elementLinks/fig04.jpg

        List<Tuple<String, String>> propList = new List<Tuple<string, string>>();
       
        if (image.ExposureTime.HasValue)
        {
            propList.Add(new Tuple<string,string>("Exposure Time", "1/" + 1/image.ExposureTime.Value + "s"));
        }
       
        addPropIfExists("FNumber", image.FNumber, propList, "ƒ");
        addPropIfExists("ISOSpeedRating", image.ISOSpeedRating, propList, "ISO Speed Rating");
        addPropIfExists("ExposureBiasValue", image.ExposureBiasValue, propList, "Exposure Bias");
        addPropIfExists("FlashFired", image.FlashFired, propList, "Flash");

        if (image.FlashFired != null && image.FlashFired == true)
        {
            addPropIfExists("FlashMode", image.FlashMode, propList, "Flash Mode");
            addPropIfExists("FlashReturn", image.FlashReturn, propList, "Flash Return");
        }

        addPropIfExists("ExposureProgram", image.ExposureProgram, propList);
        addPropIfExists("MeteringMode", image.MeteringMode, propList);
/*
        XMPLib.MetaDataProperty isoSpeedRatings = findProp("ISOSpeedRatings[1]", props);

        if (isoSpeedRatings != null)
        {
            propList.Add(new Tuple<string, string>("ISO Speed Rating", "ISO " + isoSpeedRatings.value));
        }
*/
        addPropIfExists("FocalLength", image.FocalLength, propList);
        addPropIfExists("Lens", image.Lens, propList);
        addPropIfExists("WhiteBalance", image.WhiteBalance, propList);

        addPropIfExists("SensingMethod", image.SensingMethod, propList);
        addPropIfExists("Sharpness", image.Sharpness, propList);
        addPropIfExists("Saturation", image.Saturation, propList);
        addPropIfExists("Contrast", image.Contrast, propList);
        addPropIfExists("LightSource", image.LightSource, propList);
        addPropIfExists("SceneCaptureType", image.SceneCaptureType, propList);
        addPropIfExists("SubjectDistance", image.SubjectDistance, propList);
        addPropIfExists("SubjectDistanceRange", image.SubjectDistanceRange, propList);

        addPropIfExists("Make", image.CameraMake, propList);
        addPropIfExists("Model", image.CameraModel, propList);
        addPropIfExists("SerialNumber", image.SerialNumber, propList);
        addPropIfExists("Orientation", image.Orientation, propList);
  
        return (propList);
    }

    public static String formatPropertyName(String prop)
    {

		 if(String.IsNullOrEmpty(prop)) return("");

		 String result = Char.ToString(prop[prop.Length - 1]);

		 for(int i = prop.Length - 2; i >= 0; i--) {

			 if(prop[i].Equals(':')) break;

			 if(Char.IsLower(prop[i]) && Char.IsUpper(prop[i + 1])) {

				 result = " " + result;
			 } 

			 result = prop[i] + result;

			 if(i - 1 >= 0) {

				  if(Char.IsUpper(prop[i - 1]) && Char.IsUpper(prop[i]) && Char.IsLower(prop[i + 1])) {

					 result = " " + result;
				  } 
			 }
							 
		 }

		 return(result);
	 }

    public static String formatPropertyValue(String name, String value)
    {

		if(String.IsNullOrEmpty(name) || String.IsNullOrEmpty(value)) return("");

		String result = "";

		FormatInfo info;

		if(format.TryGetValue(name, out info) == false) {

			return(value);
		}

		if(info.lookup.Count == 0) {

			result = value;

		} else {

			if(info.lookup.TryGetValue(value, out result) == false) {

				return(value);
			}
		}

		result += " " + info.type;

		return(result);

	}

	static FormatMetaData() {

		initFormatDictionary();
	}

}



}
