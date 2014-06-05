using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaViewer.UserControls.DirectoryPicker
{
    class NrImportedFilesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int nrImportedFiles = (int)value;

            if (nrImportedFiles == 0)
            {
                return ("");
            }
            else
            {
                return (nrImportedFiles.ToString());
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

