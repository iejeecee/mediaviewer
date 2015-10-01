using Google.Apis.YouTube.v3.Data;
using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubePlugin.Events;
using YoutubePlugin.Properties;

namespace YoutubePlugin.YoutubeChannelBrowser
{
    class RootNode : YoutubeChannelNodeBase
    {
        IEventAggregator EventAggregator { get; set; }

        public RootNode(IEventAggregator eventAggregator) :
            base("")
        {
            EventAggregator = eventAggregator;

            EventAggregator.GetEvent<AddFavoriteChannelEvent>().Subscribe(addChannel);
        }

        private void addChannel(Item.YoutubeChannelItem channelItem)
        {
            YoutubeChannelNode node = new YoutubeChannelNode(channelItem);
          
            Children.Add(node);

            if (Settings.Default.YoutubeChannels == null)
            {
                Settings.Default.YoutubeChannels = new List<SearchResult>();
            }

            Settings.Default.YoutubeChannels.Add(channelItem.Info as SearchResult);

        }

        protected override void LoadChildren()
        {
            if (Settings.Default.YoutubeChannels == null) return;

            foreach (SearchResult item in Settings.Default.YoutubeChannels)
            {
                Children.Add(new YoutubeChannelNode(new Item.YoutubeChannelItem(item,0)));
            }
        }

    }
}
