using MediaViewer.MediaDatabase;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediaViewer.ImageGrid
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
            MediaFileItem item = view.DataContext as MediaFileItem;

            String info = null;

            if (item.Media != null)
            {
                VideoMedia videoMedia = item.Media is VideoMedia ? item.Media as VideoMedia : null;
                ImageMedia imageMedia = item.Media is ImageMedia ? item.Media as ImageMedia : null;

                switch (infoType)
                {
                    case MediaStateSortMode.Name:
                        break;
                    case MediaStateSortMode.Size:
                        info = MediaViewer.Model.Utils.MiscUtils.formatSizeBytes(item.Media.SizeBytes);
                        break;
                    case MediaStateSortMode.Rating:

                        if(item.Media.Rating.HasValue) {
                            view.rating.Value = item.Media.Rating.Value / 5;
                            view.rating.Visibility = Visibility.Visible;
                        }
                        break;
                    case MediaStateSortMode.Imported:
                        break;
                    case MediaStateSortMode.Tags:
                        if (item.Media.Tags.Count > 0)
                        {
                            info = item.Media.Tags.Count.ToString() + " tag";

                            if (item.Media.Tags.Count > 1)
                            {
                                info += "s";
                            }
                        }
                        break;
                    case MediaStateSortMode.MimeType:
                        info = item.Media.MimeType;
                        break;
                    case MediaStateSortMode.FileDate:
                        info = item.Media.FileDate.ToString(dateFormat);
                        break;  
                    case MediaStateSortMode.LastModified:
                        info = item.Media.LastModifiedDate.ToString(dateFormat);                       
                        break;                        
                    case MediaStateSortMode.CreationDate:
                        if (item.Media.CreationDate.HasValue)
                        {
                            info = item.Media.CreationDate.Value.ToString(dateFormat);
                        }
                        break;
                    case MediaStateSortMode.SoftWare:
                        if (item.Media.Software != null)
                        {
                            info = item.Media.Software;
                        }
                        break;
                    case MediaStateSortMode.Width:
                        if (imageMedia != null)
                        {                          
                            info = imageMedia.Width.ToString() + " x " + imageMedia.Height.ToString();
                        }
                        else
                        {                         
                            info = videoMedia.Width.ToString() + " x " + videoMedia.Height.ToString();
                        }
                        break;                        
                    case MediaStateSortMode.Height:
                        if (imageMedia != null)
                        {
                            info = imageMedia.Width.ToString() + " x " + imageMedia.Height.ToString();
                        }
                        else
                        {
                            info = videoMedia.Width.ToString() + " x " + videoMedia.Height.ToString();
                        }                   
                        break;
                    case MediaStateSortMode.Duration:
                        if (videoMedia != null)
                        {
                            info = MiscUtils.formatTimeSeconds(videoMedia.DurationSeconds);
                        }                        
                        break;
                    case MediaStateSortMode.FramesPerSecond:
                        if (videoMedia != null)
                        {
                            info = videoMedia.FramesPerSecond.ToString("0.00") + " FPS";
                        }
                        break;
                    case MediaStateSortMode.VideoCodec:
                        if (videoMedia != null)
                        {
                            info = videoMedia.VideoCodec;
                        }
                        break;
                    case MediaStateSortMode.AudioCodec:
                        if (videoMedia != null)
                        {
                            info = videoMedia.AudioCodec;
                        }
                        break;
                    case MediaStateSortMode.PixelFormat:
                        if (videoMedia != null)
                        {
                            info = videoMedia.PixelFormat;
                        }
                        break;
                    case MediaStateSortMode.BitsPerSample:
                        if (videoMedia != null)
                        {
                            info = videoMedia.BitsPerSample.HasValue ? videoMedia.BitsPerSample + "bit" : "";
                        }
                        break;
                    case MediaStateSortMode.SamplesPerSecond:
                        if (videoMedia != null)
                        {
                            info = videoMedia.SamplesPerSecond.HasValue ? videoMedia.SamplesPerSecond + "hz" : "";
                        }
                        break;
                    case MediaStateSortMode.NrChannels:
                        if (videoMedia != null)
                        {
                            info = videoMedia.NrChannels.HasValue ? videoMedia.NrChannels.Value.ToString() + " chan" : "";
                        }
                        break;
                    case MediaStateSortMode.CameraMake:
                        if (imageMedia != null)
                        {
                            info = imageMedia.CameraMake != null ? imageMedia.CameraMake : "";
                        }
                        break;
                    case MediaStateSortMode.CameraModel:
                        if (imageMedia != null)
                        {
                            info = imageMedia.CameraModel != null ? imageMedia.CameraModel : "";
                        }
                        break;
                    case MediaStateSortMode.Lens:
                        if (imageMedia != null)
                        {
                            info = imageMedia.Lens != null ? imageMedia.Lens : "";
                        }
                        break;
                    case MediaStateSortMode.ISOSpeedRating:
                        if (imageMedia != null)
                        {
                            info = imageMedia.ISOSpeedRating.HasValue ? "ISO: " + imageMedia.ISOSpeedRating.Value : "";
                        }
                        break;
                    case MediaStateSortMode.FNumber:
                        if (imageMedia != null)
                        {
                            info = imageMedia.FNumber.HasValue ? "f/" + imageMedia.FNumber.Value : "";
                        }
                        break;
                    case MediaStateSortMode.ExposureTime:
                        if (imageMedia != null)
                        {
                            info = imageMedia.ExposureTime.HasValue ? "1/" + 1/imageMedia.ExposureTime.Value + "s" : "";
                        }
                        break;
                    case MediaStateSortMode.FocalLength:
                        if (imageMedia != null)
                        {
                            info = imageMedia.FocalLength.HasValue ? imageMedia.FocalLength.Value + "mm" : "";
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
                view.rating.Visibility = Visibility.Hidden;
                view.infoTextBlock.Visibility = Visibility.Visible;
            }
            else
            {          
                view.infoTextBlock.Visibility = Visibility.Hidden;
            }
           
        }

    }
}
