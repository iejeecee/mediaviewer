using MediaViewer.MediaDatabase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using VideoLib;

namespace MediaViewer.MediaFileModel
{
    class VideoMetadataReader : MetadataReader
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void readMetadata(Stream data, MediaFactory.ReadOptions options, Media media)
        {
            VideoMedia video = media as VideoMedia;

            VideoPreview videoPreview = null;
            List<String> fsMetaData = null;

            try
            {
                try
                {

                    videoPreview = new VideoPreview();
                    videoPreview.open(media.Location);

                    video.DurationSeconds = videoPreview.DurationSeconds;
                    video.SizeBytes = videoPreview.SizeBytes;

                    video.Width = videoPreview.Width;
                    video.Height = videoPreview.Height;

                    video.VideoContainer = videoPreview.Container;
                    video.VideoCodec = videoPreview.VideoCodecName;

                    video.PixelFormat = videoPreview.PixelFormat;

                    video.FramesPerSecond = videoPreview.FrameRate;

                    video.AudioCodec = videoPreview.AudioCodecName;
                    video.SamplesPerSecond = videoPreview.SamplesPerSecond;
                    video.BitsPerSample = (short)(videoPreview.BytesPerSample * 8);
                    video.NrChannels = (short)videoPreview.NrChannels;
                    fsMetaData = videoPreview.MetaData;

                }
                catch (Exception e)
                {
                    log.Error("FFMPG cannot read video file: " + video.Location, e);
                    media.MetadataReadError = e;
                }

                try
                {
                    if (options.HasFlag(MediaFactory.ReadOptions.GENERATE_THUMBNAIL) && videoPreview != null)
                    {
                        generateThumbnail(videoPreview, video);
                    }
                }
                catch (Exception e)
                {
                    log.Error("Cannot create video thumbnail: " + video.Location, e);
                    media.MetadataReadError = e;
                }
                
                if (fsMetaData != null)
                {
                    video.SupportsXMPMetadata = supportsXMPMetadata(video, fsMetaData);
                }
                else
                {
                    video.SupportsXMPMetadata = false;
                }

                base.readMetadata(data, options, media);
            
            }
            finally
            {
                if (videoPreview != null)
                {
                    videoPreview.close();
                    videoPreview.Dispose();
                }
            }
        }

         public void generateThumbnail(VideoPreview videoPreview, VideoMedia video)
         {

             List<BitmapSource> thumbBitmaps = videoPreview.grabThumbnails(MAX_THUMBNAIL_WIDTH,
                 MAX_THUMBNAIL_HEIGHT, -1, 1, 0.025);

             if (thumbBitmaps.Count == 0)
             {

                 // possibly could not seek in video, try to get the first frame in the video
                 thumbBitmaps = videoPreview.grabThumbnails(MAX_THUMBNAIL_WIDTH,
                     MAX_THUMBNAIL_HEIGHT, -1, 1, 0);
             }

             if (thumbBitmaps.Count > 0)
             {
                 video.Thumbnail = new Thumbnail(thumbBitmaps[0]);
             }
             else
             {
                 video.Thumbnail = null;
             }
           
         }

         private bool supportsXMPMetadata(VideoMedia video, List<string> fsMetaData)
         {
             // XMP Metadata does not support matroska
             if (video.MimeType.Equals("video/x-matroska"))
             {

                 return (false);

                 // mp4 versions incompatible with XMP metadata
             }
             else if (video.MimeType.Equals("video/mp4"))
             {


                 if (fsMetaData.Contains("major_brand: isom") &&
                     fsMetaData.Contains("minor_version: 1"))
                 {
                     return (false);
                 }

                 if (fsMetaData.Contains("major_brand: mp42") &&
                     fsMetaData.Contains("minor_version: 0"))
                 {

                     if (fsMetaData.Contains("compatible_brands: isom"))
                     {
                         return (false);
                     }

                     if (fsMetaData.Contains("compatible_brands: 000000964375"))
                     {
                         return (false);
                     }
                 }

             }
             else if (video.MimeType.Equals("video/avi"))
             {

                 if (video.VideoCodec.Equals("mpeg2video"))
                 {

                     return (false);
                 }
             }

             return (true);
         }
    }
}
