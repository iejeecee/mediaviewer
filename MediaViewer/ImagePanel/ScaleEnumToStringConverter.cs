using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MediaViewer.ImagePanel
{

    public class ScaleEnumToStringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ImageViewModel.ScaleMode selectedScaleMode = (ImageViewModel.ScaleMode)value;

            switch (selectedScaleMode)
            {
                case ImageViewModel.ScaleMode.NONE:
                    {
                        return ("None");
                    }
                case ImageViewModel.ScaleMode.AUTO:
                    {
                        return ("Auto");
                    }          
                case ImageViewModel.ScaleMode.FIT_HEIGHT:
                    {
                        return ("Fit Height");
                    }
                case ImageViewModel.ScaleMode.FIT_WIDTH:
                    {
                        return ("Fit Width");
                    }

            }

            return ("Error");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();

        }

        #endregion
    }
}


