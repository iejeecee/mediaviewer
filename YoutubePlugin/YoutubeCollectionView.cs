using MediaViewer.Model.Media.State;
using MediaViewer.Model.Media.State.CollectionView;
using MediaViewer.Model.Utils;
using MediaViewer.UserControls.MediaGridItem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using YoutubePlugin.Item;

namespace YoutubePlugin
{
    enum SortMode
    {
        Relevance,
        PublishedAt,
        Rating,
        ViewCount,
        Duration,
        Author,
        Width,
        Height,               
        MimeType,
        FramesPerSecond
    }

    class YoutubeCollectionView : MediaStateCollectionView        
    {
        static RatingCache RatingCache { get; set; }
        static InfoIconsCache InfoIconsCacheStatic { get; set; }

        static YoutubeCollectionView()
        {
            RatingCache = new RatingCache();           

            List<BitmapImage> icons = new List<BitmapImage>();

            String iconPath = "pack://application:,,,/YoutubePlugin;component/Resources/Icons/";

            icons.Add(new BitmapImage(new Uri(iconPath + "hd.ico", UriKind.Absolute)));
            icons.Add(new BitmapImage(new Uri(iconPath + "notsupported.ico", UriKind.Absolute)));
            icons.Add(new BitmapImage(new Uri(iconPath + "channel.ico", UriKind.Absolute)));

            InfoIconsCacheStatic = new YoutubeItemInfoIconsCache(icons);
        }

        public YoutubeCollectionView(MediaState mediaState) :
            base(mediaState)
        {
            Filter = tagFilter;

            InfoIconsCache = InfoIconsCacheStatic;

            SortModes = new System.Windows.Data.ListCollectionView(Enum.GetValues(typeof(SortMode)));

            SortModes.CurrentChanged += (s, e) =>
            {
                switch ((SortMode)SortModes.CurrentItem)
                {
                    case SortMode.Relevance:
                        SortFunc = new Func<SelectableMediaItem, SelectableMediaItem, int>((a, b) =>
                        {
                            YoutubeItem itemA = (YoutubeItem)a.Item;
                            YoutubeItem itemB = (YoutubeItem)b.Item;

                            return (itemA.Relevance.CompareTo(itemB.Relevance));
                        });
                        break;               
                    case SortMode.Width:
                        SortFunc = new Func<SelectableMediaItem, SelectableMediaItem, int>((a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            YoutubeItemMetadata metaA = (YoutubeItemMetadata)a.Item.Metadata;
                            YoutubeItemMetadata metaB = (YoutubeItemMetadata)b.Item.Metadata;

                            return (Nullable.Compare<int>(metaA.Width,metaB.Width));
                        });
                        break;
                    case SortMode.Height:
                        SortFunc = new Func<SelectableMediaItem, SelectableMediaItem, int>((a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            YoutubeItemMetadata metaA = (YoutubeItemMetadata)a.Item.Metadata;
                            YoutubeItemMetadata metaB = (YoutubeItemMetadata)b.Item.Metadata;

                            return (Nullable.Compare<int>(metaA.Height, metaB.Height));
                        });
                        break;
                    case SortMode.Duration:
                        SortFunc = new Func<SelectableMediaItem, SelectableMediaItem, int>((a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            YoutubeItemMetadata metaA = (YoutubeItemMetadata)a.Item.Metadata;
                            YoutubeItemMetadata metaB = (YoutubeItemMetadata)b.Item.Metadata;

                            return (Nullable.Compare<long>(metaA.DurationSeconds, metaB.DurationSeconds));
                        });
                        break;
                    case SortMode.MimeType:
                        SortFunc = new Func<SelectableMediaItem, SelectableMediaItem, int>((a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            YoutubeItemMetadata metaA = (YoutubeItemMetadata)a.Item.Metadata;
                            YoutubeItemMetadata metaB = (YoutubeItemMetadata)b.Item.Metadata;

                            return (String.Compare(metaA.MimeType,metaB.MimeType));
                        });
                        break;
                    case SortMode.Author:
                        SortFunc = new Func<SelectableMediaItem, SelectableMediaItem, int>((a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            YoutubeItemMetadata metaA = (YoutubeItemMetadata)a.Item.Metadata;
                            YoutubeItemMetadata metaB = (YoutubeItemMetadata)b.Item.Metadata;

                            return (String.Compare(metaA.Author, metaB.Author));
                        });
                        break;
                    case SortMode.PublishedAt:
                        SortFunc = new Func<SelectableMediaItem, SelectableMediaItem, int>((a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            YoutubeItemMetadata metaA = (YoutubeItemMetadata)a.Item.Metadata;
                            YoutubeItemMetadata metaB = (YoutubeItemMetadata)b.Item.Metadata;

                            return (Nullable.Compare<DateTime>(metaA.CreationDate,metaB.CreationDate));
                        });
                        break;
                    case SortMode.ViewCount:
                        SortFunc = new Func<SelectableMediaItem, SelectableMediaItem, int>((a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            YoutubeItemMetadata metaA = (YoutubeItemMetadata)a.Item.Metadata;
                            YoutubeItemMetadata metaB = (YoutubeItemMetadata)b.Item.Metadata;

                            return (Nullable.Compare<long>(metaA.ViewCount, metaB.ViewCount));
                        });
                        break;
                    case SortMode.Rating:
                        SortFunc = new Func<SelectableMediaItem, SelectableMediaItem, int>((a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            YoutubeItemMetadata metaA = (YoutubeItemMetadata)a.Item.Metadata;
                            YoutubeItemMetadata metaB = (YoutubeItemMetadata)b.Item.Metadata;

                            return (Nullable.Compare<double>(metaA.Rating, metaB.Rating));
                        });
                        break;
                    case SortMode.FramesPerSecond:
                        SortFunc = new Func<SelectableMediaItem, SelectableMediaItem, int>((a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            YoutubeItemMetadata metaA = (YoutubeItemMetadata)a.Item.Metadata;
                            YoutubeItemMetadata metaB = (YoutubeItemMetadata)b.Item.Metadata;

                            return (Nullable.Compare<double>(metaA.Rating, metaB.Rating));
                        });
                        break;
                    default:
                        break;
                }

                refresh();
            };
        }

