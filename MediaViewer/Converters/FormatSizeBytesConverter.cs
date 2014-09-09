using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaViewer.Converters
{
    [ValueConversion(typeof(long), typeof(string))]  
    class FormatSizeBytesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            long sizeBytes = (long)value;
            if (sizeBytes == 0)
            {
                return ("");
            }
            else
            {
                return (MediaViewer.Model.Utils.MiscUtils.formatSizeBytes(sizeBytes));
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();            
        }
    }
}
