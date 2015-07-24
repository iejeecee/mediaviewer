//http://stackoverflow.com/questions/661831/wpf-itemtemplate-not-acting-as-expected
//http://stackoverflow.com/questions/3843200/why-does-itemscontrol-not-use-my-itemtemplate
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MediaViewer.UserControls.TabbedExpander
{
    public class TemplatableItemsControl : ItemsControl
    {
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return false;

            
        }

        /*protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            ((ContentPresenter)element).ContentTemplate = ItemTemplate;
        }*/
    }
}
