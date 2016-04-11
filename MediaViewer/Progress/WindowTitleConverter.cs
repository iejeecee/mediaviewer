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
        
            string info = "";

            if (values.Count() > 4)
            {
                int itemProgress = (int)values[3];
                int itemProgressMax = (int)values[4];
            
                info = "Finished: " + totalProgress.ToString();

                if (totalProgressMax != 0)
                {
                    info += "/" + totalProgressMax.ToString();
                }

                if (itemProgressMax != 0)
                {
                    info += itemProgressMax != 0 ? ", Progress: " + ((int)((itemProgress / (float)itemProgressMax) * 100)).ToString() + "%" : "";
                }
             
            }
            else
            {
                if (totalProgressMax != 0)
                {
                    info = "Finished: " + (int)((totalProgress / (double)totalProgressMax) * 100) + "%";
                }                
            }

            return (title + " - " + info);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
