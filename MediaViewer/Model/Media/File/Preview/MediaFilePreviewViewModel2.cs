using MediaViewer.Infrastructure;
using MediaViewer.MediaDatabase;
using MediaViewer.Model.Global.Events;
using MediaViewer.Model.Media.Base.Item;
using MediaViewer.UserControls.MediaPreview;
using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VideoLib;

namespace MediaViewer.Model.Media.File.Preview
{
    class MediaFilePreviewViewModel2 : MediaPreviewViewModelBase
    {
        IEventAggregator EventAggregator { get; set; }
           
        MediaItem Item { get; set; }

        MediaProbe MediaProbe { get; set; }

        public MediaFilePreviewViewModel2(IEventAggregator eventAggregator)            
        {
            EventAggregator = eventAggregator;

            EventAggregator.GetEvent<MediaSelectionEvent>().Subscribe(selectionEvent);
        
            MediaProbe = new MediaProbe();      
        }

      
        private void selectionEvent(MediaSelectionPayload selection)
        {
           
            if (selection.Items.Count == 0 || selection.Items.ElementAt(0).Metadata == null)
            {
                MediaPreviewImage = null;
                Item = null;
                return;
            }
       
            Item = selection.Items.ElementAt(0);
                  
            if (Item.Metadata.Thumbnail != null)
            {
                MediaPreviewImage = Item.Metadata.Thumbnail.Image;
            }
            else
            {
                if (Item.Metadata is AudioMetadata)
                {
                    MediaPreviewImage = AudioImage;
                }
                else
                {
                    MediaPreviewImage = ErrorImage;
                }
            }

            
        }

        public override void endVideoPreview()
        {
            if (Item == null || !(Item.Metadata is VideoMetadata))
            {
                return;
            }
                                                   
            MediaProbe.close();                              
        }

        public override MediaThumb getVideoPreviewThumbnail(double pos, CancellationToken token)
        {
            if (Item == null || !(Item.Metadata is VideoMetadata))
            {
                return null;
            }                            

            List<MediaThumb> mediaThumbs = MediaProbe.grabThumbnails(Constants.MAX_THUMBNAIL_WIDTH, Constants.MAX_THUMBNAIL_HEIGHT, 0, 1, pos,
                                token, 60, null);
                             
            return mediaThumbs.ElementAt(0);
                                                
        }

        public override void startVideoPreview(CancellationToken token)
        {

            if (Item == null || !(Item.Metadata is VideoMetadata))
            {
                return;
            }  
                                                                                         
            MediaProbe.open(Item.Location, token);                     
                                 
        }

        
    }    
}
