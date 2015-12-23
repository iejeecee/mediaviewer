using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using MediaViewer.UserControls.TabbedExpander;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Windows.Themes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YoutubePlugin.Events;
using YoutubePlugin.YoutubeSearch;

namespace YoutubePlugin.YoutubeChannelBrowser
{
    /// <summary>
    /// Interaction logic for YoutubeChannelBrowserView.xaml
    /// </summary>
    [Export]
    public partial class YoutubeChannelBrowserView : UserControl, ITabbedExpanderAware
    {
        IEventAggregator EventAggregator { get; set; }
        YouTubeService Youtube { get; set; }

        [ImportingConstructor]
        public YoutubeChannelBrowserView(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            EventAggregator = eventAggregator;

            treeView.SelectionChanged += treeView_SelectionChanged;                       
            treeView.Root = new YoutubeChannelRootNode(eventAggregator);

            TabName = "Browse";
            TabMargin = new Thickness(2);
            TabBorderThickness = new Thickness(0);
            TabBorderBrush = null;

            Youtube = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = YoutubeApiKey.ApiKey,
                ApplicationName = "MediaViewer"
            });
        }

        void treeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (treeView.SelectedItems.Count == 0)
            {
                return;
            }

            YoutubeChannelNodeBase item = treeView.SelectedItems[0] as YoutubeChannelNodeBase;
            YoutubeSearchQuery youtubeSearch = null;

            if (item is YoutubeChannelVideosNode || item is YoutubeChannelNode)
            {
                SearchResource.ListRequest searchListRequest = searchListRequest = Youtube.Search.List("snippet");
                searchListRequest.ChannelId = item.ChannelId;
                searchListRequest.MaxResults = YoutubeSearchViewModel.maxResults;
                searchListRequest.Order = Google.Apis.YouTube.v3.SearchResource.ListRequest.OrderEnum.Date;

                youtubeSearch = new YoutubeSearchQuery(searchListRequest, item.Name);
            }
            else if (item is YoutubeChannelPlaylistsNode)
            {
                PlaylistsResource.ListRequest playlistRequest = Youtube.Playlists.List("snippet");
                playlistRequest.ChannelId = item.ChannelId;
                playlistRequest.MaxResults = YoutubeSearchViewModel.maxResults;

                youtubeSearch = new YoutubeSearchQuery(playlistRequest, item.Name);
            }
            
            EventAggregator.GetEvent<SearchEvent>().Publish(youtubeSearch);

            e.Handled = true;
        }

        public string TabName { get; set; }
        public bool TabIsSelected { get; set; }
        public Thickness TabMargin { get; set; }
        public Thickness TabBorderThickness { get; set; }
        public Brush TabBorderBrush { get; set; }

        private void removeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            YoutubeChannelNode selectedNode = treeView.SelectedItem as YoutubeChannelNode;
            if (selectedNode == null) return;

            (treeView.Root as YoutubeChannelRootNode).removeChannel(selectedNode);
        }
    }
}
