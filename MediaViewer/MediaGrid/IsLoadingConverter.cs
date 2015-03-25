using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.File;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaViewer.MediaGrid
{
    class IsLoadingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            MediaItemState state = (MediaItemState)value;

             bool isLoading = false;

             if (state == MediaItemState.LOADING ||
                 state == MediaItemState.EMPTY ||
                 state == MediaItemState.TIMED_OUT)
             {
              
                 isLoading = true;
             }
            

            return (isLoading);  

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
