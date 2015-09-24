using MediaViewer.Infrastructure.Logging;
using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.Streamed;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubePlugin.Item
{
    enum StreamType
    {
        STEREO_SCOPIC,
        VIDEO_AUDIO,
        AUDIO,
        VIDEO,
        UNKNOWN
    }

    class YoutubeVideoStreamedItem : MediaStreamedItem
    {
        public StreamType StreamType { get; protected set; }

        public YoutubeVideoStreamedItem(NameValueCollection info, String name) 
            : base(info["url"], name, MediaItemState.LOADED)
        {
            /*foreach (string v in info)
            {
                System.Diagnostics.Debug.Print(v + ": " + info[v]);
            }*/

            VideoMetadata videoMetadata = new VideoMetadata();
                                
            videoMetadata.MimeType = info["type"];
            if (videoMetadata.MimeType != null)
            {
                int pos = videoMetadata.MimeType.IndexOf(';');

                if (pos != -1)
                {
                    videoMetadata.MimeType = videoMetadata.MimeType.Substring(0, pos);
                }

            }
            //FallbackHost = info["fallback_host"];

            string fpsString = info["fps"];
            if (fpsString != null)
            {
                videoMetadata.FramesPerSecond = int.Parse(fpsString);
            }

            int iTag;

            bool success = int.TryParse(info["itag"], out iTag);

            if (success && itagFormatInfo.ContainsKey(iTag))
            {
                StreamFormatInfo formatInfo = itagFormatInfo[iTag];

                if (formatInfo.Width.HasValue && formatInfo.Height.HasValue)
                {
                    videoMetadata.Width = formatInfo.Width.Value;
                    videoMetadata.Height = formatInfo.Height.Value;
                }

                videoMetadata.SamplesPerSecond = formatInfo.AudioBitrate;

                StreamType = formatInfo.StreamType;               
            }
            else
            {
                StreamType = StreamType.UNKNOWN;
                
                if (success)
                {
                    Logger.Log.Error("Unknown itag in videostreaminfo: " + iTag);
#if DEBUG
                    throw new Exception("Unknown itag in videostreaminfo: " + iTag);
#endif
                }

            }

            Metadata = videoMetadata;
        }

        static Dictionary<int, StreamFormatInfo> itagFormatInfo;

        static YoutubeVideoStreamedItem()
        {

            itagFormatInfo = new Dictionary<int, StreamFormatInfo>();

            // video + audio

            itagFormatInfo[5] = new StreamFormatInfo(StreamType.VIDEO_AUDIO, 320, 240);
            itagFormatInfo[13] = new StreamFormatInfo(StreamType.VIDEO_AUDIO, 0, 0);
            itagFormatInfo[17] = new StreamFormatInfo(StreamType.VIDEO_AUDIO, 176, 144);
            itagFormatInfo[18] = new StreamFormatInfo(StreamType.VIDEO_AUDIO, 480, 360);
            itagFormatInfo[22] = new StreamFormatInfo(StreamType.VIDEO_AUDIO, 1280, 720);
            itagFormatInfo[34] = new StreamFormatInfo(StreamType.VIDEO_AUDIO, 480, 360);
            itagFormatInfo[35] = new StreamFormatInfo(StreamType.VIDEO_AUDIO, 640, 480);
            itagFormatInfo[36] = new StreamFormatInfo(StreamType.VIDEO_AUDIO, 320, 240);
            itagFormatInfo[37] = new StreamFormatInfo(StreamType.VIDEO_AUDIO, 1920, 1080);
            itagFormatInfo[38] = new StreamFormatInfo(StreamType.VIDEO_AUDIO, 2048, 1080);
            itagFormatInfo[43] = new StreamFormatInfo(StreamType.VIDEO_AUDIO, 480, 360);
            itagFormatInfo[44] = new StreamFormatInfo(StreamType.VIDEO_AUDIO, 640, 480);
            itagFormatInfo[45] = new StreamFormatInfo(StreamType.VIDEO_AUDIO, 1280, 720);
            itagFormatInfo[46] = new StreamFormatInfo(StreamType.VIDEO_AUDIO, 1920, 1080);

            // 3d
            itagFormatInfo[82] = new StreamFormatInfo(StreamType.STEREO_SCOPIC, 480, 360);
            itagFormatInfo[83] = new StreamFormatInfo(StreamType.STEREO_SCOPIC, 640, 480);
            itagFormatInfo[84] = new StreamFormatInfo(StreamType.STEREO_SCOPIC, 1280, 720);
            itagFormatInfo[85] = new StreamFormatInfo(StreamType.STEREO_SCOPIC, 1920, 1080);
            itagFormatInfo[100] = new StreamFormatInfo(StreamType.STEREO_SCOPIC, 480, 360);
            itagFormatInfo[101] = new StreamFormatInfo(StreamType.STEREO_SCOPIC, 640, 480);
            itagFormatInfo[102] = new StreamFormatInfo(StreamType.STEREO_SCOPIC, 1280, 720);

            // adaptive video only

            itagFormatInfo[133] = new StreamFormatInfo(StreamType.VIDEO, 320, 240);
            itagFormatInfo[134] = new StreamFormatInfo(StreamType.VIDEO, 480, 360);
            itagFormatInfo[135] = new StreamFormatInfo(StreamType.VIDEO, 640, 480);
            itagFormatInfo[136] = new StreamFormatInfo(StreamType.VIDEO, 1280, 720);
            itagFormatInfo[137] = new StreamFormatInfo(StreamType.VIDEO, 1920, 1080);
            itagFormatInfo[138] = new StreamFormatInfo(StreamType.VIDEO, 3840, 2160);
            itagFormatInfo[160] = new StreamFormatInfo(StreamType.VIDEO, 256, 144);
            itagFormatInfo[242] = new StreamFormatInfo(StreamType.VIDEO, 320, 240);
            itagFormatInfo[243] = new StreamFormatInfo(StreamType.VIDEO, 480, 360);
            itagFormatInfo[244] = new StreamFormatInfo(StreamType.VIDEO, 640, 480);
            itagFormatInfo[245] = new StreamFormatInfo(StreamType.VIDEO, 640, 480);
            itagFormatInfo[246] = new StreamFormatInfo(StreamType.VIDEO, 640, 480);
            itagFormatInfo[247] = new StreamFormatInfo(StreamType.VIDEO, 1280, 720);
            itagFormatInfo[248] = new StreamFormatInfo(StreamType.VIDEO, 1920, 1080);
            itagFormatInfo[264] = new StreamFormatInfo(StreamType.VIDEO, 2560, 1440);
            itagFormatInfo[266] = new StreamFormatInfo(StreamType.VIDEO, 3840, 2160);
            itagFormatInfo[271] = new StreamFormatInfo(StreamType.VIDEO, 2560, 1440);
            itagFormatInfo[272] = new StreamFormatInfo(StreamType.VIDEO, 3840, 2160);
            itagFormatInfo[278] = new StreamFormatInfo(StreamType.VIDEO, 256, 144);
            itagFormatInfo[298] = new StreamFormatInfo(StreamType.VIDEO, 1280, 720);
            itagFormatInfo[299] = new StreamFormatInfo(StreamType.VIDEO, 1920, 1080);
            itagFormatInfo[302] = new StreamFormatInfo(StreamType.VIDEO, 1280, 720);
            itagFormatInfo[303] = new StreamFormatInfo(StreamType.VIDEO, 1920, 1080);
            itagFormatInfo[313] = new StreamFormatInfo(StreamType.VIDEO, 3840, 2160);
            itagFormatInfo[315] = new StreamFormatInfo(StreamType.VIDEO, 3840, 2160); 

            // adaptive audio only

            // aac
            itagFormatInfo[139] = new StreamFormatInfo(StreamType.AUDIO, null, null, 48);
            itagFormatInfo[140] = new StreamFormatInfo(StreamType.AUDIO, null, null, 128);
            itagFormatInfo[141] = new StreamFormatInfo(StreamType.AUDIO, null, null, 256);

            // webm
            itagFormatInfo[171] = new StreamFormatInfo(StreamType.AUDIO, null, null, 128);
            itagFormatInfo[172] = new StreamFormatInfo(StreamType.AUDIO, null, null, 256);
            itagFormatInfo[249] = new StreamFormatInfo(StreamType.AUDIO, null, null, 50);
            itagFormatInfo[250] = new StreamFormatInfo(StreamType.AUDIO, null, null, 70);
            itagFormatInfo[251] = new StreamFormatInfo(StreamType.AUDIO, null, null, 160);

        }

        class StreamFormatInfo
        {
            public StreamFormatInfo(StreamType streamType, int? width = null, int? height = null, int? audioBitrate = null)
            {
                Width = width;
                Height = height;
                StreamType = streamType;
                AudioBitrate = audioBitrate;
            }

            public int? Width { get; set; }
            public int? Height { get; set; }
            public int? AudioBitrate { get; set; }
            public StreamType StreamType { get; set; }
        }

    }
}
