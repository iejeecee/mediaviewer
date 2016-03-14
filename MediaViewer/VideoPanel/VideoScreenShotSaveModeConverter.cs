using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaViewer.VideoPanel
{
    public class VideoScreenShotSaveModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Infrastructure.Constants.SaveLocation mode = (Infrastructure.Constants.SaveLocation)value;

            switch (mode)
            {
                case MediaViewer.Infrastructure.Constants.SaveLocation.Current:
                    return (false);                
                case MediaViewer.Infrastructure.Constants.SaveLocation.Ask:
                    return (false);                  
                case MediaViewer.Infrastructure.Constants.SaveLocation.Fixed:
                    return (true);                  
                default:
                    return (false);  
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
