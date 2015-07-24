using MediaViewer.UserControls.TabbedExpander;
// http://elegantcode.com/2012/04/18/create-a-custom-prism-regionadapter/
using Microsoft.Practices.Prism.Regions;
using Microsoft.Windows.Themes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MediaViewer.UserControls.Layout
{
    [Export]
    public class TabbedExpanderPanelRegionAdapter : RegionAdapterBase<TabbedExpanderPanel>
    {

        [ImportingConstructor]
        public TabbedExpanderPanelRegionAdapter(IRegionBehaviorFactory factory)
            : base(factory)
        {

        }

        protected override IRegion CreateRegion()
        {
            return new AllActiveRegion();
        }

        protected override void Adapt(IRegion region, TabbedExpanderPanel regionTarget)
        {
            region.Views.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (FrameworkElement element in e.NewItems)
                    {                                        
                        if (!(element is TabbedExpanderView))
                        {
                            throw new InvalidOperationException("Element inserted is not a TabbedExpanderView");
                        }
                       
                        regionTarget.Children.Add(element);
                    }
                }

                //implement remove
            };
        }
    }
}
