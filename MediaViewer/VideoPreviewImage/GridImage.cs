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
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using VideoLib;

namespace MediaViewer.VideoPreviewImage
{
    class GridImage
    {

        int headerHightPixels;
        PixelFormat gridPixelFormat = PixelFormats.Pbgra32;
        const int subImageMargin = 3;
        const int timeStampMargin = 2;
        const int spaceMargin = 5;

        RenderTargetBitmap image;

        public RenderTargetBitmap Image
        {

            get { return image; }
        }

        public GridImage(VideoMedia video, VideoPreviewImageViewModel vm, int width, int height, int nrRows, int nrColumns, List<VideoThumb> thumbs)
        {           
            this.nrRows = nrRows;
            this.nrColumns = nrColumns;
            this.width = width;
            this.height = height;
            this.bytesPerPixel = gridPixelFormat.BitsPerPixel / 8;
           
            createGridImage(video, vm, thumbs);
           
        }
       

        int bytesPerPixel;

        public int BytesPerPixel
        {
            get { return bytesPerPixel; }
          
        }

        int nrRows;

        public int NrRows
        {
            get { return nrRows; }
         
        }
        int nrColumns;

        public int NrColumns
        {
            get { return nrColumns; }
           
        }

        int width;

        public int Width
        {
            get { return width; }
          
        }
        int height;

        public int Height
        {
            get { return height; }
          
        }

        FormattedText createFormattedText(String text, String font, double size, Color color, FontWeight weight)
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

