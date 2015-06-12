using MediaViewer.Model.Media.State;
using MediaViewer.Model.Media.State.CollectionView;
using MediaViewer.Model.Utils;
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
            Title,
            Width,
            Height,
            Size,
            Location,
            MimeType
        }


        public ImageResultCollectionView(MediaState mediaState)     
            : base(mediaState)
        {
            SortModes = new System.Windows.Data.ListCollectionView(Enum.GetValues(typeof(SortMode)));

            SortModes.CurrentChanged += (s,e) => {

                switch ((SortMode)SortModes.CurrentItem)
                {
                    case SortMode.Title:
                        SortFunc = new Func<SelectableMediaItem,SelectableMediaItem, int>((a,b) =>
                        {
                            ImageResultItem itemA = (ImageResultItem)a.Item;
                            ImageResultItem itemB = (ImageResultItem)b.Item;
                            
                            return (itemA.ImageInfo.Title.CompareTo(itemB.ImageInfo.Title));                            
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

                refresh();
            };
        }

        public override object getExtraInfo(SelectableMediaItem selectableItem)
        {
            String info = null;

            ImageResultItem item = (ImageResultItem)selectableItem.Item;
            

            switch ((SortMode)SortModes.CurrentItem)
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
