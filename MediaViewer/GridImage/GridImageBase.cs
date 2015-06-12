using MediaViewer.MediaDatabase;
using MediaViewer.Model.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using VideoLib;

namespace MediaViewer.GridImage
{
    abstract class GridImageBase
    {          
        protected const int margin = 3;
        protected const int borderThickness = 2;

        protected GridImageBase(int width, int nrRows, int nrColumns, List<BitmapSource> images, 
            Color backgroundColor, Color fontColor, Stretch stretch = Stretch.UniformToFill)
        {           
            NrRows = nrRows;
            NrColumns = nrColumns;
                                              
            Images = images;
            Stretch = stretch;
            BackgroundColor = backgroundColor;
            FontColor = fontColor;
        }

        protected Color BackgroundColor { get; set; }
        protected Color FontColor { get; set; }

        Stretch Stretch { get; set; }
        List<BitmapSource> Images { get; set; }
        public int NrRows { get; protected set; }
        public int NrColumns { get; protected set; }
                                    
        protected abstract void createHeader(Grid mainGrid,String fontFamily, int margin);
        protected abstract void addImageInfo(int imageNr, Grid cell, String fontFamily, int margin);      

        public RenderTargetBitmap createGridImage(int width, int? maxGridHeight = null, String fontFamily = "Consolas", double fontSize = 20)
        {
            Grid mainGrid = new Grid();            
            mainGrid.HorizontalAlignment = HorizontalAlignment.Center;
            mainGrid.VerticalAlignment = VerticalAlignment.Center;
            mainGrid.Background = new SolidColorBrush(BackgroundColor);
            mainGrid.Width = width;        
            RowDefinition headerRow = new RowDefinition() { Height = GridLength.Auto };
            RowDefinition imageRow = new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) };

            mainGrid.RowDefinitions.Add(headerRow);
            mainGrid.RowDefinitions.Add(imageRow);

            createHeader(mainGrid, fontFamily, margin);              
                  
            Grid imageGrid = new Grid();
            imageGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            imageGrid.VerticalAlignment = VerticalAlignment.Stretch;
            Grid.SetRow(imageGrid, 1);
            mainGrid.Children.Add(imageGrid);

            for (int x = 0; x < NrColumns; x++)
            {
                ColumnDefinition column = new ColumnDefinition() { Width = new GridLength(1,GridUnitType.Star) };
                imageGrid.ColumnDefinitions.Add(column);
            }

            int imageNr = 0;
           
            for (int y = 0; y < NrRows; y++)
            {
                RowDefinition cellRow = new RowDefinition() { Height = GridLength.Auto };
              
                imageGrid.RowDefinitions.Add(cellRow);

                for (int x = 0; x < NrColumns && imageNr < Images.Count; x++)
                {
                    Grid cell = new Grid();
                   
                    Grid.SetRow(cell, y);
                    Grid.SetColumn(cell, x);

                    cell.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                    cell.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
                    cell.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

                    Border border = new Border() { BorderThickness = new Thickness(borderThickness), BorderBrush = new SolidColorBrush(Colors.Black) };
                    border.Effect = new DropShadowEffect() { ShadowDepth = 4, Direction = 330, Color = Colors.Black, Opacity = 0.5, BlurRadius = 4 };
                    border.Margin = new Thickness(margin);
                    border.HorizontalAlignment = HorizontalAlignment.Center;
                    border.VerticalAlignment = VerticalAlignment.Center;

                    Grid.SetRow(border, 1);
                   
                    Image image = new Image();
                    image.Stretch = Stretch;
                    image.MaxHeight = maxGridHeight.HasValue ? maxGridHeight.Value : double.PositiveInfinity;
                    image.Source = Images[imageNr];

                    border.Child = image;
                    cell.Children.Add(border);
                    imageGrid.Children.Add(cell);

                    addImageInfo(imageNr, cell, fontFamily, margin);

                    imageNr++;
                    
                }
            }            

            mainGrid.Measure(new Size(width, double.PositiveInfinity));
            mainGrid.Arrange(new Rect(new Size(width, double.MaxValue)));
            Rect size = VisualTreeHelper.GetDescendantBounds(mainGrid);
            mainGrid.Arrange(new Rect(new Size(size.Width, size.Height)));

            RenderTargetBitmap output = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Default);   
        
            RenderOptions.SetBitmapScalingMode(output, BitmapScalingMode.HighQuality);
            TextOptions.SetTextFormattingMode(output, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(output, TextRenderingMode.ClearType);

            output.Render(mainGrid);
            output.Freeze();
                              
            return (output);
                  
        }

       
               
    }
}
