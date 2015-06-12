using Google.Apis.YouTube.v3.Data;
using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.Base;
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
using System.Windows.Media.Imaging;

namespace YoutubePlugin.Item
{
    class YoutubeVideoItem : YoutubeItem
    {
       
        public List<VideoStreamInfo> VideoStreamInfo { get; set; }
        public List<VideoStreamInfo> AdaptiveVideoStreamInfo { get; set; }
               
        public YoutubeVideoItem(SearchResult result, int relevance) :
            base(result, relevance)
        {
            Result = result;
            VideoStreamInfo = new List<VideoStreamInfo>();
            AdaptiveVideoStreamInfo = new List<VideoStreamInfo>();
        }

        public override void readMetadata(MediaViewer.Model.metadata.Metadata.MetadataFactory.ReadOptions options, System.Threading.CancellationToken token)
        {            
            String mimeType;

            RWLock.EnterUpgradeableReadLock();
            try
            {
                ItemState = MediaItemState.LOADING;
                
                YoutubeItemMetadata metaData = new YoutubeItemMetadata();

                metaData.Thumbnail = new MediaViewer.MediaDatabase.Thumbnail(loadThumbnail(out mimeType, token));
                metaData.CreationDate = Result.Snippet.PublishedAt;
                metaData.Title = Result.Snippet.Title;
                metaData.Description = String.IsNullOrEmpty(Result.Snippet.Description) ? Result.Snippet.Title : Result.Snippet.Description;
             
                NameValueCollection videoInfo;

                bool success = getVideoInfo(Result.Id.VideoId, out videoInfo, token);
              
                if (success)
                {
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

                    if (AdaptiveVideoStreamInfo.Count > 0 && 
                        Nullable.Compare<int>(AdaptiveVideoStreamInfo[0].Width, VideoStreamInfo[0].Width) == 1)
                    {
                        metaData.Location = AdaptiveVideoStreamInfo[0].Url;
                        metaData.Width = AdaptiveVideoStreamInfo[0].Width;
                        metaData.Height = AdaptiveVideoStreamInfo[0].Height;
                    }
                    else
                    {
                        metaData.Location = VideoStreamInfo[0].Url;
                        metaData.Width = VideoStreamInfo[0].Width;
                        metaData.Height = VideoStreamInfo[0].Height;
                    }

                    metaData.MimeType = VideoStreamInfo[0].Type.Substring(0, VideoStreamInfo[0].Type.IndexOf(';'));

                    String tags = videoInfo["keywords"];
                    if (tags != null)
                    {
                        string[] tagNames = tags.Split(new char[] { ',' });

                        foreach (string tagName in tagNames)
                        {
                            Tag newTag = new Tag();
                            newTag.Name = tagName;

                            metaData.Tags.Add(newTag);
                        }
                    }

                }
                else
                {
                    metaData.MetadataReadError = new Exception(HttpUtility.UrlDecode(videoInfo["reason"]));
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
            finally
            {
                RWLock.ExitUpgradeableReadLock();
            }
        }

        bool getVideoInfo(String videoId, out NameValueCollection videoInfo, CancellationToken token)
        {                   
            Uri location = new Uri("http://www.youtube.com/get_video_info?video_id=" + videoId + "&ps=default&eurl=&gl=US&hl=en");

            videoInfo = getInfo(location,token);
            
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

                VideoStreamInfo.Add(new VideoStreamInfo(mapArgsDec));
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

                AdaptiveVideoStreamInfo.Add(new VideoStreamInfo(mapArgsDec));
            }

            return (true);

        }
    }
}
