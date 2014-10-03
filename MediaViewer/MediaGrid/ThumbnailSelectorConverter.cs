using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.File.Watcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaViewer.MediaGrid
{
    class ThumbnailSelectorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BaseMedia media = (BaseMedia)value;

            if (media == null || media.Thumbnail == null) 
            {
                return (parameter);
            }
            else
            {               
                return (media.Thumbnail.Image);
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
