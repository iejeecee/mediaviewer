using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MediaViewer.Model.Utils.Windows
{
    class ColorUtils
    {
        public static void printSystemColors()
        {

            Type scType = typeof(System.Windows.SystemColors);
            foreach (PropertyInfo pinfo in scType.GetProperties())
            {
                if (pinfo.Name.EndsWith("Color"))
                {
                    String name = pinfo.Name.Remove(pinfo.Name.Length - 5) + "BrushKey";
                    Color color = (Color)pinfo.GetValue(null, null);
                  
                    byte[] colorArray = {color.A, color.R, color.G, color.B};
                    String hexColor = BitConverter.ToString(colorArray).Replace("-", string.Empty);

                    String key = "<SolidColorBrush x:Key=\"{x:Static SystemColors." + name + "}\"";
                    String value = "Color=\"#" + hexColor + "\"/>";

                    System.Diagnostics.Debug.Print(key + " " + value);
                }
            }
        }
    }
}
