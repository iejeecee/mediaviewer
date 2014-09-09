using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.State.CollectionView;
using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.GlobalEvents
{

    public class MediaBrowserSelectedEvent : PubSubEvent<MediaFileItem> { }
    public class MediaBrowserDisplayEvent : PubSubEvent<MediaBrowserDisplayOptions> { }

    public class MediaBrowserDisplayOptions
    {
        public MediaBrowserDisplayOptions()
        {
            isHidden = false;
            filterMode = MediaStateFilterMode.All;
        }

        bool isHidden;

        public bool IsHidden
        {
            get { return isHidden; }
            set { isHidden = value; }
        }

        MediaStateFilterMode filterMode;

        public MediaStateFilterMode FilterMode
        {
            get { return filterMode; }
            set { filterMode = value; }
        }
        

    }
}
