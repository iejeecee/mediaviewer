using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaPreview
{
    public class InfoIcon
    {
        public enum IconType
        {
            ERROR = 0,
            AVI,
            MOV,
            MP4,
            WMV,
            ASF,
            BMP,
            GIF,
            JPG,
            PNG,
            TIFF,
            GEOTAG,
            COMMENTS,
            MUTE,
            MPG,
            TAGGED
        };

        string caption;
        IconType iconType;

        IconType mimeTypeToIconType(string mimeType)
        {

            if (mimeType.Equals("image/tiff"))
            {

                return (IconType.TIFF);

            }
            else if (mimeType.Equals("image/gif"))
            {

                return (IconType.GIF);

            }
            else if (mimeType.Equals("image/png"))
            {

                return (IconType.PNG);

            }
            else if (mimeType.Equals("image/jpeg"))
            {

                return (IconType.JPG);

            }
            else if (mimeType.Equals("image/bmp"))
            {

                return (IconType.BMP);

            }
            else if (mimeType.Equals("video/x-ms-asf"))
            {

                return (IconType.ASF);

            }
            else if (mimeType.Equals("video/x-ms-wmv"))
            {

                return (IconType.WMV);

            }
            else if (mimeType.Equals("video/x-flv"))
            {

                return (IconType.MP4);

            }
            else if (mimeType.Equals("video/avi") ||
              mimeType.Equals("video/vnd.avi") ||
              mimeType.Equals("video/msvideo") ||
              mimeType.Equals("video/x-msvideo"))
            {

                return (IconType.AVI);

            }
            else if (mimeType.Equals("video/mpg") ||
             mimeType.Equals("video/mpeg") ||
             mimeType.Equals("video/x-mpeg") ||
             mimeType.Equals("video/mpeg2"))
            {

                return (IconType.MPG);

            }
            else if (mimeType.Equals("video/mp4"))
            {

                return (IconType.MP4);

            }
            else if (mimeType.Equals("video/quicktime"))
            {

                return (IconType.MOV);

            }
            else if (mimeType.Equals("video/x-matroska"))
            {

                return (IconType.MP4);

            }
            else if (mimeType.Equals("video/x-m4v"))
            {

                return (IconType.MP4);

            }
            else
            {

                return (IconType.JPG);
            }

        }

        public InfoIcon(string mimeType)
        {

            iconType = mimeTypeToIconType(mimeType);
        }

        public InfoIcon(IconType iconType)
        {

            this.iconType = iconType;
        }

        public IconType IconImageType
        {
            get
            {

                return (iconType);
            }
        }

        public string Caption
        {
            get
            {

                return (caption);
            }

            set
            {

                this.caption = value;
            }
        }


    }
}
