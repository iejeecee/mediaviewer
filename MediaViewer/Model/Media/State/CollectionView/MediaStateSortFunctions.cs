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
        public static Func<SelectableMediaFileItem, SelectableMediaFileItem, int> getSortFunction(MediaStateSortMode sortMode)
        {

            Func<SelectableMediaFileItem, SelectableMediaFileItem, int> sortFunc = null;

            switch (sortMode)
            {
                case MediaStateSortMode.Name:

                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = System.IO.Path.GetFileName(a.Item.Location).CompareTo(System.IO.Path.GetFileName(b.Item.Location));
                            return (result);
                        });
                    break;
                case MediaStateSortMode.Size:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            return(onEquals(a.Item.Media.SizeBytes.CompareTo(b.Item.Media.SizeBytes), a, b));
                       
                        });
                    break;
                case MediaStateSortMode.Rating:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;
                 
                            return (onEquals(Nullable.Compare(a.Item.Media.Rating, b.Item.Media.Rating), a, b));

                        });
                    break;
                case MediaStateSortMode.Imported:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;
           
                            return (onEquals(a.Item.Media.IsImported.CompareTo(b.Item.Media.IsImported), a, b));
                        });
                    break;
                case MediaStateSortMode.Tags:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            return (onEquals(a.Item.Media.Tags.Count.CompareTo(b.Item.Media.Tags.Count),a,b));

                        });
                    break;
                case MediaStateSortMode.FileDate:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                       (a, b) =>
                       {
                           int result = hasMediaTest(a, b);
                           if (result != 0) return result;

                           return (onEquals(a.Item.Media.FileDate.CompareTo(b.Item.Media.FileDate),a,b));
                       });
                    break;
                case MediaStateSortMode.MimeType:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            return (onEquals(a.Item.Media.MimeType.CompareTo(b.Item.Media.MimeType),a,b));
                        });
                    break;
                case MediaStateSortMode.LastModified:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            return (onEquals(a.Item.Media.LastModifiedDate.CompareTo(b.Item.Media.LastModifiedDate),a,b));
                        });
                    break;
                case MediaStateSortMode.CreationDate:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            return (onEquals(Nullable.Compare(a.Item.Media.CreationDate, b.Item.Media.CreationDate),a,b));
                        });
                    break;
                case MediaStateSortMode.SoftWare:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            return (onEquals(Compare(a.Item.Media.Software, b.Item.Media.Software),a,b));
                        });
                    break;
                case MediaStateSortMode.Width:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            int widthA;
                            int widthB;

                            if (a.Item.Media is ImageMedia)
                            {
                                widthA = (a.Item.Media as ImageMedia).Width;
                            }
                            else
                            {
                                widthA = (a.Item.Media as VideoMedia).Width;
                            }

                            if (b.Item.Media is ImageMedia)
                            {
                                widthB = (b.Item.Media as ImageMedia).Width;
                            }
                            else
                            {
                                widthB = (b.Item.Media as VideoMedia).Width;
                            }

                            return (onEquals(widthA.CompareTo(widthB),a,b));
                        });
                    break;
                case MediaStateSortMode.Height:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            int heightA;
                            int heightB;

                            if (a.Item.Media is ImageMedia)
                            {
                                heightA = (a.Item.Media as ImageMedia).Height;
                            }
                            else
                            {
                                heightA = (a.Item.Media as VideoMedia).Height;
                            }

                            if (b.Item.Media is ImageMedia)
                            {
                                heightB = (b.Item.Media as ImageMedia).Height;
                            }
                            else
                            {
                                heightB = (b.Item.Media as VideoMedia).Height;
                            }

                            return (onEquals(heightA.CompareTo(heightB),a,b));
                        });
                    break;
                case MediaStateSortMode.Duration:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<int> aDuration = a.Item.Media is VideoMedia ? new Nullable<int>((a.Item.Media as VideoMedia).DurationSeconds) : null;
                            Nullable<int> bDuration = b.Item.Media is VideoMedia ? new Nullable<int>((b.Item.Media as VideoMedia).DurationSeconds) : null;

                            return (onEquals(Nullable.Compare<int>(aDuration, bDuration),a,b));
                        });
                    break;
                case MediaStateSortMode.FramesPerSecond:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<double> aFPS = a.Item.Media is VideoMedia ? new Nullable<double>((a.Item.Media as VideoMedia).FramesPerSecond) : null;
                            Nullable<double> bFPS = b.Item.Media is VideoMedia ? new Nullable<double>((b.Item.Media as VideoMedia).FramesPerSecond) : null;

                            return (onEquals(Nullable.Compare<double>(aFPS, bFPS),a,b));
                        });
                    break;
                case MediaStateSortMode.VideoCodec:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            String aVideoCodec = a.Item.Media is VideoMedia ? (a.Item.Media as VideoMedia).VideoCodec : "";
                            String bVideoCodec = b.Item.Media is VideoMedia ? (b.Item.Media as VideoMedia).VideoCodec : "";

                            return (onEquals(aVideoCodec.CompareTo(bVideoCodec),a,b));
                        });
                    break;
                case MediaStateSortMode.AudioCodec:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            String aAudioCodec = a.Item.Media is VideoMedia ? (a.Item.Media as VideoMedia).AudioCodec : null;
                            String bAudioCodec = b.Item.Media is VideoMedia ? (b.Item.Media as VideoMedia).AudioCodec : null;

                            return (onEquals(Compare(aAudioCodec, bAudioCodec),a,b));
                        });
                    break;
                case MediaStateSortMode.PixelFormat:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            String aPixelFormat = a.Item.Media is VideoMedia ? (a.Item.Media as VideoMedia).PixelFormat : null;
                            String bPixelFormat = b.Item.Media is VideoMedia ? (b.Item.Media as VideoMedia).PixelFormat : null;

                            return (onEquals(Compare(aPixelFormat, bPixelFormat),a,b));
                        });
                    break;
                case MediaStateSortMode.BitsPerSample:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<short> aBPS = a.Item.Media is VideoMedia ? (a.Item.Media as VideoMedia).BitsPerSample : null;
                            Nullable<short> bBPS = b.Item.Media is VideoMedia ? (b.Item.Media as VideoMedia).BitsPerSample : null;

                            return (onEquals(Nullable.Compare(aBPS, bBPS),a,b));
                        });
                    break;
                case MediaStateSortMode.SamplesPerSecond:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<int> aSPS = a.Item.Media is VideoMedia ? (a.Item.Media as VideoMedia).SamplesPerSecond : null;
                            Nullable<int> bSPS = b.Item.Media is VideoMedia ? (b.Item.Media as VideoMedia).SamplesPerSecond : null;

                            return (onEquals(Nullable.Compare(aSPS, bSPS),a,b));
                        });
                    break;
                case MediaStateSortMode.NrChannels:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<int> aNrChannels = a.Item.Media is VideoMedia ? (a.Item.Media as VideoMedia).NrChannels : null;
                            Nullable<int> bNrChannels = b.Item.Media is VideoMedia ? (b.Item.Media as VideoMedia).NrChannels : null;

                            return (onEquals(Nullable.Compare(aNrChannels, bNrChannels),a,b));
                        });
                    break;
                case MediaStateSortMode.CameraMake:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            String aVal = a.Item.Media is ImageMedia ? (a.Item.Media as ImageMedia).CameraMake : null;
                            String bVal = b.Item.Media is ImageMedia ? (b.Item.Media as ImageMedia).CameraMake : null;

                            return (onEquals(Compare(aVal, bVal),a,b));
                        });
                    break;
                case MediaStateSortMode.CameraModel:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            String aVal = a.Item.Media is ImageMedia ? (a.Item.Media as ImageMedia).CameraModel : null;
                            String bVal = b.Item.Media is ImageMedia ? (b.Item.Media as ImageMedia).CameraModel : null;

                            return (onEquals(Compare(aVal, bVal),a,b));
                        });
                    break;
                case MediaStateSortMode.Lens:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            String aVal = a.Item.Media is ImageMedia ? (a.Item.Media as ImageMedia).Lens : null;
                            String bVal = b.Item.Media is ImageMedia ? (b.Item.Media as ImageMedia).Lens : null;

                            return (onEquals(Compare(aVal, bVal),a,b));
                        });
                    break;
                case MediaStateSortMode.FocalLength:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<double> aVal = a.Item.Media is ImageMedia ? (a.Item.Media as ImageMedia).FocalLength : null;
                            Nullable<double> bVal = b.Item.Media is ImageMedia ? (b.Item.Media as ImageMedia).FocalLength : null;

                            return (onEquals(Nullable.Compare(aVal, bVal),a,b));
                        });
                    break;
                case MediaStateSortMode.FNumber:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<double> aVal = a.Item.Media is ImageMedia ? (a.Item.Media as ImageMedia).FNumber : null;
                            Nullable<double> bVal = b.Item.Media is ImageMedia ? (b.Item.Media as ImageMedia).FNumber : null;

                            return (onEquals(Nullable.Compare(aVal, bVal),a,b));
                        });
                    break;
                case MediaStateSortMode.ExposureTime:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<double> aVal = a.Item.Media is ImageMedia ? (a.Item.Media as ImageMedia).ExposureTime : null;
                            Nullable<double> bVal = b.Item.Media is ImageMedia ? (b.Item.Media as ImageMedia).ExposureTime : null;

                            return (onEquals(Nullable.Compare(aVal, bVal),a,b));
                        });
                    break;
                case MediaStateSortMode.ISOSpeedRating:
                    sortFunc = new Func<SelectableMediaFileItem, SelectableMediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<int> aVal = a.Item.Media is ImageMedia ? (a.Item.Media as ImageMedia).ISOSpeedRating : null;
                            Nullable<int> bVal = b.Item.Media is ImageMedia ? (b.Item.Media as ImageMedia).ISOSpeedRating : null;

                            return (onEquals(Nullable.Compare(aVal, bVal),a,b));
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

        static int onEquals(int result, SelectableMediaFileItem a, SelectableMediaFileItem b)
        {
            if (result == 0)
            {
                return (System.IO.Path.GetFileName(a.Item.Location).CompareTo(System.IO.Path.GetFileName(b.Item.Location)));
            }
            else
            {
                return (result);
            }
        }

        static int hasMediaTest(SelectableMediaFileItem a, SelectableMediaFileItem b)
        {
            if (a.Item.Media != null && b.Item.Media != null) return 0;
            if (a.Item.Media == null) return 1;
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
