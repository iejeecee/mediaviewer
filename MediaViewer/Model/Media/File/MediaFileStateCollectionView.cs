using MediaViewer.Filter;
using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Media.State;
using MediaViewer.Model.Media.State.CollectionView;
using MediaViewer.Model.Utils;
using MediaViewer.UserControls.MediaGridItem;
using MediaViewer.UserControls.SortComboBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MediaViewer.Model.Media.File
{
    public enum MediaFilterMode
    {
        None,
        Video,
        Images
    }

    public sealed class MediaFileStateCollectionView : MediaStateCollectionView
    {
        static RatingCache RatingCache { get; set; }
        static InfoIconsCache InfoIconsCacheStatic { get; set; }

        MediaFileSortMode SortMode { get; set; }

        static MediaFileStateCollectionView()
        {
            RatingCache = new RatingCache();

            List<BitmapImage> icons = new List<BitmapImage>();

            String iconPath = "pack://application:,,,/Resources/Icons/";

            icons.Add(new BitmapImage(new Uri(iconPath + "checked.ico", UriKind.Absolute)));
            icons.Add(new BitmapImage(new Uri(iconPath + "notsupported.ico", UriKind.Absolute)));
            icons.Add(new BitmapImage(new Uri(iconPath + "tag.ico", UriKind.Absolute)));
            icons.Add(new BitmapImage(new Uri(iconPath + "geotag.ico", UriKind.Absolute)));

            InfoIconsCacheStatic = new MediaFileInfoIconsCache(icons);
        }
        
        public MediaFileStateCollectionView(MediaFileState mediaState = null) :
            base(mediaState)
        {           
            Filter = filterFunc;
            InfoIconsCache = InfoIconsCacheStatic;

            MediaFilter = MediaFilterMode.None;           

            SortFunc = MediaFileSortFunctions.getSortFunction(MediaFileSortMode.Name);
            SortMode = MediaFileSortMode.Name;
            
            FilterModes = new ListCollectionView(Enum.GetValues(typeof(MediaFilterMode)));

            SortItemCollection<MediaFileSortItem, MediaFileSortMode> mediaFileSortItemCollection = new SortItemCollection<MediaFileSortItem,MediaFileSortMode>();
            mediaFileSortItemCollection.ItemSortDirectionChanged += mediaFileSortItemCollection_ItemSortDirectionChanged;

            foreach(MediaFileSortMode mode in Enum.GetValues(typeof(MediaFileSortMode))) {

                mediaFileSortItemCollection.Add(new MediaFileSortItem(mode));
            }

            SortModes = new ListCollectionView(mediaFileSortItemCollection);
         
            SortModes.CurrentChanged += (s, e) =>
            {
                MediaFileSortItem sortItem = (MediaFileSortItem)SortModes.CurrentItem;

                SortMode = sortItem.SortMode;
                SortDirection = sortItem.SortDirection;

                SortFunc = MediaFileSortFunctions.getSortFunction(SortMode);

                refresh();
            };

            SortModes.Filter = mediaStateSortModeCollectionViewFilter;

            FilterModes.CurrentChanged += (s, e) =>
            {
                MediaFilter = (MediaFilterMode)FilterModes.CurrentItem;
                              
                SortModes.Refresh();

                bool isRefreshed = false;

                switch (MediaFilter)
                {
                    case MediaFilterMode.None:
                        if (!MediaFileSortFunctions.isAllSortMode(SortMode))
                        {
                            SortModes.MoveCurrentToFirst();
                            isRefreshed = true;
                        }
                        break;
                    case MediaFilterMode.Video:
                        if (!MediaFileSortFunctions.isVideoSortMode(SortMode))
                        {
                            SortModes.MoveCurrentToFirst();
                            isRefreshed = true;
                        }
                        break;
                    case MediaFilterMode.Images:
                        if (!MediaFileSortFunctions.isImageSortMode(SortMode))
                        {
                            SortModes.MoveCurrentToFirst();
                            isRefreshed = true;
                        }
                        break;
                    default:
                        break;
                }

                if (!isRefreshed)
                {
                    refresh();
                }
       
            };
                                    
            FilterModes.MoveCurrentTo(MediaFilterMode.None);
            
        }

        void mediaFileSortItemCollection_ItemSortDirectionChanged(object sender, EventArgs e)
        {
            MediaFileSortItem item = (MediaFileSortItem)sender;

            if (SortMode == item.SortMode)
            {
                SortDirection = item.SortDirection;
                refresh();
            }
        }

        bool filterFunc(SelectableMediaItem item) 
        {                  
            item.Item.RWLock.EnterReadLock();
            try
            {
                switch (MediaFilter)
                {
                    case MediaFilterMode.None:
                        break;                           
                    case MediaFilterMode.Video:
                        if (!Utils.MediaFormatConvert.isVideoFile(item.Item.Location))
                        {
                            return (false);
                        }
                        break;
                    case MediaFilterMode.Images:
                        if (!Utils.MediaFormatConvert.isImageFile(item.Item.Location))
                        {
                            return (false);
                        }
                        break;
                    default:
                        break;
                }


                bool result = tagFilter(item);

                return (result);
            }
            finally
            {
                item.Item.RWLock.ExitReadLock();
            }            
        }
        
        MediaFilterMode mediaFilter;

        public MediaFilterMode MediaFilter
        {
            get { return mediaFilter; }
            set { SetProperty(ref mediaFilter, value); }
        }

        override protected void MediaState_ItemPropertiesChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            MediaItem item = sender as MediaItem;
                                    
            if (e.PropertyName.Equals("Location"))
            {
                if (SortMode == MediaFileSortMode.Name)
                {
                    reSort(item);
                }
            }
            else if (e.PropertyName.Equals("Metadata"))
            {
                if (item.Metadata != null)
                {
                    reSort(item);
                }
            }
            
        }
              
        private bool mediaStateSortModeCollectionViewFilter(object item)
        {
            MediaFileSortItem sortItem = (MediaFileSortItem)item;

            switch (MediaFilter)
            {
                case MediaFilterMode.None:
                    return (MediaFileSortFunctions.isAllSortMode(sortItem.SortMode));
                case MediaFilterMode.Video:
                    return (MediaFileSortFunctions.isVideoSortMode(sortItem.SortMode));
                case MediaFilterMode.Images:
                    return (MediaFileSortFunctions.isImageSortMode(sortItem.SortMode));
                default:
                    break;
            }

            return (false);
        }

        public override object getExtraInfo(SelectableMediaItem selectableItem)
        {            
            string dateFormat = "MMM d, yyyy";

            MediaItem item = selectableItem.Item;

            Object info = null;

            if (item.Metadata != null)
            {
                VideoMetadata VideoMetadata = item.Metadata is VideoMetadata ? item.Metadata as VideoMetadata : null;
                ImageMetadata ImageMetadata = item.Metadata is ImageMetadata ? item.Metadata as ImageMetadata : null;

                switch (SortMode)
                {
                    case MediaFileSortMode.Name:
                        break;
                    case MediaFileSortMode.Size:
                        info = MediaViewer.Model.Utils.MiscUtils.formatSizeBytes(item.Metadata.SizeBytes);
                        break;
                    case MediaFileSortMode.Rating:

                        if(item.Metadata.Rating.HasValue) {

                            int nrStars = (int)item.Metadata.Rating.Value;

                            info = RatingCache.RatingBitmap[nrStars];                                              
                        }
                        break;
                    case MediaFileSortMode.Imported:
                        break;
                    case MediaFileSortMode.Tags:
                        if (item.Metadata.Tags.Count > 0)
                        {
                            info = item.Metadata.Tags.Count.ToString() + " tag";

                            if (item.Metadata.Tags.Count > 1)
                            {
                                info += "s";
                            }
                        }
                        break;
                    case MediaFileSortMode.MimeType:
                        info = item.Metadata.MimeType;
                        break;
                    case MediaFileSortMode.FileDate:
                        info = item.Metadata.FileDate.ToString(dateFormat);
                        break;  
                    case MediaFileSortMode.LastModified:
                        info = item.Metadata.LastModifiedDate.ToString(dateFormat);                       
                        break;                        
                    case MediaFileSortMode.CreationDate:
                        if (item.Metadata.CreationDate.HasValue)
                        {
                            info = item.Metadata.CreationDate.Value.ToString(dateFormat);
                        }
                        break;
                    case MediaFileSortMode.SoftWare:
                        if (item.Metadata.Software != null)
                        {
                            info = item.Metadata.Software;
                        }
                        break;
                    case MediaFileSortMode.Width:
                        if (ImageMetadata != null)
                        {                          
                            info = ImageMetadata.Width.ToString() + " x " + ImageMetadata.Height.ToString();
                        }
                        else
                        {                         
                            info = VideoMetadata.Width.ToString() + " x " + VideoMetadata.Height.ToString();
                        }
                        break;                        
                    case MediaFileSortMode.Height:
                        if (ImageMetadata != null)
                        {
                            info = ImageMetadata.Width.ToString() + " x " + ImageMetadata.Height.ToString();
                        }
                        else
                        {
                            info = VideoMetadata.Width.ToString() + " x " + VideoMetadata.Height.ToString();
                        }                   
                        break;
                    case MediaFileSortMode.Duration:
                        if (VideoMetadata != null)
                        {
                            info = MiscUtils.formatTimeSeconds(VideoMetadata.DurationSeconds);
                        }                        
                        break;
                    case MediaFileSortMode.FramesPerSecond:
                        if (VideoMetadata != null)
                        {
                            info = VideoMetadata.FramesPerSecond.ToString("0.00") + " FPS";
                        }
                        break;
                    case MediaFileSortMode.VideoCodec:
                        if (VideoMetadata != null)
                        {
                            info = VideoMetadata.VideoCodec;
                        }
                        break;
                    case MediaFileSortMode.AudioCodec:
                        if (VideoMetadata != null)
                        {
                            info = VideoMetadata.AudioCodec;
                        }
                        break;
                    case MediaFileSortMode.PixelFormat:
                        if (VideoMetadata != null)
                        {
                            info = VideoMetadata.PixelFormat;
                        }
                        break;
                    case MediaFileSortMode.BitsPerSample:
                        if (VideoMetadata != null)
                        {
                            info = VideoMetadata.BitsPerSample.HasValue ? VideoMetadata.BitsPerSample + "bit" : "";
                        }
                        break;
                    case MediaFileSortMode.SamplesPerSecond:
                        if (VideoMetadata != null)
                        {
                            info = VideoMetadata.SamplesPerSecond.HasValue ? VideoMetadata.SamplesPerSecond + "hz" : "";
                        }
                        break;
                    case MediaFileSortMode.NrChannels:
                        if (VideoMetadata != null)
                        {
                            info = VideoMetadata.NrChannels.HasValue ? VideoMetadata.NrChannels.Value.ToString() + " chan" : "";
                        }
                        break;
                    case MediaFileSortMode.CameraMake:
                        if (ImageMetadata != null)
                        {
                            info = ImageMetadata.CameraMake != null ? ImageMetadata.CameraMake : "";
                        }
                        break;
                    case MediaFileSortMode.CameraModel:
                        if (ImageMetadata != null)
                        {
                            info = ImageMetadata.CameraModel != null ? ImageMetadata.CameraModel : "";
                        }
                        break;
                    case MediaFileSortMode.Lens:
                        if (ImageMetadata != null)
                        {
                            info = ImageMetadata.Lens != null ? ImageMetadata.Lens : "";
                        }
                        break;
                    case MediaFileSortMode.ISOSpeedRating:
                        if (ImageMetadata != null)
                        {
                            info = ImageMetadata.ISOSpeedRating.HasValue ? "ISO: " + ImageMetadata.ISOSpeedRating.Value : "";
                        }
                        break;
                    case MediaFileSortMode.FNumber:
                        if (ImageMetadata != null)
                        {
                            info = ImageMetadata.FNumber.HasValue ? "f/" + ImageMetadata.FNumber.Value : "";
                        }
                        break;
                    case MediaFileSortMode.ExposureTime:
                        if (ImageMetadata != null)
                        {
                            info = ImageMetadata.ExposureTime.HasValue ? "1/" + 1/ImageMetadata.ExposureTime.Value + "s" : "";
                        }
                        break;
                    case MediaFileSortMode.FocalLength:
                        if (ImageMetadata != null)
                        {
                            info = ImageMetadata.FocalLength.HasValue ? ImageMetadata.FocalLength.Value + "mm" : "";
                        }
                        break;
                    default:
                        break;
                }
            }

            return (info);
        
        }
    }
}
