using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Infrastructure.Video.TranscodeOptions
{
    public enum ContainerFormats
    {
        MP4 = 0,
        MOV,
        AVI,
        MKV,
        FLV,
        WMV,
        WEBM,
        GIF,
        PNG,
        MP3,
        FLAC,
        M4A,
        OGG
    }

    public enum StreamOptions
    {
        Discard,
        Copy,
        Encode
    }

    public enum VideoEncoders
    {
        none = 0,
        libx264,
        libx265,
        libvpx,
        libvpx_vp9,
        msmpeg4,
        wmv1,
        wmv2,
        gif,
        apng
    }

    public enum ImageEncoders
    {
        none = 0,
        jpegls,
        jpeg2000,
        webp
    }

    public enum VideoEncoderPresets
    {
        UltraFast, 
        SuperFast, 
        VeryFast, 
        Faster, 
        Fast, 
        Medium, 
        Slow, 
        Slower, 
        VerySlow
    }

    public enum AudioEncoders
    {
        none = 0,
        libvo_aacenc,      
        libmp3lame,
        libvorbis,
        wmav1,
        wmav2,
        flac,
        libopus
    }
}
