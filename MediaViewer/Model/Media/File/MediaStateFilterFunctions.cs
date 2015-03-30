using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.State.CollectionView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.File
{
    public enum MediaStateFilterMode
    {
        None,
        Video,
        Images
    }

    class MediaStateFilterFunctions
    {
        public static Func<SelectableMediaItem, bool> getFilter(MediaStateFilterMode mode)
        {
            Func<SelectableMediaItem, bool> filter = null;

            switch (mode)
            {
                case MediaStateFilterMode.None:
                    filter = new Func<SelectableMediaItem, bool>((item) =>
                    {
                        return (true);
                    });
                    break;
                case MediaStateFilterMode.Video:
                    filter = new Func<SelectableMediaItem, bool>((item) =>
                    {
                        return (Utils.MediaFormatConvert.isVideoFile(item.Item.Location));                        
                    });
                    break;
                case MediaStateFilterMode.Images:
                    filter = new Func<SelectableMediaItem, bool>((item) =>
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
