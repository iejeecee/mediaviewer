using MediaViewer.MediaDatabase;
using MediaViewer.Model.Global.Events;
using MediaViewer.Model.Media.Base;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediaViewer.MediaGrid
{
    /// <summary>
    /// Interaction logic for MediaGridItemView.xaml
    /// </summary>
    public partial class MediaGridItemView : UserControl
    {
        static InfoIconsCache InfoIconsCache { get; set; }
        static RatingCache RatingCache { get; set; }

        static IEventAggregator EventAggregator { get; set; }

        public MediaGridItemView()
        {
            InitializeComponent();

            if(InfoIconsCache == null) {                

                InfoIconsCache = new InfoIconsCache();
                RatingCache = new RatingCache();

                EventAggregator = ServiceLocator.Current.GetInstance(typeof(IEventAggregator)) as IEventAggregator;
            }

            
        }

        public SelectableMediaItem SelectableMediaItem
        {
            get { return (SelectableMediaItem)GetValue(MediaFileItemProperty); }
            set { SetValue(MediaFileItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectableMediaFileItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MediaFileItemProperty =
            DependencyProperty.Register("SelectableMediaItem", typeof(SelectableMediaItem), typeof(MediaGridItemView), new PropertyMetadata(null,selectableMediaItemChanged));

        private static void selectableMediaItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaGridItemView view = d as MediaGridItemView;

            view.selectableMediaFileItemChanged(e);
        }

        void selectableMediaFileItemChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                MediaItem item = (e.NewValue as SelectableMediaItem).Item;

                WeakEventManager<MediaItem, PropertyChangedEventArgs>.AddHandler(item, "PropertyChanged", mediaItem_PropertyChanged);
                infoIconsImage.Source = InfoIconsCache.getInfoIconsBitmap(item);
            }
        }

        private void mediaItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => {
                
                infoIconsImage.Source = InfoIconsCache.getInfoIconsBitmap(SelectableMediaItem.Item);
            }));
        }

        public MediaStateSortMode ExtraInfoType
        {
            get { return (MediaStateSortMode)GetValue(ExtraInfoTypeProperty); }
            set { SetValue(ExtraInfoTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExtraInfoType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExtraInfoTypeProperty =
            DependencyProperty.Register("ExtraInfoType", typeof(MediaStateSortMode), typeof(MediaGridItemView), new PropertyMetadata(MediaStateSortMode.Name, extraInfoTypeChangedCallback));

        private static void extraInfoTypeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaGridItemView view = d as MediaGridItemView;
            view.showExtraItemInfo((MediaStateSortMode)e.NewValue);
            //view.extraInfo.InfoType = (MediaStateSortMode)e.NewValue;
        }
        

        private void imageGridItem_Click(object sender, RoutedEventArgs e)
        {
            MediaStateCollectionView cv = (this.Tag as MediaStateCollectionView);

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                SelectableMediaItem.IsSelected = !SelectableMediaItem.IsSelected;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                cv.selectRange(SelectableMediaItem.Item);
            }
            else
            {
                cv.deselectAll();
                SelectableMediaItem.IsSelected = true;
            }

        }

        private void viewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MediaItem item = SelectableMediaItem.Item;

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
            DependencyProperty.Register("IsGridLoaded", typeof(bool), typeof(MediaGridItemView), new PropertyMetadata(true, isGridLoadedChangedCallback));

        private static void isGridLoadedChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaGridItemView item = d as MediaGridItemView;

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

            MediaItem item = SelectableMediaItem.Item;

            String location = FileUtils.getPathWithoutFileName(item.Location);

            EventAggregator.GetEvent<MediaBrowserPathChangedEvent>().Publish(location);
        }

        private void openInExplorerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MediaItem item = SelectableMediaItem.Item;

            String location = FileUtils.getPathWithoutFileName(item.Location);

            Process.Start(location);
        }



        private void imageGridItem_Checked(object sender, RoutedEventArgs e)
        {
            SelectableMediaItem item = (SelectableMediaItem)DataContext;

            if (item.IsSelected == true) return;

            MediaStateCollectionView cv = (this.Tag as MediaStateCollectionView);
            cv.deselectAll();

            item.IsSelected = true;

        }

        private void imageGridItem_Unchecked(object sender, RoutedEventArgs e)
        {
            SelectableMediaItem item = (SelectableMediaItem)DataContext;

            item.IsSelected = false;

        }

        void showExtraItemInfo(MediaStateSortMode infoType) {

            string dateFormat = "MMM d, yyyy";

            MediaItem item = SelectableMediaItem.Item;

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

                            int nrStars = (int)item.Metadata.Rating.Value;

                            ratingImage.Source = RatingCache.RatingBitmap[nrStars];
                            ratingImage.Visibility = System.Windows.Visibility.Visible;
                            //view.rating.Value = item.Media.Rating.Value / 5;
                            //view.rating.Visibility = Visibility.Visible;
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

            extraInfoTextBlock.Text = info;
            extraInfoTextBlock.ToolTip = info;
        }
       

        
    }
}
