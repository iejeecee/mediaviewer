using MediaViewer.Model.Media.Base.State;
using MediaViewer.Model.Media.Base.State.CollectionView;
using MediaViewer.Model.Utils;
using MediaViewer.UserControls.MediaGridItem;
using MediaViewer.UserControls.SortComboBox;
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
    enum FilterMode
    {
        None,
        Videos,
        Channels,
        Playlists
    }

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

    class SortItem : SortItemBase<SortMode>
    {
        public SortItem(SortMode mode) :
            base(mode)
        {
        }
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

            icons.Add(new BitmapImage(new Uri(iconPath + "hd.png", UriKind.Absolute)));
            icons.Add(new BitmapImage(new Uri(iconPath + "notsupported.ico", UriKind.Absolute)));
            icons.Add(new BitmapImage(new Uri(iconPath + "channel.ico", UriKind.Absolute)));
            icons.Add(new BitmapImage(new Uri(iconPath + "playlist.ico", UriKind.Absolute)));
            icons.Add(new BitmapImage(new Uri(iconPath + "4k.png", UriKind.Absolute)));

            InfoIconsCacheStatic = new YoutubeItemInfoIconsCache(icons);
        }

        public YoutubeCollectionView(MediaState mediaState) :
            base(mediaState)
        {
            Filter = filterFunc;

            InfoIconsCache = InfoIconsCacheStatic;

            FilterModes = new System.Windows.Data.ListCollectionView(Enum.GetValues(typeof(FilterMode)));

            FilterModes.CurrentChanged += (s, e) =>
                {
                    refresh();
                };
           
            SortItemCollection<SortItem, SortMode> sortItems = new SortItemCollection<SortItem,SortMode>();

            foreach (SortMode mode in Enum.GetValues(typeof(SortMode)))
            {
                sortItems.Add(new SortItem(mode));
            }

            sortItems.ItemSortDirectionChanged += sortItems_ItemSortDirectionChanged;

            SortModes = new System.Windows.Data.ListCollectionView(sortItems);

            SortModes.CurrentChanged += (s, e) =>
            {
                SortItem item = (SortItem)SortModes.CurrentItem;

                switch (item.SortMode)
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

                SortDirection = item.SortDirection;

                refresh();
            };
        }

        bool filterFunc(SelectableMediaItem selectableItem)
        {
            YoutubeItem item = (YoutubeItem)selectableItem.Item;

            switch ((FilterMode)FilterModes.CurrentItem)
            {
                case FilterMode.None:
                    break;
                case FilterMode.Videos:
                    if (!(item is YoutubeVideoItem))
                    {
                        return (false);
                    }
                    break;
                case FilterMode.Channels:
                    if (!(item is YoutubeChannelItem))
                    {
                        return (false);
                    }
                    break;
                case FilterMode.Playlists:
                    if (!(item is YoutubePlaylistItem))
                    {
                        return (false);
                    }
                    break;
                default:
                    break;
            }

            bool result = tagFilter(selectableItem);

            return (result);            
        }

        void sortItems_ItemSortDirectionChanged(object sender, EventArgs e)
        {
            SortItem sortItem = (SortItem)sender;

            if (((SortItem)SortModes.CurrentItem).SortMode == sortItem.SortMode)
            {
                SortDirection = sortItem.SortDirection;

                refresh();
            }
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
            //string dateFormat = "MMM d, yyyy";

            YoutubeItemMetadata metadata = (YoutubeItemMetadata)selectableItem.Item.Metadata;

            if(metadata == null) return(info);

            SortItem item = (SortItem)SortModes.CurrentItem;

            switch (item.SortMode)
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
                    info = MiscUtils.formatTimeAgo(metadata.CreationDate.Value) + " ago";
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
                        return RatingCache.RatingBitmap[(int)(metadata.Rating.Value + 0.5)];
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