        static int hasMediaTest(SelectableMediaItem a, SelectableMediaItem b)
        {
            if (a.Item.Metadata != null && b.Item.Metadata != null) return 0;
            if (a.Item.Metadata == null) return 1;
            else return -1;
        }

        public override object getExtraInfo(SelectableMediaItem selectableItem)
        {
            String info = null;
            string dateFormat = "MMM d, yyyy";

            YoutubeItemMetadata metadata = (YoutubeItemMetadata)selectableItem.Item.Metadata;

            if(metadata == null) return(info);
                        
            switch ((SortMode)SortModes.CurrentItem)
            {               
                case SortMode.Width:
                                       
                case SortMode.Height:
                    if (metadata.Width.HasValue && metadata.Height.HasValue)
                    {
                        info = metadata.Width + " x " + metadata.Height;
                    }
                    break;
               
                case SortMode.MimeType:
                    info = metadata.MimeType;
                    break;
                case SortMode.Duration:
                    if (metadata.DurationSeconds != null)
                    {
                        info = MiscUtils.formatTimeSeconds(metadata.DurationSeconds.Value);
                    }
                    break;
                case SortMode.Author:
                    info = metadata.Author;
                    break;
                case SortMode.PublishedAt:
                    info = metadata.CreationDate.Value.ToString(dateFormat);
                    break;
                case SortMode.ViewCount:
                    if (metadata.ViewCount != null)
                    {
                        info = metadata.ViewCount.Value.ToString("#,##0", new CultureInfo("en-US"));
                    }
                    break;
                case SortMode.Rating:
                    if (metadata.Rating != null)
                    {
                        return RatingCache.RatingBitmap[(int)metadata.Rating.Value];
                    }
                    break;
                case SortMode.FramesPerSecond:
                    if (metadata.FramesPerSecond != null)
                    {
                        return (metadata.FramesPerSecond.Value.ToString() + "fps");
                    }
                    break;
                default:
                    break;
            }

            return (info);
        }        

    }
}
