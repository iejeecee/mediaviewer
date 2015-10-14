using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.File.Watcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaViewer.UserControls.MediaGridItem
{
    class ThumbnailSelectorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BaseMetadata media = (BaseMetadata)value;

            if (media == null || media.Thumbnails.Count == 0) 
            {
                return (parameter);
            }
            else
            {               
                return (media.Thumbnails.ElementAt(0).Image);
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
