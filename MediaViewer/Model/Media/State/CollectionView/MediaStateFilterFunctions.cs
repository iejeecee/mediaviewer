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
        public static Func<MediaFileItem, bool> getFilter(MediaStateFilterMode mode)
        {
            Func<MediaFileItem, bool> filter = null;

            switch (mode)
            {
                case MediaStateFilterMode.All:
                    filter = new Func<MediaFileItem, bool>((item) =>
                    {
                        return (true);
                    });
                    break;
                case MediaStateFilterMode.Video:
                    filter = new Func<MediaFileItem, bool>((item) =>
                    {
                        return (Utils.MediaFormatConvert.isVideoFile(item.Location));                        
                    });
                    break;
                case MediaStateFilterMode.Images:
                    filter = new Func<MediaFileItem, bool>((item) =>
                    {
                        return (Utils.MediaFormatConvert.isImageFile(item.Location)); 
                    });
                    break;
                default:
                    break;
            }

            return (filter);
        }
    }
}
