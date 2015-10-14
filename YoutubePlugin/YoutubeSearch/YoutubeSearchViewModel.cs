using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using MediaViewer.Model.Mvvm;
using Microsoft.Maps.MapControl.WPF;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using YoutubePlugin.Events;

namespace YoutubePlugin.YoutubeSearch
{
    enum PublishedFilter
    {
        Any,
        LastHour,
        Today,
        ThisWeek,
        ThisMonth,
        ThisYear
    }

    enum ResourceType
    {
        Any,
        Video,
        Channel,
        Playlist,
        Movie,
        Show
    }

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

                DateTime? publishedAfter = null;
                DateTime current = DateTime.Now;
               
                switch ((PublishedFilter)PublishedDate.CurrentItem)
                {
                    case PublishedFilter.Any:
                        break;
                    case PublishedFilter.LastHour:
                        publishedAfter = current - new TimeSpan(1, 0, 0);
                        break;
                    case PublishedFilter.Today:
                        publishedAfter = DateTime.Today;
                        break;
                    case PublishedFilter.ThisWeek:
                        int delta = DayOfWeek.Monday - DateTime.Today.DayOfWeek;
                        publishedAfter = DateTime.Today.AddDays(delta);
                        break;
                    case PublishedFilter.ThisMonth:                   
                        publishedAfter = new DateTime(current.Year, current.Month, 1);
                        break;
                    case PublishedFilter.ThisYear:
                        publishedAfter = new DateTime(current.Year, 1, 1);
                        break;
                    default:
                        break;
                }
                
               
                searchListRequest.PublishedAfter = publishedAfter;              
                searchListRequest.VideoDuration = (Google.Apis.YouTube.v3.SearchResource.ListRequest.VideoDurationEnum)VideoDuration.CurrentItem;

                if (searchListRequest.VideoDuration != SearchResource.ListRequest.VideoDurationEnum.Any)
                {
                    searchListRequest.Type = "video";
                }

                if((ResourceType)Type.CurrentItem != ResourceType.Any) {

                    switch ((ResourceType)Type.CurrentItem)
                    {                        
                        case ResourceType.Movie:
                            searchListRequest.VideoType = Google.Apis.YouTube.v3.SearchResource.ListRequest.VideoTypeEnum.Movie;
                            searchListRequest.Type = "video";
                            break;
                        case ResourceType.Show:
                            searchListRequest.VideoType = Google.Apis.YouTube.v3.SearchResource.ListRequest.VideoTypeEnum.Episode;
                            searchListRequest.Type = "video";
                            break;
                        default:
                            searchListRequest.Type = ((ResourceType)Type.CurrentItem).ToString().ToLower();
                            break;
                    }
                    
                }

                if (GeoLocationRect != null)
                {
                    string location = GeoLocationRect.Center.Latitude.ToString(CultureInfo.InvariantCulture) + "," + GeoLocationRect.Center.Longitude.ToString(CultureInfo.InvariantCulture);

                    searchListRequest.Location = location;

                    double radius = DistanceTo(GeoLocationRect.Center, new Location(GeoLocationRect.Center.Latitude, GeoLocationRect.East));

                    searchListRequest.LocationRadius = radius.ToString(CultureInfo.InvariantCulture) + "km";
                    searchListRequest.Type = "video,list";
                }

                YoutubeSearchQuery youtubeSearch = new YoutubeSearchQuery(searchListRequest, "Search Result");

                EventAggregator.GetEvent<SearchEvent>().Publish(youtubeSearch);
               
            });
            
            PublishedDate = new ListCollectionView(Enum.GetValues(typeof(PublishedFilter)));
            VideoDuration = new ListCollectionView(Enum.GetValues(typeof(Google.Apis.YouTube.v3.SearchResource.ListRequest.VideoDurationEnum)));
            Type = new ListCollectionView(Enum.GetValues(typeof(ResourceType)));
        }

        String query;

        public String Query
        {
            get { return query; }
            set { SetProperty(ref query, value); }
        }

        public ListCollectionView PublishedDate { get; set; }
        public ListCollectionView VideoDuration { get; set; }
        public ListCollectionView Type { get; set; }

        LocationRect geoLocationRect;

        public LocationRect GeoLocationRect
        {
            get { return geoLocationRect; }
            set { SetProperty(ref geoLocationRect, value); }
        }

        public Command SearchCommand { get; set; }


        static double DistanceTo(Location a, Location b, char unit = 'K')
        {
            double rlat1 = Math.PI * a.Latitude / 180;
            double rlat2 = Math.PI * b.Latitude / 180;
            double theta = a.Longitude - b.Longitude;
            double rtheta = Math.PI * theta / 180;
            double dist =
                Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
                Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            switch (unit)
            {
                case 'K': //Kilometers -> default
                    return dist * 1.609344;
                case 'N': //Nautical Miles 
                    return dist * 0.8684;
                case 'M': //Miles
                    return dist;
            }

            return dist;
        }
    }
}
