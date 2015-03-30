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
            filter = MediaStateFilterFunctions.getFilter(MediaStateFilterMode.None);
            sortFunc = MediaStateSortFunctions.getSortFunction(MediaStateSortMode.Name);

            FilterModes = new ListCollectionView(Enum.GetValues(typeof(MediaStateFilterMode)));         
            SortModes = new ListCollectionView(Enum.GetValues(typeof(MediaStateSortMode)));

            SortModes.CurrentChanged += (s, e) =>
            {
                MediaStateSortMode sortMode = (MediaStateSortMode)SortModes.CurrentItem;

                SortFunc = MediaStateSortFunctions.getSortFunction(sortMode);        
            };

            SortModes.Filter = mediaStateSortModeCollectionViewFilter;

            FilterModes.CurrentChanged += (s, e) =>
            {
                MediaStateFilterMode filterMode = (MediaStateFilterMode)FilterModes.CurrentItem;
                MediaStateSortMode sortMode = (MediaStateSortMode)SortModes.CurrentItem;

                Filter = MediaStateFilterFunctions.getFilter(filterMode);

                SortModes.Refresh();

                switch (filterMode)
                {
                    case MediaStateFilterMode.None:
                        if (!MediaStateSortFunctions.isAllSortMode(sortMode))
                        {
                            SortModes.MoveCurrentToFirst();
                        }
                        break;
                    case MediaStateFilterMode.Video:
                        if (!MediaStateSortFunctions.isVideoSortMode(sortMode))
                        {
                            SortModes.MoveCurrentToFirst();
                        }
                        break;
                    case MediaStateFilterMode.Images:
                        if (!MediaStateSortFunctions.isImageSortMode(sortMode))
                        {
                            SortModes.MoveCurrentToFirst();
                        }
                        break;
                    default:
                        break;
                }

       
            };
                                    
            FilterModes.MoveCurrentTo(MediaStateFilterMode.None);
            
        }

        override protected void MediaState_ItemPropertiesChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            MediaItem item = sender as MediaItem;
            MediaStateSortMode sortMode = (MediaStateSortMode)SortModes.CurrentItem;

            if (e.PropertyName.Equals("Location"))
            {
                if (sortMode == MediaStateSortMode.Name)
                {
                    reSort(item);
                }
            }
            else if (e.PropertyName.Equals("Metadata"))
            {
                if (item.Metadata != null && sortMode != MediaStateSortMode.Name)
                {
                    reSort(item);
                }
            }
            
        }

      
        private bool mediaStateSortModeCollectionViewFilter(object item)
        {
            MediaStateSortMode mode = (MediaStateSortMode)item;

            switch ((MediaStateFilterMode)FilterModes.CurrentItem)
            {
                case MediaStateFilterMode.None:
                    return (MediaStateSortFunctions.isAllSortMode(mode));
                case MediaStateFilterMode.Video:
                    return (MediaStateSortFunctions.isVideoSortMode(mode));
                case MediaStateFilterMode.Images:
                    return (MediaStateSortFunctions.isImageSortMode(mode));
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

                switch ((MediaStateSortMode)SortModes.CurrentItem)
                {
                    case MediaStateSortMode.Name:
                        break;
                    case MediaStateSortMode.Size:
                        info = MediaViewer.Model.Utils.MiscUtils.formatSizeBytes(item.Metadata.SizeBytes);
                        break;
                    case MediaStateSortMode.Rating:

                        if(item.Metadata.Rating.HasValue) {

                            int nrStars = (int)item.Metadata.Rating.Value;

                            info = RatingCache.RatingBitmap[nrStars];                                              
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

            return (info);
        
        }
    }
}
