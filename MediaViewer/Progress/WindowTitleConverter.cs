using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaViewer.Progress
{
    class WindowTitleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            String title = (String)values[0];
            int totalProgress = (int)values[1];
            int totalProgressMax = (int)values[2];
            int itemProgress = (int)values[3];
            int itemProgressMax = (int)values[4];

            String totalProgressString = "Finished: " + totalProgress.ToString() + "/" + totalProgressMax.ToString() + ", ";
            String itemProgressString = itemProgressMax != 0 ? "Progress: " + ((int)((itemProgress / (float)itemProgressMax) * 100)).ToString() + "%" : "";

            return (title + " - " + totalProgressString + itemProgressString);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
