using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaViewer.Progress
{
    class TotalProgressTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int totalProgress = (int)values[0];
            int totalProgressMax = (int)values[1];

            String info = "Finished " + totalProgress;

            if (totalProgressMax != 0)
            {
                info += " / " + totalProgressMax;
            }

            return (info);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
