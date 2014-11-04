using MediaViewer.Model.Media.File;
using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Global.Events
{
    public class MediaBatchSelectionEvent : PubSubEvent<ICollection<MediaFileItem>> { }
    public class MediaSelectionEvent : PubSubEvent<MediaFileItem> { }
   
}
