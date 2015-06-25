using MediaViewer.Model.Media.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.VideoPanel
{
    public class VideoAudioPair
    {
        public VideoAudioPair(MediaItem video, MediaItem audio)
        {
            Video = video;
            Audio = audio;
        }

        public MediaItem Video { get; set; }
        public MediaItem Audio { get; set; }

        public bool IsEmpty
        {
            get
            {
                return (Video == null && Audio == null);
            }

            set
            {
                if (value == true)
                {
                    Video = null;
                    Audio = null;
                }
            }
        }
    }
}
