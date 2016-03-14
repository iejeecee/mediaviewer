using MediaViewer.Infrastructure;
using MediaViewer.MediaDatabase;
using MediaViewer.UserControls.MediaPreview;
using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VideoLib;
using YoutubePlugin.Events;
using YoutubePlugin.Item;

namespace YoutubePlugin.Preview
{
    class YoutubePreviewViewModel : MediaPreviewViewModelBase
    {
 
        IEventAggregator EventAggregator { get; set; }
           
        YoutubeItem Item { get; set; }

        MediaProbe MediaProbe { get; set; }

        public YoutubePreviewViewModel(IEventAggregator eventAggregator)            
        {
            EventAggregator = eventAggregator;

            EventAggregator.GetEvent<SelectionEvent>().Subscribe(selectionEvent);
        
            MediaProbe = new MediaProbe();      
        }

      
        private void selectionEvent(ICollection<YoutubeItem> items)
        {
           
            if (items.Count == 0 || items.Count > 1)
            {
                MediaPreviewImage = null;
                return;
            }

            Item = items.ElementAt(0);

            if (Item.Metadata == null || Item.Metadata.Thumbnail == null)
            {
                MediaPreviewImage = ErrorImage;
                return;
            }

            MediaPreviewImage = Item.Metadata.Thumbnail.Image;
        }

        public override void endVideoPreview()
        {
            if (!(Item is YoutubeVideoItem))
            {
                return;
            }
                                                   
            MediaProbe.close();                              
        }

        public override MediaThumb getVideoPreviewThumbnail(double pos, CancellationToken token)
        {
            if (!(Item is YoutubeVideoItem))
            {
                return null;
            }
         
            List<MediaThumb> mediaThumbs = MediaProbe.grabThumbnails(Constants.MAX_THUMBNAIL_WIDTH, Constants.MAX_THUMBNAIL_HEIGHT, 0, 1, pos,
                                token, 60, null);
                             
            return mediaThumbs.ElementAt(0);
                                                
        }

        public override void startVideoPreview(CancellationToken token)
        {
         
            if (!(Item is YoutubeVideoItem))
            {
                return;
            }
                                                                              
            YoutubeVideoStreamedItem video, audio;

            YoutubeVideoItem videoItem = Item as YoutubeVideoItem;

            videoItem.getStreams(out video, out audio, 500);

            if (video != null)
            {
                MediaProbe.open(video.Location, token);                     
            }                        
        }

        
    }

    
}
