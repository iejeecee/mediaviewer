using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MediaViewer.ImagePanel
{
   
    public class RotationEnumToStringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ImageViewModel.RotationMode selectedRotationMode = (ImageViewModel.RotationMode)value;

            switch (selectedRotationMode)
            {
                case ImageViewModel.RotationMode.NONE:
                    {
                        return ("None");                     
                    }
                case ImageViewModel.RotationMode.CW_90:
                    {
                        return ("90°");
                    }
                case ImageViewModel.RotationMode.CW_180:
                    {
                        return ("180°");
                    }
                case ImageViewModel.RotationMode.CCW_90:
                    {
                        return ("-90°");
                    }
                case ImageViewModel.RotationMode.CUSTOM:
                    {
                        return ("Custom");
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

