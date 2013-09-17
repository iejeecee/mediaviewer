using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MediaViewer.ExtensionMethods
{
    static class DependencyObjectExtensions
    {
        public static List<T> getChildrenOfType<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            List<T> mergedResults = new List<T>();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                List<T> result = getChildrenOfType<T>(child);

                if (child as T != null)
                {
                    result.Add(child as T);
                }

                mergedResults.AddRange(result);
            }

            return mergedResults;
        }
    }

}
