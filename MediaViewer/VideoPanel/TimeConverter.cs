using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaViewer.VideoPanel
{
    [ValueConversion(typeof(double), typeof(string))]
    public class TimeConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double temp = (double)value;
            int s = (int)Math.Floor(temp);
                 
            TimeSpan timeSpan = new TimeSpan(0, 0, 0, s);

            String timeString = "";

            if (timeSpan.Hours > 0)
            {
                timeString = timeSpan.Hours.ToString() + ":";
            }

            timeString += timeSpan.Minutes.ToString("D2") + ":" + timeSpan.Seconds.ToString("D2");

            return (timeString);

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double result = 0;
        
            return (result);
        }

        #endregion
    }
}
