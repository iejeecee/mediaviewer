using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Media.File.Watcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MediaViewer.UserControls.MediaGridItem
{
    class ThumbnailSelectorConverter : IValueConverter
    {
        static BitmapImage audioIcon;

        static ThumbnailSelectorConverter()
        {
            audioIcon = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/audio.ico", UriKind.Absolute));
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BaseMetadata media = (BaseMetadata)value;
                                                   
            if (media == null || media.Thumbnail == null)               
            {
                if (media is AudioMetadata) return audioIcon;
                else return parameter;
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
