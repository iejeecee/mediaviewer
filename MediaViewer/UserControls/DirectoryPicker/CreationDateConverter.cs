using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaViewer.UserControls.DirectoryPicker
{
    class CreationDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Nullable<DateTime> creationDate = (Nullable<DateTime>)value;

            if (creationDate == null)
            {
                return ("");
            }
            else
            {
                return (creationDate.Value.ToString("dd MMM yyyy HH:mm:ss"));
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

