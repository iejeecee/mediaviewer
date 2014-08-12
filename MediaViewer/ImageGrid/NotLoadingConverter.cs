﻿using MediaViewer.MediaDatabase;
using MediaViewer.MediaFileModel.Watcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaViewer.ImageGrid
{
    class NotLoadingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            MediaFileItemState state = (MediaFileItemState)value;

            if (state != MediaFileItemState.LOADING) return (true);
            else return (false);

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}