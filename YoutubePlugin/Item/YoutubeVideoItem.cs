using Google.Apis.Requests;
using Google.Apis.YouTube.v3.Data;
using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.Base.Item;
using MediaViewer.Model.Media.Base.Metadata;
using MediaViewer.Model.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Media.Imaging;

namespace YoutubePlugin.Item
{
    class YoutubeVideoItem : YoutubeItem
    {
        public bool IsEmbeddedOnly { get; set; }
        public bool HasPlayableStreams { get; set; }
        public String VideoId { get; protected set; }

        public List<YoutubeVideoStreamedItem> StreamedItem { get; set; }
                                
        public YoutubeVideoItem(SearchResult item, int relevance) :
            base(item.Snippet.Title, item, relevance)
        {
            ChannelTitle = item.Snippet.ChannelTitle;
            ChannelId = item.Snippet.ChannelId;

            ResourceId = item.Id;
            Thumbnail = item.Snippet.Thumbnails;
          
            PublishedAt = item.Snippet.PublishedAt;
            Description = item.Snippet.Description;

            VideoId = item.Id.VideoId;

            IsEmbeddedOnly = false;

            Info = item;

            StreamedItem = new List<YoutubeVideoStreamedItem>();                
        }

        public YoutubeVideoItem(PlaylistItem item, int relevance) :
            base(item.Snippet.Title, item, relevance)
        {
            ChannelTitle = item.Snippet.ChannelTitle;
            ChannelId = item.Snippet.ChannelId;

            ResourceId = item.Snippet.ResourceId;
            Thumbnail = item.Snippet.Thumbnails;

            PublishedAt = item.Snippet.PublishedAt;
            Description = item.Snippet.Description;

            VideoId = item.Snippet.ResourceId.VideoId;

            IsEmbeddedOnly = false;

            Info = item;

            StreamedItem = new List<YoutubeVideoStreamedItem>();
        }

        

        public void getStreams(out YoutubeVideoStreamedItem video, out YoutubeVideoStreamedItem audio, int maxHeight = 4096, int maxSamplesPerSecond = 48000)
        {
            video = null;
            audio = null;

            int bestHeight = 0;
            int bestSamplesPerSecond = 0;

            // get best video stream
            foreach (YoutubeVideoStreamedItem item in StreamedItem)
            {
                if (item.StreamType == StreamType.UNKNOWN) continue;

                VideoMetadata metaData = item.Metadata as VideoMetadata;

                if (metaData.Height >= bestHeight && metaData.Height <= maxHeight)
                {
                    if (metaData.Height == bestHeight)
                    {
                        // prefer mp4 over webm
                        if (item.Metadata.MimeType.Equals("video/webm")) continue;

                        // prefer adaptive over normal streams
                        if (item.StreamType == StreamType.VIDEO_AUDIO) continue;
                    }

                    video = item;

                    bestHeight = metaData.Height;
                }
            }

            if (video != null && video.StreamType == StreamType.VIDEO_AUDIO) return;

            // get best matching audio stream
            foreach (YoutubeVideoStreamedItem item in StreamedItem)
            {
                if (item.StreamType == StreamType.UNKNOWN) continue;

                VideoMetadata metaData = item.Metadata as VideoMetadata;

                if (metaData.SamplesPerSecond >= bestSamplesPerSecond && metaData.SamplesPerSecond <= maxSamplesPerSecond)
                {
                    int idx = video.Metadata.MimeType.IndexOf('/');

                    if (!metaData.MimeType.EndsWith(video.Metadata.MimeType.Substring(idx)))
                    {
                        // only use audio stream that matches with the video stream
                        continue;
                    }

                    if (metaData.SamplesPerSecond == bestSamplesPerSecond)
                    {
                        // prefer aac over ogg
                        if (item.Metadata.MimeType.Equals("audio/webm")) continue;
                    }

                    audio = item;

                    bestSamplesPerSecond = metaData.SamplesPerSecond.Value;
                }
            }

        }        
       
