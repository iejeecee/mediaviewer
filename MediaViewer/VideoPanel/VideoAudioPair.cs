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
        public VideoAudioPair(MediaItem video, MediaItem audio = null)
        {
            Video = video;
            Audio = audio;
        }

        public MediaItem Video { get; set; }
        public MediaItem Audio { get; set; }

        public string Location
        {
            get
            {
                if (IsEmpty == false)
                {
                    return Video != null ? Video.Location : Audio.Location;
                }
                else
                {
                    return (null);
                }
            }
        }

        public string Name
        {
            get
            {
                if (IsEmpty == false)
                {
                    return Video != null ? Video.Name : Audio.Name;
                }
                else
                {
                    return (null);
                }
            }
        }

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
