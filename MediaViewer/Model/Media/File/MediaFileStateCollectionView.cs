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
        Images,
        Audio
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
            icons.Add(new BitmapImage(new Uri(iconPath + "readonly.ico", UriKind.Absolute)));

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
                    case MediaFilterMode.Audio:
                        if (!MediaFileSortFunctions.isAudioSortMode(SortMode))
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
            item.Item.EnterReadLock();
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
                    case MediaFilterMode.Audio:
                        if (!Utils.MediaFormatConvert.isAudioFile(item.Item.Location))
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
                item.Item.ExitReadLock();
            }            
        }
        
        MediaFilterMode mediaFilter;

        public MediaFilterMode MediaFilter
        {
            get { return mediaFilter; }
            set { SetProperty(ref mediaFilter, value); }
        }

        override protected void MediaState_ItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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
                case MediaFilterMode.Audio:
                    return (MediaFileSortFunctions.isAudioSortMode(sortItem.SortMode));
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
                VideoMetadata videoMetadata = item.Metadata is VideoMetadata ? item.Metadata as VideoMetadata : null;
                ImageMetadata imageMetadata = item.Metadata is ImageMetadata ? item.Metadata as ImageMetadata : null;
                AudioMetadata audioMetadata = item.Metadata is AudioMetadata ? item.Metadata as AudioMetadata : null;

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
                        if (imageMetadata != null)
                        {                          
                            info = imageMetadata.Width.ToString() + " x " + imageMetadata.Height.ToString();
                        }
                        else
                        {                         
                            info = videoMetadata.Width.ToString() + " x " + videoMetadata.Height.ToString();
                        }
                        break;                        
                    case MediaFileSortMode.Height:
                        if (imageMetadata != null)
                        {
                            info = imageMetadata.Width.ToString() + " x " + imageMetadata.Height.ToString();
                        }
                        else
                        {
                            info = videoMetadata.Width.ToString() + " x " + videoMetadata.Height.ToString();
                        }                   
                        break;
                    case MediaFileSortMode.Duration:
                        
                        if (videoMetadata != null)
                        {
                            info = MiscUtils.formatTimeSeconds(videoMetadata.DurationSeconds);
                        }
                        else if (audioMetadata != null)
                        {
                            info = MiscUtils.formatTimeSeconds(audioMetadata.DurationSeconds);
                        }
                        break;
                    case MediaFileSortMode.FramesPerSecond:
                        if (videoMetadata != null)
                        {
                            info = videoMetadata.FramesPerSecond.ToString("0.00") + " FPS";
                        }
                        break;
                    case MediaFileSortMode.VideoCodec:
                        if (videoMetadata != null)
                        {
                            info = videoMetadata.VideoCodec;
                        }
                        break;
                    case MediaFileSortMode.AudioCodec:
                        if (videoMetadata != null)
                        {
                            info = videoMetadata.AudioCodec;
                        }
                        else if (audioMetadata != null)
                        {
                            info = audioMetadata.AudioCodec;
                        }
                        break;
                    case MediaFileSortMode.PixelFormat:
                        if (videoMetadata != null)
                        {
                            info = videoMetadata.PixelFormat;
                        }
                        break;
                    case MediaFileSortMode.BitsPerSample:
                        if (videoMetadata != null)
                        {
                            info = videoMetadata.BitsPerSample.HasValue ? videoMetadata.BitsPerSample + "bit" : "";
                        }
                        else if (audioMetadata != null)
                        {
                            info = audioMetadata.BitsPerSample + "bit";
                        }
                        break;
                    case MediaFileSortMode.SamplesPerSecond:
                        if (videoMetadata != null)
                        {
                            info = videoMetadata.SamplesPerSecond.HasValue ? videoMetadata.SamplesPerSecond + "hz" : "";
                        }
                        else if (audioMetadata != null)
                        {
                            info = audioMetadata.SamplesPerSecond + "hz";
                        }
                        break;
                    case MediaFileSortMode.NrChannels:
                        if (videoMetadata != null)
                        {
                            info = videoMetadata.NrChannels.HasValue ? videoMetadata.NrChannels.Value.ToString() + " chan" : "";
                        }
                        else if (audioMetadata != null)
                        {
                            info = audioMetadata.NrChannels.ToString();
                        }
                        break;
                    case MediaFileSortMode.CameraMake:
                        if (imageMetadata != null)
                        {
                            info = imageMetadata.CameraMake != null ? imageMetadata.CameraMake : "";
                        }
                        break;
                    case MediaFileSortMode.CameraModel:
                        if (imageMetadata != null)
                        {
                            info = imageMetadata.CameraModel != null ? imageMetadata.CameraModel : "";
                        }
                        break;
                    case MediaFileSortMode.Lens:
                        if (imageMetadata != null)
                        {
                            info = imageMetadata.Lens != null ? imageMetadata.Lens : "";
                        }
                        break;
                    case MediaFileSortMode.ISOSpeedRating:
                        if (imageMetadata != null)
                        {
                            info = imageMetadata.ISOSpeedRating.HasValue ? "ISO: " + imageMetadata.ISOSpeedRating.Value : "";
                        }
                        break;
                    case MediaFileSortMode.FNumber:
                        if (imageMetadata != null)
                        {
                            info = imageMetadata.FNumber.HasValue ? "f/" + imageMetadata.FNumber.Value : "";
                        }
                        break;
                    case MediaFileSortMode.ExposureTime:
                        if (imageMetadata != null)
                        {
                            info = imageMetadata.ExposureTime.HasValue ? "1/" + 1/imageMetadata.ExposureTime.Value + "s" : "";
                        }
                        break;
                    case MediaFileSortMode.FocalLength:
                        if (imageMetadata != null)
                        {
                            info = imageMetadata.FocalLength.HasValue ? imageMetadata.FocalLength.Value + "mm" : "";
                        }
                        break;
                    case MediaFileSortMode.Genre:
                        if (audioMetadata != null)
                        {
                            info = audioMetadata.Genre == null ? "" : audioMetadata.Genre;
                        }
                        break;
                    case MediaFileSortMode.Album:
                        if (audioMetadata != null)
                        {
                            info = audioMetadata.Album == null ? "" : audioMetadata.Album;
                        }
                        break;
                    case MediaFileSortMode.Track:
                        if (audioMetadata != null)
                        {
                            info = audioMetadata.TrackNr == null ? "" : audioMetadata.TrackNr.Value.ToString();
                        }
                        break;
                    case MediaFileSortMode.TotalTracks:
                        if (audioMetadata != null)
                        {
                            info = audioMetadata.TotalTracks == null ? "" : audioMetadata.TotalTracks.Value.ToString();
                        }
                        break;
                    case MediaFileSortMode.Disc:
                        if (audioMetadata != null)
                        {
                            info = audioMetadata.DiscNr == null ? "" : audioMetadata.TrackNr.Value.ToString();
                        }
                        break;
                    case MediaFileSortMode.TotalDiscs:
                        if (audioMetadata != null)
                        {
                            info = audioMetadata.TotalDiscs == null ? "" : audioMetadata.TotalTracks.Value.ToString();
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