        public override void readMetadata_URLock(MetadataFactory.ReadOptions options, System.Threading.CancellationToken token)
        {            
            String thumbnailMimeType;

            try 
            {           
                ItemState = MediaItemState.LOADING;
                
                YoutubeItemMetadata metaData = new YoutubeItemMetadata();

                metaData.Thumbnail = new MediaViewer.MediaDatabase.Thumbnail(loadThumbnail(out thumbnailMimeType, token));
                metaData.CreationDate = PublishedAt;
                metaData.Title = Title;
                metaData.Description = String.IsNullOrEmpty(Description) ? Title : Description;
                metaData.SupportsXMPMetadata = true;
             
                NameValueCollection videoInfo;

                IsEmbeddedOnly = !getVideoInfo(VideoId, out videoInfo, token);

                if (!IsEmbeddedOnly)
                {
                    YoutubeVideoStreamedItem video, audio;

                    getStreams(out video, out audio);

                    VideoMetadata videoMetadata = video == null ? null : video.Metadata as VideoMetadata;
                    VideoMetadata audioMetadata = audio == null ? null : audio.Metadata as VideoMetadata;

                    if (videoMetadata != null)
                    {
                        metaData.Width = videoMetadata.Width;
                        metaData.Height = videoMetadata.Height;
                        metaData.MimeType = videoMetadata.MimeType;
                        metaData.FramesPerSecond = videoMetadata.FramesPerSecond == 0 ? null : new Nullable<double>(videoMetadata.FramesPerSecond);

                        metaData.SizeBytes = videoMetadata.SizeBytes;
                    }

                    if (audioMetadata != null)
                    {
                        metaData.SizeBytes += audioMetadata.SizeBytes;
                    }

                    if (videoMetadata == null && audioMetadata == null)
                    {
                        HasPlayableStreams = false;
                    }
                    else
                    {
                        HasPlayableStreams = true;
                    }
                }
                              
                long durationSeconds;

                if (long.TryParse(videoInfo["length_seconds"], out durationSeconds))
                {
                    metaData.DurationSeconds = durationSeconds;
                }

                long viewCount;

                if (long.TryParse(videoInfo["view_count"], out viewCount))
                {
                    metaData.ViewCount = viewCount;
                }

                double rating;

                if (double.TryParse(videoInfo["avg_rating"], NumberStyles.Float, new CultureInfo("en-US"), out rating))
                {
                    metaData.Rating = rating;
                }

                metaData.Author = videoInfo["author"];
                                   
                String tags = videoInfo["keywords"];
                if (tags != null)
                {
                    string[] tagNames = tags.Split(new char[] { ',' });

                    foreach (string tagName in tagNames)
                    {
                        if (String.IsNullOrEmpty(tagName) || String.IsNullOrWhiteSpace(tagName)) continue;

                        Tag newTag = new Tag();
                        newTag.Name = tagName;

                        metaData.Tags.Add(newTag);
                    }
                }
                                               
                Metadata = metaData;

                ItemState = MediaItemState.LOADED;
            }
            catch (Exception e)
            {
                if (e is System.Net.WebException &&
                    ((System.Net.WebException)e).Status == WebExceptionStatus.Timeout)
                {
                    ItemState = MediaItemState.TIMED_OUT;
                }
                else
                {
                    ItemState = MediaItemState.ERROR;
                }
            }
            
        }

        bool getVideoInfo(String videoId, out NameValueCollection videoInfo, CancellationToken token)
        {                   
            Uri location = new Uri("http://www.youtube.com/get_video_info?video_id=" + videoId + "&ps=default&eurl=&gl=US&hl=en");

            videoInfo = getInfo(location, token);
            
            if (videoInfo["status"].Equals("fail"))
            {
                return (false);
            }

            String streamMapsEncoded = videoInfo["url_encoded_fmt_stream_map"];

            String[] streamMapEncoded = streamMapsEncoded.Split(new char[] { ',' });

            foreach (String map in streamMapEncoded)
            {
                NameValueCollection mapArgsEnc = (HttpUtility.ParseQueryString(map));
                NameValueCollection mapArgsDec = new NameValueCollection();

                foreach (String key in mapArgsEnc)
                {
                    mapArgsDec.Add(key, HttpUtility.UrlDecode(mapArgsEnc[key]));
                }

                StreamedItem.Add(new YoutubeVideoStreamedItem(mapArgsDec, Name));
            }

            streamMapsEncoded = videoInfo["adaptive_fmts"];

            if (streamMapsEncoded == null) return(true);

            streamMapEncoded = streamMapsEncoded.Split(new char[] { ',' });

            foreach (String map in streamMapEncoded)
            {
                NameValueCollection mapArgsEnc = (HttpUtility.ParseQueryString(map));
                NameValueCollection mapArgsDec = new NameValueCollection();

                foreach (String key in mapArgsEnc)
                {
                    mapArgsDec.Add(key, HttpUtility.UrlDecode(mapArgsEnc[key]));
                }

                StreamedItem.Add(new YoutubeVideoStreamedItem(mapArgsDec, Name));                
            }

            return (true);
        }

        

    }
}
