using MediaViewer.Model.Media.State.CollectionView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using YoutubePlugin.Item;

namespace YoutubePlugin
{
    class IsYoutubeItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return (System.Windows.Visibility.Collapsed);
            }

            YoutubeItem item = ((SelectableMediaItem)value).Item as YoutubeItem;
            String matchString = (String)parameter;

            String[] types = matchString.Split(new char[] { '|' });
           
            if (types.Contains(item.GetType().Name))
            {
                return (System.Windows.Visibility.Visible);
            }
            else
            {
                return (System.Windows.Visibility.Collapsed);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
