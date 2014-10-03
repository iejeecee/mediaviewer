using MediaViewer.Model.Media.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.State.CollectionView
{
    public enum MediaStateFilterMode
    {
        All,
        Video,
        Images
    }

    class MediaStateFilterFunctions
    {
        public static Func<SelectableMediaFileItem, bool> getFilter(MediaStateFilterMode mode)
        {
            Func<SelectableMediaFileItem, bool> filter = null;

            switch (mode)
            {
                case MediaStateFilterMode.All:
                    filter = new Func<SelectableMediaFileItem, bool>((item) =>
                    {
                        return (true);
                    });
                    break;
                case MediaStateFilterMode.Video:
                    filter = new Func<SelectableMediaFileItem, bool>((item) =>
                    {
                        return (Utils.MediaFormatConvert.isVideoFile(item.Item.Location));                        
                    });
                    break;
                case MediaStateFilterMode.Images:
                    filter = new Func<SelectableMediaFileItem, bool>((item) =>
                    {
                        return (Utils.MediaFormatConvert.isImageFile(item.Item.Location)); 
                    });
                    break;
                default:
                    break;
            }

            return (filter);
        }
    }
}
