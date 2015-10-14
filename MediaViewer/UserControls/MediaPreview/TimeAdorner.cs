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

namespace MediaViewer.UserControls.MediaPreview
{
    class TimeAdorner : Adorner
    {
        public int TimeTextSize { get; set; }
        public int TimeTextMargin { get; set; }
        public int TimeSeconds { get; set; }
        public Point Location { get; set; }
        
        public Size Size
        {
            get
            {
                FormattedText timeText = createFormattedText(MiscUtils.formatTimeSeconds(TimeSeconds), "Consolas", TimeTextSize, Colors.Black, FontWeights.Normal);
                
                Size size = new Size(timeText.Width + TimeTextMargin * 2, timeText.Height + TimeTextMargin * 2);

                return (size);
            }

        }

        public TimeAdorner(UIElement adornedElement) :
            base(adornedElement)
        {
            TimeSeconds = 0;
            Location = new Point(0, 0);
            TimeTextMargin = 1;
            TimeTextSize = 8;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            FormattedText timeText = createFormattedText(MiscUtils.formatTimeSeconds(TimeSeconds), "Consolas", TimeTextSize, Colors.White, FontWeights.Normal);

            Brush brush = new SolidColorBrush(Colors.Black);
            brush.Opacity = 0.6;
            Pen pen = new Pen();
            Size size = new Size(timeText.Width + TimeTextMargin * 2, timeText.Height + TimeTextMargin * 2);
            Point drawLocation = new Point(Location.X, Location.Y);
            Rect rectangle = new Rect(drawLocation, size);

            drawingContext.DrawRectangle(brush, pen, rectangle);

            Point textLocation = new Point(drawLocation.X + TimeTextMargin, drawLocation.Y + TimeTextMargin);
            drawingContext.DrawText(timeText, textLocation);
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