        String createHeaderText(VideoMedia video, VideoPreviewImageViewModel vm, bool labels)
        {
            StringBuilder sb = new StringBuilder();

            if (labels == true) sb.AppendLine("Name: "); else sb.AppendLine(Path.GetFileName(video.Location));
            if (labels == true) sb.AppendLine("Container: "); else sb.AppendLine(video.VideoContainer);

            if (video.Software != null)
            {
                if (labels == true) sb.AppendLine("Encoder: "); else sb.AppendLine(video.Software);
            }

            if (labels == true) sb.AppendLine("Video Codec: "); else sb.AppendLine(video.VideoCodec + ", " + video.Width + "x" + video.Height + ", " + video.FramesPerSecond.ToString("0.00") + "fps" + ", " + video.PixelFormat);

            if(video.AudioCodec != null) {

                if (labels == true) sb.AppendLine("Audio Codec: "); else sb.AppendLine(video.AudioCodec + ", " + video.NrChannels + "chan" + ", " + video.SamplesPerSecond + "hz, " + video.BitsPerSample + "bit");
            }

            if (labels == true) sb.AppendLine("Duration: "); else sb.AppendLine(MiscUtils.formatTimeSeconds(video.DurationSeconds));
            if (labels == true) sb.AppendLine("Size: "); else sb.AppendLine(MiscUtils.formatSizeBytes(video.SizeBytes));

            if (video.Tags.Count > 0 && vm.IsAddTags)
            {
                if (labels == true) sb.Append("Tags: ");
                else
                {
                    for (int i = 0; i < video.Tags.Count(); i++)
                    {

                        sb.Append(video.Tags.ElementAt(i).Name);
                        if (i != video.Tags.Count() - 1)
                        {
                            sb.Append(", ");
                        }
                    }
                }
            }
                       
            return (sb.ToString());
        }

        
        void createGridImage(VideoMedia video, VideoPreviewImageViewModel vm, List<VideoThumb> thumbs)
        {
            String headerTextLabels = createHeaderText(video, vm, true);

            FormattedText formattedHeaderTextLabels =
                createFormattedText(headerTextLabels, "Consolas", 20, Colors.Black, FontWeights.Normal);

            formattedHeaderTextLabels.TextAlignment = TextAlignment.Right;
            formattedHeaderTextLabels.MaxTextWidth = Width;
            
            String headerTextValues = createHeaderText(video, vm, false);

            FormattedText formattedHeaderTextValues =
                createFormattedText(headerTextValues, "Consolas", 20, Colors.Black, FontWeights.Normal);

            formattedHeaderTextValues.MaxTextWidth = Width - formattedHeaderTextLabels.Width - spaceMargin;

            if (vm.IsAddHeader == true)
            {
                headerHightPixels = (int)formattedHeaderTextValues.Height;
            }
            else
            {
                headerHightPixels = 0;
            }

            image = new RenderTargetBitmap(Width, Height + headerHightPixels, 96, 96, gridPixelFormat);
                                
            DrawingVisual drawingVisual = new DrawingVisual();
            RenderOptions.SetBitmapScalingMode(drawingVisual, BitmapScalingMode.HighQuality);
            TextOptions.SetTextFormattingMode(drawingVisual, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(drawingVisual, TextRenderingMode.ClearType);

            using (DrawingContext drawingContext = drawingVisual.RenderOpen())                        
            {                
                SolidColorBrush backgroundBrush = new SolidColorBrush(Colors.White);
      
                drawingContext.DrawRectangle(backgroundBrush, null,
                    new Rect(new Point(0,0), new Size(Width, Height + headerHightPixels)));
              
                if (vm.IsAddHeader)
                {
                    Geometry labelsGeometry = formattedHeaderTextLabels.BuildGeometry(new Point(-Width + formattedHeaderTextLabels.Width + subImageMargin, subImageMargin));
                    Brush labelsBrush = new SolidColorBrush(Colors.Gray);

                    Geometry valuesGeometry = formattedHeaderTextValues.BuildGeometry(new Point(formattedHeaderTextLabels.Width + subImageMargin + spaceMargin, subImageMargin));
                    Brush valuesBrush = new SolidColorBrush(Colors.Black);

                    drawingContext.DrawGeometry(labelsBrush, new Pen(labelsBrush, 1), labelsGeometry);
                    drawingContext.DrawGeometry(valuesBrush, new Pen(valuesBrush, 1), valuesGeometry);
                }
                                
                for (int i = 0; i < thumbs.Count; i++)
                {
                    addSubImage(drawingContext, thumbs[i], i, vm);
                }
                                
            }

            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
            TextOptions.SetTextFormattingMode(image, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(image, TextRenderingMode.ClearType);
            image.Render(drawingVisual);
                  
        }

        void addSubImage(DrawingContext drawingContext, VideoThumb thumb, int pos, VideoPreviewImageViewModel vm)
        {                 
            int columnNr = pos % nrColumns;
            int rowNr = pos / nrColumns;

            BitmapSource subImage = thumb.Thumb;

            int x = columnNr * subImage.PixelWidth + subImageMargin;
            int y = rowNr * subImage.PixelHeight + headerHightPixels + subImageMargin;
            Rect destRect = new Rect(x, y, 
                subImage.PixelWidth - subImageMargin * 2, subImage.PixelHeight - subImageMargin * 2);
     
            drawingContext.DrawImage(subImage, destRect);

            if (vm.IsAddTimestamps == false) return;

            FormattedText timeStamp = createFormattedText(MiscUtils.formatTimeSeconds(thumb.PositionSeconds),
                "Consolas", 12, Colors.White, FontWeights.Normal);

            double timeStampPosX = x + subImage.PixelWidth - subImageMargin * 2 - timeStamp.Width - timeStampMargin;
            double timeStampPosY = y + subImage.PixelHeight - subImageMargin * 2 - timeStamp.Height - timeStampMargin;
       
            Geometry textGeometry = timeStamp.BuildGeometry(new Point(timeStampPosX, timeStampPosY));

            Geometry textHighLightGeometry = timeStamp.BuildHighlightGeometry(new Point(timeStampPosX, timeStampPosY));

            Brush highLightBrush = new SolidColorBrush(Colors.Black);
            Brush textBrush = new SolidColorBrush(Colors.White);

            drawingContext.DrawGeometry(highLightBrush, new Pen(highLightBrush, 1), textHighLightGeometry);
            drawingContext.DrawGeometry(textBrush, new Pen(textBrush, 1), textGeometry);
                    
        }
               
    }
}
