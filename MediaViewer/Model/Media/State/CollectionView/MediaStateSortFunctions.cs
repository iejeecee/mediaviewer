using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.State.CollectionView
{
    public enum MediaStateSortMode
    {
        Name,
        Size,
        Rating,
        Imported,
        Tags,
        MimeType,
        CreationDate,
        FileDate,
        LastModified,
        SoftWare,
        Width,
        Height,
        // video
        Duration,
        FramesPerSecond,
        VideoCodec,
        AudioCodec,
        PixelFormat,
        BitsPerSample,
        SamplesPerSecond,
        NrChannels,
        // image
        CameraMake,
        CameraModel,
        Lens,
        FocalLength,
        ExposureTime,
        FNumber,
        ISOSpeedRating
    }

    class MediaStateSortFunctions
    {
        public static Func<MediaFileItem, MediaFileItem, int> getSortFunction(MediaStateSortMode sortMode)
        {

            Func<MediaFileItem, MediaFileItem, int> sortFunc = null;

            switch (sortMode)
            {
                case MediaStateSortMode.Name:

                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = System.IO.Path.GetFileName(a.Location).CompareTo(System.IO.Path.GetFileName(b.Location));
                            return (result);
                        });
                    break;
                case MediaStateSortMode.Size:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            return (a.Media.SizeBytes.CompareTo(b.Media.SizeBytes));
                        });
                    break;
                case MediaStateSortMode.Rating:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            return (Nullable.Compare(a.Media.Rating, b.Media.Rating));
                        });
                    break;
                case MediaStateSortMode.Imported:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            return (a.Media.IsImported.CompareTo(b.Media.IsImported));
                        });
                    break;
                case MediaStateSortMode.Tags:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            return (a.Media.Tags.Count.CompareTo(b.Media.Tags.Count));
                        });
                    break;
                case MediaStateSortMode.FileDate:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                       (a, b) =>
                       {
                           int result = hasMediaTest(a, b);
                           if (result != 0) return result;

                           return (a.Media.FileDate.CompareTo(b.Media.FileDate));
                       });
                    break;
                case MediaStateSortMode.MimeType:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            return (a.Media.MimeType.CompareTo(b.Media.MimeType));
                        });
                    break;
                case MediaStateSortMode.LastModified:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            return (a.Media.LastModifiedDate.CompareTo(b.Media.LastModifiedDate));
                        });
                    break;
                case MediaStateSortMode.CreationDate:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            return (Nullable.Compare(a.Media.CreationDate, b.Media.CreationDate));
                        });
                    break;
                case MediaStateSortMode.SoftWare:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            return (Compare(a.Media.Software, b.Media.Software));
                        });
                    break;
                case MediaStateSortMode.Width:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            int widthA;
                            int widthB;

                            if (a.Media is ImageMedia)
                            {
                                widthA = (a.Media as ImageMedia).Width;
                            }
                            else
                            {
                                widthA = (a.Media as VideoMedia).Width;
                            }

                            if (b.Media is ImageMedia)
                            {
                                widthB = (b.Media as ImageMedia).Width;
                            }
                            else
                            {
                                widthB = (b.Media as VideoMedia).Width;
                            }

                            return (widthA.CompareTo(widthB));
                        });
                    break;
                case MediaStateSortMode.Height:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            int heightA;
                            int heightB;

                            if (a.Media is ImageMedia)
                            {
                                heightA = (a.Media as ImageMedia).Height;
                            }
                            else
                            {
                                heightA = (a.Media as VideoMedia).Height;
                            }

                            if (b.Media is ImageMedia)
                            {
                                heightB = (b.Media as ImageMedia).Height;
                            }
                            else
                            {
                                heightB = (b.Media as VideoMedia).Height;
                            }

                            return (heightA.CompareTo(heightB));
                        });
                    break;
                case MediaStateSortMode.Duration:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<int> aDuration = a.Media is VideoMedia ? new Nullable<int>((a.Media as VideoMedia).DurationSeconds) : null;
                            Nullable<int> bDuration = b.Media is VideoMedia ? new Nullable<int>((b.Media as VideoMedia).DurationSeconds) : null;

                            return (Nullable.Compare<int>(aDuration, bDuration));
                        });
                    break;
                case MediaStateSortMode.FramesPerSecond:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<double> aFPS = a.Media is VideoMedia ? new Nullable<double>((a.Media as VideoMedia).FramesPerSecond) : null;
                            Nullable<double> bFPS = b.Media is VideoMedia ? new Nullable<double>((b.Media as VideoMedia).FramesPerSecond) : null;

                            return (Nullable.Compare<double>(aFPS, bFPS));
                        });
                    break;
                case MediaStateSortMode.VideoCodec:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            String aVideoCodec = a.Media is VideoMedia ? (a.Media as VideoMedia).VideoCodec : "";
                            String bVideoCodec = b.Media is VideoMedia ? (b.Media as VideoMedia).VideoCodec : "";

                            return (aVideoCodec.CompareTo(bVideoCodec));
                        });
                    break;
                case MediaStateSortMode.AudioCodec:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            String aAudioCodec = a.Media is VideoMedia ? (a.Media as VideoMedia).AudioCodec : null;
                            String bAudioCodec = b.Media is VideoMedia ? (b.Media as VideoMedia).AudioCodec : null;

                            return (Compare(aAudioCodec, bAudioCodec));
                        });
                    break;
                case MediaStateSortMode.PixelFormat:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            String aPixelFormat = a.Media is VideoMedia ? (a.Media as VideoMedia).PixelFormat : null;
                            String bPixelFormat = b.Media is VideoMedia ? (b.Media as VideoMedia).PixelFormat : null;

                            return (Compare(aPixelFormat, bPixelFormat));
                        });
                    break;
                case MediaStateSortMode.BitsPerSample:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<short> aBPS = a.Media is VideoMedia ? (a.Media as VideoMedia).BitsPerSample : null;
                            Nullable<short> bBPS = b.Media is VideoMedia ? (b.Media as VideoMedia).BitsPerSample : null;

                            return (Nullable.Compare(aBPS, bBPS));
                        });
                    break;
                case MediaStateSortMode.SamplesPerSecond:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<int> aSPS = a.Media is VideoMedia ? (a.Media as VideoMedia).SamplesPerSecond : null;
                            Nullable<int> bSPS = b.Media is VideoMedia ? (b.Media as VideoMedia).SamplesPerSecond : null;

                            return (Nullable.Compare(aSPS, bSPS));
                        });
                    break;
                case MediaStateSortMode.NrChannels:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<int> aNrChannels = a.Media is VideoMedia ? (a.Media as VideoMedia).NrChannels : null;
                            Nullable<int> bNrChannels = b.Media is VideoMedia ? (b.Media as VideoMedia).NrChannels : null;

                            return (Nullable.Compare(aNrChannels, bNrChannels));
                        });
                    break;
                case MediaStateSortMode.CameraMake:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            String aVal = a.Media is ImageMedia ? (a.Media as ImageMedia).CameraMake : null;
                            String bVal = b.Media is ImageMedia ? (b.Media as ImageMedia).CameraMake : null;

                            return (Compare(aVal, bVal));
                        });
                    break;
                case MediaStateSortMode.CameraModel:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            String aVal = a.Media is ImageMedia ? (a.Media as ImageMedia).CameraModel : null;
                            String bVal = b.Media is ImageMedia ? (b.Media as ImageMedia).CameraModel : null;

                            return (Compare(aVal, bVal));
                        });
                    break;
                case MediaStateSortMode.Lens:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            String aVal = a.Media is ImageMedia ? (a.Media as ImageMedia).Lens : null;
                            String bVal = b.Media is ImageMedia ? (b.Media as ImageMedia).Lens : null;

                            return (Compare(aVal, bVal));
                        });
                    break;
                case MediaStateSortMode.FocalLength:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<double> aVal = a.Media is ImageMedia ? (a.Media as ImageMedia).FocalLength : null;
                            Nullable<double> bVal = b.Media is ImageMedia ? (b.Media as ImageMedia).FocalLength : null;

                            return (Nullable.Compare(aVal, bVal));
                        });
                    break;
                case MediaStateSortMode.FNumber:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<double> aVal = a.Media is ImageMedia ? (a.Media as ImageMedia).FNumber : null;
                            Nullable<double> bVal = b.Media is ImageMedia ? (b.Media as ImageMedia).FNumber : null;

                            return (Nullable.Compare(aVal, bVal));
                        });
                    break;
                case MediaStateSortMode.ExposureTime:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<double> aVal = a.Media is ImageMedia ? (a.Media as ImageMedia).ExposureTime : null;
                            Nullable<double> bVal = b.Media is ImageMedia ? (b.Media as ImageMedia).ExposureTime : null;

                            return (Nullable.Compare(aVal, bVal));
                        });
                    break;
                case MediaStateSortMode.ISOSpeedRating:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<int> aVal = a.Media is ImageMedia ? (a.Media as ImageMedia).ISOSpeedRating : null;
                            Nullable<int> bVal = b.Media is ImageMedia ? (b.Media as ImageMedia).ISOSpeedRating : null;

                            return (Nullable.Compare(aVal, bVal));
                        });
                    break;
                default:
                    break;
            }

            return (sortFunc);
        }

        public static bool isAllSortMode(MediaStateSortMode mode)
        {
            if ((int)mode <= (int)MediaStateSortMode.SoftWare) return (true);
            else return (false);
        }

        public static bool isVideoSortMode(MediaStateSortMode mode)
        {
            if ((int)mode <= (int)MediaStateSortMode.NrChannels) return (true);
            else return (false);
        }

        public static bool isImageSortMode(MediaStateSortMode mode)
        {
            if ((int)mode <= (int)MediaStateSortMode.Height || (int)mode >= (int)MediaStateSortMode.CameraMake) return (true);
            else return (false);
        }

        static int hasMediaTest(MediaFileItem a, MediaFileItem b)
        {
            if (a.Media != null && b.Media != null) return 0;
            if (a.Media == null) return 1;
            else return -1;
        }

        static int Compare<T>(T a, T b) where T : IComparable
        {
            if (a == null) return -1;
            if (b == null) return 1;
            return (a.CompareTo(b));
        }
    }
}
