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

        public RatingCache(bool saveToDisk = false)
        {
            RatingBitmap = new List<BitmapSource>();

            Grid grid = new Grid();
            grid.Background = new SolidColorBrush(Colors.Red);         
            grid.HorizontalAlignment = HorizontalAlignment.Stretch;
            grid.VerticalAlignment = VerticalAlignment.Stretch;
            grid.ShowGridLines = true;

            RowDefinition rowDef1 = new RowDefinition();
            grid.RowDefinitions.Add(rowDef1);

            RatingItem[] ratingItems = new RatingItem[] {new RatingItem(),new RatingItem(), new RatingItem(), new RatingItem(), new RatingItem()};
  
            Rating ratingControl = new Rating();
                     
            foreach(RatingItem item in ratingItems) {
                ratingControl.Items.Add(item);
            }
            //ratingControl.ItemCount = 5;
            //ratingControl.Foreground = new SolidColorBrush(Colors.Red);
            ratingControl.Background = null;
           // ratingControl.IsReadOnly = true;
            ratingControl.SelectionMode = RatingSelectionMode.Continuous;
           
            ratingControl.LayoutTransform = new ScaleTransform(2, 2);

            Grid.SetRow(ratingControl, 0);
            grid.Children.Add(ratingControl);        

            int i = 0;

            double nrStars = 0;
            do
            {

                ratingControl.Value = nrStars;

                RenderTargetBitmap bitmap = new RenderTargetBitmap(300, 300, 96, 96, PixelFormats.Pbgra32);

                grid.Measure(new Size(bitmap.Width, bitmap.Height));
                grid.Arrange(new Rect(new Size(bitmap.Width, bitmap.Height)));
                
                bitmap.Render(grid);

                RatingBitmap.Add(bitmap);
           
                nrStars += 1.0/5;
                
                if (saveToDisk == true)
                {
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();

                    encoder.Frames.Add(BitmapFrame.Create(bitmap, null, null, null));

                    FileStream outputFile = new FileStream("d:\\image" + i + ".jpg", FileMode.Create);

                    encoder.Save(outputFile);
                    i++;

                }

            } while (nrStars <= 1);
            
        }
    }
}
