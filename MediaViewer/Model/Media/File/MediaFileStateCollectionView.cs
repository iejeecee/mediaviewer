using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Media.State;
using MediaViewer.Model.Media.State.CollectionView;
using MediaViewer.Model.Utils;
using MediaViewer.UserControls.MediaGridItem;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

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

        static MediaFileStateCollectionView()
        {
            RatingCache = new RatingCache();
        }

        public MediaFileStateCollectionView(MediaState mediaState)
            : base(mediaState)
        {           
            Filter = filterFunc;

            MediaFilter = MediaFilterMode.None;
            TagFilter = new List<Tag>();

            SortFunc = MediaFileSortFunctions.getSortFunction(MediaSortMode.Name);
            
            FilterModes = new ListCollectionView(Enum.GetValues(typeof(MediaFilterMode)));         
            SortModes = new ListCollectionView(Enum.GetValues(typeof(MediaSortMode)));

            SortModes.CurrentChanged += (s, e) =>
            {
                MediaSortMode sortMode = (MediaSortMode)SortModes.CurrentItem;

                SortFunc = MediaFileSortFunctions.getSortFunction(sortMode);

                refresh();
            };

            SortModes.Filter = mediaStateSortModeCollectionViewFilter;

            FilterModes.CurrentChanged += (s, e) =>
            {
                MediaFilter = (MediaFilterMode)FilterModes.CurrentItem;
                MediaSortMode sortMode = (MediaSortMode)SortModes.CurrentItem;                

                SortModes.Refresh();

                bool isRefreshed = false;

                switch (MediaFilter)
                {
                    case MediaFilterMode.None:
                        if (!MediaFileSortFunctions.isAllSortMode(sortMode))
                        {
                            SortModes.MoveCurrentToFirst();
                            isRefreshed = true;
                        }
                        break;
                    case MediaFilterMode.Video:
                        if (!MediaFileSortFunctions.isVideoSortMode(sortMode))
                        {
                            SortModes.MoveCurrentToFirst();
                            isRefreshed = true;
                        }
                        break;
                    case MediaFilterMode.Images:
                        if (!MediaFileSortFunctions.isImageSortMode(sortMode))
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


                // tag filter
                MediaItem media = item.Item;

                if (TagFilter.Count == 0) return (true);
                if (media.ItemState == Base.MediaItemState.LOADING) return (true);

                foreach (Tag tag in TagFilter)
                {
                    if (!media.Metadata.Tags.Contains(tag))
                    {
                        return (false);
                    }
                }

                return (true);
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

        List<Tag> tagFilter;

        public List<Tag> TagFilter
        {
            get { return tagFilter; }
            set { tagFilter = value; }
        }

        override protected void MediaState_ItemPropertiesChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            MediaItem item = sender as MediaItem;
            MediaSortMode sortMode = (MediaSortMode)SortModes.CurrentItem;

            if (e.PropertyName.Equals("Location"))
            {
                if (sortMode == MediaSortMode.Name)
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
            MediaSortMode mode = (MediaSortMode)item;

            switch (MediaFilter)
            {
                case MediaFilterMode.None:
                    return (MediaFileSortFunctions.isAllSortMode(mode));
                case MediaFilterMode.Video:
                    return (MediaFileSortFunctions.isVideoSortMode(mode));
                case MediaFilterMode.Images:
                    return (MediaFileSortFunctions.isImageSortMode(mode));
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

                switch ((MediaSortMode)SortModes.CurrentItem)
                {
                    case MediaSortMode.Name:
                        break;
                    case MediaSortMode.Size:
                        info = MediaViewer.Model.Utils.MiscUtils.formatSizeBytes(item.Metadata.SizeBytes);
                        break;
                    case MediaSortMode.Rating:

                        if(item.Metadata.Rating.HasValue) {

                            int nrStars = (int)item.Metadata.Rating.Value;

                            info = RatingCache.RatingBitmap[nrStars];                                              
                        }
                        break;
                    case MediaSortMode.Imported:
                        break;
                    case MediaSortMode.Tags:
                        if (item.Metadata.Tags.Count > 0)
                        {
                            info = item.Metadata.Tags.Count.ToString() + " tag";

                            if (item.Metadata.Tags.Count > 1)
                            {
                                info += "s";
                            }
                        }
                        break;
                    case MediaSortMode.MimeType:
                        info = item.Metadata.MimeType;
                        break;
                    case MediaSortMode.FileDate:
                        info = item.Metadata.FileDate.ToString(dateFormat);
                        break;  
                    case MediaSortMode.LastModified:
                        info = item.Metadata.LastModifiedDate.ToString(dateFormat);                       
                        break;                        
                    case MediaSortMode.CreationDate:
                        if (item.Metadata.CreationDate.HasValue)
                        {
                            info = item.Metadata.CreationDate.Value.ToString(dateFormat);
                        }
                        break;
                    case MediaSortMode.SoftWare:
                        if (item.Metadata.Software != null)
                        {
                            info = item.Metadata.Software;
                        }
                        break;
                    case MediaSortMode.Width:
                        if (ImageMetadata != null)
                        {                          
                            info = ImageMetadata.Width.ToString() + " x " + ImageMetadata.Height.ToString();
                        }
                        else
                        {                         
                            info = VideoMetadata.Width.ToString() + " x " + VideoMetadata.Height.ToString();
                        }
                        break;                        
                    case MediaSortMode.Height:
                        if (ImageMetadata != null)
                        {
                            info = ImageMetadata.Width.ToString() + " x " + ImageMetadata.Height.ToString();
                        }
                        else
                        {
                            info = VideoMetadata.Width.ToString() + " x " + VideoMetadata.Height.ToString();
                        }                   
                        break;
                    case MediaSortMode.Duration:
                        if (VideoMetadata != null)
                        {
                            info = MiscUtils.formatTimeSeconds(VideoMetadata.DurationSeconds);
                        }                        
                        break;
                    case MediaSortMode.FramesPerSecond:
                        if (VideoMetadata != null)
                        {
                            info = VideoMetadata.FramesPerSecond.ToString("0.00") + " FPS";
                        }
                        break;
                    case MediaSortMode.VideoCodec:
                        if (VideoMetadata != null)
                        {
                            info = VideoMetadata.VideoCodec;
                        }
                        break;
                    case MediaSortMode.AudioCodec:
                        if (VideoMetadata != null)
                        {
                            info = VideoMetadata.AudioCodec;
                        }
                        break;
                    case MediaSortMode.PixelFormat:
                        if (VideoMetadata != null)
                        {
                            info = VideoMetadata.PixelFormat;
                        }
                        break;
                    case MediaSortMode.BitsPerSample:
                        if (VideoMetadata != null)
                        {
                            info = VideoMetadata.BitsPerSample.HasValue ? VideoMetadata.BitsPerSample + "bit" : "";
                        }
                        break;
                    case MediaSortMode.SamplesPerSecond:
                        if (VideoMetadata != null)
                        {
                            info = VideoMetadata.SamplesPerSecond.HasValue ? VideoMetadata.SamplesPerSecond + "hz" : "";
                        }
                        break;
                    case MediaSortMode.NrChannels:
                        if (VideoMetadata != null)
                        {
                            info = VideoMetadata.NrChannels.HasValue ? VideoMetadata.NrChannels.Value.ToString() + " chan" : "";
                        }
                        break;
                    case MediaSortMode.CameraMake:
                        if (ImageMetadata != null)
                        {
                            info = ImageMetadata.CameraMake != null ? ImageMetadata.CameraMake : "";
                        }
                        break;
                    case MediaSortMode.CameraModel:
                        if (ImageMetadata != null)
                        {
                            info = ImageMetadata.CameraModel != null ? ImageMetadata.CameraModel : "";
                        }
                        break;
                    case MediaSortMode.Lens:
                        if (ImageMetadata != null)
                        {
                            info = ImageMetadata.Lens != null ? ImageMetadata.Lens : "";
                        }
                        break;
                    case MediaSortMode.ISOSpeedRating:
                        if (ImageMetadata != null)
                        {
                            info = ImageMetadata.ISOSpeedRating.HasValue ? "ISO: " + ImageMetadata.ISOSpeedRating.Value : "";
                        }
                        break;
                    case MediaSortMode.FNumber:
                        if (ImageMetadata != null)
                        {
                            info = ImageMetadata.FNumber.HasValue ? "f/" + ImageMetadata.FNumber.Value : "";
                        }
                        break;
                    case MediaSortMode.ExposureTime:
                        if (ImageMetadata != null)
                        {
                            info = ImageMetadata.ExposureTime.HasValue ? "1/" + 1/ImageMetadata.ExposureTime.Value + "s" : "";
                        }
                        break;
                    case MediaSortMode.FocalLength:
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
