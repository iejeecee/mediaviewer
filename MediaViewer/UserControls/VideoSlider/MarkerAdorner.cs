using MediaViewer.Model.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace MediaViewer.UserControls.VideoSlider
{
    class MarkerAdorner : Adorner
    {     
        public Point Location { get; set; }

        public MarkerAdorner(UIElement adornedElement) :
            base(adornedElement)
        {        
            Location = new Point(0, 0);        
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
                                      
            var start = new Point(Location.X, Location.Y + 6);

            var segments = new[]
            { 
                new LineSegment(new Point(start.X - 3 , start.Y + 5), true), 
                new LineSegment(new Point(start.X + 3 , start.Y + 5), true)
            };

            PathFigure figure = new PathFigure(start, segments, true);
            PathGeometry geo = new PathGeometry(new[] { figure });

            drawingContext.DrawGeometry(Brushes.Black, null, geo);
          
        }

        private FormattedText createFormattedText(String text, String font, double size, Color color, FontWeight weight)
        {
            FontFamily family = new FontFamily(font);
            Typeface typeface = new Typeface(family, FontStyles.Normal, weight, new FontStretch());

            SolidColorBrush headerForegroundBrush = new SolidColorBrush(color);

            FormattedText formattedText = new FormattedText(text,
                 new CultureInfo("en-us"),
                 FlowDirection.LeftToRight,
                 typeface,
                 size,
                 headerForegroundBrush, null, TextFormattingMode.Display);

            return (formattedText);
        }
    }
}
