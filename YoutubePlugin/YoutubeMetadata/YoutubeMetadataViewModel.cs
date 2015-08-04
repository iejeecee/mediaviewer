using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using MediaViewer.Infrastructure.Logging;
using MediaViewer.MediaDatabase;
using MediaViewer.Model.Utils;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YoutubePlugin.Events;
using YoutubePlugin.Item;

namespace YoutubePlugin.YoutubeMetadata
{
    class YoutubeMetadataViewModel : BindableBase
    {
        IEventAggregator EventAggregator { get; set; }
        YouTubeService Youtube { get; set; }

        Task<VideoListResponse> RequestVideoInfoTask { get; set; }
        CancellationTokenSource RequestVideoInfoTaskTokenSource { get; set; }

        public YoutubeMetadataViewModel(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;

            Tags = new ObservableCollection<Tag>();

            EventAggregator.GetEvent<SelectionEvent>().Subscribe(selectionEvent);

            IsBatchMode = false;

            Youtube = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = YoutubeApiKey.ApiKey,
                ApplicationName = "MediaViewer"
            });

            RequestVideoInfoTaskTokenSource = new CancellationTokenSource();
            DynamicProperties = new ObservableCollection<Tuple<string, string>>();

            clear();
        }

        void clear()
        {
            Title = null;
            Description = null;
            Author = null;
            CreationDate = null;
            Tags.Clear();
            DynamicProperties.Clear();
            Rating = null;

            IsBatchMode = false;
            IsEnabled = false;

            IsTitleEnabled = false;
            IsAuthorEnabled = false;
            IsDescriptionEnabled = false;
            IsCreationDateEnabled = false;
        }

        private async void selectionEvent(ICollection<YoutubeItem> items)
        {
            clear();

            if (items.Count == 0)
            {                
                return;
            }
            else if (items.Count == 1)
            {
                IsBatchMode = false;
                IsTitleEnabled = true;
                IsAuthorEnabled = true;
                IsCreationDateEnabled = true;

                await showItem(items.ElementAt(0));

                IsDescriptionEnabled = true;
                IsEnabled = true;
                              
            }
        }

        async Task showItem(YoutubeItem item)
        {
            YoutubeItemMetadata metaData = item.Metadata as YoutubeItemMetadata;

            if (metaData == null)
            {               
                return;
            }

            Title = metaData.Title;           
            Rating = metaData.Rating == null ? null : metaData.Rating / 5;
            Author = metaData.Author;
            CreationDate = metaData.CreationDate;

            Tags.Clear();
            foreach (Tag tag in metaData.Tags)
            {
                Tags.Add(tag);
            }
           
            if (!metaData.Description.EndsWith("..."))
            {
                Description = metaData.Description;
            } 
            else if (item is YoutubeVideoItem)
            {                
                Video info = await requestVideoInfo((item as YoutubeVideoItem).VideoId);
               
                if (info != null)
                {
                    Description = metaData.Description = info.Snippet.Description;
                }
                else
                {
                    Description = metaData.Description;
                }
                               
            }
            else
            {
                Description = metaData.Description;
            }

            if (metaData.ViewCount != null)
            {
                DynamicProperties.Add(new Tuple<string, string>("Views", metaData.ViewCount.Value.ToString("#,##0", new CultureInfo("en-US"))));
            }

            if (metaData.MimeType != null)
            {
                DynamicProperties.Add(new Tuple<string, string>("Mime Type", metaData.MimeType));
            }

            if (metaData.Width != null && metaData.Height != null)
            {
                DynamicProperties.Add(new Tuple<string, string>("Resolution", metaData.Width + " x " + metaData.Height));
            }

            if (metaData.FramesPerSecond != null)
            {
                DynamicProperties.Add(new Tuple<string, string>("Frames Per Second", metaData.FramesPerSecond.Value.ToString()));
            }

            if (metaData.DurationSeconds != null)
            {
                DynamicProperties.Add(new Tuple<string, string>("Duration", MiscUtils.formatTimeSeconds(metaData.DurationSeconds.Value)));
            }
        }

        async Task<Video> requestVideoInfo(String videoId)
        {
            var request = Youtube.Videos.List("snippet");
            request.Id = videoId;

            if (RequestVideoInfoTask != null && !RequestVideoInfoTask.IsCompleted)
            {
                RequestVideoInfoTaskTokenSource.Cancel();
                await RequestVideoInfoTask;

                RequestVideoInfoTaskTokenSource = new CancellationTokenSource();
            }

            try
            {
                RequestVideoInfoTask = request.ExecuteAsync(RequestVideoInfoTaskTokenSource.Token);

                VideoListResponse response = await RequestVideoInfoTask;

                if (response.Items.Count > 0)
                {
                    return response.Items[0];
                }
                
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception e)
            {
                Logger.Log.Error("Error requesting youtube video info: " + videoId, e);
            }

            return (null);
        }

        string title;

        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        double? rating;

        public double? Rating
        {
            get { return rating; }
            set { SetProperty(ref rating, value); }
        }

        string description;

        public string Description
        {
            get { return description; }
            set { SetProperty(ref description, value); }
        }

        string author;

        public string Author
        {
            get { return author; }
            set { SetProperty(ref author, value); }
        }

        ObservableCollection<Tag> tags;

        public ObservableCollection<Tag> Tags
        {
            get { return tags; }
            set { tags = value; }
        }

        DateTime? creationDate;

        public DateTime? CreationDate
        {
          get { return creationDate; }
          set { SetProperty(ref creationDate, value); }
        }

        bool isBatchMode;

        public bool IsBatchMode
        {
            get { return isBatchMode; }
            set { SetProperty(ref isBatchMode, value); }
        }

        bool isEnabled;

        public bool IsEnabled
        {
            get { return isEnabled; }
            set { SetProperty(ref isEnabled, value); }
        }

        bool isAuthorEnabled;

        public bool IsAuthorEnabled
        {
            get { return isAuthorEnabled; }
            set { SetProperty(ref isAuthorEnabled, value); }
        }
        bool isDescriptionEnabled;

        public bool IsDescriptionEnabled
        {
            get { return isDescriptionEnabled; }
            set { SetProperty(ref isDescriptionEnabled, value); }
        }
        bool isTitleEnabled;

        public bool IsTitleEnabled
        {
            get { return isTitleEnabled; }
            set { SetProperty(ref isTitleEnabled, value); }
        }

        bool isCreationDateEnabled;

        public bool IsCreationDateEnabled
        {
            get { return isCreationDateEnabled; }
            set { SetProperty(ref isCreationDateEnabled, value); }
        }

        ObservableCollection<Tuple<String, String>> dynamicProperties;

        public ObservableCollection<Tuple<String, String>> DynamicProperties
        {
            get { return dynamicProperties; }
            set { dynamicProperties = value; }
        }
    }
}
