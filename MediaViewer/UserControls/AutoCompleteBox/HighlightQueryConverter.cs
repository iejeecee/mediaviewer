using MediaViewer.MediaDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace MediaViewer.UserControls.AutoCompleteBox
{
    class HighlightQueryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Tag tag = (Tag)value;
            String text = tag.Name;
            String query = ((AutoCompleteBoxView)parameter).Text;

            int pos = text.IndexOf(query,StringComparison.CurrentCultureIgnoreCase);

            Run head = new Run(text.Substring(0, pos));
            Run highlight = new Run(query);
            highlight.FontWeight = FontWeights.SemiBold;
            Run tail = new Run(text.Substring(pos + query.Length));

            TextBlock textBlock = new TextBlock();
            textBlock.Inlines.Add(head);
            textBlock.Inlines.Add(highlight);
            textBlock.Inlines.Add(tail);

            return (textBlock);

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
