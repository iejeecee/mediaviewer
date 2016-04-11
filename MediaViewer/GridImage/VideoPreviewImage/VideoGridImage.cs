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
        static List<BitmapSource> getImages(List<MediaThumb> thumbs)
        {
            List<BitmapSource> images = new List<BitmapSource>();

            foreach(MediaThumb videoThumb in thumbs) {

                images.Add(videoThumb.Thumb);
            }

            return (images);
        }

        public VideoGridImage(VideoMetadata video, VideoPreviewImageViewModel vm, List<MediaThumb> thumbs) :
            base(vm.MaxPreviewImageWidth,vm.NrRows, vm.NrColumns, getImages(thumbs), vm.BackgroundColor, vm.FontColor)
        {                                 
            Video = video;
            Vm = vm;
            Thumbs = thumbs;
        }

        List<MediaThumb> Thumbs { get; set; }
        VideoMetadata Video { get; set; }
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
            List<UIElement> values = new List<UIElement>();

            labels.Add(new TextBlock(new Run("Filename: ")));
            TextBlock location = new TextBlock(new Run(Path.GetFileName(Video.Location)));
            location.TextWrapping = TextWrapping.Wrap;
            values.Add(location);

            labels.Add(new TextBlock(new Run("Container: ")));
            values.Add(new TextBlock(new Run(Video.VideoContainer)));

            if (Video.Software != null)
            {
                labels.Add(new TextBlock(new Run("Encoder: ")));
                values.Add(new TextBlock(new Run(Video.Software)));
            }
            
            labels.Add(new TextBlock(new Run("Video Codec: ")));

            String videoRate = Video.VideoBitRate.HasValue ? MiscUtils.formatSizeBytes(Video.VideoBitRate.Value / 8) + "/s" : "";

            String videoCodecInfo = Video.VideoCodec + ", " + Video.Width + "x" + Video.Height + ", " + Video.FramesPerSecond.ToString("0.00") + "fps" + ", " + Video.PixelFormat + ", " + videoRate;

            values.Add(new TextBlock(new Run(videoCodecInfo)));
           
            if(Video.AudioCodec != null) {

                String audioRate = Video.AudioBitRate.HasValue ? MiscUtils.formatSizeBytes(Video.AudioBitRate.Value / 8) + "/s" : "";

                labels.Add(new TextBlock(new Run("Audio Codec: ")));

                String audioCodecInfo = Video.AudioCodec + ", " + Video.NrChannels + "chan" + ", " + Video.SamplesPerSecond + "hz, " + Video.BitsPerSample + "bit" + ", " + audioRate;

                values.Add(new TextBlock(new Run(audioCodecInfo)));
            }

            labels.Add(new TextBlock(new Run("Duration: ")));
            values.Add(new TextBlock(new Run(MiscUtils.formatTimeSeconds(Video.DurationSeconds))));

            labels.Add(new TextBlock(new Run("Size: ")));
            values.Add(new TextBlock(new Run(MiscUtils.formatSizeBytes(Video.SizeBytes))));

            if (Video.Rating != null)
            {
                labels.Add(new TextBlock(new Run("Rating: ")));
            
                Rating ratingControl = new Rating();
                ratingControl.HorizontalAlignment = HorizontalAlignment.Left;
                ratingControl.VerticalAlignment = VerticalAlignment.Top;
                ratingControl.ItemCount = 5;
                ratingControl.Foreground = new SolidColorBrush(Colors.Red);
                ratingControl.Background = null;
                ratingControl.IsReadOnly = true;
                ratingControl.SelectionMode = RatingSelectionMode.Continuous;

                ratingControl.Value = Video.Rating * (1.0 / 5);

                values.Add(ratingControl);
              
            }

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
                labelRun.Foreground = new SolidColorBrush(FontColor);

                if (values[i] is TextBlock)
                {
                    TextBlock value = values[i] as TextBlock;

                    Run valueRun = (Run)value.Inlines.First();
                    valueRun.FontFamily = new FontFamily(fontFamily);
                    valueRun.FontSize = Vm.FontSize;
                    valueRun.Foreground = new SolidColorBrush(FontColor);
                }
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
