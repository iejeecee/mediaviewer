using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MediaViewer.MediaGrid
{
    class RatingCache
    {
        public List<BitmapSource> RatingBitmap { get; set; }
      
        public RatingCache()
        {
            RatingBitmap = new List<BitmapSource>();

            for (int i = 0; i < 6; i++)
            {                 
                RatingBitmap.Add(new BitmapImage(new Uri("pack://application:,,,/Resources/Images/" + i + "stars.png")));                    
            }

            //create();

        }

        void create()
        {
            Grid grid = new Grid();
            grid.HorizontalAlignment = HorizontalAlignment.Center;
            grid.VerticalAlignment = VerticalAlignment.Center;
            grid.Background = null;

            Rating ratingControl = new Rating();

            ratingControl.HorizontalAlignment = HorizontalAlignment.Left;
            ratingControl.VerticalAlignment = VerticalAlignment.Top;
            ratingControl.ItemCount = 5;
            ratingControl.Foreground = new SolidColorBrush(Colors.Red);
            ratingControl.Background = null;
            ratingControl.IsReadOnly = true;
            ratingControl.SelectionMode = RatingSelectionMode.Continuous;

            double scale = 0.6;
            ratingControl.LayoutTransform = new ScaleTransform(scale, scale);

            grid.Children.Add(ratingControl);

            int i = 0;

            double nrStars = 0;
            do
            {
                ratingControl.Value = nrStars;

                grid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                grid.Arrange(new Rect(new Size(double.MaxValue, double.MaxValue)));
                Rect size = VisualTreeHelper.GetDescendantBounds(ratingControl);
                //grid.Arrange(new Rect(new Size(size.Width, size.Height)));

                RenderTargetBitmap bitmap = new RenderTargetBitmap((int)(size.Width * scale), (int)(size.Height * scale),
                    96, 96, PixelFormats.Default);

                Window dummy = new Window();
                dummy.Content = grid;
                dummy.SizeToContent = SizeToContent.WidthAndHeight;
                dummy.Show();

                bitmap.Render(ratingControl);

                RatingBitmap.Add(bitmap);

                nrStars += 1.0 / 5;
               
                BitmapEncoder encoder = new PngBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(bitmap, null, null, null));

                FileStream outputFile = new FileStream("d:\\" + i + "stars.png", FileMode.Create);

                encoder.Save(outputFile);
                
                i++;

            } while (nrStars <= 1);
        }
    }
}
