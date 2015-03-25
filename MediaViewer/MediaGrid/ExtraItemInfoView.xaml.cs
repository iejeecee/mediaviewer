using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.State.CollectionView;
using MediaViewer.Model.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediaViewer.MediaGrid
{
    /// <summary>
    /// Interaction logic for ExtraImageGridItemInfoView.xaml
    /// </summary>
    public partial class ExtraItemInfoView : UserControl
    {
        const string dateFormat = "MMM d, yyyy";

        public ExtraItemInfoView()
        {
            InitializeComponent();
        }
               
        public MediaStateSortMode InfoType
        {
            get { return (MediaStateSortMode)GetValue(InfoTypeProperty); }
            set { SetValue(InfoTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InfoType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InfoTypeProperty =
            DependencyProperty.Register("InfoType", typeof(MediaStateSortMode), typeof(ExtraItemInfoView), new PropertyMetadata(MediaStateSortMode.Name, extraImageGridItemInfoView_InfoTypeChangedCallback));

        private static void extraImageGridItemInfoView_InfoTypeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ExtraItemInfoView view = d as ExtraItemInfoView;
            MediaStateSortMode infoType = (MediaStateSortMode)e.NewValue;
            MediaItem item = (view.DataContext as SelectableMediaItem).Item;

            String info = null;

            if (item.Metadata != null)
            {
                VideoMetadata VideoMetadata = item.Metadata is VideoMetadata ? item.Metadata as VideoMetadata : null;
                ImageMetadata ImageMetadata = item.Metadata is ImageMetadata ? item.Metadata as ImageMetadata : null;

                switch (infoType)
                {
                    case MediaStateSortMode.Name:
                        break;
                    case MediaStateSortMode.Size:
                        info = MediaViewer.Model.Utils.MiscUtils.formatSizeBytes(item.Metadata.SizeBytes);
                        break;
                    case MediaStateSortMode.Rating:

                        if(item.Metadata.Rating.HasValue) {
                            view.rating.Value = item.Metadata.Rating.Value / 5;
                            view.rating.Visibility = Visibility.Visible;
                        }
                        break;
                    case MediaStateSortMode.Imported:
                        break;
                    case MediaStateSortMode.Tags:
                        if (item.Metadata.Tags.Count > 0)
                        {
                            info = item.Metadata.Tags.Count.ToString() + " tag";

                            if (item.Metadata.Tags.Count > 1)
                            {
                                info += "s";
                            }
                        }
                        break;
                    case MediaStateSortMode.MimeType:
                        info = item.Metadata.MimeType;
                        break;
                    case MediaStateSortMode.FileDate:
                        info = item.Metadata.FileDate.ToString(dateFormat);
                        break;  
                    case MediaStateSortMode.LastModified:
                        info = item.Metadata.LastModifiedDate.ToString(dateFormat);                       
                        break;                        
                    case MediaStateSortMode.CreationDate:
                        if (item.Metadata.CreationDate.HasValue)
                        {
                            info = item.Metadata.CreationDate.Value.ToString(dateFormat);
                        }
                        break;
                    case MediaStateSortMode.SoftWare:
                        if (item.Metadata.Software != null)
                        {
                            info = item.Metadata.Software;
                        }
                        break;
                    case MediaStateSortMode.Width:
                        if (ImageMetadata != null)
                        {                          
                            info = ImageMetadata.Width.ToString() + " x " + ImageMetadata.Height.ToString();
                        }
                        else
                        {                         
                            info = VideoMetadata.Width.ToString() + " x " + VideoMetadata.Height.ToString();
                        }
                        break;                        
                    case MediaStateSortMode.Height:
                        if (ImageMetadata != null)
                        {
                            info = ImageMetadata.Width.ToString() + " x " + ImageMetadata.Height.ToString();
                        }
                        else
                        {
                            info = VideoMetadata.Width.ToString() + " x " + VideoMetadata.Height.ToString();
                        }                   
                        break;
                    case MediaStateSortMode.Duration:
                        if (VideoMetadata != null)
                        {
                            info = MiscUtils.formatTimeSeconds(VideoMetadata.DurationSeconds);
                        }                        
                        break;
                    case MediaStateSortMode.FramesPerSecond:
                        if (VideoMetadata != null)
                        {
                            info = VideoMetadata.FramesPerSecond.ToString("0.00") + " FPS";
                        }
                        break;
                    case MediaStateSortMode.VideoCodec:
                        if (VideoMetadata != null)
                        {
                            info = VideoMetadata.VideoCodec;
                        }
                        break;
                    case MediaStateSortMode.AudioCodec:
                        if (VideoMetadata != null)
                        {
                            info = VideoMetadata.AudioCodec;
                        }
                        break;
                    case MediaStateSortMode.PixelFormat:
                        if (VideoMetadata != null)
                        {
                            info = VideoMetadata.PixelFormat;
                        }
                        break;
                    case MediaStateSortMode.BitsPerSample:
                        if (VideoMetadata != null)
                        {
                            info = VideoMetadata.BitsPerSample.HasValue ? VideoMetadata.BitsPerSample + "bit" : "";
                        }
                        break;
                    case MediaStateSortMode.SamplesPerSecond:
                        if (VideoMetadata != null)
                        {
                            info = VideoMetadata.SamplesPerSecond.HasValue ? VideoMetadata.SamplesPerSecond + "hz" : "";
                        }
                        break;
                    case MediaStateSortMode.NrChannels:
                        if (VideoMetadata != null)
                        {
                            info = VideoMetadata.NrChannels.HasValue ? VideoMetadata.NrChannels.Value.ToString() + " chan" : "";
                        }
                        break;
                    case MediaStateSortMode.CameraMake:
                        if (ImageMetadata != null)
                        {
                            info = ImageMetadata.CameraMake != null ? ImageMetadata.CameraMake : "";
                        }
                        break;
                    case MediaStateSortMode.CameraModel:
                        if (ImageMetadata != null)
                        {
                            info = ImageMetadata.CameraModel != null ? ImageMetadata.CameraModel : "";
                        }
                        break;
                    case MediaStateSortMode.Lens:
                        if (ImageMetadata != null)
                        {
                            info = ImageMetadata.Lens != null ? ImageMetadata.Lens : "";
                        }
                        break;
                    case MediaStateSortMode.ISOSpeedRating:
                        if (ImageMetadata != null)
                        {
                            info = ImageMetadata.ISOSpeedRating.HasValue ? "ISO: " + ImageMetadata.ISOSpeedRating.Value : "";
                        }
                        break;
                    case MediaStateSortMode.FNumber:
                        if (ImageMetadata != null)
                        {
                            info = ImageMetadata.FNumber.HasValue ? "f/" + ImageMetadata.FNumber.Value : "";
                        }
                        break;
                    case MediaStateSortMode.ExposureTime:
                        if (ImageMetadata != null)
                        {
                            info = ImageMetadata.ExposureTime.HasValue ? "1/" + 1/ImageMetadata.ExposureTime.Value + "s" : "";
                        }
                        break;
                    case MediaStateSortMode.FocalLength:
                        if (ImageMetadata != null)
                        {
                            info = ImageMetadata.FocalLength.HasValue ? ImageMetadata.FocalLength.Value + "mm" : "";
                        }
                        break;
                    default:
                        break;
                }
            }

            view.infoTextBlock.Text = info;
            view.infoTextBlock.ToolTip = info;

            if (infoType != MediaStateSortMode.Rating)
            {
                view.rating.Visibility = Visibility.Collapsed;
                view.infoTextBlock.Visibility = Visibility.Visible;
            }
            else
            {          
                view.infoTextBlock.Visibility = Visibility.Collapsed;
            }
           
        }

    }
}
