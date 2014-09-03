using MediaViewer.MediaFileModel.Watcher;
using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.GlobalEvents
{

    public class MediaBrowserSelectedEvent : PubSubEvent<MediaFileItem> { }
    public class MediaBrowserDisplayEvent : PubSubEvent<MediaBrowserDisplayOptions> { }

    public class MediaBrowserDisplayOptions
    {
        public MediaBrowserDisplayOptions()
        {
            isHidden = false;
            filterMode = ImageGrid.FilterMode.All;
        }

        bool isHidden;

        public bool IsHidden
        {
            get { return isHidden; }
            set { isHidden = value; }
        }

        ImageGrid.FilterMode filterMode;

        public ImageGrid.FilterMode FilterMode
        {
            get { return filterMode; }
            set { filterMode = value; }
        }
        

    }
}
