using MediaViewer.MediaDatabase;
using MediaViewer.Model.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using VideoLib;

namespace MediaViewer.GridImage.VideoPreviewImage
{
   class VideoGridImage : GridImageBase
    {            
        static List<BitmapSource> getImages(List<VideoThumb> thumbs)
        {
            List<BitmapSource> images = new List<BitmapSource>();

            foreach(VideoThumb thumb in thumbs) {

                images.Add(thumb.Thumb);
            }

            return (images);
        }

        public VideoGridImage(VideoMedia video, VideoPreviewImageViewModel vm, List<VideoThumb> thumbs) :
            base(vm.MaxPreviewImageWidth,vm.NrRows, vm.NrColumns, getImages(thumbs))
        {                                 
            Video = video;
            Vm = vm;
            Thumbs = thumbs;
        }

        List<VideoThumb> Thumbs { get; set; }
        VideoMedia Video { get; set; }
        VideoPreviewImageViewModel Vm { get; set; }

        override protected void createHeader(Grid mainGrid, String fontFamily, int margin)
        {
            if (Vm.IsAddHeader == false) return;
          
            Grid headerGrid = new Grid();
            headerGrid.Margin = new Thickness(margin, 0, 0, 0);
            
            ColumnDefinition labelColumn = new ColumnDefinition() { Width = GridLength.Auto };
            ColumnDefinition valueColumn = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };

            headerGrid.ColumnDefinitions.Add(labelColumn);
            headerGrid.ColumnDefinitions.Add(valueColumn);

            List<TextBlock> labels = new List<TextBlock>();
            List<TextBlock> values = new List<TextBlock>();

            labels.Add(new TextBlock(new Run("Filename: ")));
            values.Add(new TextBlock(new Run(Path.GetFileName(Video.Location))));

            labels.Add(new TextBlock(new Run("Container: ")));
            values.Add(new TextBlock(new Run(Video.VideoContainer)));

            if (Video.Software != null)
            {
                labels.Add(new TextBlock(new Run("Encoder: ")));
                values.Add(new TextBlock(new Run(Video.Software)));
            }

            labels.Add(new TextBlock(new Run("Video Codec: ")));

            String videoCodecInfo = Video.VideoCodec + ", " + Video.Width + "x" + Video.Height + ", " + Video.FramesPerSecond.ToString("0.00") + "fps" + ", " + Video.PixelFormat;

            values.Add(new TextBlock(new Run(videoCodecInfo)));
           
            if(Video.AudioCodec != null) {

                labels.Add(new TextBlock(new Run("Audio Codec: ")));

                String audioCodecInfo = Video.AudioCodec + ", " + Video.NrChannels + "chan" + ", " + Video.SamplesPerSecond + "hz, " + Video.BitsPerSample + "bit";

                values.Add(new TextBlock(new Run(audioCodecInfo)));
            }

            labels.Add(new TextBlock(new Run("Duration: ")));
            values.Add(new TextBlock(new Run(MiscUtils.formatTimeSeconds(Video.DurationSeconds))));

            labels.Add(new TextBlock(new Run("Size: ")));
            values.Add(new TextBlock(new Run(MiscUtils.formatSizeBytes(Video.SizeBytes))));
           
            if (Video.Tags.Count > 0 && Vm.IsAddTags)
            {
                labels.Add(new TextBlock(new Run("Tags: ")));
               
                String tags = Video.Tags.ElementAt(0).Name;

                for (int i = 1; i < Video.Tags.Count(); i++)
                {                   
                    tags += ", " + Video.Tags.ElementAt(i).Name;                   
                }

                TextBlock tagsTextBlock = new TextBlock(new Run(tags));
                tagsTextBlock.TextWrapping = TextWrapping.Wrap;

                values.Add(tagsTextBlock);
            }

            if (!String.IsNullOrEmpty(Vm.Comment) && Vm.IsCommentEnabled)
            {
                labels.Add(new TextBlock(new Run("Comment: ")));
                values.Add(new TextBlock(new Run(Vm.Comment)));
            }
            
            for (int i = 0; i < labels.Count; i++)
            {
                RowDefinition row = new RowDefinition() { Height = GridLength.Auto };
                headerGrid.RowDefinitions.Add(row);

                Grid.SetRow(labels[i],i);
                Grid.SetColumn(labels[i],0);

                Grid.SetRow(values[i],i);
                Grid.SetColumn(values[i],1);

                headerGrid.Children.Add(labels[i]);
                headerGrid.Children.Add(values[i]);

                labels[i].TextAlignment = TextAlignment.Right;
                Run labelRun = ((Run)labels[i].Inlines.First());
                labelRun.FontFamily = new FontFamily(fontFamily);
                labelRun.FontSize = Vm.FontSize;
                labelRun.Foreground = new SolidColorBrush(Colors.Gray);

                Run valueRun = ((Run)values[i].Inlines.First());
                valueRun.FontFamily = new FontFamily(fontFamily);
                valueRun.FontSize = Vm.FontSize;
            }

            Grid.SetRow(headerGrid, 0);
            mainGrid.Children.Add(headerGrid);
                  
        }

        override protected void addImageInfo(int imageNr, Grid cell, String fontFamily, int margin)
        {
            if (Vm.IsAddTimestamps == false) return;

            String time = MiscUtils.formatTimeSeconds(Thumbs[imageNr].PositionSeconds);
            Run timeRun = new Run(time);
            timeRun.FontFamily = new FontFamily(fontFamily);
            timeRun.FontSize = 12;
            timeRun.Background = new SolidColorBrush(Colors.Black);
            timeRun.Foreground = new SolidColorBrush(Colors.White);

            TextBlock timeStamp = new TextBlock(timeRun);
            timeStamp.HorizontalAlignment = HorizontalAlignment.Right;
            timeStamp.VerticalAlignment = VerticalAlignment.Bottom;

            timeStamp.Margin = new Thickness(margin + borderThickness);            

            Grid.SetRow(timeStamp, 1);
          
            cell.Children.Add(timeStamp);
        }


       
    }
}
