using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MediaViewer.UserControls.MediaGridItem
{
    class InfoIconsCache
    {
                
        Dictionary<String, BitmapSource> ImageHash { get; set; }

        public InfoIconsCache() {

            List<BitmapImage> icons = new List<BitmapImage>();

            String iconPath = "pack://application:,,,/Resources/Icons/";

            icons.Add(new BitmapImage(new Uri(iconPath + "checked.ico", UriKind.Absolute)));
            icons.Add(new BitmapImage(new Uri(iconPath + "notsupported.ico", UriKind.Absolute)));
            icons.Add(new BitmapImage(new Uri(iconPath + "tag.ico", UriKind.Absolute)));
            icons.Add(new BitmapImage(new Uri(iconPath + "geotag.ico", UriKind.Absolute)));

            ImageHash = new Dictionary<string, BitmapSource>();

            String iconChars = "";

            for (int i = 0; i < icons.Count; i++)
            {
                iconChars += i.ToString();
            }

            List<String> combos = generateCombinations(iconChars);

            generateImages(combos, icons, false);
            
        }

        private void generateImages(List<string> combos, List<BitmapImage> icons, bool saveToDisk)
        {
            for(int i = 1; i < combos.Count; i++)
            {
                String combo = combos[i];

                int maxIconWidth = 32;
                int maxIconHeight = 32;

                int imageWidth = combo.Length * maxIconWidth;
                int imageHeight = maxIconHeight;       

                RenderTargetBitmap bitmap = new RenderTargetBitmap(imageWidth, imageHeight, 96, 96, PixelFormats.Pbgra32);

                DrawingVisual drawingVisual = new DrawingVisual();             
                RenderOptions.SetBitmapScalingMode(drawingVisual, BitmapScalingMode.HighQuality);

                using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                {
                    for (int j = 0; j < combo.Length; j++)
                    {
                        BitmapImage icon = icons[int.Parse(combo[j].ToString())];

                        int x = j * maxIconWidth;
                        int y = 0;
                        Rect destRect = new Rect(x, y, maxIconWidth, maxIconHeight);

                        drawingContext.DrawImage(icon, destRect);                      

                    }

                }

                RenderOptions.SetBitmapScalingMode(bitmap, BitmapScalingMode.HighQuality);                  
                bitmap.Render(drawingVisual);
                bitmap.Freeze();

                if (saveToDisk == true)
                {
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();

                    encoder.Frames.Add(BitmapFrame.Create(bitmap, null, null, null));
                   
                    FileStream outputFile = new FileStream("d:\\image" + i + ".jpg", FileMode.Create);
                
                    encoder.Save(outputFile);

                }

                ImageHash.Add(combo, bitmap);
            }
        }

        List<String> generateCombinations(String iconChars)
        {
            List<String> results = new List<string>();
            results.Add("");

            foreach (char iconChar in iconChars)
            {
                List<String> newCombinations = charCombinations(results, iconChar);
                results.AddRange(newCombinations);
            }

            return (results);

        }

        List<String> charCombinations(List<String> combos, char c)
        {
            List<String> newCombinations = new List<string>();

            foreach (String combo in combos)
            {
                newCombinations.Add(combo + c);
            }

            return (newCombinations);
        }

        public BitmapSource getInfoIconsBitmap(MediaItem item)
        {
            BitmapSource bitmap = null;

            String key = "";

            if (item.ItemState != MediaItemState.LOADED)
            {
                return (bitmap);
            }

            if (item.Metadata != null)
            {
                if (item.Metadata.IsImported)
                {
                    key += '0';
                }

                if (!item.Metadata.SupportsXMPMetadata)
                {
                    key += '1';
                }
            }

            if (item.HasTags)
            {
                key += '2';
            }

            if (item.HasGeoTag)
            {
                key += '3';
            }

            if (String.IsNullOrEmpty(key))
            {
                return (null);
            }
            else
            {
                return(ImageHash[key]);
            }

        }

    }
}
