using MediaViewer.Model.Media.Base.Item;
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
    public abstract class InfoIconsCache
    {                
        protected Dictionary<String, BitmapSource> ImageHash { get; private set; }

        public InfoIconsCache(List<BitmapImage> icons)
        {            
            ImageHash = new Dictionary<string, BitmapSource>();

            String iconChars = "";

            for (int i = 0; i < icons.Count; i++)
            {
                iconChars += i.ToString();
            }

            MaxIconWidth = 32;
            MaxIconHeight = 32;

            List<String> combos = generateCombinations(iconChars);

            generateImages(combos, icons, false);
            
        }

        private void generateImages(List<string> combos, List<BitmapImage> icons, bool saveToDisk)
        {
            for(int i = 1; i < combos.Count; i++)
            {
                String combo = combos[i];
               
                int imageWidth = combo.Length * MaxIconWidth;
                int imageHeight = MaxIconHeight;       

                RenderTargetBitmap bitmap = new RenderTargetBitmap(imageWidth, imageHeight, 96, 96, PixelFormats.Pbgra32);

                DrawingVisual drawingVisual = new DrawingVisual();             
                RenderOptions.SetBitmapScalingMode(drawingVisual, BitmapScalingMode.HighQuality);

                using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                {
                    for (int j = 0; j < combo.Length; j++)
                    {
                        BitmapImage icon = icons[int.Parse(combo[j].ToString())];

                        int x = j * MaxIconWidth;
                        int y = 0;
                        Rect destRect = new Rect(x, y, MaxIconWidth, MaxIconHeight);

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

        public int MaxIconHeight
        {
            protected set;
            get;
        }

        public int MaxIconWidth
        {
            protected set;
            get;
        }

        protected abstract String getKey(MediaItem item);      
        public abstract String getToolTip(int iconNr, MediaItem item);

        public BitmapSource getInfoIconsBitmap(MediaItem item)
        {
            if (item.ItemState != MediaItemState.LOADED || item.Metadata == null)
            {
                return (null);
            }

            String key = getKey(item);

            if (String.IsNullOrEmpty(key))
            {
                return (null);
            }
            else
            {
                return (ImageHash[key]);
            }
        }

        public int getNrIcons(MediaItem item)
        {
            String key = getKey(item);

            return String.IsNullOrEmpty(key) ? 0 : key.Length;
        }

    }
}
