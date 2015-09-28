using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubePlugin.Events;

namespace YoutubePlugin.YoutubeChannelBrowser
{
    class RootNode : YoutubeNodeBase
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
            Children.Add(new YoutubeChannelNode(channelItem));
        }

        protected override void LoadChildren()
        {
           
            Children.Add(new YoutubeChannelNode("8 Bites", "UCYljqyDdMZzqqHLZOgs49JQ"));
            Children.Add(new YoutubeChannelNode("Chess24", "UCTzRQxC3i7GOT4jtiTq4e0w"));
            Children.Add(new YoutubeChannelNode("FullTimeDEVILS", "UC7w8GnTF2Sp3wldDMtCCtVw"));
            Children.Add(new YoutubeChannelNode("Nayla Games", "UCQ71w-t-1FSlwa57iuBfKTw"));         
            Children.Add(new YoutubeChannelNode("DSPGaming", "UCGAQFQoZNIFUnQQuA-Llu9A"));
            Children.Add(new YoutubeChannelNode("TYT Sports", "UCPahzXZvF8f5bRJ5TNvZS8w"));           
        }

    }
}
