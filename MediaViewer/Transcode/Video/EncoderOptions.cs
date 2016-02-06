using MediaViewer.Infrastructure.Video.TranscodeOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Transcode.Video
{
    class EncoderOptions
    {

        public static void setVideoOptions(Dictionary<String, Object> options, VideoEncoderPresets preset, VideoEncoders encoder)
        {
            
            switch (encoder)
            {

                case VideoEncoders.none:
                    break;
                case VideoEncoders.libx264:
                    options.Add("preset", preset.ToString().ToLower());
                    break;
                case VideoEncoders.libx265:
                    options.Add("preset", preset.ToString().ToLower());
                    break;
                case VideoEncoders.libvpx:
                    options.Add("b", 1000000);
                    options.Add("crf", "5");
                    options.Add("qmin", "0");
                    options.Add("qmax", "50");
                    options.Add("threads", "3");
                    options.Add("slices", "2");
                    break;
                case VideoEncoders.libvpx_vp9:
                    options.Add("b", 1000000);
                    options.Add("threads", "8");
                    options.Add("speed", "1");
                    options.Add("tile-columns", "6");
                    options.Add("frame-parallel", "1");
                    options.Add("auto-alt-ref", "1");
                    options.Add("lag-in-frames", "25");
                    break;
                case VideoEncoders.msmpeg4:
                    options.Add("b", 2000000);
                    break;
                case VideoEncoders.wmv1:
                    options.Add("b", 2000000);
                    break;
                case VideoEncoders.wmv2:
                    options.Add("b", 2000000);
                    break;
                case VideoEncoders.gif:
                    break;
                case VideoEncoders.apng:
                    break;
                default:
                    break;
            }

        }


    }
}
