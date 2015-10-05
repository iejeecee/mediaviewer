using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace YoutubePlugin.YoutubeChannelBrowser
{
    class NrVideosConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ulong? nrVideos = (ulong?)value;

            if (nrVideos == 0)
            {
                return ("");
            }
            else
            {
                return (nrVideos.ToString());
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

