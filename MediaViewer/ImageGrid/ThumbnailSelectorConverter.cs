using MediaViewer.MediaDatabase;
using MediaViewer.MediaFileModel.Watcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaViewer.ImageGrid
{
    class ThumbnailSelectorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Media media = (Media)value;

            if (media == null || media.Thumbnail == null) 
            {
                return (parameter);
            }
            else
            {
                return (media.Thumbnail);
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
