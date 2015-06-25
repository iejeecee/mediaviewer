using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using MediaViewer;
using MediaViewer.Filter;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Media.State;
using MediaViewer.Model.Media.State.CollectionView;
using MediaViewer.Model.Mvvm;
using MediaViewer.Progress;
using Microsoft.Practices.Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
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
        IRegionManager RegionManager { get; set; }
        YouTubeService Youtube { get; set; }

        public YoutubeViewModel(IRegionManager regionManager) {

            Youtube = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = YoutubeApiKey.ApiKey,
                ApplicationName = "MediaViewer"
            });

            RegionManager = regionManager;
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

                    var searchListRequest = Youtube.Search.List("snippet");
                    searchListRequest.Q = Query;  

                    List<YoutubeItem> result = await search(searchListRequest);

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

            ViewCommand = new Command<SelectableMediaItem>(async (selectableItem) =>
            {               
                if(selectableItem.Item.Metadata == null) return;

                if (selectableItem.Item is YoutubeVideoItem)
                {
                    YoutubeVideoItem item = selectableItem.Item as YoutubeVideoItem;

                    if (item.IsEmbeddedOnly)
                    {
                        Process.Start("https://www.youtube.com/watch?v=" + item.SearchResult.Id.VideoId);
                    }
                    else
                    {
                        YoutubeVideoStreamedItem video, audio;
                        item.getBestQualityStreams(out video, out audio);

                        Shell.ShellViewModel.navigateToVideoView(video, null, audio);
                    }
                }
                else if (selectableItem.Item is YoutubeChannelItem)
                {
                    YoutubeChannelItem item = selectableItem.Item as YoutubeChannelItem;

                    MediaState.clearUIState(item.Name, DateTime.Now, MediaStateType.SearchResult);

                    var searchListRequest = Youtube.Search.List("snippet");
                    searchListRequest.ChannelId = item.SearchResult.Id.ChannelId;
                    searchListRequest.Order = Google.Apis.YouTube.v3.SearchResource.ListRequest.OrderEnum.Date;

                    List<YoutubeItem> result = await search(searchListRequest);

                    MediaState.addUIState(result);
                }
               
            });

            DownloadCommand = new Command<SelectableMediaItem>(async (selectableItem) => {

                List<MediaItem> items = MediaStateCollectionView.getSelectedItems();
                if (items.Count == 0)
                {
                    items.Add(selectableItem.Item);
                }

                String outputPath = MediaFileWatcher.Instance.Path;

                CancellableOperationProgressView progressView = new CancellableOperationProgressView();
                DownloadProgressViewModel vm = new DownloadProgressViewModel();
                progressView.DataContext = vm;

                progressView.Show();
                vm.OkCommand.IsExecutable = false;
                vm.CancelCommand.IsExecutable = true;

                await Task.Factory.StartNew(() =>
                {
                    vm.startDownload(outputPath, items);
                });

                vm.OkCommand.IsExecutable = true;
                vm.CancelCommand.IsExecutable = false;
                        
            });

            SelectAllCommand = new Command(() =>
            {
                MediaStateCollectionView.selectAll();
            }, false);

            DeselectAllCommand = new Command(() =>
            {
                MediaStateCollectionView.deselectAll();
            });

            CloseCommand = new Command(() => OnClosingRequest());

            Uri tagFilterViewUri = new Uri(typeof(TagFilterView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();
            navigationParams.Add("MediaStateCollectionView", MediaStateCollectionView);

            RegionManager.RequestNavigate("youtubeExpanderPanelRegion", tagFilterViewUri, navigationParams);
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

        private async Task<List<YoutubeItem>> search(Google.Apis.YouTube.v3.SearchResource.ListRequest searchListRequest)
        {                                              
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
