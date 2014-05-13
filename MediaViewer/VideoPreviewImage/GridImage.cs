using MediaViewer.MediaDatabase;
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
using System.Windows.Media.Imaging;

namespace MediaViewer.VideoPreviewImage
{
    class GridImage
    {

        int headerHightPixels;
        PixelFormat gridPixelFormat = PixelFormats.Pbgra32;

        WriteableBitmap image;

        public WriteableBitmap Image
        {

            get { return image; }
        }

        public GridImage(VideoMedia video, VideoPreviewImageViewModel vm, int width, int height, int nrRows, int nrColumns)
        {           
            this.nrRows = nrRows;
            this.nrColumns = nrColumns;
            this.width = width;
            this.height = height;
            this.bytesPerPixel = gridPixelFormat.BitsPerPixel / 8;

            addHeader(video, vm);
           
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

        String createHeaderString(VideoMedia video, VideoPreviewImageViewModel vm)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Name: " + Path.GetFileName(video.Location));
            sb.Append("Container: " + video.VideoContainer);
            if (video.Software != null)
            {
                sb.Append(" Encoder: " + video.Software);
            }
            sb.AppendLine();
            sb.AppendLine("Video Codec: " + video.VideoCodec + ", " + video.Width + "x" + video.Height + ", " + video.FramesPerSecond.ToString("0.00") + "fps" + ", " + video.PixelFormat);
            if(video.AudioCodec != null) {
                sb.AppendLine("Audio Codec: " + video.AudioCodec + ", " + video.NrChannels + "chan" + ", " + video.SamplesPerSecond + "hz, " + video.BitsPerSample + "bit");
            }
            sb.AppendLine("Duration: " + Utils.Misc.formatTimeSeconds(video.DurationSeconds));
            sb.AppendLine("Size: " + Utils.Misc.formatSizeBytes(video.SizeBytes));

            if (vm.IsCommentEnabled && !String.IsNullOrEmpty(vm.Comment))
            {
                sb.AppendLine(vm.Comment);
            }

            if (video.Tags.Count > 0 && vm.IsAddTags)
            {
                sb.Append("Tags: ");
                for (int i = 0; i < video.Tags.Count(); i++)
                {

                    sb.Append(video.Tags.ElementAt(i).Name);
                    if (i != video.Tags.Count() - 1)
                    {
                        sb.Append(", ");
                    }
                }
            }
                        
            return (sb.ToString());
        }

        void addHeader(VideoMedia video, VideoPreviewImageViewModel vm)
        {            
            FontFamily family = new FontFamily("Calibri");
            Typeface typeface = new Typeface(family, FontStyles.Normal, FontWeights.Black, new FontStretch());

            SolidColorBrush headerForegroundBrush = new SolidColorBrush(Colors.Black);

            FormattedText text = new FormattedText(createHeaderString(video, vm),
                 new CultureInfo("en-us"),
                 FlowDirection.LeftToRight,
                 typeface,
                 20,
                 headerForegroundBrush, null, TextFormattingMode.Display);

            text.MaxTextWidth = Width;
            headerHightPixels = (int)text.Height;

            RenderTargetBitmap header = new RenderTargetBitmap(Width, headerHightPixels, 96, 96, gridPixelFormat);
                     
            DrawingVisual drawingVisual = new DrawingVisual();
            TextOptions.SetTextFormattingMode(drawingVisual, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(drawingVisual, TextRenderingMode.ClearType);
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())                        
            {

                //SolidColorBrush headerBackgroundBrush = (SolidColorBrush)Application.Current.FindResource("imageGridBackgroundColorBrush");
                SolidColorBrush headerBackgroundBrush = new SolidColorBrush(Colors.White);

                drawingContext.DrawRectangle(headerBackgroundBrush, null,
                    new Rect(new Point(0,0), new Size(header.Width, header.Height)));

                drawingContext.DrawText(text, new Point(0.5, 0.5));
                                
            }

            TextOptions.SetTextFormattingMode(header, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(header, TextRenderingMode.ClearType);              
            header.Render(drawingVisual);
       
            int headerStride = header.PixelWidth * BytesPerPixel;

            byte[] headerData = new byte[headerStride * header.PixelHeight];            
            header.CopyPixels(headerData, headerStride, 0);

            Int32Rect destRect = new Int32Rect(0, 0, header.PixelWidth, header.PixelHeight);

            image = new WriteableBitmap(Width, Height + headerHightPixels, 96, 96, gridPixelFormat,
               BitmapPalettes.BlackAndWhite);

            image.WritePixels(destRect, headerData, headerStride, 0);
        }

        public void addSubImage(BitmapSource subImage, int pos)
        {         
            FormatConvertedBitmap convertedSubImage = new FormatConvertedBitmap();
            convertedSubImage.BeginInit();
            convertedSubImage.Source = subImage;
            convertedSubImage.DestinationFormat = gridPixelFormat;
            convertedSubImage.EndInit();

            int columnNr = pos % nrColumns;
            int rowNr = pos / nrColumns;

            int subImageStride = subImage.PixelWidth * BytesPerPixel;
                             
            byte[] imageData = new byte[subImageStride * subImage.PixelHeight];
            convertedSubImage.CopyPixels(imageData, subImageStride, 0);
           
            int x = columnNr * subImage.PixelWidth;
            int y = rowNr * subImage.PixelHeight;
            Int32Rect destRect = new Int32Rect(x, y + headerHightPixels, 
                subImage.PixelWidth, subImage.PixelHeight);

            image.WritePixels(destRect,imageData,subImageStride,0);
        
        }

        
       

    }
}
