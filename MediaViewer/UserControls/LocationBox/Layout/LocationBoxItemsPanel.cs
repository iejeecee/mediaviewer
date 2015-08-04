using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MediaViewer.UserControls.LocationBox.Layout
{
    class LocationBoxItemsPanel : Panel
    {

        public VerticalAlignment VerticalContentAlignment
        {
            get { return (VerticalAlignment)GetValue(VerticalContentAlignmentProperty); }
            set { SetValue(VerticalContentAlignmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VerticalContentAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VerticalContentAlignmentProperty =
            DependencyProperty.Register("VerticalContentAlignment", typeof(VerticalAlignment), typeof(LocationBoxItemsPanel), new PropertyMetadata(VerticalAlignment.Stretch));

        
        protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
        {
            Size idealSize = new Size(0, 0);

            foreach (UIElement child in Children)
            {
                child.Measure(new Size(availableSize.Width, availableSize.Height));

                idealSize.Height = Math.Max(idealSize.Height, child.DesiredSize.Height);
                idealSize.Width += child.DesiredSize.Width;
            }

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

            double availableWidth = finalSize.Width;
           
            int visibleChild = Children.Count - 1;

            do
            {
                availableWidth -= Children[visibleChild].DesiredSize.Width;

                visibleChild--;
               
            } while (visibleChild >= 0 && Children[visibleChild].DesiredSize.Width <= availableWidth);


            double xPos = 0;

            for (int i = 0; i < Children.Count; i++)
            {
                UIElement child = Children[i];

                if (i >= visibleChild + 1)
                {
                    child.Arrange(new Rect(xPos, getHeightOffset(child,finalSize.Height), child.DesiredSize.Width, child.DesiredSize.Height));
                    xPos += child.DesiredSize.Width;
                }
                else
                {
                    child.Arrange(new Rect(0, 0, 0, 0));
                }
                
            }

            return finalSize;
        }

        double getHeightOffset(UIElement child, double height)
        {
            double offset = 0;

            switch (VerticalContentAlignment)
            {
                case VerticalAlignment.Bottom:
                    offset = height - child.DesiredSize.Height;
                    break;
                case VerticalAlignment.Center:
                    offset = (height - child.DesiredSize.Height) / 2;
                    break;
                case VerticalAlignment.Stretch:
                    offset = 0;
                    break;
                case VerticalAlignment.Top:
                    offset = 0;
                    break;
                default:
                    break;
            }

            return (offset);
        }
    }
}
