using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaViewer.UserControls.MediaGridItem
{
    [ValueConversion(typeof(string), typeof(string))]
    public class PathToFilenameConverter : IValueConverter
    {
        bool isValidPath(String path)
        {
            if (path.Any(c => Path.GetInvalidPathChars().Contains(c)))
            {
                return (false);
            }

            return (Path.IsPathRooted(path));
        }



        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            String path = (string)value;
            char[] dot = new char[] {'.'};
       
            if (!String.IsNullOrEmpty(path))
            {                
                if (parameter is string && parameter.Equals("ext"))
                {
                    if (isValidPath(path))
                    {
                        return (Path.GetExtension(path).TrimStart(dot).ToUpper());
                    }
                    else
                    {
                        return (null);
                    }
                     
                }
                else if (!isValidPath(path))
                {
                    return (path);
                }
                else
                {
                    return (Path.GetFileNameWithoutExtension(path));
                }
            }
            else
            {
                return ("");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
