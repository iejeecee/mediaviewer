using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaViewer.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    class PathToFileNameConverter : IValueConverter
    {
     
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            String path = (string)value;
       
            if (!String.IsNullOrEmpty(path))
            {
                return (Path.GetFileName(path));
            }
            else
            {
                return ("");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
