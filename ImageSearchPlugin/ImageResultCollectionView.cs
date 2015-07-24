using MediaViewer.Model.Media.State;
using MediaViewer.Model.Media.State.CollectionView;
using MediaViewer.Model.Utils;
using MediaViewer.UserControls.SortComboBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearchPlugin
{
    class ImageResultCollectionView : MediaStateCollectionView
    {
        enum SortMode
        {
            Relevance,
            Width,
            Height,
            Size,
            Location,
            MimeType
        }

        class SortItem : SortItemBase<SortMode>
        {
            public SortItem(SortMode mode)
                : base(mode)
            {

            }
        }

        public ImageResultCollectionView(MediaState mediaState)     
            : base(mediaState)
        {
            SortItemCollection<SortItem, SortMode> sortItems = new SortItemCollection<SortItem, SortMode>();

            foreach (SortMode mode in Enum.GetValues(typeof(SortMode)))
            {
                sortItems.Add(new SortItem(mode));
            }

            sortItems.ItemSortDirectionChanged += sortItems_ItemSortDirectionChanged;

            SortModes = new System.Windows.Data.ListCollectionView(sortItems);
           
            SortModes.CurrentChanged += (s,e) => {

                SortItem sortItem = (SortItem)SortModes.CurrentItem;

                switch (sortItem.SortMode)
                {
                    case SortMode.Relevance:
                        SortFunc = new Func<SelectableMediaItem,SelectableMediaItem, int>((a,b) =>
                        {
                            ImageResultItem itemA = (ImageResultItem)a.Item;
                            ImageResultItem itemB = (ImageResultItem)b.Item;
                            
                            return (itemA.Relevance.CompareTo(itemB.Relevance));                            
                        });
                        break;
                    case SortMode.Location:
                        SortFunc = new Func<SelectableMediaItem, SelectableMediaItem, int>((a, b) =>
                        {
                            ImageResultItem itemA = (ImageResultItem)a.Item;
                            ImageResultItem itemB = (ImageResultItem)b.Item;

                            return (itemA.ImageInfo.SourceUrl.CompareTo(itemB.ImageInfo.SourceUrl));
                        });
                        break;
                    case SortMode.Width:
                        SortFunc = new Func<SelectableMediaItem, SelectableMediaItem, int>((a, b) =>
                        {
                            ImageResultItem itemA = (ImageResultItem)a.Item;
                            ImageResultItem itemB = (ImageResultItem)b.Item;

                            return (Nullable.Compare(itemA.ImageInfo.Width,itemB.ImageInfo.Width));
                        });
                        break;
                    case SortMode.Height:
                        SortFunc = new Func<SelectableMediaItem, SelectableMediaItem, int>((a, b) =>
                        {
                            ImageResultItem itemA = (ImageResultItem)a.Item;
                            ImageResultItem itemB = (ImageResultItem)b.Item;

                            return (Nullable.Compare(itemA.ImageInfo.Height, itemB.ImageInfo.Height));
                        });
                        break;
                    case SortMode.Size:
                        SortFunc = new Func<SelectableMediaItem, SelectableMediaItem, int>((a, b) =>
                        {
                            ImageResultItem itemA = (ImageResultItem)a.Item;
                            ImageResultItem itemB = (ImageResultItem)b.Item;

                            return (Nullable.Compare(itemA.ImageInfo.FileSize,itemB.ImageInfo.FileSize));
                        });
                        break;
                    case SortMode.MimeType:
                        SortFunc = new Func<SelectableMediaItem, SelectableMediaItem, int>((a, b) =>
                        {
                            ImageResultItem itemA = (ImageResultItem)a.Item;
                            ImageResultItem itemB = (ImageResultItem)b.Item;

                            return (itemA.ImageInfo.ContentType.CompareTo(itemB.ImageInfo.ContentType));
                        });
                        break;
                    default:
                        break;
                }

                SortDirection = sortItem.SortDirection;

                refresh();
            };
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

        public override object getExtraInfo(SelectableMediaItem selectableItem)
        {
            String info = null;

            ImageResultItem item = (ImageResultItem)selectableItem.Item;

            SortItem sortItem = (SortItem)SortModes.CurrentItem;

            switch (sortItem.SortMode)
            {               
                case SortMode.Width:
                                       
                case SortMode.Height:
                    info = item.ImageInfo.Width + " x " + item.ImageInfo.Height;
                    break;
                case SortMode.Size:
                    info = MiscUtils.formatSizeBytes(item.ImageInfo.FileSize.Value);
                    break;
                case SortMode.MimeType:
                    info = item.ImageInfo.ContentType;
                    break;
                case SortMode.Location:
                    info = item.ImageInfo.SourceUrl;
                    break;
                default:
                    break;
            }

            return (info);
        }
    }
}
