using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

// Converts null to parameter, or parameter back to null
namespace MediaViewer.Converters
{
    class NullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return (parameter);
            }
            else
            {
                return (value);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value.Equals(parameter))
            {
                return (null);
            }
            else
            {
                return (value);
            }
        }
    }
}

