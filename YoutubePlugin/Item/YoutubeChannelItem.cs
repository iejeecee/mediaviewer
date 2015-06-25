using Google.Apis.YouTube.v3.Data;
using MediaViewer.Model.Media.Base;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubePlugin.Item
{
    class YoutubeChannelItem : YoutubeItem
    {
        public YoutubeChannelItem(SearchResult result, int relevance) :
            base(result, relevance)
        {
            SearchResult = result;
          
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
                metaData.CreationDate = SearchResult.Snippet.PublishedAt;
                metaData.Title = SearchResult.Snippet.Title;
                metaData.Description = String.IsNullOrEmpty(SearchResult.Snippet.Description) ? SearchResult.Snippet.Title : SearchResult.Snippet.Description;

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

       /* bool getChannelInfo(String channelId, out NameValueCollection channelInfo, CancellationToken token)
        {
            //channelInfo = getInfo("http://www.youtube.com/get_channel_info?channel_id=" + channelId, token);
            
            return (true);

        }*/
    }
}
