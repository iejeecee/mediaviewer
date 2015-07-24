using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using MediaViewer.Model.Mvvm;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubePlugin.Events;

namespace YoutubePlugin.YoutubeSearch
{
    class YoutubeSearchViewModel : BindableBase
    {
        IEventAggregator EventAggregator { get; set; }
        YouTubeService Youtube { get; set; }
        const int maxResults = 50;

        public YoutubeSearchViewModel(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;

            Youtube = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = YoutubeApiKey.ApiKey,
                ApplicationName = "MediaViewer"
            });

            SearchCommand = new Command(() =>
            {                              
                var searchListRequest = Youtube.Search.List("snippet");
                searchListRequest.Q = Query;
                searchListRequest.MaxResults = maxResults;

                YoutubeSearchQuery youtubeSearch = new YoutubeSearchQuery(searchListRequest, "Search Result");

                EventAggregator.GetEvent<SearchEvent>().Publish(youtubeSearch);
               
            });         
        }

        String query;

        public String Query
        {
            get { return query; }
            set { SetProperty(ref query, value); }
        }

        public Command SearchCommand { get; set; }
    }
}
