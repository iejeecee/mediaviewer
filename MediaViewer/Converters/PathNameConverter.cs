using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaViewer.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    class PathNameConverter : IMultiValueConverter
    {
      

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            String volumeLabel = (string)values[0];
            String pathName = (string)values[1];

            if (!String.IsNullOrEmpty(volumeLabel))
            {
                if (pathName.EndsWith("\\"))
                {
                    pathName = pathName.Remove(pathName.Length - 1);
                }

                return (volumeLabel + " (" + pathName + ")");
            }
            else
            {
                return (pathName);
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
