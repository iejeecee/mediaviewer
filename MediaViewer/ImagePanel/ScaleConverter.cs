using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MediaViewer.ImagePanel
{
    [ValueConversion(typeof(double), typeof(string))]
    class ScaleConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (string.Format("{0:N4}", value));

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double result = 0;
            try
            {

                result = double.Parse((string)value);

                if (result > 4)
                {
                    return (DependencyProperty.UnsetValue);
                }
                else if (result < 0)
                {
                    return (DependencyProperty.UnsetValue);
                }

            }
            catch (Exception)
            {
                return (DependencyProperty.UnsetValue);
            }

            return (result);


        }

        #endregion
    }
}
