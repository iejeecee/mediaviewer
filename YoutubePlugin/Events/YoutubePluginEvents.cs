using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubePlugin.Item;
using YoutubePlugin.YoutubeChannelBrowser;
using YoutubePlugin.YoutubeSearch;

namespace YoutubePlugin.Events
{
    class SelectionEvent : PubSubEvent<ICollection<YoutubeItem>> { }
    class SearchEvent : PubSubEvent<YoutubeSearchQuery> { }
    class AddFavoriteChannelEvent : PubSubEvent<YoutubeChannelItem> { }
}
