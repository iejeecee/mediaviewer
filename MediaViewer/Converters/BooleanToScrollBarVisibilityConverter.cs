using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MediaViewer.Converters
{
    class BooleanToScrollBarVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = (bool)value;

            if (parameter is String && parameter.Equals("invert"))
            {
                return isVisible == false ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto;
            }
            else
            {
                return isVisible == true ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
