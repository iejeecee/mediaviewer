using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MetaData
{

public class FormatMetaData
{
    private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

		FormatInfo i = new FormatInfo("photoshop:ColorMode", "");

		i.lookup["0"] = "Bitmap";
		i.lookup["1"] = "Gray scale";
		i.lookup["2"] = "Indexed colour";
		i.lookup["3"] = "RGB colour";
		i.lookup["4"] = "CMYK colour";
		i.lookup["7"] = "Multi-channel";
		i.lookup["8"] = "Duotone";
		i.lookup["9"] = "LAB colour";

		format.Add(i.name, i);

		i = new FormatInfo("tiff:Compression", "");
		i.lookup["1"] = "uncompressed";
		i.lookup["6"] = "jpeg";

		format.Add(i.name, i);

		i = new FormatInfo("tiff:ImageLength", "pixels");

		format.Add(i.name, i);

		i = new FormatInfo("tiff:ImageWidth", "pixels");

		format.Add(i.name, i);

		i = new FormatInfo("tiff:Orientation", "");

		i.lookup["1"] = "0th row at top, 0th column at left";
		i.lookup["2"] = "0th row at top, 0th column at right";
		i.lookup["3"] = "0th row at bottom, 0th column at right";
		i.lookup["4"] = "0th row at bottom, 0th column at left";
		i.lookup["5"] = "0th row at left, 0th column at top";
		i.lookup["6"] = "0th row at right, 0th column at top";
		i.lookup["7"] = "0th row at right, 0th column at bottom";
		i.lookup["8"] = "0th row at left, 0th column at bottom";

		format.Add(i.name, i);

		i = new FormatInfo("tiff:PhotometricInterpretation", "");;
		i.lookup["2"] = "RGB";
		i.lookup["6"] = "YCbCr";

		format.Add(i.name, i);

		i = new FormatInfo("tiff:PlanarConfiguration", "");
		i.lookup["1"] = "chunky";
		i.lookup["2"] = "planar";

		format.Add(i.name, i);

		i = new FormatInfo("tiff:ResolutionUnit", "");
		i.lookup["2"] = "inches";
		i.lookup["3"] = "centimeters";

		format.Add(i.name, i);

		i = new FormatInfo("tiff:XResolution", "pixels per unit");

		format.Add(i.name, i);

		i = new FormatInfo("tiff:YResolution", "pixels per unit");

		format.Add(i.name, i);

		i = new FormatInfo("tiff:YCbCrPositioning", "");
		i.lookup["1"] = "centered";
		i.lookup["2"] = "co-sited";

		format.Add(i.name, i);

		i = new FormatInfo("exif:ApertureValue", "");

		format.Add(i.name, i);

		i = new FormatInfo("exif:BrightnessValue", "");

		format.Add(i.name, i);

		i = new FormatInfo("exif:ColorSpace", "");
		i.lookup["1"] = "sRGB";
		i.lookup["65535"] = "uncalibrated";

		format.Add(i.name, i);

		i = new FormatInfo("exif:ComponentsConfiguration", "");
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

		i = new FormatInfo("exif:Contrast", "");
		i.lookup["0"] = "Normal";
		i.lookup["1"] = "Soft";
		i.lookup["2"] = "Hard";

		format.Add(i.name, i);

		i = new FormatInfo("exif:CustomRendered", "");
		i.lookup["0"] = "Normal process";
		i.lookup["1"] = "Custom process";

		format.Add(i.name, i);

		i = new FormatInfo("exif:ExposureBiasValue", "EV");

		format.Add(i.name, i);

		i = new FormatInfo("exif:ExposureMode", "");
		i.lookup["0"] = "Auto exposure";
		i.lookup["1"] = "Manual exposure";
		i.lookup["2"] = "Auto bracket";

		format.Add(i.name, i);

		i = new FormatInfo("exif:ExposureProgram", "");

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

		i = new FormatInfo("exif:ExposureTime", "seconds");

		format.Add(i.name, i);

		i = new FormatInfo("exif:FileSource", "");
		i.lookup["3"] = "DSC";

		format.Add(i.name, i);

		i = new FormatInfo("exif:FocalLength", "millimeters");

		format.Add(i.name, i);
		
		i = new FormatInfo("exif:FocalPlaneResolutionUnit", "");
		i.lookup["2"] = "inches";
		i.lookup["3"] = "centimeters";

		format.Add(i.name, i);

		i = new FormatInfo("exif:FocalPlaneXResolution", "pixels per unit");

		format.Add(i.name, i);

		i = new FormatInfo("exif:FocalPlaneYResolution", "pixels per unit");

		format.Add(i.name, i);

		i = new FormatInfo("exif:GainControl", "");

		i.lookup["0"] = "None";
		i.lookup["1"] = "Low gain up";
		i.lookup["2"] = "High gain up";
		i.lookup["3"] = "Low gain down";
		i.lookup["4"] = "High gain down";

		format.Add(i.name, i);

		i = new FormatInfo("exif:GPSAltitudeRef", "");

		i.lookup["0"] = "Above sea level";
		i.lookup["1"] = "Below sea level";

		format.Add(i.name, i);

		i = new FormatInfo("exif:GPSDestBearingRef", "");

		i.lookup["T"] = "true direction";
		i.lookup["M"] = "magnetic direction";

		format.Add(i.name, i);

		i = new FormatInfo("exif:GPSDestDistanceRef", "");

		i.lookup["K"] = "kilometers per hour";
		i.lookup["M"] = "miles per hour";
		i.lookup["N"] = "knots";

		format.Add(i.name, i);

		i = new FormatInfo("exif:GPSDifferential", "");

		i.lookup["0"] = "Without correction";
		i.lookup["1"] = "Correction applied";

		format.Add(i.name, i);

		i = new FormatInfo("exif:GPSImgDirectionRef", "");

		i.lookup["T"] = "true direction";
		i.lookup["M"] = "magnetic direction";

		format.Add(i.name, i);

		i = new FormatInfo("exif:GPSMeasureMode", "");

		i.lookup["2"] = "two-dimensional measurement";
		i.lookup["3"] = "three-dimensional measurement";

		format.Add(i.name, i);

		i = new FormatInfo("exif:GPSTrackRef", "");

		i.lookup["T"] = "true direction";
		i.lookup["M"] = "magnetic direction";

		format.Add(i.name, i);

		i = new FormatInfo("exif:GPSSpeedRef", "");

		i.lookup["K"] = "kilometers per hour";
		i.lookup["M"] = "miles per hour";
		i.lookup["N"] = "knots";

		format.Add(i.name, i);

		i = new FormatInfo("exif:GPSStatus", "");

		i.lookup["A"] = "Measurement in progress";
		i.lookup["V"] = "Measurement is interoperability";

		format.Add(i.name, i);

		i = new FormatInfo("exif:LightSource", "");

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

		i = new FormatInfo("exif:MaxApertureValue", "");

		format.Add(i.name, i);

		i = new FormatInfo("exif:MeteringMode", "");

		i.lookup["0"] = "Unknown";
		i.lookup["1"] = "Average";
		i.lookup["2"] = "CenterWeightedAverage";
		i.lookup["3"] = "Spot";
		i.lookup["4"] = "MultiSpot";
		i.lookup["5"] = "Pattern";
		i.lookup["6"] = "Partial";
		i.lookup["7"] = "Other";

		format.Add(i.name, i);

		i = new FormatInfo("exif:PixelXDimension", "pixels");

		format.Add(i.name, i);

		i = new FormatInfo("exif:PixelYDimension", "pixels");

		format.Add(i.name, i);

		i = new FormatInfo("exif:Saturation", "");

		i.lookup["0"] = "Normal saturation";
		i.lookup["1"] = "Low saturation";
		i.lookup["2"] = "High saturation";

		format.Add(i.name, i);

		i = new FormatInfo("exif:SceneCaptureType", "");

		i.lookup["0"] = "Standard";
		i.lookup["1"] = "Landscape";
		i.lookup["2"] = "Portrait";
		i.lookup["3"] = "Night scene";

		format.Add(i.name, i);

		i = new FormatInfo("exif:SceneType", "");

		i.lookup["1"] = "Directly photographed image";
		
		format.Add(i.name, i);

		i = new FormatInfo("exif:SensingMethod", "");

		i.lookup["1"] = "Not defined";
		i.lookup["2"] = "One-chip colour area sensor";
		i.lookup["3"] = "Two-chip colour area sensor";
		i.lookup["4"] = "Three-chip colour area sensor";
		i.lookup["5"] = "Colour sequential area sensor";
		i.lookup["7"] = "Trilinear sensor";
		i.lookup["8"] = "Colour sequential linear sensor";

		format.Add(i.name, i);

		i = new FormatInfo("exif:Sharpness", "");

		i.lookup["0"] = "Normal";
		i.lookup["1"] = "Soft";
		i.lookup["2"] = "Hard";

		format.Add(i.name, i);

		i = new FormatInfo("exif:ShutterSpeedValue", "");

		format.Add(i.name, i);

		i = new FormatInfo("exif:SubjectDistance", "meters");

		format.Add(i.name, i);

		i = new FormatInfo("exif:SubjectDistanceRange", "");

		i.lookup["0"] = "Unknown";
		i.lookup["1"] = "Macro";
		i.lookup["2"] = "Close view";
		i.lookup["3"] = "Distant view";

		format.Add(i.name, i);

		i = new FormatInfo("exif:WhiteBalance", "");

		i.lookup["0"] = "Auto white balance";
		i.lookup["1"] = "Manual white balance";

		format.Add(i.name, i);

		i = new FormatInfo("exif:Flash/exif:Mode", "");

		i.lookup["0"] = "unknown";
		i.lookup["1"] = "compulsory flash firing";
		i.lookup["2"] = "compulsory flash suppression";
		i.lookup["3"] = "auto mode";

		format.Add(i.name, i);

		i = new FormatInfo("exif:Flash/exif:Return", "");

		i.lookup["0"] = "no strobe return detection";
		i.lookup["2"] = "strobe return light not detected";
		i.lookup["3"] = "strobe return light detected";
		
		format.Add(i.name, i);

        i = new FormatInfo("exif:Flash/exif:Fired", "");

        i.lookup["False"] = "Did not fire";
        i.lookup["True"] = "Fired";

        format.Add(i.name, i);
	}

    private static XMPLib.MetaDataProperty findProp(String path, List<XMPLib.MetaDataProperty> props)
    {

       XMPLib.MetaDataProperty result = props.Find(p => p.path == path);
       return (result);
    }

    private static Tuple<String, String> addPropIfExists(String path, List<XMPLib.MetaDataProperty> props, List<Tuple<String, String>> propList)
    {
        XMPLib.MetaDataProperty result = props.Find(p => p.path == path);
        Tuple<String, String> prop = null;

        if (result != null)
        {
            prop = new Tuple<string, string>(formatPropertyName(result.path), formatPropertyValue(result.path, result.value));

            propList.Add(prop);
        }

        return (prop);

    }

    private static Tuple<String, String> addPropIfExists(String path, List<XMPLib.MetaDataProperty> props, List<Tuple<String, String>> propList,
        string overrideName)
    {
        XMPLib.MetaDataProperty result = props.Find(p => p.path == path);
        Tuple<String, String> prop = null;

        if (result != null)
        {
            prop = new Tuple<string, string>(overrideName, formatPropertyValue(result.path, result.value));

            propList.Add(prop);
        }

        return (prop);
    }


    public static List<Tuple<String, String>> formatProperties(List<XMPLib.MetaDataProperty> props)
    {
        // http://ptgmedia.pearsoncmg.com/images/art_evening_lrmetadata/elementLinks/fig04.jpg

        List<Tuple<String, String>> propList = new List<Tuple<string, string>>();

        XMPLib.MetaDataProperty length = findProp("tiff:ImageLength", props);
        XMPLib.MetaDataProperty width = findProp("tiff:ImageWidth", props);

        if (length != null && width != null)
        {
            propList.Add(new Tuple<string,string>("Size", width.value + " x " + length.value));
        }
             
        addPropIfExists("exif:ExposureTime", props, propList);
        addPropIfExists("exif:FNumber", props, propList, "ƒ");
        addPropIfExists("exif:ExposureBiasValue", props, propList, "Exposure Bias");
        Tuple<String,String> flash = addPropIfExists("exif:Flash/exif:Fired", props, propList, "Flash");

        if (flash != null && flash.Item2.Equals("Fired"))
        {
            addPropIfExists("exif:Flash/exif:Mode", props, propList, "Flash Mode");
            addPropIfExists("exif:Flash/exif:Return", props, propList, "Flash Return");
        }

        addPropIfExists("exif:ExposureProgram", props, propList);
        addPropIfExists("exif:MeteringMode", props, propList);

        XMPLib.MetaDataProperty isoSpeedRatings = findProp("exif:ISOSpeedRatings[1]", props);

        if (isoSpeedRatings != null)
        {
            propList.Add(new Tuple<string, string>("ISO Speed Rating", "ISO " + isoSpeedRatings.value));
        }

        addPropIfExists("exif:FocalLength", props, propList);
        addPropIfExists("aux:Lens", props, propList);
        addPropIfExists("exif:WhiteBalance", props, propList);

        addPropIfExists("exif:SensingMethod", props, propList);
        addPropIfExists("exif:Sharpness", props, propList);
        addPropIfExists("exif:Saturation", props, propList);
        addPropIfExists("exif:Contrast", props, propList);
        addPropIfExists("exif:LightSource", props, propList);
        addPropIfExists("exif:SceneCaptureType", props, propList);
        addPropIfExists("exif:SubjectDistance", props, propList);
        addPropIfExists("exif:SubjectDistanceRange", props, propList);

        addPropIfExists("tiff:Make", props, propList);
        addPropIfExists("tiff:Model", props, propList);
        addPropIfExists("aux:SerialNumber", props, propList);
        addPropIfExists("xmp:CreatorTool", props, propList,"Software");

        if (propList.Count > 0)
        {
            propList.Insert(0, new Tuple<string,string>("","EXIF"));
        }

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

		int pos = value.IndexOf("/");

		if(pos != -1) {

			try {

				String nom = value.Substring(0, pos);
				String den = value.Substring(pos + 1);

				double val = Convert.ToDouble(nom) / Convert.ToDouble(den);

				value = String.Format("{0:0.###}", val);

			} catch (Exception e) {

                log.Warn("Error converting property value to number: " + value, e);
			}
		}

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
