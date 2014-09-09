using MediaViewer.Model.Media.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaViewer.ImageGrid
{
    class IsLoadingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            MediaFileItemState state = (MediaFileItemState)value;

            bool isLoading = state == MediaFileItemState.LOADING || state == MediaFileItemState.TIMED_OUT;

            return (isLoading);  

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
