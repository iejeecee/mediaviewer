using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.File;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.ImageGrid
{

    class MediaNameComparer : IComparer
    {
        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = System.IO.Path.GetFileName(x.Location).CompareTo(System.IO.Path.GetFileName(y.Location));
            return (result);
        }
    }

    class MediaSizeBytesComparer : IComparer
    {
        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            return (x.Media.SizeBytes.CompareTo(y.Media.SizeBytes));
        }
    }

    class MediaRatingComparer : IComparer
    {
        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            return (Nullable.Compare(x.Media.Rating, y.Media.Rating));
        }
    }

    class MediaImportedComparer : IComparer
    {
        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            return (x.Media.IsImported.CompareTo(y.Media.IsImported));
        }
    }

    class MediaNrTagsComparer : IComparer
    {
        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            return (x.Media.Tags.Count.CompareTo(y.Media.Tags.Count));
        }
    }

    class MediaFileDateComparer : IComparer
    {
        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            return (x.Media.FileDate.CompareTo(y.Media.FileDate));
        }
    }

    class MediaMimeTypeComparer : IComparer
    {
        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            return (x.Media.MimeType.CompareTo(y.Media.MimeType));
        }
    
    }

    class MediaLastModifiedComparer : IComparer
    {
        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            return (x.Media.LastModifiedDate.CompareTo(y.Media.LastModifiedDate));
        }
    }

    class MediaCreationDateComparer : IComparer
    {
        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            return (Nullable.Compare(x.Media.CreationDate, y.Media.CreationDate));
        }
    }

    class MediaSoftWareComparer : IComparer
    {
        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            return (MediaComparerUtils.Compare(x.Media.Software, y.Media.Software));
        }
    }

    class MediaWidthComparer : IComparer
    {
        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            int widthA;
            int widthB;

            if (x.Media is ImageMedia)
            {
                widthA = (x.Media as ImageMedia).Width;
            }
            else
            {
                widthA = (x.Media as VideoMedia).Width;
            }

            if (y.Media is ImageMedia)
            {
                widthB = (y.Media as ImageMedia).Width;
            }
            else
            {
                widthB = (y.Media as VideoMedia).Width;
            }

            return (widthA.CompareTo(widthB));
        }
    }

    class MediaHeightComparer : IComparer
    {
        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            int heightA;
            int heightB;

            if (x.Media is ImageMedia)
            {
                heightA = (x.Media as ImageMedia).Height;
            }
            else
            {
                heightA = (x.Media as VideoMedia).Height;
            }

            if (y.Media is ImageMedia)
            {
                heightB = (y.Media as ImageMedia).Height;
            }
            else
            {
                heightB = (y.Media as VideoMedia).Height;
            }

            return (heightA.CompareTo(heightB));
        }
    }
    class MediaDurationComparer : IComparer
    {
        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            Nullable<int> aDuration = x.Media is VideoMedia ? new Nullable<int>((x.Media as VideoMedia).DurationSeconds) : null;
            Nullable<int> bDuration = y.Media is VideoMedia ? new Nullable<int>((y.Media as VideoMedia).DurationSeconds) : null;

            return (Nullable.Compare<int>(aDuration, bDuration));
        }
    }

    class MediaFramesPerSecondComparer : IComparer
    {
        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            Nullable<double> aFPS = x.Media is VideoMedia ? new Nullable<double>((x.Media as VideoMedia).FramesPerSecond) : null;
            Nullable<double> bFPS = y.Media is VideoMedia ? new Nullable<double>((y.Media as VideoMedia).FramesPerSecond) : null;

            return (Nullable.Compare<double>(aFPS, bFPS));
        }
    }

    class MediaVideoCodecComparer : IComparer
    {
        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            String aVideoCodec = x.Media is VideoMedia ? (x.Media as VideoMedia).VideoCodec : "";
            String bVideoCodec = y.Media is VideoMedia ? (y.Media as VideoMedia).VideoCodec : "";

            return (aVideoCodec.CompareTo(bVideoCodec));
        }
    }

    class MediaAudioComparer : IComparer
    {
        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            String aAudioCodec = x.Media is VideoMedia ? (x.Media as VideoMedia).AudioCodec : null;
            String bAudioCodec = y.Media is VideoMedia ? (y.Media as VideoMedia).AudioCodec : null;

            return (MediaComparerUtils.Compare(aAudioCodec, bAudioCodec));
        }
    }

    class MediaPixelFormatComparer : IComparer
    {
        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            String aPixelFormat = x.Media is VideoMedia ? (x.Media as VideoMedia).PixelFormat : null;
            String bPixelFormat = y.Media is VideoMedia ? (y.Media as VideoMedia).PixelFormat : null;

            return (MediaComparerUtils.Compare(aPixelFormat, bPixelFormat));
        }
    }

    class MediaBitsPerSampleComparer : IComparer
    {

        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            Nullable<short> aBPS = x.Media is VideoMedia ? (x.Media as VideoMedia).BitsPerSample : null;
            Nullable<short> bBPS = y.Media is VideoMedia ? (y.Media as VideoMedia).BitsPerSample : null;

            return (Nullable.Compare(aBPS, bBPS));
        }
    }
    class MediaSamplesPerSecondComparer : IComparer
    {

        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            Nullable<int> aSPS = x.Media is VideoMedia ? (x.Media as VideoMedia).SamplesPerSecond : null;
            Nullable<int> bSPS = y.Media is VideoMedia ? (y.Media as VideoMedia).SamplesPerSecond : null;

            return (Nullable.Compare(aSPS, bSPS));
        }
    }
    class MediaNrChannelsComparer : IComparer
    {

        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            Nullable<int> aNrChannels = x.Media is VideoMedia ? (x.Media as VideoMedia).NrChannels : null;
            Nullable<int> bNrChannels = y.Media is VideoMedia ? (y.Media as VideoMedia).NrChannels : null;

            return (Nullable.Compare(aNrChannels, bNrChannels));
        }
    }
    class MediaCameraMakeComparer : IComparer
    {

        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            String aVal = x.Media is ImageMedia ? (x.Media as ImageMedia).CameraMake : null;
            String bVal = y.Media is ImageMedia ? (y.Media as ImageMedia).CameraMake : null;

            return (MediaComparerUtils.Compare(aVal, bVal));
        }
    }
    class MediaCameraModelComparer : IComparer
    {

        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            String aVal = x.Media is ImageMedia ? (x.Media as ImageMedia).CameraModel : null;
            String bVal = y.Media is ImageMedia ? (y.Media as ImageMedia).CameraModel : null;

            return (MediaComparerUtils.Compare(aVal, bVal));
        }
    }
    class MediaLensComparer : IComparer
    {
        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            String aVal = x.Media is ImageMedia ? (x.Media as ImageMedia).Lens : null;
            String bVal = y.Media is ImageMedia ? (y.Media as ImageMedia).Lens : null;

            return (MediaComparerUtils.Compare(aVal, bVal));
        }
    }
    class MediaFocalLengthComparer : IComparer
    {
        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            Nullable<double> aVal = x.Media is ImageMedia ? (x.Media as ImageMedia).FocalLength : null;
            Nullable<double> bVal = y.Media is ImageMedia ? (y.Media as ImageMedia).FocalLength : null;

            return (Nullable.Compare(aVal, bVal));
        }
    }

    class MediaFNumberComparer : IComparer
    {
        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            Nullable<double> aVal = x.Media is ImageMedia ? (x.Media as ImageMedia).FNumber : null;
            Nullable<double> bVal = y.Media is ImageMedia ? (y.Media as ImageMedia).FNumber : null;

            return (Nullable.Compare(aVal, bVal));
        }
    }

    class MediaExposureTimeComparer : IComparer
    {
        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            Nullable<double> aVal = x.Media is ImageMedia ? (x.Media as ImageMedia).ExposureTime : null;
            Nullable<double> bVal = y.Media is ImageMedia ? (y.Media as ImageMedia).ExposureTime : null;

            return (Nullable.Compare(aVal, bVal));
        }
    }

    class MediaISOSpeedRatingComparer : IComparer
    {
        public int Compare(Object a, Object b)
        {
            MediaFileItem x = (MediaFileItem)a;
            MediaFileItem y = (MediaFileItem)b;

            int result = MediaComparerUtils.hasMediaTest(x, y);
            if (result != 0) return result;

            Nullable<int> aVal = x.Media is ImageMedia ? (x.Media as ImageMedia).ISOSpeedRating : null;
            Nullable<int> bVal = y.Media is ImageMedia ? (y.Media as ImageMedia).ISOSpeedRating : null;

            return (Nullable.Compare(aVal, bVal));
        }
    }


    static class MediaComparerUtils
    {
        public static int Compare<T>(T x, T y) where T : IComparable
        {
            if (x == null) return -1;
            if (y == null) return 1;
            return (x.CompareTo(y));
        }

        public static int hasMediaTest(MediaFileItem x, MediaFileItem y)
        {
            if (x.Media != null && y.Media != null) return 0;
            if (x.Media == null) return 1;
            else return -1;
        }
    }



}
