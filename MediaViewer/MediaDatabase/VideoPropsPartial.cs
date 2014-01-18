using MediaViewer.MediaFileModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase
{
    partial class VideoProps
    {
        public VideoProps()
        {

        }

        public VideoProps(VideoFile video)
        {
            Height = video.Height;
            Width = video.Width;
            VideoCodec = video.VideoCodecName;
            VideoContainer = video.Container;
            DurationSeconds = video.DurationSeconds;
            FramesPerSecond = video.FrameRate;
            if (video.HasAudio)
            {
                NrChannels = (short)video.NrChannels;
                BitsPerSample = (short)(video.BytesPerSample * 8);
                SamplesPerSecond = (short)video.SamplesPerSecond;
            }

        }

          

    }
}
