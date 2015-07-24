using MediaViewer.UserControls.Layout;
using Microsoft.Practices.Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MediaViewer.UserControls.TabbedExpander
{
    [Export]
    public class TabbedExpanderRegionAdapter : RegionAdapterBase<TabbedExpanderView>
    {
        [ImportingConstructor]
        public TabbedExpanderRegionAdapter(IRegionBehaviorFactory factory)
            : base(factory)
        {

        }

        protected override void Adapt(IRegion region, TabbedExpanderView regionTarget)
        {
            region.Views.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {                   
                    foreach (FrameworkElement element in e.NewItems)
                    {
                        TabbedExpanderTab tab;

                        if (!(element is TabbedExpanderTab))
                        {
                            // insert element into a tab and add it to the tabbedExpander
                            tab = new TabbedExpanderTab();
                            tab.Content = element;

                            if (element is ITabbedExpanderAware)
                            {
                                ITabbedExpanderAware tabInfo = element as ITabbedExpanderAware;

                                tab.TabName = tabInfo.TabName;
                                tab.IsSelected = tabInfo.TabIsSelected;
                                tab.BorderBrush = tabInfo.TabBorderBrush;
                                tab.BorderThickness = tabInfo.TabBorderThickness;
                                tab.Margin = tabInfo.TabMargin;
                            }
                            else
                            {
                                tab.TabName = element.Name;

                                if (regionTarget.Items.Count == 0)
                                {
                                    tab.IsSelected = true;
                                }
                            }
                        }
                        else
                        {
                            tab = element as TabbedExpanderTab;
                        }

                        regionTarget.Items.Add(tab);
                    }                    
                }
            };
        }

        protected override IRegion CreateRegion()
        {
            return new AllActiveRegion();
        }
    }
}
