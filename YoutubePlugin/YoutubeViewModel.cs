using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using MediaViewer;
using MediaViewer.Model.Media.State;
using MediaViewer.Model.Media.State.CollectionView;
using MediaViewer.Model.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using YoutubePlugin.Item;

namespace YoutubePlugin
{
    
    class YoutubeViewModel : CloseableBindableBase
    {
        public YoutubeViewModel() {

            NrColumns = 4;

            MediaState = new MediaState();
            MediaStateCollectionView = new YoutubeCollectionView(MediaState);
            MediaState.clearUIState("Empty", DateTime.Now, MediaStateType.SearchResult);

            SearchCommand = new Command(async () =>
            {
                SearchCommand.IsExecutable = false;
                try
                {
                    MediaState.clearUIState("Search Results",DateTime.Now, MediaStateType.SearchResult);

                    List<YoutubeItem> result = await search(Query);

                    MediaState.addUIState(result);

                } catch (AggregateException ex) {

                    String error = "";

                    foreach (var e in ex.InnerExceptions)
                    {
                        error += e.Message + "\n";
                    }

                    MessageBox.Show(error, "error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                SearchCommand.IsExecutable = true;

            });            

            ViewCommand = new Command<SelectableMediaItem>((selectableItem) =>
            {
                YoutubeVideoItem item = selectableItem.Item as YoutubeVideoItem;

                if (item == null || item.Metadata == null) return;
                                  
                Shell.ShellViewModel.navigateToVideoView(item.Metadata.Location);
               
            });

            DownloadCommand = new Command<SelectableMediaItem>((selectableItem) => { });

            SelectAllCommand = new Command(() =>
            {
                MediaStateCollectionView.selectAll();
            }, false);

            DeselectAllCommand = new Command(() =>
            {
                MediaStateCollectionView.deselectAll();
            });

            CloseCommand = new Command(() => OnClosingRequest());
        }

        int nrColumns;

        public int NrColumns
        {
            get { return nrColumns; }
            set { SetProperty(ref nrColumns, value); }
        }

        String query;

        public String Query
        {
            get { return query; }
            set { SetProperty(ref query, value); }
        }

        public Command SearchCommand { get; set; }
        public Command<SelectableMediaItem> ViewCommand { get; set; }
        public Command<SelectableMediaItem> DownloadCommand { get; set; }
        public Command CloseCommand { get; set; }
        public Command SelectAllCommand { get; set; }
        public Command DeselectAllCommand { get; set; }
        public MediaState MediaState { get; set; }
        public YoutubeCollectionView MediaStateCollectionView { get; set; }
        
        private async Task<List<YoutubeItem>> search(String searchQuery)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = YoutubeApiKey.ApiKey,
                ApplicationName = "MediaViewer"
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = searchQuery; // Replace with your search term.
            
            searchListRequest.MaxResults = 50;
     
            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await searchListRequest.ExecuteAsync();
        
            List<YoutubeItem> items = new List<YoutubeItem>();
            int relevance = 0;

            // Add each result to the appropriate list, and then display the lists of
            // matching videos, channels, and playlists.
            foreach (var searchResult in searchListResponse.Items)
            {                              
                switch (searchResult.Id.Kind)
                {
                    case "youtube#video":
                        items.Add(new YoutubeVideoItem(searchResult, relevance));
                        break;

                    case "youtube#channel":
                        items.Add(new YoutubeChannelItem(searchResult, relevance));
                        break;

                    case "youtube#playlist":                        
                        break;
                }

                relevance++;
            }

            return (items);
        }
  

    }
}
