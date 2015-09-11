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
            if (this.Children == null || this.Children.Count == 0)
                return availableSize;
                        
            Children[Children.Count - 1].Measure(new Size(double.PositiveInfinity, availableSize.Height));

            if (Children.Count > 1)
            {

                Children[Children.Count - 2].Measure(new Size(double.PositiveInfinity, availableSize.Height));

                if (Children[Children.Count - 1].DesiredSize.Width + Children[Children.Count - 2].DesiredSize.Width > availableSize.Width)
                {
                    Children[Children.Count - 1].Measure(new Size(availableSize.Width / 2, availableSize.Height));
                    Children[Children.Count - 2].Measure(new Size(availableSize.Width / 2, availableSize.Height));
                }

                double availableWidth = availableSize.Width - Children[Children.Count - 1].DesiredSize.Width - Children[Children.Count - 2].DesiredSize.Width;

                for (int i = Children.Count - 3; i >= 0; i--)
                {
                    UIElement child = Children[i];

                    child.Measure(new Size(double.PositiveInfinity, availableSize.Height));

                    if (child.DesiredSize.Width > availableWidth)
                    {
                        child.Measure(new Size(0, 0));
                        break;
                    }

                    availableWidth -= child.DesiredSize.Width;

                }
            }

            return availableSize;
           
        }

        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
        {
            if (this.Children == null || this.Children.Count == 0)
                return finalSize;
           
            double xPos = 0;

            for (int i = 0; i < Children.Count; i++)
            {
                UIElement child = Children[i];
                
                child.Arrange(new Rect(xPos, getHeightOffset(child, finalSize.Height), child.DesiredSize.Width, child.DesiredSize.Height));
                xPos += child.DesiredSize.Width;               
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
