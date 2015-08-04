using MediaViewer.UserControls.TabbedExpander;
//http://stackoverflow.com/questions/8269995/problems-with-arrange-measure-is-layout-broken-in-wpf
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MediaViewer.UserControls.Layout
{
    public class TabbedExpanderPanel : Panel
    {
        static TabbedExpanderPanel()
         {
               //tell DP sub system, this DP, will affect
               //Arrange and Measure phases
               FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata();
               metadata.AffectsArrange = true;
               metadata.AffectsMeasure = true;

               ElemHeightProperty = DependencyProperty.RegisterAttached("ElemHeight",
                   typeof(int), typeof(TabbedExpanderPanel), metadata);
          }


        public static DependencyProperty ElemHeightProperty;

        public static void SetElemHeight(UIElement element, int value)
        {
            element.SetValue(ElemHeightProperty, value);
        }

        public static int GetElemHeight(UIElement element)
        {
            return (int)element.GetValue(ElemHeightProperty);
        }

        protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
        {
            Size idealSize = new Size(0, 0);

            double nom = 0;
            double availableHeight = availableSize.Height;

            int nrExpanded = 0;

            foreach (TabbedExpanderView child in this.Children)
            {
                if (!child.IsExpanded)
                {
                    child.Measure(new Size(availableSize.Width, Double.PositiveInfinity));
                    availableHeight -= child.DesiredSize.Height;
                }
                else
                {
                    nom += GetElemHeight(child);
                    nrExpanded++;
                }
                
            }

            foreach (TabbedExpanderView child in this.Children)
            {
                double elemHeight;
                child.SetValue(WidthProperty, availableSize.Width);

                if (child.IsExpanded)
                {
                    if (nrExpanded == 1)
                    {
                        elemHeight = availableHeight;
                    }
                    else
                    {
                        double num = GetElemHeight(child);

                        elemHeight = (num / nom) * availableHeight;
                    }

                    // in-order to make sure the child is properly sized measure should be done 
                    // only with available space
                    child.Measure(new Size(availableSize.Width, elemHeight));
                }

                idealSize.Width = Math.Max(idealSize.Width,
                                   child.DesiredSize.Width);
                idealSize.Height += child.DesiredSize.Height; 
            
            }           
            
            // EID calls us with infinity, but framework
            // doesn't like us to return infinity
            if (double.IsInfinity(availableSize.Height) ||
                double.IsInfinity(availableSize.Width))
                return idealSize;
            else
                return availableSize;
        }

        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
        {
            if (this.Children == null || this.Children.Count == 0)
                return finalSize;
            
            double nom = 0;                 
            double availableHeight = finalSize.Height;

            int nrExpanded = 0;

            foreach (TabbedExpanderView child in this.Children)
            {
                if (!child.IsExpanded)
                {
                    availableHeight -= child.DesiredSize.Height;
                }
                else
                {
                    nom += GetElemHeight(child);   
                    nrExpanded++;
                }
                               
            }

            double yPos = 0;

            foreach (TabbedExpanderView child in this.Children)
            {
                double elemHeight;
                double elemWidth = finalSize.Width;

                if (!child.IsExpanded)
                {
                    elemHeight = child.DesiredSize.Height;                   
                }
                else
                {
                    if (nrExpanded == 1)
                    {
                        elemHeight = availableHeight;                      
                    }
                    else
                    {
                        double num = GetElemHeight(child);
                   
                        elemHeight = (num / nom) * availableHeight;
                    }
                }

                child.Arrange(new Rect(0, yPos, elemWidth,
                             elemHeight));

                yPos += elemHeight;
                               
            }            

            return finalSize;
        }
    }
}
