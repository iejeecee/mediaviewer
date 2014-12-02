using MediaViewer.MediaDatabase;
using MediaViewer.Model.Global.Events;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.State.CollectionView;
using MediaViewer.Model.Utils;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

namespace MediaViewer.MediaGrid
{
    /// <summary>
    /// Interaction logic for MediaGridItemView2.xaml
    /// </summary>
    public partial class MediaGridItemView2 : UserControl
    {
        static InfoIconsCache InfoIconsCache { get; set; }
        static RatingCache RatingCache { get; set; }

        static IEventAggregator EventAggregator { get; set; }

        public MediaGridItemView2()
        {
            InitializeComponent();

            if(InfoIconsCache == null) {                

                InfoIconsCache = new InfoIconsCache();
                RatingCache = new RatingCache();

                EventAggregator = ServiceLocator.Current.GetInstance(typeof(IEventAggregator)) as IEventAggregator;
            }

            
        }

        public SelectableMediaFileItem SelectableMediaFileItem
        {
            get { return (SelectableMediaFileItem)GetValue(MediaFileItemProperty); }
            set { SetValue(MediaFileItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectableMediaFileItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MediaFileItemProperty =
            DependencyProperty.Register("SelectableMediaFileItem", typeof(SelectableMediaFileItem), typeof(MediaGridItemView2), new PropertyMetadata(null,selectableMediaFileItemChanged));

        private static void selectableMediaFileItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaGridItemView2 view = d as MediaGridItemView2;

            view.selectableMediaFileItemChanged(e);
        }

        void selectableMediaFileItemChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                MediaFileItem item = (e.NewValue as SelectableMediaFileItem).Item;

                WeakEventManager<MediaFileItem, PropertyChangedEventArgs>.AddHandler(item, "PropertyChanged", mediaFileItem_PropertyChanged);
                infoIconsImage.Source = InfoIconsCache.getInfoIconsBitmap(item);
            }
        }

        private void mediaFileItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => {
                
                infoIconsImage.Source = InfoIconsCache.getInfoIconsBitmap(SelectableMediaFileItem.Item);
            }));
        }

        public MediaStateSortMode ExtraInfoType
        {
            get { return (MediaStateSortMode)GetValue(ExtraInfoTypeProperty); }
            set { SetValue(ExtraInfoTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExtraInfoType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExtraInfoTypeProperty =
            DependencyProperty.Register("ExtraInfoType", typeof(MediaStateSortMode), typeof(MediaGridItemView2), new PropertyMetadata(MediaStateSortMode.Name, extraInfoTypeChangedCallback));

        private static void extraInfoTypeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaGridItemView2 view = d as MediaGridItemView2;
            view.showExtraItemInfo((MediaStateSortMode)e.NewValue);
            //view.extraInfo.InfoType = (MediaStateSortMode)e.NewValue;
        }
        

        private void imageGridItem_Click(object sender, RoutedEventArgs e)
        {
            MediaStateCollectionView cv = (this.Tag as MediaStateCollectionView);

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                SelectableMediaFileItem.IsSelected = !SelectableMediaFileItem.IsSelected;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                cv.selectRange(SelectableMediaFileItem.Item);
            }
            else
            {
                cv.deselectAll();
                SelectableMediaFileItem.IsSelected = true;
            }

        }

        private void viewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MediaFileItem item = SelectableMediaFileItem.Item;

            if (MediaViewer.Model.Utils.MediaFormatConvert.isImageFile(item.Location))
            {
                Shell.ShellViewModel.navigateToImageView(item.Location);
            }
            else if (MediaFormatConvert.isVideoFile(item.Location))
            {
                Shell.ShellViewModel.navigateToVideoView(item.Location);
            }

        }

        public bool IsGridLoaded
        {
            get { return (bool)GetValue(IsGridLoadedProperty); }
            set { SetValue(IsGridLoadedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsGridLoaded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsGridLoadedProperty =
            DependencyProperty.Register("IsGridLoaded", typeof(bool), typeof(MediaGridItemView2), new PropertyMetadata(true, isGridLoadedChangedCallback));

        private static void isGridLoadedChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaGridItemView2 item = d as MediaGridItemView2;

           // item.selectAllMenuItem.IsEnabled = !(bool)e.NewValue;

        }

        private void selectAllMenuItem_Click(object sender, RoutedEventArgs e)
        {

            MediaStateCollectionView cv = (this.Tag as MediaStateCollectionView);
            cv.selectAll();
        }

        private void deselectAllMenuItem_Click(object sender, RoutedEventArgs e)
        {

            MediaStateCollectionView cv = (this.Tag as MediaStateCollectionView);
            cv.deselectAll();

        }

        private void browseMenuItem_Click(object sender, RoutedEventArgs e)
        {

            MediaFileItem item = SelectableMediaFileItem.Item;

            String location = FileUtils.getPathWithoutFileName(item.Location);

            EventAggregator.GetEvent<MediaBrowserPathChangedEvent>().Publish(location);
        }

        private void openInExplorerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MediaFileItem item = SelectableMediaFileItem.Item;

            String location = FileUtils.getPathWithoutFileName(item.Location);

            Process.Start(location);
        }



        private void imageGridItem_Checked(object sender, RoutedEventArgs e)
        {
            SelectableMediaFileItem item = (SelectableMediaFileItem)DataContext;

            if (item.IsSelected == true) return;

            MediaStateCollectionView cv = (this.Tag as MediaStateCollectionView);
            cv.deselectAll();

            item.IsSelected = true;

        }

        private void imageGridItem_Unchecked(object sender, RoutedEventArgs e)
        {
            SelectableMediaFileItem item = (SelectableMediaFileItem)DataContext;

            item.IsSelected = false;

        }

        void showExtraItemInfo(MediaStateSortMode infoType) {

            string dateFormat = "MMM d, yyyy";

            MediaFileItem item = SelectableMediaFileItem.Item;

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

                            int nrStars = (int)item.Media.Rating.Value;

                            ratingImage.Source = RatingCache.RatingBitmap[nrStars];
                            ratingImage.Visibility = System.Windows.Visibility.Visible;
                            //view.rating.Value = item.Media.Rating.Value / 5;
                            //view.rating.Visibility = Visibility.Visible;
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

            extraInfoTextBlock.Text = info;
            extraInfoTextBlock.ToolTip = info;
        }
       

        
    }
}
