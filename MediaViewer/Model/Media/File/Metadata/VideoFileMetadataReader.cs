using MediaViewer.Infrastructure;
using MediaViewer.Infrastructure.Logging;
using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.Base.Metadata;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using VideoLib;

namespace MediaViewer.Model.Media.File.Metadata
{
    class VideoFileMetadataReader : MetadataFileReader
    {

        public override void readMetadata(MediaProbe mediaProbe, Stream data, MetadataFactory.ReadOptions options, BaseMetadata media,
            CancellationToken token, int timeoutSeconds)
        {
            VideoMetadata video = media as VideoMetadata;

            video.DurationSeconds = mediaProbe.DurationSeconds;
            video.SizeBytes = mediaProbe.SizeBytes;

            video.Width = mediaProbe.Width;
            video.Height = mediaProbe.Height;

            video.VideoContainer = mediaProbe.Container;
            video.VideoCodec = mediaProbe.VideoCodecName;

            video.PixelFormat = mediaProbe.PixelFormat;
            video.BitsPerPixel = (short)mediaProbe.BitsPerPixel;

            video.FramesPerSecond = mediaProbe.FrameRate;

            video.VideoBitRate = mediaProbe.VideoBitRate == 0 ? (long?)null : mediaProbe.VideoBitRate;
            video.AudioBitRate = mediaProbe.AudioBitRate == 0 ? (long?)null : mediaProbe.AudioBitRate;

            video.MimeType = MediaFormatConvert.fileNameToMimeType(media.Name); 

            if (!String.IsNullOrEmpty(mediaProbe.AudioCodecName))
            {
                video.AudioCodec = mediaProbe.AudioCodecName;
                video.SamplesPerSecond = mediaProbe.SamplesPerSecond;
                video.BitsPerSample = (short)(mediaProbe.BytesPerSample * 8);
                video.NrChannels = (short)mediaProbe.NrChannels;
            }
            else
            {
                video.AudioCodec = null;
                video.SamplesPerSecond = null;
                video.BitsPerSample = null;
                video.NrChannels = null;
            }

            List<String> fsMetaData = mediaProbe.MetaData;

            try
            {
                if (options.HasFlag(MetadataFactory.ReadOptions.GENERATE_THUMBNAIL))
                {
                    generateThumbnail(mediaProbe, video, token, timeoutSeconds, 1);
                }
               
            }
            catch (Exception e)
            {
                Logger.Log.Error("Cannot create video thumbnail: " + video.Location, e);
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

            base.readMetadata(mediaProbe, data, options, media, token, timeoutSeconds);

            parseFFMpegMetaData(fsMetaData, video);


        }

        public void generateThumbnail(MediaProbe mediaProbe, VideoMetadata video,
            CancellationToken token, int timeoutSeconds, int nrThumbnails)
        {
            video.Thumbnail = null;
         
            List<MediaThumb> coverBitmaps = mediaProbe.grabAttachedImages(Constants.MAX_THUMBNAIL_WIDTH,
                 Constants.MAX_THUMBNAIL_HEIGHT, token, timeoutSeconds);

            if (coverBitmaps.Count > 0)
            {
                video.Thumbnail = new Thumbnail(coverBitmaps[0].Thumb);
            }

            if (coverBitmaps.Count == 0 || nrThumbnails > 1)
            {

                List<MediaThumb> thumbBitmaps = mediaProbe.grabThumbnails(Constants.MAX_THUMBNAIL_WIDTH,
                     Constants.MAX_THUMBNAIL_HEIGHT, 0, nrThumbnails, 0.025, token, timeoutSeconds, null);
              
                if (thumbBitmaps.Count > 0 && coverBitmaps.Count == 0)
                {
                    video.Thumbnail = new Thumbnail(thumbBitmaps[0].Thumb);
                }
                
            }

        }

        static List<String> encoderMatch = new List<String>() { "encoder", "wm/toolname", "encoded_with", "encoded_by" };
        static List<String> descriptionMatch = new List<string>() { "description", "comment" };
        static List<String> authorMatch = new List<string>() { "artist", "author" };

        void parseFFMpegMetaData(List<string> fsMetaData, VideoMetadata video)
        {
            if (fsMetaData == null) return;

            foreach (String info in fsMetaData)
            {
                string[] temp = info.Split(new char[] { ':' }, 2);

                if (temp != null)
                {
                    String param = temp[0].ToLower();
                    String value = temp[1].Trim();

                    if (String.IsNullOrEmpty(value) || String.IsNullOrWhiteSpace(value)) continue;

                    // Note that when setting the title like this, if the user clears the (XMP) title it will 
                    // revert to the title stored in the ffmpeg metadata. This will be confusing for the user
                    // and should probably be fixed.
                    if (video.Title == null && param.Equals("title"))
                    {
                        video.Title = value;
                    }
                    else if (video.Description == null && descriptionMatch.Any(s => s.Equals(param)))
                    {
                        video.Description = value;
                    }
                    else if (video.Author == null && authorMatch.Any(s => s.Equals(param)))
                    {
                        video.Author = value;
                    }
                    else if (video.Copyright == null && param.Equals("copyright"))
                    {
                        video.Copyright = value;
                    }
                    else if (video.Software == null && encoderMatch.Any(s => s.Equals(param)))
                    {
                        video.Software = value;
                    }
                    else if (param.Equals("major_brand"))
                    {
                        video.MajorBrand = value;
                    }
                    else if (param.Equals("minor_version"))
                    {
                        int minorVersion;
                        if (int.TryParse(value, out minorVersion))
                        {

                            video.MinorVersion = minorVersion;
                        }
                    }
                    else if (param.Equals("wmfsdkversion"))
                    {
                        video.WMFSDKVersion = value;
                    }
                    else if (param.Equals("isvbr"))
                    {
                        bool isVBR;
                        if (bool.TryParse(value, out isVBR))
                        {
                            video.IsVariableBitRate = isVBR;
                        }
                    }

                }
            }
        }

        private bool supportsXMPMetadata(VideoMetadata video, List<string> fsMetaData)
        {

            // XMP Metadata does not support matroska
            if (video.MimeType.Equals("video/x-matroska") || video.MimeType.Equals("video/webm"))
            {
                return (false);
            }
            else if (video.MimeType.Equals("video/mp4"))
            {
                // mp4 versions incompatible with XMP metadata

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
