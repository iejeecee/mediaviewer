using ICSharpCode.TreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YoutubePlugin.YoutubeChannelBrowser
{
    class YoutubeChannelNode : YoutubeNodeBase
    {
        public YoutubeChannelNode(String name, String channelId) :
            base("pack://application:,,,/YoutubePlugin;component/Resources/Icons/channel.ico")
        {
            Name = name;
            ChannelId = channelId;
        }

        protected override void LoadChildren()
        {
            Children.Add(new YoutubeChannelVideosNode(ChannelId));
            Children.Add(new YoutubeChannelPlaylistsNode(ChannelId));

            /*LoadingChildrenTask = Task.Run(() =>
            {
                List<SharpTreeNode> directories = createDirectoryNodes(FullName);

                App.Current.Dispatcher.Invoke(() =>
                {
                    Children.AddRange(directories);
                });

                foreach (SharpTreeNode directory in directories)
                {
                    ((Location)directory).infoGatherTask.addLocation((Location)directory);
                }
            });*/

        }

        String name;

        public String Name
        {
            get { return name; }
            set
            {
                name = value;

                RaisePropertyChanged("Name");
               
            }
        }
                
    }
}
