using MediaViewer.MediaDatabase;
using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.TagEditor
{
    class TagCreatedEvent : PubSubEvent<Tag> { }
    class TagDeletedEvent : PubSubEvent<Tag> { }
    class TagUpdatedEvent : PubSubEvent<Tag> { }

    class TagCategoryCreatedEvent : PubSubEvent<TagCategory> { }
    class TagCategoryUpdatedEvent : PubSubEvent<TagCategory> { }
    class TagCategoryDeletedEvent : PubSubEvent<TagCategory> { }
    
    
}
