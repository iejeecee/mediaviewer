using MediaViewer.Model.Utils.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MediaViewer.UserControls.TabbedExpander
{
    class TabbedExpanderHeaderDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            
            TabbedExpanderView tabbedExpander = VisualTreeUtils.FindVisualParent<TabbedExpanderView>(element);

            if (tabbedExpander.Items.Count == 1)
            {
                return Application.Current.Resources["ExpanderHeaderItemStyle"] as DataTemplate;
            }
            else
            {
                return Application.Current.Resources["ExpanderHeaderItemsStyle"] as DataTemplate;
            }

            //return null;
        }
    }
}
