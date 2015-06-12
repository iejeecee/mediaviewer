using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubePlugin.Item
{
    enum StreamFlags
    {
        STEREO_SCOPIC,
        AUDIO_ONLY,
        VIDEO_ONLY
    }

    class VideoStreamInfo
    {

        public VideoStreamInfo(NameValueCollection info)
        {
            ITag = int.Parse(info["itag"]);
            Quality = info["quality"];
            Type = info["type"];
            Url = info["url"];
            FallbackHost = info["fallback_host"];

            if (itagFormatInfo.ContainsKey(ITag))
            {
                StreamFormatInfo formatInfo = itagFormatInfo[ITag];

                if (formatInfo != null)
                {
                    Width = formatInfo.Width;
                    Height = formatInfo.Height;
                    Flags = formatInfo.Flags;
                }
            }
            else
            {
                throw new Exception("Unknown itag in videostreaminfo: " + ITag);
            }

        }

        public int ITag { get; set; }
        public String Quality { get; set; }
        public String Type { get; set; }
        public String Url { get; set; }
        public String FallbackHost { get; set; }
        public int? Width;
        public int? Height;
        StreamFlags? Flags;

        static Dictionary<int, StreamFormatInfo> itagFormatInfo;

        static VideoStreamInfo()
        {

            itagFormatInfo = new Dictionary<int, StreamFormatInfo>();

            itagFormatInfo[5] = new StreamFormatInfo(320, 240);
            itagFormatInfo[17] = new StreamFormatInfo(176, 144);
            itagFormatInfo[18] = new StreamFormatInfo(480, 360);
            itagFormatInfo[22] = new StreamFormatInfo(1280, 720);
            itagFormatInfo[34] = new StreamFormatInfo(480, 360);
            itagFormatInfo[35] = new StreamFormatInfo(640, 480);
            itagFormatInfo[36] = new StreamFormatInfo(320, 240);
            itagFormatInfo[37] = new StreamFormatInfo(1920, 1080);
            itagFormatInfo[38] = new StreamFormatInfo(2048, 1080);
            itagFormatInfo[43] = new StreamFormatInfo(480, 360);
            itagFormatInfo[44] = new StreamFormatInfo(640, 480);
            itagFormatInfo[45] = new StreamFormatInfo(1280, 720);
            itagFormatInfo[46] = new StreamFormatInfo(1920, 1080);

            // progressive

            itagFormatInfo[82] = new StreamFormatInfo(480, 360, null, StreamFlags.STEREO_SCOPIC);
            itagFormatInfo[83] = new StreamFormatInfo(640, 480, null, StreamFlags.STEREO_SCOPIC);
            itagFormatInfo[84] = new StreamFormatInfo(1280, 720, null, StreamFlags.STEREO_SCOPIC);
            itagFormatInfo[85] = new StreamFormatInfo(1920, 1080, null, StreamFlags.STEREO_SCOPIC);
            itagFormatInfo[100] = new StreamFormatInfo(480, 360, null, StreamFlags.STEREO_SCOPIC);
            itagFormatInfo[101] = new StreamFormatInfo(640, 480, null, StreamFlags.STEREO_SCOPIC);
            itagFormatInfo[102] = new StreamFormatInfo(1280, 720, null, StreamFlags.STEREO_SCOPIC);
            itagFormatInfo[133] = new StreamFormatInfo(320, 240, null, StreamFlags.VIDEO_ONLY);
            itagFormatInfo[134] = new StreamFormatInfo(480, 360, null, StreamFlags.VIDEO_ONLY);
            itagFormatInfo[135] = new StreamFormatInfo(640, 480, null, StreamFlags.VIDEO_ONLY);
            itagFormatInfo[136] = new StreamFormatInfo(1280, 720, null, StreamFlags.VIDEO_ONLY);
            itagFormatInfo[137] = new StreamFormatInfo(1920, 1080, null, StreamFlags.VIDEO_ONLY);            
            itagFormatInfo[160] = new StreamFormatInfo(256, 144, null, StreamFlags.VIDEO_ONLY);            
            itagFormatInfo[242] = new StreamFormatInfo(320, 240, null, StreamFlags.VIDEO_ONLY);
            itagFormatInfo[243] = new StreamFormatInfo(480, 360, null, StreamFlags.VIDEO_ONLY);
            itagFormatInfo[244] = new StreamFormatInfo(640, 480, null, StreamFlags.VIDEO_ONLY);
            itagFormatInfo[245] = new StreamFormatInfo(640, 480, null, StreamFlags.VIDEO_ONLY);
            itagFormatInfo[246] = new StreamFormatInfo(640, 480, null, StreamFlags.VIDEO_ONLY);
            itagFormatInfo[247] = new StreamFormatInfo(1280, 720, null, StreamFlags.VIDEO_ONLY);
            itagFormatInfo[248] = new StreamFormatInfo(1920, 1080, null, StreamFlags.VIDEO_ONLY);            
            itagFormatInfo[264] = new StreamFormatInfo(2560, 1440, null, StreamFlags.VIDEO_ONLY);
            itagFormatInfo[266] = new StreamFormatInfo(3840, 2160, null, StreamFlags.VIDEO_ONLY);
            itagFormatInfo[271] = new StreamFormatInfo(2560, 1440, null, StreamFlags.VIDEO_ONLY);
            itagFormatInfo[272] = new StreamFormatInfo(3840, 2160, null, StreamFlags.VIDEO_ONLY);
            itagFormatInfo[278] = new StreamFormatInfo(256, 144, null, StreamFlags.VIDEO_ONLY);
            itagFormatInfo[298] = new StreamFormatInfo(1280, 720, null, StreamFlags.VIDEO_ONLY);
            itagFormatInfo[299] = new StreamFormatInfo(1920, 1080, null, StreamFlags.VIDEO_ONLY);
            itagFormatInfo[302] = new StreamFormatInfo(1280, 720, null, StreamFlags.VIDEO_ONLY);
            itagFormatInfo[303] = new StreamFormatInfo(1920, 1080, null, StreamFlags.VIDEO_ONLY);

            // audio

            itagFormatInfo[139] = new StreamFormatInfo(null, null, "Low bitrate", StreamFlags.AUDIO_ONLY);
            itagFormatInfo[140] = new StreamFormatInfo(null, null, "Med bitrate", StreamFlags.AUDIO_ONLY);
            itagFormatInfo[141] = new StreamFormatInfo(null, null, "Hi  bitrate", StreamFlags.AUDIO_ONLY);
            itagFormatInfo[171] = new StreamFormatInfo(null, null, "Med bitrate", StreamFlags.AUDIO_ONLY);
            itagFormatInfo[172] = new StreamFormatInfo(null, null, "Hi  bitrate", StreamFlags.AUDIO_ONLY);
            itagFormatInfo[249] = new StreamFormatInfo(null, null, "Low bitrate", StreamFlags.AUDIO_ONLY);
            itagFormatInfo[250] = new StreamFormatInfo(null, null, "Med bitrate", StreamFlags.AUDIO_ONLY);
            itagFormatInfo[251] = new StreamFormatInfo(null, null, "Hi  bitrate", StreamFlags.AUDIO_ONLY);

        }

        class StreamFormatInfo
        {
            public StreamFormatInfo(int? width = null, int? height = null, string info = null, StreamFlags? flags = null)
            {
                Width = width;
                Height = height;
                Flags = flags;
                Info = info;
            }

            public int? Width { get; set; }
            public int? Height { get; set; }
            public string Info { get; set; }
            public StreamFlags? Flags { get; set; }
        }
    }


}
