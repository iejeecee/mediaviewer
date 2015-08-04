using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.File;
using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Global.Events
{
    public class MediaSelectionPayload
    {
        public MediaSelectionPayload(Guid senderId, MediaItem item)
        {
            SenderId = senderId;
            Items = new MediaItem[]{item};
        }

        public MediaSelectionPayload(Guid senderId, ICollection<MediaItem> items)
        {
            SenderId = senderId;
            Items = items;
        }

        public Guid SenderId { get; private set; }
        public ICollection<MediaItem> Items { get; private set; }

    }


    public class MediaSelectionEvent : PubSubEvent<MediaSelectionPayload> { }
      
}
