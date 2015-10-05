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
                Settings.Default.YoutubeChannels.Add(state);
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
                        Settings.Default.YoutubeChannels.RemoveAt(i);                   
                        return;
                    }
                }
            } 
           
        }

        protected override void LoadChildren()
        {                              
            foreach(YoutubeChannelNodeState state in Settings.Default.YoutubeChannels)
            {              
                YoutubeChannelNode node = new YoutubeChannelNode(state);

                Children.Add(node);               
            }

            Task task = loadChannelStatistics();

            //recieveYoutubePushNotifications();
        }

        async Task loadChannelStatistics()
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

        String getLocalIpAdress()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }
       

        void recieveYoutubePushNotifications()
        {
            const string FeedToSubscribe = "https://www.youtube.com/xml/feeds/videos.xml?channel_id=UCYljqyDdMZzqqHLZOgs49JQ";
            const string HubUrl = "https://pubsubhubbub.appspot.com/";

            String ip = getLocalIpAdress();

            const string CallbackUrl = "your callback url here";

            // So trace output will go to the console.        
            var callback = new PushSubscriberCallback("http://+:8080/");
            try
            {
                callback.Start();
                callback.PushPost += callback_PushPost;
                callback.PushVerify += callback_PushVerify;
                // Subscribe to a feed
                Console.WriteLine("Subscribing to {0}", FeedToSubscribe);
                var statusCode = PushSubscriber.Subscribe(
                    HubUrl,
                    CallbackUrl,
                    FeedToSubscribe,
                    PushVerifyType.Sync,
                    0,
                    "xyzzy",
                    null);
                Console.WriteLine("Status code = {0}", statusCode);
                Console.WriteLine("Listening for connections from hub.");
                Console.WriteLine("Press Enter to exit program.");
                Console.ReadLine();
                // Unsubscribe
                Console.WriteLine("Unsubscribing...");
                statusCode = PushSubscriber.Unsubscribe(
                    HubUrl, 
                    CallbackUrl, 
                    FeedToSubscribe, 
                    PushVerifyType.Sync, 
                    "xyzzy");
                Console.WriteLine("Return value = {0}", statusCode);
            }
            finally
            {
                callback.Stop();
                callback.Dispose();
            }
           
        }

        const string FeedBaseName = "feed";
        const string FeedExtension = ".xml";
        static void callback_PushPost(object sender, PushPostEventArgs args)
        {
            Console.WriteLine("{0} - Received update from hub!", DateTime.Now);
            try
            {
                // Save the update to file.
                string timestamp = DateTime.Now.ToString("yyyyMMdd_hhmm");
                /*string saveFilename = FeedBaseName + "_" + timestamp + FeedExtension;
                Console.WriteLine("Writing feed to {0}", saveFilename);
                using (var writer = XmlWriter.Create(saveFilename))
                {
                    args.Feed.SaveAsRss20(writer);
                }*/
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception!\r\n{0}", ex);
            }
            Console.WriteLine("Done");
        }

        static void callback_PushVerify(object sender, PushVerifyEventArgs args)
        {
            Console.WriteLine("{0} - Received verify message from hub.", DateTime.Now);
            // Verify all requests.
            args.Allow = true;
        }
    
    }
}
