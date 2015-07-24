using Google.Apis.YouTube.v3.Data;
using MediaViewer.Model.Media.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace YoutubePlugin.Item
{
    class YoutubePlaylistItem : YoutubeItem
    {
        public YoutubePlaylistItem(Google.Apis.YouTube.v3.Data.SearchResult result, int relevance) 
            : base(result, relevance)
        {

        }

        public override void readMetadata(MediaViewer.Model.metadata.Metadata.MetadataFactory.ReadOptions options, System.Threading.CancellationToken token)
        {
            String mimeType;

            RWLock.EnterUpgradeableReadLock();
            try
            {
                ItemState = MediaItemState.LOADING;

                YoutubeItemMetadata metaData = new YoutubeItemMetadata();

                SearchResult searchInfo = Info as SearchResult;

                metaData.Thumbnail = new MediaViewer.MediaDatabase.Thumbnail(loadThumbnail(out mimeType, token));
                metaData.CreationDate = searchInfo.Snippet.PublishedAt;
                metaData.Title = searchInfo.Snippet.Title;
                metaData.Description = String.IsNullOrEmpty(searchInfo.Snippet.Description) ? searchInfo.Snippet.Title : searchInfo.Snippet.Description;

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

        public String PlaylistId
        {
            get
            {               
                return (Info as SearchResult).Id.PlaylistId;   
            }
        }
    }
}
