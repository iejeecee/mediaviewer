using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using MediaViewer.Infrastructure.Pubsubhubbub;
using MediaViewer.Infrastructure.Utils;
using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using YoutubePlugin.Events;
using YoutubePlugin.Item;
using YoutubePlugin.Properties;

namespace YoutubePlugin.YoutubeChannelBrowser
{
    
    class YoutubeChannelRootNode : YoutubeChannelNodeBase
    {
        IEventAggregator EventAggregator { get; set; }
        Object channelLock;

        public YoutubeChannelRootNode(IEventAggregator eventAggregator) :
            base("")
        {
            EventAggregator = eventAggregator;

            EventAggregator.GetEvent<AddFavoriteChannelEvent>().Subscribe(addChannel);
            LazyLoading = true;
            channelLock = new Object();
        }

        private void addChannel(YoutubeChannelItem channelItem)
        {
            YoutubeChannelNodeState state = new YoutubeChannelNodeState(channelItem);
            YoutubeChannelNode node = new YoutubeChannelNode(state);

            lock (channelLock)
            {
                Children.Add(node);                
                Properties.Settings.Default.YoutubeChannels.Add(state);
            }  
           
        }

        public void removeChannel(YoutubeChannelNode node)
        {
            lock (channelLock)
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    YoutubeChannelNode child = Children[i] as YoutubeChannelNode;

                    if(child.ChannelId.Equals(node.ChannelId)) {

                        Children.RemoveAt(i);
                        Properties.Settings.Default.YoutubeChannels.RemoveAt(i);                   
                        return;
                    }
                }
            } 
           
        }

        protected override void LoadChildren()
        {                              
            foreach(YoutubeChannelNodeState state in Properties.Settings.Default.YoutubeChannels)
            {              
                YoutubeChannelNode node = new YoutubeChannelNode(state);

                Children.Add(node);               
            }

            Task task = loadChannelStatistics();
          
        }

        public async Task loadChannelStatistics()
        {
            String channelIds = "";

            lock (channelLock)
            {
                if (Children.Count == 0) return;

                channelIds = (Children[0] as YoutubeChannelNode).ChannelId;

                for (int i = 1; i < Children.Count; i++)
                {
                    channelIds += "," + (Children[i] as YoutubeChannelNode).ChannelId;
                }
            }
          
            Google.Apis.YouTube.v3.ChannelsResource.ListRequest listRequest = new ChannelsResource.ListRequest(YoutubeViewModel.Youtube, "statistics");

            listRequest.Id = channelIds;
          
            ChannelListResponse response = await listRequest.ExecuteAsync();

            lock (channelLock)
            {
                foreach (Channel item in response.Items)
                {
                    YoutubeChannelNode node = getChannelWithId(item.Id);

                    if (node != null)
                    {
                        node.updateStatistics(item.Statistics);
                    }
                }
            }

        }

        YoutubeChannelNode getChannelWithId(String channelId)
        {
            foreach (YoutubeChannelNode child in Children)
            {
                if (child.ChannelId.Equals(channelId))
                {
                    return (child);
                }
            }

            return (null);
        }

        
    
    }
}
