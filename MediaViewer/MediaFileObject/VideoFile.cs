using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaViewer.MetaData;
using MediaViewer.Utils;
using VideoLib;

namespace MediaViewer.MediaFileObject
{
    class VideoFile : MediaFile
    {

        private static Image defaultVideoThumb;

        private VideoPreview videoPreview;

        private int durationSeconds;

        private int width;
        private int height;

        private string container;
        private string videoCodecName;
        private List<string> fsMetaData;

        private float frameRate;

        private string audioCodecName;
        private int samplesPerSecond;
        private int bytesPerSample;
        private int nrChannels;

        private bool videoSupportsXMPMetaData()
        {

            // XMP Metadata does not support matroska
            if (MimeType.Equals("video/x-matroska"))
            {

                return (false);

                // mp4 versions incompatible with XMP metadata
            }
            else if (mimeType.Equals("video/mp4"))
            {


                if (FSMetaData.Contains("major_brand: isom") &&
                    FSMetaData.Contains("minor_version: 1"))
                {
                    return (false);
                }

                if (FSMetaData.Contains("major_brand: mp42") &&
                    FSMetaData.Contains("minor_version: 0"))
                {

                    if (FSMetaData.Contains("compatible_brands: isom"))
                    {
                        return (false);
                    }

                    if (FSMetaData.Contains("compatible_brands: 000000964375"))
                    {
                        return (false);
                    }
                }

            }
            else if (mimeType.Equals("video/avi"))
            {

                if (VideoCodecName.Equals("mpeg2video"))
                {

                    return (false);
                }
            }

            return (true);
        }



        protected override void readMetaData()
        {

            if (videoPreview == null)
            {

                videoPreview = new VideoPreview();
            }

            try
            {

                videoPreview.open(Location);

                durationSeconds = videoPreview.DurationSeconds;
                sizeBytes = videoPreview.SizeBytes;

                width = videoPreview.Width;
                height = videoPreview.Height;

                container = videoPreview.Container;
                videoCodecName = videoPreview.VideoCodecName;
                fsMetaData = videoPreview.MetaData;

                frameRate = videoPreview.FrameRate;

                audioCodecName = videoPreview.AudioCodecName;
                samplesPerSecond = videoPreview.SamplesPerSecond;
                bytesPerSample = videoPreview.BytesPerSample;
                nrChannels = videoPreview.NrChannels;

                if (videoSupportsXMPMetaData())
                {

                    base.readMetaData();

                }
                else
                {

                    metaDataError = new Exception("Metadata not supported for this format");
                }

            }
            catch (Exception e)
            {

                log.Error("Cannot read video meta data: " + Location, e);
                videoPreview.close();
            }
        }



        static VideoFile()
        {

            defaultVideoThumb = new Bitmap("C:\\game\\icons\\video.png");
        }

        public VideoFile(string location, string mimeType, Stream data, MediaFile.MetaDataMode mode)
            : base(location, mimeType, data, mode)
        {


        }

        public override MediaType MediaFormat
        {
            get
            {

                return (MediaType.VIDEO);
            }
        }

        public int Width
        {

            get
            {

                return (width);
            }
        }

        public int Height
        {

            get
            {

                return (height);
            }
        }

        public int DurationSeconds
        {

            get
            {

                return (durationSeconds);
            }
        }

        public string Container
        {

            get
            {

                return (container);
            }
        }

        public string VideoCodecName
        {

            get
            {

                return (videoCodecName);
            }
        }

        public List<string> FSMetaData
        {

            get
            {

                return (fsMetaData);
            }
        }

        public float FrameRate
        {

            get
            {

                return (frameRate);
            }
        }

        public bool HasAudio
        {

            get
            {

                return (!string.IsNullOrEmpty(AudioCodecName));
            }
        }

        public string AudioCodecName
        {

            get
            {

                return (audioCodecName);
            }
        }

        public int SamplesPerSecond
        {

            get
            {

                return (samplesPerSecond);
            }
        }

        public int BytesPerSample
        {

            get
            {

                return (bytesPerSample);
            }
        }

        public int NrChannels
        {

            get
            {

                return (nrChannels);
            }
        }


        protected override List<MetaDataThumb> generateThumbnails()
        {

            List<MetaDataThumb> thumbs = new List<MetaDataThumb>();

            List<Bitmap> thumbBitmaps = videoPreview.grabThumbnails(MAX_THUMBNAIL_WIDTH,
                MAX_THUMBNAIL_HEIGHT, -1, 1, 0.025);

            if (thumbBitmaps.Count == 0)
            {

                // possibly could not seek in video, try to get the first frame in the video
                thumbBitmaps = videoPreview.grabThumbnails(MAX_THUMBNAIL_WIDTH,
                    MAX_THUMBNAIL_HEIGHT, -1, 1, 0);
            }

            foreach (Bitmap bitmap in thumbBitmaps)
            {

                thumbs.Add(new MetaDataThumb(bitmap));
            }

            return (thumbs);
        }

        public override string getDefaultCaption()
        {

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(Path.GetFileName(Location));
            sb.AppendLine();


            /*
            foreach(string info in FSMetaData) {

            sb.AppendLine(info);
            }
            */

            if (MetaData != null)
            {

                if (!string.IsNullOrEmpty(MetaData.Description))
                {

                    sb.AppendLine("Description:");

                    //string temp = System.Text.RegularExpressions.Regex.Replace(MetaData.Description,"(.{50}\\s)","$1`n");
                    sb.AppendLine(MetaData.Description);
                    sb.AppendLine();
                }

                if (!string.IsNullOrEmpty(MetaData.Creator))
                {

                    sb.AppendLine("Creator:");
                    sb.AppendLine(MetaData.Creator);
                    sb.AppendLine();

                }

                if (MetaData.CreationDate != DateTime.MinValue)
                {

                    sb.AppendLine("Creation date:");
                    sb.Append(MetaData.CreationDate);
                    sb.AppendLine();
                }
            }

            return (sb.ToString());
        }

        public override string getDefaultFormatCaption()
        {

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(Path.GetFileName(Location));
            sb.AppendLine();

            sb.AppendLine("Mime type:");
            sb.Append(MimeType);
            sb.AppendLine();
            sb.AppendLine();

            sb.Append("Video Codec (");
            sb.Append(VideoCodecName);
            sb.AppendLine("):");
            sb.Append(width);
            sb.Append("x");
            sb.Append(height);
            sb.Append(", " + videoPreview.PixelFormat + ", " + videoPreview.FrameRate.ToString() + " fps");
            sb.AppendLine();
            sb.AppendLine();

            if (HasAudio == true)
            {

                sb.Append("Audio Codec (");
                sb.Append(AudioCodecName);
                sb.AppendLine("):");
                sb.Append(SamplesPerSecond);
                sb.Append("Hz, ");
                sb.Append(bytesPerSample * 8);
                sb.Append("bit, ");
                sb.Append(NrChannels);
                sb.Append(" chan");
                sb.AppendLine();
                sb.AppendLine();
            }

            sb.AppendLine("Duration:");
            sb.AppendLine(Misc.formatTimeSeconds(DurationSeconds));
            sb.AppendLine();

            sb.AppendLine("Size");
            sb.AppendLine(Misc.formatSizeBytes(SizeBytes));
            sb.AppendLine();

            return (sb.ToString());
        }

        public override void close()
        {

            videoPreview.close();

            base.close();
        }
    }
}
