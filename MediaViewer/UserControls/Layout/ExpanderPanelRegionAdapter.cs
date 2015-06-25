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
    public class ExpanderPanelRegionAdapter : RegionAdapterBase<ExpanderPanel>
    {

        [ImportingConstructor]
        public ExpanderPanelRegionAdapter(IRegionBehaviorFactory factory)
            : base(factory)
        {

        }

        protected override IRegion CreateRegion()
        {
            return new AllActiveRegion();
        }

        protected override void Adapt(IRegion region, ExpanderPanel regionTarget)
        {
            region.Views.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (FrameworkElement element in e.NewItems)
                    {                
                        Expander expander = new Expander();

                        FrameworkElement content = element;

                        if (element is IExpanderPanelAware)
                        {
                            IExpanderPanelAware options = element as IExpanderPanelAware;

                            expander.Header = options.Header;
                            expander.IsExpanded = options.IsIntiallyExpanded;

                            ExpanderPanel.SetElemHeight(expander, options.ElementHeight);

                            if (options.IsAddBorder)
                            {
                                ClassicBorderDecorator border = new ClassicBorderDecorator();
                                border.BorderStyle = ClassicBorderStyle.Etched;
                                border.BorderThickness = new Thickness(2);
                                border.Margin = new Thickness(5);

                                border.Child = element;

                                content = border;
                            }
                        }

                        expander.Content = content;

                        regionTarget.Children.Add(expander);
                    }
                }

                //implement remove
            };
        }
    }
}
